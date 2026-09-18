namespace Mono.CSharp
{
	public class Using : TryFinallyBlock
	{
		public class VariableDeclaration : BlockVariable
		{
			private Statement dispose_call;

			public bool IsNested { get; private set; }

			public VariableDeclaration(FullNamedExpression type, LocalVariable li)
				: base(type, li)
			{
			}

			public VariableDeclaration(LocalVariable li, Location loc)
				: base(li)
			{
				reachable = true;
				base.loc = loc;
			}

			public VariableDeclaration(Expression expr)
				: base(null)
			{
				loc = expr.Location;
				base.Initializer = expr;
			}

			public void EmitDispose(EmitContext ec)
			{
				dispose_call.Emit(ec);
			}

			public override bool Resolve(BlockContext bc)
			{
				if (IsNested)
				{
					return true;
				}
				return Resolve(bc, false);
			}

			public Expression ResolveExpression(BlockContext bc)
			{
				Expression expression = base.Initializer.Resolve(bc);
				if (expression == null)
				{
					return null;
				}
				li = LocalVariable.CreateCompilerGenerated(expression.Type, bc.CurrentBlock, loc);
				base.Initializer = ResolveInitializer(bc, base.Variable, expression);
				return expression;
			}

			protected override Expression ResolveInitializer(BlockContext bc, LocalVariable li, Expression initializer)
			{
				if (li.Type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
				{
					initializer = initializer.Resolve(bc);
					if (initializer == null)
					{
						return null;
					}
					Arguments arguments = new Arguments(1);
					arguments.Add(new Argument(initializer));
					initializer = new DynamicConversion(bc.BuiltinTypes.IDisposable, CSharpBinderFlags.None, arguments, initializer.Location).Resolve(bc);
					if (initializer == null)
					{
						return null;
					}
					LocalVariable localVariable = LocalVariable.CreateCompilerGenerated(initializer.Type, bc.CurrentBlock, loc);
					dispose_call = CreateDisposeCall(bc, localVariable);
					dispose_call.Resolve(bc);
					return base.ResolveInitializer(bc, li, new SimpleAssign(localVariable.CreateReferenceExpression(bc, loc), initializer, loc));
				}
				if (li == base.Variable)
				{
					CheckIDiposableConversion(bc, li, initializer);
					dispose_call = CreateDisposeCall(bc, li);
					dispose_call.Resolve(bc);
				}
				return base.ResolveInitializer(bc, li, initializer);
			}

			protected virtual void CheckIDiposableConversion(BlockContext bc, LocalVariable li, Expression initializer)
			{
				TypeSpec typeSpec = li.Type;
				if (typeSpec.BuiltinType != BuiltinTypeSpec.Type.IDisposable && !CanConvertToIDisposable(bc, typeSpec) && !typeSpec.IsNullableType && typeSpec != InternalType.ErrorType)
				{
					bc.Report.SymbolRelatedToPreviousError(typeSpec);
					Location location = ((type_expr == null) ? initializer.Location : type_expr.Location);
					bc.Report.Error(1674, location, "`{0}': type used in a using statement must be implicitly convertible to `System.IDisposable'", typeSpec.GetSignatureForError());
				}
			}

			private static bool CanConvertToIDisposable(BlockContext bc, TypeSpec type)
			{
				BuiltinTypeSpec iDisposable = bc.BuiltinTypes.IDisposable;
				TypeParameterSpec typeParameterSpec = type as TypeParameterSpec;
				if (typeParameterSpec != null)
				{
					return Convert.ImplicitTypeParameterConversion(null, typeParameterSpec, iDisposable) != null;
				}
				return type.ImplementsInterface(iDisposable, false);
			}

			protected virtual Statement CreateDisposeCall(BlockContext bc, LocalVariable lv)
			{
				Expression expression = lv.CreateReferenceExpression(bc, lv.Location);
				TypeSpec typeSpec = lv.Type;
				Location location = lv.Location;
				BuiltinTypeSpec iDisposable = bc.BuiltinTypes.IDisposable;
				MethodGroupExpr methodGroupExpr = MethodGroupExpr.CreatePredefined(bc.Module.PredefinedMembers.IDisposableDispose.Resolve(location), iDisposable, location);
				methodGroupExpr.InstanceExpression = (typeSpec.IsNullableType ? new Cast(new TypeExpression(iDisposable, location), expression, location).Resolve(bc) : expression);
				Statement statement = new StatementExpression(new Invocation(methodGroupExpr, null), Location.Null);
				if (!TypeSpec.IsValueType(typeSpec) || typeSpec.IsNullableType)
				{
					Binary bool_expr = new Binary(Binary.Operator.Inequality, expression, new NullLiteral(location));
					Statement statement2 = statement;
					statement = new If(bool_expr, statement2, statement2.loc);
				}
				return statement;
			}

			public void ResolveDeclaratorInitializer(BlockContext bc)
			{
				base.Initializer = base.ResolveInitializer(bc, base.Variable, base.Initializer);
			}

			public Statement RewriteUsingDeclarators(BlockContext bc, Statement stmt)
			{
				for (int num = declarators.Count - 1; num >= 0; num--)
				{
					BlockVariableDeclarator blockVariableDeclarator = declarators[num];
					VariableDeclaration variableDeclaration = new VariableDeclaration(blockVariableDeclarator.Variable, blockVariableDeclarator.Variable.Location);
					variableDeclaration.Initializer = blockVariableDeclarator.Initializer;
					variableDeclaration.IsNested = true;
					variableDeclaration.dispose_call = CreateDisposeCall(bc, blockVariableDeclarator.Variable);
					variableDeclaration.dispose_call.Resolve(bc);
					stmt = new Using(variableDeclaration, stmt, blockVariableDeclarator.Variable.Location);
				}
				declarators = null;
				return stmt;
			}

			public override object Accept(StructuralVisitor visitor)
			{
				return visitor.Visit(this);
			}
		}

		private VariableDeclaration decl;

		public Expression Expr
		{
			get
			{
				if (decl.Variable != null)
				{
					return null;
				}
				return decl.Initializer;
			}
		}

		public BlockVariable Variables => decl;

		public Using(VariableDeclaration decl, Statement stmt, Location loc)
			: base(stmt, loc)
		{
			this.decl = decl;
		}

		public Using(Expression expr, Statement stmt, Location loc)
			: base(stmt, loc)
		{
			decl = new VariableDeclaration(expr);
		}

		public override void Emit(EmitContext ec)
		{
			DoEmit(ec);
		}

		protected override void EmitTryBodyPrepare(EmitContext ec)
		{
			decl.Emit(ec);
			base.EmitTryBodyPrepare(ec);
		}

		protected override void EmitTryBody(EmitContext ec)
		{
			stmt.Emit(ec);
		}

		public override void EmitFinallyBody(EmitContext ec)
		{
			decl.EmitDispose(ec);
		}

		protected override bool DoFlowAnalysis(FlowAnalysisContext fc)
		{
			decl.FlowAnalysis(fc);
			return stmt.FlowAnalysis(fc);
		}

		public override Reachability MarkReachable(Reachability rc)
		{
			decl.MarkReachable(rc);
			return base.MarkReachable(rc);
		}

		public override bool Resolve(BlockContext ec)
		{
			bool isLockedByStatement = false;
			VariableReference variableReference;
			using (ec.Set(ResolveContext.Options.UsingInitializerScope))
			{
				if (decl.Variable == null)
				{
					variableReference = decl.ResolveExpression(ec) as VariableReference;
					if (variableReference != null)
					{
						isLockedByStatement = variableReference.IsLockedByStatement;
						variableReference.IsLockedByStatement = true;
					}
				}
				else
				{
					if (decl.IsNested)
					{
						decl.ResolveDeclaratorInitializer(ec);
					}
					else
					{
						if (!decl.Resolve(ec))
						{
							return false;
						}
						if (decl.Declarators != null)
						{
							stmt = decl.RewriteUsingDeclarators(ec, stmt);
						}
					}
					variableReference = null;
				}
			}
			bool result = base.Resolve(ec);
			if (variableReference != null)
			{
				variableReference.IsLockedByStatement = isLockedByStatement;
			}
			return result;
		}

		protected override void CloneTo(CloneContext clonectx, Statement t)
		{
			Using obj = (Using)t;
			obj.decl = (VariableDeclaration)decl.Clone(clonectx);
			obj.stmt = stmt.Clone(clonectx);
		}

		public override object Accept(StructuralVisitor visitor)
		{
			return visitor.Visit(this);
		}
	}
}
