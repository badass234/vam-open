using System;
using System.Collections.Generic;
using System.Linq;

namespace Mono.CSharp
{
	public class Namespace
	{
		private readonly Namespace parent;

		private string fullname;

		protected Dictionary<string, Namespace> namespaces;

		protected Dictionary<string, IList<TypeSpec>> types;

		private List<TypeSpec> extension_method_types;

		private Dictionary<string, TypeSpec> cached_types;

		private bool cls_checked;

		public string Name => fullname;

		public Namespace Parent => parent;

		public Namespace(Namespace parent, string name)
			: this()
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			this.parent = parent;
			string text = parent?.fullname;
			if (text == null)
			{
				fullname = name;
			}
			else
			{
				fullname = text + "." + name;
			}
			while (parent.parent != null)
			{
				parent = parent.parent;
			}
			RootNamespace obj = parent as RootNamespace;
			if (obj == null)
			{
				throw new InternalErrorException("Root namespaces must be created using RootNamespace");
			}
			obj.RegisterNamespace(this);
		}

		protected Namespace()
		{
			namespaces = new Dictionary<string, Namespace>();
			cached_types = new Dictionary<string, TypeSpec>();
		}

		public Namespace AddNamespace(MemberName name)
		{
			return ((name.Left == null) ? this : AddNamespace(name.Left)).TryAddNamespace(name.Basename);
		}

		private Namespace TryAddNamespace(string name)
		{
			Namespace value;
			if (!namespaces.TryGetValue(name, out value))
			{
				value = new Namespace(this, name);
				namespaces.Add(name, value);
			}
			return value;
		}

		public bool TryGetNamespace(string name, out Namespace ns)
		{
			return namespaces.TryGetValue(name, out ns);
		}

		public Namespace GetNamespace(string name, bool create)
		{
			int num = name.IndexOf('.');
			string text = ((num < 0) ? name : name.Substring(0, num));
			Namespace value;
			if (!namespaces.TryGetValue(text, out value))
			{
				if (!create)
				{
					return null;
				}
				value = new Namespace(this, text);
				namespaces.Add(text, value);
			}
			if (num >= 0)
			{
				value = value.GetNamespace(name.Substring(num + 1), create);
			}
			return value;
		}

		public IList<TypeSpec> GetAllTypes(string name)
		{
			IList<TypeSpec> value;
			if (types == null || !types.TryGetValue(name, out value))
			{
				return null;
			}
			return value;
		}

		public virtual string GetSignatureForError()
		{
			return fullname;
		}

		public TypeSpec LookupType(IMemberContext ctx, string name, int arity, LookupMode mode, Location loc)
		{
			if (types == null)
			{
				return null;
			}
			TypeSpec value = null;
			if (arity == 0 && cached_types.TryGetValue(name, out value) && (value != null || mode != LookupMode.IgnoreAccessibility))
			{
				return value;
			}
			IList<TypeSpec> value2;
			if (!types.TryGetValue(name, out value2))
			{
				return null;
			}
			foreach (TypeSpec item in value2)
			{
				if (item.Arity == arity)
				{
					if (value == null)
					{
						if ((item.Modifiers & Modifiers.INTERNAL) == 0 || item.MemberDefinition.IsInternalAsPublic(ctx.Module.DeclaringAssembly) || mode == LookupMode.IgnoreAccessibility)
						{
							value = item;
						}
						continue;
					}
					if (value.MemberDefinition.IsImported && item.MemberDefinition.IsImported)
					{
						if (item.Kind == MemberKind.MissingType)
						{
							continue;
						}
						if (value.Kind == MemberKind.MissingType)
						{
							value = item;
							continue;
						}
						if (mode == LookupMode.Normal)
						{
							ctx.Module.Compiler.Report.SymbolRelatedToPreviousError(value);
							ctx.Module.Compiler.Report.SymbolRelatedToPreviousError(item);
							ctx.Module.Compiler.Report.Error(433, loc, "The imported type `{0}' is defined multiple times", item.GetSignatureForError());
						}
						break;
					}
					if (item.Kind == MemberKind.MissingType)
					{
						continue;
					}
					if (value.MemberDefinition.IsImported)
					{
						value = item;
					}
					if (((value.Modifiers & Modifiers.INTERNAL) != 0 && !value.MemberDefinition.IsInternalAsPublic(ctx.Module.DeclaringAssembly)) || mode != LookupMode.Normal)
					{
						continue;
					}
					if (item.MemberDefinition.IsImported)
					{
						ctx.Module.Compiler.Report.SymbolRelatedToPreviousError(value);
						ctx.Module.Compiler.Report.SymbolRelatedToPreviousError(item);
					}
					ctx.Module.Compiler.Report.Warning(436, 2, loc, "The type `{0}' conflicts with the imported type of same name'. Ignoring the imported type definition", value.GetSignatureForError());
				}
				if (arity < 0)
				{
					if (value == null)
					{
						value = item;
					}
					else if (System.Math.Abs(item.Arity + arity) < System.Math.Abs(value.Arity + arity))
					{
						value = item;
					}
				}
			}
			if (arity == 0 && mode == LookupMode.Normal)
			{
				cached_types.Add(name, value);
			}
			if (value != null)
			{
				List<MissingTypeSpecReference> missingDependencies = value.GetMissingDependencies();
				if (missingDependencies != null)
				{
					ImportedTypeDefinition.Error_MissingDependency(ctx, missingDependencies, loc);
				}
			}
			return value;
		}

		public FullNamedExpression LookupTypeOrNamespace(IMemberContext ctx, string name, int arity, LookupMode mode, Location loc)
		{
			TypeSpec typeSpec = LookupType(ctx, name, arity, mode, loc);
			Namespace value;
			if (arity == 0 && namespaces.TryGetValue(name, out value))
			{
				if (typeSpec == null)
				{
					return new NamespaceExpression(value, loc);
				}
				if (mode != LookupMode.Probing)
				{
					ctx.Module.Compiler.Report.Warning(437, 2, loc, "The type `{0}' conflicts with the imported namespace `{1}'. Using the definition found in the source file", typeSpec.GetSignatureForError(), value.GetSignatureForError());
				}
				if (typeSpec.MemberDefinition.IsImported)
				{
					return new NamespaceExpression(value, loc);
				}
			}
			if (typeSpec == null)
			{
				return null;
			}
			return new TypeExpression(typeSpec, loc);
		}

		public IEnumerable<string> CompletionGetTypesStartingWith(string prefix)
		{
			if (types == null)
			{
				return Enumerable.Empty<string>();
			}
			IEnumerable<string> enumerable = from item in types
				where item.Key.StartsWith(prefix) && item.Value.Any((TypeSpec l) => (l.Modifiers & Modifiers.PUBLIC) != 0)
				select item.Key;
			if (namespaces != null)
			{
				enumerable = enumerable.Concat(from item in namespaces
					where item.Key.StartsWith(prefix)
					select item.Key);
			}
			return enumerable;
		}

		public List<MethodSpec> LookupExtensionMethod(IMemberContext invocationContext, string name, int arity)
		{
			if (extension_method_types == null)
			{
				return null;
			}
			List<MethodSpec> list = null;
			for (int i = 0; i < extension_method_types.Count; i++)
			{
				TypeSpec typeSpec = extension_method_types[i];
				if ((typeSpec.Modifiers & Modifiers.METHOD_EXTENSION) == 0)
				{
					if (extension_method_types.Count == 1)
					{
						extension_method_types = null;
						return list;
					}
					extension_method_types.RemoveAt(i--);
					continue;
				}
				List<MethodSpec> list2 = typeSpec.MemberCache.FindExtensionMethods(invocationContext, name, arity);
				if (list2 != null)
				{
					if (list == null)
					{
						list = list2;
					}
					else
					{
						list.AddRange(list2);
					}
				}
			}
			return list;
		}

		public void AddType(ModuleContainer module, TypeSpec ts)
		{
			if (types == null)
			{
				types = new Dictionary<string, IList<TypeSpec>>(64);
			}
			if (ts.IsClass && ts.Arity == 0)
			{
				bool num;
				if (!ts.MemberDefinition.IsImported)
				{
					if (ts.IsStatic)
					{
						goto IL_005b;
					}
					num = ts.MemberDefinition.IsPartial;
				}
				else
				{
					num = (ts.Modifiers & Modifiers.METHOD_EXTENSION) != 0;
				}
				if (num)
				{
					goto IL_005b;
				}
			}
			goto IL_007a;
			IL_007a:
			string name = ts.Name;
			IList<TypeSpec> value;
			if (types.TryGetValue(name, out value))
			{
				if (value.Count == 1)
				{
					TypeSpec typeSpec = value[0];
					if (ts.Arity == typeSpec.Arity)
					{
						TypeSpec typeSpec2 = IsImportedTypeOverride(module, ts, typeSpec);
						if (typeSpec2 == typeSpec)
						{
							return;
						}
						if (typeSpec2 != null)
						{
							value[0] = typeSpec2;
							return;
						}
					}
					value = new List<TypeSpec>();
					value.Add(typeSpec);
					types[name] = value;
				}
				else
				{
					for (int i = 0; i < value.Count; i++)
					{
						TypeSpec typeSpec = value[i];
						if (ts.Arity == typeSpec.Arity)
						{
							TypeSpec typeSpec2 = IsImportedTypeOverride(module, ts, typeSpec);
							if (typeSpec2 == typeSpec)
							{
								return;
							}
							if (typeSpec2 != null)
							{
								value.RemoveAt(i);
								i--;
							}
						}
					}
				}
				value.Add(ts);
			}
			else
			{
				types.Add(name, new TypeSpec[1] { ts });
			}
			return;
			IL_005b:
			if (extension_method_types == null)
			{
				extension_method_types = new List<TypeSpec>();
			}
			extension_method_types.Add(ts);
			goto IL_007a;
		}

		public static TypeSpec IsImportedTypeOverride(ModuleContainer module, TypeSpec ts, TypeSpec found)
		{
			bool flag = (ts.Modifiers & Modifiers.PUBLIC) != 0 || ts.MemberDefinition.IsInternalAsPublic(module.DeclaringAssembly);
			bool flag2 = (found.Modifiers & Modifiers.PUBLIC) != 0 || found.MemberDefinition.IsInternalAsPublic(module.DeclaringAssembly);
			if (flag && !flag2)
			{
				return ts;
			}
			if (!flag)
			{
				return found;
			}
			return null;
		}

		public void RemoveContainer(TypeContainer tc)
		{
			IList<TypeSpec> value;
			if (types.TryGetValue(tc.MemberName.Name, out value))
			{
				for (int i = 0; i < value.Count; i++)
				{
					if (tc.MemberName.Arity == value[i].Arity)
					{
						if (value.Count == 1)
						{
							types.Remove(tc.MemberName.Name);
						}
						else
						{
							value.RemoveAt(i);
						}
						break;
					}
				}
			}
			cached_types.Remove(tc.MemberName.Basename);
		}

		public void SetBuiltinType(BuiltinTypeSpec pts)
		{
			IList<TypeSpec> list = types[pts.Name];
			cached_types.Remove(pts.Name);
			if (list.Count == 1)
			{
				types[pts.Name][0] = pts;
				return;
			}
			throw new NotImplementedException();
		}

		public void VerifyClsCompliance()
		{
			if (types == null || cls_checked)
			{
				return;
			}
			cls_checked = true;
			Dictionary<string, List<TypeSpec>> dictionary = new Dictionary<string, List<TypeSpec>>(StringComparer.OrdinalIgnoreCase);
			foreach (IList<TypeSpec> value2 in types.Values)
			{
				foreach (TypeSpec item in value2)
				{
					if ((item.Modifiers & Modifiers.PUBLIC) != 0 && item.IsCLSCompliant())
					{
						List<TypeSpec> value;
						if (!dictionary.TryGetValue(item.Name, out value))
						{
							value = new List<TypeSpec>();
							dictionary.Add(item.Name, value);
						}
						value.Add(item);
					}
				}
			}
			foreach (List<TypeSpec> value3 in dictionary.Values)
			{
				if (value3.Count < 2)
				{
					continue;
				}
				bool flag = true;
				foreach (TypeSpec item2 in value3)
				{
					flag = item2.Name == value3[0].Name;
					if (!flag)
					{
						break;
					}
				}
				if (flag)
				{
					continue;
				}
				TypeContainer typeContainer = null;
				foreach (TypeSpec item3 in value3)
				{
					if (!item3.MemberDefinition.IsImported)
					{
						typeContainer?.Compiler.Report.SymbolRelatedToPreviousError(typeContainer);
						typeContainer = item3.MemberDefinition as TypeContainer;
					}
					else
					{
						typeContainer.Compiler.Report.SymbolRelatedToPreviousError(item3);
					}
				}
				typeContainer.Compiler.Report.Warning(3005, 1, typeContainer.Location, "Identifier `{0}' differing only in case is not CLS-compliant", typeContainer.GetSignatureForError());
			}
		}
	}
}
