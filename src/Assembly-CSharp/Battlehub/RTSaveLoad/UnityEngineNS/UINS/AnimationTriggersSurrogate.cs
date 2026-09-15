using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.UnityEngineNS.UINS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class AnimationTriggersSurrogate : ISerializationSurrogate
	{
		public string normalTrigger;

		public string highlightedTrigger;

		public string pressedTrigger;

		public string disabledTrigger;

		public static implicit operator AnimationTriggers(AnimationTriggersSurrogate v)
		{
			AnimationTriggers animationTriggers = new AnimationTriggers();
			animationTriggers.normalTrigger = v.normalTrigger;
			animationTriggers.highlightedTrigger = v.highlightedTrigger;
			animationTriggers.pressedTrigger = v.pressedTrigger;
			animationTriggers.disabledTrigger = v.disabledTrigger;
			return animationTriggers;
		}

		public static implicit operator AnimationTriggersSurrogate(AnimationTriggers v)
		{
			AnimationTriggersSurrogate animationTriggersSurrogate = new AnimationTriggersSurrogate();
			animationTriggersSurrogate.normalTrigger = v.normalTrigger;
			animationTriggersSurrogate.highlightedTrigger = v.highlightedTrigger;
			animationTriggersSurrogate.pressedTrigger = v.pressedTrigger;
			animationTriggersSurrogate.disabledTrigger = v.disabledTrigger;
			return animationTriggersSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			AnimationTriggers animationTriggers = (AnimationTriggers)obj;
			info.AddValue("normalTrigger", animationTriggers.normalTrigger);
			info.AddValue("highlightedTrigger", animationTriggers.highlightedTrigger);
			info.AddValue("pressedTrigger", animationTriggers.pressedTrigger);
			info.AddValue("disabledTrigger", animationTriggers.disabledTrigger);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			AnimationTriggers animationTriggers = (AnimationTriggers)obj;
			animationTriggers.normalTrigger = (string)info.GetValue("normalTrigger", typeof(string));
			animationTriggers.highlightedTrigger = (string)info.GetValue("highlightedTrigger", typeof(string));
			animationTriggers.pressedTrigger = (string)info.GetValue("pressedTrigger", typeof(string));
			animationTriggers.disabledTrigger = (string)info.GetValue("disabledTrigger", typeof(string));
			return animationTriggers;
		}
	}
}
