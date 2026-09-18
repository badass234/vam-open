using System;
using System.IO;
using IKVM.Reflection.Emit;

namespace IKVM.Reflection.Writer
{
	internal sealed class VersionInfo
	{
		private AssemblyName name;

		private string fileName;

		internal string copyright;

		internal string trademark;

		internal string product;

		internal string company;

		private string description;

		private string title;

		internal string informationalVersion;

		private string fileVersion;

		internal void SetName(AssemblyName name)
		{
			this.name = name;
		}

		internal void SetFileName(string assemblyFileName)
		{
			fileName = Path.GetFileName(assemblyFileName);
		}

		internal void SetAttribute(AssemblyBuilder asm, CustomAttributeBuilder cab)
		{
			Universe universe = cab.Constructor.Module.universe;
			Type declaringType = cab.Constructor.DeclaringType;
			if (copyright == null && declaringType == universe.System_Reflection_AssemblyCopyrightAttribute)
			{
				copyright = (string)cab.DecodeBlob(asm).GetConstructorArgument(0);
			}
			else if (trademark == null && declaringType == universe.System_Reflection_AssemblyTrademarkAttribute)
			{
				trademark = (string)cab.DecodeBlob(asm).GetConstructorArgument(0);
			}
			else if (product == null && declaringType == universe.System_Reflection_AssemblyProductAttribute)
			{
				product = (string)cab.DecodeBlob(asm).GetConstructorArgument(0);
			}
			else if (company == null && declaringType == universe.System_Reflection_AssemblyCompanyAttribute)
			{
				company = (string)cab.DecodeBlob(asm).GetConstructorArgument(0);
			}
			else if (description == null && declaringType == universe.System_Reflection_AssemblyDescriptionAttribute)
			{
				description = (string)cab.DecodeBlob(asm).GetConstructorArgument(0);
			}
			else if (title == null && declaringType == universe.System_Reflection_AssemblyTitleAttribute)
			{
				title = (string)cab.DecodeBlob(asm).GetConstructorArgument(0);
			}
			else if (informationalVersion == null && declaringType == universe.System_Reflection_AssemblyInformationalVersionAttribute)
			{
				informationalVersion = (string)cab.DecodeBlob(asm).GetConstructorArgument(0);
			}
			else if (fileVersion == null && declaringType == universe.System_Reflection_AssemblyFileVersionAttribute)
			{
				fileVersion = (string)cab.DecodeBlob(asm).GetConstructorArgument(0);
			}
		}

		internal void Write(ByteBuffer bb)
		{
			if (fileVersion == null)
			{
				if (name.Version != null)
				{
					fileVersion = name.Version.ToString();
				}
				else
				{
					fileVersion = "0.0.0.0";
				}
			}
			int num = 1200;
			int num2 = 127;
			try
			{
				if (name.CultureInfo != null)
				{
					num2 = name.CultureInfo.LCID;
				}
			}
			catch (ArgumentException)
			{
			}
			Version version = ParseVersionRobust(fileVersion);
			int major = version.Major;
			int minor = version.Minor;
			int build = version.Build;
			int revision = version.Revision;
			int num3 = major;
			int num4 = minor;
			int num5 = build;
			int num6 = revision;
			if (informationalVersion != null)
			{
				Version version2 = ParseVersionRobust(informationalVersion);
				num3 = version2.Major;
				num4 = version2.Minor;
				num5 = version2.Build;
				num6 = version2.Revision;
			}
			ByteBuffer byteBuffer = new ByteBuffer(512);
			byteBuffer.Write((short)0);
			byteBuffer.Write((short)0);
			byteBuffer.Write((short)1);
			WriteUTF16Z(byteBuffer, $"{num2:x4}{num:x4}");
			byteBuffer.Align(4);
			WriteString(byteBuffer, "Comments", description);
			WriteString(byteBuffer, "CompanyName", company);
			WriteString(byteBuffer, "FileDescription", title);
			WriteString(byteBuffer, "FileVersion", fileVersion);
			WriteString(byteBuffer, "InternalName", name.Name);
			WriteString(byteBuffer, "LegalCopyright", copyright);
			WriteString(byteBuffer, "LegalTrademarks", trademark);
			WriteString(byteBuffer, "OriginalFilename", fileName);
			WriteString(byteBuffer, "ProductName", product);
			WriteString(byteBuffer, "ProductVersion", informationalVersion);
			byteBuffer.Position = 0;
			byteBuffer.Write((short)byteBuffer.Length);
			ByteBuffer byteBuffer2 = new ByteBuffer(512);
			byteBuffer2.Write((short)0);
			byteBuffer2.Write((short)0);
			byteBuffer2.Write((short)1);
			WriteUTF16Z(byteBuffer2, "StringFileInfo");
			byteBuffer2.Align(4);
			byteBuffer2.Write(byteBuffer);
			byteBuffer2.Position = 0;
			byteBuffer2.Write((short)byteBuffer2.Length);
			byte[] array = new byte[46]
			{
				52, 0, 0, 0, 86, 0, 83, 0, 95, 0,
				86, 0, 69, 0, 82, 0, 83, 0, 73, 0,
				79, 0, 78, 0, 95, 0, 73, 0, 78, 0,
				70, 0, 79, 0, 0, 0, 0, 0, 189, 4,
				239, 254, 0, 0, 1, 0
			};
			byte[] array2 = new byte[92]
			{
				63, 0, 0, 0, 0, 0, 0, 0, 4, 0,
				0, 0, 2, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 68, 0,
				0, 0, 1, 0, 86, 0, 97, 0, 114, 0,
				70, 0, 105, 0, 108, 0, 101, 0, 73, 0,
				110, 0, 102, 0, 111, 0, 0, 0, 0, 0,
				36, 0, 4, 0, 0, 0, 84, 0, 114, 0,
				97, 0, 110, 0, 115, 0, 108, 0, 97, 0,
				116, 0, 105, 0, 111, 0, 110, 0, 0, 0,
				0, 0
			};
			bb.Write((short)(2 + array.Length + 8 + 8 + array2.Length + 4 + byteBuffer2.Length));
			bb.Write(array);
			bb.Write((short)minor);
			bb.Write((short)major);
			bb.Write((short)revision);
			bb.Write((short)build);
			bb.Write((short)num4);
			bb.Write((short)num3);
			bb.Write((short)num6);
			bb.Write((short)num5);
			bb.Write(array2);
			bb.Write((short)num2);
			bb.Write((short)num);
			bb.Write(byteBuffer2);
		}

		private static void WriteUTF16Z(ByteBuffer bb, string str)
		{
			foreach (char c in str)
			{
				bb.Write((short)c);
			}
			bb.Write((short)0);
		}

		private static void WriteString(ByteBuffer bb, string name, string value)
		{
			value = value ?? " ";
			int position = bb.Position;
			bb.Write((short)0);
			bb.Write((short)(value.Length + 1));
			bb.Write((short)1);
			WriteUTF16Z(bb, name);
			bb.Align(4);
			WriteUTF16Z(bb, value);
			bb.Align(4);
			int position2 = bb.Position;
			bb.Position = position;
			bb.Write((short)(position2 - position));
			bb.Position = position2;
		}

		private static Version ParseVersionRobust(string ver)
		{
			int pos = 0;
			ushort major = ParseVersionPart(ver, ref pos);
			ushort minor = ParseVersionPart(ver, ref pos);
			ushort build = ParseVersionPart(ver, ref pos);
			ushort revision = ParseVersionPart(ver, ref pos);
			return new Version(major, minor, build, revision);
		}

		private static ushort ParseVersionPart(string str, ref int pos)
		{
			ushort num;
			char c;
			for (num = 0; pos < str.Length; num *= 10, num += (ushort)(c - 48), pos++)
			{
				c = str[pos];
				switch (c)
				{
				case '.':
					pos++;
					break;
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					continue;
				}
				break;
			}
			return num;
		}
	}
}
