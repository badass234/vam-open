using System.Collections.Generic;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class TryCatch : ExceptionStatement
	{
		public Block Block;

		private List<Catch> clauses;

		private readonly bool inside_try_finally;

		private List<Catch> catch_sm;

		public List<Catch> Clauses => clauses;

		public bool IsTryCatchFinally => inside_try_finally;

		public TryCatch(Block block, List<Catch> catch_clauses, Location l, bool inside_try_finally)
			: base(l)
		{
			Block = block;
			clauses = catch_clauses;
			this.inside_try_finally = inside_try_finally;
		}

		public override bool Resolve(BlockContext bc)
		{
			bool flag;
			using (bc.Set(ResolveContext.Options.TryScope))
			{
				parent = bc.CurrentTryBlock;
				if (IsTryCatchFinally)
				{
					flag = Block.Resolve(bc);
				}
				else
				{
					using (bc.Set(ResolveContext.Options.TryWithCatchScope))
					{
						bc.CurrentTryBlock = this;
						flag = Block.Resolve(bc);
						bc.CurrentTryBlock = parent;
					}
				}
			}
			for (int i = 0; i < clauses.Count; i++)
			{
				Catch obj = clauses[i];
				flag &= obj.Resolve(bc);
				if (obj.Block.HasAwait)
				{
					if (catch_sm == null)
					{
						catch_sm = new List<Catch>();
					}
					catch_sm.Add(obj);
				}
				if (obj.Filter != null)
				{
					continue;
				}
				TypeSpec catchType = obj.CatchType;
				if (catchType == null)
				{
					continue;
				}
				for (int j = 0; j < clauses.Count; j++)
				{
					if (j == i || clauses[j].Filter != null)
					{
						continue;
					}
					if (clauses[j].IsGeneral)
					{
						if (catchType.BuiltinType == BuiltinTypeSpec.Type.Exception && bc.Module.DeclaringAssembly.WrapNonExceptionThrows && bc.Module.PredefinedAttributes.RuntimeCompatibility.IsDefined)
						{
							bc.Report.Warning(1058, 1, obj.loc, "A previous catch clause already catches all exceptions. All non-exceptions thrown will be wrapped in a `System.Runtime.CompilerServices.RuntimeWrappedException'");
						}
					}
					else if (j < i)
					{
						TypeSpec catchType2 = clauses[j].CatchType;
						if (catchType2 != null && (catchType == catchType2 || TypeSpec.IsBaseClass(catchType, catchType2, true)))
						{
							bc.Report.Error(160, obj.loc, "A previous catch clause already catches all exceptions of this or a super type `{0}'", catchType2.GetSignatureForError());
							flag = false;
						}
					}
				}
			}
			return base.Resolve(bc) & flag;
		}

		protected sealed override void DoEmit(EmitContext ec)
		{
			if (!inside_try_finally)
			{
				EmitTryBodyPrepare(ec);
			}
			Block.Emit(ec);
			LocalBuilder localBuilder = null;
			foreach (Catch clause in clauses)
			{
				clause.Emit(ec);
				if (catch_sm != null)
				{
					if (localBuilder == null)
					{
						localBuilder = ec.DeclareLocal(ec.Module.Compiler.BuiltinTypes.Int, false);
					}
					int num = catch_sm.IndexOf(clause);
					if (num >= 0)
					{
						ec.EmitInt(num + 1);
						ec.Emit(OpCodes.Stloc, localBuilder);
					}
				}
			}
			if (!inside_try_finally)
			{
				ec.EndExceptionBlock();
			}
			if (localBuilder == null)
			{
				return;
			}
			ec.Emit(OpCodes.Ldloc, localBuilder);
			Label[] array = new Label[catch_sm.Count + 1];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = ec.DefineLabel();
			}
			Label label = ec.DefineLabel();
			ec.Emit(OpCodes.Switch, array);
			ec.MarkLabel(array[0]);
			ec.Emit(OpCodes.Br, label);
			LocalVariable asyncThrowVariable = ec.AsyncThrowVariable;
			Catch obj = null;
			for (int j = 0; j < catch_sm.Count; j++)
			{
				if (obj != null && obj.Block.HasReachableClosingBrace)
				{
					ec.Emit(OpCodes.Br, label);
				}
				ec.MarkLabel(array[j + 1]);
				obj = catch_sm[j];
				ec.AsyncThrowVariable = obj.Variable;
				obj.Block.Emit(ec);
			}
			ec.AsyncThrowVariable = asyncThrowVariable;
			ec.MarkLabel(label);
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			DefiniteAssignmentBitSet definiteAssignmentBitSet = fc.BranchDefiniteAssignment();
			bool flag = Block.FlowAnalysis(fc);
			DefiniteAssignmentBitSet definiteAssignmentBitSet2 = (flag ? null : fc.DefiniteAssignment);
			foreach (Catch clause in clauses)
			{
				fc.BranchDefiniteAssignment(definiteAssignmentBitSet);
				if (!clause.FlowAnalysis(fc))
				{
					if (definiteAssignmentBitSet2 == null)
					{
						definiteAssignmentBitSet2 = fc.DefiniteAssignment;
					}
					else
					{
						definiteAssignmentBitSet2 &= fc.DefiniteAssignment;
					}
					flag = false;
				}
			}
			fc.DefiniteAssignment = definiteAssignmentBitSet2 ?? definiteAssignmentBitSet;
			parent = null;
			return flag;
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			if (rc.IsUnreachable)
			{
				return rc;
			}
			base.MarkReachable(rc);
			Reachability result = Block.MarkReachable(rc);
			foreach (Catch clause in clauses)
			{
				result &= clause.MarkReachable(rc);
			}
			return result;
		}

		protected override void CloneTo(CloneContext clonectx, Statement t)
		{
			TryCatch tryCatch = (TryCatch)t;
			tryCatch.Block = clonectx.LookupBlock(Block);
			if (clauses == null)
			{
				return;
			}
			tryCatch.clauses = new List<Catch>();
			foreach (Catch clause in clauses)
			{
				tryCatch.clauses.Add((Catch)clause.Clone(clonectx));
			}
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
