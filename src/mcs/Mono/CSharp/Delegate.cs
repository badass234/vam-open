using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace Mono.CSharp
{
	public class Delegate : TypeDefinition, IParametersMember, IInterfaceMemberSpec
	{
		private FullNamedExpression ReturnType;

		private readonly ParametersCompiled parameters;

		private Constructor Constructor;

		private Method InvokeBuilder;

		private Method BeginInvokeBuilder;

		private Method EndInvokeBuilder;

		private static readonly string[] attribute_targets = new string[2] { "type", "return" };

		public static readonly string InvokeMethodName = "Invoke";

		private Expression instance_expr;

		private ReturnParameter return_attributes;

		private Dictionary<SecurityAction, PermissionSet> declarative_security;

		private const Modifiers MethodModifiers = Modifiers.PUBLIC | Modifiers.VIRTUAL;

		private const Modifiers AllowedModifiers = Modifiers.AccessibilityMask | Modifiers.NEW | Modifiers.UNSAFE;

		public TypeSpec MemberType => ReturnType.Type;

		public AParametersCollection Parameters => parameters;

		public FullNamedExpression TypExpression => ReturnType;

		public override AttributeTargets AttributeTargets => AttributeTargets.Delegate;

		protected override TypeAttributes TypeAttr => base.TypeAttr | TypeAttributes.NotPublic | TypeAttributes.Sealed;

		public override string[] ValidAttributeTargets => attribute_targets;

		public Expression InstanceExpression
		{
			get
			{
				return instance_expr;
			}
			set
			{
				instance_expr = value;
			}
		}

		public Delegate(TypeContainer parent, FullNamedExpression type, Modifiers mod_flags, MemberName name, ParametersCompiled param_list, Attributes attrs)
			: base(parent, name, attrs, MemberKind.Delegate)
		{
			ReturnType = type;
			base.ModFlags = ModifiersExtensions.Check(Modifiers.AccessibilityMask | Modifiers.NEW | Modifiers.UNSAFE, mod_flags, base.IsTopLevel ? Modifiers.INTERNAL : Modifiers.PRIVATE, name.Location, base.Report);
			parameters = param_list;
			spec = new TypeSpec(Kind, null, this, null, base.ModFlags | Modifiers.SEALED);
		}

		public override void Accept(StructuralVisitor visitor)
		{
			visitor.Visit(this);
		}

		public override void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.Target == AttributeTargets.ReturnValue)
			{
				if (return_attributes == null)
				{
					return_attributes = new ReturnParameter(this, InvokeBuilder.MethodBuilder, base.Location);
				}
				return_attributes.ApplyAttributeBuilder(a, ctor, cdata, pa);
			}
			else if (a.IsValidSecurityAttribute())
			{
				a.ExtractSecurityPermissionSet(ctor, ref declarative_security);
			}
			else
			{
				base.ApplyAttributeBuilder(a, ctor, cdata, pa);
			}
		}

		protected override bool DoDefineMembers()
		{
			BuiltinTypes builtinTypes = Compiler.BuiltinTypes;
			ParametersCompiled args = ParametersCompiled.CreateFullyResolved(new Parameter[2]
			{
				new Parameter(new TypeExpression(builtinTypes.Object, base.Location), "object", Parameter.Modifier.NONE, null, base.Location),
				new Parameter(new TypeExpression(builtinTypes.IntPtr, base.Location), "method", Parameter.Modifier.NONE, null, base.Location)
			}, new BuiltinTypeSpec[2] { builtinTypes.Object, builtinTypes.IntPtr });
			Constructor = new Constructor(this, Constructor.ConstructorName, Modifiers.PUBLIC, null, args, base.Location);
			Constructor.Define();
			ParametersCompiled parametersCompiled = parameters;
			if (!parametersCompiled.Resolve(this))
			{
				return false;
			}
			TypeSpec[] types = parametersCompiled.Types;
			foreach (TypeSpec typeSpec in types)
			{
				if (!IsAccessibleAs(typeSpec))
				{
					base.Report.SymbolRelatedToPreviousError(typeSpec);
					base.Report.Error(59, base.Location, "Inconsistent accessibility: parameter type `{0}' is less accessible than delegate `{1}'", typeSpec.GetSignatureForError(), GetSignatureForError());
				}
			}
			TypeSpec typeSpec2 = ReturnType.ResolveAsType(this);
			if (typeSpec2 == null)
			{
				return false;
			}
			if (!IsAccessibleAs(typeSpec2))
			{
				base.Report.SymbolRelatedToPreviousError(typeSpec2);
				base.Report.Error(58, base.Location, "Inconsistent accessibility: return type `" + typeSpec2.GetSignatureForError() + "' is less accessible than delegate `" + GetSignatureForError() + "'");
				return false;
			}
			CheckProtectedModifier();
			if (Compiler.Settings.StdLib && typeSpec2.IsSpecialRuntimeType)
			{
				Method.Error1599(base.Location, typeSpec2, base.Report);
				return false;
			}
			VarianceDecl.CheckTypeVariance(typeSpec2, Variance.Covariant, this);
			TypeExpression typeExpression = new TypeExpression(typeSpec2, base.Location);
			InvokeBuilder = new Method(this, typeExpression, Modifiers.PUBLIC | Modifiers.VIRTUAL, new MemberName(InvokeMethodName), parametersCompiled, null);
			InvokeBuilder.Define();
			if (!base.IsCompilerGenerated)
			{
				DefineAsyncMethods(typeExpression);
			}
			return true;
		}

		private void DefineAsyncMethods(TypeExpression returnType)
		{
			PredefinedType iAsyncResult = Module.PredefinedTypes.IAsyncResult;
			PredefinedType asyncCallback = Module.PredefinedTypes.AsyncCallback;
			if (!iAsyncResult.Define() || !asyncCallback.Define())
			{
				return;
			}
			ParametersCompiled userParams;
			if (Parameters.Count == 0)
			{
				userParams = ParametersCompiled.EmptyReadOnlyParameters;
			}
			else
			{
				Parameter[] array = new Parameter[Parameters.Count];
				for (int i = 0; i < array.Length; i++)
				{
					Parameter parameter = parameters[i];
					array[i] = new Parameter(new TypeExpression(parameters.Types[i], base.Location), parameter.Name, parameter.ModFlags & Parameter.Modifier.RefOutMask, (parameter.OptAttributes == null) ? null : parameter.OptAttributes.Clone(), base.Location);
				}
				userParams = new ParametersCompiled(array);
			}
			userParams = ParametersCompiled.MergeGenerated(Compiler, userParams, false, new Parameter[2]
			{
				new Parameter(new TypeExpression(asyncCallback.TypeSpec, base.Location), "callback", Parameter.Modifier.NONE, null, base.Location),
				new Parameter(new TypeExpression(Compiler.BuiltinTypes.Object, base.Location), "object", Parameter.Modifier.NONE, null, base.Location)
			}, new TypeSpec[2]
			{
				asyncCallback.TypeSpec,
				Compiler.BuiltinTypes.Object
			});
			BeginInvokeBuilder = new Method(this, new TypeExpression(iAsyncResult.TypeSpec, base.Location), Modifiers.PUBLIC | Modifiers.VIRTUAL, new MemberName("BeginInvoke"), userParams, null);
			BeginInvokeBuilder.Define();
			int num = 0;
			IParameterData[] fixedParameters = Parameters.FixedParameters;
			for (int j = 0; j < fixedParameters.Length; j++)
			{
				if ((((Parameter)fixedParameters[j]).ModFlags & Parameter.Modifier.RefOutMask) != Parameter.Modifier.NONE)
				{
					num++;
				}
			}
			ParametersCompiled userParams2;
			if (num > 0)
			{
				Parameter[] array2 = new Parameter[num];
				int num2 = 0;
				for (int k = 0; k < Parameters.FixedParameters.Length; k++)
				{
					Parameter parameter2 = parameters[k];
					if ((parameter2.ModFlags & Parameter.Modifier.RefOutMask) != Parameter.Modifier.NONE)
					{
						array2[num2++] = new Parameter(new TypeExpression(parameter2.Type, base.Location), parameter2.Name, parameter2.ModFlags & Parameter.Modifier.RefOutMask, (parameter2.OptAttributes == null) ? null : parameter2.OptAttributes.Clone(), base.Location);
					}
				}
				userParams2 = new ParametersCompiled(array2);
			}
			else
			{
				userParams2 = ParametersCompiled.EmptyReadOnlyParameters;
			}
			userParams2 = ParametersCompiled.MergeGenerated(Compiler, userParams2, false, new Parameter(new TypeExpression(iAsyncResult.TypeSpec, base.Location), "result", Parameter.Modifier.NONE, null, base.Location), iAsyncResult.TypeSpec);
			EndInvokeBuilder = new Method(this, returnType, Modifiers.PUBLIC | Modifiers.VIRTUAL, new MemberName("EndInvoke"), userParams2, null);
			EndInvokeBuilder.Define();
		}

		public override void PrepareEmit()
		{
			if ((caching_flags & Flags.CloseTypeCreated) == 0)
			{
				InvokeBuilder.PrepareEmit();
				if (BeginInvokeBuilder != null)
				{
					BeginInvokeBuilder.PrepareEmit();
					EndInvokeBuilder.PrepareEmit();
				}
			}
		}

		public override void Emit()
		{
			base.Emit();
			if (declarative_security != null)
			{
				foreach (KeyValuePair<SecurityAction, PermissionSet> item in declarative_security)
				{
					TypeBuilder.AddDeclarativeSecurity(item.Key, item.Value);
				}
			}
			if (ReturnType.Type != null)
			{
				if (ReturnType.Type.BuiltinType == BuiltinTypeSpec.Type.Dynamic)
				{
					return_attributes = new ReturnParameter(this, InvokeBuilder.MethodBuilder, base.Location);
					Module.PredefinedAttributes.Dynamic.EmitAttribute(return_attributes.Builder);
				}
				else if (ReturnType.Type.HasDynamicElement)
				{
					return_attributes = new ReturnParameter(this, InvokeBuilder.MethodBuilder, base.Location);
					Module.PredefinedAttributes.Dynamic.EmitAttribute(return_attributes.Builder, ReturnType.Type, base.Location);
				}
				ConstraintChecker.Check(this, ReturnType.Type, ReturnType.Location);
			}
			Constructor.ParameterInfo.ApplyAttributes(this, Constructor.ConstructorBuilder);
			Constructor.ConstructorBuilder.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
			parameters.CheckConstraints(this);
			parameters.ApplyAttributes(this, InvokeBuilder.MethodBuilder);
			InvokeBuilder.MethodBuilder.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
			if (BeginInvokeBuilder != null)
			{
				BeginInvokeBuilder.ParameterInfo.ApplyAttributes(this, BeginInvokeBuilder.MethodBuilder);
				EndInvokeBuilder.ParameterInfo.ApplyAttributes(this, EndInvokeBuilder.MethodBuilder);
				BeginInvokeBuilder.MethodBuilder.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
				EndInvokeBuilder.MethodBuilder.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
			}
		}

		protected override TypeSpec[] ResolveBaseTypes(out FullNamedExpression base_class)
		{
			base_type = Compiler.BuiltinTypes.MulticastDelegate;
			base_class = null;
			return null;
		}

		protected override bool VerifyClsCompliance()
		{
			if (!base.VerifyClsCompliance())
			{
				return false;
			}
			parameters.VerifyClsCompliance(this);
			if (!InvokeBuilder.MemberType.IsCLSCompliant())
			{
				base.Report.Warning(3002, 1, base.Location, "Return type of `{0}' is not CLS-compliant", GetSignatureForError());
			}
			return true;
		}

		public static MethodSpec GetConstructor(TypeSpec delType)
		{
			return (MethodSpec)MemberCache.FindMember(delType, MemberFilter.Constructor(null), BindingRestriction.DeclaredOnly);
		}

		public static MethodSpec GetInvokeMethod(TypeSpec delType)
		{
			return (MethodSpec)MemberCache.FindMember(delType, MemberFilter.Method(InvokeMethodName, 0, null, null), BindingRestriction.DeclaredOnly);
		}

		public static AParametersCollection GetParameters(TypeSpec delType)
		{
			return GetInvokeMethod(delType).Parameters;
		}

		public static bool IsTypeCovariant(ResolveContext rc, TypeSpec a, TypeSpec b)
		{
			if (a == b)
			{
				return true;
			}
			if (rc.Module.Compiler.Settings.Version == LanguageVersion.ISO_1)
			{
				return false;
			}
			if (a.IsGenericParameter && b.IsGenericParameter)
			{
				return a == b;
			}
			return Convert.ImplicitReferenceConversionExists(a, b);
		}

		public static string FullDelegateDesc(MethodSpec invoke_method)
		{
			return TypeManager.GetFullNameSignature(invoke_method).Replace(".Invoke", "");
		}
	}
}
