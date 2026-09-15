using System;

namespace Leap.Unity
{
	[Serializable]
	public struct SingleLayer : IEquatable<SingleLayer>
	{
		public int layerIndex;

		public int layerMask
		{
			get
			{
				return 1 << layerIndex;
			}
			set
			{
				if (value == 0)
				{
					throw new ArgumentException("Single layer can only represent exactly one layer.  The provided mask represents no layers (mask was zero).");
				}
				int num = 0;
				while ((value & 1) == 0)
				{
					value >>= 1;
					num++;
				}
				if (value != 1)
				{
					throw new ArgumentException("Single layer can only represent exactly one layer.  The provided mask represents more than one layer.");
				}
				layerIndex = num;
			}
		}

		public static implicit operator int(SingleLayer singleLayer)
		{
			return singleLayer.layerIndex;
		}

		public static implicit operator SingleLayer(int layerIndex)
		{
			SingleLayer result = default(SingleLayer);
			result.layerIndex = layerIndex;
			return result;
		}

		public bool Equals(SingleLayer other)
		{
			return layerIndex == other.layerIndex;
		}
	}
}
