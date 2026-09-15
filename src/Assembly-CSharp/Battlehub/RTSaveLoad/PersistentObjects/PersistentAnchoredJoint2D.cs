using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(1093, typeof(PersistentSpringJoint2D))]
	[ProtoInclude(1094, typeof(PersistentDistanceJoint2D))]
	[ProtoInclude(1095, typeof(PersistentFrictionJoint2D))]
	[ProtoInclude(1096, typeof(PersistentHingeJoint2D))]
	[ProtoInclude(1097, typeof(PersistentSliderJoint2D))]
	[ProtoInclude(1098, typeof(PersistentFixedJoint2D))]
	[ProtoInclude(1099, typeof(PersistentWheelJoint2D))]
	public class PersistentAnchoredJoint2D : PersistentJoint2D
	{
		public Vector2 anchor;

		public Vector2 connectedAnchor;

		public bool autoConfigureConnectedAnchor;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			AnchoredJoint2D anchoredJoint2D = (AnchoredJoint2D)obj;
			anchoredJoint2D.anchor = anchor;
			anchoredJoint2D.connectedAnchor = connectedAnchor;
			anchoredJoint2D.autoConfigureConnectedAnchor = autoConfigureConnectedAnchor;
			return anchoredJoint2D;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				AnchoredJoint2D anchoredJoint2D = (AnchoredJoint2D)obj;
				anchor = anchoredJoint2D.anchor;
				connectedAnchor = anchoredJoint2D.connectedAnchor;
				autoConfigureConnectedAnchor = anchoredJoint2D.autoConfigureConnectedAnchor;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
