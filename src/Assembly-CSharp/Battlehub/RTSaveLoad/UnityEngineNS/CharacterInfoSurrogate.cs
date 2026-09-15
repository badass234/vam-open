using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class CharacterInfoSurrogate : ISerializationSurrogate
	{
		public int index;

		public int size;

		public uint style;

		public int advance;

		public int glyphWidth;

		public int glyphHeight;

		public int bearing;

		public int minY;

		public int maxY;

		public int minX;

		public int maxX;

		public Vector2 uvBottomLeft;

		public Vector2 uvBottomRight;

		public Vector2 uvTopRight;

		public Vector2 uvTopLeft;

		public static implicit operator CharacterInfo(CharacterInfoSurrogate v)
		{
			CharacterInfo result = default(CharacterInfo);
			result.index = v.index;
			result.size = v.size;
			result.style = (FontStyle)v.style;
			result.advance = v.advance;
			result.glyphWidth = v.glyphWidth;
			result.glyphHeight = v.glyphHeight;
			result.bearing = v.bearing;
			result.minY = v.minY;
			result.maxY = v.maxY;
			result.minX = v.minX;
			result.maxX = v.maxX;
			result.uvBottomLeft = v.uvBottomLeft;
			result.uvBottomRight = v.uvBottomRight;
			result.uvTopRight = v.uvTopRight;
			result.uvTopLeft = v.uvTopLeft;
			return result;
		}

		public static implicit operator CharacterInfoSurrogate(CharacterInfo v)
		{
			CharacterInfoSurrogate characterInfoSurrogate = new CharacterInfoSurrogate();
			characterInfoSurrogate.index = v.index;
			characterInfoSurrogate.size = v.size;
			characterInfoSurrogate.style = (uint)v.style;
			characterInfoSurrogate.advance = v.advance;
			characterInfoSurrogate.glyphWidth = v.glyphWidth;
			characterInfoSurrogate.glyphHeight = v.glyphHeight;
			characterInfoSurrogate.bearing = v.bearing;
			characterInfoSurrogate.minY = v.minY;
			characterInfoSurrogate.maxY = v.maxY;
			characterInfoSurrogate.minX = v.minX;
			characterInfoSurrogate.maxX = v.maxX;
			characterInfoSurrogate.uvBottomLeft = v.uvBottomLeft;
			characterInfoSurrogate.uvBottomRight = v.uvBottomRight;
			characterInfoSurrogate.uvTopRight = v.uvTopRight;
			characterInfoSurrogate.uvTopLeft = v.uvTopLeft;
			return characterInfoSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			CharacterInfo characterInfo = (CharacterInfo)obj;
			info.AddValue("index", characterInfo.index);
			info.AddValue("size", characterInfo.size);
			info.AddValue("style", characterInfo.style);
			info.AddValue("advance", characterInfo.advance);
			info.AddValue("glyphWidth", characterInfo.glyphWidth);
			info.AddValue("glyphHeight", characterInfo.glyphHeight);
			info.AddValue("bearing", characterInfo.bearing);
			info.AddValue("minY", characterInfo.minY);
			info.AddValue("maxY", characterInfo.maxY);
			info.AddValue("minX", characterInfo.minX);
			info.AddValue("maxX", characterInfo.maxX);
			info.AddValue("uvBottomLeft", characterInfo.uvBottomLeft);
			info.AddValue("uvBottomRight", characterInfo.uvBottomRight);
			info.AddValue("uvTopRight", characterInfo.uvTopRight);
			info.AddValue("uvTopLeft", characterInfo.uvTopLeft);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			CharacterInfo characterInfo = (CharacterInfo)obj;
			characterInfo.index = (int)info.GetValue("index", typeof(int));
			characterInfo.size = (int)info.GetValue("size", typeof(int));
			characterInfo.style = (FontStyle)info.GetValue("style", typeof(FontStyle));
			characterInfo.advance = (int)info.GetValue("advance", typeof(int));
			characterInfo.glyphWidth = (int)info.GetValue("glyphWidth", typeof(int));
			characterInfo.glyphHeight = (int)info.GetValue("glyphHeight", typeof(int));
			characterInfo.bearing = (int)info.GetValue("bearing", typeof(int));
			characterInfo.minY = (int)info.GetValue("minY", typeof(int));
			characterInfo.maxY = (int)info.GetValue("maxY", typeof(int));
			characterInfo.minX = (int)info.GetValue("minX", typeof(int));
			characterInfo.maxX = (int)info.GetValue("maxX", typeof(int));
			characterInfo.uvBottomLeft = (Vector2)info.GetValue("uvBottomLeft", typeof(Vector2));
			characterInfo.uvBottomRight = (Vector2)info.GetValue("uvBottomRight", typeof(Vector2));
			characterInfo.uvTopRight = (Vector2)info.GetValue("uvTopRight", typeof(Vector2));
			characterInfo.uvTopLeft = (Vector2)info.GetValue("uvTopLeft", typeof(Vector2));
			return characterInfo;
		}
	}
}
