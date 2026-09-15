namespace Battlehub.RTSaveLoad
{
	public class Serializer : ISerializer
	{
		public byte[] Serialize<TData>(TData data)
		{
			return ProtobufSerializer.Serialize(data);
		}

		public TData Deserialize<TData>(byte[] data)
		{
			return ProtobufSerializer.Deserialize<TData>(data);
		}

		public TData DeepClone<TData>(TData data)
		{
			return ProtobufSerializer.DeepClone(data);
		}
	}
}
