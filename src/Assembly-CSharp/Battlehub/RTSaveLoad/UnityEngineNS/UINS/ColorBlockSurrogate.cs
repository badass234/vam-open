using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.UnityEngineNS.UINS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class ColorBlockSurrogate : ISerializationSurrogate
	{
		public Color normalColor;

		public Color highlightedColor;

		public Color pressedColor;

		public Color disabledColor;

		public float colorMultiplier;

		public float fadeDuration;

		public static implicit operator ColorBlock(ColorBlockSurrogate v)
		{
			ColorBlock result = default(ColorBlock);
			result.normalColor = v.normalColor;
			result.highlightedColor = v.highlightedColor;
			result.pressedColor = v.pressedColor;
			result.disabledColor = v.disabledColor;
			result.colorMultiplier = v.colorMultiplier;
			result.fadeDuration = v.fadeDuration;
			return result;
		}

		public static implicit operator ColorBlockSurrogate(ColorBlock v)
		{
			ColorBlockSurrogate colorBlockSurrogate = new ColorBlockSurrogate();
			colorBlockSurrogate.normalColor = v.normalColor;
			colorBlockSurrogate.highlightedColor = v.highlightedColor;
			colorBlockSurrogate.pressedColor = v.pressedColor;
			colorBlockSurrogate.disabledColor = v.disabledColor;
			colorBlockSurrogate.colorMultiplier = v.colorMultiplier;
			colorBlockSurrogate.fadeDuration = v.fadeDuration;
			return colorBlockSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			ColorBlock colorBlock = (ColorBlock)obj;
			info.AddValue("normalColor", colorBlock.normalColor);
			info.AddValue("highlightedColor", colorBlock.highlightedColor);
			info.AddValue("pressedColor", colorBlock.pressedColor);
			info.AddValue("disabledColor", colorBlock.disabledColor);
			info.AddValue("colorMultiplier", colorBlock.colorMultiplier);
			info.AddValue("fadeDuration", colorBlock.fadeDuration);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			ColorBlock colorBlock = (ColorBlock)obj;
			colorBlock.normalColor = (Color)info.GetValue("normalColor", typeof(Color));
			colorBlock.highlightedColor = (Color)info.GetValue("highlightedColor", typeof(Color));
			colorBlock.pressedColor = (Color)info.GetValue("pressedColor", typeof(Color));
			colorBlock.disabledColor = (Color)info.GetValue("disabledColor", typeof(Color));
			colorBlock.colorMultiplier = (float)info.GetValue("colorMultiplier", typeof(float));
			colorBlock.fadeDuration = (float)info.GetValue("fadeDuration", typeof(float));
			return colorBlock;
		}
	}
}
