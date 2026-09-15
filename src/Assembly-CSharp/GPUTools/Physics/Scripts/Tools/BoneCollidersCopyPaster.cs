using GPUTools.Physics.Scripts.Behaviours;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Tools
{
	public class BoneCollidersCopyPaster : MonoBehaviour
	{
		[SerializeField]
		private Transform from;

		[SerializeField]
		private Transform to;

		[ContextMenu("CopyPaste")]
		public void CopyPaste()
		{
			CopyPasteRecursive(from, to);
		}

		private void CopyPasteRecursive(Transform from, Transform to)
		{
			CopyPasteForBone(from, to);
			for (int i = 0; i < from.childCount; i++)
			{
				Transform child = from.GetChild(i);
				Transform child2 = to.GetChild(i);
				CopyPasteRecursive(child, child2);
			}
		}

		private void CopyPasteForBone(Transform from, Transform to)
		{
			LineSphereCollider[] components = from.GetComponents<LineSphereCollider>();
			foreach (LineSphereCollider lineSphereCollider in components)
			{
				LineSphereCollider lineSphereCollider2 = to.gameObject.AddComponent<LineSphereCollider>();
				lineSphereCollider2.WorldA = lineSphereCollider.WorldA;
				lineSphereCollider2.WorldB = lineSphereCollider.WorldB;
				lineSphereCollider2.WorldRadiusA = lineSphereCollider.WorldRadiusA;
				lineSphereCollider2.WorldRadiusB = lineSphereCollider.WorldRadiusB;
			}
			SphereCollider[] components2 = from.GetComponents<SphereCollider>();
			foreach (SphereCollider sphereCollider in components2)
			{
				SphereCollider sphereCollider2 = to.gameObject.AddComponent<SphereCollider>();
				sphereCollider2.center = sphereCollider.center;
				sphereCollider2.radius = sphereCollider.radius;
			}
		}
	}
}
