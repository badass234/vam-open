using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;
using Mono.CompilerServices.SymbolWriter;
using Mono.Security.Cryptography;

namespace Mono.CSharp
{
	public abstract class AssemblyDefinition : IAssemblyDefinition
	{
		public AssemblyBuilder Builder;

		protected AssemblyBuilderExtension builder_extra;

		private MonoSymbolFile symbol_writer;

		private bool is_cls_compliant;

		private bool wrap_non_exception_throws;

		private bool wrap_non_exception_throws_custom;

		private bool has_user_debuggable;

		protected ModuleContainer module;

		private readonly string name;

		protected readonly string file_name;

		private byte[] public_key;

		private byte[] public_key_token;

		private bool delay_sign;

		private StrongNameKeyPair private_key;

		private Attribute cls_attribute;

		private Method entry_point;

		protected List<ImportedModuleDefinition> added_modules;

		private Dictionary<SecurityAction, PermissionSet> declarative_security;

		private Dictionary<ITypeDefinition, Attribute> emitted_forwarders;

		private AssemblyAttributesPlaceholder module_target_attrs;

		private string vi_product;

		private string vi_product_version;

		private string vi_company;

		private string vi_copyright;

		private string vi_trademark;

		public Attribute CLSCompliantAttribute => cls_attribute;

		public CompilerContext Compiler => module.Compiler;

		public Method EntryPoint
		{
			get
			{
				return entry_point;
			}
			set
			{
				entry_point = value;
			}
		}

		public string FullName => Builder.FullName;

		public bool HasCLSCompliantAttribute => cls_attribute != null;

		public MetadataImporter Importer { get; set; }

		public bool IsCLSCompliant => is_cls_compliant;

		bool IAssemblyDefinition.IsMissing => false;

		public bool IsSatelliteAssembly { get; private set; }

		public string Name => name;

		public bool WrapNonExceptionThrows => wrap_non_exception_throws;

		protected Report Report => Compiler.Report;

		public MonoSymbolFile SymbolWriter => symbol_writer;

		protected AssemblyDefinition(ModuleContainer module, string name)
		{
			this.module = module;
			this.name = Path.GetFileNameWithoutExtension(name);
			wrap_non_exception_throws = true;
			delay_sign = Compiler.Settings.StrongNameDelaySign;
			if (Compiler.Settings.HasKeyFileOrContainer)
			{
				LoadPublicKey(Compiler.Settings.StrongNameKeyFile, Compiler.Settings.StrongNameKeyContainer);
			}
		}

		protected AssemblyDefinition(ModuleContainer module, string name, string fileName)
			: this(module, name)
		{
			file_name = fileName;
		}

		public void AddModule(ImportedModuleDefinition module)
		{
			if (added_modules == null)
			{
				added_modules = new List<ImportedModuleDefinition>();
				added_modules.Add(module);
			}
		}

		public void ApplyAttributeBuilder(Attribute a, MethodSpec ctor, byte[] cdata, PredefinedAttributes pa)
		{
			if (a.IsValidSecurityAttribute())
			{
				a.ExtractSecurityPermissionSet(ctor, ref declarative_security);
				return;
			}
			if (a.Type == pa.AssemblyCulture)
			{
				string text = a.GetString();
				if (text == null || text.Length == 0)
				{
					return;
				}
				if (Compiler.Settings.Target == Target.Exe)
				{
					Report.Error(7059, a.Location, "Executables cannot be satellite assemblies. Remove the attribute or keep it empty");
					return;
				}
				if (text == "neutral")
				{
					text = "";
				}
				if (Compiler.Settings.Target == Target.Module)
				{
					SetCustomAttribute(ctor, cdata);
				}
				else
				{
					builder_extra.SetCulture(text, a.Location);
				}
				IsSatelliteAssembly = true;
				return;
			}
			if (a.Type == pa.AssemblyVersion)
			{
				string text2 = a.GetString();
				if (text2 != null && text2.Length != 0)
				{
					Version version = IsValidAssemblyVersion(text2, true);
					if (version == null)
					{
						Report.Error(7034, a.Location, "The specified version string `{0}' does not conform to the required format - major[.minor[.build[.revision]]]", text2);
					}
					else if (Compiler.Settings.Target == Target.Module)
					{
						SetCustomAttribute(ctor, cdata);
					}
					else
					{
						builder_extra.SetVersion(version, a.Location);
					}
				}
				return;
			}
			if (a.Type == pa.AssemblyAlgorithmId)
			{
				uint num = cdata[2];
				num |= (uint)(cdata[3] << 8);
				num |= (uint)(cdata[4] << 16);
				num |= (uint)(cdata[5] << 24);
				if (Compiler.Settings.Target == Target.Module)
				{
					SetCustomAttribute(ctor, cdata);
				}
				else
				{
					builder_extra.SetAlgorithmId(num, a.Location);
				}
				return;
			}
			if (a.Type == pa.AssemblyFlags)
			{
				uint num2 = cdata[2];
				num2 |= (uint)(cdata[3] << 8);
				num2 |= (uint)(cdata[4] << 16);
				num2 |= (uint)(cdata[5] << 24);
				if ((num2 & 1) != 0 && public_key == null)
				{
					num2 &= 0xFFFFFFFEu;
				}
				if (Compiler.Settings.Target == Target.Module)
				{
					SetCustomAttribute(ctor, cdata);
				}
				else
				{
					builder_extra.SetFlags(num2, a.Location);
				}
				return;
			}
			if (a.Type == pa.TypeForwarder)
			{
				TypeSpec argumentType = a.GetArgumentType();
				if (argumentType == null || TypeManager.HasElementType(argumentType))
				{
					Report.Error(735, a.Location, "Invalid type specified as an argument for TypeForwardedTo attribute");
					return;
				}
				if (emitted_forwarders == null)
				{
					emitted_forwarders = new Dictionary<ITypeDefinition, Attribute>();
				}
				else if (emitted_forwarders.ContainsKey(argumentType.MemberDefinition))
				{
					Report.SymbolRelatedToPreviousError(emitted_forwarders[argumentType.MemberDefinition].Location, null);
					Report.Error(739, a.Location, "A duplicate type forward of type `{0}'", argumentType.GetSignatureForError());
					return;
				}
				emitted_forwarders.Add(argumentType.MemberDefinition, a);
				if (argumentType.MemberDefinition.DeclaringAssembly == this)
				{
					Report.SymbolRelatedToPreviousError(argumentType);
					Report.Error(729, a.Location, "Cannot forward type `{0}' because it is defined in this assembly", argumentType.GetSignatureForError());
				}
				else if (argumentType.IsNested)
				{
					Report.Error(730, a.Location, "Cannot forward type `{0}' because it is a nested type", argumentType.GetSignatureForError());
				}
				else
				{
					builder_extra.AddTypeForwarder(argumentType.GetDefinition(), a.Location);
				}
				return;
			}
			if (a.Type == pa.Extension)
			{
				a.Error_MisusedExtensionAttribute();
				return;
			}
			if (a.Type == pa.InternalsVisibleTo)
			{
				string text3 = a.GetString();
				if (text3 == null)
				{
					Report.Error(7030, a.Location, "Friend assembly reference cannot have `null' value");
					return;
				}
				if (text3.Length == 0)
				{
					return;
				}
			}
			else if (a.Type == pa.RuntimeCompatibility)
			{
				wrap_non_exception_throws_custom = true;
			}
			else
			{
				if (a.Type == pa.AssemblyFileVersion)
				{
					vi_product_version = a.GetString();
					if (string.IsNullOrEmpty(vi_product_version) || IsValidAssemblyVersion(vi_product_version, false) == null)
					{
						Report.Warning(7035, 1, a.Location, "The specified version string `{0}' does not conform to the recommended format major.minor.build.revision", vi_product_version, a.Name);
						return;
					}
					CustomAttributeBuilder customAttribute = new CustomAttributeBuilder((ConstructorInfo)ctor.GetMetaInfo(), new object[1] { vi_product_version });
					Builder.SetCustomAttribute(customAttribute);
					return;
				}
				if (a.Type == pa.AssemblyProduct)
				{
					vi_product = a.GetString();
				}
				else if (a.Type == pa.AssemblyCompany)
				{
					vi_company = a.GetString();
				}
				else if (a.Type == pa.AssemblyCopyright)
				{
					vi_copyright = a.GetString();
				}
				else if (a.Type == pa.AssemblyTrademark)
				{
					vi_trademark = a.GetString();
				}
				else if (a.Type == pa.Debuggable)
				{
					has_user_debuggable = true;
				}
			}
			SetCustomAttribute(ctor, cdata);
		}

		private void CheckReferencesPublicToken()
		{
			foreach (IAssemblyDefinition assembly in Importer.Assemblies)
			{
				ImportedAssemblyDefinition importedAssemblyDefinition = assembly as ImportedAssemblyDefinition;
				if (importedAssemblyDefinition == null || importedAssemblyDefinition.IsMissing)
				{
					continue;
				}
				if (public_key != null && !importedAssemblyDefinition.HasStrongName)
				{
					Report.Error(1577, "Referenced assembly `{0}' does not have a strong name", importedAssemblyDefinition.FullName);
				}
				CultureInfo cultureInfo = importedAssemblyDefinition.Assembly.GetName().CultureInfo;
				if (!cultureInfo.Equals(CultureInfo.InvariantCulture))
				{
					Report.Warning(8009, 1, "Referenced assembly `{0}' has different culture setting of `{1}'", importedAssemblyDefinition.Name, cultureInfo.Name);
				}
				if (importedAssemblyDefinition.IsFriendAssemblyTo(this))
				{
					AssemblyName assemblyVisibleToName = importedAssemblyDefinition.GetAssemblyVisibleToName(this);
					byte[] publicKeyToken = assemblyVisibleToName.GetPublicKeyToken();
					if (!ArrayComparer.IsEqual(GetPublicKeyToken(), publicKeyToken))
					{
						Report.SymbolRelatedToPreviousError(importedAssemblyDefinition.Location);
						Report.Error(281, "Friend access was granted to `{0}', but the output assembly is named `{1}'. Try adding a reference to `{0}' or change the output assembly name to match it", assemblyVisibleToName.FullName, FullName);
					}
				}
			}
		}

		protected AssemblyName CreateAssemblyName()
		{
			AssemblyName assemblyName = new AssemblyName(name);
			if (public_key != null && Compiler.Settings.Target != Target.Module)
			{
				if (delay_sign)
				{
					assemblyName.SetPublicKey(public_key);
				}
				else if (public_key.Length == 16)
				{
					Report.Error(1606, "Could not sign the assembly. ECMA key can only be used to delay-sign assemblies");
				}
				else if (private_key == null)
				{
					Error_AssemblySigning("The specified key file does not have a private key");
				}
				else
				{
					assemblyName.KeyPair = private_key;
				}
			}
			return assemblyName;
		}

		public virtual ModuleBuilder CreateModuleBuilder()
		{
			if (file_name == null)
			{
				throw new NotSupportedException("transient module in static assembly");
			}
			string fileName = Path.GetFileName(file_name);
			return Builder.DefineDynamicModule(fileName, fileName, false);
		}

		public virtual void Emit()
		{
			if (Compiler.Settings.Target == Target.Module)
			{
				module_target_attrs = new AssemblyAttributesPlaceholder(module, name);
				module_target_attrs.CreateContainer();
				module_target_attrs.DefineContainer();
				module_target_attrs.Define();
				module.AddCompilerGeneratedClass(module_target_attrs);
			}
			else if (added_modules != null)
			{
				ReadModulesAssemblyAttributes();
			}
			if (Compiler.Settings.GenerateDebugInfo)
			{
				symbol_writer = new MonoSymbolFile();
			}
			module.EmitContainer();
			if (module.HasExtensionMethod)
			{
				PredefinedAttribute extension = module.PredefinedAttributes.Extension;
				if (extension.IsDefined)
				{
					SetCustomAttribute(extension.Constructor, AttributeEncoder.Empty);
				}
			}
			if (!IsSatelliteAssembly)
			{
				if (!has_user_debuggable && Compiler.Settings.GenerateDebugInfo)
				{
					PredefinedDebuggableAttribute debuggable = module.PredefinedAttributes.Debuggable;
					if (debuggable.IsDefined)
					{
						DebuggableAttribute.DebuggingModes debuggingModes = DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints;
						if (!Compiler.Settings.Optimize)
						{
							debuggingModes |= DebuggableAttribute.DebuggingModes.DisableOptimizations;
						}
						debuggable.EmitAttribute(Builder, debuggingModes);
					}
				}
				if (!wrap_non_exception_throws_custom)
				{
					PredefinedAttribute runtimeCompatibility = module.PredefinedAttributes.RuntimeCompatibility;
					if (runtimeCompatibility.IsDefined && runtimeCompatibility.ResolveBuilder())
					{
						PropertySpec propertySpec = module.PredefinedMembers.RuntimeCompatibilityWrapNonExceptionThrows.Get();
						if (propertySpec != null)
						{
							AttributeEncoder attributeEncoder = new AttributeEncoder();
							attributeEncoder.EncodeNamedPropertyArgument(propertySpec, new BoolLiteral(Compiler.BuiltinTypes, true, Location.Null));
							SetCustomAttribute(runtimeCompatibility.Constructor, attributeEncoder.ToArray());
						}
					}
				}
				if (declarative_security != null)
				{
					throw new NotSupportedException("Assembly-level security");
				}
			}
			CheckReferencesPublicToken();
			SetEntryPoint();
		}

		public byte[] GetPublicKeyToken()
		{
			if (public_key == null || public_key_token != null)
			{
				return public_key_token;
			}
			byte[] array = SHA1.Create().ComputeHash(public_key);
			public_key_token = new byte[8];
			Buffer.BlockCopy(array, array.Length - 8, public_key_token, 0, 8);
			Array.Reverse(public_key_token, 0, 8);
			return public_key_token;
		}

		private void LoadPublicKey(string keyFile, string keyContainer)
		{
			if (keyContainer != null)
			{
				try
				{
					private_key = new StrongNameKeyPair(keyContainer);
					public_key = private_key.PublicKey;
					return;
				}
				catch
				{
					Error_AssemblySigning("The specified key container `" + keyContainer + "' does not exist");
					return;
				}
			}
			bool flag = File.Exists(keyFile);
			if (!flag && Compiler.Settings.StrongNameKeyFile == null)
			{
				string text = Path.Combine(Path.GetDirectoryName(file_name), keyFile);
				flag = File.Exists(text);
				if (flag)
				{
					keyFile = text;
				}
			}
			if (!flag)
			{
				Error_AssemblySigning("The specified key file `" + keyFile + "' does not exist");
				return;
			}
			using (FileStream fileStream = new FileStream(keyFile, FileMode.Open, FileAccess.Read))
			{
				byte[] array = new byte[fileStream.Length];
				fileStream.Read(array, 0, array.Length);
				if (array.Length == 16)
				{
					public_key = array;
					return;
				}
				try
				{
					byte[] array2 = CryptoConvert.ToCapiPublicKeyBlob(CryptoConvert.FromCapiKeyBlob(array));
					byte[] array3 = new byte[8] { 0, 36, 0, 0, 4, 128, 0, 0 };
					public_key = new byte[12 + array2.Length];
					Buffer.BlockCopy(array3, 0, public_key, 0, array3.Length);
					int num = public_key.Length - 12;
					public_key[8] = (byte)(num & 0xFF);
					public_key[9] = (byte)((num >> 8) & 0xFF);
					public_key[10] = (byte)((num >> 16) & 0xFF);
					public_key[11] = (byte)((num >> 24) & 0xFF);
					Buffer.BlockCopy(array2, 0, public_key, 12, array2.Length);
				}
				catch
				{
					Error_AssemblySigning("The specified key file `" + keyFile + "' has incorrect format");
					return;
				}
				if (delay_sign)
				{
					return;
				}
				try
				{
					CryptoConvert.FromCapiPrivateKeyBlob(array);
					private_key = new StrongNameKeyPair(array);
				}
				catch
				{
				}
			}
		}

		private void ReadModulesAssemblyAttributes()
		{
			foreach (ImportedModuleDefinition added_module in added_modules)
			{
				List<Attribute> list = added_module.ReadAssemblyAttributes();
				if (list != null)
				{
					module.OptAttributes.AddAttributes(list);
				}
			}
		}

		public void Resolve()
		{
			if (Compiler.Settings.Unsafe && module.PredefinedTypes.SecurityAction.Define())
			{
				Location location = Location.Null;
				MemberAccess expr = new MemberAccess(new MemberAccess(new QualifiedAliasMember(QualifiedAliasMember.GlobalAlias, "System", location), "Security", location), "Permissions", location);
				ConstSpec constSpec = module.PredefinedMembers.SecurityActionRequestMinimum.Resolve(location);
				Arguments arguments = new Arguments(1);
				arguments.Add(new Argument(constSpec.GetConstant(null)));
				Arguments arguments2 = new Arguments(1);
				arguments2.Add(new NamedArgument("SkipVerification", location, new BoolLiteral(Compiler.BuiltinTypes, true, location)));
				Attribute attribute = new Attribute("assembly", new MemberAccess(expr, "SecurityPermissionAttribute"), new Arguments[2] { arguments, arguments2 }, location, false);
				attribute.AttachTo(module, module);
				Compiler.Report.DisableReporting();
				try
				{
					MethodSpec methodSpec = attribute.Resolve();
					if (methodSpec != null)
					{
						attribute.ExtractSecurityPermissionSet(methodSpec, ref declarative_security);
					}
				}
				finally
				{
					Compiler.Report.EnableReporting();
				}
			}
			if (module.OptAttributes == null || !module.OptAttributes.CheckTargets())
			{
				return;
			}
			cls_attribute = module.ResolveAssemblyAttribute(module.PredefinedAttributes.CLSCompliant);
			if (cls_attribute != null)
			{
				is_cls_compliant = cls_attribute.GetClsCompliantAttributeValue();
			}
			if (added_modules != null && Compiler.Settings.VerifyClsCompliance && is_cls_compliant)
			{
				foreach (ImportedModuleDefinition added_module in added_modules)
				{
					if (!added_module.IsCLSCompliant)
					{
						Report.Error(3013, "Added modules must be marked with the CLSCompliant attribute to match the assembly", added_module.Name);
					}
				}
			}
			Attribute attribute2 = module.ResolveAssemblyAttribute(module.PredefinedAttributes.RuntimeCompatibility);
			if (attribute2 != null)
			{
				BoolConstant boolConstant = attribute2.GetNamedValue("WrapNonExceptionThrows") as BoolConstant;
				if (boolConstant != null)
				{
					wrap_non_exception_throws = boolConstant.Value;
				}
			}
		}

		protected void ResolveAssemblySecurityAttributes()
		{
			string text = null;
			string text2 = null;
			if (module.OptAttributes != null)
			{
				foreach (Attribute attr in module.OptAttributes.Attrs)
				{
					if (attr.ExplicitTarget != "assembly")
					{
						continue;
					}
					switch (attr.Name)
					{
					case "AssemblyKeyFile":
					case "AssemblyKeyFileAttribute":
					case "System.Reflection.AssemblyKeyFileAttribute":
					{
						if (Compiler.Settings.StrongNameKeyFile != null)
						{
							Report.SymbolRelatedToPreviousError(attr.Location, attr.GetSignatureForError());
							Report.Warning(1616, 1, "Option `{0}' overrides attribute `{1}' given in a source file or added module", "keyfile", "System.Reflection.AssemblyKeyFileAttribute");
							break;
						}
						string text3 = attr.GetString();
						if (!string.IsNullOrEmpty(text3))
						{
							Error_ObsoleteSecurityAttribute(attr, "keyfile");
							text = text3;
						}
						break;
					}
					case "AssemblyKeyName":
					case "AssemblyKeyNameAttribute":
					case "System.Reflection.AssemblyKeyNameAttribute":
					{
						if (Compiler.Settings.StrongNameKeyContainer != null)
						{
							Report.SymbolRelatedToPreviousError(attr.Location, attr.GetSignatureForError());
							Report.Warning(1616, 1, "Option `{0}' overrides attribute `{1}' given in a source file or added module", "keycontainer", "System.Reflection.AssemblyKeyNameAttribute");
							break;
						}
						string text4 = attr.GetString();
						if (!string.IsNullOrEmpty(text4))
						{
							Error_ObsoleteSecurityAttribute(attr, "keycontainer");
							text2 = text4;
						}
						break;
					}
					case "AssemblyDelaySign":
					case "AssemblyDelaySignAttribute":
					case "System.Reflection.AssemblyDelaySignAttribute":
					{
						bool boolean = attr.GetBoolean();
						if (boolean)
						{
							Error_ObsoleteSecurityAttribute(attr, "delaysign");
						}
						delay_sign = boolean;
						break;
					}
					}
				}
			}
			if (public_key == null)
			{
				if (text != null || text2 != null)
				{
					LoadPublicKey(text, text2);
				}
				else if (delay_sign)
				{
					Report.Warning(1607, 1, "Delay signing was requested but no key file was given");
				}
			}
		}

		public void EmbedResources()
		{
			if (Compiler.Settings.Win32ResourceFile != null)
			{
				Builder.DefineUnmanagedResource(Compiler.Settings.Win32ResourceFile);
			}
			else
			{
				Builder.DefineVersionInfoResource(vi_product, vi_product_version, vi_company, vi_copyright, vi_trademark);
			}
			if (Compiler.Settings.Win32IconFile != null)
			{
				builder_extra.DefineWin32IconResource(Compiler.Settings.Win32IconFile);
			}
			if (Compiler.Settings.Resources == null)
			{
				return;
			}
			if (Compiler.Settings.Target == Target.Module)
			{
				Report.Error(1507, "Cannot link resource file when building a module");
				return;
			}
			int num = 0;
			foreach (AssemblyResource resource in Compiler.Settings.Resources)
			{
				if (!File.Exists(resource.FileName))
				{
					Report.Error(1566, "Error reading resource file `{0}'", resource.FileName);
				}
				else if (resource.IsEmbeded)
				{
					Stream stream = ((num++ >= 10) ? ((Stream)new MemoryStream(File.ReadAllBytes(resource.FileName))) : ((Stream)File.OpenRead(resource.FileName)));
					module.Builder.DefineManifestResource(resource.Name, stream, resource.Attributes);
				}
				else
				{
					Builder.AddResourceFile(resource.Name, Path.GetFileName(resource.FileName), resource.Attributes);
				}
			}
		}

		public void Save()
		{
			PortableExecutableKinds portableExecutableKinds = PortableExecutableKinds.ILOnly;
			ImageFileMachine imageFileMachine;
			switch (Compiler.Settings.Platform)
			{
			case Platform.X86:
				portableExecutableKinds |= PortableExecutableKinds.Required32Bit;
				imageFileMachine = ImageFileMachine.I386;
				break;
			case Platform.X64:
				portableExecutableKinds |= PortableExecutableKinds.PE32Plus;
				imageFileMachine = ImageFileMachine.AMD64;
				break;
			case Platform.IA64:
				imageFileMachine = ImageFileMachine.IA64;
				break;
			case Platform.AnyCPU32Preferred:
				throw new NotSupportedException();
			case Platform.Arm:
				throw new NotSupportedException();
			default:
				imageFileMachine = ImageFileMachine.I386;
				break;
			}
			Compiler.TimeReporter.Start(TimeReporter.TimerType.OutputSave);
			try
			{
				if (Compiler.Settings.Target == Target.Module)
				{
					SaveModule(portableExecutableKinds, imageFileMachine);
				}
				else
				{
					Builder.Save(module.Builder.ScopeName, portableExecutableKinds, imageFileMachine);
				}
			}
			catch (Exception ex)
			{
				Report.Error(16, "Could not write to file `" + name + "', cause: " + ex.Message);
			}
			Compiler.TimeReporter.Stop(TimeReporter.TimerType.OutputSave);
			if (symbol_writer != null && Compiler.Report.Errors == 0)
			{
				Compiler.TimeReporter.Start(TimeReporter.TimerType.DebugSave);
				string path = file_name + ".mdb";
				try
				{
					File.Delete(path);
				}
				catch
				{
				}
				module.WriteDebugSymbol(symbol_writer);
				using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
				{
					symbol_writer.CreateSymbolFile(module.Builder.ModuleVersionId, fs);
				}
				Compiler.TimeReporter.Stop(TimeReporter.TimerType.DebugSave);
			}
		}

		protected virtual void SaveModule(PortableExecutableKinds pekind, ImageFileMachine machine)
		{
			Report.RuntimeMissingSupport(Location.Null, "-target:module");
		}

		private void SetCustomAttribute(MethodSpec ctor, byte[] data)
		{
			if (module_target_attrs != null)
			{
				module_target_attrs.AddAssemblyAttribute(ctor, data);
			}
			else
			{
				Builder.SetCustomAttribute((ConstructorInfo)ctor.GetMetaInfo(), data);
			}
		}

		private void SetEntryPoint()
		{
			if (!Compiler.Settings.NeedsEntryPoint)
			{
				if (Compiler.Settings.MainClass != null)
				{
					Report.Error(2017, "Cannot specify -main if building a module or library");
				}
				return;
			}
			PEFileKinds fileKind;
			switch (Compiler.Settings.Target)
			{
			case Target.Library:
			case Target.Module:
				fileKind = PEFileKinds.Dll;
				break;
			case Target.WinExe:
				fileKind = PEFileKinds.WindowApplication;
				break;
			default:
				fileKind = PEFileKinds.ConsoleApplication;
				break;
			}
			if (entry_point == null)
			{
				string mainClass = Compiler.Settings.MainClass;
				if (mainClass != null)
				{
					TypeSpec typeSpec = module.GlobalRootNamespace.LookupType(module, mainClass, 0, LookupMode.Probing, Location.Null);
					if (typeSpec == null)
					{
						Report.Error(1555, "Could not find `{0}' specified for Main method", mainClass);
						return;
					}
					ClassOrStruct classOrStruct = typeSpec.MemberDefinition as ClassOrStruct;
					if (classOrStruct == null)
					{
						Report.Error(1556, "`{0}' specified for Main method must be a valid class or struct", mainClass);
					}
					else
					{
						Report.Error(1558, classOrStruct.Location, "`{0}' does not have a suitable static Main method", classOrStruct.GetSignatureForError());
					}
				}
				else
				{
					string arg = ((file_name == null) ? name : Path.GetFileName(file_name));
					Report.Error(5001, "Program `{0}' does not contain a static `Main' method suitable for an entry point", arg);
				}
			}
			else
			{
				Builder.SetEntryPoint(entry_point.MethodBuilder, fileKind);
			}
		}

		private void Error_ObsoleteSecurityAttribute(Attribute a, string option)
		{
			Report.Warning(1699, 1, a.Location, "Use compiler option `{0}' or appropriate project settings instead of `{1}' attribute", option, a.Name);
		}

		private void Error_AssemblySigning(string text)
		{
			Report.Error(1548, "Error during assembly signing. " + text);
		}

		public bool IsFriendAssemblyTo(IAssemblyDefinition assembly)
		{
			return false;
		}

		private static Version IsValidAssemblyVersion(string version, bool allowGenerated)
		{
			string[] array = version.Split('.');
			if (array.Length < 1 || array.Length > 4)
			{
				return null;
			}
			int[] array2 = new int[4];
			for (int i = 0; i < array.Length; i++)
			{
				if (!int.TryParse(array[i], out array2[i]))
				{
					if ((array[i].Length == 1 && array[i][0] == '*') & allowGenerated)
					{
						if (i == 2)
						{
							if (array.Length > 3)
							{
								return null;
							}
							array2[i] = System.Math.Max((DateTime.Today - new DateTime(2000, 1, 1)).Days, 0);
							i = 3;
						}
						if (i == 3)
						{
							array2[i] = (int)(DateTime.Now - DateTime.Today).TotalSeconds / 2;
							continue;
						}
					}
					return null;
				}
				if (array2[i] > 65535)
				{
					return null;
				}
			}
			return new Version(array2[0], array2[1], array2[2], array2[3]);
		}
	}
}
