using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentCharacterJoint : PersistentJoint
	{
		public Vector3 swingAxis;

		public SoftJointLimitSpring twistLimitSpring;

		public SoftJointLimitSpring swingLimitSpring;

		public SoftJointLimit lowTwistLimit;

		public SoftJointLimit highTwistLimit;

		public SoftJointLimit swing1Limit;

		public SoftJointLimit swing2Limit;

		public bool enableProjection;

		public float projectionDistance;

		public float projectionAngle;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			CharacterJoint characterJoint = (CharacterJoint)obj;
			characterJoint.swingAxis = swingAxis;
			characterJoint.twistLimitSpring = twistLimitSpring;
			characterJoint.swingLimitSpring = swingLimitSpring;
			characterJoint.lowTwistLimit = lowTwistLimit;
			characterJoint.highTwistLimit = highTwistLimit;
			characterJoint.swing1Limit = swing1Limit;
			characterJoint.swing2Limit = swing2Limit;
			characterJoint.enableProjection = enableProjection;
			characterJoint.projectionDistance = projectionDistance;
			characterJoint.projectionAngle = projectionAngle;
			return characterJoint;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				CharacterJoint characterJoint = (CharacterJoint)obj;
				swingAxis = characterJoint.swingAxis;
				twistLimitSpring = characterJoint.twistLimitSpring;
				swingLimitSpring = characterJoint.swingLimitSpring;
				lowTwistLimit = characterJoint.lowTwistLimit;
				highTwistLimit = characterJoint.highTwistLimit;
				swing1Limit = characterJoint.swing1Limit;
				swing2Limit = characterJoint.swing2Limit;
				enableProjection = characterJoint.enableProjection;
				projectionDistance = characterJoint.projectionDistance;
				projectionAngle = characterJoint.projectionAngle;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
