namespace Mono.CSharp
{
	public class PredefinedTypes
	{
		public readonly PredefinedType ArgIterator;

		public readonly PredefinedType TypedReference;

		public readonly PredefinedType MarshalByRefObject;

		public readonly PredefinedType RuntimeHelpers;

		public readonly PredefinedType IAsyncResult;

		public readonly PredefinedType AsyncCallback;

		public readonly PredefinedType RuntimeArgumentHandle;

		public readonly PredefinedType CharSet;

		public readonly PredefinedType IsVolatile;

		public readonly PredefinedType IEnumeratorGeneric;

		public readonly PredefinedType IListGeneric;

		public readonly PredefinedType IReadOnlyListGeneric;

		public readonly PredefinedType ICollectionGeneric;

		public readonly PredefinedType IReadOnlyCollectionGeneric;

		public readonly PredefinedType IEnumerableGeneric;

		public readonly PredefinedType Nullable;

		public readonly PredefinedType Activator;

		public readonly PredefinedType Interlocked;

		public readonly PredefinedType Monitor;

		public readonly PredefinedType NotSupportedException;

		public readonly PredefinedType RuntimeFieldHandle;

		public readonly PredefinedType RuntimeMethodHandle;

		public readonly PredefinedType SecurityAction;

		public readonly PredefinedType Dictionary;

		public readonly PredefinedType Hashtable;

		public readonly TypeSpec[] SwitchUserTypes;

		public readonly PredefinedType Expression;

		public readonly PredefinedType ExpressionGeneric;

		public readonly PredefinedType ParameterExpression;

		public readonly PredefinedType FieldInfo;

		public readonly PredefinedType MethodBase;

		public readonly PredefinedType MethodInfo;

		public readonly PredefinedType ConstructorInfo;

		public readonly PredefinedType MemberBinding;

		public readonly PredefinedType Binder;

		public readonly PredefinedType CallSite;

		public readonly PredefinedType CallSiteGeneric;

		public readonly PredefinedType BinderFlags;

		public readonly PredefinedType AsyncVoidMethodBuilder;

		public readonly PredefinedType AsyncTaskMethodBuilder;

		public readonly PredefinedType AsyncTaskMethodBuilderGeneric;

		public readonly PredefinedType Action;

		public readonly PredefinedType Task;

		public readonly PredefinedType TaskGeneric;

		public readonly PredefinedType IAsyncStateMachine;

		public readonly PredefinedType INotifyCompletion;

		public readonly PredefinedType ICriticalNotifyCompletion;

		public readonly PredefinedType IFormattable;

		public readonly PredefinedType FormattableString;

		public readonly PredefinedType FormattableStringFactory;

		public PredefinedTypes(ModuleContainer module)
		{
			TypedReference = new PredefinedType(module, MemberKind.Struct, "System", "TypedReference");
			ArgIterator = new PredefinedType(module, MemberKind.Struct, "System", "ArgIterator");
			MarshalByRefObject = new PredefinedType(module, MemberKind.Class, "System", "MarshalByRefObject");
			RuntimeHelpers = new PredefinedType(module, MemberKind.Class, "System.Runtime.CompilerServices", "RuntimeHelpers");
			IAsyncResult = new PredefinedType(module, MemberKind.Interface, "System", "IAsyncResult");
			AsyncCallback = new PredefinedType(module, MemberKind.Delegate, "System", "AsyncCallback");
			RuntimeArgumentHandle = new PredefinedType(module, MemberKind.Struct, "System", "RuntimeArgumentHandle");
			CharSet = new PredefinedType(module, MemberKind.Enum, "System.Runtime.InteropServices", "CharSet");
			IsVolatile = new PredefinedType(module, MemberKind.Class, "System.Runtime.CompilerServices", "IsVolatile");
			IEnumeratorGeneric = new PredefinedType(module, MemberKind.Interface, "System.Collections.Generic", "IEnumerator", 1);
			IListGeneric = new PredefinedType(module, MemberKind.Interface, "System.Collections.Generic", "IList", 1);
			IReadOnlyListGeneric = new PredefinedType(module, MemberKind.Interface, "System.Collections.Generic", "IReadOnlyList", 1);
			ICollectionGeneric = new PredefinedType(module, MemberKind.Interface, "System.Collections.Generic", "ICollection", 1);
			IReadOnlyCollectionGeneric = new PredefinedType(module, MemberKind.Interface, "System.Collections.Generic", "IReadOnlyCollection", 1);
			IEnumerableGeneric = new PredefinedType(module, MemberKind.Interface, "System.Collections.Generic", "IEnumerable", 1);
			Nullable = new PredefinedType(module, MemberKind.Struct, "System", "Nullable", 1);
			Activator = new PredefinedType(module, MemberKind.Class, "System", "Activator");
			Interlocked = new PredefinedType(module, MemberKind.Class, "System.Threading", "Interlocked");
			Monitor = new PredefinedType(module, MemberKind.Class, "System.Threading", "Monitor");
			NotSupportedException = new PredefinedType(module, MemberKind.Class, "System", "NotSupportedException");
			RuntimeFieldHandle = new PredefinedType(module, MemberKind.Struct, "System", "RuntimeFieldHandle");
			RuntimeMethodHandle = new PredefinedType(module, MemberKind.Struct, "System", "RuntimeMethodHandle");
			SecurityAction = new PredefinedType(module, MemberKind.Enum, "System.Security.Permissions", "SecurityAction");
			Dictionary = new PredefinedType(module, MemberKind.Class, "System.Collections.Generic", "Dictionary", 2);
			Hashtable = new PredefinedType(module, MemberKind.Class, "System.Collections", "Hashtable");
			Expression = new PredefinedType(module, MemberKind.Class, "System.Linq.Expressions", "Expression");
			ExpressionGeneric = new PredefinedType(module, MemberKind.Class, "System.Linq.Expressions", "Expression", 1);
			MemberBinding = new PredefinedType(module, MemberKind.Class, "System.Linq.Expressions", "MemberBinding");
			ParameterExpression = new PredefinedType(module, MemberKind.Class, "System.Linq.Expressions", "ParameterExpression");
			FieldInfo = new PredefinedType(module, MemberKind.Class, "System.Reflection", "FieldInfo");
			MethodBase = new PredefinedType(module, MemberKind.Class, "System.Reflection", "MethodBase");
			MethodInfo = new PredefinedType(module, MemberKind.Class, "System.Reflection", "MethodInfo");
			ConstructorInfo = new PredefinedType(module, MemberKind.Class, "System.Reflection", "ConstructorInfo");
			CallSite = new PredefinedType(module, MemberKind.Class, "System.Runtime.CompilerServices", "CallSite");
			CallSiteGeneric = new PredefinedType(module, MemberKind.Class, "System.Runtime.CompilerServices", "CallSite", 1);
			Binder = new PredefinedType(module, MemberKind.Class, "Microsoft.CSharp.RuntimeBinder", "Binder");
			BinderFlags = new PredefinedType(module, MemberKind.Enum, "Microsoft.CSharp.RuntimeBinder", "CSharpBinderFlags");
			Action = new PredefinedType(module, MemberKind.Delegate, "System", "Action");
			AsyncVoidMethodBuilder = new PredefinedType(module, MemberKind.Struct, "System.Runtime.CompilerServices", "AsyncVoidMethodBuilder");
			AsyncTaskMethodBuilder = new PredefinedType(module, MemberKind.Struct, "System.Runtime.CompilerServices", "AsyncTaskMethodBuilder");
			AsyncTaskMethodBuilderGeneric = new PredefinedType(module, MemberKind.Struct, "System.Runtime.CompilerServices", "AsyncTaskMethodBuilder", 1);
			Task = new PredefinedType(module, MemberKind.Class, "System.Threading.Tasks", "Task");
			TaskGeneric = new PredefinedType(module, MemberKind.Class, "System.Threading.Tasks", "Task", 1);
			IAsyncStateMachine = new PredefinedType(module, MemberKind.Interface, "System.Runtime.CompilerServices", "IAsyncStateMachine");
			INotifyCompletion = new PredefinedType(module, MemberKind.Interface, "System.Runtime.CompilerServices", "INotifyCompletion");
			ICriticalNotifyCompletion = new PredefinedType(module, MemberKind.Interface, "System.Runtime.CompilerServices", "ICriticalNotifyCompletion");
			IFormattable = new PredefinedType(module, MemberKind.Interface, "System", "IFormattable");
			FormattableString = new PredefinedType(module, MemberKind.Class, "System", "FormattableString");
			FormattableStringFactory = new PredefinedType(module, MemberKind.Class, "System.Runtime.CompilerServices", "FormattableStringFactory");
			if (TypedReference.Define())
			{
				TypedReference.TypeSpec.IsSpecialRuntimeType = true;
			}
			if (ArgIterator.Define())
			{
				ArgIterator.TypeSpec.IsSpecialRuntimeType = true;
			}
			if (IEnumerableGeneric.Define())
			{
				IEnumerableGeneric.TypeSpec.IsArrayGenericInterface = true;
			}
			if (IListGeneric.Define())
			{
				IListGeneric.TypeSpec.IsArrayGenericInterface = true;
			}
			if (IReadOnlyListGeneric.Define())
			{
				IReadOnlyListGeneric.TypeSpec.IsArrayGenericInterface = true;
			}
			if (ICollectionGeneric.Define())
			{
				ICollectionGeneric.TypeSpec.IsArrayGenericInterface = true;
			}
			if (IReadOnlyCollectionGeneric.Define())
			{
				IReadOnlyCollectionGeneric.TypeSpec.IsArrayGenericInterface = true;
			}
			if (Nullable.Define())
			{
				Nullable.TypeSpec.IsNullableType = true;
			}
			if (ExpressionGeneric.Define())
			{
				ExpressionGeneric.TypeSpec.IsExpressionTreeType = true;
			}
			Task.Define();
			if (TaskGeneric.Define())
			{
				TaskGeneric.TypeSpec.IsGenericTask = true;
			}
			SwitchUserTypes = Switch.CreateSwitchUserTypes(module, Nullable.TypeSpec);
			IFormattable.Define();
			FormattableString.Define();
		}
	}
}
