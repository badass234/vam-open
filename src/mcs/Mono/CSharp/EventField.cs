using System;
using System.Collections.Generic;

namespace Mono.CSharp
{
	public class EventField : Event
	{
		private abstract class EventFieldAccessor : AEventAccessor
		{
			protected EventFieldAccessor(EventField method, string prefix)
				: base(method, prefix, null, method.Location)
			{
			}

			protected abstract MethodSpec GetOperation(Location loc);

			public override void Emit(TypeDefinition parent)
			{
				if ((method.ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) == 0 && !Compiler.Settings.WriteMetadataOnly)
				{
					block = new ToplevelBlock(Compiler, ParameterInfo, base.Location)
					{
						IsCompilerGenerated = true
					};
					FabricateBodyStatement();
				}
				base.Emit(parent);
			}

			private void FabricateBodyStatement()
			{
				Field backing_field = ((EventField)method).backing_field;
				FieldExpr fieldExpr = new FieldExpr(backing_field, base.Location);
				if (!base.IsStatic)
				{
					fieldExpr.InstanceExpression = new CompilerGeneratedThis(Parent.CurrentType, base.Location);
				}
				LocalVariable li = LocalVariable.CreateCompilerGenerated(backing_field.MemberType, block, base.Location);
				LocalVariable li2 = LocalVariable.CreateCompilerGenerated(backing_field.MemberType, block, base.Location);
				block.AddStatement(new StatementExpression(new SimpleAssign(new LocalVariableReference(li, base.Location), fieldExpr)));
				BooleanExpression bool_expr = new BooleanExpression(new Binary(Binary.Operator.Inequality, new Cast(new TypeExpression(Module.Compiler.BuiltinTypes.Object, base.Location), new LocalVariableReference(li, base.Location), base.Location), new Cast(new TypeExpression(Module.Compiler.BuiltinTypes.Object, base.Location), new LocalVariableReference(li2, base.Location), base.Location)));
				ExplicitBlock explicitBlock = new ExplicitBlock(block, base.Location, base.Location);
				block.AddStatement(new Do(explicitBlock, bool_expr, base.Location, base.Location));
				explicitBlock.AddStatement(new StatementExpression(new SimpleAssign(new LocalVariableReference(li2, base.Location), new LocalVariableReference(li, base.Location))));
				Arguments arguments = new Arguments(2);
				arguments.Add(new Argument(new LocalVariableReference(li2, base.Location)));
				arguments.Add(new Argument(block.GetParameterReference(0, base.Location)));
				MethodSpec operation = GetOperation(base.Location);
				Arguments arguments2 = new Arguments(3);
				arguments2.Add(new Argument(fieldExpr, Argument.AType.Ref));
				arguments2.Add(new Argument(new Cast(new TypeExpression(backing_field.MemberType, base.Location), new Invocation(MethodGroupExpr.CreatePredefined(operation, operation.DeclaringType, base.Location), arguments), base.Location)));
				arguments2.Add(new Argument(new LocalVariableReference(li, base.Location)));
				MethodSpec methodSpec = Module.PredefinedMembers.InterlockedCompareExchange_T.Get();
				if (methodSpec == null)
				{
					if (Module.PredefinedMembers.MonitorEnter_v4.Get() != null || Module.PredefinedMembers.MonitorEnter.Get() != null)
					{
						explicitBlock.AddStatement(new Lock(block.GetParameterReference(0, base.Location), new StatementExpression(new SimpleAssign(fieldExpr, arguments2[1].Expr, base.Location), base.Location), base.Location));
					}
					else
					{
						Module.PredefinedMembers.InterlockedCompareExchange_T.Resolve(base.Location);
					}
				}
				else
				{
					explicitBlock.AddStatement(new StatementExpression(new SimpleAssign(new LocalVariableReference(li, base.Location), new Invocation(MethodGroupExpr.CreatePredefined(methodSpec, methodSpec.DeclaringType, base.Location), arguments2))));
				}
			}
		}

		private sealed class AddDelegateMethod : EventFieldAccessor
		{
			public AddDelegateMethod(EventField method)
				: base(method, "add_")
			{
			}

			protected override MethodSpec GetOperation(Location loc)
			{
				return Module.PredefinedMembers.DelegateCombine.Resolve(loc);
			}
		}

		private sealed class RemoveDelegateMethod : EventFieldAccessor
		{
			public RemoveDelegateMethod(EventField method)
				: base(method, "remove_")
			{
			}

			protected override MethodSpec GetOperation(Location loc)
			{
				return Module.PredefinedMembers.DelegateRemove.Resolve(loc);
			}
		}

		private static readonly string[] attribute_targets = new string[3] { "event", "field", "method" };

		private static readonly string[] attribute_targets_interface = new string[2] { "event", "method" };

		private Expression initializer;

		private Field backing_field;

		private List<FieldDeclarator> declarators;

		public List<FieldDeclarator> Declarators => declarators;

		private bool HasBackingField
		{
			get
			{
				if (!IsInterface)
				{
					return (base.ModFlags & Modifiers.ABSTRACT) == 0;
				}
				return false;
			}
		}

		public Expression Initializer
		{
			get
			{
				return initializer;
			}
			set
			{
				initializer = value;
			}
		}

		public override string[] ValidAttributeTargets
		{
			get
			{
				if (!HasBackingField)
				{
					return attribute_targets_interface;
				}
				return attribute_targets;
			}
		}

		public EventField(TypeDefinition parent, FullNamedExpression type, Modifiers mod_flags, MemberName name, Attributes attrs)
			: base(parent, type, mod_flags, name, attrs)
		{
			base.Add = new AddDelegateMethod(this);
			base.Remove = new RemoveDelegateMethod(this);
		}

		public override void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}

		public void AddDeclarator(FieldDeclarator declarator)
		{
			if (declarators == null)
			{
				declarators = new List<FieldDeclarator>(2);
			}
			declarators.Add(declarator);
			Parent.AddNameToContainer(this, declarator.Name.Value);
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Target == AttributeTargets.Field)
			{
				backing_field.ApplyAttributeBuilder(a, ctor, cdata, pa);
			}
			else if (a.Target == AttributeTargets.Method)
			{
				int errors = base.Report.Errors;
				base.Add.ApplyAttributeBuilder(a, ctor, cdata, pa);
				if (errors == base.Report.Errors)
				{
					base.Remove.ApplyAttributeBuilder(a, ctor, cdata, pa);
				}
			}
			else
			{
				base.ApplyAttributeBuilder(a, ctor, cdata, pa);
			}
		}

		public override bool Define()
		{
			Modifiers modifiers = base.ModFlags;
			if (!base.Define())
			{
				return false;
			}
			if (declarators != null)
			{
				if ((modifiers & Modifiers.DEFAULT_ACCESS_MODIFIER) != 0)
				{
					modifiers &= ~(Modifiers.AccessibilityMask | Modifiers.DEFAULT_ACCESS_MODIFIER);
				}
				TypeExpression type = new TypeExpression(base.MemberType, base.TypeExpression.Location);
				foreach (FieldDeclarator declarator in declarators)
				{
					EventField eventField = new EventField(Parent, type, modifiers, new MemberName(declarator.Name.Value, declarator.Name.Location), base.OptAttributes);
					if (declarator.Initializer != null)
					{
						eventField.initializer = declarator.Initializer;
					}
					eventField.Define();
					Parent.PartialContainer.Members.Add(eventField);
				}
			}
			if (!HasBackingField)
			{
				SetIsUsed();
				return true;
			}
			backing_field = new Field(Parent, new TypeExpression(base.MemberType, base.Location), Modifiers.PRIVATE | Modifiers.COMPILER_GENERATED | Modifiers.BACKING_FIELD | (base.ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE)), base.MemberName, null);
			Parent.PartialContainer.Members.Add(backing_field);
			backing_field.Initializer = Initializer;
			backing_field.ModFlags &= ~Modifiers.COMPILER_GENERATED;
			backing_field.Define();
			spec.BackingField = backing_field.Spec;
			return true;
		}
	}
}
