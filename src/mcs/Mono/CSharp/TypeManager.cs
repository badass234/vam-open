using System;
using System.Collections.Generic;
using System.Text;

namespace Mono.CSharp
{
	internal class TypeManager
	{
		public static string CSharpName(IList<TypeSpec> types)
		{
			if (types.Count == 0)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < types.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append(types[i].GetSignatureForError());
			}
			return stringBuilder.ToString();
		}

		public static string GetFullNameSignature(MemberSpec mi)
		{
			return mi.GetSignatureForError();
		}

		public static string CSharpSignature(MemberSpec mb)
		{
			return mb.GetSignatureForError();
		}

		public static bool IsFamilyAccessible(TypeSpec type, TypeSpec parent)
		{
			if (type.Kind == MemberKind.TypeParameter && parent.Kind == MemberKind.TypeParameter)
			{
				if (type == parent)
				{
					return true;
				}
				throw new NotImplementedException("net");
			}
			do
			{
				if (IsInstantiationOfSameGenericType(type, parent))
				{
					return true;
				}
				type = type.BaseType;
			}
			while (type != null);
			return false;
		}

		public static bool IsNestedChildOf(TypeSpec type, ITypeDefinition parent)
		{
			if (type == null)
			{
				return false;
			}
			if (type.MemberDefinition == parent)
			{
				return false;
			}
			for (type = type.DeclaringType; type != null; type = type.DeclaringType)
			{
				if (type.MemberDefinition == parent)
				{
					return true;
				}
			}
			return false;
		}

		public static TypeSpec GetElementType(TypeSpec t)
		{
			return ((ElementTypeSpec)t).Element;
		}

		public static bool HasElementType(TypeSpec t)
		{
			return t is ElementTypeSpec;
		}

		public static bool VerifyUnmanaged(ModuleContainer rc, TypeSpec t, Location loc)
		{
			if (t.IsUnmanaged)
			{
				return true;
			}
			while (t.IsPointer)
			{
				t = ((ElementTypeSpec)t).Element;
			}
			rc.Compiler.Report.SymbolRelatedToPreviousError(t);
			rc.Compiler.Report.Error(208, loc, "Cannot take the address of, get the size of, or declare a pointer to a managed type `{0}'", t.GetSignatureForError());
			return false;
		}

		public static bool IsGenericParameter(TypeSpec type)
		{
			return type.IsGenericParameter;
		}

		public static bool IsGenericType(TypeSpec type)
		{
			return type.IsGeneric;
		}

		public static TypeSpec[] GetTypeArguments(TypeSpec t)
		{
			return t.TypeArguments;
		}

		public static bool IsInstantiationOfSameGenericType(TypeSpec type, TypeSpec parent)
		{
			if (type != parent)
			{
				return type.MemberDefinition == parent.MemberDefinition;
			}
			return true;
		}
	}
}
