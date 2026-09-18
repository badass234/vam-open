using System;
using System.Reflection;

namespace Mono.CSharp
{
	internal static class ModifiersExtensions
	{
		public static string AccessibilityName(Modifiers mod)
		{
			switch (mod & Modifiers.AccessibilityMask)
			{
			case Modifiers.PUBLIC:
				return "public";
			case Modifiers.PROTECTED:
				return "protected";
			case Modifiers.PROTECTED | Modifiers.INTERNAL:
				return "protected internal";
			case Modifiers.INTERNAL:
				return "internal";
			case Modifiers.PRIVATE:
				return "private";
			default:
				throw new NotImplementedException(mod.ToString());
			}
		}

		public static string Name(Modifiers i)
		{
			string result = "";
			switch (i)
			{
			case Modifiers.NEW:
				result = "new";
				break;
			case Modifiers.PUBLIC:
				result = "public";
				break;
			case Modifiers.PROTECTED:
				result = "protected";
				break;
			case Modifiers.INTERNAL:
				result = "internal";
				break;
			case Modifiers.PRIVATE:
				result = "private";
				break;
			case Modifiers.ABSTRACT:
				result = "abstract";
				break;
			case Modifiers.SEALED:
				result = "sealed";
				break;
			case Modifiers.STATIC:
				result = "static";
				break;
			case Modifiers.READONLY:
				result = "readonly";
				break;
			case Modifiers.VIRTUAL:
				result = "virtual";
				break;
			case Modifiers.OVERRIDE:
				result = "override";
				break;
			case Modifiers.EXTERN:
				result = "extern";
				break;
			case Modifiers.VOLATILE:
				result = "volatile";
				break;
			case Modifiers.UNSAFE:
				result = "unsafe";
				break;
			case Modifiers.ASYNC:
				result = "async";
				break;
			}
			return result;
		}

		public static bool IsRestrictedModifier(Modifiers modA, Modifiers modB)
		{
			Modifiers modifiers = (Modifiers)0;
			if ((modB & Modifiers.PUBLIC) != 0)
			{
				modifiers = Modifiers.PROTECTED | Modifiers.PRIVATE | Modifiers.INTERNAL;
			}
			else if ((modB & Modifiers.PROTECTED) != 0)
			{
				if ((modB & Modifiers.INTERNAL) != 0)
				{
					modifiers = Modifiers.PROTECTED | Modifiers.INTERNAL;
				}
				modifiers |= Modifiers.PRIVATE;
			}
			else if ((modB & Modifiers.INTERNAL) != 0)
			{
				modifiers = Modifiers.PRIVATE;
			}
			if (modB != modA)
			{
				return (modA & ~modifiers) == 0;
			}
			return false;
		}

		public static TypeAttributes TypeAttr(Modifiers mod_flags, bool is_toplevel)
		{
			TypeAttributes typeAttributes = TypeAttributes.NotPublic;
			if (is_toplevel)
			{
				if ((mod_flags & Modifiers.PUBLIC) != 0)
				{
					typeAttributes = TypeAttributes.Public;
				}
				else if ((mod_flags & Modifiers.PRIVATE) != 0)
				{
					typeAttributes = TypeAttributes.NotPublic;
				}
			}
			else if ((mod_flags & Modifiers.PUBLIC) != 0)
			{
				typeAttributes = TypeAttributes.NestedPublic;
			}
			else if ((mod_flags & Modifiers.PRIVATE) != 0)
			{
				typeAttributes = TypeAttributes.NestedPrivate;
			}
			else if ((mod_flags & (Modifiers.PROTECTED | Modifiers.INTERNAL)) == (Modifiers.PROTECTED | Modifiers.INTERNAL))
			{
				typeAttributes = TypeAttributes.VisibilityMask;
			}
			else if ((mod_flags & Modifiers.PROTECTED) != 0)
			{
				typeAttributes = TypeAttributes.NestedFamily;
			}
			else if ((mod_flags & Modifiers.INTERNAL) != 0)
			{
				typeAttributes = TypeAttributes.NestedAssembly;
			}
			if ((mod_flags & Modifiers.SEALED) != 0)
			{
				typeAttributes |= TypeAttributes.Sealed;
			}
			if ((mod_flags & Modifiers.ABSTRACT) != 0)
			{
				typeAttributes |= TypeAttributes.Abstract;
			}
			return typeAttributes;
		}

		public static FieldAttributes FieldAttr(Modifiers mod_flags)
		{
			FieldAttributes fieldAttributes = FieldAttributes.PrivateScope;
			if ((mod_flags & Modifiers.PUBLIC) != 0)
			{
				fieldAttributes |= FieldAttributes.Public;
			}
			if ((mod_flags & Modifiers.PRIVATE) != 0)
			{
				fieldAttributes |= FieldAttributes.Private;
			}
			if ((mod_flags & Modifiers.PROTECTED) != 0)
			{
				fieldAttributes = (((mod_flags & Modifiers.INTERNAL) == 0) ? (fieldAttributes | FieldAttributes.Family) : (fieldAttributes | FieldAttributes.FamORAssem));
			}
			else if ((mod_flags & Modifiers.INTERNAL) != 0)
			{
				fieldAttributes |= FieldAttributes.Assembly;
			}
			if ((mod_flags & Modifiers.STATIC) != 0)
			{
				fieldAttributes |= FieldAttributes.Static;
			}
			if ((mod_flags & Modifiers.READONLY) != 0)
			{
				fieldAttributes |= FieldAttributes.InitOnly;
			}
			return fieldAttributes;
		}

		public static MethodAttributes MethodAttr(Modifiers mod_flags)
		{
			MethodAttributes methodAttributes = MethodAttributes.HideBySig;
			switch (mod_flags & Modifiers.AccessibilityMask)
			{
			case Modifiers.PUBLIC:
				methodAttributes |= MethodAttributes.Public;
				break;
			case Modifiers.PRIVATE:
				methodAttributes |= MethodAttributes.Private;
				break;
			case Modifiers.PROTECTED | Modifiers.INTERNAL:
				methodAttributes |= MethodAttributes.FamORAssem;
				break;
			case Modifiers.PROTECTED:
				methodAttributes |= MethodAttributes.Family;
				break;
			case Modifiers.INTERNAL:
				methodAttributes |= MethodAttributes.Assembly;
				break;
			default:
				throw new NotImplementedException(mod_flags.ToString());
			}
			if ((mod_flags & Modifiers.STATIC) != 0)
			{
				methodAttributes |= MethodAttributes.Static;
			}
			if ((mod_flags & Modifiers.ABSTRACT) != 0)
			{
				methodAttributes |= MethodAttributes.Virtual | MethodAttributes.Abstract;
			}
			if ((mod_flags & Modifiers.SEALED) != 0)
			{
				methodAttributes |= MethodAttributes.Final;
			}
			if ((mod_flags & Modifiers.VIRTUAL) != 0)
			{
				methodAttributes |= MethodAttributes.Virtual;
			}
			if ((mod_flags & Modifiers.OVERRIDE) != 0)
			{
				methodAttributes |= MethodAttributes.Virtual;
			}
			else if ((methodAttributes & MethodAttributes.Virtual) != MethodAttributes.PrivateScope)
			{
				methodAttributes |= MethodAttributes.VtableLayoutMask;
			}
			return methodAttributes;
		}

		public static Modifiers Check(Modifiers allowed, Modifiers mod, Modifiers def_access, Location l, Report Report)
		{
			int num = (int)(~allowed & (mod & (Modifiers.AccessibilityMask | Modifiers.AllowedExplicitImplFlags | Modifiers.NEW | Modifiers.ABSTRACT | Modifiers.SEALED | Modifiers.STATIC | Modifiers.READONLY | Modifiers.VIRTUAL | Modifiers.OVERRIDE | Modifiers.VOLATILE | Modifiers.ASYNC)));
			if (num == 0)
			{
				if ((mod & Modifiers.AccessibilityMask) == 0)
				{
					mod |= def_access;
					if (def_access != 0)
					{
						mod |= Modifiers.DEFAULT_ACCESS_MODIFIER;
					}
					return mod;
				}
				return mod;
			}
			for (int num2 = 1; num2 < 32768; num2 <<= 1)
			{
				if ((num2 & num) != 0)
				{
					Error_InvalidModifier((Modifiers)num2, l, Report);
				}
			}
			return allowed & mod;
		}

		private static void Error_InvalidModifier(Modifiers mod, Location l, Report Report)
		{
			Report.Error(106, l, "The modifier `{0}' is not valid for this item", Name(mod));
		}
	}
}
