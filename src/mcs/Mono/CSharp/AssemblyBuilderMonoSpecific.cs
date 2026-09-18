using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;

namespace Mono.CSharp
{
	internal class AssemblyBuilderMonoSpecific : AssemblyBuilderExtension
	{
		private static MethodInfo adder_method;

		private static MethodInfo add_permission;

		private static MethodInfo add_type_forwarder;

		private static MethodInfo win32_icon_define;

		private static FieldInfo assembly_version;

		private static FieldInfo assembly_algorithm;

		private static FieldInfo assembly_culture;

		private static FieldInfo assembly_flags;

		private AssemblyBuilder builder;

		public AssemblyBuilderMonoSpecific(AssemblyBuilder ab, CompilerContext ctx)
			: base(ctx)
		{
			builder = ab;
		}

		public override Module AddModule(string module)
		{
			try
			{
				if (adder_method == null)
				{
					adder_method = typeof(AssemblyBuilder).GetMethod("AddModule", BindingFlags.Instance | BindingFlags.NonPublic);
				}
				return (Module)adder_method.Invoke(builder, new object[1] { module });
			}
			catch
			{
				return base.AddModule(module);
			}
		}

		public override void AddPermissionRequests(PermissionSet[] permissions)
		{
			try
			{
				if (add_permission == null)
				{
					add_permission = typeof(AssemblyBuilder).GetMethod("AddPermissionRequests", BindingFlags.Instance | BindingFlags.NonPublic);
				}
				add_permission.Invoke(builder, permissions);
			}
			catch
			{
				base.AddPermissionRequests(permissions);
			}
		}

		public override void AddTypeForwarder(TypeSpec type, Location loc)
		{
			try
			{
				if (add_type_forwarder == null)
				{
					add_type_forwarder = typeof(AssemblyBuilder).GetMethod("AddTypeForwarder", BindingFlags.Instance | BindingFlags.NonPublic);
				}
				add_type_forwarder.Invoke(builder, new object[1] { type.GetMetaInfo() });
			}
			catch
			{
				base.AddTypeForwarder(type, loc);
			}
		}

		public override void DefineWin32IconResource(string fileName)
		{
			try
			{
				if (win32_icon_define == null)
				{
					win32_icon_define = typeof(AssemblyBuilder).GetMethod("DefineIconResource", BindingFlags.Instance | BindingFlags.NonPublic);
				}
				win32_icon_define.Invoke(builder, new object[1] { fileName });
			}
			catch
			{
				base.DefineWin32IconResource(fileName);
			}
		}

		public override void SetAlgorithmId(uint value, Location loc)
		{
			try
			{
				if (assembly_algorithm == null)
				{
					assembly_algorithm = typeof(AssemblyBuilder).GetField("algid", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);
				}
				assembly_algorithm.SetValue(builder, value);
			}
			catch
			{
				base.SetAlgorithmId(value, loc);
			}
		}

		public override void SetCulture(string culture, Location loc)
		{
			try
			{
				if (assembly_culture == null)
				{
					assembly_culture = typeof(AssemblyBuilder).GetField("culture", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);
				}
				assembly_culture.SetValue(builder, culture);
			}
			catch
			{
				base.SetCulture(culture, loc);
			}
		}

		public override void SetFlags(uint flags, Location loc)
		{
			try
			{
				if (assembly_flags == null)
				{
					assembly_flags = typeof(AssemblyBuilder).GetField("flags", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);
				}
				assembly_flags.SetValue(builder, flags);
			}
			catch
			{
				base.SetFlags(flags, loc);
			}
		}

		public override void SetVersion(Version version, Location loc)
		{
			try
			{
				if (assembly_version == null)
				{
					assembly_version = typeof(AssemblyBuilder).GetField("version", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField);
				}
				assembly_version.SetValue(builder, version.ToString(4));
			}
			catch
			{
				base.SetVersion(version, loc);
			}
		}
	}
}
