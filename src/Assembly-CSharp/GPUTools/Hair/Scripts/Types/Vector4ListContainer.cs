using System;
using System.Collections.Generic;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Types
{
	[Serializable]
	public class Vector4ListContainer
	{
		[SerializeField]
		public List<Vector4> List = new List<Vector4>();
	}
}
