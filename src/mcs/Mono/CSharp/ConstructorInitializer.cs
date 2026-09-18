using System;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public abstract class ConstructorInitializer : ExpressionStatement
	{
		private Arguments argument_list;

		private MethodSpec base_ctor;

		public Arguments Arguments => argument_list;

		protected ConstructorInitializer(Arguments argument_list, Location loc)
		{
			this.argument_list = argument_list;
			base.loc = loc;
		}

		public override bool ContainsEmitWithAwait()
		{
			throw new NotSupportedException();
		}

		public override Expression CreateExpressionTree(ResolveContext ec)
		{
			throw new NotSupportedException("ET");
		}

		protected override Expression DoResolve(ResolveContext ec)
		{
			eclass = ExprClass.Value;
			Constructor constructor = (Constructor)ec.MemberContext;
			using (ec.Set(ResolveContext.Options.BaseInitializer))
			{
				if (argument_list != null)
				{
					bool dynamic;
					argument_list.Resolve(ec, out dynamic);
					if (dynamic)
					{
						ec.Report.Error(1975, loc, "The constructor call cannot be dynamically dispatched within constructor initializer");
						return null;
					}
				}
				type = ec.CurrentType;
				if (this is ConstructorBaseInitializer)
				{
					if (ec.CurrentType.BaseType == null)
					{
						return this;
					}
					type = ec.CurrentType.BaseType;
					if (ec.CurrentType.IsStruct)
					{
						ec.Report.Error(522, loc, "`{0}': Struct constructors cannot call base constructors", constructor.GetSignatureForError());
						return this;
					}
				}
				base_ctor = Expression.ConstructorLookup(ec, type, ref argument_list, loc);
			}
			if (base_ctor != null && base_ctor.MemberDefinition == constructor.Spec.MemberDefinition)
			{
				ec.Report.Error(516, loc, "Constructor `{0}' cannot call itself", constructor.GetSignatureForError());
			}
			return this;
		}

		public override void Emit(EmitContext ec)
		{
			if (base_ctor == null)
			{
				if (type != ec.BuiltinTypes.Object)
				{
					ec.Emit(OpCodes.Ldarg_0);
					ec.Emit(OpCodes.Initobj, type);
				}
			}
			else
			{
				CallEmitter callEmitter = default(CallEmitter);
				callEmitter.InstanceExpression = new CompilerGeneratedThis(type, loc);
				callEmitter.EmitPredefined(ec, base_ctor, argument_list);
			}
		}

		public override void EmitStatement(EmitContext ec)
		{
			Emit(ec);
		}

		public override void FlowAnalysis(FlowAnalysisContext fc)
		{
			if (argument_list != null)
			{
				argument_list.FlowAnalysis(fc);
			}
		}
	}
}
