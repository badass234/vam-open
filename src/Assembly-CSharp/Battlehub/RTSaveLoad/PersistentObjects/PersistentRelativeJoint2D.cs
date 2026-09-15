using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentRelativeJoint2D : PersistentJoint2D
	{
		public float maxForce;

		public float maxTorque;

		public float correctionScale;

		public bool autoConfigureOffset;

		public Vector2 linearOffset;

		public float angularOffset;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			RelativeJoint2D relativeJoint2D = (RelativeJoint2D)obj;
			relativeJoint2D.maxForce = maxForce;
			relativeJoint2D.maxTorque = maxTorque;
			relativeJoint2D.correctionScale = correctionScale;
			relativeJoint2D.autoConfigureOffset = autoConfigureOffset;
			relativeJoint2D.linearOffset = linearOffset;
			relativeJoint2D.angularOffset = angularOffset;
			return relativeJoint2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				RelativeJoint2D relativeJoint2D = (RelativeJoint2D)obj;
				maxForce = relativeJoint2D.maxForce;
				maxTorque = relativeJoint2D.maxTorque;
				correctionScale = relativeJoint2D.correctionScale;
				autoConfigureOffset = relativeJoint2D.autoConfigureOffset;
				linearOffset = relativeJoint2D.linearOffset;
				angularOffset = relativeJoint2D.angularOffset;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
