using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class AssemblyDefinitionDynamic : AssemblyDefinition
	{
		public AssemblyDefinitionDynamic(ModuleContainer module, string name)
			: base(module, name)
		{
		}

		public AssemblyDefinitionDynamic(ModuleContainer module, string name, string fileName)
			: base(module, name, fileName)
		{
		}

		public Module IncludeModule(string moduleFile)
		{
			return builder_extra.AddModule(moduleFile);
		}

		public override ModuleBuilder CreateModuleBuilder()
		{
			if (file_name == null)
			{
				return Builder.DefineDynamicModule(base.Name, false);
			}
			return base.CreateModuleBuilder();
		}

		public bool Create(AppDomain domain, AssemblyBuilderAccess access)
		{
			ResolveAssemblySecurityAttributes();
			AssemblyName assemblyName = CreateAssemblyName();
			Builder = ((file_name == null) ? domain.DefineDynamicAssembly(assemblyName, access) : domain.DefineDynamicAssembly(assemblyName, access, Dirname(file_name)));
			module.Create(this, CreateModuleBuilder());
			builder_extra = new AssemblyBuilderMonoSpecific(Builder, base.Compiler);
			return true;
		}

		private static string Dirname(string name)
		{
			int num = name.LastIndexOf('/');
			if (num != -1)
			{
				return name.Substring(0, num);
			}
			num = name.LastIndexOf('\\');
			if (num != -1)
			{
				return name.Substring(0, num);
			}
			return ".";
		}

		protected override void SaveModule(PortableExecutableKinds pekind, ImageFileMachine machine)
		{
			try
			{
				typeof(AssemblyBuilder).GetProperty("IsModuleOnly", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetSetMethod(true).Invoke(Builder, new object[1] { true });
			}
			catch
			{
				base.SaveModule(pekind, machine);
			}
			Builder.Save(file_name, pekind, machine);
		}
	}
}
