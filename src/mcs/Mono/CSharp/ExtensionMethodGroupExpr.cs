using System.Collections.Generic;
using System.Linq;

namespace Mono.CSharp
{
	internal class ExtensionMethodGroupExpr : MethodGroupExpr, OverloadResolver.IErrorHandler
	{
		private ExtensionMethodCandidates candidates;

		public Expression ExtensionExpression;

		public override bool IsStatic => true;

		public ExtensionMethodGroupExpr(ExtensionMethodCandidates candidates, Expression extensionExpr, Location loc)
			: base(candidates.Methods.Cast<MemberSpec>().ToList(), extensionExpr.Type, loc)
		{
			this.candidates = candidates;
			ExtensionExpression = extensionExpr;
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			if (base.ConditionalAccess)
			{
				fc.BranchConditionalAccessDefiniteAssignment();
			}
		}

		public override IList<MemberSpec> GetBaseMembers(TypeSpec baseType)
		{
			if (candidates == null)
			{
				return null;
			}
			int arity = ((type_arguments != null) ? type_arguments.Count : 0);
			candidates = candidates.Container.LookupExtensionMethod(candidates.Context, Name, arity, candidates.LookupIndex);
			if (candidates == null)
			{
				return null;
			}
			return candidates.Methods.Cast<MemberSpec>().ToList();
		}

		public static bool IsExtensionTypeCompatible(TypeSpec argType, TypeSpec extensionType)
		{
			if (argType != extensionType && !TypeSpecComparer.IsEqual(argType, extensionType) && !Convert.ImplicitReferenceConversionExists(argType, extensionType, false))
			{
				return Convert.ImplicitBoxingConversion(null, argType, extensionType) != null;
			}
			return true;
		}

		public bool ResolveNameOf(ResolveContext rc, MemberAccess ma)
		{
			rc.Report.Error(8093, ma.Location, "An argument to nameof operator cannot be extension method group");
			return false;
		}

		public override MethodGroupExpr LookupExtensionMethod(ResolveContext rc)
		{
			return null;
		}

		public override MethodGroupExpr OverloadResolve(ResolveContext ec, ref Arguments arguments, OverloadResolver.IErrorHandler ehandler, OverloadResolver.Restrictions restr)
		{
			if (arguments == null)
			{
				arguments = new Arguments(1);
			}
			ExtensionExpression = ExtensionExpression.Resolve(ec);
			if (ExtensionExpression == null)
			{
				return null;
			}
			ExtensionMethodCandidates extensionMethodCandidates = candidates;
			Argument.AType aType = (base.ConditionalAccess ? Argument.AType.ExtensionTypeConditionalAccess : Argument.AType.ExtensionType);
			arguments.Insert(0, new Argument(ExtensionExpression, aType));
			MethodGroupExpr methodGroupExpr = base.OverloadResolve(ec, ref arguments, ehandler ?? this, restr);
			candidates = extensionMethodCandidates;
			if (methodGroupExpr == null)
			{
				arguments.RemoveAt(0);
				return null;
			}
			MemberExpr memberExpr = ExtensionExpression as MemberExpr;
			if (memberExpr != null)
			{
				memberExpr.ResolveInstanceExpression(ec, null);
				(memberExpr as FieldExpr)?.Spec.MemberDefinition.SetIsUsed();
			}
			InstanceExpression = null;
			return this;
		}

		bool OverloadResolver.IErrorHandler.AmbiguousCandidates(ResolveContext rc, MemberSpec best, MemberSpec ambiguous)
		{
			return false;
		}

		bool OverloadResolver.IErrorHandler.ArgumentMismatch(ResolveContext rc, MemberSpec best, Argument arg, int index)
		{
			rc.Report.SymbolRelatedToPreviousError(best);
			if (index == 0)
			{
				rc.Report.Error(1929, loc, "Type `{0}' does not contain a member `{1}' and the best extension method overload `{2}' requires an instance of type `{3}'", queried_type.GetSignatureForError(), Name, best.GetSignatureForError(), ((MethodSpec)best).Parameters.ExtensionMethodType.GetSignatureForError());
			}
			else
			{
				rc.Report.Error(1928, loc, "Type `{0}' does not contain a member `{1}' and the best extension method overload `{2}' has some invalid arguments", queried_type.GetSignatureForError(), Name, best.GetSignatureForError());
			}
			return true;
		}

		bool OverloadResolver.IErrorHandler.NoArgumentMatch(ResolveContext rc, MemberSpec best)
		{
			return false;
		}

		bool OverloadResolver.IErrorHandler.TypeInferenceFailed(ResolveContext rc, MemberSpec best)
		{
			return false;
		}
	}
}
