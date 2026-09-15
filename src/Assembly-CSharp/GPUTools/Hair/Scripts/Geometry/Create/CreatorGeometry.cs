using System;
using System.Collections.Generic;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Geometry.Create
{
	[Serializable]
	public class CreatorGeometry
	{
		public List<GeometryGroupData> List = new List<GeometryGroupData>();

		public int SelectedIndex;

		public GeometryGroupData Selected => (SelectedIndex < 0 || SelectedIndex >= List.Count) ? null : List[SelectedIndex];

		public bool Validate(bool log)
		{
			if (List.Count == 0)
			{
				if (log)
				{
					Debug.LogError("No geometry was created");
				}
				return false;
			}
			return true;
		}
	}
}
