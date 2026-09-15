using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1072, typeof(PersistentHingeJoint))]
	[ProtoInclude(1073, typeof(PersistentSpringJoint))]
	[ProtoInclude(1074, typeof(PersistentFixedJoint))]
	[ProtoInclude(1075, typeof(PersistentCharacterJoint))]
	[ProtoInclude(1076, typeof(PersistentConfigurableJoint))]
	public class PersistentJoint : PersistentComponent
	{
		public long connectedBody;

		public Vector3 axis;

		public Vector3 anchor;

		public Vector3 connectedAnchor;

		public bool autoConfigureConnectedAnchor;

		public float breakForce;

		public float breakTorque;

		public bool enableCollision;

		public bool enablePreprocessing;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Joint joint = (Joint)obj;
			joint.connectedBody = (Rigidbody)objects.Get(connectedBody);
			joint.axis = axis;
			joint.anchor = anchor;
			joint.connectedAnchor = connectedAnchor;
			joint.autoConfigureConnectedAnchor = autoConfigureConnectedAnchor;
			joint.breakForce = breakForce;
			joint.breakTorque = breakTorque;
			joint.enableCollision = enableCollision;
			joint.enablePreprocessing = enablePreprocessing;
			return joint;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Joint joint = (Joint)obj;
				connectedBody = joint.connectedBody.GetMappedInstanceID();
				axis = joint.axis;
				anchor = joint.anchor;
				connectedAnchor = joint.connectedAnchor;
				autoConfigureConnectedAnchor = joint.autoConfigureConnectedAnchor;
				breakForce = joint.breakForce;
				breakTorque = joint.breakTorque;
				enableCollision = joint.enableCollision;
				enablePreprocessing = joint.enablePreprocessing;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(connectedBody, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				Joint joint = (Joint)obj;
				AddDependency(joint.connectedBody, dependencies);
			}
		}
	}
}
