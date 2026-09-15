using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1084, typeof(PersistentAnchoredJoint2D))]
	[ProtoInclude(1085, typeof(PersistentRelativeJoint2D))]
	[ProtoInclude(1086, typeof(PersistentTargetJoint2D))]
	public class PersistentJoint2D : PersistentBehaviour
	{
		public long connectedBody;

		public bool enableCollision;

		public float breakForce;

		public float breakTorque;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Joint2D joint2D = (Joint2D)obj;
			joint2D.connectedBody = (Rigidbody2D)objects.Get(connectedBody);
			joint2D.enableCollision = enableCollision;
			joint2D.breakForce = breakForce;
			joint2D.breakTorque = breakTorque;
			return joint2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Joint2D joint2D = (Joint2D)obj;
				connectedBody = joint2D.connectedBody.GetMappedInstanceID();
				enableCollision = joint2D.enableCollision;
				breakForce = joint2D.breakForce;
				breakTorque = joint2D.breakTorque;
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
				Joint2D joint2D = (Joint2D)obj;
				AddDependency(joint2D.connectedBody, dependencies);
			}
		}
	}
}
