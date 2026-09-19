using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using Mono.CSharp;

namespace DynamicCSharp.Compiler
{
	internal sealed class McsDriver
	{
		private readonly CompilerContext context;

		// The five fields this repair reflects into are what makes an assembly's method builders reachable
		// while it is still being written: nothing public lists them, and the type builder's own methods
		// array is the one that is known to hold the builders this has to inspect.
		private static readonly FieldInfo MethodOverrideField = typeof(MethodBuilder).GetField("override_methods", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly FieldInfo ModuleTypeBuildersField = typeof(ModuleBuilder).GetField("types", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly FieldInfo AssemblyModulesField = typeof(AssemblyBuilder).GetField("modules", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly FieldInfo TypeBuilderMethodsField = typeof(TypeBuilder).GetField("methods", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly FieldInfo TypeBuilderSubtypesField = typeof(TypeBuilder).GetField("subtypes", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly FieldInfo TypeBuilderCreatedField = typeof(TypeBuilder).GetField("created", BindingFlags.Instance | BindingFlags.NonPublic);

		public Report Report => context.Report;

		public McsDriver(CompilerContext context)
		{
			this.context = context;
		}

		public void TokenizeFile(SourceFile source, ModuleContainer module, ParserSession session)
		{
			Stream stream = null;
			try
			{
				stream = source.GetDataStream();
			}
			catch
			{
				Report.Error(2001, "Failed to open file '{0}' for reading", source.Name);
				return;
			}
			using (stream)
			{
				using (SeekableStreamReader input = new SeekableStreamReader(stream, context.Settings.Encoding))
				{
					CompilationSourceFile file = new CompilationSourceFile(module, source);
					Tokenizer tokenizer = new Tokenizer(input, file, session, context.Report);
					int num = 0;
					int num2 = 0;
					int num3 = 0;
					while ((num = tokenizer.token()) != 257)
					{
						num2++;
						if (num == 259)
						{
							num3++;
						}
					}
				}
			}
		}

		public void Parse(ModuleContainer module)
		{
			bool tokenizeOnly = module.Compiler.Settings.TokenizeOnly;
			List<SourceFile> sourceFiles = module.Compiler.SourceFiles;
			Location.Initialize(sourceFiles);
			ParserSession parserSession = new ParserSession();
			parserSession.UseJayGlobalArrays = true;
			parserSession.LocatedTokens = new LocatedToken[15000];
			ParserSession session = parserSession;
			for (int i = 0; i < sourceFiles.Count; i++)
			{
				if (tokenizeOnly)
				{
					TokenizeFile(sourceFiles[i], module, session);
				}
				else
				{
					Parse(sourceFiles[i], module, session, Report);
				}
			}
		}

		public void Parse(SourceFile source, ModuleContainer module, ParserSession session, Report report)
		{
			Stream stream = null;
			try
			{
				stream = source.GetDataStream();
			}
			catch
			{
				Report.Error(2001, "Failed to open file '{0}' for reading", source.Name);
				return;
			}
			using (stream)
			{
				if (stream.ReadByte() == 77 && stream.ReadByte() == 90)
				{
					report.Error(2015, "Failed to open file '{0}' for reading because it is a binary file. A text file was expected", source.Name);
					stream.Close();
					return;
				}
				stream.Position = 0L;
				using (SeekableStreamReader reader = new SeekableStreamReader(stream, context.Settings.Encoding, session.StreamReaderBuffer))
				{
					Parse(reader, source, module, session, report);
					if (context.Settings.GenerateDebugInfo && report.Errors == 0 && !source.HasChecksum)
					{
						stream.Position = 0L;
						MD5 checksumAlgorithm = session.GetChecksumAlgorithm();
						source.SetChecksum(checksumAlgorithm.ComputeHash(stream));
					}
				}
			}
		}

		public bool Compile(out AssemblyBuilder assembly, AppDomain domain, bool generateInMemory)
		{
			CompilerSettings settings = context.Settings;
			assembly = null;
			if (settings.FirstSourceFile == null && (settings.Target == Target.Exe || settings.Target == Target.WinExe || settings.Target == Target.Module || settings.Resources == null))
			{
				Report.Error(2008, "No source files specified");
				return false;
			}
			if (settings.Platform == Platform.AnyCPU32Preferred && (settings.Target == Target.Library || settings.Target == Target.Module))
			{
				Report.Error(4023, "The preferred platform '{0}' is only valid on executable outputs", Platform.AnyCPU32Preferred.ToString());
				return false;
			}
			TimeReporter timeReporter = new TimeReporter(settings.Timestamps);
			context.TimeReporter = timeReporter;
			timeReporter.StartTotal();
			ModuleContainer moduleContainer2 = (RootContext.ToplevelTypes = new ModuleContainer(context));
			timeReporter.Start(TimeReporter.TimerType.ParseTotal);
			Parse(moduleContainer2);
			timeReporter.Stop(TimeReporter.TimerType.ParseTotal);
			if (Report.Errors > 0)
			{
				return false;
			}
			if (settings.TokenizeOnly || settings.ParseOnly)
			{
				timeReporter.StopTotal();
				timeReporter.ShowStats();
				return true;
			}
			string outputFile = settings.OutputFile;
			string fileName = Path.GetFileName(outputFile);
			AssemblyDefinitionDynamic assemblyDefinitionDynamic = new AssemblyDefinitionDynamic(moduleContainer2, fileName, outputFile);
			moduleContainer2.SetDeclaringAssembly(assemblyDefinitionDynamic);
			ReflectionImporter importer = (ReflectionImporter)(assemblyDefinitionDynamic.Importer = new ReflectionImporter(moduleContainer2, context.BuiltinTypes));
			DynamicLoader dynamicLoader = new DynamicLoader(importer, context);
			dynamicLoader.LoadReferences(moduleContainer2);
			if (!context.BuiltinTypes.CheckDefinitions(moduleContainer2))
			{
				return false;
			}
			if (!assemblyDefinitionDynamic.Create(domain, AssemblyBuilderAccess.RunAndSave))
			{
				return false;
			}
			moduleContainer2.CreateContainer();
			dynamicLoader.LoadModules(assemblyDefinitionDynamic, moduleContainer2.GlobalRootNamespace);
			moduleContainer2.InitializePredefinedTypes();
			if (settings.GetResourceStrings != null)
			{
				moduleContainer2.LoadGetResourceStrings(settings.GetResourceStrings);
			}
			timeReporter.Start(TimeReporter.TimerType.ModuleDefinitionTotal);
			try
			{
				moduleContainer2.Define();
			}
			catch
			{
				return false;
			}
			timeReporter.Stop(TimeReporter.TimerType.ModuleDefinitionTotal);
			if (Report.Errors > 0)
			{
				return false;
			}
			if (settings.DocumentationFile != null)
			{
				DocumentationBuilder documentationBuilder = new DocumentationBuilder(moduleContainer2);
				documentationBuilder.OutputDocComment(outputFile, settings.DocumentationFile);
			}
			assemblyDefinitionDynamic.Resolve();
			if (Report.Errors > 0)
			{
				return false;
			}
			timeReporter.Start(TimeReporter.TimerType.EmitTotal);
			assemblyDefinitionDynamic.Emit();
			timeReporter.Stop(TimeReporter.TimerType.EmitTotal);
			if (Report.Errors > 0)
			{
				return false;
			}
			timeReporter.Start(TimeReporter.TimerType.CloseTypes);
			moduleContainer2.CloseContainer();
			timeReporter.Stop(TimeReporter.TimerType.CloseTypes);
			timeReporter.Start(TimeReporter.TimerType.Resouces);
			if (!settings.WriteMetadataOnly)
			{
				assemblyDefinitionDynamic.EmbedResources();
			}
			timeReporter.Stop(TimeReporter.TimerType.Resouces);
			if (Report.Errors > 0)
			{
				return false;
			}
			if (!generateInMemory)
			{
				RepairMethodOverrideDeclarations(assemblyDefinitionDynamic.Builder);
				assemblyDefinitionDynamic.Save();
			}
			assembly = assemblyDefinitionDynamic.Builder;
			timeReporter.StopTotal();
			timeReporter.ShowStats();
			return Report.Errors == 0;
		}

		/// <summary>
		/// Collects a module's type builders, nested ones included.
		/// </summary>
		/// <remarks>
		/// ModuleBuilder.types holds the top-level types only, so a nested type is reached through the
		/// subtypes array of the type that declares it - and a compiled plugin nests a great deal of what
		/// it defines, including the interfaces its classes implement.
		/// </remarks>
		private static void CollectTypeBuilders(TypeBuilder[] typeBuilders, List<TypeBuilder> collected)
		{
			if (typeBuilders == null)
			{
				return;
			}
			foreach (TypeBuilder typeBuilder in typeBuilders)
			{
				if (typeBuilder == null)
				{
					continue;
				}
				collected.Add(typeBuilder);
				CollectTypeBuilders(TypeBuilderSubtypesField.GetValue(typeBuilder) as TypeBuilder[], collected);
			}
		}

		/// <summary>
		/// Indexes every method of a created type and of the types nested inside it by metadata token.
		/// </summary>
		/// <remarks>
		/// ModuleBuilder.GetTypes answers with the top-level types of a dynamic module only, so the nested
		/// ones are walked by hand; on a created type the ordinary reflection API lists them.
		/// </remarks>
		private static void CollectCreatedMethods(Type created, Dictionary<int, MethodInfo> createdMethods)
		{
			foreach (MethodInfo method in created.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
			{
				createdMethods[method.MetadataToken] = method;
			}
			foreach (Type nested in created.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
			{
				CollectCreatedMethods(nested, createdMethods);
			}
		}

		/// <summary>
		/// Replaces every method override declaration that still stands for a type that is being built
		/// with the method it stands for, so that the metadata writer can name it.
		/// </summary>
		/// <remarks>
		/// Mono writes an override by asking the runtime for the metadata token of its declaration
		/// (sre-save.c, mono_image_add_methodimpl), and a declaration that is still a MethodBuilder has
		/// none to give. That refusal is reported with g_error, which on Windows is a non-continuable
		/// RaiseException - no catch, no AppDomain.UnhandledException handler and no FirstChanceException
		/// handler can see it, so the process dies inside the runtime instead of the compile failing.
		///
		/// mcs leaves that shape behind whenever an explicit interface implementation targets an
		/// interface declared in the sources being compiled, because MethodSpec.GetMetaInfo answers with
		/// the interface's own MethodBuilder before the interface exists. Interfaces that are generic and
		/// instantiated with the parameters of the type being built arrive as a MethodOnTypeBuilderInst
		/// instead, which the writer refuses in the same place. By the time the types are closed they all
		/// exist, and each builder and the created method it stands for carry the same metadata token, so
		/// a declaration is resolved without knowing which builder is which.
		/// </remarks>
		private static int RepairMethodOverrideDeclarations(AssemblyBuilder assembly)
		{
			if (assembly == null || MethodOverrideField == null || ModuleTypeBuildersField == null || AssemblyModulesField == null || TypeBuilderMethodsField == null || TypeBuilderSubtypesField == null || TypeBuilderCreatedField == null)
			{
				return 0;
			}
			ModuleBuilder[] modules = AssemblyModulesField.GetValue(assembly) as ModuleBuilder[];
			if (modules == null)
			{
				return 0;
			}
			Dictionary<int, MethodInfo> createdMethods = new Dictionary<int, MethodInfo>();
			foreach (ModuleBuilder module in modules)
			{
				if (module == null)
				{
					continue;
				}
				foreach (Type created in module.GetTypes())
				{
					CollectCreatedMethods(created, createdMethods);
				}
			}
			int repaired = 0;
			List<string> unresolved = null;
			foreach (ModuleBuilder module2 in modules)
			{
				if (module2 == null)
				{
					continue;
				}
				TypeBuilder[] typeBuilders = ModuleTypeBuildersField.GetValue(module2) as TypeBuilder[];
				if (typeBuilders == null)
				{
					continue;
				}
				List<TypeBuilder> allBuilders = new List<TypeBuilder>();
				CollectTypeBuilders(typeBuilders, allBuilders);
				foreach (TypeBuilder typeBuilder in allBuilders)
				{
					// A declaration is resolved below by looking its token up among the methods of the
					// created types, so a method of a type that was never closed has nothing to match
					// and is left alone here.
					if (typeBuilder == null || TypeBuilderCreatedField.GetValue(typeBuilder) == null)
					{
						continue;
					}
					MethodBuilder[] declaredMethods = TypeBuilderMethodsField.GetValue(typeBuilder) as MethodBuilder[];
					if (declaredMethods == null)
					{
						continue;
					}
					foreach (MethodBuilder methodBuilder in declaredMethods)
					{
						if (methodBuilder == null)
						{
							continue;
						}
						MethodInfo[] overrides = MethodOverrideField.GetValue(methodBuilder) as MethodInfo[];
						if (overrides == null || overrides.Length == 0)
						{
							continue;
						}
						bool changed = false;
						for (int i = 0; i < overrides.Length; i++)
						{
							MethodInfo instance = overrides[i];
							if (instance != null && instance.GetType().Name == "MethodOnTypeBuilderInst")
							{
								MethodInfo resolvedInstance = ResolveOnTypeBuilderInst(instance, createdMethods);
								if (resolvedInstance != null)
								{
									overrides[i] = resolvedInstance;
									changed = true;
									repaired++;
									continue;
								}
								if (unresolved == null)
								{
									unresolved = new List<string>();
								}
								unresolved.Add(string.Format("{0} for {1}::{2}", instance.Name, typeBuilder.FullName, methodBuilder.Name));
								continue;
							}
							MethodBuilder declaration = instance as MethodBuilder;
							if (declaration == null)
							{
								continue;
							}
							TypeBuilder declaringType = declaration.DeclaringType as TypeBuilder;
							MethodInfo resolved;
							if ((declaringType == null || TypeBuilderCreatedField.GetValue(declaringType) != null) && createdMethods.TryGetValue(ReadMethodToken(declaration), out resolved) && resolved != null && resolved.Name == declaration.Name)
							{
								overrides[i] = resolved;
								changed = true;
								repaired++;
								continue;
							}
							if (unresolved == null)
							{
								unresolved = new List<string>();
							}
							unresolved.Add(string.Format("{0}::{1} for {2}::{3}", declaration.DeclaringType, declaration.Name, typeBuilder.FullName, methodBuilder.Name));
						}
						if (changed)
						{
							MethodOverrideField.SetValue(methodBuilder, overrides);
						}
					}
				}
			}
			if (repaired > 0)
			{
				UnityEngine.Debug.Log(string.Format("[DynamicCSharp] resolved {0} method override declaration(s) that mcs left as builders and mono could not write", repaired));
			}
			if (unresolved != null)
			{
				UnityEngine.Debug.LogWarning(string.Format("[DynamicCSharp] {0} method override declaration(s) could not be resolved and will fail the metadata write: {1}", unresolved.Count, string.Join("; ", unresolved.ToArray())));
			}
			return repaired;
		}

		/// <summary>
		/// Reads the metadata token a method builder will carry in the assembly being written.
		/// </summary>
		/// <remarks>
		/// MethodBuilder does not override MemberInfo.MetadataToken, and that base implementation only
		/// throws InvalidOperationException - on every engine this project runs on, 2018.1.9f2 through
		/// 2021.3.45f2 alike, whose own mscorlib carries no MetadataToken declaration in MethodBuilder
		/// at all. Reading it on a builder therefore throws whether the type is closed or not, which is
		/// how the repair used to give up on a declaration: the override stayed a builder and Save()
		/// then died in the runtime with "requested token for MethodBuilder". GetToken() is the token to
		/// read instead - it answers from the builder's metadata table index, the same number the created
		/// method reports. MetadataToken is still tried first so that a runtime which implements it is
		/// preferred, and the InvalidOperationException it raises on Mono is the case this method exists
		/// for.
		/// </remarks>
		private static int ReadMethodToken(MethodBuilder methodBuilder)
		{
			try
			{
				return methodBuilder.MetadataToken;
			}
			catch (InvalidOperationException)
			{
				return methodBuilder.GetToken().Token;
			}
		}

		/// <summary>
		/// Resolves a method that a builder type implements an interface through while its interface is
		/// only an instantiation of a generic type built from the declaring builder's own parameters.
		/// </summary>
		/// <remarks>
		/// mcs hands such an interface method over as a MethodOnTypeBuilderInst, and the instantiation it
		/// carries is read back on the created types where that is possible. It is not possible while the
		/// interface itself is still a builder: rebuilding the instantiation answers with another
		/// TypeBuilderInstantiation, and that type answers every method query with NotSupportedException.
		/// The declaration is then taken from the created methods by token instead - which is also what a
		/// candidate read off a builder needs, because TypeBuilder.GetMethods answers with the builders it
		/// holds and the writer cannot name a builder.
		/// </remarks>
		private static MethodInfo ResolveOnTypeBuilderInst(MethodInfo instance, Dictionary<int, MethodInfo> createdMethods)
		{
			Type instanceType = instance.GetType();
			FieldInfo instantiationField = FindField(instanceType, "instantiation");
			FieldInfo baseMethodField = FindField(instanceType, "base_method");
			if (instantiationField == null || baseMethodField == null)
			{
				return null;
			}
			Type instantiation = instantiationField.GetValue(instance) as Type;
			MethodInfo baseMethod = baseMethodField.GetValue(instance) as MethodInfo;
			if (instantiation == null || baseMethod == null)
			{
				return null;
			}
			Type inflated = ResolveBuilderType(instantiation);
			if (inflated == null)
			{
				return null;
			}
			int baseToken = 0;
			try
			{
				baseToken = baseMethod.MetadataToken;
			}
			catch (InvalidOperationException)
			{
			}
			// A generic interface method is left as its definition: a MethodImpl row cannot name a
			// method specification, and the parameters the interface is instantiated with are already
			// part of the interface type the row refers to.
			MethodInfo bySignature = null;
			MethodInfo[] inflatedMethods;
			try
			{
				inflatedMethods = inflated.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
			}
			catch (NotSupportedException)
			{
				// An instantiation of a generic type that is still a builder: it has no methods to list,
				// and no argument list changes that. The method the definition was declared with is in
				// the index under the token this declaration reports, so it is read from there.
				MethodInfo defined = ResolveCreatedMethod(baseMethod, createdMethods);
				if (defined == null)
				{
					// Nothing the writer can name: let the compile fail here instead of leaving a
					// declaration in the overrides for Save() to abort the process over.
					throw;
				}
				return defined;
			}
			foreach (MethodInfo candidate in inflatedMethods)
			{
				if (candidate.Name != baseMethod.Name)
				{
					continue;
				}
				if (baseToken != 0)
				{
					int candidateToken = 0;
					try
					{
						candidateToken = candidate.MetadataToken;
					}
					catch (InvalidOperationException)
					{
					}
					if (candidateToken == baseToken)
					{
						return candidate;
					}
				}
				if (bySignature == null && candidate.GetParameters().Length == baseMethod.GetParameters().Length)
				{
					bySignature = candidate;
				}
			}
			MethodInfo created = ResolveCreatedMethod(bySignature, createdMethods);
			if (created == null && bySignature != null)
			{
				// A candidate read off a builder that no created type registered: the writer has no
				// token for it and Save() would abort the process over it.
				throw new NotSupportedException(string.Format("the method override declaration {0}::{1} is a method builder no created method accounts for", bySignature.DeclaringType, bySignature.Name));
			}
			return created;
		}

		/// <summary>
		/// Answers with the runtime method a declaration stands for, or with the declaration itself when
		/// it is already one, and with null when the declaration is a builder the created types do not
		/// know.
		/// </summary>
		/// <remarks>
		/// A declaration that is not a builder already carries the token the writer asks it for - a
		/// method definition when it is in this module, a member reference when it is not - so it is
		/// answered as it stands. A declaration that is still a builder is named by the token it will
		/// carry, which ReadMethodToken answers with and which the created method is indexed under.
		/// </remarks>
		private static MethodInfo ResolveCreatedMethod(MethodInfo declaration, Dictionary<int, MethodInfo> createdMethods)
		{
			if (declaration == null)
			{
				return null;
			}
			MethodBuilder methodBuilder = declaration as MethodBuilder;
			if (methodBuilder == null)
			{
				return declaration;
			}
			MethodInfo created;
			if (!createdMethods.TryGetValue(ReadMethodToken(methodBuilder), out created) || created == null || created.Name != declaration.Name)
			{
				return null;
			}
			return created;
		}

		/// <summary>
		/// Rebuilds a type on the created types, which answers with an ordinary runtime type where a
		/// generic parameter of a builder is one of the arguments.
		/// </summary>
		private static Type ResolveBuilderType(Type type)
		{
			if (type == null)
			{
				return null;
			}
			if (type.IsGenericParameter)
			{
				TypeBuilder declaringBuilder = type.DeclaringType as TypeBuilder;
				if (declaringBuilder != null && TypeBuilderCreatedField.GetValue(declaringBuilder) as Type != null)
				{
					Type[] genericArguments = (TypeBuilderCreatedField.GetValue(declaringBuilder) as Type).GetGenericArguments();
					int position = type.GenericParameterPosition;
					if (position >= 0 && position < genericArguments.Length)
					{
						return genericArguments[position];
					}
				}
				return type;
			}
			if (type.IsArray)
			{
				Type element = ResolveBuilderType(type.GetElementType());
				if (element == null)
				{
					return null;
				}
				return (type.GetArrayRank() == 1) ? element.MakeArrayType() : element.MakeArrayType(type.GetArrayRank());
			}
			if (type.GetType().Name != "TypeBuilderInstantiation")
			{
				return type;
			}
			Type[] arguments = type.GetGenericArguments();
			Type[] resolvedArguments = new Type[arguments.Length];
			for (int i = 0; i < arguments.Length; i++)
			{
				resolvedArguments[i] = ResolveBuilderType(arguments[i]);
				if (resolvedArguments[i] == null)
				{
					return null;
				}
			}
			Type definition = null;
			try
			{
				definition = type.GetGenericTypeDefinition();
			}
			catch (InvalidOperationException)
			{
			}
			if (definition == null)
			{
				FieldInfo definitionField = FindField(type.GetType(), "generic_type");
				definition = ((definitionField != null) ? (definitionField.GetValue(type) as Type) : null);
			}
			if (definition == null)
			{
				return null;
			}
			try
			{
				return definition.MakeGenericType(resolvedArguments);
			}
			catch (ArgumentException)
			{
				return null;
			}
			catch (NotSupportedException)
			{
				return null;
			}
		}

		private static FieldInfo FindField(Type type, string name)
		{
			for (Type current = type; current != null; current = current.BaseType)
			{
				FieldInfo field = current.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field != null)
				{
					return field;
				}
			}
			return null;
		}

		public static void Parse(SeekableStreamReader reader, SourceFile source, ModuleContainer module, ParserSession session, Report report)
		{
			CompilationSourceFile compilationSourceFile = new CompilationSourceFile(module, source);
			module.AddTypeContainer(compilationSourceFile);
			CSharpParser cSharpParser = new CSharpParser(reader, compilationSourceFile, report, session);
			cSharpParser.parse();
		}
	}
}
