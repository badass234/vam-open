using Leap.Unity.Attributes;
using UnityEngine;

namespace Leap.Unity.Space
{
	public abstract class LeapRadialSpace : LeapSpace
	{
		[MinValue(0.001f)]
		[SerializeField]
		private float _radius = 1f;

		public float radius
		{
			get
			{
				return _radius;
			}
			set
			{
				_radius = value;
			}
		}

		public override Hash GetSettingHash()
		{
			Hash result = default(Hash);
			result.Add(_radius);
			return result;
		}

		protected sealed override void UpdateTransformer(ITransformer transformer, ITransformer parent)
		{
			Vector3 vector = base.transform.InverseTransformPoint(transformer.anchor.transform.position);
			Vector3 vector2 = base.transform.InverseTransformPoint(parent.anchor.transform.position);
			Vector3 rectSpaceDelta = vector - vector2;
			UpdateRadialTransformer(transformer, parent, rectSpaceDelta);
		}

		protected abstract void UpdateRadialTransformer(ITransformer transformer, ITransformer parent, Vector3 rectSpaceDelta);
	}
}
