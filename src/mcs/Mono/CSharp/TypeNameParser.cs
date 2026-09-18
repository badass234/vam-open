using System.Runtime.InteropServices;
using System.Text;

namespace Mono.CSharp
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct TypeNameParser
	{
		public static string Escape(string name)
		{
			if (name == null)
			{
				return null;
			}
			StringBuilder stringBuilder = null;
			for (int i = 0; i < name.Length; i++)
			{
				char c = name[i];
				switch (c)
				{
				case '&':
				case '*':
				case '+':
				case ',':
				case '[':
				case '\\':
				case ']':
					if (stringBuilder == null)
					{
						stringBuilder = new StringBuilder(name, 0, i, name.Length + 3);
					}
					stringBuilder.Append("\\").Append(c);
					break;
				default:
					stringBuilder?.Append(c);
					break;
				}
			}
			if (stringBuilder == null)
			{
				return name;
			}
			return stringBuilder.ToString();
		}
	}
}
