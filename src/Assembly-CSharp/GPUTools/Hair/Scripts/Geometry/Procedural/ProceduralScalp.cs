using UnityEngine;

namespace GPUTools.Hair.Scripts.Geometry.Procedural
{
	public class ProceduralScalp : MonoBehaviour
	{
		[SerializeField]
		public CurveGrid Grid;

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			for (int i = 0; i <= Grid.ViewSizeX; i++)
			{
				for (int j = 0; j <= Grid.ViewSizeY; j++)
				{
					Vector3 splinePoint = Grid.GetSplinePoint((float)i / (float)Grid.ViewSizeX, (float)j / (float)Grid.ViewSizeY);
					Vector3 from = base.transform.TransformPoint(splinePoint);
					if (i < Grid.ViewSizeX)
					{
						Vector3 splinePoint2 = Grid.GetSplinePoint((float)(i + 1) / (float)Grid.ViewSizeX, (float)j / (float)Grid.ViewSizeY);
						Vector3 to = base.transform.TransformPoint(splinePoint2);
						Gizmos.DrawLine(from, to);
					}
					if (j < Grid.ViewSizeY)
					{
						Vector3 splinePoint3 = Grid.GetSplinePoint((float)i / (float)Grid.ViewSizeX, (float)(j + 1) / (float)Grid.ViewSizeY);
						Vector3 to2 = base.transform.TransformPoint(splinePoint3);
						Gizmos.DrawLine(from, to2);
					}
				}
			}
		}
	}
}
