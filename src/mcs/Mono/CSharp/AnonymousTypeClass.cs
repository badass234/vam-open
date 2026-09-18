using System.Collections.Generic;

namespace Mono.CSharp
{
	public class AnonymousTypeClass : CompilerGeneratedContainer
	{
		public const string ClassNamePrefix = "<>__AnonType";

		public const string SignatureForError = "anonymous type";

		private readonly IList<AnonymousTypeParameter> parameters;

		public IList<AnonymousTypeParameter> Parameters => parameters;

		private AnonymousTypeClass(ModuleContainer parent, MemberName name, IList<AnonymousTypeParameter> parameters, Location loc)
			: base(parent, name, (parent.Evaluator != null) ? Modifiers.PUBLIC : Modifiers.INTERNAL)
		{
			this.parameters = parameters;
		}

		public static AnonymousTypeClass Create(TypeContainer parent, IList<AnonymousTypeParameter> parameters, Location loc)
		{
			string name = "<>__AnonType" + parent.Module.CounterAnonymousTypes++;
			TypeParameters typeParameters = null;
			ParametersCompiled args;
			SimpleName[] array;
			if (parameters.Count == 0)
			{
				args = ParametersCompiled.EmptyReadOnlyParameters;
				array = null;
			}
			else
			{
				array = new SimpleName[parameters.Count];
				typeParameters = new TypeParameters();
				Parameter[] array2 = new Parameter[parameters.Count];
				for (int i = 0; i < parameters.Count; i++)
				{
					AnonymousTypeParameter anonymousTypeParameter = parameters[i];
					for (int j = 0; j < i; j++)
					{
						if (parameters[j].Name == anonymousTypeParameter.Name)
						{
							parent.Compiler.Report.Error(833, parameters[j].Location, "`{0}': An anonymous type cannot have multiple properties with the same name", anonymousTypeParameter.Name);
							anonymousTypeParameter = (parameters[i] = new AnonymousTypeParameter(null, "$" + i, anonymousTypeParameter.Location));
							break;
						}
					}
					array[i] = new SimpleName("<" + anonymousTypeParameter.Name + ">__T", anonymousTypeParameter.Location);
					typeParameters.Add(new TypeParameter(i, new MemberName(array[i].Name, anonymousTypeParameter.Location), null, null, Variance.None));
					array2[i] = new Parameter(array[i], anonymousTypeParameter.Name, Parameter.Modifier.NONE, null, anonymousTypeParameter.Location);
				}
				args = new ParametersCompiled(array2);
			}
			AnonymousTypeClass anonymousTypeClass = new AnonymousTypeClass(parent.Module, new MemberName(name, typeParameters, loc), parameters, loc);
			Constructor constructor = new Constructor(anonymousTypeClass, name, Modifiers.PUBLIC | Modifiers.DEBUGGER_HIDDEN, null, args, loc);
			constructor.Block = new ToplevelBlock(parent.Module.Compiler, constructor.ParameterInfo, loc);
			bool flag = false;
			for (int k = 0; k < parameters.Count; k++)
			{
				AnonymousTypeParameter anonymousTypeParameter3 = parameters[k];
				Field field = new Field(anonymousTypeClass, array[k], Modifiers.PRIVATE | Modifiers.READONLY | Modifiers.DEBUGGER_HIDDEN, new MemberName("<" + anonymousTypeParameter3.Name + ">", anonymousTypeParameter3.Location), null);
				if (!anonymousTypeClass.AddField(field))
				{
					flag = true;
					continue;
				}
				constructor.Block.AddStatement(new StatementExpression(new SimpleAssign(new MemberAccess(new This(anonymousTypeParameter3.Location), field.Name), constructor.Block.GetParameterReference(k, anonymousTypeParameter3.Location))));
				ToplevelBlock toplevelBlock = new ToplevelBlock(parent.Module.Compiler, anonymousTypeParameter3.Location);
				toplevelBlock.AddStatement(new Return(new MemberAccess(new This(anonymousTypeParameter3.Location), field.Name), anonymousTypeParameter3.Location));
				Property property = new Property(anonymousTypeClass, array[k], Modifiers.PUBLIC, new MemberName(anonymousTypeParameter3.Name, anonymousTypeParameter3.Location), null);
				property.Get = new PropertyBase.GetMethod(property, (Modifiers)0, null, anonymousTypeParameter3.Location);
				property.Get.Block = toplevelBlock;
				anonymousTypeClass.AddMember(property);
			}
			if (flag)
			{
				return null;
			}
			anonymousTypeClass.AddConstructor(constructor);
			return anonymousTypeClass;
		}

		protected override bool DoDefineMembers()
		{
			if (!base.DoDefineMembers())
			{
				return false;
			}
			Location location = base.Location;
			ParametersCompiled parametersCompiled = ParametersCompiled.CreateFullyResolved(new Parameter(new TypeExpression(Compiler.BuiltinTypes.Object, location), "obj", Parameter.Modifier.NONE, null, location), Compiler.BuiltinTypes.Object);
			Method method = new Method(this, new TypeExpression(Compiler.BuiltinTypes.Bool, location), Modifiers.PUBLIC | Modifiers.OVERRIDE | Modifiers.DEBUGGER_HIDDEN, new MemberName("Equals", location), parametersCompiled, null);
			parametersCompiled[0].Resolve(method, 0);
			Method method2 = new Method(this, new TypeExpression(Compiler.BuiltinTypes.String, location), Modifiers.PUBLIC | Modifiers.OVERRIDE | Modifiers.DEBUGGER_HIDDEN, new MemberName("ToString", location), ParametersCompiled.EmptyReadOnlyParameters, null);
			ToplevelBlock toplevelBlock = new ToplevelBlock(Compiler, method.ParameterInfo, location);
			TypeExpr probe_type;
			if (CurrentTypeParameters != null)
			{
				TypeArguments typeArguments = new TypeArguments();
				for (int i = 0; i < CurrentTypeParameters.Count; i++)
				{
					typeArguments.Add(new TypeParameterExpr(CurrentTypeParameters[i], base.Location));
				}
				probe_type = new GenericTypeExpr(base.Definition, typeArguments, location);
			}
			else
			{
				probe_type = new TypeExpression(base.Definition, location);
			}
			LocalVariable localVariable = LocalVariable.CreateCompilerGenerated(CurrentType, toplevelBlock, location);
			toplevelBlock.AddStatement(new BlockVariable(new TypeExpression(localVariable.Type, location), localVariable));
			LocalVariableReference localVariableReference = new LocalVariableReference(localVariable, location);
			MemberAccess expr = new MemberAccess(new MemberAccess(new QualifiedAliasMember("global", "System", location), "Collections", location), "Generic", location);
			Expression expression = null;
			Expression left = new StringConstant(Compiler.BuiltinTypes, "{", location);
			Expression expression2 = new IntConstant(Compiler.BuiltinTypes, -2128831035, location);
			for (int j = 0; j < parameters.Count; j++)
			{
				AnonymousTypeParameter anonymousTypeParameter = parameters[j];
				Field field = (Field)base.Members[j * 2];
				MemberAccess expr2 = new MemberAccess(new MemberAccess(expr, "EqualityComparer", new TypeArguments(new SimpleName(CurrentTypeParameters[j].Name, location)), location), "Default", location);
				Arguments arguments = new Arguments(2);
				arguments.Add(new Argument(new MemberAccess(new This(field.Location), field.Name)));
				arguments.Add(new Argument(new MemberAccess(localVariableReference, field.Name)));
				Expression expression3 = new Invocation(new MemberAccess(expr2, "Equals", location), arguments);
				Arguments arguments2 = new Arguments(1);
				arguments2.Add(new Argument(new MemberAccess(new This(field.Location), field.Name)));
				Expression right = new Invocation(new MemberAccess(expr2, "GetHashCode", location), arguments2);
				IntConstant right2 = new IntConstant(Compiler.BuiltinTypes, 16777619, location);
				expression2 = new Binary(Binary.Operator.Multiply, new Binary(Binary.Operator.ExclusiveOr, expression2, right), right2);
				Expression right3 = new Conditional(new BooleanExpression(new Binary(Binary.Operator.Inequality, new MemberAccess(new This(field.Location), field.Name), new NullLiteral(location))), new Invocation(new MemberAccess(new MemberAccess(new This(field.Location), field.Name), "ToString"), null), new StringConstant(Compiler.BuiltinTypes, string.Empty, location), location);
				if (expression == null)
				{
					expression = expression3;
					left = new Binary(Binary.Operator.Addition, left, new Binary(Binary.Operator.Addition, new StringConstant(Compiler.BuiltinTypes, " " + anonymousTypeParameter.Name + " = ", location), right3));
				}
				else
				{
					left = new Binary(Binary.Operator.Addition, new Binary(Binary.Operator.Addition, left, new StringConstant(Compiler.BuiltinTypes, ", " + anonymousTypeParameter.Name + " = ", location)), right3);
					expression = new Binary(Binary.Operator.LogicalAnd, expression, expression3);
				}
			}
			left = new Binary(Binary.Operator.Addition, left, new StringConstant(Compiler.BuiltinTypes, " }", location));
			TemporaryVariableReference target = new TemporaryVariableReference(localVariable, location);
			toplevelBlock.AddStatement(new StatementExpression(new SimpleAssign(target, new As(toplevelBlock.GetParameterReference(0, location), probe_type, location), location)));
			Expression expression4 = new Binary(Binary.Operator.Inequality, localVariableReference, new NullLiteral(location));
			if (expression != null)
			{
				expression4 = new Binary(Binary.Operator.LogicalAnd, expression4, expression);
			}
			toplevelBlock.AddStatement(new Return(expression4, location));
			method.Block = toplevelBlock;
			method.Define();
			base.Members.Add(method);
			Method method3 = new Method(this, new TypeExpression(Compiler.BuiltinTypes.Int, location), Modifiers.PUBLIC | Modifiers.OVERRIDE | Modifiers.DEBUGGER_HIDDEN, new MemberName("GetHashCode", location), ParametersCompiled.EmptyReadOnlyParameters, null);
			ToplevelBlock toplevelBlock2 = new ToplevelBlock(Compiler, location);
			Block block = new Block(toplevelBlock2, location, location);
			toplevelBlock2.AddStatement(new Unchecked(block, location));
			LocalVariable localVariable2 = LocalVariable.CreateCompilerGenerated(Compiler.BuiltinTypes.Int, toplevelBlock2, location);
			block.AddStatement(new BlockVariable(new TypeExpression(localVariable2.Type, location), localVariable2));
			LocalVariableReference target2 = new LocalVariableReference(localVariable2, location);
			block.AddStatement(new StatementExpression(new SimpleAssign(target2, expression2)));
			LocalVariableReference localVariableReference2 = new LocalVariableReference(localVariable2, location);
			block.AddStatement(new StatementExpression(new CompoundAssign(Binary.Operator.Addition, localVariableReference2, new Binary(Binary.Operator.LeftShift, localVariableReference2, new IntConstant(Compiler.BuiltinTypes, 13, location)))));
			block.AddStatement(new StatementExpression(new CompoundAssign(Binary.Operator.ExclusiveOr, localVariableReference2, new Binary(Binary.Operator.RightShift, localVariableReference2, new IntConstant(Compiler.BuiltinTypes, 7, location)))));
			block.AddStatement(new StatementExpression(new CompoundAssign(Binary.Operator.Addition, localVariableReference2, new Binary(Binary.Operator.LeftShift, localVariableReference2, new IntConstant(Compiler.BuiltinTypes, 3, location)))));
			block.AddStatement(new StatementExpression(new CompoundAssign(Binary.Operator.ExclusiveOr, localVariableReference2, new Binary(Binary.Operator.RightShift, localVariableReference2, new IntConstant(Compiler.BuiltinTypes, 17, location)))));
			block.AddStatement(new StatementExpression(new CompoundAssign(Binary.Operator.Addition, localVariableReference2, new Binary(Binary.Operator.LeftShift, localVariableReference2, new IntConstant(Compiler.BuiltinTypes, 5, location)))));
			block.AddStatement(new Return(localVariableReference2, location));
			method3.Block = toplevelBlock2;
			method3.Define();
			base.Members.Add(method3);
			ToplevelBlock toplevelBlock3 = new ToplevelBlock(Compiler, location);
			toplevelBlock3.AddStatement(new Return(left, location));
			method2.Block = toplevelBlock3;
			method2.Define();
			base.Members.Add(method2);
			return true;
		}

		public override string GetSignatureForError()
		{
			return "anonymous type";
		}

		public override CompilationSourceFile GetCompilationSourceFile()
		{
			return null;
		}
	}
}
