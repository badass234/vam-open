namespace Mono.CSharp
{
	public class PredefinedMembers
	{
		public readonly PredefinedMember<MethodSpec> ActivatorCreateInstance;

		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderCreate;

		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderStart;

		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderSetResult;

		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderSetException;

		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderSetStateMachine;

		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderOnCompleted;

		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderOnCompletedUnsafe;

		public readonly PredefinedMember<PropertySpec> AsyncTaskMethodBuilderTask;

		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderGenericCreate;

		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderGenericStart;

		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderGenericSetResult;

		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderGenericSetException;

		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderGenericSetStateMachine;

		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderGenericOnCompleted;

		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderGenericOnCompletedUnsafe;

		public readonly PredefinedMember<PropertySpec> AsyncTaskMethodBuilderGenericTask;

		public readonly PredefinedMember<MethodSpec> AsyncVoidMethodBuilderCreate;

		public readonly PredefinedMember<MethodSpec> AsyncVoidMethodBuilderStart;

		public readonly PredefinedMember<MethodSpec> AsyncVoidMethodBuilderSetException;

		public readonly PredefinedMember<MethodSpec> AsyncVoidMethodBuilderSetResult;

		public readonly PredefinedMember<MethodSpec> AsyncVoidMethodBuilderSetStateMachine;

		public readonly PredefinedMember<MethodSpec> AsyncVoidMethodBuilderOnCompleted;

		public readonly PredefinedMember<MethodSpec> AsyncVoidMethodBuilderOnCompletedUnsafe;

		public readonly PredefinedMember<MethodSpec> AsyncStateMachineAttributeCtor;

		public readonly PredefinedMember<MethodSpec> DebuggerBrowsableAttributeCtor;

		public readonly PredefinedMember<MethodSpec> DecimalCtor;

		public readonly PredefinedMember<MethodSpec> DecimalCtorInt;

		public readonly PredefinedMember<MethodSpec> DecimalCtorLong;

		public readonly PredefinedMember<MethodSpec> DecimalConstantAttributeCtor;

		public readonly PredefinedMember<MethodSpec> DefaultMemberAttributeCtor;

		public readonly PredefinedMember<MethodSpec> DelegateCombine;

		public readonly PredefinedMember<MethodSpec> DelegateEqual;

		public readonly PredefinedMember<MethodSpec> DelegateInequal;

		public readonly PredefinedMember<MethodSpec> DelegateRemove;

		public readonly PredefinedMember<MethodSpec> DynamicAttributeCtor;

		public readonly PredefinedMember<MethodSpec> FieldInfoGetFieldFromHandle;

		public readonly PredefinedMember<MethodSpec> FieldInfoGetFieldFromHandle2;

		public readonly PredefinedMember<MethodSpec> IDisposableDispose;

		public readonly PredefinedMember<MethodSpec> IEnumerableGetEnumerator;

		public readonly PredefinedMember<MethodSpec> InterlockedCompareExchange;

		public readonly PredefinedMember<MethodSpec> InterlockedCompareExchange_T;

		public readonly PredefinedMember<MethodSpec> FixedBufferAttributeCtor;

		public readonly PredefinedMember<MethodSpec> MethodInfoGetMethodFromHandle;

		public readonly PredefinedMember<MethodSpec> MethodInfoGetMethodFromHandle2;

		public readonly PredefinedMember<MethodSpec> MonitorEnter;

		public readonly PredefinedMember<MethodSpec> MonitorEnter_v4;

		public readonly PredefinedMember<MethodSpec> MonitorExit;

		public readonly PredefinedMember<PropertySpec> RuntimeCompatibilityWrapNonExceptionThrows;

		public readonly PredefinedMember<MethodSpec> RuntimeHelpersInitializeArray;

		public readonly PredefinedMember<PropertySpec> RuntimeHelpersOffsetToStringData;

		public readonly PredefinedMember<ConstSpec> SecurityActionRequestMinimum;

		public readonly PredefinedMember<FieldSpec> StringEmpty;

		public readonly PredefinedMember<MethodSpec> StringEqual;

		public readonly PredefinedMember<MethodSpec> StringInequal;

		public readonly PredefinedMember<MethodSpec> StructLayoutAttributeCtor;

		public readonly PredefinedMember<FieldSpec> StructLayoutCharSet;

		public readonly PredefinedMember<FieldSpec> StructLayoutSize;

		public readonly PredefinedMember<MethodSpec> TypeGetTypeFromHandle;

		public PredefinedMembers(ModuleContainer module)
		{
			PredefinedTypes types = module.PredefinedTypes;
			PredefinedAttributes predefinedAttributes = module.PredefinedAttributes;
			BuiltinTypes builtinTypes = module.Compiler.BuiltinTypes;
			TypeParameter definition = new TypeParameter(0, new MemberName("T"), null, null, Variance.None);
			ActivatorCreateInstance = new PredefinedMember<MethodSpec>(module, types.Activator, MemberFilter.Method("CreateInstance", 1, ParametersCompiled.EmptyReadOnlyParameters, null));
			AsyncTaskMethodBuilderCreate = new PredefinedMember<MethodSpec>(module, types.AsyncTaskMethodBuilder, MemberFilter.Method("Create", 0, ParametersCompiled.EmptyReadOnlyParameters, types.AsyncTaskMethodBuilder.TypeSpec));
			AsyncTaskMethodBuilderSetResult = new PredefinedMember<MethodSpec>(module, types.AsyncTaskMethodBuilder, MemberFilter.Method("SetResult", 0, ParametersCompiled.EmptyReadOnlyParameters, builtinTypes.Void));
			AsyncTaskMethodBuilderSetStateMachine = new PredefinedMember<MethodSpec>(module, types.AsyncTaskMethodBuilder, "SetStateMachine", MemberKind.Method, () => new TypeSpec[1] { types.IAsyncStateMachine.TypeSpec }, builtinTypes.Void);
			AsyncTaskMethodBuilderSetException = new PredefinedMember<MethodSpec>(module, types.AsyncTaskMethodBuilder, MemberFilter.Method("SetException", 0, ParametersCompiled.CreateFullyResolved(builtinTypes.Exception), builtinTypes.Void));
			AsyncTaskMethodBuilderOnCompleted = new PredefinedMember<MethodSpec>(module, types.AsyncTaskMethodBuilder, MemberFilter.Method("AwaitOnCompleted", 2, new ParametersImported(new ParameterData[2]
			{
				new ParameterData(null, Parameter.Modifier.REF),
				new ParameterData(null, Parameter.Modifier.REF)
			}, new TypeParameterSpec[2]
			{
				new TypeParameterSpec(0, definition, SpecialConstraint.None, Variance.None, null),
				new TypeParameterSpec(1, definition, SpecialConstraint.None, Variance.None, null)
			}, false), builtinTypes.Void));
			AsyncTaskMethodBuilderOnCompletedUnsafe = new PredefinedMember<MethodSpec>(module, types.AsyncTaskMethodBuilder, MemberFilter.Method("AwaitUnsafeOnCompleted", 2, new ParametersImported(new ParameterData[2]
			{
				new ParameterData(null, Parameter.Modifier.REF),
				new ParameterData(null, Parameter.Modifier.REF)
			}, new TypeParameterSpec[2]
			{
				new TypeParameterSpec(0, definition, SpecialConstraint.None, Variance.None, null),
				new TypeParameterSpec(1, definition, SpecialConstraint.None, Variance.None, null)
			}, false), builtinTypes.Void));
			AsyncTaskMethodBuilderStart = new PredefinedMember<MethodSpec>(module, types.AsyncTaskMethodBuilder, MemberFilter.Method("Start", 1, new ParametersImported(new ParameterData[1]
			{
				new ParameterData(null, Parameter.Modifier.REF)
			}, new TypeParameterSpec[1]
			{
				new TypeParameterSpec(0, definition, SpecialConstraint.None, Variance.None, null)
			}, false), builtinTypes.Void));
			AsyncTaskMethodBuilderTask = new PredefinedMember<PropertySpec>(module, types.AsyncTaskMethodBuilder, MemberFilter.Property("Task", null));
			AsyncTaskMethodBuilderGenericCreate = new PredefinedMember<MethodSpec>(module, types.AsyncTaskMethodBuilderGeneric, MemberFilter.Method("Create", 0, ParametersCompiled.EmptyReadOnlyParameters, types.AsyncVoidMethodBuilder.TypeSpec));
			AsyncTaskMethodBuilderGenericSetResult = new PredefinedMember<MethodSpec>(module, types.AsyncTaskMethodBuilderGeneric, "SetResult", MemberKind.Method, () => new TypeSpec[1] { types.AsyncTaskMethodBuilderGeneric.TypeSpec.MemberDefinition.TypeParameters[0] }, builtinTypes.Void);
			AsyncTaskMethodBuilderGenericSetStateMachine = new PredefinedMember<MethodSpec>(module, types.AsyncTaskMethodBuilderGeneric, "SetStateMachine", MemberKind.Method, () => new TypeSpec[1] { types.IAsyncStateMachine.TypeSpec }, builtinTypes.Void);
			AsyncTaskMethodBuilderGenericSetException = new PredefinedMember<MethodSpec>(module, types.AsyncTaskMethodBuilderGeneric, MemberFilter.Method("SetException", 0, ParametersCompiled.CreateFullyResolved(builtinTypes.Exception), builtinTypes.Void));
			AsyncTaskMethodBuilderGenericOnCompleted = new PredefinedMember<MethodSpec>(module, types.AsyncTaskMethodBuilderGeneric, MemberFilter.Method("AwaitOnCompleted", 2, new ParametersImported(new ParameterData[2]
			{
				new ParameterData(null, Parameter.Modifier.REF),
				new ParameterData(null, Parameter.Modifier.REF)
			}, new TypeParameterSpec[2]
			{
				new TypeParameterSpec(0, definition, SpecialConstraint.None, Variance.None, null),
				new TypeParameterSpec(1, definition, SpecialConstraint.None, Variance.None, null)
			}, false), builtinTypes.Void));
			AsyncTaskMethodBuilderGenericOnCompletedUnsafe = new PredefinedMember<MethodSpec>(module, types.AsyncTaskMethodBuilderGeneric, MemberFilter.Method("AwaitUnsafeOnCompleted", 2, new ParametersImported(new ParameterData[2]
			{
				new ParameterData(null, Parameter.Modifier.REF),
				new ParameterData(null, Parameter.Modifier.REF)
			}, new TypeParameterSpec[2]
			{
				new TypeParameterSpec(0, definition, SpecialConstraint.None, Variance.None, null),
				new TypeParameterSpec(1, definition, SpecialConstraint.None, Variance.None, null)
			}, false), builtinTypes.Void));
			AsyncTaskMethodBuilderGenericStart = new PredefinedMember<MethodSpec>(module, types.AsyncTaskMethodBuilderGeneric, MemberFilter.Method("Start", 1, new ParametersImported(new ParameterData[1]
			{
				new ParameterData(null, Parameter.Modifier.REF)
			}, new TypeParameterSpec[1]
			{
				new TypeParameterSpec(0, definition, SpecialConstraint.None, Variance.None, null)
			}, false), builtinTypes.Void));
			AsyncTaskMethodBuilderGenericTask = new PredefinedMember<PropertySpec>(module, types.AsyncTaskMethodBuilderGeneric, MemberFilter.Property("Task", null));
			AsyncVoidMethodBuilderCreate = new PredefinedMember<MethodSpec>(module, types.AsyncVoidMethodBuilder, MemberFilter.Method("Create", 0, ParametersCompiled.EmptyReadOnlyParameters, types.AsyncVoidMethodBuilder.TypeSpec));
			AsyncVoidMethodBuilderSetException = new PredefinedMember<MethodSpec>(module, types.AsyncVoidMethodBuilder, MemberFilter.Method("SetException", 0, null, builtinTypes.Void));
			AsyncVoidMethodBuilderSetResult = new PredefinedMember<MethodSpec>(module, types.AsyncVoidMethodBuilder, MemberFilter.Method("SetResult", 0, ParametersCompiled.EmptyReadOnlyParameters, builtinTypes.Void));
			AsyncVoidMethodBuilderSetStateMachine = new PredefinedMember<MethodSpec>(module, types.AsyncVoidMethodBuilder, "SetStateMachine", MemberKind.Method, () => new TypeSpec[1] { types.IAsyncStateMachine.TypeSpec }, builtinTypes.Void);
			AsyncVoidMethodBuilderOnCompleted = new PredefinedMember<MethodSpec>(module, types.AsyncVoidMethodBuilder, MemberFilter.Method("AwaitOnCompleted", 2, new ParametersImported(new ParameterData[2]
			{
				new ParameterData(null, Parameter.Modifier.REF),
				new ParameterData(null, Parameter.Modifier.REF)
			}, new TypeParameterSpec[2]
			{
				new TypeParameterSpec(0, definition, SpecialConstraint.None, Variance.None, null),
				new TypeParameterSpec(1, definition, SpecialConstraint.None, Variance.None, null)
			}, false), builtinTypes.Void));
			AsyncVoidMethodBuilderOnCompletedUnsafe = new PredefinedMember<MethodSpec>(module, types.AsyncVoidMethodBuilder, MemberFilter.Method("AwaitUnsafeOnCompleted", 2, new ParametersImported(new ParameterData[2]
			{
				new ParameterData(null, Parameter.Modifier.REF),
				new ParameterData(null, Parameter.Modifier.REF)
			}, new TypeParameterSpec[2]
			{
				new TypeParameterSpec(0, definition, SpecialConstraint.None, Variance.None, null),
				new TypeParameterSpec(1, definition, SpecialConstraint.None, Variance.None, null)
			}, false), builtinTypes.Void));
			AsyncVoidMethodBuilderStart = new PredefinedMember<MethodSpec>(module, types.AsyncVoidMethodBuilder, MemberFilter.Method("Start", 1, new ParametersImported(new ParameterData[1]
			{
				new ParameterData(null, Parameter.Modifier.REF)
			}, new TypeParameterSpec[1]
			{
				new TypeParameterSpec(0, definition, SpecialConstraint.None, Variance.None, null)
			}, false), builtinTypes.Void));
			AsyncStateMachineAttributeCtor = new PredefinedMember<MethodSpec>(module, predefinedAttributes.AsyncStateMachine, MemberFilter.Constructor(ParametersCompiled.CreateFullyResolved(builtinTypes.Type)));
			DebuggerBrowsableAttributeCtor = new PredefinedMember<MethodSpec>(module, predefinedAttributes.DebuggerBrowsable, MemberFilter.Constructor(null));
			DecimalCtor = new PredefinedMember<MethodSpec>(module, builtinTypes.Decimal, MemberFilter.Constructor(ParametersCompiled.CreateFullyResolved(builtinTypes.Int, builtinTypes.Int, builtinTypes.Int, builtinTypes.Bool, builtinTypes.Byte)));
			DecimalCtorInt = new PredefinedMember<MethodSpec>(module, builtinTypes.Decimal, MemberFilter.Constructor(ParametersCompiled.CreateFullyResolved(builtinTypes.Int)));
			DecimalCtorLong = new PredefinedMember<MethodSpec>(module, builtinTypes.Decimal, MemberFilter.Constructor(ParametersCompiled.CreateFullyResolved(builtinTypes.Long)));
			DecimalConstantAttributeCtor = new PredefinedMember<MethodSpec>(module, predefinedAttributes.DecimalConstant, MemberFilter.Constructor(ParametersCompiled.CreateFullyResolved(builtinTypes.Byte, builtinTypes.Byte, builtinTypes.UInt, builtinTypes.UInt, builtinTypes.UInt)));
			DefaultMemberAttributeCtor = new PredefinedMember<MethodSpec>(module, predefinedAttributes.DefaultMember, MemberFilter.Constructor(ParametersCompiled.CreateFullyResolved(builtinTypes.String)));
			DelegateCombine = new PredefinedMember<MethodSpec>(module, builtinTypes.Delegate, "Combine", builtinTypes.Delegate, builtinTypes.Delegate);
			DelegateRemove = new PredefinedMember<MethodSpec>(module, builtinTypes.Delegate, "Remove", builtinTypes.Delegate, builtinTypes.Delegate);
			DelegateEqual = new PredefinedMember<MethodSpec>(module, builtinTypes.Delegate, new MemberFilter(Operator.GetMetadataName(Operator.OpType.Equality), 0, MemberKind.Operator, null, builtinTypes.Bool));
			DelegateInequal = new PredefinedMember<MethodSpec>(module, builtinTypes.Delegate, new MemberFilter(Operator.GetMetadataName(Operator.OpType.Inequality), 0, MemberKind.Operator, null, builtinTypes.Bool));
			DynamicAttributeCtor = new PredefinedMember<MethodSpec>(module, predefinedAttributes.Dynamic, MemberFilter.Constructor(ParametersCompiled.CreateFullyResolved(ArrayContainer.MakeType(module, builtinTypes.Bool))));
			FieldInfoGetFieldFromHandle = new PredefinedMember<MethodSpec>(module, types.FieldInfo, "GetFieldFromHandle", MemberKind.Method, types.RuntimeFieldHandle);
			FieldInfoGetFieldFromHandle2 = new PredefinedMember<MethodSpec>(module, types.FieldInfo, "GetFieldFromHandle", MemberKind.Method, types.RuntimeFieldHandle, new PredefinedType(builtinTypes.RuntimeTypeHandle));
			FixedBufferAttributeCtor = new PredefinedMember<MethodSpec>(module, predefinedAttributes.FixedBuffer, MemberFilter.Constructor(ParametersCompiled.CreateFullyResolved(builtinTypes.Type, builtinTypes.Int)));
			IDisposableDispose = new PredefinedMember<MethodSpec>(module, builtinTypes.IDisposable, "Dispose", TypeSpec.EmptyTypes);
			IEnumerableGetEnumerator = new PredefinedMember<MethodSpec>(module, builtinTypes.IEnumerable, "GetEnumerator", TypeSpec.EmptyTypes);
			InterlockedCompareExchange = new PredefinedMember<MethodSpec>(module, types.Interlocked, MemberFilter.Method("CompareExchange", 0, new ParametersImported(new ParameterData[3]
			{
				new ParameterData(null, Parameter.Modifier.REF),
				new ParameterData(null, Parameter.Modifier.NONE),
				new ParameterData(null, Parameter.Modifier.NONE)
			}, new BuiltinTypeSpec[3] { builtinTypes.Int, builtinTypes.Int, builtinTypes.Int }, false), builtinTypes.Int));
			InterlockedCompareExchange_T = new PredefinedMember<MethodSpec>(module, types.Interlocked, MemberFilter.Method("CompareExchange", 1, new ParametersImported(new ParameterData[3]
			{
				new ParameterData(null, Parameter.Modifier.REF),
				new ParameterData(null, Parameter.Modifier.NONE),
				new ParameterData(null, Parameter.Modifier.NONE)
			}, new TypeParameterSpec[3]
			{
				new TypeParameterSpec(0, definition, SpecialConstraint.None, Variance.None, null),
				new TypeParameterSpec(0, definition, SpecialConstraint.None, Variance.None, null),
				new TypeParameterSpec(0, definition, SpecialConstraint.None, Variance.None, null)
			}, false), null));
			MethodInfoGetMethodFromHandle = new PredefinedMember<MethodSpec>(module, types.MethodBase, "GetMethodFromHandle", MemberKind.Method, types.RuntimeMethodHandle);
			MethodInfoGetMethodFromHandle2 = new PredefinedMember<MethodSpec>(module, types.MethodBase, "GetMethodFromHandle", MemberKind.Method, types.RuntimeMethodHandle, new PredefinedType(builtinTypes.RuntimeTypeHandle));
			MonitorEnter = new PredefinedMember<MethodSpec>(module, types.Monitor, "Enter", builtinTypes.Object);
			MonitorEnter_v4 = new PredefinedMember<MethodSpec>(module, types.Monitor, MemberFilter.Method("Enter", 0, new ParametersImported(new ParameterData[2]
			{
				new ParameterData(null, Parameter.Modifier.NONE),
				new ParameterData(null, Parameter.Modifier.REF)
			}, new BuiltinTypeSpec[2] { builtinTypes.Object, builtinTypes.Bool }, false), null));
			MonitorExit = new PredefinedMember<MethodSpec>(module, types.Monitor, "Exit", builtinTypes.Object);
			RuntimeCompatibilityWrapNonExceptionThrows = new PredefinedMember<PropertySpec>(module, predefinedAttributes.RuntimeCompatibility, MemberFilter.Property("WrapNonExceptionThrows", builtinTypes.Bool));
			RuntimeHelpersInitializeArray = new PredefinedMember<MethodSpec>(module, types.RuntimeHelpers, "InitializeArray", builtinTypes.Array, builtinTypes.RuntimeFieldHandle);
			RuntimeHelpersOffsetToStringData = new PredefinedMember<PropertySpec>(module, types.RuntimeHelpers, MemberFilter.Property("OffsetToStringData", builtinTypes.Int));
			SecurityActionRequestMinimum = new PredefinedMember<ConstSpec>(module, types.SecurityAction, "RequestMinimum", MemberKind.Field, types.SecurityAction);
			StringEmpty = new PredefinedMember<FieldSpec>(module, builtinTypes.String, MemberFilter.Field("Empty", builtinTypes.String));
			StringEqual = new PredefinedMember<MethodSpec>(module, builtinTypes.String, new MemberFilter(Operator.GetMetadataName(Operator.OpType.Equality), 0, MemberKind.Operator, null, builtinTypes.Bool));
			StringInequal = new PredefinedMember<MethodSpec>(module, builtinTypes.String, new MemberFilter(Operator.GetMetadataName(Operator.OpType.Inequality), 0, MemberKind.Operator, null, builtinTypes.Bool));
			StructLayoutAttributeCtor = new PredefinedMember<MethodSpec>(module, predefinedAttributes.StructLayout, MemberFilter.Constructor(ParametersCompiled.CreateFullyResolved(builtinTypes.Short)));
			StructLayoutCharSet = new PredefinedMember<FieldSpec>(module, predefinedAttributes.StructLayout, "CharSet", MemberKind.Field, types.CharSet);
			StructLayoutSize = new PredefinedMember<FieldSpec>(module, predefinedAttributes.StructLayout, MemberFilter.Field("Size", builtinTypes.Int));
			TypeGetTypeFromHandle = new PredefinedMember<MethodSpec>(module, builtinTypes.Type, "GetTypeFromHandle", builtinTypes.RuntimeTypeHandle);
		}
	}
}
