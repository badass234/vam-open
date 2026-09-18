using System.Collections.Generic;
using System.Text;

namespace Mono.CSharp
{
	public class TypeArguments
	{
		private List<FullNamedExpression> args;

		private TypeSpec[] atypes;

		public TypeSpec[] Arguments
		{
			get
			{
				return atypes;
			}
			set
			{
				atypes = value;
			}
		}

		public int Count => args.Count;

		public virtual bool IsEmpty => false;

		public List<FullNamedExpression> TypeExpressions => args;

		public TypeArguments(params FullNamedExpression[] types)
		{
			args = new List<FullNamedExpression>(types);
		}

		public void Add(FullNamedExpression type)
		{
			args.Add(type);
		}

		public string GetSignatureForError()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < Count; i++)
			{
				FullNamedExpression fullNamedExpression = args[i];
				if (fullNamedExpression != null)
				{
					stringBuilder.Append(fullNamedExpression.GetSignatureForError());
				}
				if (i + 1 < Count)
				{
					stringBuilder.Append(',');
				}
			}
			return stringBuilder.ToString();
		}

		public virtual bool Resolve(IMemberContext ec, bool allowUnbound)
		{
			if (atypes != null)
			{
				return true;
			}
			int count = args.Count;
			bool flag = true;
			atypes = new TypeSpec[count];
			int errors = ec.Module.Compiler.Report.Errors;
			for (int i = 0; i < count; i++)
			{
				TypeSpec typeSpec = args[i].ResolveAsType(ec);
				if (typeSpec == null)
				{
					flag = false;
					continue;
				}
				atypes[i] = typeSpec;
				if (typeSpec.IsStatic)
				{
					ec.Module.Compiler.Report.Error(718, args[i].Location, "`{0}': static classes cannot be used as generic arguments", typeSpec.GetSignatureForError());
					flag = false;
				}
				if (typeSpec.IsPointer || typeSpec.IsSpecialRuntimeType)
				{
					ec.Module.Compiler.Report.Error(306, args[i].Location, "The type `{0}' may not be used as a type argument", typeSpec.GetSignatureForError());
					flag = false;
				}
			}
			if (!flag || errors != ec.Module.Compiler.Report.Errors)
			{
				atypes = null;
			}
			return flag;
		}

		public TypeArguments Clone()
		{
			TypeArguments typeArguments = new TypeArguments();
			foreach (FullNamedExpression arg in args)
			{
				typeArguments.args.Add(arg);
			}
			return typeArguments;
		}
	}
}
