using System.Runtime.InteropServices;
using UnityEngine;

namespace Obi
{
	public class ObiEdgeShapeTracker2D : ObiShapeTracker
	{
		private int pointCount;

		private GCHandle pointsHandle;

		private GCHandle indicesHandle;

		private bool edgeDataHasChanged;

		public ObiEdgeShapeTracker2D(EdgeCollider2D collider)
		{
			base.collider = collider;
			adaptor.is2D = true;
			oniShape = Oni.CreateShape(Oni.ShapeType.EdgeMesh);
			UpdateEdgeData();
		}

		public void UpdateEdgeData()
		{
			EdgeCollider2D edgeCollider2D = collider as EdgeCollider2D;
			if (edgeCollider2D != null)
			{
				Vector3[] array = new Vector3[edgeCollider2D.pointCount];
				int[] array2 = new int[edgeCollider2D.edgeCount * 2];
				Vector2[] points = edgeCollider2D.points;
				for (int i = 0; i < edgeCollider2D.pointCount; i++)
				{
					array[i] = points[i];
				}
				for (int j = 0; j < edgeCollider2D.edgeCount; j++)
				{
					array2[j * 2] = j;
					array2[j * 2 + 1] = j + 1;
				}
				Oni.UnpinMemory(pointsHandle);
				Oni.UnpinMemory(indicesHandle);
				pointsHandle = Oni.PinMemory(array);
				indicesHandle = Oni.PinMemory(array2);
				edgeDataHasChanged = true;
			}
		}

		public override void UpdateIfNeeded()
		{
			EdgeCollider2D edgeCollider2D = collider as EdgeCollider2D;
			if (edgeCollider2D != null && (edgeCollider2D.pointCount != pointCount || edgeDataHasChanged))
			{
				pointCount = edgeCollider2D.pointCount;
				edgeDataHasChanged = false;
				adaptor.Set(pointsHandle.AddrOfPinnedObject(), indicesHandle.AddrOfPinnedObject(), edgeCollider2D.pointCount, edgeCollider2D.edgeCount * 2);
				Oni.UpdateShape(oniShape, ref adaptor);
			}
		}

		public override void Destroy()
		{
			base.Destroy();
			Oni.UnpinMemory(pointsHandle);
			Oni.UnpinMemory(indicesHandle);
		}
	}
}
