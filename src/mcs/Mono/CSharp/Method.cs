using System;
using System.Linq;

namespace Mono.CSharp
{
	public class Method : MethodOrOperator, IGenericMethodDefinition, IMethodDefinition, IMemberDefinition
	{
		private Method partialMethodImplementation;

		public override TypeParameters CurrentTypeParameters => base.MemberName.TypeParameters;

		public TypeParameterSpec[] TypeParameters => CurrentTypeParameters.Types;

		public int TypeParametersCount
		{
			get
			{
				if (CurrentTypeParameters != null)
				{
					return CurrentTypeParameters.Count;
				}
				return 0;
			}
		}

		public Method(TypeDefinition parent, FullNamedExpression return_type, Modifiers mod, MemberName name, ParametersCompiled parameters, Attributes attrs)
			: base(parent, return_type, mod, (parent.PartialContainer.Kind == MemberKind.Interface) ? (Modifiers.NEW | Modifiers.UNSAFE) : ((parent.PartialContainer.Kind == MemberKind.Struct) ? (Modifiers.AccessibilityMask | Modifiers.AllowedExplicitImplFlags | Modifiers.NEW | Modifiers.STATIC | Modifiers.OVERRIDE | Modifiers.ASYNC) : (Modifiers.AccessibilityMask | Modifiers.AllowedExplicitImplFlags | Modifiers.NEW | Modifiers.ABSTRACT | Modifiers.SEALED | Modifiers.STATIC | Modifiers.VIRTUAL | Modifiers.OVERRIDE | Modifiers.ASYNC)), name, attrs, parameters)
		{
		}

		protected Method(TypeDefinition parent, FullNamedExpression return_type, Modifiers mod, Modifiers amod, MemberName name, ParametersCompiled parameters, Attributes attrs)
			: base(parent, return_type, mod, amod, name, attrs, parameters)
		{
		}

		public override void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}

		public static Method Create(TypeDefinition parent, FullNamedExpression returnType, Modifiers mod, MemberName name, ParametersCompiled parameters, Attributes attrs)
		{
			Method method = new Method(parent, returnType, mod, name, parameters, attrs);
			if ((mod & Modifiers.PARTIAL) != 0)
			{
				if ((mod & (Modifiers.AccessibilityMask | Modifiers.NEW | Modifiers.ABSTRACT | Modifiers.SEALED | Modifiers.VIRTUAL | Modifiers.OVERRIDE | Modifiers.EXTERN)) != 0)
				{
					method.Report.Error(750, method.Location, "A partial method cannot define access modifier or any of abstract, extern, new, override, sealed, or virtual modifiers");
					mod &= ~(Modifiers.AccessibilityMask | Modifiers.NEW | Modifiers.ABSTRACT | Modifiers.SEALED | Modifiers.VIRTUAL | Modifiers.OVERRIDE | Modifiers.EXTERN);
				}
				if ((parent.ModFlags & Modifiers.PARTIAL) == 0)
				{
					method.Report.Error(751, method.Location, "A partial method must be declared within a partial class or partial struct");
				}
			}
			if ((mod & Modifiers.STATIC) == 0 && parameters.HasExtensionMethodType)
			{
				method.Report.Error(1105, method.Location, "`{0}': Extension methods must be declared static", method.GetSignatureForError());
			}
			return method;
		}

		public override string GetSignatureForError()
		{
			return base.GetSignatureForError() + parameters.GetSignatureForError();
		}

		private void Error_DuplicateEntryPoint(Method b)
		{
			base.Report.Error(17, b.Location, "Program `{0}' has more than one entry point defined: `{1}'", b.Module.Builder.ScopeName, b.GetSignatureForError());
		}

		private bool IsEntryPoint()
		{
			if (base.ReturnType.Kind != MemberKind.Void && base.ReturnType.BuiltinType != BuiltinTypeSpec.Type.Int)
			{
				return false;
			}
			if (parameters.IsEmpty)
			{
				return true;
			}
			if (parameters.Count > 1)
			{
				return false;
			}
			ArrayContainer arrayContainer = parameters.Types[0] as ArrayContainer;
			if (arrayContainer != null && arrayContainer.Rank == 1 && arrayContainer.Element.BuiltinType == BuiltinTypeSpec.Type.String)
			{
				return (parameters[0].ModFlags & Parameter.Modifier.RefOutMask) == 0;
			}
			return false;
		}

		public override FullNamedExpression LookupNamespaceOrType(string name, int arity, LookupMode mode, Location loc)
		{
			if (arity == 0)
			{
				TypeParameters currentTypeParameters = CurrentTypeParameters;
				if (currentTypeParameters != null)
				{
					TypeParameter typeParameter = currentTypeParameters.Find(name);
					if (typeParameter != null)
					{
						return new TypeParameterExpr(typeParameter, loc);
					}
				}
			}
			return base.LookupNamespaceOrType(name, arity, mode, loc);
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Type == pa.Conditional)
			{
				if (IsExplicitImpl)
				{
					Error_ConditionalAttributeIsNotValid();
					return;
				}
				if ((base.ModFlags & Modifiers.OVERRIDE) != 0)
				{
					base.Report.Error(243, base.Location, "Conditional not valid on `{0}' because it is an override method", GetSignatureForError());
					return;
				}
				if (base.ReturnType.Kind != MemberKind.Void)
				{
					base.Report.Error(578, base.Location, "Conditional not valid on `{0}' because its return type is not void", GetSignatureForError());
					return;
				}
				if (IsInterface)
				{
					base.Report.Error(582, base.Location, "Conditional not valid on interface members");
					return;
				}
				if (MethodData.implementing != null)
				{
					base.Report.SymbolRelatedToPreviousError(MethodData.implementing.DeclaringType);
					base.Report.Error(629, base.Location, "Conditional member `{0}' cannot implement interface member `{1}'", GetSignatureForError(), TypeManager.CSharpSignature(MethodData.implementing));
					return;
				}
				for (int i = 0; i < parameters.Count; i++)
				{
					if ((parameters.FixedParameters[i].ModFlags & Parameter.Modifier.OUT) != Parameter.Modifier.NONE)
					{
						base.Report.Error(685, base.Location, "Conditional method `{0}' cannot have an out parameter", GetSignatureForError());
						return;
					}
				}
			}
			if (a.Type == pa.Extension)
			{
				a.Error_MisusedExtensionAttribute();
			}
			else
			{
				base.ApplyAttributeBuilder(a, ctor, cdata, pa);
			}
		}

		private void CreateTypeParameters()
		{
			TypeParameters typeParameters = base.MemberName.TypeParameters;
			TypeParameters typeParametersAll = Parent.TypeParametersAll;
			for (int i = 0; i < base.MemberName.Arity; i++)
			{
				string name = typeParameters[i].MemberName.Name;
				if (block == null)
				{
					if (parameters.GetParameterIndexByName(name) >= 0)
					{
						ToplevelBlock toplevelBlock = block;
						if (toplevelBlock == null)
						{
							toplevelBlock = new ToplevelBlock(Compiler, base.Location);
						}
						toplevelBlock.Error_AlreadyDeclaredTypeParameter(name, parameters[i].Location);
					}
				}
				else
				{
					INamedBlockVariable variable = null;
					block.GetLocalName(name, block, ref variable);
					variable?.Block.Error_AlreadyDeclaredTypeParameter(name, variable.Location);
				}
				if (typeParametersAll != null)
				{
					TypeParameter typeParameter = typeParametersAll.Find(name);
					if (typeParameter != null)
					{
						typeParameters[i].WarningParentNameConflict(typeParameter);
					}
				}
			}
			typeParameters.Create(null, 0, Parent);
		}

		protected virtual void DefineTypeParameters()
		{
			TypeParameters currentTypeParameters = CurrentTypeParameters;
			TypeParameterSpec[] array = null;
			TypeParameterSpec[] array2 = TypeParameterSpec.EmptyTypes;
			TypeSpec[] array3 = TypeSpec.EmptyTypes;
			if ((base.ModFlags & Modifiers.OVERRIDE) != 0 || IsExplicitImpl)
			{
				MethodSpec methodSpec = base_method ?? MethodData.implementing;
				if (methodSpec != null)
				{
					array = methodSpec.GenericDefinition.TypeParameters;
					if (methodSpec.DeclaringType.IsGeneric)
					{
						array2 = methodSpec.DeclaringType.MemberDefinition.TypeParameters;
						if (base_method != null)
						{
							TypeSpec typeSpec = CurrentType;
							while (typeSpec.BaseType != methodSpec.DeclaringType)
							{
								typeSpec = typeSpec.BaseType;
							}
							array3 = typeSpec.BaseType.TypeArguments;
						}
						else
						{
							foreach (TypeSpec @interface in Parent.CurrentType.Interfaces)
							{
								if (@interface == methodSpec.DeclaringType)
								{
									array3 = @interface.TypeArguments;
									break;
								}
							}
						}
					}
					if (methodSpec.IsGeneric)
					{
						TypeParameterSpec[] array4 = array;
						foreach (TypeParameterSpec typeParameterSpec in array4)
						{
							ObsoleteAttribute attributeObsolete = typeParameterSpec.BaseType.GetAttributeObsolete();
							if (attributeObsolete != null)
							{
								AttributeTester.Report_ObsoleteMessage(attributeObsolete, typeParameterSpec.BaseType.GetSignatureForError(), base.Location, base.Report);
							}
							if (typeParameterSpec.InterfacesDefined == null)
							{
								continue;
							}
							TypeSpec[] interfacesDefined = typeParameterSpec.InterfacesDefined;
							foreach (TypeSpec typeSpec2 in interfacesDefined)
							{
								attributeObsolete = typeSpec2.GetAttributeObsolete();
								if (attributeObsolete != null)
								{
									AttributeTester.Report_ObsoleteMessage(attributeObsolete, typeSpec2.GetSignatureForError(), base.Location, base.Report);
								}
							}
						}
						if (array2.Length != 0)
						{
							array2 = array2.Concat(array).ToArray();
							array3 = array3.Concat(currentTypeParameters.Types).ToArray();
						}
						else
						{
							array2 = array;
							array3 = currentTypeParameters.Types;
						}
					}
				}
			}
			for (int k = 0; k < currentTypeParameters.Count; k++)
			{
				TypeParameter typeParameter = currentTypeParameters[k];
				if (array == null)
				{
					typeParameter.ResolveConstraints(this);
					continue;
				}
				TypeParameterSpec typeParameterSpec2 = array[k];
				TypeParameterSpec type = typeParameter.Type;
				type.SpecialConstraint = typeParameterSpec2.SpecialConstraint;
				TypeParameterInflator inflator = new TypeParameterInflator(this, CurrentType, array2, array3);
				typeParameterSpec2.InflateConstraints(inflator, type);
				TypeSpec[] array5 = type.TypeArguments;
				if (array5 == null)
				{
					continue;
				}
				for (int l = 0; l < array5.Length; l++)
				{
					TypeSpec typeSpec3 = array5[l];
					if (!typeSpec3.IsClass && !typeSpec3.IsStruct)
					{
						continue;
					}
					TypeSpec[] array6 = null;
					for (int m = l + 1; m < array5.Length; m++)
					{
						TypeSpec typeSpec4 = array5[m];
						if (TypeSpecComparer.IsEqual(typeSpec3, typeSpec4) || TypeSpec.IsBaseClass(typeSpec3, typeSpec4, false))
						{
							array6 = new TypeSpec[array5.Length - 1];
							Array.Copy(array5, 0, array6, 0, m);
							Array.Copy(array5, m + 1, array6, m, array5.Length - m - 1);
						}
						else if (!TypeSpec.IsBaseClass(typeSpec4, typeSpec3, false))
						{
							Constraints.Error_ConflictingConstraints(this, type, typeSpec3, typeSpec4, base.Location);
						}
					}
					if (array6 != null)
					{
						array5 = (type.TypeArguments = array6);
					}
					else
					{
						Constraints.CheckConflictingInheritedConstraint(type, typeSpec3, this, base.Location);
					}
				}
			}
			if (array == null && MethodData != null && MethodData.implementing != null)
			{
				CheckImplementingMethodConstraints(Parent, spec, MethodData.implementing);
			}
		}

		public static bool CheckImplementingMethodConstraints(TypeContainer container, MethodSpec method, MethodSpec baseMethod)
		{
			TypeParameterSpec[] constraints = method.Constraints;
			TypeParameterSpec[] constraints2 = baseMethod.Constraints;
			for (int i = 0; i < constraints.Length; i++)
			{
				if (!constraints[i].HasSameConstraintsImplementation(constraints2[i]))
				{
					container.Compiler.Report.SymbolRelatedToPreviousError(method);
					container.Compiler.Report.SymbolRelatedToPreviousError(baseMethod);
					MemberCore memberCore = (constraints[i].MemberDefinition as MemberCore) ?? container;
					container.Compiler.Report.Error(425, memberCore.Location, "The constraints for type parameter `{0}' of method `{1}' must match the constraints for type parameter `{2}' of interface method `{3}'. Consider using an explicit interface implementation instead", constraints[i].GetSignatureForError(), method.GetSignatureForError(), constraints2[i].GetSignatureForError(), baseMethod.GetSignatureForError());
					return false;
				}
			}
			return true;
		}

		public override bool Define()
		{
			if (!base.Define())
			{
				return false;
			}
			if (member_type.Kind == MemberKind.Void && parameters.IsEmpty && base.MemberName.Arity == 0 && base.MemberName.Name == Destructor.MetadataName)
			{
				base.Report.Warning(465, 1, base.Location, "Introducing `Finalize' method can interfere with destructor invocation. Did you intend to declare a destructor?");
			}
			if (Compiler.Settings.StdLib && base.ReturnType.IsSpecialRuntimeType)
			{
				Error1599(base.Location, base.ReturnType, base.Report);
				return false;
			}
			if (CurrentTypeParameters == null)
			{
				if (base_method != null && !IsExplicitImpl)
				{
					if (parameters.Count == 1 && base.ParameterTypes[0].BuiltinType == BuiltinTypeSpec.Type.Object && base.MemberName.Name == "Equals")
					{
						Parent.PartialContainer.Mark_HasEquals();
					}
					else if (parameters.IsEmpty && base.MemberName.Name == "GetHashCode")
					{
						Parent.PartialContainer.Mark_HasGetHashCode();
					}
				}
			}
			else
			{
				DefineTypeParameters();
			}
			if (block != null)
			{
				if (block.IsIterator)
				{
					Iterator.CreateIterator(this, Parent.PartialContainer, base.ModFlags);
					base.ModFlags |= Modifiers.DEBUGGER_HIDDEN;
				}
				if ((base.ModFlags & Modifiers.ASYNC) != 0)
				{
					if (base.ReturnType.Kind != MemberKind.Void && base.ReturnType != Module.PredefinedTypes.Task.TypeSpec && !base.ReturnType.IsGenericTask)
					{
						base.Report.Error(1983, base.Location, "The return type of an async method must be void, Task, or Task<T>");
					}
					block = (ToplevelBlock)block.ConvertToAsyncTask(this, Parent.PartialContainer, parameters, base.ReturnType, null, base.Location);
					base.ModFlags |= Modifiers.DEBUGGER_STEP_THROUGH;
				}
				if (Compiler.Settings.WriteMetadataOnly)
				{
					block = null;
				}
			}
			if ((base.ModFlags & Modifiers.STATIC) == 0)
			{
				return true;
			}
			if (parameters.HasExtensionMethodType)
			{
				if (Parent.PartialContainer.IsStatic && !Parent.IsGenericOrParentIsGeneric)
				{
					if (!Parent.IsTopLevel)
					{
						base.Report.Error(1109, base.Location, "`{0}': Extension methods cannot be defined in a nested class", GetSignatureForError());
					}
					if (!Module.PredefinedAttributes.Extension.IsDefined)
					{
						base.Report.Error(1110, base.Location, "`{0}': Extension methods require `System.Runtime.CompilerServices.ExtensionAttribute' type to be available. Are you missing an assembly reference?", GetSignatureForError());
					}
					base.ModFlags |= Modifiers.METHOD_EXTENSION;
					Parent.PartialContainer.ModFlags |= Modifiers.METHOD_EXTENSION;
					base.Spec.DeclaringType.SetExtensionMethodContainer();
					Parent.Module.HasExtensionMethod = true;
				}
				else
				{
					base.Report.Error(1106, base.Location, "`{0}': Extension methods must be defined in a non-generic static class", GetSignatureForError());
				}
			}
			CompilerSettings settings = Compiler.Settings;
			if (settings.NeedsEntryPoint && base.MemberName.Name == "Main" && !base.IsPartialDefinition && (settings.MainClass == null || settings.MainClass == Parent.TypeBuilder.FullName))
			{
				if (IsEntryPoint())
				{
					if (Parent.DeclaringAssembly.EntryPoint == null)
					{
						if (Parent.IsGenericOrParentIsGeneric || base.MemberName.IsGeneric)
						{
							base.Report.Warning(402, 4, base.Location, "`{0}': an entry point cannot be generic or in a generic type", GetSignatureForError());
						}
						else if ((base.ModFlags & Modifiers.ASYNC) != 0)
						{
							base.Report.Error(4009, base.Location, "`{0}': an entry point cannot be async method", GetSignatureForError());
						}
						else
						{
							SetIsUsed();
							Parent.DeclaringAssembly.EntryPoint = this;
						}
					}
					else
					{
						Error_DuplicateEntryPoint(Parent.DeclaringAssembly.EntryPoint);
						Error_DuplicateEntryPoint(this);
					}
				}
				else
				{
					base.Report.Warning(28, 4, base.Location, "`{0}' has the wrong signature to be an entry point", GetSignatureForError());
				}
			}
			return true;
		}

		public override void PrepareEmit()
		{
			if (base.IsPartialDefinition)
			{
				if (partialMethodImplementation != null)
				{
					MethodData = partialMethodImplementation.MethodData;
				}
			}
			else
			{
				base.PrepareEmit();
			}
		}

		public override void Emit()
		{
			try
			{
				if (base.IsPartialDefinition)
				{
					if (partialMethodImplementation != null && CurrentTypeParameters != null)
					{
						CurrentTypeParameters.CheckPartialConstraints(partialMethodImplementation);
						TypeParameters currentTypeParameters = partialMethodImplementation.CurrentTypeParameters;
						for (int i = 0; i < CurrentTypeParameters.Count; i++)
						{
							CurrentTypeParameters[i].Define(currentTypeParameters[i]);
						}
					}
					return;
				}
				if ((base.ModFlags & Modifiers.PARTIAL) != 0 && (caching_flags & Flags.PartialDefinitionExists) == 0)
				{
					base.Report.Error(759, base.Location, "A partial method `{0}' implementation is missing a partial method declaration", GetSignatureForError());
				}
				if (CurrentTypeParameters != null)
				{
					for (int j = 0; j < CurrentTypeParameters.Count; j++)
					{
						TypeParameter typeParameter = CurrentTypeParameters[j];
						typeParameter.CheckGenericConstraints(false);
						typeParameter.Emit();
					}
				}
				if ((base.ModFlags & Modifiers.METHOD_EXTENSION) != 0)
				{
					Module.PredefinedAttributes.Extension.EmitAttribute(base.MethodBuilder);
				}
				base.Emit();
			}
			catch (Exception e)
			{
				throw new InternalErrorException(this, e);
			}
		}

		public override bool EnableOverloadChecks(MemberCore overload)
		{
			if (overload is Indexer)
			{
				return false;
			}
			return base.EnableOverloadChecks(overload);
		}

		public static void Error1599(Location loc, TypeSpec t, Report Report)
		{
			Report.Error(1599, loc, "Method or delegate cannot return type `{0}'", t.GetSignatureForError());
		}

		protected override bool ResolveMemberType()
		{
			if (CurrentTypeParameters != null)
			{
				CreateTypeParameters();
			}
			return base.ResolveMemberType();
		}

		public void SetPartialDefinition(Method methodDefinition)
		{
			caching_flags |= Flags.PartialDefinitionExists;
			methodDefinition.partialMethodImplementation = this;
			for (int i = 0; i < methodDefinition.parameters.Count; i++)
			{
				Parameter parameter = methodDefinition.parameters[i];
				Parameter parameter2 = parameters[i];
				parameter2.Name = parameter.Name;
				parameter2.DefaultValue = parameter.DefaultValue;
				if (parameter.OptAttributes != null)
				{
					if (parameter2.OptAttributes == null)
					{
						parameter2.OptAttributes = parameter.OptAttributes;
					}
					else
					{
						parameter2.OptAttributes.Attrs.AddRange(parameter.OptAttributes.Attrs);
					}
				}
			}
			if (methodDefinition.attributes != null)
			{
				if (attributes == null)
				{
					attributes = methodDefinition.attributes;
				}
				else
				{
					attributes.Attrs.AddRange(methodDefinition.attributes.Attrs);
				}
			}
			if (CurrentTypeParameters == null)
			{
				return;
			}
			for (int j = 0; j < CurrentTypeParameters.Count; j++)
			{
				TypeParameter typeParameter = methodDefinition.CurrentTypeParameters[j];
				if (typeParameter.OptAttributes != null)
				{
					TypeParameter typeParameter2 = CurrentTypeParameters[j];
					if (typeParameter2.OptAttributes == null)
					{
						typeParameter2.OptAttributes = typeParameter.OptAttributes;
					}
					else
					{
						typeParameter2.OptAttributes.Attrs.AddRange(typeParameter2.OptAttributes.Attrs);
					}
				}
			}
		}
	}
}
