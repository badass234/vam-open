using UnityEngine;

namespace MeshVR
{
	public class CubicBezierPointCompact
	{
		public CubicBezierCurveCompact parent;

		protected Vector3 _position;

		public Quaternion rotation;

		public Vector3 controlPointIn;

		public Vector3 controlPointOut;

		public Vector3 position
		{
			get
			{
				return _position;
			}
			set
			{
				position = value;
				dirty = true;
			}
		}

		public Vector3 worldPosition
		{
			get
			{
				return parent.transform.TransformPoint(position);
			}
			set
			{
				position = parent.transform.InverseTransformPoint(value);
			}
		}

		public Quaternion worldRotation
		{
			get
			{
				return parent.transform.rotation * rotation;
			}
			set
			{
				rotation = Quaternion.Inverse(parent.transform.rotation) * value;
			}
		}

		public bool dirty { get; set; }
	}
}
