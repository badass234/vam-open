using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Mono.CSharp.Nullable;

namespace Mono.CSharp
{
	public class Foreach : LoopStatement
	{
		private abstract class IteratorStatement : Statement
		{
			protected readonly Foreach for_each;

			protected IteratorStatement(Foreach @foreach)
			{
				for_each = @foreach;
				loc = @foreach.expr.Location;
			}

			protected override void CloneTo(CloneContext clonectx, Statement target)
			{
				throw new NotImplementedException();
			}

			public override void Emit(EmitContext ec)
			{
				if (ec.EmitAccurateDebugInfo)
				{
					ec.Emit(OpCodes.Nop);
				}
				base.Emit(ec);
			}

			protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
			{
				throw new NotImplementedException();
			}
		}

		private sealed class ArrayForeach : IteratorStatement
		{
			private TemporaryVariableReference[] lengths;

			private Expression[] length_exprs;

			private StatementExpression[] counter;

			private TemporaryVariableReference[] variables;

			private TemporaryVariableReference copy;

			public ArrayForeach(Foreach @foreach, int rank)
				: base(@foreach)
			{
				counter = new StatementExpression[rank];
				variables = new TemporaryVariableReference[rank];
				length_exprs = new Expression[rank];
				if (rank > 1)
				{
					lengths = new TemporaryVariableReference[rank];
				}
			}

			public override bool Resolve(BlockContext ec)
			{
				Block block = for_each.variable.Block;
				copy = TemporaryVariableReference.Create(for_each.expr.Type, block, loc);
				copy.Resolve(ec);
				int num = length_exprs.Length;
				Arguments arguments = new Arguments(num);
				for (int i = 0; i < num; i++)
				{
					TemporaryVariableReference temporaryVariableReference = TemporaryVariableReference.Create(ec.BuiltinTypes.Int, block, loc);
					variables[i] = temporaryVariableReference;
					counter[i] = new StatementExpression(new UnaryMutator(UnaryMutator.Mode.IsPost, temporaryVariableReference, Location.Null));
					counter[i].Resolve(ec);
					if (num == 1)
					{
						length_exprs[i] = new MemberAccess(copy, "Length").Resolve(ec);
					}
					else
					{
						lengths[i] = TemporaryVariableReference.Create(ec.BuiltinTypes.Int, block, loc);
						lengths[i].Resolve(ec);
						Arguments arguments2 = new Arguments(1);
						arguments2.Add(new Argument(new IntConstant(ec.BuiltinTypes, i, loc)));
						length_exprs[i] = new Invocation(new MemberAccess(copy, "GetLength"), arguments2).Resolve(ec);
					}
					arguments.Add(new Argument(temporaryVariableReference));
				}
				Expression expression = new ElementAccess(copy, arguments, loc).Resolve(ec);
				if (expression == null)
				{
					return false;
				}
				TypeSpec typeSpec;
				if (for_each.type is VarExpr)
				{
					typeSpec = expression.Type;
				}
				else
				{
					typeSpec = for_each.type.ResolveAsType(ec);
					if (typeSpec == null)
					{
						return false;
					}
					expression = Convert.ExplicitConversion(ec, expression, typeSpec, loc);
					if (expression == null)
					{
						return false;
					}
				}
				for_each.variable.Type = typeSpec;
				Expression expression2 = new LocalVariableReference(for_each.variable, loc).Resolve(ec);
				if (expression2 == null)
				{
					return false;
				}
				for_each.body.AddScopeStatement(new StatementExpression(new CompilerAssign(expression2, expression, Location.Null), for_each.type.Location));
				return for_each.body.Resolve(ec);
			}

			protected override void DoEmit(EmitContext ec)
			{
				copy.EmitAssign(ec, for_each.expr);
				int num = length_exprs.Length;
				Label[] array = new Label[num];
				Label[] array2 = new Label[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = ec.DefineLabel();
					array2[i] = ec.DefineLabel();
					if (lengths != null)
					{
						lengths[i].EmitAssign(ec, length_exprs[i]);
					}
				}
				IntConstant source = new IntConstant(ec.BuiltinTypes, 0, loc);
				for (int j = 0; j < num; j++)
				{
					variables[j].EmitAssign(ec, source);
					ec.Emit(OpCodes.Br, array[j]);
					ec.MarkLabel(array2[j]);
				}
				for_each.body.Emit(ec);
				ec.MarkLabel(ec.LoopBegin);
				ec.Mark(for_each.expr.Location);
				for (int num2 = num - 1; num2 >= 0; num2--)
				{
					counter[num2].Emit(ec);
					ec.MarkLabel(array[num2]);
					variables[num2].Emit(ec);
					if (lengths != null)
					{
						lengths[num2].Emit(ec);
					}
					else
					{
						length_exprs[num2].Emit(ec);
					}
					ec.Emit(OpCodes.Blt, array2[num2]);
				}
				ec.MarkLabel(ec.LoopEnd);
			}
		}

		private sealed class CollectionForeach : IteratorStatement, OverloadResolver.IErrorHandler
		{
			private class RuntimeDispose : Using.VariableDeclaration
			{
				public RuntimeDispose(LocalVariable lv, Location loc)
					: base(lv, loc)
				{
					reachable = true;
				}

				protected override void CheckIDiposableConversion(BlockContext bc, LocalVariable li, Expression initializer)
				{
				}

				protected override Statement CreateDisposeCall(BlockContext bc, LocalVariable lv)
				{
					BuiltinTypeSpec iDisposable = bc.BuiltinTypes.IDisposable;
					LocalVariable localVariable = LocalVariable.CreateCompilerGenerated(iDisposable, bc.CurrentBlock, loc);
					Binary bool_expr = new Binary(Binary.Operator.Inequality, new CompilerAssign(localVariable.CreateReferenceExpression(bc, loc), new As(lv.CreateReferenceExpression(bc, loc), new TypeExpression(localVariable.Type, loc), loc), loc), new NullLiteral(loc));
					MethodGroupExpr methodGroupExpr = MethodGroupExpr.CreatePredefined(bc.Module.PredefinedMembers.IDisposableDispose.Resolve(loc), iDisposable, loc);
					methodGroupExpr.InstanceExpression = localVariable.CreateReferenceExpression(bc, loc);
					Statement true_statement = new StatementExpression(new Invocation(methodGroupExpr, null));
					return new If(bool_expr, true_statement, loc);
				}
			}

			private LocalVariable variable;

			private Expression expr;

			private Statement statement;

			private ExpressionStatement init;

			private TemporaryVariableReference enumerator_variable;

			private bool ambiguous_getenumerator_name;

			public CollectionForeach(Foreach @foreach, LocalVariable var, Expression expr)
				: base(@foreach)
			{
				variable = var;
				this.expr = expr;
			}

			private void Error_WrongEnumerator(ResolveContext rc, MethodSpec enumerator)
			{
				rc.Report.SymbolRelatedToPreviousError(enumerator);
				rc.Report.Error(202, loc, "foreach statement requires that the return type `{0}' of `{1}' must have a suitable public MoveNext method and public Current property", enumerator.ReturnType.GetSignatureForError(), enumerator.GetSignatureForError());
			}

			private MethodGroupExpr ResolveGetEnumerator(ResolveContext rc)
			{
				MethodGroupExpr methodGroupExpr = Expression.MemberLookup(rc, false, expr.Type, "GetEnumerator", 0, Expression.MemberLookupRestrictions.ExactArity, loc) as MethodGroupExpr;
				if (methodGroupExpr != null)
				{
					methodGroupExpr.InstanceExpression = expr;
					Arguments args = new Arguments(0);
					methodGroupExpr = methodGroupExpr.OverloadResolve(rc, ref args, this, OverloadResolver.Restrictions.ProbingOnly | OverloadResolver.Restrictions.GetEnumeratorLookup);
					if (ambiguous_getenumerator_name)
					{
						methodGroupExpr = null;
					}
					if (methodGroupExpr != null && !methodGroupExpr.BestCandidate.IsStatic && methodGroupExpr.BestCandidate.IsPublic)
					{
						return methodGroupExpr;
					}
				}
				TypeSpec type = expr.Type;
				PredefinedMember<MethodSpec> predefinedMember = null;
				PredefinedType predefinedType = rc.Module.PredefinedTypes.IEnumerableGeneric;
				if (!predefinedType.Define())
				{
					predefinedType = null;
				}
				IList<TypeSpec> interfaces = type.Interfaces;
				if (interfaces != null)
				{
					foreach (TypeSpec item in interfaces)
					{
						if (predefinedType != null && item.MemberDefinition == predefinedType.TypeSpec.MemberDefinition)
						{
							if (predefinedMember != null && predefinedMember != rc.Module.PredefinedMembers.IEnumerableGetEnumerator)
							{
								rc.Report.SymbolRelatedToPreviousError(expr.Type);
								rc.Report.Error(1640, loc, "foreach statement cannot operate on variables of type `{0}' because it contains multiple implementation of `{1}'. Try casting to a specific implementation", expr.Type.GetSignatureForError(), predefinedType.TypeSpec.GetSignatureForError());
								return null;
							}
							predefinedMember = new PredefinedMember<MethodSpec>(rc.Module, item, MemberFilter.Method("GetEnumerator", 0, ParametersCompiled.EmptyReadOnlyParameters, null));
						}
						else if (item.BuiltinType == BuiltinTypeSpec.Type.IEnumerable && predefinedMember == null)
						{
							predefinedMember = rc.Module.PredefinedMembers.IEnumerableGetEnumerator;
						}
					}
				}
				if (predefinedMember == null)
				{
					if (expr.Type != InternalType.ErrorType)
					{
						rc.Report.Error(1579, loc, "foreach statement cannot operate on variables of type `{0}' because it does not contain a definition for `{1}' or is inaccessible", expr.Type.GetSignatureForError(), "GetEnumerator");
					}
					return null;
				}
				MethodSpec methodSpec = predefinedMember.Resolve(loc);
				if (methodSpec == null)
				{
					return null;
				}
				methodGroupExpr = MethodGroupExpr.CreatePredefined(methodSpec, expr.Type, loc);
				methodGroupExpr.InstanceExpression = expr;
				return methodGroupExpr;
			}

			private MethodGroupExpr ResolveMoveNext(ResolveContext rc, MethodSpec enumerator)
			{
				MethodSpec methodSpec = MemberCache.FindMember(enumerator.ReturnType, MemberFilter.Method("MoveNext", 0, ParametersCompiled.EmptyReadOnlyParameters, rc.BuiltinTypes.Bool), BindingRestriction.InstanceOnly) as MethodSpec;
				if (methodSpec == null || !methodSpec.IsPublic)
				{
					Error_WrongEnumerator(rc, enumerator);
					return null;
				}
				return MethodGroupExpr.CreatePredefined(methodSpec, enumerator.ReturnType, expr.Location);
			}

			private PropertySpec ResolveCurrent(ResolveContext rc, MethodSpec enumerator)
			{
				PropertySpec propertySpec = MemberCache.FindMember(enumerator.ReturnType, MemberFilter.Property("Current", null), BindingRestriction.InstanceOnly) as PropertySpec;
				if (propertySpec == null || !propertySpec.IsPublic)
				{
					Error_WrongEnumerator(rc, enumerator);
					return null;
				}
				return propertySpec;
			}

			public override bool Resolve(BlockContext ec)
			{
				bool flag = expr.Type.BuiltinType == BuiltinTypeSpec.Type.Dynamic;
				if (flag)
				{
					expr = Convert.ImplicitConversionRequired(ec, expr, ec.BuiltinTypes.IEnumerable, loc);
				}
				else if (expr.Type.IsNullableType)
				{
					expr = new UnwrapCall(expr).Resolve(ec);
				}
				MethodGroupExpr methodGroupExpr = ResolveGetEnumerator(ec);
				if (methodGroupExpr == null)
				{
					return false;
				}
				MethodSpec bestCandidate = methodGroupExpr.BestCandidate;
				enumerator_variable = TemporaryVariableReference.Create(bestCandidate.ReturnType, variable.Block, loc);
				enumerator_variable.Resolve(ec);
				MethodGroupExpr methodGroupExpr2 = ResolveMoveNext(ec, bestCandidate);
				if (methodGroupExpr2 == null)
				{
					return false;
				}
				methodGroupExpr2.InstanceExpression = enumerator_variable;
				PropertySpec propertySpec = ResolveCurrent(ec, bestCandidate);
				if (propertySpec == null)
				{
					return false;
				}
				Expression expression = new PropertyExpr(propertySpec, loc)
				{
					InstanceExpression = enumerator_variable
				}.Resolve(ec);
				if (expression == null)
				{
					return false;
				}
				if (for_each.type is VarExpr)
				{
					if (flag)
					{
						variable.Type = ec.BuiltinTypes.Dynamic;
					}
					else
					{
						variable.Type = expression.Type;
					}
				}
				else
				{
					if (flag)
					{
						expression = EmptyCast.Create(expression, ec.BuiltinTypes.Dynamic);
					}
					variable.Type = for_each.type.ResolveAsType(ec);
					if (variable.Type == null)
					{
						return false;
					}
					expression = Convert.ExplicitConversion(ec, expression, variable.Type, loc);
					if (expression == null)
					{
						return false;
					}
				}
				Expression expression2 = new LocalVariableReference(variable, loc).Resolve(ec);
				if (expression2 == null)
				{
					return false;
				}
				for_each.body.AddScopeStatement(new StatementExpression(new CompilerAssign(expression2, expression, Location.Null), for_each.type.Location));
				Invocation.Predefined predefined = new Invocation.Predefined(methodGroupExpr, null);
				statement = new While(new BooleanExpression(new Invocation(methodGroupExpr2, null)), for_each.body, Location.Null);
				TypeSpec type = enumerator_variable.Type;
				if (!type.ImplementsInterface(ec.BuiltinTypes.IDisposable, false))
				{
					if (!type.IsSealed && !TypeSpec.IsValueType(type))
					{
						RuntimeDispose runtimeDispose = new RuntimeDispose(enumerator_variable.LocalInfo, Location.Null);
						runtimeDispose.Initializer = predefined;
						statement = new Using(runtimeDispose, statement, Location.Null);
					}
					else
					{
						init = new SimpleAssign(enumerator_variable, predefined, Location.Null);
						init.Resolve(ec);
					}
				}
				else
				{
					Using.VariableDeclaration variableDeclaration = new Using.VariableDeclaration(enumerator_variable.LocalInfo, Location.Null);
					variableDeclaration.Initializer = predefined;
					statement = new Using(variableDeclaration, statement, Location.Null);
				}
				return statement.Resolve(ec);
			}

			protected override void DoEmit(EmitContext ec)
			{
				enumerator_variable.LocalInfo.CreateBuilder(ec);
				if (init != null)
				{
					init.EmitStatement(ec);
				}
				statement.Emit(ec);
			}

			bool OverloadResolver.IErrorHandler.AmbiguousCandidates(ResolveContext ec, MemberSpec best, MemberSpec ambiguous)
			{
				ec.Report.SymbolRelatedToPreviousError(best);
				ec.Report.Warning(278, 2, expr.Location, "`{0}' contains ambiguous implementation of `{1}' pattern. Method `{2}' is ambiguous with method `{3}'", expr.Type.GetSignatureForError(), "enumerable", best.GetSignatureForError(), ambiguous.GetSignatureForError());
				ambiguous_getenumerator_name = true;
				return true;
			}

			bool OverloadResolver.IErrorHandler.ArgumentMismatch(ResolveContext rc, MemberSpec best, Argument arg, int index)
			{
				return false;
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

		private Expression type;

		private LocalVariable variable;

		private Expression expr;

		private Block body;

		public Expression Expr => expr;

		public Expression TypeExpression => type;

		public LocalVariable Variable => variable;

		public Foreach(Expression type, LocalVariable var, Expression expr, Statement stmt, Block body, Location l)
			: base(stmt)
		{
			this.type = type;
			variable = var;
			this.expr = expr;
			this.body = body;
			loc = l;
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			base.MarkReachable(rc);
			body.MarkReachable(rc);
			return rc;
		}

		public override bool Resolve(BlockContext ec)
		{
			expr = expr.Resolve(ec);
			if (expr == null)
			{
				return false;
			}
			if (expr.IsNull)
			{
				ec.Report.Error(186, loc, "Use of null is not valid in this context");
				return false;
			}
			body.AddStatement(base.Statement);
			if (expr.Type.BuiltinType == BuiltinTypeSpec.Type.String)
			{
				base.Statement = new ArrayForeach(this, 1);
			}
			else if (expr.Type is ArrayContainer)
			{
				base.Statement = new ArrayForeach(this, ((ArrayContainer)expr.Type).Rank);
			}
			else
			{
				if (expr.eclass == ExprClass.MethodGroup || expr is AnonymousMethodExpression)
				{
					ec.Report.Error(446, expr.Location, "Foreach statement cannot operate on a `{0}'", expr.ExprClassName);
					return false;
				}
				base.Statement = new CollectionForeach(this, variable, expr);
			}
			return base.Resolve(ec);
		}

		protected override void DoEmit(EmitContext ec)
		{
			Label loopBegin = ec.LoopBegin;
			Label loopEnd = ec.LoopEnd;
			ec.LoopBegin = ec.DefineLabel();
			ec.LoopEnd = ec.DefineLabel();
			if (!(base.Statement is Block))
			{
				ec.BeginCompilerScope();
			}
			variable.CreateBuilder(ec);
			base.Statement.Emit(ec);
			if (!(base.Statement is Block))
			{
				ec.EndScope();
			}
			ec.LoopBegin = loopBegin;
			ec.LoopEnd = loopEnd;
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			expr.FlowAnalysis(fc);
			DefiniteAssignmentBitSet definiteAssignment = fc.BranchDefiniteAssignment();
			body.FlowAnalysis(fc);
			fc.DefiniteAssignment = definiteAssignment;
			return false;
		}

		protected override void CloneTo(CloneContext clonectx, Statement t)
		{
			Foreach obj = (Foreach)t;
			obj.type = type.Clone(clonectx);
			obj.expr = expr.Clone(clonectx);
			obj.body = (Block)body.Clone(clonectx);
			obj.Statement = base.Statement.Clone(clonectx);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
