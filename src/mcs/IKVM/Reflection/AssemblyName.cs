using System;
using System.Configuration.Assemblies;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	public sealed class AssemblyName : ICloneable
	{
		private string name;

		private string culture;

		private Version version;

		private byte[] publicKeyToken;

		private byte[] publicKey;

		private StrongNameKeyPair keyPair;

		private AssemblyNameFlags flags;

		private AssemblyHashAlgorithm hashAlgorithm;

		private AssemblyVersionCompatibility versionCompatibility = AssemblyVersionCompatibility.SameMachine;

		private string codeBase;

		internal byte[] hash;

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		public CultureInfo CultureInfo
		{
			get
			{
				if (culture != null)
				{
					return new CultureInfo(culture);
				}
				return null;
			}
			set
			{
				culture = value?.Name;
			}
		}

		public string CultureName => culture;

		internal string Culture
		{
			get
			{
				return culture;
			}
			set
			{
				culture = value;
			}
		}

		public Version Version
		{
			get
			{
				return version;
			}
			set
			{
				version = value;
			}
		}

		public StrongNameKeyPair KeyPair
		{
			get
			{
				return keyPair;
			}
			set
			{
				keyPair = value;
			}
		}

		public string CodeBase
		{
			get
			{
				return codeBase;
			}
			set
			{
				codeBase = value;
			}
		}

		public string EscapedCodeBase => new System.Reflection.AssemblyName
		{
			CodeBase = codeBase
		}.EscapedCodeBase;

		public ProcessorArchitecture ProcessorArchitecture
		{
			get
			{
				return (ProcessorArchitecture)((int)(flags & (AssemblyNameFlags)0x70) >> 4);
			}
			set
			{
				if (value >= ProcessorArchitecture.None && value <= ProcessorArchitecture.Arm)
				{
					flags = (AssemblyNameFlags)((int)(flags & (AssemblyNameFlags)(-113)) | ((int)value << 4));
				}
			}
		}

		public AssemblyNameFlags Flags
		{
			get
			{
				return flags & (AssemblyNameFlags)(-3825);
			}
			set
			{
				flags = (flags & (AssemblyNameFlags)0xEF0) | (value & (AssemblyNameFlags)(-3825));
			}
		}

		public AssemblyVersionCompatibility VersionCompatibility
		{
			get
			{
				return versionCompatibility;
			}
			set
			{
				versionCompatibility = value;
			}
		}

		public AssemblyContentType ContentType
		{
			get
			{
				return (AssemblyContentType)((int)(flags & (AssemblyNameFlags)0xE00) >> 9);
			}
			set
			{
				if (value >= AssemblyContentType.Default && value <= AssemblyContentType.WindowsRuntime)
				{
					flags = (AssemblyNameFlags)((int)(flags & (AssemblyNameFlags)(-3585)) | ((int)value << 9));
				}
			}
		}

		public AssemblyHashAlgorithm HashAlgorithm
		{
			get
			{
				return hashAlgorithm;
			}
			set
			{
				hashAlgorithm = value;
			}
		}

		public byte[] __Hash => hash;

		public string FullName
		{
			get
			{
				if (name == null)
				{
					return "";
				}
				ushort versionMajor = ushort.MaxValue;
				ushort versionMinor = ushort.MaxValue;
				ushort versionBuild = ushort.MaxValue;
				ushort versionRevision = ushort.MaxValue;
				if (version != null)
				{
					versionMajor = (ushort)version.Major;
					versionMinor = (ushort)version.Minor;
					versionBuild = (ushort)version.Build;
					versionRevision = (ushort)version.Revision;
				}
				byte[] array = publicKeyToken;
				if ((array == null || array.Length == 0) && publicKey != null)
				{
					array = ComputePublicKeyToken(publicKey);
				}
				return GetFullName(name, versionMajor, versionMinor, versionBuild, versionRevision, culture, array, (int)flags);
			}
		}

		internal AssemblyNameFlags RawFlags
		{
			get
			{
				return flags;
			}
			set
			{
				flags = value;
			}
		}

		public AssemblyName()
		{
		}

		public AssemblyName(string assemblyName)
		{
			if (assemblyName == null)
			{
				throw new ArgumentNullException("assemblyName");
			}
			if (assemblyName == "")
			{
				throw new ArgumentException();
			}
			ParsedAssemblyName parsedName;
			ParseAssemblyResult parseAssemblyResult = Fusion.ParseAssemblyName(assemblyName, out parsedName);
			if (parseAssemblyResult == ParseAssemblyResult.GenericError || parseAssemblyResult == ParseAssemblyResult.DuplicateKey)
			{
				throw new FileLoadException();
			}
			if (!ParseVersion(parsedName.Version, parsedName.Retargetable.HasValue, out version))
			{
				throw new FileLoadException();
			}
			name = parsedName.Name;
			if (parsedName.Culture != null)
			{
				if (parsedName.Culture.Equals("neutral", StringComparison.OrdinalIgnoreCase))
				{
					culture = "";
				}
				else
				{
					if (parsedName.Culture == "")
					{
						throw new FileLoadException();
					}
					culture = new CultureInfo(parsedName.Culture).Name;
				}
			}
			if (parsedName.PublicKeyToken != null)
			{
				if (parsedName.PublicKeyToken.Equals("null", StringComparison.OrdinalIgnoreCase))
				{
					publicKeyToken = Empty<byte>.Array;
				}
				else
				{
					if (parsedName.PublicKeyToken.Length != 16)
					{
						throw new FileLoadException();
					}
					publicKeyToken = ParseKey(parsedName.PublicKeyToken);
				}
			}
			if (parsedName.Retargetable.HasValue)
			{
				if (parsedName.Culture == null || parsedName.PublicKeyToken == null || version == null)
				{
					throw new FileLoadException();
				}
				if (parsedName.Retargetable.Value)
				{
					flags |= AssemblyNameFlags.Retargetable;
				}
			}
			ProcessorArchitecture = parsedName.ProcessorArchitecture;
			if (parsedName.WindowsRuntime)
			{
				ContentType = AssemblyContentType.WindowsRuntime;
			}
		}

		private static byte[] ParseKey(string key)
		{
			if ((key.Length & 1) != 0)
			{
				throw new FileLoadException();
			}
			byte[] array = new byte[key.Length / 2];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = (byte)(ParseHexDigit(key[i * 2]) * 16 + ParseHexDigit(key[i * 2 + 1]));
			}
			return array;
		}

		private static int ParseHexDigit(char digit)
		{
			if (digit >= '0' && digit <= '9')
			{
				return digit - 48;
			}
			digit = (char)(digit | 0x20);
			if (digit >= 'a' && digit <= 'f')
			{
				return 10 + digit - 97;
			}
			throw new FileLoadException();
		}

		public override string ToString()
		{
			return FullName;
		}

		public byte[] GetPublicKey()
		{
			return publicKey;
		}

		public void SetPublicKey(byte[] publicKey)
		{
			this.publicKey = publicKey;
			flags = (AssemblyNameFlags)((int)(flags & ~AssemblyNameFlags.PublicKey) | ((publicKey != null) ? 1 : 0));
		}

		public byte[] GetPublicKeyToken()
		{
			if (publicKeyToken == null && publicKey != null)
			{
				publicKeyToken = ComputePublicKeyToken(publicKey);
			}
			return publicKeyToken;
		}

		public void SetPublicKeyToken(byte[] publicKeyToken)
		{
			this.publicKeyToken = publicKeyToken;
		}

		internal static string GetFullName(string name, ushort versionMajor, ushort versionMinor, ushort versionBuild, ushort versionRevision, string culture, byte[] publicKeyToken, int flags)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = name.StartsWith(" ") || name.EndsWith(" ") || name.IndexOf('\'') != -1;
			bool flag2 = name.IndexOf('"') != -1;
			if (flag2)
			{
				stringBuilder.Append('\'');
			}
			else if (flag)
			{
				stringBuilder.Append('"');
			}
			if (name.IndexOf(',') != -1 || name.IndexOf('\\') != -1 || name.IndexOf('=') != -1 || (flag2 && name.IndexOf('\'') != -1))
			{
				foreach (char c in name)
				{
					if (c == ',' || c == '\\' || c == '=' || (flag2 && c == '\''))
					{
						stringBuilder.Append('\\');
					}
					stringBuilder.Append(c);
				}
			}
			else
			{
				stringBuilder.Append(name);
			}
			if (flag2)
			{
				stringBuilder.Append('\'');
			}
			else if (flag)
			{
				stringBuilder.Append('"');
			}
			if (versionMajor != ushort.MaxValue)
			{
				stringBuilder.Append(", Version=").Append(versionMajor);
				if (versionMinor != ushort.MaxValue)
				{
					stringBuilder.Append('.').Append(versionMinor);
					if (versionBuild != ushort.MaxValue)
					{
						stringBuilder.Append('.').Append(versionBuild);
						if (versionRevision != ushort.MaxValue)
						{
							stringBuilder.Append('.').Append(versionRevision);
						}
					}
				}
			}
			if (culture != null)
			{
				stringBuilder.Append(", Culture=").Append((culture == "") ? "neutral" : culture);
			}
			if (publicKeyToken != null)
			{
				stringBuilder.Append(", PublicKeyToken=");
				if (publicKeyToken.Length == 0)
				{
					stringBuilder.Append("null");
				}
				else
				{
					AppendPublicKey(stringBuilder, publicKeyToken);
				}
			}
			if ((flags & 0x100) != 0)
			{
				stringBuilder.Append(", Retargetable=Yes");
			}
			if ((flags & 0xE00) >> 9 == 1)
			{
				stringBuilder.Append(", ContentType=WindowsRuntime");
			}
			return stringBuilder.ToString();
		}

		internal static byte[] ComputePublicKeyToken(byte[] publicKey)
		{
			if (publicKey.Length == 0)
			{
				return publicKey;
			}
			byte[] array = new SHA1Managed().ComputeHash(publicKey);
			byte[] array2 = new byte[8];
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = array[array.Length - 1 - i];
			}
			return array2;
		}

		internal static string ComputePublicKeyToken(string publicKey)
		{
			StringBuilder stringBuilder = new StringBuilder(16);
			AppendPublicKey(stringBuilder, ComputePublicKeyToken(ParseKey(publicKey)));
			return stringBuilder.ToString();
		}

		private static void AppendPublicKey(StringBuilder sb, byte[] publicKey)
		{
			for (int i = 0; i < publicKey.Length; i++)
			{
				sb.Append("0123456789abcdef"[publicKey[i] >> 4]);
				sb.Append("0123456789abcdef"[publicKey[i] & 0xF]);
			}
		}

		public override bool Equals(object obj)
		{
			AssemblyName assemblyName = obj as AssemblyName;
			if (assemblyName != null)
			{
				return assemblyName.FullName == FullName;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return FullName.GetHashCode();
		}

		public object Clone()
		{
			AssemblyName obj = (AssemblyName)MemberwiseClone();
			obj.publicKey = Copy(publicKey);
			obj.publicKeyToken = Copy(publicKeyToken);
			return obj;
		}

		private static byte[] Copy(byte[] b)
		{
			if (b != null && b.Length != 0)
			{
				return (byte[])b.Clone();
			}
			return b;
		}

		public static bool ReferenceMatchesDefinition(AssemblyName reference, AssemblyName definition)
		{
			return System.Reflection.AssemblyName.ReferenceMatchesDefinition(new System.Reflection.AssemblyName(reference.FullName), new System.Reflection.AssemblyName(definition.FullName));
		}

		public static AssemblyName GetAssemblyName(string path)
		{
			try
			{
				path = Path.GetFullPath(path);
				using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					ModuleReader moduleReader = new ModuleReader(null, null, stream, path, false);
					if (moduleReader.Assembly == null)
					{
						throw new BadImageFormatException("Module does not contain a manifest");
					}
					return moduleReader.Assembly.GetName();
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

		private static bool ParseVersion(string str, bool mustBeComplete, out Version version)
		{
			if (str == null)
			{
				version = null;
				return true;
			}
			string[] array = str.Split('.');
			if (array.Length < 2 || array.Length > 4)
			{
				version = null;
				ushort result;
				if (array.Length == 1)
				{
					return ushort.TryParse(array[0], NumberStyles.Integer, null, out result);
				}
				return false;
			}
			if (array[0] == "" || array[1] == "")
			{
				version = null;
				return true;
			}
			ushort result2 = ushort.MaxValue;
			ushort result3 = ushort.MaxValue;
			ushort result4;
			ushort result5;
			if (ushort.TryParse(array[0], NumberStyles.Integer, null, out result4) && ushort.TryParse(array[1], NumberStyles.Integer, null, out result5) && (array.Length <= 2 || array[2] == "" || ushort.TryParse(array[2], NumberStyles.Integer, null, out result2)) && (array.Length <= 3 || array[3] == "" || (array[2] != "" && ushort.TryParse(array[3], NumberStyles.Integer, null, out result3))))
			{
				if (mustBeComplete && (array.Length < 4 || array[2] == "" || array[3] == ""))
				{
					version = null;
				}
				else if (result4 == ushort.MaxValue || result5 == ushort.MaxValue)
				{
					version = null;
				}
				else
				{
					version = new Version(result4, result5, result2, result3);
				}
				return true;
			}
			version = null;
			return false;
		}
	}
}
