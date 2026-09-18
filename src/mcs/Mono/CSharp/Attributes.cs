using System.Collections.Generic;

namespace Mono.CSharp
{
	public class Attributes
	{
		public readonly List<Attribute> Attrs;

		public Attributes(Attribute a)
		{
			Attrs = new List<Attribute>();
			Attrs.Add(a);
		}

		public Attributes(List<Attribute> attrs)
		{
			Attrs = attrs ?? new List<Attribute>();
		}

		public void AddAttribute(Attribute attr)
		{
			Attrs.Add(attr);
		}

		public void AddAttributes(List<Attribute> attrs)
		{
			Attrs.AddRange(attrs);
		}

		public void AttachTo(Attributable attributable, IMemberContext context)
		{
			foreach (Attribute attr in Attrs)
			{
				attr.AttachTo(attributable, context);
			}
		}

		public Attributes Clone()
		{
			List<Attribute> list = new List<Attribute>(Attrs.Count);
			foreach (Attribute attr in Attrs)
			{
				list.Add(attr.Clone());
			}
			return new Attributes(list);
		}

		public bool CheckTargets()
		{
			for (int i = 0; i < Attrs.Count; i++)
			{
				if (!Attrs[i].CheckTarget())
				{
					Attrs.RemoveAt(i--);
				}
			}
			return true;
		}

		public void ConvertGlobalAttributes(TypeContainer member, NamespaceContainer currentNamespace, bool isGlobal)
		{
			string[] validAttributeTargets = member.ValidAttributeTargets;
			for (int i = 0; i < Attrs.Count; i++)
			{
				Attribute attribute = Attrs[0];
				if (attribute.ExplicitTarget == null)
				{
					continue;
				}
				int j;
				for (j = 0; j < validAttributeTargets.Length; j++)
				{
					if (attribute.ExplicitTarget == validAttributeTargets[j])
					{
						j = -1;
						break;
					}
				}
				if (j >= 0 && isGlobal)
				{
					member.Module.AddAttribute(attribute, currentNamespace);
					Attrs.RemoveAt(i);
					i--;
				}
			}
		}

		public bool HasResolveError()
		{
			foreach (Attribute attr in Attrs)
			{
				if (attr.ResolveError)
				{
					return true;
				}
			}
			return false;
		}

		public Attribute Search(PredefinedAttribute t)
		{
			return Search(null, t);
		}

		public Attribute Search(string explicitTarget, PredefinedAttribute t)
		{
			foreach (Attribute attr in Attrs)
			{
				if ((explicitTarget == null || !(attr.ExplicitTarget != explicitTarget)) && attr.ResolveTypeForComparison() == t)
				{
					return attr;
				}
			}
			return null;
		}

		public Attribute[] SearchMulti(PredefinedAttribute t)
		{
			List<Attribute> list = null;
			foreach (Attribute attr in Attrs)
			{
				if (attr.ResolveTypeForComparison() == t)
				{
					if (list == null)
					{
						list = new List<Attribute>(Attrs.Count);
					}
					list.Add(attr);
				}
			}
			return list?.ToArray();
		}

		public void Emit()
		{
			CheckTargets();
			Dictionary<Attribute, List<Attribute>> dictionary = ((Attrs.Count > 1) ? new Dictionary<Attribute, List<Attribute>>() : null);
			foreach (Attribute attr in Attrs)
			{
				attr.Emit(dictionary);
			}
			if (dictionary == null || dictionary.Count == 0)
			{
				return;
			}
			foreach (KeyValuePair<Attribute, List<Attribute>> item in dictionary)
			{
				if (item.Value == null)
				{
					continue;
				}
				Attribute key = item.Key;
				foreach (Attribute item2 in item.Value)
				{
					key.Report.SymbolRelatedToPreviousError(item2.Location, "");
				}
				key.Report.Error(579, key.Location, "The attribute `{0}' cannot be applied multiple times", key.GetSignatureForError());
			}
		}

		public bool Contains(PredefinedAttribute t)
		{
			return Search(t) != null;
		}
	}
}
