using System;

namespace Leap
{
	public struct LeapTransform
	{
		public static readonly LeapTransform Identity = new LeapTransform(Vector.Zero, LeapQuaternion.Identity, Vector.Ones);

		private Vector _translation;

		private Vector _scale;

		private LeapQuaternion _quaternion;

		private bool _quaternionDirty;

		private bool _flip;

		private Vector _flipAxes;

		private Vector _xBasis;

		private Vector _yBasis;

		private Vector _zBasis;

		private Vector _xBasisScaled;

		private Vector _yBasisScaled;

		private Vector _zBasisScaled;

		public Vector xBasis
		{
			get
			{
				return _xBasis;
			}
			set
			{
				_xBasis = value;
				_xBasisScaled = value * scale.x;
				_quaternionDirty = true;
			}
		}

		public Vector yBasis
		{
			get
			{
				return _yBasis;
			}
			set
			{
				_yBasis = value;
				_yBasisScaled = value * scale.y;
				_quaternionDirty = true;
			}
		}

		public Vector zBasis
		{
			get
			{
				return _zBasis;
			}
			set
			{
				_zBasis = value;
				_zBasisScaled = value * scale.z;
				_quaternionDirty = true;
			}
		}

		public Vector translation
		{
			get
			{
				return _translation;
			}
			set
			{
				_translation = value;
			}
		}

		public Vector scale
		{
			get
			{
				return _scale;
			}
			set
			{
				_scale = value;
				_xBasisScaled = _xBasis * scale.x;
				_yBasisScaled = _yBasis * scale.y;
				_zBasisScaled = _zBasis * scale.z;
			}
		}

		public LeapQuaternion rotation
		{
			get
			{
				if (_quaternionDirty)
				{
					throw new InvalidOperationException("Requesting rotation after Basis vectors have been modified.");
				}
				return _quaternion;
			}
			set
			{
				_quaternion = value;
				float magnitudeSquared = value.MagnitudeSquared;
				float num = 2f / magnitudeSquared;
				float num2 = value.x * num;
				float num3 = value.y * num;
				float num4 = value.z * num;
				float num5 = value.w * num2;
				float num6 = value.w * num3;
				float num7 = value.w * num4;
				float num8 = value.x * num2;
				float num9 = value.x * num3;
				float num10 = value.x * num4;
				float num11 = value.y * num3;
				float num12 = value.y * num4;
				float num13 = value.z * num4;
				_xBasis = new Vector(1f - (num11 + num13), num9 + num7, num10 - num6);
				_yBasis = new Vector(num9 - num7, 1f - (num8 + num13), num12 + num5);
				_zBasis = new Vector(num10 + num6, num12 - num5, 1f - (num8 + num11));
				_xBasisScaled = _xBasis * scale.x;
				_yBasisScaled = _yBasis * scale.y;
				_zBasisScaled = _zBasis * scale.z;
				_quaternionDirty = false;
				_flip = false;
				_flipAxes = new Vector(1f, 1f, 1f);
			}
		}

		public LeapTransform(Vector translation, LeapQuaternion rotation)
			: this(translation, rotation, Vector.Ones)
		{
		}

		public LeapTransform(Vector translation, LeapQuaternion rotation, Vector scale)
		{
			this = default(LeapTransform);
			_scale = scale;
			this.translation = translation;
			this.rotation = rotation;
		}

		public Vector TransformPoint(Vector point)
		{
			return _xBasisScaled * point.x + _yBasisScaled * point.y + _zBasisScaled * point.z + translation;
		}

		public Vector TransformDirection(Vector direction)
		{
			return _xBasis * direction.x + _yBasis * direction.y + _zBasis * direction.z;
		}

		public Vector TransformVelocity(Vector velocity)
		{
			return _xBasisScaled * velocity.x + _yBasisScaled * velocity.y + _zBasisScaled * velocity.z;
		}

		public LeapQuaternion TransformQuaternion(LeapQuaternion rhs)
		{
			if (_quaternionDirty)
			{
				throw new InvalidOperationException("Calling TransformQuaternion after Basis vectors have been modified.");
			}
			if (_flip)
			{
				rhs.x *= _flipAxes.x;
				rhs.y *= _flipAxes.y;
				rhs.z *= _flipAxes.z;
			}
			return _quaternion.Multiply(rhs);
		}

		public void MirrorX()
		{
			_xBasis = -_xBasis;
			_xBasisScaled = -_xBasisScaled;
			_flip = true;
			_flipAxes.y = 0f - _flipAxes.y;
			_flipAxes.z = 0f - _flipAxes.z;
		}

		public void MirrorZ()
		{
			_zBasis = -_zBasis;
			_zBasisScaled = -_zBasisScaled;
			_flip = true;
			_flipAxes.x = 0f - _flipAxes.x;
			_flipAxes.y = 0f - _flipAxes.y;
		}
	}
}
