using System;
using UnityEngine.Rendering;

namespace Battlehub.RTSaveLoad
{
	[Serializable]
	public class RuntimeShaderInfo
	{
		[Serializable]
		public struct RangeLimits
		{
			public float Def;

			public float Min;

			public float Max;

			public RangeLimits(float def, float min, float max)
			{
				Def = def;
				Min = min;
				Max = max;
			}
		}

		public int dummy;

		public string Name;

		public long InstanceId;

		public int PropertyCount;

		public string[] PropertyDescriptions;

		public string[] PropertyNames;

		public RTShaderPropertyType[] PropertyTypes;

		public RangeLimits[] PropertyRangeLimits;

		public TextureDimension[] PropertyTexDims;

		public bool[] IsHidden;
	}
}
