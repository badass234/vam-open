using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class TreeInstanceSurrogate : ISerializationSurrogate
	{
		public Vector3 position;

		public float widthScale;

		public float heightScale;

		public float rotation;

		public Color32 color;

		public Color32 lightmapColor;

		public int prototypeIndex;

		public static implicit operator TreeInstance(TreeInstanceSurrogate v)
		{
			TreeInstance result = default(TreeInstance);
			result.position = v.position;
			result.widthScale = v.widthScale;
			result.heightScale = v.heightScale;
			result.rotation = v.rotation;
			result.color = v.color;
			result.lightmapColor = v.lightmapColor;
			result.prototypeIndex = v.prototypeIndex;
			return result;
		}

		public static implicit operator TreeInstanceSurrogate(TreeInstance v)
		{
			TreeInstanceSurrogate treeInstanceSurrogate = new TreeInstanceSurrogate();
			treeInstanceSurrogate.position = v.position;
			treeInstanceSurrogate.widthScale = v.widthScale;
			treeInstanceSurrogate.heightScale = v.heightScale;
			treeInstanceSurrogate.rotation = v.rotation;
			treeInstanceSurrogate.color = v.color;
			treeInstanceSurrogate.lightmapColor = v.lightmapColor;
			treeInstanceSurrogate.prototypeIndex = v.prototypeIndex;
			return treeInstanceSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			TreeInstance treeInstance = (TreeInstance)obj;
			info.AddValue("position", treeInstance.position);
			info.AddValue("widthScale", treeInstance.widthScale);
			info.AddValue("heightScale", treeInstance.heightScale);
			info.AddValue("rotation", treeInstance.rotation);
			info.AddValue("color", treeInstance.color);
			info.AddValue("lightmapColor", treeInstance.lightmapColor);
			info.AddValue("prototypeIndex", treeInstance.prototypeIndex);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			TreeInstance treeInstance = (TreeInstance)obj;
			treeInstance.position = (Vector3)info.GetValue("position", typeof(Vector3));
			treeInstance.widthScale = (float)info.GetValue("widthScale", typeof(float));
			treeInstance.heightScale = (float)info.GetValue("heightScale", typeof(float));
			treeInstance.rotation = (float)info.GetValue("rotation", typeof(float));
			treeInstance.color = (Color32)info.GetValue("color", typeof(Color32));
			treeInstance.lightmapColor = (Color32)info.GetValue("lightmapColor", typeof(Color32));
			treeInstance.prototypeIndex = (int)info.GetValue("prototypeIndex", typeof(int));
			return treeInstance;
		}
	}
}
