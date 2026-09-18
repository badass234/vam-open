using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	public sealed class Universe : IDisposable
	{
		private struct ScopedTypeName : IEquatable<ScopedTypeName>
		{
			private readonly object scope;

			private readonly TypeName name;

			internal ScopedTypeName(object scope, TypeName name)
			{
				this.scope = scope;
				this.name = name;
			}

			public override bool Equals(object obj)
			{
				ScopedTypeName? scopedTypeName = obj as ScopedTypeName?;
				if (scopedTypeName.HasValue)
				{
					return ((IEquatable<ScopedTypeName>)scopedTypeName.Value).Equals(this);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return scope.GetHashCode() * 7 + name.GetHashCode();
			}

			bool IEquatable<ScopedTypeName>.Equals(ScopedTypeName other)
			{
				if (other.scope == scope)
				{
					return other.name == name;
				}
				return false;
			}
		}

		internal static readonly bool MonoRuntime = System.Type.GetType("Mono.Runtime") != null;

		private readonly Dictionary<Type, Type> canonicalizedTypes = new Dictionary<Type, Type>();

		private readonly List<AssemblyReader> assemblies = new List<AssemblyReader>();

		private readonly List<AssemblyBuilder> dynamicAssemblies = new List<AssemblyBuilder>();

		private readonly Dictionary<string, Assembly> assembliesByName = new Dictionary<string, Assembly>();

		private readonly Dictionary<System.Type, Type> importedTypes = new Dictionary<System.Type, Type>();

		private Dictionary<ScopedTypeName, Type> missingTypes;

		private bool resolveMissingMembers;

		private readonly bool enableFunctionPointers;

		private readonly bool useNativeFusion;

		private readonly bool returnPseudoCustomAttributes;

		private readonly bool automaticallyProvideDefaultConstructor;

		private readonly UniverseOptions options;

		private Type typeof_System_Object;

		private Type typeof_System_ValueType;

		private Type typeof_System_Enum;

		private Type typeof_System_Void;

		private Type typeof_System_Boolean;

		private Type typeof_System_Char;

		private Type typeof_System_SByte;

		private Type typeof_System_Byte;

		private Type typeof_System_Int16;

		private Type typeof_System_UInt16;

		private Type typeof_System_Int32;

		private Type typeof_System_UInt32;

		private Type typeof_System_Int64;

		private Type typeof_System_UInt64;

		private Type typeof_System_Single;

		private Type typeof_System_Double;

		private Type typeof_System_String;

		private Type typeof_System_IntPtr;

		private Type typeof_System_UIntPtr;

		private Type typeof_System_TypedReference;

		private Type typeof_System_Type;

		private Type typeof_System_Array;

		private Type typeof_System_DateTime;

		private Type typeof_System_DBNull;

		private Type typeof_System_Decimal;

		private Type typeof_System_AttributeUsageAttribute;

		private Type typeof_System_Runtime_InteropServices_DllImportAttribute;

		private Type typeof_System_Runtime_InteropServices_FieldOffsetAttribute;

		private Type typeof_System_Runtime_InteropServices_MarshalAsAttribute;

		private Type typeof_System_Runtime_InteropServices_UnmanagedType;

		private Type typeof_System_Runtime_InteropServices_VarEnum;

		private Type typeof_System_Runtime_InteropServices_PreserveSigAttribute;

		private Type typeof_System_Runtime_InteropServices_CallingConvention;

		private Type typeof_System_Runtime_InteropServices_CharSet;

		private Type typeof_System_Runtime_CompilerServices_DecimalConstantAttribute;

		private Type typeof_System_Reflection_AssemblyCopyrightAttribute;

		private Type typeof_System_Reflection_AssemblyTrademarkAttribute;

		private Type typeof_System_Reflection_AssemblyProductAttribute;

		private Type typeof_System_Reflection_AssemblyCompanyAttribute;

		private Type typeof_System_Reflection_AssemblyDescriptionAttribute;

		private Type typeof_System_Reflection_AssemblyTitleAttribute;

		private Type typeof_System_Reflection_AssemblyInformationalVersionAttribute;

		private Type typeof_System_Reflection_AssemblyFileVersionAttribute;

		private Type typeof_System_Security_Permissions_CodeAccessSecurityAttribute;

		private Type typeof_System_Security_Permissions_PermissionSetAttribute;

		private Type typeof_System_Security_Permissions_SecurityAction;

		private List<ResolveEventHandler> resolvers = new List<ResolveEventHandler>();

		private Predicate<Type> missingTypeIsValueType;

		internal Assembly Mscorlib => Load("mscorlib");

		internal Type System_Object => typeof_System_Object ?? (typeof_System_Object = ResolvePrimitive("Object"));

		internal Type System_ValueType => typeof_System_ValueType ?? (typeof_System_ValueType = ResolvePrimitive("ValueType"));

		internal Type System_Enum => typeof_System_Enum ?? (typeof_System_Enum = ResolvePrimitive("Enum"));

		internal Type System_Void => typeof_System_Void ?? (typeof_System_Void = ResolvePrimitive("Void"));

		internal Type System_Boolean => typeof_System_Boolean ?? (typeof_System_Boolean = ResolvePrimitive("Boolean"));

		internal Type System_Char => typeof_System_Char ?? (typeof_System_Char = ResolvePrimitive("Char"));

		internal Type System_SByte => typeof_System_SByte ?? (typeof_System_SByte = ResolvePrimitive("SByte"));

		internal Type System_Byte => typeof_System_Byte ?? (typeof_System_Byte = ResolvePrimitive("Byte"));

		internal Type System_Int16 => typeof_System_Int16 ?? (typeof_System_Int16 = ResolvePrimitive("Int16"));

		internal Type System_UInt16 => typeof_System_UInt16 ?? (typeof_System_UInt16 = ResolvePrimitive("UInt16"));

		internal Type System_Int32 => typeof_System_Int32 ?? (typeof_System_Int32 = ResolvePrimitive("Int32"));

		internal Type System_UInt32 => typeof_System_UInt32 ?? (typeof_System_UInt32 = ResolvePrimitive("UInt32"));

		internal Type System_Int64 => typeof_System_Int64 ?? (typeof_System_Int64 = ResolvePrimitive("Int64"));

		internal Type System_UInt64 => typeof_System_UInt64 ?? (typeof_System_UInt64 = ResolvePrimitive("UInt64"));

		internal Type System_Single => typeof_System_Single ?? (typeof_System_Single = ResolvePrimitive("Single"));

		internal Type System_Double => typeof_System_Double ?? (typeof_System_Double = ResolvePrimitive("Double"));

		internal Type System_String => typeof_System_String ?? (typeof_System_String = ResolvePrimitive("String"));

		internal Type System_IntPtr => typeof_System_IntPtr ?? (typeof_System_IntPtr = ResolvePrimitive("IntPtr"));

		internal Type System_UIntPtr => typeof_System_UIntPtr ?? (typeof_System_UIntPtr = ResolvePrimitive("UIntPtr"));

		internal Type System_TypedReference => typeof_System_TypedReference ?? (typeof_System_TypedReference = ResolvePrimitive("TypedReference"));

		internal Type System_Type => typeof_System_Type ?? (typeof_System_Type = ResolvePrimitive("Type"));

		internal Type System_Array => typeof_System_Array ?? (typeof_System_Array = ResolvePrimitive("Array"));

		internal Type System_DateTime => typeof_System_DateTime ?? (typeof_System_DateTime = ImportMscorlibType("System", "DateTime"));

		internal Type System_DBNull => typeof_System_DBNull ?? (typeof_System_DBNull = ImportMscorlibType("System", "DBNull"));

		internal Type System_Decimal => typeof_System_Decimal ?? (typeof_System_Decimal = ImportMscorlibType("System", "Decimal"));

		internal Type System_AttributeUsageAttribute => typeof_System_AttributeUsageAttribute ?? (typeof_System_AttributeUsageAttribute = ImportMscorlibType("System", "AttributeUsageAttribute"));

		internal Type System_Runtime_InteropServices_DllImportAttribute => typeof_System_Runtime_InteropServices_DllImportAttribute ?? (typeof_System_Runtime_InteropServices_DllImportAttribute = ImportMscorlibType("System.Runtime.InteropServices", "DllImportAttribute"));

		internal Type System_Runtime_InteropServices_FieldOffsetAttribute => typeof_System_Runtime_InteropServices_FieldOffsetAttribute ?? (typeof_System_Runtime_InteropServices_FieldOffsetAttribute = ImportMscorlibType("System.Runtime.InteropServices", "FieldOffsetAttribute"));

		internal Type System_Runtime_InteropServices_MarshalAsAttribute => typeof_System_Runtime_InteropServices_MarshalAsAttribute ?? (typeof_System_Runtime_InteropServices_MarshalAsAttribute = ImportMscorlibType("System.Runtime.InteropServices", "MarshalAsAttribute"));

		internal Type System_Runtime_InteropServices_UnmanagedType => typeof_System_Runtime_InteropServices_UnmanagedType ?? (typeof_System_Runtime_InteropServices_UnmanagedType = ImportMscorlibType("System.Runtime.InteropServices", "UnmanagedType"));

		internal Type System_Runtime_InteropServices_VarEnum => typeof_System_Runtime_InteropServices_VarEnum ?? (typeof_System_Runtime_InteropServices_VarEnum = ImportMscorlibType("System.Runtime.InteropServices", "VarEnum"));

		internal Type System_Runtime_InteropServices_PreserveSigAttribute => typeof_System_Runtime_InteropServices_PreserveSigAttribute ?? (typeof_System_Runtime_InteropServices_PreserveSigAttribute = ImportMscorlibType("System.Runtime.InteropServices", "PreserveSigAttribute"));

		internal Type System_Runtime_InteropServices_CallingConvention => typeof_System_Runtime_InteropServices_CallingConvention ?? (typeof_System_Runtime_InteropServices_CallingConvention = ImportMscorlibType("System.Runtime.InteropServices", "CallingConvention"));

		internal Type System_Runtime_InteropServices_CharSet => typeof_System_Runtime_InteropServices_CharSet ?? (typeof_System_Runtime_InteropServices_CharSet = ImportMscorlibType("System.Runtime.InteropServices", "CharSet"));

		internal Type System_Runtime_CompilerServices_DecimalConstantAttribute => typeof_System_Runtime_CompilerServices_DecimalConstantAttribute ?? (typeof_System_Runtime_CompilerServices_DecimalConstantAttribute = ImportMscorlibType("System.Runtime.CompilerServices", "DecimalConstantAttribute"));

		internal Type System_Reflection_AssemblyCopyrightAttribute => typeof_System_Reflection_AssemblyCopyrightAttribute ?? (typeof_System_Reflection_AssemblyCopyrightAttribute = ImportMscorlibType("System.Reflection", "AssemblyCopyrightAttribute"));

		internal Type System_Reflection_AssemblyTrademarkAttribute => typeof_System_Reflection_AssemblyTrademarkAttribute ?? (typeof_System_Reflection_AssemblyTrademarkAttribute = ImportMscorlibType("System.Reflection", "AssemblyTrademarkAttribute"));

		internal Type System_Reflection_AssemblyProductAttribute => typeof_System_Reflection_AssemblyProductAttribute ?? (typeof_System_Reflection_AssemblyProductAttribute = ImportMscorlibType("System.Reflection", "AssemblyProductAttribute"));

		internal Type System_Reflection_AssemblyCompanyAttribute => typeof_System_Reflection_AssemblyCompanyAttribute ?? (typeof_System_Reflection_AssemblyCompanyAttribute = ImportMscorlibType("System.Reflection", "AssemblyCompanyAttribute"));

		internal Type System_Reflection_AssemblyDescriptionAttribute => typeof_System_Reflection_AssemblyDescriptionAttribute ?? (typeof_System_Reflection_AssemblyDescriptionAttribute = ImportMscorlibType("System.Reflection", "AssemblyDescriptionAttribute"));

		internal Type System_Reflection_AssemblyTitleAttribute => typeof_System_Reflection_AssemblyTitleAttribute ?? (typeof_System_Reflection_AssemblyTitleAttribute = ImportMscorlibType("System.Reflection", "AssemblyTitleAttribute"));

		internal Type System_Reflection_AssemblyInformationalVersionAttribute => typeof_System_Reflection_AssemblyInformationalVersionAttribute ?? (typeof_System_Reflection_AssemblyInformationalVersionAttribute = ImportMscorlibType("System.Reflection", "AssemblyInformationalVersionAttribute"));

		internal Type System_Reflection_AssemblyFileVersionAttribute => typeof_System_Reflection_AssemblyFileVersionAttribute ?? (typeof_System_Reflection_AssemblyFileVersionAttribute = ImportMscorlibType("System.Reflection", "AssemblyFileVersionAttribute"));

		internal Type System_Security_Permissions_CodeAccessSecurityAttribute => typeof_System_Security_Permissions_CodeAccessSecurityAttribute ?? (typeof_System_Security_Permissions_CodeAccessSecurityAttribute = ImportMscorlibType("System.Security.Permissions", "CodeAccessSecurityAttribute"));

		internal Type System_Security_Permissions_PermissionSetAttribute => typeof_System_Security_Permissions_PermissionSetAttribute ?? (typeof_System_Security_Permissions_PermissionSetAttribute = ImportMscorlibType("System.Security.Permissions", "PermissionSetAttribute"));

		internal Type System_Security_Permissions_SecurityAction => typeof_System_Security_Permissions_SecurityAction ?? (typeof_System_Security_Permissions_SecurityAction = ImportMscorlibType("System.Security.Permissions", "SecurityAction"));

		internal bool HasMscorlib => GetLoadedAssembly("mscorlib") != null;

		internal bool MissingMemberResolution => resolveMissingMembers;

		internal bool EnableFunctionPointers => enableFunctionPointers;

		internal bool ReturnPseudoCustomAttributes => returnPseudoCustomAttributes;

		internal bool AutomaticallyProvideDefaultConstructor => automaticallyProvideDefaultConstructor;

		internal bool MetadataOnly => (options & UniverseOptions.MetadataOnly) != 0;

		internal bool WindowsRuntimeProjection => (options & UniverseOptions.DisableWindowsRuntimeProjection) == 0;

		internal bool DecodeVersionInfoAttributeBlobs => (options & UniverseOptions.DecodeVersionInfoAttributeBlobs) != 0;

		internal bool Deterministic => (options & UniverseOptions.DeterministicOutput) != 0;

		public event ResolveEventHandler AssemblyResolve
		{
			add
			{
				resolvers.Add(value);
			}
			remove
			{
				resolvers.Remove(value);
			}
		}

		public event ResolvedMissingMemberHandler ResolvedMissingMember;

		public event Predicate<Type> MissingTypeIsValueType
		{
			add
			{
				if (missingTypeIsValueType != null)
				{
					throw new InvalidOperationException("Only a single MissingTypeIsValueType handler can be registered.");
				}
				missingTypeIsValueType = value;
			}
			remove
			{
				if (value.Equals(missingTypeIsValueType))
				{
					missingTypeIsValueType = null;
				}
			}
		}

		public Universe()
			: this(UniverseOptions.None)
		{
		}

		public Universe(UniverseOptions options)
		{
			this.options = options;
			enableFunctionPointers = (options & UniverseOptions.EnableFunctionPointers) != 0;
			useNativeFusion = (options & UniverseOptions.DisableFusion) == 0 && GetUseNativeFusion();
			returnPseudoCustomAttributes = (options & UniverseOptions.DisablePseudoCustomAttributeRetrieval) == 0;
			automaticallyProvideDefaultConstructor = (options & UniverseOptions.DontProvideAutomaticDefaultConstructor) == 0;
			resolveMissingMembers = (options & UniverseOptions.ResolveMissingMembers) != 0;
		}

		private static bool GetUseNativeFusion()
		{
			try
			{
				return Environment.OSVersion.Platform == PlatformID.Win32NT && !MonoRuntime && Environment.GetEnvironmentVariable("IKVM_DISABLE_FUSION") == null;
			}
			catch (SecurityException)
			{
				return false;
			}
		}

		private Type ImportMscorlibType(string ns, string name)
		{
			if (Mscorlib.__IsMissing)
			{
				return Mscorlib.ResolveType(null, new TypeName(ns, name));
			}
			return Mscorlib.FindType(new TypeName(ns, name));
		}

		private Type ResolvePrimitive(string name)
		{
			return Mscorlib.FindType(new TypeName("System", name)) ?? GetMissingType(null, Mscorlib.ManifestModule, null, new TypeName("System", name));
		}

		public Type Import(System.Type type)
		{
			Type value;
			if (!importedTypes.TryGetValue(type, out value))
			{
				value = ImportImpl(type);
				if (value != null)
				{
					importedTypes.Add(type, value);
				}
			}
			return value;
		}

		private Type ImportImpl(System.Type type)
		{
			if (type.Assembly == typeof(Type).Assembly)
			{
				throw new ArgumentException("Did you really want to import " + type.FullName + "?");
			}
			if (type.HasElementType)
			{
				if (type.IsArray)
				{
					if (type.Name.EndsWith("[]"))
					{
						return Import(type.GetElementType()).MakeArrayType();
					}
					return Import(type.GetElementType()).MakeArrayType(type.GetArrayRank());
				}
				if (type.IsByRef)
				{
					return Import(type.GetElementType()).MakeByRefType();
				}
				if (type.IsPointer)
				{
					return Import(type.GetElementType()).MakePointerType();
				}
				throw new InvalidOperationException();
			}
			if (type.IsGenericParameter)
			{
				if (type.DeclaringMethod != null)
				{
					throw new NotImplementedException();
				}
				return Import(type.DeclaringType).GetGenericArguments()[type.GenericParameterPosition];
			}
			if (type.IsGenericType && !type.IsGenericTypeDefinition)
			{
				System.Type[] genericArguments = type.GetGenericArguments();
				Type[] array = new Type[genericArguments.Length];
				for (int i = 0; i < genericArguments.Length; i++)
				{
					array[i] = Import(genericArguments[i]);
				}
				return Import(type.GetGenericTypeDefinition()).MakeGenericType(array);
			}
			if (type.Assembly == typeof(object).Assembly)
			{
				return ResolveType(Mscorlib, type.FullName);
			}
			return ResolveType(Import(type.Assembly), type.FullName);
		}

		private Assembly Import(System.Reflection.Assembly asm)
		{
			return Load(asm.FullName);
		}

		public RawModule OpenRawModule(string path)
		{
			path = Path.GetFullPath(path);
			FileStream fileStream = null;
			RawModule result;
			try
			{
				fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
				result = OpenRawModule(fileStream, path);
				if (!MetadataOnly)
				{
					fileStream = null;
				}
			}
			finally
			{
				fileStream?.Close();
			}
			return result;
		}

		public RawModule OpenRawModule(Stream stream, string location)
		{
			return OpenRawModule(stream, location, false);
		}

		public RawModule OpenMappedRawModule(Stream stream, string location)
		{
			return OpenRawModule(stream, location, true);
		}

		private RawModule OpenRawModule(Stream stream, string location, bool mapped)
		{
			if (!stream.CanRead || !stream.CanSeek || stream.Position != 0L)
			{
				throw new ArgumentException("Stream must support read/seek and current position must be zero.", "stream");
			}
			return new RawModule(new ModuleReader(null, this, stream, location, mapped));
		}

		public Assembly LoadAssembly(RawModule module)
		{
			string fullName = module.GetAssemblyName().FullName;
			Assembly assembly = GetLoadedAssembly(fullName);
			if (assembly == null)
			{
				AssemblyReader assemblyReader = module.ToAssembly();
				assemblies.Add(assemblyReader);
				assembly = assemblyReader;
			}
			return assembly;
		}

		public Assembly LoadFile(string path)
		{
			try
			{
				using (RawModule module = OpenRawModule(path))
				{
					return LoadAssembly(module);
				}
			}
			catch (IOException ex)
			{
				throw new FileNotFoundException(ex.Message, ex);
			}
			catch (UnauthorizedAccessException ex2)
			{
				throw new FileNotFoundException(ex2.Message, ex2);
			}
		}

		private static string GetSimpleAssemblyName(string refname)
		{
			int pos;
			string simpleName;
			if (Fusion.ParseAssemblySimpleName(refname, out pos, out simpleName) != ParseAssemblyResult.OK)
			{
				throw new ArgumentException();
			}
			return simpleName;
		}

		private Assembly GetLoadedAssembly(string refname)
		{
			Assembly value;
			if (!assembliesByName.TryGetValue(refname, out value))
			{
				string simpleAssemblyName = GetSimpleAssemblyName(refname);
				for (int i = 0; i < assemblies.Count; i++)
				{
					AssemblyComparisonResult result;
					if (simpleAssemblyName.Equals(assemblies[i].Name, StringComparison.OrdinalIgnoreCase) && CompareAssemblyIdentity(refname, false, assemblies[i].FullName, false, out result))
					{
						value = assemblies[i];
						assembliesByName.Add(refname, value);
						break;
					}
				}
			}
			return value;
		}

		private Assembly GetDynamicAssembly(string refname)
		{
			string simpleAssemblyName = GetSimpleAssemblyName(refname);
			foreach (AssemblyBuilder dynamicAssembly in dynamicAssemblies)
			{
				AssemblyComparisonResult result;
				if (simpleAssemblyName.Equals(dynamicAssembly.Name, StringComparison.OrdinalIgnoreCase) && CompareAssemblyIdentity(refname, false, dynamicAssembly.FullName, false, out result))
				{
					return dynamicAssembly;
				}
			}
			return null;
		}

		public Assembly Load(string refname)
		{
			return Load(refname, null, true);
		}

		internal Assembly Load(string refname, Module requestingModule, bool throwOnError)
		{
			Assembly assembly = GetLoadedAssembly(refname);
			if (assembly != null)
			{
				return assembly;
			}
			if (resolvers.Count == 0)
			{
				assembly = DefaultResolver(refname, throwOnError);
			}
			else
			{
				ResolveEventArgs args = new ResolveEventArgs(refname, requestingModule?.Assembly);
				foreach (ResolveEventHandler resolver in resolvers)
				{
					assembly = resolver(this, args);
					if (assembly != null)
					{
						break;
					}
				}
				if (assembly == null)
				{
					assembly = GetDynamicAssembly(refname);
				}
			}
			if (assembly != null)
			{
				string fullName = assembly.FullName;
				if (refname != fullName)
				{
					assembliesByName.Add(refname, assembly);
				}
				return assembly;
			}
			if (throwOnError)
			{
				throw new FileNotFoundException(refname);
			}
			return null;
		}

		private Assembly DefaultResolver(string refname, bool throwOnError)
		{
			Assembly dynamicAssembly = GetDynamicAssembly(refname);
			if (dynamicAssembly != null)
			{
				return dynamicAssembly;
			}
			string location;
			if (throwOnError)
			{
				try
				{
					location = System.Reflection.Assembly.ReflectionOnlyLoad(refname).Location;
				}
				catch (System.BadImageFormatException ex)
				{
					throw new BadImageFormatException(ex.Message, ex);
				}
			}
			else
			{
				try
				{
					location = System.Reflection.Assembly.ReflectionOnlyLoad(refname).Location;
				}
				catch (System.BadImageFormatException ex2)
				{
					throw new BadImageFormatException(ex2.Message, ex2);
				}
				catch (FileNotFoundException)
				{
					return null;
				}
			}
			return LoadFile(location);
		}

		public Type GetType(string assemblyQualifiedTypeName)
		{
			return GetType(null, assemblyQualifiedTypeName, false, false);
		}

		public Type GetType(string assemblyQualifiedTypeName, bool throwOnError)
		{
			return GetType(null, assemblyQualifiedTypeName, throwOnError, false);
		}

		public Type GetType(string assemblyQualifiedTypeName, bool throwOnError, bool ignoreCase)
		{
			return GetType(null, assemblyQualifiedTypeName, throwOnError, ignoreCase);
		}

		public Type GetType(Assembly context, string assemblyQualifiedTypeName, bool throwOnError)
		{
			return GetType(context, assemblyQualifiedTypeName, throwOnError, false);
		}

		public Type GetType(Assembly context, string assemblyQualifiedTypeName, bool throwOnError, bool ignoreCase)
		{
			TypeNameParser typeNameParser = TypeNameParser.Parse(assemblyQualifiedTypeName, throwOnError);
			if (typeNameParser.Error)
			{
				return null;
			}
			return typeNameParser.GetType(this, context?.ManifestModule, throwOnError, assemblyQualifiedTypeName, false, ignoreCase);
		}

		public Type ResolveType(Assembly context, string assemblyQualifiedTypeName)
		{
			TypeNameParser typeNameParser = TypeNameParser.Parse(assemblyQualifiedTypeName, false);
			if (typeNameParser.Error)
			{
				return null;
			}
			return typeNameParser.GetType(this, context?.ManifestModule, false, assemblyQualifiedTypeName, true, false);
		}

		public Type GetBuiltInType(string ns, string name)
		{
			if (ns != "System")
			{
				return null;
			}
			switch (name)
			{
			case "Boolean":
				return System_Boolean;
			case "Char":
				return System_Char;
			case "Object":
				return System_Object;
			case "String":
				return System_String;
			case "Single":
				return System_Single;
			case "Double":
				return System_Double;
			case "SByte":
				return System_SByte;
			case "Int16":
				return System_Int16;
			case "Int32":
				return System_Int32;
			case "Int64":
				return System_Int64;
			case "IntPtr":
				return System_IntPtr;
			case "UIntPtr":
				return System_UIntPtr;
			case "TypedReference":
				return System_TypedReference;
			case "Byte":
				return System_Byte;
			case "UInt16":
				return System_UInt16;
			case "UInt32":
				return System_UInt32;
			case "UInt64":
				return System_UInt64;
			case "Void":
				return System_Void;
			default:
				return null;
			}
		}

		public Assembly[] GetAssemblies()
		{
			Assembly[] array = new Assembly[assemblies.Count + dynamicAssemblies.Count];
			for (int i = 0; i < assemblies.Count; i++)
			{
				array[i] = assemblies[i];
			}
			int num = 0;
			for (int j = assemblies.Count; j < array.Length; j++)
			{
				array[j] = dynamicAssemblies[num];
				num++;
			}
			return array;
		}

		public bool CompareAssemblyIdentity(string assemblyIdentity1, bool unified1, string assemblyIdentity2, bool unified2, out AssemblyComparisonResult result)
		{
			if (!useNativeFusion)
			{
				return Fusion.CompareAssemblyIdentityPure(assemblyIdentity1, unified1, assemblyIdentity2, unified2, out result);
			}
			return Fusion.CompareAssemblyIdentityNative(assemblyIdentity1, unified1, assemblyIdentity2, unified2, out result);
		}

		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access)
		{
			return new AssemblyBuilder(this, name, null, null);
		}

		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, IEnumerable<CustomAttributeBuilder> assemblyAttributes)
		{
			return new AssemblyBuilder(this, name, null, assemblyAttributes);
		}

		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir)
		{
			return new AssemblyBuilder(this, name, dir, null);
		}

		public AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access, string dir, PermissionSet requiredPermissions, PermissionSet optionalPermissions, PermissionSet refusedPermissions)
		{
			AssemblyBuilder assemblyBuilder = new AssemblyBuilder(this, name, dir, null);
			AddLegacyPermissionSet(assemblyBuilder, requiredPermissions, SecurityAction.RequestMinimum);
			AddLegacyPermissionSet(assemblyBuilder, optionalPermissions, SecurityAction.RequestOptional);
			AddLegacyPermissionSet(assemblyBuilder, refusedPermissions, SecurityAction.RequestRefuse);
			return assemblyBuilder;
		}

		private static void AddLegacyPermissionSet(AssemblyBuilder ab, PermissionSet permissionSet, SecurityAction action)
		{
			if (permissionSet != null)
			{
				ab.__AddDeclarativeSecurity(CustomAttributeBuilder.__FromBlob(CustomAttributeBuilder.LegacyPermissionSet, (int)action, Encoding.Unicode.GetBytes(permissionSet.ToXml().ToString())));
			}
		}

		internal void RegisterDynamicAssembly(AssemblyBuilder asm)
		{
			dynamicAssemblies.Add(asm);
		}

		internal void RenameAssembly(Assembly assembly, AssemblyName oldName)
		{
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, Assembly> item in assembliesByName)
			{
				if (item.Value == assembly)
				{
					list.Add(item.Key);
				}
			}
			foreach (string item2 in list)
			{
				assembliesByName.Remove(item2);
			}
		}

		public void Dispose()
		{
			foreach (AssemblyReader assembly in assemblies)
			{
				Module[] loadedModules = assembly.GetLoadedModules();
				for (int i = 0; i < loadedModules.Length; i++)
				{
					loadedModules[i].Dispose();
				}
			}
			foreach (AssemblyBuilder dynamicAssembly in dynamicAssemblies)
			{
				Module[] loadedModules = dynamicAssembly.GetLoadedModules();
				for (int i = 0; i < loadedModules.Length; i++)
				{
					loadedModules[i].Dispose();
				}
			}
		}

		public Assembly CreateMissingAssembly(string assemblyName)
		{
			Assembly assembly = new MissingAssembly(this, assemblyName);
			string fullName = assembly.FullName;
			if (!assembliesByName.ContainsKey(fullName))
			{
				assembliesByName.Add(fullName, assembly);
			}
			return assembly;
		}

		[Obsolete("Please set UniverseOptions.ResolveMissingMembers instead.")]
		public void EnableMissingMemberResolution()
		{
			resolveMissingMembers = true;
		}

		private Type GetMissingType(Module requester, Module module, Type declaringType, TypeName typeName)
		{
			if (missingTypes == null)
			{
				missingTypes = new Dictionary<ScopedTypeName, Type>();
			}
			ScopedTypeName key = new ScopedTypeName(((object)declaringType) ?? ((object)module), typeName);
			Type value;
			if (!missingTypes.TryGetValue(key, out value))
			{
				value = new MissingType(module, declaringType, typeName.Namespace, typeName.Name);
				missingTypes.Add(key, value);
			}
			if (ResolvedMissingMember != null && !module.Assembly.__IsMissing)
			{
				ResolvedMissingMember(requester, value);
			}
			return value;
		}

		internal Type GetMissingTypeOrThrow(Module requester, Module module, Type declaringType, TypeName typeName)
		{
			if (resolveMissingMembers || module.Assembly.__IsMissing)
			{
				return GetMissingType(requester, module, declaringType, typeName);
			}
			string text = TypeNameParser.Escape(typeName.ToString());
			if (declaringType != null)
			{
				text = declaringType.FullName + "+" + text;
			}
			throw new TypeLoadException($"Type '{text}' not found in assembly '{module.Assembly.FullName}'");
		}

		internal MethodBase GetMissingMethodOrThrow(Module requester, Type declaringType, string name, MethodSignature signature)
		{
			if (resolveMissingMembers)
			{
				MethodBase methodBase = new MissingMethod(declaringType, name, signature);
				if (name == ".ctor")
				{
					methodBase = new ConstructorInfoImpl((MethodInfo)methodBase);
				}
				if (ResolvedMissingMember != null)
				{
					ResolvedMissingMember(requester, methodBase);
				}
				return methodBase;
			}
			throw new MissingMethodException(declaringType.ToString(), name);
		}

		internal FieldInfo GetMissingFieldOrThrow(Module requester, Type declaringType, string name, FieldSignature signature)
		{
			if (resolveMissingMembers)
			{
				FieldInfo fieldInfo = new MissingField(declaringType, name, signature);
				if (ResolvedMissingMember != null)
				{
					ResolvedMissingMember(requester, fieldInfo);
				}
				return fieldInfo;
			}
			throw new MissingFieldException(declaringType.ToString(), name);
		}

		internal PropertyInfo GetMissingPropertyOrThrow(Module requester, Type declaringType, string name, PropertySignature propertySignature)
		{
			if (resolveMissingMembers || declaringType.__IsMissing)
			{
				PropertyInfo propertyInfo = new MissingProperty(declaringType, name, propertySignature);
				if (ResolvedMissingMember != null && !declaringType.__IsMissing)
				{
					ResolvedMissingMember(requester, propertyInfo);
				}
				return propertyInfo;
			}
			throw new System.MissingMemberException(declaringType.ToString(), name);
		}

		internal Type CanonicalizeType(Type type)
		{
			Type value;
			if (!canonicalizedTypes.TryGetValue(type, out value))
			{
				value = type;
				Dictionary<Type, Type> dictionary = canonicalizedTypes;
				Type type2 = value;
				dictionary.Add(type2, type2);
			}
			return value;
		}

		public Type MakeFunctionPointer(__StandAloneMethodSig sig)
		{
			return FunctionPointerType.Make(this, sig);
		}

		public __StandAloneMethodSig MakeStandAloneMethodSig(CallingConvention callingConvention, Type returnType, CustomModifiers returnTypeCustomModifiers, Type[] parameterTypes, CustomModifiers[] parameterTypeCustomModifiers)
		{
			return new __StandAloneMethodSig(true, callingConvention, (CallingConventions)0, returnType ?? System_Void, Util.Copy(parameterTypes), Type.EmptyTypes, PackedCustomModifiers.CreateFromExternal(returnTypeCustomModifiers, parameterTypeCustomModifiers, Util.NullSafeLength(parameterTypes)));
		}

		public __StandAloneMethodSig MakeStandAloneMethodSig(CallingConventions callingConvention, Type returnType, CustomModifiers returnTypeCustomModifiers, Type[] parameterTypes, Type[] optionalParameterTypes, CustomModifiers[] parameterTypeCustomModifiers)
		{
			return new __StandAloneMethodSig(false, (CallingConvention)0, callingConvention, returnType ?? System_Void, Util.Copy(parameterTypes), Util.Copy(optionalParameterTypes), PackedCustomModifiers.CreateFromExternal(returnTypeCustomModifiers, parameterTypeCustomModifiers, Util.NullSafeLength(parameterTypes) + Util.NullSafeLength(optionalParameterTypes)));
		}

		public static Universe FromAssembly(Assembly assembly)
		{
			return assembly.universe;
		}

		internal bool ResolveMissingTypeIsValueType(MissingType missingType)
		{
			if (missingTypeIsValueType != null)
			{
				return missingTypeIsValueType(missingType);
			}
			throw new MissingMemberException(missingType);
		}
	}
}
