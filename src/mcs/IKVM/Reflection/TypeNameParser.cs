using System;
using System.Text;

namespace IKVM.Reflection
{
	internal struct TypeNameParser
	{
		private struct Parser
		{
			private readonly string typeName;

			internal int pos;

			internal Parser(string typeName)
			{
				this.typeName = typeName;
				pos = 0;
			}

			private void Check(bool condition)
			{
				if (!condition)
				{
					throw new ArgumentException("Invalid type name '" + typeName + "'");
				}
			}

			private void Consume(char c)
			{
				Check(pos < typeName.Length && typeName[pos++] == c);
			}

			private bool TryConsume(char c)
			{
				if (pos < typeName.Length && typeName[pos] == c)
				{
					pos++;
					return true;
				}
				return false;
			}

			internal string NextNamePart()
			{
				SkipWhiteSpace();
				int num = pos;
				while (pos < typeName.Length)
				{
					char c = typeName[pos];
					if (c == '\\')
					{
						pos++;
						Check(pos < typeName.Length && "\\+,[]*&".IndexOf(typeName[pos]) != -1);
					}
					else if ("\\+,[]*&".IndexOf(c) != -1)
					{
						break;
					}
					pos++;
				}
				Check(pos - num != 0);
				if (num == 0 && pos == typeName.Length)
				{
					return typeName;
				}
				return typeName.Substring(num, pos - num);
			}

			internal void ParseNested(ref string[] nested)
			{
				while (TryConsume('+'))
				{
					Add(ref nested, NextNamePart());
				}
			}

			internal void ParseGenericParameters(ref TypeNameParser[] genericParameters)
			{
				int num = pos;
				if (!TryConsume('['))
				{
					return;
				}
				SkipWhiteSpace();
				if (TryConsume(']') || TryConsume('*') || TryConsume(','))
				{
					pos = num;
					return;
				}
				do
				{
					SkipWhiteSpace();
					if (TryConsume('['))
					{
						Add(ref genericParameters, new TypeNameParser(ref this, true));
						Consume(']');
					}
					else
					{
						Add(ref genericParameters, new TypeNameParser(ref this, false));
					}
				}
				while (TryConsume(','));
				Consume(']');
				SkipWhiteSpace();
			}

			internal void ParseModifiers(ref short[] modifiers)
			{
				while (pos < typeName.Length)
				{
					switch (typeName[pos])
					{
					default:
						return;
					case '*':
						pos++;
						Add<short>(ref modifiers, -3);
						break;
					case '&':
						pos++;
						Add<short>(ref modifiers, -2);
						break;
					case '[':
						pos++;
						Add(ref modifiers, ParseArray());
						Consume(']');
						break;
					}
					SkipWhiteSpace();
				}
			}

			internal void ParseAssemblyName(bool genericParameter, ref string assemblyName)
			{
				if (pos < typeName.Length)
				{
					if ((typeName[pos] == ']') & genericParameter)
					{
						return;
					}
					Consume(',');
					SkipWhiteSpace();
					if (genericParameter)
					{
						int num = pos;
						while (pos < typeName.Length)
						{
							switch (typeName[pos])
							{
							case '\\':
								pos++;
								Check(pos < typeName.Length && typeName[pos++] == ']');
								continue;
							default:
								pos++;
								continue;
							case ']':
								break;
							}
							break;
						}
						Check(pos < typeName.Length && typeName[pos] == ']');
						assemblyName = typeName.Substring(num, pos - num).Replace("\\]", "]");
					}
					else
					{
						assemblyName = typeName.Substring(pos);
					}
					Check(assemblyName.Length != 0);
				}
				else
				{
					Check(!genericParameter);
				}
			}

			private short ParseArray()
			{
				SkipWhiteSpace();
				Check(pos < typeName.Length);
				switch (typeName[pos])
				{
				case ']':
					return -1;
				case '*':
					pos++;
					SkipWhiteSpace();
					return 1;
				default:
				{
					short num = 1;
					while (TryConsume(','))
					{
						Check(num < short.MaxValue);
						num++;
						SkipWhiteSpace();
					}
					return num;
				}
				}
			}

			private void SkipWhiteSpace()
			{
				while (pos < typeName.Length && char.IsWhiteSpace(typeName[pos]))
				{
					pos++;
				}
			}

			private static void Add<T>(ref T[] array, T elem)
			{
				if (array == null)
				{
					array = new T[1] { elem };
				}
				else
				{
					Array.Resize(ref array, array.Length + 1);
					T[] obj = array;
					obj[obj.Length - 1] = elem;
				}
			}
		}

		private const string SpecialChars = "\\+,[]*&";

		private const short SZARRAY = -1;

		private const short BYREF = -2;

		private const short POINTER = -3;

		private readonly string name;

		private readonly string[] nested;

		private readonly string assemblyName;

		private readonly short[] modifiers;

		private readonly TypeNameParser[] genericParameters;

		internal bool Error => name == null;

		internal string FirstNamePart => name;

		internal string AssemblyName => assemblyName;

		internal static string Escape(string name)
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

		internal static string Unescape(string name)
		{
			int i = name.IndexOf('\\');
			if (i == -1)
			{
				return name;
			}
			StringBuilder stringBuilder = new StringBuilder(name, 0, i, name.Length - 1);
			for (; i < name.Length; i++)
			{
				char c = name[i];
				if (c == '\\')
				{
					c = name[++i];
				}
				stringBuilder.Append(c);
			}
			return stringBuilder.ToString();
		}

		internal static TypeNameParser Parse(string typeName, bool throwOnError)
		{
			if (throwOnError)
			{
				Parser parser = new Parser(typeName);
				return new TypeNameParser(ref parser, true);
			}
			try
			{
				Parser parser2 = new Parser(typeName);
				return new TypeNameParser(ref parser2, true);
			}
			catch (ArgumentException)
			{
				return default(TypeNameParser);
			}
		}

		private TypeNameParser(ref Parser parser, bool withAssemblyName)
		{
			bool genericParameter = parser.pos != 0;
			name = parser.NextNamePart();
			nested = null;
			parser.ParseNested(ref nested);
			genericParameters = null;
			parser.ParseGenericParameters(ref genericParameters);
			modifiers = null;
			parser.ParseModifiers(ref modifiers);
			assemblyName = null;
			if (withAssemblyName)
			{
				parser.ParseAssemblyName(genericParameter, ref assemblyName);
			}
		}

		internal Type GetType(Universe universe, Module context, bool throwOnError, string originalName, bool resolve, bool ignoreCase)
		{
			TypeName typeName = TypeName.Split(name);
			Type type;
			if (assemblyName != null)
			{
				Assembly assembly = universe.Load(assemblyName, context, throwOnError);
				if (assembly == null)
				{
					return null;
				}
				type = (resolve ? assembly.ResolveType(context, typeName) : ((!ignoreCase) ? assembly.FindType(typeName) : assembly.FindTypeIgnoreCase(typeName.ToLowerInvariant())));
			}
			else if (context == null)
			{
				type = (resolve ? universe.Mscorlib.ResolveType(context, typeName) : ((!ignoreCase) ? universe.Mscorlib.FindType(typeName) : universe.Mscorlib.FindTypeIgnoreCase(typeName.ToLowerInvariant())));
			}
			else
			{
				if (ignoreCase)
				{
					typeName = typeName.ToLowerInvariant();
					type = context.FindTypeIgnoreCase(typeName);
				}
				else
				{
					type = context.FindType(typeName);
				}
				if (type == null && context != universe.Mscorlib.ManifestModule)
				{
					type = ((!ignoreCase) ? universe.Mscorlib.FindType(typeName) : universe.Mscorlib.FindTypeIgnoreCase(typeName));
				}
				if ((type == null) & resolve)
				{
					type = ((!universe.Mscorlib.__IsMissing || context.__IsMissing) ? context.Assembly.ResolveType(context, typeName) : universe.Mscorlib.ResolveType(context, typeName));
				}
			}
			return Expand(type, context, throwOnError, originalName, resolve, ignoreCase);
		}

		internal Type Expand(Type type, Module context, bool throwOnError, string originalName, bool resolve, bool ignoreCase)
		{
			if (type == null)
			{
				if (throwOnError)
				{
					throw new TypeLoadException(originalName);
				}
				return null;
			}
			if (nested != null)
			{
				string[] array = nested;
				foreach (string obj in array)
				{
					Type type2 = type;
					TypeName typeName = TypeName.Split(Unescape(obj));
					type = (ignoreCase ? type2.FindNestedTypeIgnoreCase(typeName.ToLowerInvariant()) : type2.FindNestedType(typeName));
					if (!(type == null))
					{
						continue;
					}
					if (resolve)
					{
						type = type2.Module.universe.GetMissingTypeOrThrow(context, type2.Module, type2, typeName);
						continue;
					}
					if (throwOnError)
					{
						throw new TypeLoadException(originalName);
					}
					return null;
				}
			}
			if (genericParameters != null)
			{
				Type[] array2 = new Type[genericParameters.Length];
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j] = genericParameters[j].GetType(type.Assembly.universe, context, throwOnError, originalName, resolve, ignoreCase);
					if (array2[j] == null)
					{
						return null;
					}
				}
				type = type.MakeGenericType(array2);
			}
			if (modifiers != null)
			{
				short[] array3 = modifiers;
				foreach (short num in array3)
				{
					switch (num)
					{
					case -1:
						type = type.MakeArrayType();
						break;
					case -2:
						type = type.MakeByRefType();
						break;
					case -3:
						type = type.MakePointerType();
						break;
					default:
						type = type.MakeArrayType(num);
						break;
					}
				}
			}
			return type;
		}
	}
}
