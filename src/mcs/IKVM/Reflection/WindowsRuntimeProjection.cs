using System;
using System.Collections.Generic;
using IKVM.Reflection.Metadata;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	internal sealed class WindowsRuntimeProjection
	{
		private enum ProjectionAssembly
		{
			System_Runtime = 0,
			System_Runtime_InteropServices_WindowsRuntime = 1,
			System_ObjectModel = 2,
			System_Runtime_WindowsRuntime = 3,
			System_Runtime_WindowsRuntime_UI_Xaml = 4,
			Count = 5
		}

		private sealed class Mapping
		{
			internal readonly ProjectionAssembly Assembly;

			internal readonly string TypeNamespace;

			internal readonly string TypeName;

			internal Mapping(ProjectionAssembly assembly, string typeNamespace, string typeName)
			{
				Assembly = assembly;
				TypeNamespace = typeNamespace;
				TypeName = typeName;
			}
		}

		private static readonly Dictionary<TypeName, Mapping> projections;

		private readonly ModuleReader module;

		private readonly Dictionary<int, string> strings;

		private readonly Dictionary<string, int> added = new Dictionary<string, int>();

		private readonly int[] assemblyRefTokens = new int[5];

		private int typeofSystemAttribute = -1;

		private int typeofSystemAttributeUsageAttribute = -1;

		private int typeofSystemEnum = -1;

		private int typeofSystemIDisposable = -1;

		private int typeofSystemMulticastDelegate = -1;

		private int typeofWindowsFoundationMetadataAllowMultipleAttribute = -1;

		private bool[] projectedTypeRefs;

		static WindowsRuntimeProjection()
		{
			projections = new Dictionary<TypeName, Mapping>();
			projections.Add(new TypeName("System", "Attribute"), new Mapping(ProjectionAssembly.System_Runtime, "System", "Attribute"));
			projections.Add(new TypeName("System", "MulticastDelegate"), new Mapping(ProjectionAssembly.System_Runtime, "System", "MulticastDelegate"));
			projections.Add(new TypeName("Windows.Foundation", "DateTime"), new Mapping(ProjectionAssembly.System_Runtime, "System", "DateTimeOffset"));
			projections.Add(new TypeName("Windows.Foundation", "EventHandler`1"), new Mapping(ProjectionAssembly.System_Runtime, "System", "EventHandler`1"));
			projections.Add(new TypeName("Windows.Foundation", "EventRegistrationToken"), new Mapping(ProjectionAssembly.System_Runtime_InteropServices_WindowsRuntime, "System.Runtime.InteropServices.WindowsRuntime", "EventRegistrationToken"));
			projections.Add(new TypeName("Windows.Foundation", "HResult"), new Mapping(ProjectionAssembly.System_Runtime, "System", "Exception"));
			projections.Add(new TypeName("Windows.Foundation", "IClosable"), new Mapping(ProjectionAssembly.System_Runtime, "System", "IDisposable"));
			projections.Add(new TypeName("Windows.Foundation", "IReference`1"), new Mapping(ProjectionAssembly.System_Runtime, "System", "Nullable`1"));
			projections.Add(new TypeName("Windows.Foundation", "Point"), new Mapping(ProjectionAssembly.System_Runtime_WindowsRuntime, "Windows.Foundation", "Point"));
			projections.Add(new TypeName("Windows.Foundation", "Rect"), new Mapping(ProjectionAssembly.System_Runtime_WindowsRuntime, "Windows.Foundation", "Rect"));
			projections.Add(new TypeName("Windows.Foundation", "Size"), new Mapping(ProjectionAssembly.System_Runtime_WindowsRuntime, "Windows.Foundation", "Size"));
			projections.Add(new TypeName("Windows.Foundation", "TimeSpan"), new Mapping(ProjectionAssembly.System_Runtime, "System", "TimeSpan"));
			projections.Add(new TypeName("Windows.Foundation", "Uri"), new Mapping(ProjectionAssembly.System_Runtime, "System", "Uri"));
			projections.Add(new TypeName("Windows.Foundation.Collections", "IIterable`1"), new Mapping(ProjectionAssembly.System_Runtime, "System.Collections.Generic", "IEnumerable`1"));
			projections.Add(new TypeName("Windows.Foundation.Collections", "IKeyValuePair`2"), new Mapping(ProjectionAssembly.System_Runtime, "System.Collections.Generic", "KeyValuePair`2"));
			projections.Add(new TypeName("Windows.Foundation.Collections", "IMap`2"), new Mapping(ProjectionAssembly.System_Runtime, "System.Collections.Generic", "IDictionary`2"));
			projections.Add(new TypeName("Windows.Foundation.Collections", "IMapView`2"), new Mapping(ProjectionAssembly.System_Runtime, "System.Collections.Generic", "IReadOnlyDictionary`2"));
			projections.Add(new TypeName("Windows.Foundation.Collections", "IVector`1"), new Mapping(ProjectionAssembly.System_Runtime, "System.Collections.Generic", "IList`1"));
			projections.Add(new TypeName("Windows.Foundation.Collections", "IVectorView`1"), new Mapping(ProjectionAssembly.System_Runtime, "System.Collections.Generic", "IReadOnlyList`1"));
			projections.Add(new TypeName("Windows.Foundation.Metadata", "AttributeTargets"), new Mapping(ProjectionAssembly.System_Runtime, "System", "AttributeTargets"));
			projections.Add(new TypeName("Windows.Foundation.Metadata", "AttributeUsageAttribute"), new Mapping(ProjectionAssembly.System_Runtime, "System", "AttributeUsageAttribute"));
			projections.Add(new TypeName("Windows.UI", "Color"), new Mapping(ProjectionAssembly.System_Runtime_WindowsRuntime, "Windows.UI", "Color"));
			projections.Add(new TypeName("Windows.UI.Xaml", "CornerRadius"), new Mapping(ProjectionAssembly.System_Runtime_WindowsRuntime_UI_Xaml, "Windows.UI.Xaml", "CornerRadius"));
			projections.Add(new TypeName("Windows.UI.Xaml", "Duration"), new Mapping(ProjectionAssembly.System_Runtime_WindowsRuntime_UI_Xaml, "Windows.UI.Xaml", "Duration"));
			projections.Add(new TypeName("Windows.UI.Xaml", "DurationType"), new Mapping(ProjectionAssembly.System_Runtime_WindowsRuntime_UI_Xaml, "Windows.UI.Xaml", "DurationType"));
			projections.Add(new TypeName("Windows.UI.Xaml", "GridLength"), new Mapping(ProjectionAssembly.System_Runtime_WindowsRuntime_UI_Xaml, "Windows.UI.Xaml", "GridLength"));
			projections.Add(new TypeName("Windows.UI.Xaml", "GridUnitType"), new Mapping(ProjectionAssembly.System_Runtime_WindowsRuntime_UI_Xaml, "Windows.UI.Xaml", "GridUnitType"));
			projections.Add(new TypeName("Windows.UI.Xaml", "Thickness"), new Mapping(ProjectionAssembly.System_Runtime_WindowsRuntime_UI_Xaml, "Windows.UI.Xaml", "Thickness"));
			projections.Add(new TypeName("Windows.UI.Xaml.Controls.Primitives", "GeneratorPosition"), new Mapping(ProjectionAssembly.System_Runtime_WindowsRuntime_UI_Xaml, "Windows.UI.Xaml.Controls.Primitives", "GeneratorPosition"));
			projections.Add(new TypeName("Windows.UI.Xaml.Data", "INotifyPropertyChanged"), new Mapping(ProjectionAssembly.System_ObjectModel, "System.ComponentModel", "INotifyPropertyChanged"));
			projections.Add(new TypeName("Windows.UI.Xaml.Data", "PropertyChangedEventArgs"), new Mapping(ProjectionAssembly.System_ObjectModel, "System.ComponentModel", "PropertyChangedEventArgs"));
			projections.Add(new TypeName("Windows.UI.Xaml.Data", "PropertyChangedEventHandler"), new Mapping(ProjectionAssembly.System_ObjectModel, "System.ComponentModel", "PropertyChangedEventHandler"));
			projections.Add(new TypeName("Windows.UI.Xaml.Input", "ICommand"), new Mapping(ProjectionAssembly.System_ObjectModel, "System.Windows.Input", "ICommand"));
			projections.Add(new TypeName("Windows.UI.Xaml.Interop", "IBindableIterable"), new Mapping(ProjectionAssembly.System_Runtime, "System.Collections", "IEnumerable"));
			projections.Add(new TypeName("Windows.UI.Xaml.Interop", "IBindableVector"), new Mapping(ProjectionAssembly.System_Runtime, "System.Collections", "IList"));
			projections.Add(new TypeName("Windows.UI.Xaml.Interop", "NotifyCollectionChangedAction"), new Mapping(ProjectionAssembly.System_ObjectModel, "System.Collections.Specialized", "NotifyCollectionChangedAction"));
			projections.Add(new TypeName("Windows.UI.Xaml.Interop", "NotifyCollectionChangedEventArgs"), new Mapping(ProjectionAssembly.System_ObjectModel, "System.Collections.Specialized", "NotifyCollectionChangedEventArgs"));
			projections.Add(new TypeName("Windows.UI.Xaml.Interop", "NotifyCollectionChangedEventHandler"), new Mapping(ProjectionAssembly.System_ObjectModel, "System.Collections.Specialized", "NotifyCollectionChangedEventHandler"));
			projections.Add(new TypeName("Windows.UI.Xaml.Interop", "TypeName"), new Mapping(ProjectionAssembly.System_Runtime, "System", "Type"));
			projections.Add(new TypeName("Windows.UI.Xaml.Media", "Matrix"), new Mapping(ProjectionAssembly.System_Runtime_WindowsRuntime_UI_Xaml, "Windows.UI.Xaml.Media", "Matrix"));
			projections.Add(new TypeName("Windows.UI.Xaml.Media.Animation", "KeyTime"), new Mapping(ProjectionAssembly.System_Runtime_WindowsRuntime_UI_Xaml, "Windows.UI.Xaml.Media.Animation", "KeyTime"));
			projections.Add(new TypeName("Windows.UI.Xaml.Media.Animation", "RepeatBehavior"), new Mapping(ProjectionAssembly.System_Runtime_WindowsRuntime_UI_Xaml, "Windows.UI.Xaml.Media.Animation", "RepeatBehavior"));
			projections.Add(new TypeName("Windows.UI.Xaml.Media.Animation", "RepeatBehaviorType"), new Mapping(ProjectionAssembly.System_Runtime_WindowsRuntime_UI_Xaml, "Windows.UI.Xaml.Media.Animation", "RepeatBehaviorType"));
			projections.Add(new TypeName("Windows.UI.Xaml.Media.Media3D", "Matrix3D"), new Mapping(ProjectionAssembly.System_Runtime_WindowsRuntime_UI_Xaml, "Windows.UI.Xaml.Media.Media3D", "Matrix3D"));
			projections.Add(new TypeName("Windows.Foundation", "IPropertyValue"), null);
			projections.Add(new TypeName("Windows.Foundation", "IReferenceArray`1"), null);
			projections.Add(new TypeName("Windows.Foundation.Metadata", "GCPressureAmount"), null);
			projections.Add(new TypeName("Windows.Foundation.Metadata", "GCPressureAttribute"), null);
			projections.Add(new TypeName("Windows.UI.Xaml", "CornerRadiusHelper"), null);
			projections.Add(new TypeName("Windows.UI.Xaml", "DurationHelper"), null);
			projections.Add(new TypeName("Windows.UI.Xaml", "GridLengthHelper"), null);
			projections.Add(new TypeName("Windows.UI.Xaml", "ThicknessHelper"), null);
			projections.Add(new TypeName("Windows.UI.Xaml.Controls.Primitives", "GeneratorPositionHelper"), null);
			projections.Add(new TypeName("Windows.UI.Xaml.Interop", "INotifyCollectionChanged"), null);
			projections.Add(new TypeName("Windows.UI.Xaml.Media", "MatrixHelper"), null);
			projections.Add(new TypeName("Windows.UI.Xaml.Media.Animation", "KeyTimeHelper"), null);
			projections.Add(new TypeName("Windows.UI.Xaml.Media.Animation", "RepeatBehaviorHelper"), null);
			projections.Add(new TypeName("Windows.UI.Xaml.Media.Media3D", "Matrix3DHelper"), null);
		}

		private WindowsRuntimeProjection(ModuleReader module, Dictionary<int, string> strings)
		{
			this.module = module;
			this.strings = strings;
		}

		internal static void Patch(ModuleReader module, Dictionary<int, string> strings, ref string imageRuntimeVersion, ref byte[] blobHeap)
		{
			if (!module.CustomAttribute.Sorted)
			{
				throw new NotImplementedException("CustomAttribute table must be sorted");
			}
			bool flag = imageRuntimeVersion.Contains(";");
			if (flag)
			{
				string obj = imageRuntimeVersion;
				imageRuntimeVersion = obj.Substring(obj.IndexOf(';') + 1);
				if (imageRuntimeVersion.StartsWith("CLR", StringComparison.OrdinalIgnoreCase))
				{
					imageRuntimeVersion = imageRuntimeVersion.Substring(3);
				}
				imageRuntimeVersion = imageRuntimeVersion.TrimStart(' ');
			}
			else
			{
				Assembly mscorlib = module.universe.Mscorlib;
				imageRuntimeVersion = (mscorlib.__IsMissing ? "v4.0.30319" : mscorlib.ImageRuntimeVersion);
			}
			WindowsRuntimeProjection windowsRuntimeProjection = new WindowsRuntimeProjection(module, strings);
			windowsRuntimeProjection.PatchAssemblyRef(ref blobHeap);
			windowsRuntimeProjection.PatchTypeRef();
			windowsRuntimeProjection.PatchTypes(flag);
			windowsRuntimeProjection.PatchMethodImpl();
			windowsRuntimeProjection.PatchCustomAttribute(ref blobHeap);
		}

		private void PatchAssemblyRef(ref byte[] blobHeap)
		{
			AssemblyRefTable assemblyRef = module.AssemblyRef;
			for (int i = 0; i < assemblyRef.records.Length; i++)
			{
				if (module.GetString(assemblyRef.records[i].Name) == "mscorlib")
				{
					Version mscorlibVersion = GetMscorlibVersion();
					assemblyRef.records[i].MajorVersion = (ushort)mscorlibVersion.Major;
					assemblyRef.records[i].MinorVersion = (ushort)mscorlibVersion.Minor;
					assemblyRef.records[i].BuildNumber = (ushort)mscorlibVersion.Build;
					assemblyRef.records[i].RevisionNumber = (ushort)mscorlibVersion.Revision;
					break;
				}
			}
			int publicKeyToken = AddBlob(ref blobHeap, new byte[8] { 176, 63, 95, 127, 17, 213, 10, 58 });
			int publicKeyToken2 = AddBlob(ref blobHeap, new byte[8] { 183, 122, 92, 86, 25, 52, 224, 137 });
			assemblyRefTokens[0] = AddAssemblyReference("System.Runtime", publicKeyToken);
			assemblyRefTokens[1] = AddAssemblyReference("System.Runtime.InteropServices.WindowsRuntime", publicKeyToken);
			assemblyRefTokens[2] = AddAssemblyReference("System.ObjectModel", publicKeyToken);
			assemblyRefTokens[3] = AddAssemblyReference("System.Runtime.WindowsRuntime", publicKeyToken2);
			assemblyRefTokens[4] = AddAssemblyReference("System.Runtime.WindowsRuntime.UI.Xaml", publicKeyToken2);
		}

		private void PatchTypeRef()
		{
			TypeRefTable.Record[] records = module.TypeRef.records;
			projectedTypeRefs = new bool[records.Length];
			for (int i = 0; i < records.Length; i++)
			{
				TypeName typeRefName = GetTypeRefName(i);
				Mapping value;
				projections.TryGetValue(typeRefName, out value);
				if (value != null)
				{
					records[i].ResolutionScope = assemblyRefTokens[(int)value.Assembly];
					records[i].TypeNamespace = GetString(value.TypeNamespace);
					records[i].TypeName = GetString(value.TypeName);
					projectedTypeRefs[i] = true;
				}
				switch (typeRefName.Namespace)
				{
				case "System":
					switch (typeRefName.Name)
					{
					case "Attribute":
						typeofSystemAttribute = (1 << 24) + i + 1;
						break;
					case "Enum":
						typeofSystemEnum = (1 << 24) + i + 1;
						break;
					case "MulticastDelegate":
						typeofSystemMulticastDelegate = (1 << 24) + i + 1;
						break;
					}
					break;
				case "Windows.Foundation":
				{
					string name = typeRefName.Name;
					if (name == "IClosable")
					{
						typeofSystemIDisposable = (1 << 24) + i + 1;
					}
					break;
				}
				case "Windows.Foundation.Metadata":
				{
					string name = typeRefName.Name;
					if (!(name == "AllowMultipleAttribute"))
					{
						if (name == "AttributeUsageAttribute")
						{
							typeofSystemAttributeUsageAttribute = (1 << 24) + i + 1;
						}
					}
					else
					{
						typeofWindowsFoundationMetadataAllowMultipleAttribute = (1 << 24) + i + 1;
					}
					break;
				}
				}
			}
		}

		private void PatchTypes(bool clr)
		{
			TypeDefTable.Record[] records = module.TypeDef.records;
			MethodDefTable.Record[] records2 = module.MethodDef.records;
			FieldTable.Record[] records3 = module.Field.records;
			for (int i = 0; i < records.Length; i++)
			{
				TypeAttributes flags = (TypeAttributes)records[i].Flags;
				if ((flags & TypeAttributes.WindowsRuntime) != TypeAttributes.AnsiClass)
				{
					if (clr && (flags & (TypeAttributes.VisibilityMask | TypeAttributes.ClassSemanticsMask | TypeAttributes.WindowsRuntime)) == (TypeAttributes.Public | TypeAttributes.WindowsRuntime))
					{
						records[i].TypeName = GetString("<WinRT>" + module.GetString(records[i].TypeName));
						records[i].Flags &= -2;
					}
					if (records[i].Extends != typeofSystemAttribute && (!clr || (flags & TypeAttributes.ClassSemanticsMask) == 0))
					{
						records[i].Flags |= 4096;
					}
					if (projections.ContainsKey(GetTypeDefName(i)))
					{
						records[i].Flags &= -2;
					}
					int num = ((i == records.Length - 1) ? records2.Length : (records[i + 1].MethodList - 1));
					for (int j = records[i].MethodList - 1; j < num; j++)
					{
						if (records[i].Extends == typeofSystemMulticastDelegate)
						{
							if (module.GetString(records2[j].Name) == ".ctor")
							{
								records2[j].Flags &= -8;
								records2[j].Flags |= 6;
							}
						}
						else if (records2[j].RVA == 0)
						{
							records2[j].ImplFlags = 4099;
						}
					}
					if (records[i].Extends == typeofSystemEnum)
					{
						int num2 = ((i == records.Length - 1) ? records3.Length : (records[i + 1].FieldList - 1));
						for (int k = records[i].FieldList - 1; k < num2; k++)
						{
							records3[k].Flags &= -8;
							records3[k].Flags |= 6;
						}
					}
				}
				else if (clr && (flags & (TypeAttributes.VisibilityMask | TypeAttributes.SpecialName)) == TypeAttributes.SpecialName)
				{
					string text = module.GetString(records[i].TypeName);
					if (text.StartsWith("<CLR>", StringComparison.Ordinal))
					{
						records[i].TypeName = GetString(text.Substring(5));
						records[i].Flags |= 1;
						records[i].Flags &= -1025;
					}
				}
			}
		}

		private void PatchMethodImpl()
		{
			MethodImplTable.Record[] records = module.MethodImpl.records;
			MemberRefTable.Record[] records2 = module.MemberRef.records;
			MethodDefTable.Record[] records3 = module.MethodDef.records;
			int[] records4 = module.TypeSpec.records;
			for (int i = 0; i < records.Length; i++)
			{
				int methodDeclaration = records[i].MethodDeclaration;
				if (methodDeclaration >> 24 != 10)
				{
					continue;
				}
				int num = records2[(methodDeclaration & 0xFFFFFF) - 1].Class;
				if (num >> 24 == 27)
				{
					num = ReadTypeSpec(module.GetBlob(records4[(num & 0xFFFFFF) - 1]));
				}
				if (num >> 24 == 1)
				{
					if (num == typeofSystemIDisposable)
					{
						int name = GetString("Dispose");
						records3[(records[i].MethodBody & 0xFFFFFF) - 1].Name = name;
						records2[(records[i].MethodDeclaration & 0xFFFFFF) - 1].Name = name;
					}
					else if (projectedTypeRefs[(num & 0xFFFFFF) - 1])
					{
						records3[(records[i].MethodBody & 0xFFFFFF) - 1].Flags &= -8;
						records3[(records[i].MethodBody & 0xFFFFFF) - 1].Flags |= 1;
						records[i].MethodBody = 0;
						records[i].MethodDeclaration = 0;
					}
				}
				else if (num >> 24 != 2)
				{
					if (num >> 24 == 27)
					{
						throw new NotImplementedException();
					}
					throw new BadImageFormatException();
				}
			}
		}

		private void PatchCustomAttribute(ref byte[] blobHeap)
		{
			MemberRefTable.Record[] records = module.MemberRef.records;
			int num = -1;
			int ctorWindowsFoundationMetadataAllowMultipleAttribute = -1;
			for (int i = 0; i < records.Length; i++)
			{
				if (records[i].Class == typeofSystemAttributeUsageAttribute && module.GetString(records[i].Name) == ".ctor")
				{
					num = (10 << 24) + i + 1;
				}
				else if (records[i].Class == typeofWindowsFoundationMetadataAllowMultipleAttribute && module.GetString(records[i].Name) == ".ctor")
				{
					ctorWindowsFoundationMetadataAllowMultipleAttribute = (10 << 24) + i + 1;
				}
			}
			if (num == -1)
			{
				return;
			}
			CustomAttributeTable.Record[] records2 = module.CustomAttribute.records;
			Dictionary<int, int> map = new Dictionary<int, int>();
			for (int j = 0; j < records2.Length; j++)
			{
				if (records2[j].Type != num)
				{
					continue;
				}
				ByteReader blob = module.GetBlob(records2[j].Value);
				blob.ReadInt16();
				AttributeTargets attributeTargets = MapAttributeTargets(blob.ReadInt32());
				if ((attributeTargets & AttributeTargets.Method) != 0)
				{
					attributeTargets |= AttributeTargets.Constructor;
					if (records2[j].Parent >> 24 == 2)
					{
						TypeName typeDefName = GetTypeDefName((records2[j].Parent & 0xFFFFFF) - 1);
						if (typeDefName.Namespace == "Windows.Foundation.Metadata" && (typeDefName.Name == "OverloadAttribute" || typeDefName.Name == "DefaultOverloadAttribute"))
						{
							attributeTargets &= ~AttributeTargets.Constructor;
						}
					}
				}
				records2[j].Value = GetAttributeUsageAttributeBlob(ref blobHeap, map, attributeTargets, HasAllowMultipleAttribute(records2, j, ctorWindowsFoundationMetadataAllowMultipleAttribute));
			}
		}

		private int AddAssemblyReference(string name, int publicKeyToken)
		{
			Version mscorlibVersion = GetMscorlibVersion();
			AssemblyRefTable.Record rec = default(AssemblyRefTable.Record);
			rec.MajorVersion = (ushort)mscorlibVersion.Major;
			rec.MinorVersion = (ushort)mscorlibVersion.Minor;
			rec.BuildNumber = (ushort)mscorlibVersion.Build;
			rec.RevisionNumber = (ushort)mscorlibVersion.Revision;
			rec.Flags = 0;
			rec.PublicKeyOrToken = publicKeyToken;
			rec.Name = GetString(name);
			rec.Culture = 0;
			rec.HashValue = 0;
			int result = 0x23000000 | module.AssemblyRef.FindOrAddRecord(rec);
			Array.Resize(ref module.AssemblyRef.records, module.AssemblyRef.RowCount);
			return result;
		}

		private TypeName GetTypeRefName(int index)
		{
			return new TypeName(module.GetString(module.TypeRef.records[index].TypeNamespace), module.GetString(module.TypeRef.records[index].TypeName));
		}

		private TypeName GetTypeDefName(int index)
		{
			return new TypeName(module.GetString(module.TypeDef.records[index].TypeNamespace), module.GetString(module.TypeDef.records[index].TypeName));
		}

		private int GetString(string str)
		{
			int value;
			if (!added.TryGetValue(str, out value))
			{
				value = -(added.Count + 1);
				added.Add(str, value);
				strings.Add(value, str);
			}
			return value;
		}

		private Version GetMscorlibVersion()
		{
			Assembly mscorlib = module.universe.Mscorlib;
			if (!mscorlib.__IsMissing)
			{
				return mscorlib.GetName().Version;
			}
			return new Version(4, 0, 0, 0);
		}

		private static bool HasAllowMultipleAttribute(CustomAttributeTable.Record[] customAttributes, int i, int ctorWindowsFoundationMetadataAllowMultipleAttribute)
		{
			int parent = customAttributes[i].Parent;
			while (i > 0 && customAttributes[i - 1].Parent == parent)
			{
				i--;
			}
			while (i < customAttributes.Length && customAttributes[i].Parent == parent)
			{
				if (customAttributes[i].Type == ctorWindowsFoundationMetadataAllowMultipleAttribute)
				{
					return true;
				}
				i++;
			}
			return false;
		}

		private static AttributeTargets MapAttributeTargets(int targets)
		{
			if (targets == -1)
			{
				return AttributeTargets.All;
			}
			AttributeTargets attributeTargets = (AttributeTargets)0;
			if ((targets & 1) != 0)
			{
				attributeTargets |= AttributeTargets.Delegate;
			}
			if ((targets & 2) != 0)
			{
				attributeTargets |= AttributeTargets.Enum;
			}
			if ((targets & 4) != 0)
			{
				attributeTargets |= AttributeTargets.Event;
			}
			if ((targets & 8) != 0)
			{
				attributeTargets |= AttributeTargets.Field;
			}
			if ((targets & 0x10) != 0)
			{
				attributeTargets |= AttributeTargets.Interface;
			}
			if ((targets & 0x40) != 0)
			{
				attributeTargets |= AttributeTargets.Method;
			}
			if ((targets & 0x80) != 0)
			{
				attributeTargets |= AttributeTargets.Parameter;
			}
			if ((targets & 0x100) != 0)
			{
				attributeTargets |= AttributeTargets.Property;
			}
			if ((targets & 0x200) != 0)
			{
				attributeTargets |= AttributeTargets.Class;
			}
			if ((targets & 0x400) != 0)
			{
				attributeTargets |= AttributeTargets.Struct;
			}
			return attributeTargets;
		}

		private static int GetAttributeUsageAttributeBlob(ref byte[] blobHeap, Dictionary<int, int> map, AttributeTargets targets, bool allowMultiple)
		{
			int num = (int)targets;
			if (allowMultiple)
			{
				num |= int.MinValue;
			}
			int value;
			if (!map.TryGetValue(num, out value))
			{
				byte[] obj = new byte[25]
				{
					1, 0, 0, 0, 0, 0, 1, 0, 84, 2,
					13, 65, 108, 108, 111, 119, 77, 117, 108, 116,
					105, 112, 108, 101, 0
				};
				obj[2] = (byte)targets;
				obj[3] = (byte)((int)targets >> 8);
				obj[4] = (byte)((int)targets >> 16);
				obj[5] = (byte)((int)targets >> 24);
				obj[24] = (byte)(allowMultiple ? 1 : 0);
				value = AddBlob(ref blobHeap, obj);
				map.Add(num, value);
			}
			return value;
		}

		private static int ReadTypeSpec(ByteReader br)
		{
			if (br.ReadByte() != 21)
			{
				throw new NotImplementedException("Expected ELEMENT_TYPE_GENERICINST");
			}
			byte b = br.ReadByte();
			if (b != 17 && b != 18)
			{
				throw new NotImplementedException("Expected ELEMENT_TYPE_CLASS or ELEMENT_TYPE_VALUETYPE");
			}
			int num = br.ReadCompressedUInt();
			switch (num & 3)
			{
			case 0:
				return (2 << 24) + (num >> 2);
			case 1:
				return (1 << 24) + (num >> 2);
			case 2:
				return (27 << 24) + (num >> 2);
			default:
				throw new BadImageFormatException();
			}
		}

		private static int AddBlob(ref byte[] blobHeap, byte[] blob)
		{
			if (blob.Length > 127)
			{
				throw new NotImplementedException();
			}
			int num = blobHeap.Length;
			Array.Resize(ref blobHeap, num + blob.Length + 1);
			blobHeap[num] = (byte)blob.Length;
			Buffer.BlockCopy(blob, 0, blobHeap, num + 1, blob.Length);
			return num;
		}

		internal static bool IsProjectedValueType(string ns, string name, Module module)
		{
			if ((ns == "System.Collections.Generic" && name == "KeyValuePair`2") || (ns == "System" && name == "Nullable`1"))
			{
				return module.Assembly.GetName().Name == "System.Runtime";
			}
			return false;
		}

		internal static bool IsProjectedReferenceType(string ns, string name, Module module)
		{
			if ((ns == "System" && name == "Exception") || (ns == "System" && name == "Type"))
			{
				return module.Assembly.GetName().Name == "System.Runtime";
			}
			return false;
		}
	}
}
