using System.Reflection;

namespace Mono.CSharp
{
	public class PredefinedAttributes
	{
		public readonly PredefinedAttribute ParamArray;

		public readonly PredefinedAttribute Out;

		public readonly PredefinedAttribute Obsolete;

		public readonly PredefinedAttribute DllImport;

		public readonly PredefinedAttribute MethodImpl;

		public readonly PredefinedAttribute MarshalAs;

		public readonly PredefinedAttribute In;

		public readonly PredefinedAttribute IndexerName;

		public readonly PredefinedAttribute Conditional;

		public readonly PredefinedAttribute CLSCompliant;

		public readonly PredefinedAttribute Security;

		public readonly PredefinedAttribute Required;

		public readonly PredefinedAttribute Guid;

		public readonly PredefinedAttribute AssemblyCulture;

		public readonly PredefinedAttribute AssemblyVersion;

		public readonly PredefinedAttribute AssemblyAlgorithmId;

		public readonly PredefinedAttribute AssemblyFlags;

		public readonly PredefinedAttribute AssemblyFileVersion;

		public readonly PredefinedAttribute ComImport;

		public readonly PredefinedAttribute CoClass;

		public readonly PredefinedAttribute AttributeUsage;

		public readonly PredefinedAttribute DefaultParameterValue;

		public readonly PredefinedAttribute OptionalParameter;

		public readonly PredefinedAttribute UnverifiableCode;

		public readonly PredefinedAttribute DefaultCharset;

		public readonly PredefinedAttribute TypeForwarder;

		public readonly PredefinedAttribute FixedBuffer;

		public readonly PredefinedAttribute CompilerGenerated;

		public readonly PredefinedAttribute InternalsVisibleTo;

		public readonly PredefinedAttribute RuntimeCompatibility;

		public readonly PredefinedAttribute DebuggerHidden;

		public readonly PredefinedAttribute UnsafeValueType;

		public readonly PredefinedAttribute UnmanagedFunctionPointer;

		public readonly PredefinedDebuggerBrowsableAttribute DebuggerBrowsable;

		public readonly PredefinedAttribute DebuggerStepThrough;

		public readonly PredefinedDebuggableAttribute Debuggable;

		public readonly PredefinedAttribute HostProtection;

		public readonly PredefinedAttribute Extension;

		public readonly PredefinedDynamicAttribute Dynamic;

		public readonly PredefinedStateMachineAttribute AsyncStateMachine;

		public readonly PredefinedAttribute DefaultMember;

		public readonly PredefinedDecimalAttribute DecimalConstant;

		public readonly PredefinedAttribute StructLayout;

		public readonly PredefinedAttribute FieldOffset;

		public readonly PredefinedAttribute AssemblyProduct;

		public readonly PredefinedAttribute AssemblyCompany;

		public readonly PredefinedAttribute AssemblyCopyright;

		public readonly PredefinedAttribute AssemblyTrademark;

		public readonly PredefinedAttribute CallerMemberNameAttribute;

		public readonly PredefinedAttribute CallerLineNumberAttribute;

		public readonly PredefinedAttribute CallerFilePathAttribute;

		public PredefinedAttributes(ModuleContainer module)
		{
			ParamArray = new PredefinedAttribute(module, "System", "ParamArrayAttribute");
			Out = new PredefinedAttribute(module, "System.Runtime.InteropServices", "OutAttribute");
			ParamArray.Resolve();
			Out.Resolve();
			Obsolete = new PredefinedAttribute(module, "System", "ObsoleteAttribute");
			DllImport = new PredefinedAttribute(module, "System.Runtime.InteropServices", "DllImportAttribute");
			MethodImpl = new PredefinedAttribute(module, "System.Runtime.CompilerServices", "MethodImplAttribute");
			MarshalAs = new PredefinedAttribute(module, "System.Runtime.InteropServices", "MarshalAsAttribute");
			In = new PredefinedAttribute(module, "System.Runtime.InteropServices", "InAttribute");
			IndexerName = new PredefinedAttribute(module, "System.Runtime.CompilerServices", "IndexerNameAttribute");
			Conditional = new PredefinedAttribute(module, "System.Diagnostics", "ConditionalAttribute");
			CLSCompliant = new PredefinedAttribute(module, "System", "CLSCompliantAttribute");
			Security = new PredefinedAttribute(module, "System.Security.Permissions", "SecurityAttribute");
			Required = new PredefinedAttribute(module, "System.Runtime.CompilerServices", "RequiredAttributeAttribute");
			Guid = new PredefinedAttribute(module, "System.Runtime.InteropServices", "GuidAttribute");
			AssemblyCulture = new PredefinedAttribute(module, "System.Reflection", "AssemblyCultureAttribute");
			AssemblyVersion = new PredefinedAttribute(module, "System.Reflection", "AssemblyVersionAttribute");
			AssemblyAlgorithmId = new PredefinedAttribute(module, "System.Reflection", "AssemblyAlgorithmIdAttribute");
			AssemblyFlags = new PredefinedAttribute(module, "System.Reflection", "AssemblyFlagsAttribute");
			AssemblyFileVersion = new PredefinedAttribute(module, "System.Reflection", "AssemblyFileVersionAttribute");
			ComImport = new PredefinedAttribute(module, "System.Runtime.InteropServices", "ComImportAttribute");
			CoClass = new PredefinedAttribute(module, "System.Runtime.InteropServices", "CoClassAttribute");
			AttributeUsage = new PredefinedAttribute(module, "System", "AttributeUsageAttribute");
			DefaultParameterValue = new PredefinedAttribute(module, "System.Runtime.InteropServices", "DefaultParameterValueAttribute");
			OptionalParameter = new PredefinedAttribute(module, "System.Runtime.InteropServices", "OptionalAttribute");
			UnverifiableCode = new PredefinedAttribute(module, "System.Security", "UnverifiableCodeAttribute");
			HostProtection = new PredefinedAttribute(module, "System.Security.Permissions", "HostProtectionAttribute");
			DefaultCharset = new PredefinedAttribute(module, "System.Runtime.InteropServices", "DefaultCharSetAttribute");
			TypeForwarder = new PredefinedAttribute(module, "System.Runtime.CompilerServices", "TypeForwardedToAttribute");
			FixedBuffer = new PredefinedAttribute(module, "System.Runtime.CompilerServices", "FixedBufferAttribute");
			CompilerGenerated = new PredefinedAttribute(module, "System.Runtime.CompilerServices", "CompilerGeneratedAttribute");
			InternalsVisibleTo = new PredefinedAttribute(module, "System.Runtime.CompilerServices", "InternalsVisibleToAttribute");
			RuntimeCompatibility = new PredefinedAttribute(module, "System.Runtime.CompilerServices", "RuntimeCompatibilityAttribute");
			DebuggerHidden = new PredefinedAttribute(module, "System.Diagnostics", "DebuggerHiddenAttribute");
			UnsafeValueType = new PredefinedAttribute(module, "System.Runtime.CompilerServices", "UnsafeValueTypeAttribute");
			UnmanagedFunctionPointer = new PredefinedAttribute(module, "System.Runtime.InteropServices", "UnmanagedFunctionPointerAttribute");
			DebuggerBrowsable = new PredefinedDebuggerBrowsableAttribute(module, "System.Diagnostics", "DebuggerBrowsableAttribute");
			DebuggerStepThrough = new PredefinedAttribute(module, "System.Diagnostics", "DebuggerStepThroughAttribute");
			Debuggable = new PredefinedDebuggableAttribute(module, "System.Diagnostics", "DebuggableAttribute");
			Extension = new PredefinedAttribute(module, "System.Runtime.CompilerServices", "ExtensionAttribute");
			Dynamic = new PredefinedDynamicAttribute(module, "System.Runtime.CompilerServices", "DynamicAttribute");
			DefaultMember = new PredefinedAttribute(module, "System.Reflection", "DefaultMemberAttribute");
			DecimalConstant = new PredefinedDecimalAttribute(module, "System.Runtime.CompilerServices", "DecimalConstantAttribute");
			StructLayout = new PredefinedAttribute(module, "System.Runtime.InteropServices", "StructLayoutAttribute");
			FieldOffset = new PredefinedAttribute(module, "System.Runtime.InteropServices", "FieldOffsetAttribute");
			AssemblyProduct = new PredefinedAttribute(module, "System.Reflection", "AssemblyProductAttribute");
			AssemblyCompany = new PredefinedAttribute(module, "System.Reflection", "AssemblyCompanyAttribute");
			AssemblyCopyright = new PredefinedAttribute(module, "System.Reflection", "AssemblyCopyrightAttribute");
			AssemblyTrademark = new PredefinedAttribute(module, "System.Reflection", "AssemblyTrademarkAttribute");
			AsyncStateMachine = new PredefinedStateMachineAttribute(module, "System.Runtime.CompilerServices", "AsyncStateMachineAttribute");
			CallerMemberNameAttribute = new PredefinedAttribute(module, "System.Runtime.CompilerServices", "CallerMemberNameAttribute");
			CallerLineNumberAttribute = new PredefinedAttribute(module, "System.Runtime.CompilerServices", "CallerLineNumberAttribute");
			CallerFilePathAttribute = new PredefinedAttribute(module, "System.Runtime.CompilerServices", "CallerFilePathAttribute");
			FieldInfo[] fields = GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			for (int i = 0; i < fields.Length; i++)
			{
				((PredefinedAttribute)fields[i].GetValue(this)).Define();
			}
		}
	}
}
