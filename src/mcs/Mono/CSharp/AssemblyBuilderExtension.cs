using System;
using System.Reflection;
using System.Security;

namespace Mono.CSharp
{
	public class AssemblyBuilderExtension
	{
		private readonly CompilerContext ctx;

		public AssemblyBuilderExtension(CompilerContext ctx)
		{
			this.ctx = ctx;
		}

		public virtual Module AddModule(string module)
		{
			ctx.Report.RuntimeMissingSupport(Location.Null, "-addmodule");
			return null;
		}

		public virtual void AddPermissionRequests(PermissionSet[] permissions)
		{
			ctx.Report.RuntimeMissingSupport(Location.Null, "assembly declarative security");
		}

		public virtual void AddTypeForwarder(TypeSpec type, Location loc)
		{
			ctx.Report.RuntimeMissingSupport(loc, "TypeForwardedToAttribute");
		}

		public virtual void DefineWin32IconResource(string fileName)
		{
			ctx.Report.RuntimeMissingSupport(Location.Null, "-win32icon");
		}

		public virtual void SetAlgorithmId(uint value, Location loc)
		{
			ctx.Report.RuntimeMissingSupport(loc, "AssemblyAlgorithmIdAttribute");
		}

		public virtual void SetCulture(string culture, Location loc)
		{
			ctx.Report.RuntimeMissingSupport(loc, "AssemblyCultureAttribute");
		}

		public virtual void SetFlags(uint flags, Location loc)
		{
			ctx.Report.RuntimeMissingSupport(loc, "AssemblyFlagsAttribute");
		}

		public virtual void SetVersion(Version version, Location loc)
		{
			ctx.Report.RuntimeMissingSupport(loc, "AssemblyVersionAttribute");
		}
	}
}
