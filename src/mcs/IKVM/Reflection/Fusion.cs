using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace IKVM.Reflection
{
	internal static class Fusion
	{
		private static readonly Version FrameworkVersion = new Version(4, 0, 0, 0);

		private static readonly Version FrameworkVersionNext = new Version(4, 1, 0, 0);

		private static readonly Version SilverlightVersion = new Version(2, 0, 5, 0);

		private static readonly Version SilverlightVersionMinimum = new Version(2, 0, 0, 0);

		private static readonly Version SilverlightVersionMaximum = new Version(5, 9, 0, 0);

		private const string PublicKeyTokenEcma = "b77a5c561934e089";

		private const string PublicKeyTokenMicrosoft = "b03f5f7f11d50a3a";

		private const string PublicKeyTokenSilverlight = "7cec85d7bea7798e";

		private const string PublicKeyTokenWinFX = "31bf3856ad364e35";

		internal static bool CompareAssemblyIdentityNative(string assemblyIdentity1, bool unified1, string assemblyIdentity2, bool unified2, out AssemblyComparisonResult result)
		{
			bool pfEquivalent;
			Marshal.ThrowExceptionForHR(CompareAssemblyIdentity(assemblyIdentity1, unified1, assemblyIdentity2, unified2, out pfEquivalent, out result));
			return pfEquivalent;
		}

		[DllImport("fusion", CharSet = CharSet.Unicode)]
		private static extern int CompareAssemblyIdentity(string pwzAssemblyIdentity1, bool fUnified1, string pwzAssemblyIdentity2, bool fUnified2, out bool pfEquivalent, out AssemblyComparisonResult pResult);

		internal static bool CompareAssemblyIdentityPure(string assemblyIdentity1, bool unified1, string assemblyIdentity2, bool unified2, out AssemblyComparisonResult result)
		{
			ParsedAssemblyName parsedName;
			ParseAssemblyResult parseAssemblyResult = ParseAssemblyName(assemblyIdentity1, out parsedName);
			ParsedAssemblyName parsedName2;
			ParseAssemblyResult parseAssemblyResult2 = ParseAssemblyName(assemblyIdentity2, out parsedName2);
			Version version;
			if (unified1 && (parsedName.Name == null || !ParseVersion(parsedName.Version, out version) || version == null || version.Revision == -1 || parsedName.Culture == null || parsedName.PublicKeyToken == null || parsedName.PublicKeyToken.Length < 2))
			{
				result = AssemblyComparisonResult.NonEquivalent;
				throw new ArgumentException();
			}
			Version version2 = null;
			if (!ParseVersion(parsedName2.Version, out version2) || version2 == null || version2.Revision == -1 || parsedName2.Culture == null || parsedName2.PublicKeyToken == null || parsedName2.PublicKeyToken.Length < 2)
			{
				result = AssemblyComparisonResult.NonEquivalent;
				throw new ArgumentException();
			}
			if (parsedName2.Name != null && parsedName2.Name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase))
			{
				if (parsedName.Name != null && parsedName.Name.Equals(parsedName2.Name, StringComparison.OrdinalIgnoreCase))
				{
					result = AssemblyComparisonResult.EquivalentFullMatch;
					return true;
				}
				result = AssemblyComparisonResult.NonEquivalent;
				return false;
			}
			if (parseAssemblyResult != ParseAssemblyResult.OK)
			{
				result = AssemblyComparisonResult.NonEquivalent;
				if (parseAssemblyResult != ParseAssemblyResult.GenericError && parseAssemblyResult == ParseAssemblyResult.DuplicateKey)
				{
					throw new FileLoadException();
				}
				throw new ArgumentException();
			}
			if (parseAssemblyResult2 != ParseAssemblyResult.OK)
			{
				result = AssemblyComparisonResult.NonEquivalent;
				if (parseAssemblyResult2 != ParseAssemblyResult.GenericError && parseAssemblyResult2 == ParseAssemblyResult.DuplicateKey)
				{
					throw new FileLoadException();
				}
				throw new ArgumentException();
			}
			if (!ParseVersion(parsedName.Version, out version))
			{
				result = AssemblyComparisonResult.NonEquivalent;
				throw new ArgumentException();
			}
			bool flag = IsPartial(parsedName, version);
			if (flag && parsedName.Retargetable.HasValue)
			{
				result = AssemblyComparisonResult.NonEquivalent;
				throw new FileLoadException();
			}
			if ((flag & unified1) || IsPartial(parsedName2, version2))
			{
				result = AssemblyComparisonResult.NonEquivalent;
				throw new ArgumentException();
			}
			if (!parsedName.Name.Equals(parsedName2.Name, StringComparison.OrdinalIgnoreCase))
			{
				result = AssemblyComparisonResult.NonEquivalent;
				return false;
			}
			if ((!flag || parsedName.Culture != null) && !parsedName.Culture.Equals(parsedName2.Culture, StringComparison.OrdinalIgnoreCase))
			{
				result = AssemblyComparisonResult.NonEquivalent;
				return false;
			}
			if (parsedName.Retargetable != true && parsedName2.Retargetable == true)
			{
				result = AssemblyComparisonResult.NonEquivalent;
				return false;
			}
			if (parsedName.PublicKeyToken == parsedName2.PublicKeyToken && version != null && parsedName.Retargetable == true && parsedName2.Retargetable != true && GetRemappedPublicKeyToken(ref parsedName, version) != null)
			{
				parsedName.Retargetable = false;
			}
			string text = null;
			string text2 = null;
			if (version != null && (text = GetRemappedPublicKeyToken(ref parsedName, version)) != null)
			{
				parsedName.PublicKeyToken = text;
				version = FrameworkVersion;
			}
			if ((text2 = GetRemappedPublicKeyToken(ref parsedName2, version2)) != null)
			{
				parsedName2.PublicKeyToken = text2;
				version2 = FrameworkVersion;
			}
			if (parsedName.Retargetable == true)
			{
				if (parsedName2.Retargetable == true)
				{
					if ((text != null) ^ (text2 != null))
					{
						result = AssemblyComparisonResult.NonEquivalent;
						return false;
					}
				}
				else if (text == null || text2 != null)
				{
					result = AssemblyComparisonResult.Unknown;
					return false;
				}
			}
			bool flag2 = false;
			bool flag3 = version == version2;
			if (IsFrameworkAssembly(parsedName))
			{
				flag2 |= !flag3;
				version = FrameworkVersion;
			}
			if (IsFrameworkAssembly(parsedName2) && version2 < FrameworkVersionNext)
			{
				flag2 |= !flag3;
				version2 = FrameworkVersion;
			}
			if (IsStrongNamed(parsedName2))
			{
				if (parsedName.PublicKeyToken != null && parsedName.PublicKeyToken != parsedName2.PublicKeyToken)
				{
					result = AssemblyComparisonResult.NonEquivalent;
					return false;
				}
				if (version == null)
				{
					result = AssemblyComparisonResult.EquivalentPartialMatch;
					return true;
				}
				if (version.Revision == -1 || version2.Revision == -1)
				{
					result = AssemblyComparisonResult.NonEquivalent;
					throw new ArgumentException();
				}
				if (version < version2)
				{
					if (unified2)
					{
						result = (flag ? AssemblyComparisonResult.EquivalentPartialUnified : AssemblyComparisonResult.EquivalentUnified);
						return true;
					}
					result = (flag ? AssemblyComparisonResult.NonEquivalentPartialVersion : AssemblyComparisonResult.NonEquivalentVersion);
					return false;
				}
				if (version > version2)
				{
					if (unified1)
					{
						result = (flag ? AssemblyComparisonResult.EquivalentPartialUnified : AssemblyComparisonResult.EquivalentUnified);
						return true;
					}
					result = (flag ? AssemblyComparisonResult.NonEquivalentPartialVersion : AssemblyComparisonResult.NonEquivalentVersion);
					return false;
				}
				if (!flag3 | flag2)
				{
					result = (flag ? AssemblyComparisonResult.EquivalentPartialFXUnified : AssemblyComparisonResult.EquivalentFXUnified);
					return true;
				}
				result = ((!flag) ? AssemblyComparisonResult.EquivalentFullMatch : AssemblyComparisonResult.EquivalentPartialMatch);
				return true;
			}
			if (IsStrongNamed(parsedName))
			{
				result = AssemblyComparisonResult.NonEquivalent;
				return false;
			}
			result = (flag ? AssemblyComparisonResult.EquivalentPartialWeakNamed : AssemblyComparisonResult.EquivalentWeakNamed);
			return true;
		}

		private static bool IsFrameworkAssembly(ParsedAssemblyName name)
		{
			switch (name.Name)
			{
			case "System":
			case "System.Core":
			case "System.Data":
			case "System.Data.DataSetExtensions":
			case "System.Data.Linq":
			case "System.Data.OracleClient":
			case "System.Data.Services":
			case "System.Data.Services.Client":
			case "System.IdentityModel":
			case "System.IdentityModel.Selectors":
			case "System.IO.Compression":
			case "System.Numerics":
			case "System.Reflection.Context":
			case "System.Runtime.Remoting":
			case "System.Runtime.Serialization":
			case "System.Runtime.WindowsRuntime":
			case "System.Runtime.WindowsRuntime.UI.Xaml":
			case "System.ServiceModel":
			case "System.Transactions":
			case "System.Windows.Forms":
			case "System.Xml":
			case "System.Xml.Linq":
			case "System.Xml.Serialization":
				return name.PublicKeyToken == "b77a5c561934e089";
			case "Microsoft.CSharp":
			case "Microsoft.VisualBasic":
			case "System.Collections":
			case "System.Collections.Concurrent":
			case "System.ComponentModel":
			case "System.ComponentModel.Annotations":
			case "System.ComponentModel.EventBasedAsync":
			case "System.Configuration":
			case "System.Configuration.Install":
			case "System.Design":
			case "System.Diagnostics.Contracts":
			case "System.Diagnostics.Debug":
			case "System.Diagnostics.Tools":
			case "System.Diagnostics.Tracing":
			case "System.DirectoryServices":
			case "System.Drawing":
			case "System.Drawing.Design":
			case "System.Dynamic.Runtime":
			case "System.EnterpriseServices":
			case "System.Globalization":
			case "System.IO":
			case "System.Linq":
			case "System.Linq.Expressions":
			case "System.Linq.Parallel":
			case "System.Linq.Queryable":
			case "System.Management":
			case "System.Messaging":
			case "System.Net":
			case "System.Net.Http":
			case "System.Net.Http.Rtc":
			case "System.Net.NetworkInformation":
			case "System.Net.Primitives":
			case "System.Net.Requests":
			case "System.ObjectModel":
			case "System.Reflection":
			case "System.Reflection.Extensions":
			case "System.Reflection.Primitives":
			case "System.Resources.ResourceManager":
			case "System.Runtime":
			case "System.Runtime.Extensions":
			case "System.Runtime.InteropServices":
			case "System.Runtime.InteropServices.WindowsRuntime":
			case "System.Runtime.Numerics":
			case "System.Runtime.Serialization.Formatters.Soap":
			case "System.Runtime.Serialization.Json":
			case "System.Runtime.Serialization.Primitives":
			case "System.Runtime.Serialization.Xml":
			case "System.Security":
			case "System.Security.Principal":
			case "System.ServiceModel.Duplex":
			case "System.ServiceModel.Http":
			case "System.ServiceModel.NetTcp":
			case "System.ServiceModel.Primitives":
			case "System.ServiceModel.Security":
			case "System.ServiceProcess":
			case "System.Text.Encoding":
			case "System.Text.Encoding.Extensions":
			case "System.Text.RegularExpressions":
			case "System.Threading":
			case "System.Threading.Tasks":
			case "System.Threading.Tasks.Parallel":
			case "System.Web":
			case "System.Web.Mobile":
			case "System.Web.Services":
			case "System.Windows":
			case "System.Xml.ReaderWriter":
			case "System.Xml.XDocument":
			case "System.Xml.XmlSerializer":
				return name.PublicKeyToken == "b03f5f7f11d50a3a";
			case "System.ComponentModel.DataAnnotations":
			case "System.ServiceModel.Web":
			case "System.Web.Abstractions":
			case "System.Web.Extensions":
			case "System.Web.Extensions.Design":
			case "System.Web.DynamicData":
			case "System.Web.Routing":
				return name.PublicKeyToken == "31bf3856ad364e35";
			default:
				return false;
			}
		}

		private static string GetRemappedPublicKeyToken(ref ParsedAssemblyName name, Version version)
		{
			if (name.Retargetable == true && version < SilverlightVersion)
			{
				return null;
			}
			if (name.PublicKeyToken == "ddd0da4d3e678217" && name.Name == "System.ComponentModel.DataAnnotations" && name.Retargetable == true)
			{
				return "31bf3856ad364e35";
			}
			if (SilverlightVersionMinimum <= version && version <= SilverlightVersionMaximum)
			{
				string publicKeyToken = name.PublicKeyToken;
				if (!(publicKeyToken == "7cec85d7bea7798e"))
				{
					if (publicKeyToken == "31bf3856ad364e35")
					{
						publicKeyToken = name.Name;
						if (publicKeyToken == "System.ComponentModel.Composition")
						{
							return "b77a5c561934e089";
						}
						if (name.Retargetable == true)
						{
							switch (name.Name)
							{
							case "Microsoft.CSharp":
								return "b03f5f7f11d50a3a";
							case "System.Numerics":
							case "System.ServiceModel":
							case "System.Xml.Serialization":
							case "System.Xml.Linq":
								return "b77a5c561934e089";
							}
						}
					}
				}
				else
				{
					publicKeyToken = name.Name;
					if (publicKeyToken == "System" || publicKeyToken == "System.Core")
					{
						return "b77a5c561934e089";
					}
					if (name.Retargetable == true)
					{
						switch (name.Name)
						{
						case "System.Runtime.Serialization":
						case "System.Xml":
							return "b77a5c561934e089";
						case "System.Net":
						case "System.Windows":
							return "b03f5f7f11d50a3a";
						case "System.ServiceModel.Web":
							return "31bf3856ad364e35";
						}
					}
				}
			}
			return null;
		}

		internal static ParseAssemblyResult ParseAssemblySimpleName(string fullName, out int pos, out string simpleName)
		{
			pos = 0;
			if (!TryParse(fullName, ref pos, out simpleName) || simpleName.Length == 0)
			{
				return ParseAssemblyResult.GenericError;
			}
			if (pos == fullName.Length)
			{
				if (fullName[fullName.Length - 1] == ',')
				{
					return ParseAssemblyResult.GenericError;
				}
			}
			return ParseAssemblyResult.OK;
		}

		private static bool TryParse(string fullName, ref int pos, out string value)
		{
			value = null;
			StringBuilder stringBuilder = new StringBuilder();
			while (pos < fullName.Length && char.IsWhiteSpace(fullName[pos]))
			{
				pos++;
			}
			int num = -1;
			if (pos < fullName.Length && (fullName[pos] == '"' || fullName[pos] == '\''))
			{
				num = fullName[pos++];
			}
			while (pos < fullName.Length)
			{
				char c = fullName[pos];
				if (c == '\\')
				{
					if (++pos == fullName.Length)
					{
						return false;
					}
					c = fullName[pos];
					if (c == '\\')
					{
						return false;
					}
				}
				else
				{
					if (c == num)
					{
						pos++;
						while (pos != fullName.Length)
						{
							c = fullName[pos];
							if (c == ',' || c == '=')
							{
								break;
							}
							if (!char.IsWhiteSpace(c))
							{
								return false;
							}
							pos++;
						}
						break;
					}
					if (num == -1 && (c == '"' || c == '\''))
					{
						return false;
					}
					if (num == -1 && (c == ',' || c == '='))
					{
						break;
					}
				}
				stringBuilder.Append(c);
				pos++;
			}
			value = stringBuilder.ToString().Trim();
			if (value.Length == 0)
			{
				return num != -1;
			}
			return true;
		}

		private static bool TryConsume(string fullName, char ch, ref int pos)
		{
			if (pos < fullName.Length && fullName[pos] == ch)
			{
				pos++;
				return true;
			}
			return false;
		}

		private static bool TryParseAssemblyAttribute(string fullName, ref int pos, ref string key, ref string value)
		{
			if (TryConsume(fullName, ',', ref pos) && TryParse(fullName, ref pos, out key) && TryConsume(fullName, '=', ref pos))
			{
				return TryParse(fullName, ref pos, out value);
			}
			return false;
		}

		internal static ParseAssemblyResult ParseAssemblyName(string fullName, out ParsedAssemblyName parsedName)
		{
			parsedName = default(ParsedAssemblyName);
			int pos;
			ParseAssemblyResult parseAssemblyResult = ParseAssemblySimpleName(fullName, out pos, out parsedName.Name);
			if (parseAssemblyResult != ParseAssemblyResult.OK)
			{
				return parseAssemblyResult;
			}
			Dictionary<string, string> dictionary = null;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			while (pos != fullName.Length)
			{
				string key = null;
				string value = null;
				if (!TryParseAssemblyAttribute(fullName, ref pos, ref key, ref value))
				{
					return ParseAssemblyResult.GenericError;
				}
				key = key.ToLowerInvariant();
				string publicKeyToken;
				switch (key)
				{
				case "version":
					if (parsedName.Version != null)
					{
						return ParseAssemblyResult.DuplicateKey;
					}
					parsedName.Version = value;
					break;
				case "culture":
					if (parsedName.Culture != null)
					{
						return ParseAssemblyResult.DuplicateKey;
					}
					if (!ParseCulture(value, out parsedName.Culture))
					{
						return ParseAssemblyResult.GenericError;
					}
					break;
				case "publickeytoken":
					if (flag3)
					{
						return ParseAssemblyResult.DuplicateKey;
					}
					if (!ParsePublicKeyToken(value, out publicKeyToken))
					{
						return ParseAssemblyResult.GenericError;
					}
					if (parsedName.HasPublicKey && parsedName.PublicKeyToken != publicKeyToken)
					{
						Marshal.ThrowExceptionForHR(-2147010794);
					}
					parsedName.PublicKeyToken = publicKeyToken;
					flag3 = true;
					break;
				case "publickey":
					if (parsedName.HasPublicKey)
					{
						return ParseAssemblyResult.DuplicateKey;
					}
					if (!ParsePublicKey(value, out publicKeyToken))
					{
						return ParseAssemblyResult.GenericError;
					}
					if (flag3 && parsedName.PublicKeyToken != publicKeyToken)
					{
						Marshal.ThrowExceptionForHR(-2147010794);
					}
					parsedName.PublicKeyToken = publicKeyToken;
					parsedName.HasPublicKey = true;
					break;
				case "retargetable":
				{
					if (parsedName.Retargetable.HasValue)
					{
						return ParseAssemblyResult.DuplicateKey;
					}
					string text = value.ToLowerInvariant();
					if (!(text == "yes"))
					{
						if (!(text == "no"))
						{
							return ParseAssemblyResult.GenericError;
						}
						parsedName.Retargetable = false;
					}
					else
					{
						parsedName.Retargetable = true;
					}
					break;
				}
				case "processorarchitecture":
					if (flag)
					{
						return ParseAssemblyResult.DuplicateKey;
					}
					flag = true;
					switch (value.ToLowerInvariant())
					{
					case "none":
						parsedName.ProcessorArchitecture = ProcessorArchitecture.None;
						break;
					case "msil":
						parsedName.ProcessorArchitecture = ProcessorArchitecture.MSIL;
						break;
					case "x86":
						parsedName.ProcessorArchitecture = ProcessorArchitecture.X86;
						break;
					case "ia64":
						parsedName.ProcessorArchitecture = ProcessorArchitecture.IA64;
						break;
					case "amd64":
						parsedName.ProcessorArchitecture = ProcessorArchitecture.Amd64;
						break;
					case "arm":
						parsedName.ProcessorArchitecture = ProcessorArchitecture.Arm;
						break;
					default:
						return ParseAssemblyResult.GenericError;
					}
					break;
				case "contenttype":
					if (flag2)
					{
						return ParseAssemblyResult.DuplicateKey;
					}
					flag2 = true;
					if (!value.Equals("windowsruntime", StringComparison.OrdinalIgnoreCase))
					{
						return ParseAssemblyResult.GenericError;
					}
					parsedName.WindowsRuntime = true;
					break;
				default:
					if (key.Length == 0)
					{
						return ParseAssemblyResult.GenericError;
					}
					if (dictionary == null)
					{
						dictionary = new Dictionary<string, string>();
					}
					if (dictionary.ContainsKey(key))
					{
						return ParseAssemblyResult.DuplicateKey;
					}
					dictionary.Add(key, null);
					break;
				}
			}
			return ParseAssemblyResult.OK;
		}

		private static bool ParseVersion(string str, out Version version)
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
				if (array.Length == 4 && array[3] != "" && array[2] != "")
				{
					version = new Version(result4, result5, result2, result3);
				}
				else if (array.Length == 3 && array[2] != "")
				{
					version = new Version(result4, result5, result2);
				}
				else
				{
					version = new Version(result4, result5);
				}
				return true;
			}
			version = null;
			return false;
		}

		private static bool ParseCulture(string str, out string culture)
		{
			if (str == null)
			{
				culture = null;
				return false;
			}
			culture = str;
			return true;
		}

		private static bool ParsePublicKeyToken(string str, out string publicKeyToken)
		{
			if (str == null)
			{
				publicKeyToken = null;
				return false;
			}
			publicKeyToken = str.ToLowerInvariant();
			return true;
		}

		private static bool ParsePublicKey(string str, out string publicKeyToken)
		{
			if (str == null)
			{
				publicKeyToken = null;
				return false;
			}
			publicKeyToken = AssemblyName.ComputePublicKeyToken(str);
			return true;
		}

		private static bool IsPartial(ParsedAssemblyName name, Version version)
		{
			if (!(version == null) && name.Culture != null)
			{
				return name.PublicKeyToken == null;
			}
			return true;
		}

		private static bool IsStrongNamed(ParsedAssemblyName name)
		{
			if (name.PublicKeyToken != null)
			{
				return name.PublicKeyToken != "null";
			}
			return false;
		}
	}
}
