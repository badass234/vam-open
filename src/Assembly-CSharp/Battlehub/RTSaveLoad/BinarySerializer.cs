using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Battlehub.RTSaveLoad
{
	public static class BinarySerializer
	{
		public static TData Deserialize<TData>(byte[] b)
		{
			using (MemoryStream memoryStream = new MemoryStream(b))
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				binaryFormatter.SurrogateSelector = SerializationSurrogates.CreateSelector();
				memoryStream.Seek(0L, SeekOrigin.Begin);
				object obj = binaryFormatter.Deserialize(memoryStream);
				return (TData)obj;
			}
		}

		public static TData DeserializeFromString<TData>(string data)
		{
			return Deserialize<TData>(Convert.FromBase64String(data));
		}

		public static byte[] Serialize<TData>(TData settings)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				binaryFormatter.SurrogateSelector = SerializationSurrogates.CreateSelector();
				binaryFormatter.Serialize(memoryStream, settings);
				memoryStream.Flush();
				memoryStream.Position = 0L;
				return memoryStream.ToArray();
			}
		}

		public static string SerializeToString<TData>(TData settings)
		{
			return Convert.ToBase64String(Serialize(settings));
		}
	}
}
