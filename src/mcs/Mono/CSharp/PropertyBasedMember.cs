using System;

namespace Mono.CSharp
{
	public abstract class PropertyBasedMember : InterfaceMemberBase
	{
		protected PropertyBasedMember(TypeDefinition parent, FullNamedExpression type, Modifiers mod, Modifiers allowed_mod, MemberName name, Attributes attrs)
			: base(parent, type, mod, allowed_mod, name, attrs)
		{
		}

		protected void CheckReservedNameConflict(string prefix, MethodSpec accessor)
		{
			string text;
			AParametersCollection aParametersCollection;
			if (accessor != null)
			{
				text = accessor.Name;
				aParametersCollection = accessor.Parameters;
			}
			else
			{
				text = prefix + base.ShortName;
				if (IsExplicitImpl)
				{
					text = string.Concat(base.MemberName.Left, ".", text);
				}
				if (!(this is Indexer))
				{
					aParametersCollection = ((prefix[0] != 's') ? ParametersCompiled.EmptyReadOnlyParameters : ParametersCompiled.CreateFullyResolved(member_type));
				}
				else
				{
					aParametersCollection = ((Indexer)this).ParameterInfo;
					if (prefix[0] == 's')
					{
						IParameterData[] array = new IParameterData[aParametersCollection.Count + 1];
						Array.Copy(aParametersCollection.FixedParameters, array, array.Length - 1);
						array[array.Length - 1] = new ParameterData("value", Parameter.Modifier.NONE);
						TypeSpec[] array2 = new TypeSpec[array.Length];
						Array.Copy(aParametersCollection.Types, array2, array.Length - 1);
						array2[array.Length - 1] = member_type;
						aParametersCollection = new ParametersImported(array, array2, false);
					}
				}
			}
			MemberSpec memberSpec = MemberCache.FindMember(Parent.Definition, new MemberFilter(text, 0, MemberKind.Method, aParametersCollection, null), BindingRestriction.DeclaredOnly | BindingRestriction.NoAccessors);
			if (memberSpec != null)
			{
				base.Report.SymbolRelatedToPreviousError(memberSpec);
				base.Report.Error(82, base.Location, "A member `{0}' is already reserved", memberSpec.GetSignatureForError());
			}
		}

		public abstract void PrepareEmit();

		protected override bool VerifyClsCompliance()
		{
			if (!base.VerifyClsCompliance())
			{
				return false;
			}
			if (!base.MemberType.IsCLSCompliant())
			{
				base.Report.Warning(3003, 1, base.Location, "Type of `{0}' is not CLS-compliant", GetSignatureForError());
			}
			return true;
		}
	}
}
