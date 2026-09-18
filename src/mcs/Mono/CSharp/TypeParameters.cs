using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace Mono.CSharp
{
	public class TypeParameters
	{
		private List<TypeParameter> names;

		private TypeParameterSpec[] types;

		public int Count => names.Count;

		public TypeParameterSpec[] Types => types;

		public TypeParameter this[int index]
		{
			get
			{
				return names[index];
			}
			set
			{
				names[index] = value;
			}
		}

		public TypeParameters()
		{
			names = new List<TypeParameter>();
		}

		public TypeParameters(int count)
		{
			names = new List<TypeParameter>(count);
		}

		public void Add(TypeParameter tparam)
		{
			names.Add(tparam);
		}

		public void Add(TypeParameters tparams)
		{
			names.AddRange(tparams.names);
		}

		public void Create(TypeSpec declaringType, int parentOffset, TypeContainer parent)
		{
			types = new TypeParameterSpec[Count];
			for (int i = 0; i < types.Length; i++)
			{
				TypeParameter typeParameter = names[i];
				typeParameter.Create(declaringType, parent);
				types[i] = typeParameter.Type;
				types[i].DeclaredPosition = i + parentOffset;
				if (typeParameter.Variance != Variance.None && (declaringType == null || (declaringType.Kind != MemberKind.Interface && declaringType.Kind != MemberKind.Delegate)))
				{
					parent.Compiler.Report.Error(1960, typeParameter.Location, "Variant type parameters can only be used with interfaces and delegates");
				}
			}
		}

		public void Define(GenericTypeParameterBuilder[] builders)
		{
			for (int i = 0; i < types.Length; i++)
			{
				names[i].Define(builders[types[i].DeclaredPosition]);
			}
		}

		public TypeParameter Find(string name)
		{
			foreach (TypeParameter name2 in names)
			{
				if (name2.Name == name)
				{
					return name2;
				}
			}
			return null;
		}

		public string[] GetAllNames()
		{
			return names.Select((TypeParameter l) => l.Name).ToArray();
		}

		public string GetSignatureForError()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(',');
				}
				TypeParameter typeParameter = names[i];
				if (typeParameter != null)
				{
					stringBuilder.Append(typeParameter.GetSignatureForError());
				}
			}
			return stringBuilder.ToString();
		}

		public void CheckPartialConstraints(Method part)
		{
			TypeParameters currentTypeParameters = part.CurrentTypeParameters;
			for (int i = 0; i < Count; i++)
			{
				TypeParameter typeParameter = names[i];
				TypeParameter typeParameter2 = currentTypeParameters[i];
				if (typeParameter.Constraints == null)
				{
					if (typeParameter2.Constraints == null)
					{
						continue;
					}
				}
				else if (typeParameter2.Constraints != null && typeParameter.Type.HasSameConstraintsDefinition(typeParameter2.Type))
				{
					continue;
				}
				part.Compiler.Report.SymbolRelatedToPreviousError(this[i].CurrentMemberDefinition.Location, "");
				part.Compiler.Report.Error(761, part.Location, "Partial method declarations of `{0}' have inconsistent constraints for type parameter `{1}'", part.GetSignatureForError(), currentTypeParameters[i].GetSignatureForError());
			}
		}

		public void UpdateConstraints(TypeDefinition part)
		{
			TypeParameters typeParameters = part.MemberName.TypeParameters;
			for (int i = 0; i < Count; i++)
			{
				TypeParameter typeParameter = names[i];
				if (!typeParameter.AddPartialConstraints(part, typeParameters[i]))
				{
					part.Compiler.Report.SymbolRelatedToPreviousError(this[i].CurrentMemberDefinition);
					part.Compiler.Report.Error(265, part.Location, "Partial declarations of `{0}' have inconsistent constraints for type parameter `{1}'", part.GetSignatureForError(), typeParameter.GetSignatureForError());
				}
			}
		}

		public void VerifyClsCompliance()
		{
			foreach (TypeParameter name in names)
			{
				name.VerifyClsCompliance();
			}
		}
	}
}
