using System.Runtime.Serialization;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.UnityEngineNS
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
	public class WheelFrictionCurveSurrogate : ISerializationSurrogate
	{
		public float extremumSlip;

		public float extremumValue;

		public float asymptoteSlip;

		public float asymptoteValue;

		public float stiffness;

		public static implicit operator WheelFrictionCurve(WheelFrictionCurveSurrogate v)
		{
			WheelFrictionCurve result = default(WheelFrictionCurve);
			result.extremumSlip = v.extremumSlip;
			result.extremumValue = v.extremumValue;
			result.asymptoteSlip = v.asymptoteSlip;
			result.asymptoteValue = v.asymptoteValue;
			result.stiffness = v.stiffness;
			return result;
		}

		public static implicit operator WheelFrictionCurveSurrogate(WheelFrictionCurve v)
		{
			WheelFrictionCurveSurrogate wheelFrictionCurveSurrogate = new WheelFrictionCurveSurrogate();
			wheelFrictionCurveSurrogate.extremumSlip = v.extremumSlip;
			wheelFrictionCurveSurrogate.extremumValue = v.extremumValue;
			wheelFrictionCurveSurrogate.asymptoteSlip = v.asymptoteSlip;
			wheelFrictionCurveSurrogate.asymptoteValue = v.asymptoteValue;
			wheelFrictionCurveSurrogate.stiffness = v.stiffness;
			return wheelFrictionCurveSurrogate;
		}

		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			WheelFrictionCurve wheelFrictionCurve = (WheelFrictionCurve)obj;
			info.AddValue("extremumSlip", wheelFrictionCurve.extremumSlip);
			info.AddValue("extremumValue", wheelFrictionCurve.extremumValue);
			info.AddValue("asymptoteSlip", wheelFrictionCurve.asymptoteSlip);
			info.AddValue("asymptoteValue", wheelFrictionCurve.asymptoteValue);
			info.AddValue("stiffness", wheelFrictionCurve.stiffness);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			WheelFrictionCurve wheelFrictionCurve = (WheelFrictionCurve)obj;
			wheelFrictionCurve.extremumSlip = (float)info.GetValue("extremumSlip", typeof(float));
			wheelFrictionCurve.extremumValue = (float)info.GetValue("extremumValue", typeof(float));
			wheelFrictionCurve.asymptoteSlip = (float)info.GetValue("asymptoteSlip", typeof(float));
			wheelFrictionCurve.asymptoteValue = (float)info.GetValue("asymptoteValue", typeof(float));
			wheelFrictionCurve.stiffness = (float)info.GetValue("stiffness", typeof(float));
			return wheelFrictionCurve;
		}
	}
}
