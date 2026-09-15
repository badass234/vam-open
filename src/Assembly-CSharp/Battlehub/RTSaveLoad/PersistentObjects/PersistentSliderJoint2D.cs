using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentSliderJoint2D : PersistentAnchoredJoint2D
	{
		public bool autoConfigureAngle;

		public float angle;

		public bool useMotor;

		public bool useLimits;

		public JointMotor2D motor;

		public JointTranslationLimits2D limits;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			SliderJoint2D sliderJoint2D = (SliderJoint2D)obj;
			sliderJoint2D.autoConfigureAngle = autoConfigureAngle;
			sliderJoint2D.angle = angle;
			sliderJoint2D.useMotor = useMotor;
			sliderJoint2D.useLimits = useLimits;
			sliderJoint2D.motor = motor;
			sliderJoint2D.limits = limits;
			return sliderJoint2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				SliderJoint2D sliderJoint2D = (SliderJoint2D)obj;
				autoConfigureAngle = sliderJoint2D.autoConfigureAngle;
				angle = sliderJoint2D.angle;
				useMotor = sliderJoint2D.useMotor;
				useLimits = sliderJoint2D.useLimits;
				motor = sliderJoint2D.motor;
				limits = sliderJoint2D.limits;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
