using System.Reflection;
using System.Reflection.Emit;
using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp
{
	public class MethodData
	{
		public readonly IMethodData method;

		public MethodSpec implementing;

		protected InterfaceMemberBase member;

		protected Modifiers modifiers;

		protected MethodAttributes flags;

		protected TypeSpec declaring_type;

		protected MethodSpec parent_method;

		private SourceMethodBuilder debug_builder;

		private string full_name;

		private MethodBuilder builder;

		public MethodBuilder MethodBuilder => builder;

		public TypeSpec DeclaringType => declaring_type;

		public string MetadataName => full_name;

		public MethodData(InterfaceMemberBase member, Modifiers modifiers, MethodAttributes flags, IMethodData method)
		{
			this.member = member;
			this.modifiers = modifiers;
			this.flags = flags;
			this.method = method;
		}

		public MethodData(InterfaceMemberBase member, Modifiers modifiers, MethodAttributes flags, IMethodData method, MethodSpec parent_method)
			: this(member, modifiers, flags, method)
		{
			this.parent_method = parent_method;
		}

		public bool Define(TypeDefinition container, string method_full_name)
		{
			PendingImplementation pendingImplementations = container.PendingImplementations;
			bool optional = false;
			MethodSpec ambiguousCandidate;
			if (pendingImplementations != null)
			{
				implementing = pendingImplementations.IsInterfaceMethod(method.MethodName, member.InterfaceType, this, out ambiguousCandidate, ref optional);
				if (member.InterfaceType != null)
				{
					if (implementing == null)
					{
						if (member is PropertyBase)
						{
							container.Compiler.Report.Error(550, method.Location, "`{0}' is an accessor not found in interface member `{1}{2}'", method.GetSignatureForError(), member.InterfaceType.GetSignatureForError(), member.GetSignatureForError().Substring(member.GetSignatureForError().LastIndexOf('.')));
						}
						else
						{
							container.Compiler.Report.Error(539, method.Location, "`{0}.{1}' in explicit interface declaration is not a member of interface", member.InterfaceType.GetSignatureForError(), member.ShortName);
						}
						return false;
					}
					if (implementing.IsAccessor && !method.IsAccessor)
					{
						container.Compiler.Report.SymbolRelatedToPreviousError(implementing);
						container.Compiler.Report.Error(683, method.Location, "`{0}' explicit method implementation cannot implement `{1}' because it is an accessor", member.GetSignatureForError(), implementing.GetSignatureForError());
						return false;
					}
				}
				else if (implementing != null && !optional)
				{
					if (!method.IsAccessor)
					{
						if (implementing.IsAccessor)
						{
							container.Compiler.Report.SymbolRelatedToPreviousError(implementing);
							container.Compiler.Report.Error(470, method.Location, "Method `{0}' cannot implement interface accessor `{1}'", method.GetSignatureForError(), TypeManager.CSharpSignature(implementing));
						}
					}
					else if (implementing.DeclaringType.IsInterface)
					{
						if (!implementing.IsAccessor)
						{
							container.Compiler.Report.SymbolRelatedToPreviousError(implementing);
							container.Compiler.Report.Error(686, method.Location, "Accessor `{0}' cannot implement interface member `{1}' for type `{2}'. Use an explicit interface implementation", method.GetSignatureForError(), TypeManager.CSharpSignature(implementing), container.GetSignatureForError());
						}
						else
						{
							PropertyBase.PropertyMethod propertyMethod = method as PropertyBase.PropertyMethod;
							if (propertyMethod != null && propertyMethod.HasCustomAccessModifier && (propertyMethod.ModFlags & Modifiers.PUBLIC) == 0)
							{
								container.Compiler.Report.SymbolRelatedToPreviousError(implementing);
								container.Compiler.Report.Error(277, method.Location, "Accessor `{0}' must be declared public to implement interface member `{1}'", method.GetSignatureForError(), implementing.GetSignatureForError());
							}
						}
					}
				}
			}
			else
			{
				ambiguousCandidate = null;
			}
			if (implementing != null)
			{
				if (member.IsExplicitImpl)
				{
					if (method.ParameterInfo.HasParams && !implementing.Parameters.HasParams)
					{
						container.Compiler.Report.SymbolRelatedToPreviousError(implementing);
						container.Compiler.Report.Error(466, method.Location, "`{0}': the explicit interface implementation cannot introduce the params modifier", method.GetSignatureForError());
					}
					if (ambiguousCandidate != null)
					{
						container.Compiler.Report.SymbolRelatedToPreviousError(ambiguousCandidate);
						container.Compiler.Report.SymbolRelatedToPreviousError(implementing);
						container.Compiler.Report.Warning(473, 2, method.Location, "Explicit interface implementation `{0}' matches more than one interface member. Consider using a non-explicit implementation instead", method.GetSignatureForError());
					}
				}
				else if (implementing.DeclaringType.IsInterface)
				{
					if ((flags & MethodAttributes.MemberAccessMask) != MethodAttributes.Public)
					{
						implementing = null;
					}
					else if (optional && (container.Interfaces == null || !container.Definition.Interfaces.Contains(implementing.DeclaringType)))
					{
						implementing = null;
					}
				}
				else if ((flags & MethodAttributes.MemberAccessMask) == MethodAttributes.Private)
				{
					implementing = null;
				}
				else if ((modifiers & Modifiers.OVERRIDE) == 0)
				{
					implementing = null;
				}
				if ((modifiers & Modifiers.STATIC) != 0)
				{
					implementing = null;
				}
			}
			if (implementing != null)
			{
				if ((modifiers & Modifiers.OVERRIDE) == 0 && implementing.DeclaringType.IsInterface)
				{
					flags |= MethodAttributes.VtableLayoutMask;
				}
				flags |= MethodAttributes.Virtual | MethodAttributes.HideBySig;
				if ((modifiers & (Modifiers.ABSTRACT | Modifiers.VIRTUAL | Modifiers.OVERRIDE)) == 0)
				{
					flags |= MethodAttributes.Final;
				}
				pendingImplementations.ImplementMethod(method.MethodName, member.InterfaceType, this, member.IsExplicitImpl, out ambiguousCandidate, ref optional);
				if (!implementing.DeclaringType.IsInterface && !member.IsExplicitImpl && implementing.IsAccessor)
				{
					method_full_name = implementing.MemberDefinition.Name;
				}
			}
			full_name = method_full_name;
			declaring_type = container.Definition;
			return true;
		}

		private void DefineOverride(TypeDefinition container)
		{
			if (implementing != null && member.IsExplicitImpl)
			{
				container.TypeBuilder.DefineMethodOverride(builder, (MethodInfo)implementing.GetMetaInfo());
			}
		}

		public MethodBuilder DefineMethodBuilder(TypeDefinition container)
		{
			if (builder != null)
			{
				throw new InternalErrorException();
			}
			builder = container.TypeBuilder.DefineMethod(full_name, flags, method.CallingConventions);
			return builder;
		}

		public MethodBuilder DefineMethodBuilder(TypeDefinition container, ParametersCompiled param)
		{
			DefineMethodBuilder(container);
			builder.SetReturnType(method.ReturnType.GetMetaInfo());
			builder.SetParameters(param.GetMetaInfo());
			return builder;
		}

		public void Emit(TypeDefinition parent)
		{
			DefineOverride(parent);
			method.ParameterInfo.ApplyAttributes(method, MethodBuilder);
			ToplevelBlock block = method.Block;
			if (block != null)
			{
				BlockContext bc = new BlockContext(method, block, method.ReturnType);
				if (block.Resolve(bc, method))
				{
					debug_builder = member.Parent.CreateMethodSymbolEntry();
					EmitContext ec = method.CreateEmitContext(MethodBuilder.GetILGenerator(), debug_builder);
					block.Emit(ec);
				}
			}
		}

		public void WriteDebugSymbol(MonoSymbolFile file)
		{
			if (debug_builder != null)
			{
				int token = builder.GetToken().Token;
				debug_builder.DefineMethod(file, token);
			}
		}
	}
}
