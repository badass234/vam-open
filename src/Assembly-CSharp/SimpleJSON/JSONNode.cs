using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using MVR.FileManagement;

namespace SimpleJSON
{
	public class JSONNode
	{
		internal static StringBuilder escapeStringBuilder;

		public virtual JSONNode this[int aIndex]
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public virtual JSONNode this[string aKey]
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public virtual string Value
		{
			get
			{
				return string.Empty;
			}
			set
			{
			}
		}

		public virtual int Count => 0;

		public virtual IEnumerable<JSONNode> Childs
		{
			get
			{
				yield break;
			}
		}

		public IEnumerable<JSONNode> DeepChilds
		{
			get
			{
				foreach (JSONNode C in Childs)
				{
					foreach (JSONNode deepChild in C.DeepChilds)
					{
						yield return deepChild;
					}
				}
			}
		}

		public virtual int AsInt
		{
			get
			{
				int result = 0;
				if (int.TryParse(Value, out result))
				{
					return result;
				}
				return 0;
			}
			set
			{
				Value = value.ToString();
			}
		}

		public virtual float AsFloat
		{
			get
			{
				float result = 0f;
				if (float.TryParse(Value, out result))
				{
					return result;
				}
				return 0f;
			}
			set
			{
				Value = value.ToString();
			}
		}

		public virtual double AsDouble
		{
			get
			{
				double result = 0.0;
				if (double.TryParse(Value, out result))
				{
					return result;
				}
				return 0.0;
			}
			set
			{
				Value = value.ToString();
			}
		}

		public virtual bool AsBool
		{
			get
			{
				bool result = false;
				if (bool.TryParse(Value, out result))
				{
					return result;
				}
				return !string.IsNullOrEmpty(Value);
			}
			set
			{
				Value = ((!value) ? "false" : "true");
			}
		}

		public virtual JSONArray AsArray => this as JSONArray;

		public virtual JSONClass AsObject => this as JSONClass;

		public virtual void Add(string aKey, JSONNode aItem)
		{
		}

		public virtual void Add(JSONNode aItem)
		{
			Add(string.Empty, aItem);
		}

		public virtual JSONNode Remove(string aKey)
		{
			return null;
		}

		public virtual JSONNode Remove(int aIndex)
		{
			return null;
		}

		public virtual JSONNode Remove(JSONNode aNode)
		{
			return aNode;
		}

		public override string ToString()
		{
			return "JSONNode";
		}

		public virtual string ToString(string aPrefix)
		{
			return "JSONNode";
		}

		public virtual void ToString(string aPrefix, StringBuilder sb)
		{
		}

		public static implicit operator JSONNode(string s)
		{
			return new JSONData(s);
		}

		public static implicit operator string(JSONNode d)
		{
			return (!(d == null)) ? d.Value : null;
		}

		public static bool operator ==(JSONNode a, object b)
		{
			if (b == null && a is JSONLazyCreator)
			{
				return true;
			}
			return object.ReferenceEquals(a, b);
		}

		public static bool operator !=(JSONNode a, object b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			return object.ReferenceEquals(this, obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		internal static string Escape(string aText)
		{
			if (escapeStringBuilder == null)
			{
				escapeStringBuilder = new StringBuilder(4096);
			}
			else
			{
				escapeStringBuilder.Length = 0;
			}
			if (aText != null)
			{
				foreach (char c in aText)
				{
					switch (c)
					{
					case '\\':
						escapeStringBuilder.Append("\\\\");
						break;
					case '"':
						escapeStringBuilder.Append("\\\"");
						break;
					case '\n':
						escapeStringBuilder.Append("\\n");
						break;
					case '\r':
						escapeStringBuilder.Append("\\r");
						break;
					case '\t':
						escapeStringBuilder.Append("\\t");
						break;
					case '\b':
						escapeStringBuilder.Append("\\b");
						break;
					case '\f':
						escapeStringBuilder.Append("\\f");
						break;
					default:
						escapeStringBuilder.Append(c);
						break;
					}
				}
			}
			return escapeStringBuilder.ToString();
		}

		internal static void EscapeAppend(string aText, StringBuilder sb)
		{
			if (aText == null)
			{
				return;
			}
			foreach (char c in aText)
			{
				switch (c)
				{
				case '\\':
					sb.Append("\\\\");
					break;
				case '"':
					sb.Append("\\\"");
					break;
				case '\n':
					sb.Append("\\n");
					break;
				case '\r':
					sb.Append("\\r");
					break;
				case '\t':
					sb.Append("\\t");
					break;
				case '\b':
					sb.Append("\\b");
					break;
				case '\f':
					sb.Append("\\f");
					break;
				default:
					sb.Append(c);
					break;
				}
			}
		}

		public static JSONNode Parse(string aJSON)
		{
			Stack<JSONNode> stack = new Stack<JSONNode>();
			JSONNode jSONNode = null;
			int i = 0;
			bool flag = false;
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			bool flag2 = false;
			for (; i < aJSON.Length; i++)
			{
				switch (aJSON[i])
				{
				case '{':
					if (flag2)
					{
						stringBuilder.Append(aJSON[i]);
						break;
					}
					stack.Push(new JSONClass());
					if (jSONNode != null)
					{
						if (jSONNode is JSONArray)
						{
							jSONNode.Add(stack.Peek());
						}
						else if (stringBuilder2.Length > 0)
						{
							string text = stringBuilder2.ToString().Trim();
							if (text.Length > 0)
							{
								jSONNode.Add(text, stack.Peek());
							}
						}
					}
					stringBuilder2.Length = 0;
					stringBuilder.Length = 0;
					flag = false;
					jSONNode = stack.Peek();
					break;
				case '[':
					if (flag2)
					{
						stringBuilder.Append(aJSON[i]);
						break;
					}
					stack.Push(new JSONArray());
					if (jSONNode != null)
					{
						if (jSONNode is JSONArray)
						{
							jSONNode.Add(stack.Peek());
						}
						else if (stringBuilder2.Length > 0)
						{
							string text2 = stringBuilder2.ToString().Trim();
							if (text2.Length > 0)
							{
								jSONNode.Add(text2, stack.Peek());
							}
						}
					}
					stringBuilder2.Length = 0;
					stringBuilder.Length = 0;
					flag = false;
					jSONNode = stack.Peek();
					break;
				case ']':
				case '}':
					if (flag2)
					{
						stringBuilder.Append(aJSON[i]);
						break;
					}
					if (stack.Count == 0)
					{
						throw new Exception("JSON Parse: Too many closing brackets");
					}
					stack.Pop();
					if (flag)
					{
						if (jSONNode is JSONArray)
						{
							jSONNode.Add(stringBuilder.ToString());
						}
						else if (stringBuilder2.Length > 0)
						{
							string text3 = stringBuilder2.ToString().Trim();
							if (text3.Length > 0)
							{
								jSONNode.Add(text3, stringBuilder.ToString());
							}
						}
					}
					stringBuilder2.Length = 0;
					stringBuilder.Length = 0;
					flag = false;
					if (stack.Count > 0)
					{
						jSONNode = stack.Peek();
					}
					break;
				case ':':
					if (flag2)
					{
						stringBuilder.Append(aJSON[i]);
						break;
					}
					stringBuilder2.Length = 0;
					stringBuilder2.Append(stringBuilder);
					stringBuilder.Length = 0;
					flag = false;
					break;
				case '"':
					flag2 = (byte)((flag2 ? 1u : 0u) ^ 1u) != 0;
					flag = true;
					break;
				case ',':
					if (flag2)
					{
						stringBuilder.Append(aJSON[i]);
						break;
					}
					if (flag)
					{
						if (jSONNode is JSONArray)
						{
							jSONNode.Add(stringBuilder.ToString());
						}
						else if (stringBuilder2.Length > 0)
						{
							jSONNode.Add(stringBuilder2.ToString(), stringBuilder.ToString());
						}
					}
					stringBuilder2.Length = 0;
					stringBuilder.Length = 0;
					flag = false;
					break;
				case '\t':
				case ' ':
					if (flag2)
					{
						stringBuilder.Append(aJSON[i]);
					}
					break;
				case '\\':
					i++;
					if (flag2)
					{
						char c = aJSON[i];
						switch (c)
						{
						case 't':
							stringBuilder.Append('\t');
							break;
						case 'r':
							stringBuilder.Append('\r');
							break;
						case 'n':
							stringBuilder.Append('\n');
							break;
						case 'b':
							stringBuilder.Append('\b');
							break;
						case 'f':
							stringBuilder.Append('\f');
							break;
						case 'u':
						{
							string s = aJSON.Substring(i + 1, 4);
							stringBuilder.Append((char)int.Parse(s, NumberStyles.AllowHexSpecifier));
							i += 4;
							break;
						}
						default:
							stringBuilder.Append(c);
							break;
						}
					}
					break;
				default:
					stringBuilder.Append(aJSON[i]);
					flag = true;
					break;
				case '\n':
				case '\r':
					break;
				}
			}
			if (flag2)
			{
				throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
			}
			return jSONNode;
		}

		public virtual void Serialize(BinaryWriter aWriter)
		{
		}

		public void SaveToStream(Stream aData)
		{
			BinaryWriter aWriter = new BinaryWriter(aData);
			Serialize(aWriter);
		}

		public void SaveToCompressedStream(Stream aData)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}

		public void SaveToCompressedFile(string aFileName)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}

		public string SaveToCompressedBase64()
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}

		public void SaveToFile(string aFileName)
		{
			if (!FileManager.IsSecurePluginWritePath(aFileName))
			{
				throw new Exception("Plugin attempted to write to non-secure path: " + aFileName);
			}
			using (FileStream aData = File.OpenWrite(aFileName))
			{
				SaveToStream(aData);
			}
		}

		public string SaveToBase64()
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				SaveToStream(memoryStream);
				memoryStream.Position = 0L;
				return Convert.ToBase64String(memoryStream.ToArray());
			}
		}

		public static JSONNode Deserialize(BinaryReader aReader)
		{
			JSONBinaryTag jSONBinaryTag = (JSONBinaryTag)aReader.ReadByte();
			switch (jSONBinaryTag)
			{
			case JSONBinaryTag.Array:
			{
				int num2 = aReader.ReadInt32();
				JSONArray jSONArray = new JSONArray();
				for (int j = 0; j < num2; j++)
				{
					jSONArray.Add(Deserialize(aReader));
				}
				return jSONArray;
			}
			case JSONBinaryTag.Class:
			{
				int num = aReader.ReadInt32();
				JSONClass jSONClass = new JSONClass();
				for (int i = 0; i < num; i++)
				{
					string aKey = aReader.ReadString();
					JSONNode aItem = Deserialize(aReader);
					jSONClass.Add(aKey, aItem);
				}
				return jSONClass;
			}
			case JSONBinaryTag.Value:
				return new JSONData(aReader.ReadString());
			case JSONBinaryTag.IntValue:
				return new JSONData(aReader.ReadInt32());
			case JSONBinaryTag.DoubleValue:
				return new JSONData(aReader.ReadDouble());
			case JSONBinaryTag.BoolValue:
				return new JSONData(aReader.ReadBoolean());
			case JSONBinaryTag.FloatValue:
				return new JSONData(aReader.ReadSingle());
			default:
				throw new Exception("Error deserializing JSON. Unknown tag: " + jSONBinaryTag);
			}
		}

		public static JSONNode LoadFromCompressedFile(string aFileName)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}

		public static JSONNode LoadFromCompressedStream(Stream aData)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}

		public static JSONNode LoadFromCompressedBase64(string aBase64)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}

		public static JSONNode LoadFromStream(Stream aData)
		{
			using (BinaryReader aReader = new BinaryReader(aData))
			{
				return Deserialize(aReader);
			}
		}

		public static JSONNode LoadFromFile(string aFileName)
		{
			using (FileStream aData = File.OpenRead(aFileName))
			{
				return LoadFromStream(aData);
			}
		}

		public static JSONNode LoadFromBase64(string aBase64)
		{
			byte[] buffer = Convert.FromBase64String(aBase64);
			MemoryStream memoryStream = new MemoryStream(buffer);
			memoryStream.Position = 0L;
			return LoadFromStream(memoryStream);
		}
	}
}
