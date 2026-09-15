using System;
using UnityEngine;

namespace GPUTools.Painter.Scripts
{
	[Serializable]
	public class ColorBrush
	{
		[SerializeField]
		public Color Color = new Color(0.95f, 0f, 0f);

		[SerializeField]
		public float Radius = 0.02f;

		[SerializeField]
		public float Strength = 1f;

		[SerializeField]
		public ColorChannel Channel;

		public Color CurrentDrawColor
		{
			get
			{
				if (Channel == ColorChannel.R)
				{
					return Color.red;
				}
				if (Channel == ColorChannel.G)
				{
					return Color.green;
				}
				if (Channel == ColorChannel.B)
				{
					return Color.blue;
				}
				return Color.white;
			}
		}

		public float CurrentChannelValue
		{
			get
			{
				if (Channel == ColorChannel.R)
				{
					return Color.r;
				}
				if (Channel == ColorChannel.G)
				{
					return Color.g;
				}
				if (Channel == ColorChannel.B)
				{
					return Color.b;
				}
				return Color.a;
			}
			set
			{
				if (Channel == ColorChannel.R)
				{
					Color.r = value;
				}
				if (Channel == ColorChannel.G)
				{
					Color.g = value;
				}
				if (Channel == ColorChannel.B)
				{
					Color.b = value;
				}
				if (Channel == ColorChannel.A)
				{
					Color.a = value;
				}
			}
		}
	}
}
