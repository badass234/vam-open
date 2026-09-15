using ProtoBuf;

namespace Battlehub.RTSaveLoad
{
	[ProtoContract(AsReferenceDefault = true)]
	public class DataContract
	{
		[ProtoMember(1, DynamicType = true)]
		public object Data { get; set; }

		public PrimitiveContract AsPrimitive => Data as PrimitiveContract;

		public PersistentUnityEventBase AsUnityEvent => Data as PersistentUnityEventBase;

		public PersistentData AsPersistentData => Data as PersistentData;

		public DataContract()
		{
		}

		public DataContract(PersistentData data)
		{
			Data = data;
		}

		public DataContract(PrimitiveContract primitive)
		{
			Data = primitive;
		}

		public DataContract(PersistentUnityEventBase data)
		{
			Data = data;
		}
	}
}
