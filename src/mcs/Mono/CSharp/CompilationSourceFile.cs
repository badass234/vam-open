using System.Collections.Generic;
using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp
{
	public class CompilationSourceFile : NamespaceContainer
	{
		private readonly SourceFile file;

		private CompileUnitEntry comp_unit;

		private Dictionary<string, SourceFile> include_files;

		private Dictionary<string, bool> conditionals;

		public CompileUnitEntry SymbolUnitEntry => comp_unit;

		public string FileName => file.Name;

		public SourceFile SourceFile => file;

		public CompilationSourceFile(ModuleContainer parent, SourceFile sourceFile)
			: this(parent)
		{
			file = sourceFile;
		}

		public CompilationSourceFile(ModuleContainer parent)
			: base(parent)
		{
		}

		public void AddIncludeFile(SourceFile file)
		{
			if (file != this.file)
			{
				if (include_files == null)
				{
					include_files = new Dictionary<string, SourceFile>();
				}
				if (!include_files.ContainsKey(file.FullPathName))
				{
					include_files.Add(file.FullPathName, file);
				}
			}
		}

		public void AddDefine(string value)
		{
			if (conditionals == null)
			{
				conditionals = new Dictionary<string, bool>(2);
			}
			conditionals[value] = true;
		}

		public void AddUndefine(string value)
		{
			if (conditionals == null)
			{
				conditionals = new Dictionary<string, bool>(2);
			}
			conditionals[value] = false;
		}

		public override void PrepareEmit()
		{
			MonoSymbolFile symbolWriter = Module.DeclaringAssembly.SymbolWriter;
			if (symbolWriter != null)
			{
				CreateUnitSymbolInfo(symbolWriter);
			}
			base.PrepareEmit();
		}

		private void CreateUnitSymbolInfo(MonoSymbolFile symwriter)
		{
			SourceFileEntry source = file.CreateSymbolInfo(symwriter);
			comp_unit = new CompileUnitEntry(symwriter, source);
			if (include_files == null)
			{
				return;
			}
			foreach (SourceFile value in include_files.Values)
			{
				source = value.CreateSymbolInfo(symwriter);
				comp_unit.AddFile(source);
			}
		}

		public bool IsConditionalDefined(string value)
		{
			if (conditionals != null)
			{
				bool value2;
				if (conditionals.TryGetValue(value, out value2))
				{
					return value2;
				}
				if (conditionals.ContainsKey(value))
				{
					return false;
				}
			}
			return Compiler.Settings.IsConditionalSymbolDefined(value);
		}

		public override void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
