using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class ParametersCompiled : AParametersCollection
	{
		public static readonly ParametersCompiled EmptyReadOnlyParameters = new ParametersCompiled();

		public static readonly ParametersCompiled Undefined = new ParametersCompiled();

		public Parameter this[int pos] => (Parameter)parameters[pos];

		private ParametersCompiled()
		{
			parameters = new Parameter[0];
			types = TypeSpec.EmptyTypes;
		}

		private ParametersCompiled(IParameterData[] parameters, TypeSpec[] types)
		{
			base.parameters = parameters;
			base.types = types;
		}

		public ParametersCompiled(params Parameter[] parameters)
		{
			if (parameters == null || parameters.Length == 0)
			{
				throw new ArgumentException("Use EmptyReadOnlyParameters");
			}
			base.parameters = parameters;
			int num = parameters.Length;
			for (int i = 0; i < num; i++)
			{
				has_params |= (parameters[i].ModFlags & Parameter.Modifier.PARAMS) != 0;
			}
		}

		public ParametersCompiled(Parameter[] parameters, bool has_arglist)
			: this(parameters)
		{
			base.has_arglist = has_arglist;
		}

		public static ParametersCompiled CreateFullyResolved(Parameter p, TypeSpec type)
		{
			return new ParametersCompiled(new Parameter[1] { p }, new TypeSpec[1] { type });
		}

		public static ParametersCompiled CreateFullyResolved(Parameter[] parameters, TypeSpec[] types)
		{
			return new ParametersCompiled(parameters, types);
		}

		public static ParametersCompiled Prefix(ParametersCompiled parameters, Parameter p, TypeSpec type)
		{
			TypeSpec[] array = new TypeSpec[parameters.Count + 1];
			array[0] = type;
			Array.Copy(parameters.Types, 0, array, 1, parameters.Count);
			Parameter[] array2 = new Parameter[array.Length];
			array2[0] = p;
			for (int i = 0; i < parameters.Count; i++)
			{
				(array2[i + 1] = parameters[i]).SetIndex(i + 1);
			}
			return CreateFullyResolved(array2, array);
		}

		public static AParametersCollection CreateFullyResolved(params TypeSpec[] types)
		{
			ParameterData[] array = new ParameterData[types.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new ParameterData(null, Parameter.Modifier.NONE, null);
			}
			return new ParametersCompiled(array, types);
		}

		public static ParametersCompiled CreateImplicitParameter(FullNamedExpression texpr, Location loc)
		{
			return new ParametersCompiled(new Parameter[1]
			{
				new Parameter(texpr, "value", Parameter.Modifier.NONE, null, loc)
			}, null);
		}

		public void CheckConstraints(IMemberContext mc)
		{
			IParameterData[] array = parameters;
			for (int i = 0; i < array.Length; i++)
			{
				Parameter parameter = (Parameter)array[i];
				if (parameter.TypeExpression != null)
				{
					ConstraintChecker.Check(mc, parameter.Type, parameter.TypeExpression.Location);
				}
			}
		}

		public static int IsSameClsSignature(AParametersCollection a, AParametersCollection b)
		{
			int num = 0;
			for (int i = 0; i < a.Count; i++)
			{
				TypeSpec typeSpec = a.Types[i];
				TypeSpec typeSpec2 = b.Types[i];
				if (TypeSpecComparer.Override.IsEqual(typeSpec, typeSpec2))
				{
					if ((a.FixedParameters[i].ModFlags & Parameter.Modifier.RefOutMask) != (b.FixedParameters[i].ModFlags & Parameter.Modifier.RefOutMask))
					{
						num |= 1;
					}
					continue;
				}
				ArrayContainer arrayContainer = typeSpec as ArrayContainer;
				if (arrayContainer == null)
				{
					return 0;
				}
				ArrayContainer arrayContainer2 = typeSpec2 as ArrayContainer;
				if (arrayContainer2 == null)
				{
					return 0;
				}
				if (arrayContainer.Element is ArrayContainer || arrayContainer2.Element is ArrayContainer)
				{
					num |= 2;
					continue;
				}
				if (arrayContainer.Rank != arrayContainer2.Rank && TypeSpecComparer.Override.IsEqual(arrayContainer.Element, arrayContainer2.Element))
				{
					num |= 1;
					continue;
				}
				return 0;
			}
			return num;
		}

		public static ParametersCompiled MergeGenerated(CompilerContext ctx, ParametersCompiled userParams, bool checkConflicts, Parameter compilerParams, TypeSpec compilerTypes)
		{
			return MergeGenerated(ctx, userParams, checkConflicts, new Parameter[1] { compilerParams }, new TypeSpec[1] { compilerTypes });
		}

		public static ParametersCompiled MergeGenerated(CompilerContext ctx, ParametersCompiled userParams, bool checkConflicts, Parameter[] compilerParams, TypeSpec[] compilerTypes)
		{
			Parameter[] array = new Parameter[userParams.Count + compilerParams.Length];
			userParams.FixedParameters.CopyTo(array, 0);
			TypeSpec[] array2;
			if (userParams.types != null)
			{
				array2 = new TypeSpec[array.Length];
				userParams.Types.CopyTo(array2, 0);
			}
			else
			{
				array2 = null;
			}
			int num = userParams.Count;
			int num2 = 0;
			foreach (Parameter parameter in compilerParams)
			{
				for (int j = 0; j < num; j++)
				{
					while (parameter.Name == array[j].Name)
					{
						if (checkConflicts && j < userParams.Count)
						{
							ctx.Report.Error(316, userParams[j].Location, "The parameter name `{0}' conflicts with a compiler generated name", parameter.Name);
						}
						parameter.Name = "_" + parameter.Name;
					}
				}
				array[num] = parameter;
				if (array2 != null)
				{
					array2[num] = compilerTypes[num2++];
				}
				num++;
			}
			return new ParametersCompiled(array, array2)
			{
				has_params = userParams.has_params
			};
		}

		public void CheckParameters(MemberCore member)
		{
			for (int i = 0; i < parameters.Length; i++)
			{
				string name = parameters[i].Name;
				for (int j = i + 1; j < parameters.Length; j++)
				{
					if (parameters[j].Name == name)
					{
						this[j].Error_DuplicateName(member.Compiler.Report);
					}
				}
			}
		}

		public bool Resolve(IMemberContext ec)
		{
			if (types != null)
			{
				return true;
			}
			types = new TypeSpec[base.Count];
			bool result = true;
			for (int i = 0; i < base.FixedParameters.Length; i++)
			{
				TypeSpec typeSpec = this[i].Resolve(ec, i);
				if (typeSpec == null)
				{
					result = false;
				}
				else
				{
					types[i] = typeSpec;
				}
			}
			return result;
		}

		public void ResolveDefaultValues(MemberCore m)
		{
			ResolveContext resolveContext = null;
			for (int i = 0; i < parameters.Length; i++)
			{
				Parameter parameter = (Parameter)parameters[i];
				if (parameter.HasDefaultValue || parameter.OptAttributes != null)
				{
					if (resolveContext == null)
					{
						resolveContext = new ResolveContext(m);
					}
					parameter.ResolveDefaultValue(resolveContext);
				}
			}
		}

		public void ApplyAttributes(IMemberContext mc, MethodBase builder)
		{
			if (base.Count != 0)
			{
				MethodBuilder mb = builder as MethodBuilder;
				ConstructorBuilder cb = builder as ConstructorBuilder;
				PredefinedAttributes predefinedAttributes = mc.Module.PredefinedAttributes;
				for (int i = 0; i < base.Count; i++)
				{
					this[i].ApplyAttributes(mb, cb, i + 1, predefinedAttributes);
				}
			}
		}

		public void VerifyClsCompliance(IMemberContext ctx)
		{
			IParameterData[] fixedParameters = base.FixedParameters;
			for (int i = 0; i < fixedParameters.Length; i++)
			{
				((Parameter)fixedParameters[i]).IsClsCompliant(ctx);
			}
		}

		public Expression CreateExpressionTree(BlockContext ec, Location loc)
		{
			ArrayInitializer arrayInitializer = new ArrayInitializer(base.Count, loc);
			IParameterData[] fixedParameters = base.FixedParameters;
			for (int i = 0; i < fixedParameters.Length; i++)
			{
				Parameter parameter = (Parameter)fixedParameters[i];
				StatementExpression statementExpression = new StatementExpression(parameter.CreateExpressionTreeVariable(ec), Location.Null);
				if (statementExpression.Resolve(ec))
				{
					ec.CurrentBlock.AddScopeStatement(new TemporaryVariableReference.Declarator(parameter.ExpressionTreeVariableReference()));
					ec.CurrentBlock.AddScopeStatement(statementExpression);
				}
				arrayInitializer.Add(parameter.ExpressionTreeVariableReference());
			}
			return new ArrayCreation(Parameter.ResolveParameterExpressionType(ec, loc), arrayInitializer, loc);
		}

		public ParametersCompiled Clone()
		{
			ParametersCompiled parametersCompiled = (ParametersCompiled)MemberwiseClone();
			parametersCompiled.parameters = new IParameterData[parameters.Length];
			for (int i = 0; i < base.Count; i++)
			{
				parametersCompiled.parameters[i] = this[i].Clone();
			}
			return parametersCompiled;
		}
	}
}
