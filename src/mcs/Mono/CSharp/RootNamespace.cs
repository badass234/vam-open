using System.Collections.Generic;

namespace Mono.CSharp
{
	public class RootNamespace : Namespace
	{
		private readonly string alias_name;

		private readonly Dictionary<string, Namespace> all_namespaces;

		public string Alias => alias_name;

		public RootNamespace(string alias_name)
		{
			this.alias_name = alias_name;
			RegisterNamespace(this);
			all_namespaces = new Dictionary<string, Namespace>();
			all_namespaces.Add("", this);
		}

		public static void Error_GlobalNamespaceRedefined(Report report, Location loc)
		{
			report.Error(1681, loc, "The global extern alias cannot be redefined");
		}

		public List<string> FindTypeNamespaces(IMemberContext ctx, string name, int arity)
		{
			List<string> list = null;
			foreach (KeyValuePair<string, Namespace> all_namespace in all_namespaces)
			{
				if (all_namespace.Value.LookupType(ctx, name, arity, LookupMode.Normal, Location.Null) != null)
				{
					if (list == null)
					{
						list = new List<string>();
					}
					list.Add(all_namespace.Key);
				}
			}
			return list;
		}

		public List<string> FindExtensionMethodNamespaces(IMemberContext ctx, string name, int arity)
		{
			List<string> list = null;
			foreach (KeyValuePair<string, Namespace> all_namespace in all_namespaces)
			{
				if (all_namespace.Key.Length != 0 && all_namespace.Value.LookupExtensionMethod(ctx, name, arity) != null)
				{
					if (list == null)
					{
						list = new List<string>();
					}
					list.Add(all_namespace.Key);
				}
			}
			return list;
		}

		public void RegisterNamespace(Namespace child)
		{
			if (child != this)
			{
				all_namespaces.Add(child.Name, child);
			}
		}

		public override string GetSignatureForError()
		{
			return alias_name + "::";
		}
	}
}
