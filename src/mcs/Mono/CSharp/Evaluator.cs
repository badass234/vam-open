using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace Mono.CSharp
{
	public class Evaluator
	{
		private enum ParseMode
		{
			Silent = 0,
			ReportErrors = 1,
			GetCompletions = 2
		}

		private enum InputKind
		{
			EOF = 0,
			StatementOrExpression = 1,
			CompilationUnit = 2,
			Error = 3
		}

		public static class QuitValue
		{
		}

		private static object evaluator_lock = new object();

		private static volatile bool invoking;

		private static int count;

		private static Thread invoke_thread;

		private readonly Dictionary<string, Tuple<FieldSpec, FieldInfo>> fields;

		private Type base_class;

		private bool inited;

		private int startup_files;

		private readonly CompilerContext ctx;

		private readonly ModuleContainer module;

		private readonly ReflectionImporter importer;

		private readonly CompilationSourceFile source_file;

		private int? listener_id;

		public bool DescribeTypeExpressions;

		public bool Terse = true;

		private static MethodInfo listener_proxy_value;

		public bool WaitOnTask { get; set; }

		public Type InteractiveBaseClass
		{
			get
			{
				return base_class;
			}
			set
			{
				base_class = value;
				if (value != null && typeof(InteractiveBase).IsAssignableFrom(value))
				{
					InteractiveBase.Evaluator = this;
				}
			}
		}

		public ValueModificationHandler ModificationListener { get; set; }

		public Evaluator(CompilerContext ctx)
		{
			this.ctx = ctx;
			module = new ModuleContainer(ctx);
			module.Evaluator = this;
			source_file = new CompilationSourceFile(module, null);
			module.AddTypeContainer(source_file);
			startup_files = ctx.SourceFiles.Count;
			module.SetDeclaringAssembly(new AssemblyDefinitionDynamic(module, "evaluator"));
			importer = new ReflectionImporter(module, ctx.BuiltinTypes);
			InteractiveBaseClass = typeof(InteractiveBase);
			fields = new Dictionary<string, Tuple<FieldSpec, FieldInfo>>();
		}

		private void Init()
		{
			DynamicLoader dynamicLoader = new DynamicLoader(importer, ctx);
			CompilerCallableEntryPoint.Reset();
			RootContext.ToplevelTypes = module;
			dynamicLoader.LoadReferences(module);
			ctx.BuiltinTypes.CheckDefinitions(module);
			module.InitializePredefinedTypes();
			inited = true;
		}

		private void ParseStartupFiles()
		{
			Driver driver = new Driver(ctx);
			Location.Initialize(ctx.SourceFiles);
			ParserSession session = new ParserSession();
			for (int i = 0; i < startup_files; i++)
			{
				SourceFile file = ctx.SourceFiles[i];
				driver.Parse(file, module, session, ctx.Report);
			}
		}

		private void Reset()
		{
			CompilerCallableEntryPoint.PartialReset();
			Location.Reset();
			Location.Initialize(ctx.SourceFiles);
		}

		public void Interrupt()
		{
			if (inited && invoking && invoke_thread != null)
			{
				invoke_thread.Abort();
			}
		}

		public string Compile(string input, out CompiledMethod compiled)
		{
			if (input == null || input.Length == 0)
			{
				compiled = null;
				return null;
			}
			lock (evaluator_lock)
			{
				if (!inited)
				{
					Init();
					ParseStartupFiles();
				}
				else
				{
					ctx.Report.Printer.Reset();
				}
				bool partial_input;
				CSharpParser cSharpParser = ParseString(ParseMode.Silent, input, out partial_input);
				bool partial_input2;
				if (((cSharpParser == null && Terse) & partial_input) && ParseString(ParseMode.Silent, input + "{}", out partial_input2) == null)
				{
					cSharpParser = ParseString(ParseMode.Silent, input + ";", out partial_input2);
				}
				if (cSharpParser == null)
				{
					compiled = null;
					if (partial_input)
					{
						return input;
					}
					ParseString(ParseMode.ReportErrors, input, out partial_input);
					return null;
				}
				Class interactiveResult = cSharpParser.InteractiveResult;
				compiled = CompileBlock(interactiveResult, cSharpParser.undo, ctx.Report);
				return null;
			}
		}

		public CompiledMethod Compile(string input)
		{
			CompiledMethod compiled;
			if (Compile(input, out compiled) != null)
			{
				return null;
			}
			return compiled;
		}

		public void EmitValueChangedCallback(EmitContext ec, string name, TypeSpec type, Location loc)
		{
			if (!listener_id.HasValue)
			{
				listener_id = ListenerProxy.Register(ModificationListener);
			}
			if (listener_proxy_value == null)
			{
				listener_proxy_value = typeof(ListenerProxy).GetMethod("ValueChanged");
			}
			if (type.IsStructOrEnum)
			{
				ec.Emit(OpCodes.Box, type);
			}
			ec.EmitInt(loc.Row);
			ec.EmitInt(loc.Column);
			ec.Emit(OpCodes.Ldstr, name);
			ec.EmitInt(listener_id.Value);
			ec.Emit(OpCodes.Call, listener_proxy_value);
		}

		public string Evaluate(string input, out object result, out bool result_set)
		{
			result_set = false;
			result = null;
			CompiledMethod compiled;
			input = Compile(input, out compiled);
			if (input != null)
			{
				return input;
			}
			if (compiled == null)
			{
				return null;
			}
			object retvalue = typeof(QuitValue);
			try
			{
				invoke_thread = Thread.CurrentThread;
				invoking = true;
				compiled(ref retvalue);
			}
			catch (ThreadAbortException arg)
			{
				Thread.ResetAbort();
				Console.WriteLine("Interrupted!\n{0}", arg);
			}
			finally
			{
				invoking = false;
				if (listener_id.HasValue)
				{
					ListenerProxy.Unregister(listener_id.Value);
					listener_id = null;
				}
			}
			if (retvalue != typeof(QuitValue))
			{
				result_set = true;
				result = retvalue;
			}
			return null;
		}

		public string[] GetCompletions(string input, out string prefix)
		{
			prefix = "";
			if (input == null || input.Length == 0)
			{
				return null;
			}
			lock (evaluator_lock)
			{
				if (!inited)
				{
					Init();
				}
				bool partial_input;
				CSharpParser cSharpParser = ParseString(ParseMode.GetCompletions, input, out partial_input);
				if (cSharpParser == null)
				{
					return null;
				}
				Class interactiveResult = cSharpParser.InteractiveResult;
				TypeSpec t = importer.ImportType(base_class);
				List<FullNamedExpression> baseTypes = new List<FullNamedExpression>(1)
				{
					new TypeExpression(t, interactiveResult.Location)
				};
				interactiveResult.SetBaseTypes(baseTypes);
				AssemblyBuilderAccess access = AssemblyBuilderAccess.Run;
				AssemblyDefinitionDynamic assemblyDefinitionDynamic = new AssemblyDefinitionDynamic(module, "completions");
				assemblyDefinitionDynamic.Create(AppDomain.CurrentDomain, access);
				module.SetDeclaringAssembly(assemblyDefinitionDynamic);
				interactiveResult.CreateContainer();
				interactiveResult.DefineContainer();
				Method method = interactiveResult.Members[0] as Method;
				BlockContext bc = new BlockContext(method, method.Block, ctx.BuiltinTypes.Void);
				try
				{
					method.Block.Resolve(bc, method);
				}
				catch (CompletionResult completionResult)
				{
					prefix = completionResult.BaseText;
					return completionResult.Result;
				}
			}
			return null;
		}

		public bool Run(string statement)
		{
			object result;
			bool result_set;
			return Evaluate(statement, out result, out result_set) == null;
		}

		public object Evaluate(string input)
		{
			object result;
			bool result_set;
			if (Evaluate(input, out result, out result_set) != null)
			{
				throw new ArgumentException("Syntax error on input: partial input");
			}
			if (!result_set)
			{
				throw new ArgumentException("The expression failed to resolve");
			}
			return result;
		}

		private InputKind ToplevelOrStatement(SeekableStreamReader seekable)
		{
			Tokenizer tokenizer = new Tokenizer(seekable, source_file, new ParserSession(), ctx.Report);
			tokenizer.parsing_block++;
			switch (tokenizer.token())
			{
			case 257:
				return InputKind.EOF;
			case 261:
			case 272:
			case 281:
			case 284:
			case 296:
			case 297:
			case 301:
			case 309:
			case 310:
			case 311:
			case 317:
			case 321:
			case 323:
			case 373:
			case 427:
				return InputKind.CompilationUnit;
			case 265:
			case 267:
			case 270:
			case 275:
			case 279:
			case 287:
			case 288:
			case 295:
			case 300:
			case 302:
			case 304:
			case 316:
			case 318:
			case 322:
			case 330:
			case 331:
				return InputKind.StatementOrExpression;
			case 335:
				switch (tokenizer.token())
				{
				case 257:
					return InputKind.EOF;
				case 321:
				case 422:
					return InputKind.CompilationUnit;
				default:
					return InputKind.StatementOrExpression;
				}
			case 277:
				switch (tokenizer.token())
				{
				case 257:
					return InputKind.EOF;
				case 371:
				case 375:
					return InputKind.StatementOrExpression;
				default:
					return InputKind.CompilationUnit;
				}
			case 333:
				switch (tokenizer.token())
				{
				case 257:
					return InputKind.EOF;
				case 375:
					return InputKind.StatementOrExpression;
				default:
					return InputKind.CompilationUnit;
				}
			case 259:
			case 307:
			case 312:
				return InputKind.Error;
			default:
				return InputKind.StatementOrExpression;
			}
		}

		private CSharpParser ParseString(ParseMode mode, string input, out bool partial_input)
		{
			partial_input = false;
			Reset();
			Encoding encoding = ctx.Settings.Encoding;
			SeekableStreamReader seekableStreamReader = new SeekableStreamReader(new MemoryStream(encoding.GetBytes(input)), encoding);
			InputKind inputKind = ToplevelOrStatement(seekableStreamReader);
			switch (inputKind)
			{
			case InputKind.Error:
				if (mode == ParseMode.ReportErrors)
				{
					ctx.Report.Error(-25, "Detection Parsing Error");
				}
				partial_input = false;
				return null;
			case InputKind.EOF:
				if (mode == ParseMode.ReportErrors)
				{
					Console.Error.WriteLine("Internal error: EOF condition should have been detected in a previous call with silent=true");
				}
				partial_input = true;
				return null;
			default:
			{
				seekableStreamReader.Position = 0;
				source_file.DeclarationFound = false;
				CSharpParser cSharpParser = new CSharpParser(seekableStreamReader, source_file, new ParserSession());
				if (inputKind == InputKind.StatementOrExpression)
				{
					cSharpParser.Lexer.putback_char = 1048576;
					cSharpParser.Lexer.parsing_block++;
					ctx.Settings.StatementMode = true;
				}
				else
				{
					cSharpParser.Lexer.putback_char = 1048577;
					ctx.Settings.StatementMode = false;
				}
				if (mode == ParseMode.GetCompletions)
				{
					cSharpParser.Lexer.CompleteOnEOF = true;
				}
				ReportPrinter reportPrinter = null;
				if (mode == ParseMode.Silent || mode == ParseMode.GetCompletions)
				{
					reportPrinter = ctx.Report.SetPrinter(new StreamReportPrinter(TextWriter.Null));
				}
				try
				{
					cSharpParser.parse();
				}
				finally
				{
					if (ctx.Report.Errors != 0)
					{
						if (mode != ParseMode.ReportErrors && cSharpParser.UnexpectedEOF)
						{
							partial_input = true;
						}
						if (cSharpParser.undo != null)
						{
							cSharpParser.undo.ExecuteUndo();
						}
						cSharpParser = null;
					}
					if (reportPrinter != null)
					{
						ctx.Report.SetPrinter(reportPrinter);
					}
				}
				return cSharpParser;
			}
			}
		}

		private CompiledMethod CompileBlock(Class host, Undo undo, Report Report)
		{
			string text = "eval-" + count + ".dll";
			count++;
			AssemblyBuilderAccess assemblyBuilderAccess;
			AssemblyDefinitionDynamic assemblyDefinitionDynamic;
			if (Environment.GetEnvironmentVariable("SAVE") != null)
			{
				assemblyBuilderAccess = AssemblyBuilderAccess.RunAndSave;
				assemblyDefinitionDynamic = new AssemblyDefinitionDynamic(module, text, text);
				assemblyDefinitionDynamic.Importer = importer;
			}
			else
			{
				assemblyBuilderAccess = AssemblyBuilderAccess.Run;
				assemblyDefinitionDynamic = new AssemblyDefinitionDynamic(module, text);
			}
			assemblyDefinitionDynamic.Create(AppDomain.CurrentDomain, assemblyBuilderAccess);
			Method method;
			if (host != null)
			{
				TypeSpec t = importer.ImportType(base_class);
				List<FullNamedExpression> baseTypes = new List<FullNamedExpression>(1)
				{
					new TypeExpression(t, host.Location)
				};
				host.SetBaseTypes(baseTypes);
				method = (Method)host.Members[0];
				if ((method.ModFlags & Modifiers.ASYNC) != 0)
				{
					ParametersCompiled parametersCompiled = new ParametersCompiled(new Parameter(new TypeExpression(module.Compiler.BuiltinTypes.Object, Location.Null), "$retval", Parameter.Modifier.REF, null, Location.Null));
					Method method2 = new Method(host, new TypeExpression(module.Compiler.BuiltinTypes.Void, Location.Null), Modifiers.PUBLIC | Modifiers.STATIC, new MemberName("AsyncWait"), parametersCompiled, null);
					method2.Block = new ToplevelBlock(method2.Compiler, parametersCompiled, Location.Null);
					method2.Block.AddStatement(new StatementExpression(new SimpleAssign(new SimpleName(parametersCompiled[0].Name, Location.Null), new Invocation(new SimpleName(method.MemberName.Name, Location.Null), new Arguments(0)), Location.Null), Location.Null));
					if (WaitOnTask)
					{
						Cast expr = new Cast(method.TypeExpression, new SimpleName(parametersCompiled[0].Name, Location.Null), Location.Null);
						method2.Block.AddStatement(new StatementExpression(new Invocation(new MemberAccess(expr, "Wait", Location.Null), new Arguments(0)), Location.Null));
					}
					host.AddMember(method2);
					method = method2;
				}
				host.CreateContainer();
				host.DefineContainer();
				host.Define();
			}
			else
			{
				method = null;
			}
			module.CreateContainer();
			module.EnableRedefinition();
			source_file.EnableRedefinition();
			module.Define();
			if (Report.Errors != 0)
			{
				undo?.ExecuteUndo();
				return null;
			}
			if (host != null)
			{
				host.PrepareEmit();
				host.EmitContainer();
			}
			module.EmitContainer();
			if (Report.Errors != 0)
			{
				undo?.ExecuteUndo();
				return null;
			}
			module.CloseContainer();
			host?.CloseContainer();
			if (assemblyBuilderAccess == AssemblyBuilderAccess.RunAndSave)
			{
				assemblyDefinitionDynamic.Save();
			}
			if (host == null)
			{
				return null;
			}
			Type type = assemblyDefinitionDynamic.Builder.GetType(host.TypeBuilder.Name);
			MethodInfo method3 = type.GetMethod(method.MemberName.Name);
			foreach (MemberCore member in host.Members)
			{
				Field field = member as Field;
				if (field == null)
				{
					continue;
				}
				FieldInfo field2 = type.GetField(field.Name);
				Tuple<FieldSpec, FieldInfo> value;
				if (fields.TryGetValue(field.Name, out value) && !value.Item1.MemberType.IsStruct)
				{
					try
					{
						value.Item2.SetValue(null, null);
					}
					catch
					{
					}
				}
				fields[field.Name] = Tuple.Create(field.Spec, field2);
			}
			return (CompiledMethod)System.Delegate.CreateDelegate(typeof(CompiledMethod), method3);
		}

		public Tuple<FieldSpec, FieldInfo> LookupField(string name)
		{
			Tuple<FieldSpec, FieldInfo> value;
			fields.TryGetValue(name, out value);
			return value;
		}

		private static string Quote(string s)
		{
			if (s.IndexOf('"') != -1)
			{
				s = s.Replace("\"", "\\\"");
			}
			return "\"" + s + "\"";
		}

		public string GetUsing()
		{
			if (source_file == null || source_file.Usings == null)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (UsingClause @using in source_file.Usings)
			{
				if (@using.Alias == null && @using.ResolvedExpression != null)
				{
					stringBuilder.AppendFormat("using {0};", @using.ToString());
					stringBuilder.Append(Environment.NewLine);
				}
			}
			return stringBuilder.ToString();
		}

		public List<string> GetUsingList()
		{
			List<string> list = new List<string>();
			if (source_file == null || source_file.Usings == null)
			{
				return list;
			}
			foreach (UsingClause @using in source_file.Usings)
			{
				if (@using.Alias == null && @using.ResolvedExpression != null)
				{
					list.Add(@using.NamespaceExpression.Name);
				}
			}
			return list;
		}

		public string[] GetVarNames()
		{
			lock (evaluator_lock)
			{
				return new List<string>(fields.Keys).ToArray();
			}
		}

		public string GetVars()
		{
			lock (evaluator_lock)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (KeyValuePair<string, Tuple<FieldSpec, FieldInfo>> field in fields)
				{
					Tuple<FieldSpec, FieldInfo> tuple = LookupField(field.Key);
					object obj;
					try
					{
						obj = tuple.Item2.GetValue(null);
						if (obj is string)
						{
							obj = Quote((string)obj);
						}
					}
					catch
					{
						obj = "<error reading value>";
					}
					stringBuilder.AppendFormat("{0} {1} = {2}", tuple.Item1.MemberType.GetSignatureForError(), field.Key, obj);
					stringBuilder.AppendLine();
				}
				return stringBuilder.ToString();
			}
		}

		public void LoadAssembly(string file)
		{
			Assembly assembly = new DynamicLoader(importer, ctx).LoadAssemblyFile(file, false);
			if (assembly == null)
			{
				return;
			}
			lock (evaluator_lock)
			{
				importer.ImportAssembly(assembly, module.GlobalRootNamespace);
			}
		}

		public void ReferenceAssembly(Assembly a)
		{
			lock (evaluator_lock)
			{
				importer.ImportAssembly(a, module.GlobalRootNamespace);
			}
		}
	}
}
