using System;
using System.IO;
using ProtoBuf.Meta;

namespace Battlehub.RTSaveLoad
{
	public static class ProtobufSerializer
	{
		private static RTTypeModel model;

		static ProtobufSerializer()
		{
			model = new RTTypeModel();
			model.DynamicTypeFormatting += delegate(object sender, TypeFormatEventArgs args)
			{
				if (args.FormattedName != null && Type.GetType(args.FormattedName) == null)
				{
					args.Type = typeof(NilContainer);
				}
			};
		}

		public static TData DeepClone<TData>(TData data)
		{
			return (TData)model.DeepClone(data);
		}

		public static TData Deserialize<TData>(byte[] b)
		{
			using (MemoryStream source = new MemoryStream(b))
			{
				return (TData)model.Deserialize(source, null, typeof(TData));
			}
		}

		public static byte[] Serialize<TData>(TData data)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				model.Serialize(memoryStream, data);
				memoryStream.Flush();
				memoryStream.Position = 0L;
				return memoryStream.ToArray();
			}
		}
	}
}
