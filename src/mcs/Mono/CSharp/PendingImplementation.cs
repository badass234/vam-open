using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp
{
	public class PendingImplementation
	{
		private struct MissingInterfacesInfo
		{
			public TypeSpec Type;

			public bool Optional;

			public MissingInterfacesInfo(TypeSpec t)
			{
				Type = t;
				Optional = false;
			}
		}

		public enum Operation
		{
			Lookup = 0,
			ClearOne = 1,
			ClearAll = 2
		}

		private readonly TypeDefinition container;

		private TypeAndMethods[] pending_implementations;

		private static readonly MissingInterfacesInfo[] EmptyMissingInterfacesInfo = new MissingInterfacesInfo[0];

		private Report Report => container.Module.Compiler.Report;

		private PendingImplementation(TypeDefinition container, MissingInterfacesInfo[] missing_ifaces, MethodSpec[] abstract_methods, int total)
		{
			TypeSpec definition = container.Definition;
			this.container = container;
			pending_implementations = new TypeAndMethods[total];
			int num = 0;
			if (abstract_methods != null)
			{
				int num2 = abstract_methods.Length;
				pending_implementations[num].methods = new MethodSpec[num2];
				pending_implementations[num].need_proxy = new MethodSpec[num2];
				pending_implementations[num].methods = abstract_methods;
				pending_implementations[num].found = new MethodData[num2];
				pending_implementations[num].type = definition;
				num++;
			}
			for (int i = 0; i < missing_ifaces.Length; i++)
			{
				MissingInterfacesInfo missingInterfacesInfo = missing_ifaces[i];
				TypeSpec type = missingInterfacesInfo.Type;
				List<MethodSpec> interfaceMethods = MemberCache.GetInterfaceMethods(type);
				int count = interfaceMethods.Count;
				pending_implementations[num].type = type;
				pending_implementations[num].optional = missingInterfacesInfo.Optional;
				pending_implementations[num].methods = interfaceMethods;
				pending_implementations[num].found = new MethodData[count];
				pending_implementations[num].need_proxy = new MethodSpec[count];
				num++;
			}
		}

		private static MissingInterfacesInfo[] GetMissingInterfaces(TypeDefinition container)
		{
			IList<TypeSpec> interfaces = container.Definition.Interfaces;
			if (interfaces == null || interfaces.Count == 0)
			{
				return EmptyMissingInterfacesInfo;
			}
			MissingInterfacesInfo[] array = new MissingInterfacesInfo[interfaces.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new MissingInterfacesInfo(interfaces[i]);
			}
			if (container.BaseType == null)
			{
				return array;
			}
			IList<TypeSpec> interfaces2 = container.BaseType.Interfaces;
			if (interfaces2 != null)
			{
				foreach (TypeSpec item in interfaces2)
				{
					for (int j = 0; j < array.Length; j++)
					{
						if (item == array[j].Type)
						{
							array[j].Optional = true;
							break;
						}
					}
				}
			}
			return array;
		}

		public static PendingImplementation GetPendingImplementations(TypeDefinition container)
		{
			TypeSpec baseType = container.BaseType;
			MissingInterfacesInfo[] missingInterfaces = GetMissingInterfaces(container);
			bool flag = baseType != null && baseType.IsAbstract && (container.ModFlags & Modifiers.ABSTRACT) == 0;
			MethodSpec[] array = null;
			if (flag)
			{
				IList<MethodSpec> notImplementedAbstractMethods = MemberCache.GetNotImplementedAbstractMethods(baseType);
				if (notImplementedAbstractMethods == null)
				{
					flag = false;
				}
				else
				{
					array = new MethodSpec[notImplementedAbstractMethods.Count];
					notImplementedAbstractMethods.CopyTo(array, 0);
				}
			}
			int num = missingInterfaces.Length + (flag ? 1 : 0);
			if (num == 0)
			{
				return null;
			}
			PendingImplementation pendingImplementation = new PendingImplementation(container, missingInterfaces, array, num);
			TypeAndMethods[] array2 = pendingImplementation.pending_implementations;
			for (int i = 0; i < array2.Length; i++)
			{
				TypeAndMethods typeAndMethods = array2[i];
				if (!typeAndMethods.type.IsGeneric)
				{
					continue;
				}
				for (int j = 0; j < typeAndMethods.methods.Count; j++)
				{
					MethodSpec methodSpec = typeAndMethods.methods[j];
					if (methodSpec.Parameters.IsEmpty)
					{
						continue;
					}
					for (int k = j + 1; k < typeAndMethods.methods.Count; k++)
					{
						MethodSpec methodSpec2 = typeAndMethods.methods[k];
						if (methodSpec.Name != methodSpec2.Name || typeAndMethods.type != methodSpec2.DeclaringType || !TypeSpecComparer.Override.IsSame(methodSpec.Parameters.Types, methodSpec2.Parameters.Types))
						{
							continue;
						}
						bool flag2 = true;
						bool flag3 = false;
						IParameterData[] fixedParameters = methodSpec.Parameters.FixedParameters;
						IParameterData[] fixedParameters2 = methodSpec2.Parameters.FixedParameters;
						for (int l = 0; l < fixedParameters.Length; l++)
						{
							if ((fixedParameters[l].ModFlags & Parameter.Modifier.RefOutMask) != (fixedParameters2[l].ModFlags & Parameter.Modifier.RefOutMask))
							{
								if (((fixedParameters[l].ModFlags | fixedParameters2[l].ModFlags) & Parameter.Modifier.RefOutMask) != Parameter.Modifier.RefOutMask)
								{
									flag2 = false;
									break;
								}
								flag3 = true;
							}
						}
						if (flag2 && flag3)
						{
							pendingImplementation.Report.SymbolRelatedToPreviousError(methodSpec);
							pendingImplementation.Report.SymbolRelatedToPreviousError(methodSpec2);
							pendingImplementation.Report.Error(767, container.Location, "Cannot implement interface `{0}' with the specified type parameters because it causes method `{1}' to differ on parameter modifiers only", typeAndMethods.type.GetDefinition().GetSignatureForError(), methodSpec.GetSignatureForError());
							break;
						}
					}
				}
			}
			return pendingImplementation;
		}

		public MethodSpec IsInterfaceMethod(MemberName name, TypeSpec ifaceType, MethodData method, out MethodSpec ambiguousCandidate, ref bool optional)
		{
			return InterfaceMethod(name, ifaceType, method, Operation.Lookup, out ambiguousCandidate, ref optional);
		}

		public void ImplementMethod(MemberName name, TypeSpec ifaceType, MethodData method, bool clear_one, out MethodSpec ambiguousCandidate, ref bool optional)
		{
			InterfaceMethod(name, ifaceType, method, clear_one ? Operation.ClearOne : Operation.ClearAll, out ambiguousCandidate, ref optional);
		}

		public MethodSpec InterfaceMethod(MemberName name, TypeSpec iType, MethodData method, Operation op, out MethodSpec ambiguousCandidate, ref bool optional)
		{
			ambiguousCandidate = null;
			if (pending_implementations == null)
			{
				return null;
			}
			TypeSpec returnType = method.method.ReturnType;
			ParametersCompiled parameterInfo = method.method.ParameterInfo;
			bool flag = method.method is Indexer.SetIndexerMethod || method.method is Indexer.GetIndexerMethod;
			TypeAndMethods[] array = pending_implementations;
			MethodSpec methodSpec;
			for (int i = 0; i < array.Length; i++)
			{
				TypeAndMethods typeAndMethods = array[i];
				if (iType != null && typeAndMethods.type != iType)
				{
					continue;
				}
				int count = typeAndMethods.methods.Count;
				for (int j = 0; j < count; j++)
				{
					methodSpec = typeAndMethods.methods[j];
					if (methodSpec == null)
					{
						continue;
					}
					if (flag)
					{
						if (!methodSpec.IsAccessor || methodSpec.Parameters.IsEmpty)
						{
							continue;
						}
					}
					else if (name.Name != methodSpec.Name || methodSpec.Arity != name.Arity)
					{
						continue;
					}
					if (!TypeSpecComparer.Override.IsEqual(methodSpec.Parameters, parameterInfo))
					{
						continue;
					}
					if (!TypeSpecComparer.Override.IsEqual(methodSpec.ReturnType, returnType))
					{
						typeAndMethods.found[j] = method;
						continue;
					}
					if (op != Operation.Lookup)
					{
						if (methodSpec.IsAccessor != method.method.IsAccessor)
						{
							continue;
						}
						if (methodSpec.DeclaringType.IsInterface && iType == null && name.Name != methodSpec.Name)
						{
							typeAndMethods.need_proxy[j] = method.method.Spec;
						}
						else
						{
							typeAndMethods.methods[j] = null;
						}
					}
					else
					{
						typeAndMethods.found[j] = method;
						optional = typeAndMethods.optional;
					}
					if (op == Operation.Lookup && name.ExplicitInterface != null && ambiguousCandidate == null)
					{
						ambiguousCandidate = methodSpec;
					}
					else if (op != Operation.ClearAll)
					{
						return methodSpec;
					}
				}
				if (typeAndMethods.type == iType)
				{
					break;
				}
			}
			methodSpec = ambiguousCandidate;
			ambiguousCandidate = null;
			return methodSpec;
		}

		private void DefineProxy(TypeSpec iface, MethodSpec base_method, MethodSpec iface_method)
		{
			string text = iface.MemberDefinition.Namespace;
			string name = ((!string.IsNullOrEmpty(text)) ? (text + "." + iface.MemberDefinition.Name + "." + iface_method.Name) : (iface.MemberDefinition.Name + "." + iface_method.Name));
			AParametersCollection parameters = iface_method.Parameters;
			MethodBuilder methodBuilder = container.TypeBuilder.DefineMethod(name, MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask | MethodAttributes.CheckAccessOnOverride, CallingConventions.Standard | CallingConventions.HasThis, base_method.ReturnType.GetMetaInfo(), parameters.GetMetaInfo());
			if (iface_method.IsGeneric)
			{
				string[] names = iface_method.GenericDefinition.TypeParameters.Select((TypeParameterSpec l) => l.Name).ToArray();
				methodBuilder.DefineGenericParameters(names);
			}
			for (int num = 0; num < parameters.Count; num++)
			{
				string name2 = parameters.FixedParameters[num].Name;
				ParameterAttributes parameterAttribute = AParametersCollection.GetParameterAttribute(parameters.FixedParameters[num].ModFlags);
				methodBuilder.DefineParameter(num + 1, parameterAttribute, name2);
			}
			int count = parameters.Count;
			EmitContext emitContext = new EmitContext(new ProxyMethodContext(container), methodBuilder.GetILGenerator(), null, null);
			emitContext.EmitThis();
			for (int num2 = 0; num2 < count; num2++)
			{
				emitContext.EmitArgumentLoad(num2);
			}
			emitContext.Emit(OpCodes.Call, base_method);
			emitContext.Emit(OpCodes.Ret);
			container.TypeBuilder.DefineMethodOverride(methodBuilder, (MethodInfo)iface_method.GetMetaInfo());
		}

		private bool BaseImplements(TypeSpec iface_type, MethodSpec mi, out MethodSpec base_method)
		{
			base_method = null;
			TypeSpec baseType = container.BaseType;
			AParametersCollection parameters = mi.Parameters;
			MethodSpec methodSpec = null;
			while (true)
			{
				IList<MemberSpec> list = MemberCache.FindMembers(baseType, mi.Name, false);
				if (list == null)
				{
					base_method = methodSpec;
					return false;
				}
				MethodSpec methodSpec2 = null;
				foreach (MemberSpec item in list)
				{
					if (item.Kind != MemberKind.Method || item.Arity != mi.Arity)
					{
						continue;
					}
					AParametersCollection parameters2 = ((MethodSpec)item).Parameters;
					if (!TypeSpecComparer.Override.IsEqual(parameters.Types, parameters2.Types))
					{
						continue;
					}
					bool flag = true;
					for (int i = 0; i < parameters.Count; i++)
					{
						if ((parameters.FixedParameters[i].ModFlags & Parameter.Modifier.RefOutMask) == (parameters2.FixedParameters[i].ModFlags & Parameter.Modifier.RefOutMask))
						{
							continue;
						}
						flag = false;
						if ((parameters.FixedParameters[i].ModFlags & Parameter.Modifier.RefOutMask) != (parameters2.FixedParameters[i].ModFlags & Parameter.Modifier.RefOutMask))
						{
							if (methodSpec2 == null)
							{
								if (!item.IsPublic || !TypeSpecComparer.Override.IsEqual(mi.ReturnType, ((MethodSpec)item).ReturnType))
								{
									break;
								}
								methodSpec2 = (MethodSpec)item;
							}
							continue;
						}
						methodSpec2 = null;
						break;
					}
					if (!flag)
					{
						continue;
					}
					MethodSpec methodSpec3 = (MethodSpec)item;
					if (!methodSpec3.IsPublic)
					{
						if (methodSpec == null)
						{
							methodSpec = methodSpec3;
						}
						continue;
					}
					if (!TypeSpecComparer.Override.IsEqual(mi.ReturnType, methodSpec3.ReturnType))
					{
						if (methodSpec == null)
						{
							methodSpec = methodSpec3;
						}
						continue;
					}
					base_method = methodSpec3;
					if (!mi.IsGeneric || Method.CheckImplementingMethodConstraints(container, methodSpec3, mi))
					{
						continue;
					}
					return true;
				}
				if (base_method != null)
				{
					if (methodSpec2 != null)
					{
						Report.SymbolRelatedToPreviousError(methodSpec2);
						Report.SymbolRelatedToPreviousError(mi);
						Report.SymbolRelatedToPreviousError(container);
						Report.Warning(1956, 1, ((MemberCore)base_method.MemberDefinition).Location, "The interface method `{0}' implementation is ambiguous between following methods: `{1}' and `{2}' in type `{3}'", mi.GetSignatureForError(), base_method.GetSignatureForError(), methodSpec2.GetSignatureForError(), container.GetSignatureForError());
					}
					break;
				}
				baseType = list[0].DeclaringType.BaseType;
				if (baseType == null)
				{
					base_method = methodSpec;
					return false;
				}
			}
			if (!base_method.IsVirtual)
			{
				DefineProxy(iface_type, base_method, mi);
			}
			return true;
		}

		public bool VerifyPendingMethods()
		{
			int num = pending_implementations.Length;
			bool result = false;
			for (int i = 0; i < num; i++)
			{
				TypeSpec type = pending_implementations[i].type;
				bool flag = type.IsInterface && container.BaseType != null && container.BaseType.ImplementsInterface(type, false);
				for (int j = 0; j < pending_implementations[i].methods.Count; j++)
				{
					MethodSpec methodSpec = pending_implementations[i].methods[j];
					if (methodSpec == null)
					{
						continue;
					}
					if (type.IsInterface)
					{
						MethodSpec methodSpec2 = pending_implementations[i].need_proxy[j];
						if (methodSpec2 != null)
						{
							DefineProxy(type, methodSpec2, methodSpec);
							continue;
						}
						MethodSpec base_method;
						if (pending_implementations[i].optional || flag || BaseImplements(type, methodSpec, out base_method))
						{
							continue;
						}
						if (base_method == null)
						{
							MethodData methodData = pending_implementations[i].found[j];
							if (methodData != null)
							{
								base_method = methodData.method.Spec;
							}
						}
						Report.SymbolRelatedToPreviousError(methodSpec);
						if (base_method != null)
						{
							Report.SymbolRelatedToPreviousError(base_method);
							if (base_method.IsStatic)
							{
								Report.Error(736, container.Location, "`{0}' does not implement interface member `{1}' and the best implementing candidate `{2}' is static", container.GetSignatureForError(), methodSpec.GetSignatureForError(), base_method.GetSignatureForError());
							}
							else if ((base_method.Modifiers & Modifiers.PUBLIC) == 0)
							{
								Report.Error(737, container.Location, "`{0}' does not implement interface member `{1}' and the best implementing candidate `{2}' is not public", container.GetSignatureForError(), methodSpec.GetSignatureForError(), base_method.GetSignatureForError());
							}
							else
							{
								Report.Error(738, container.Location, "`{0}' does not implement interface member `{1}' and the best implementing candidate `{2}' return type `{3}' does not match interface member return type `{4}'", container.GetSignatureForError(), methodSpec.GetSignatureForError(), base_method.GetSignatureForError(), base_method.ReturnType.GetSignatureForError(), methodSpec.ReturnType.GetSignatureForError());
							}
						}
						else
						{
							Report.Error(535, container.Location, "`{0}' does not implement interface member `{1}'", container.GetSignatureForError(), methodSpec.GetSignatureForError());
						}
					}
					else
					{
						Report.SymbolRelatedToPreviousError(methodSpec);
						Report.Error(534, container.Location, "`{0}' does not implement inherited abstract member `{1}'", container.GetSignatureForError(), methodSpec.GetSignatureForError());
					}
					result = true;
				}
			}
			return result;
		}
	}
}
