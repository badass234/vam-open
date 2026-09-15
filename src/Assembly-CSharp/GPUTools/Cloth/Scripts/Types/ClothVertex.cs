using UnityEngine;

namespace GPUTools.Cloth.Scripts.Types
{
	public struct ClothVertex
	{
		public Vector3 Position;

		public Vector3 LastPosition;

		public Vector3 Normal;

		public ClothVertex(Vector3 position, Vector3 normal)
		{
			Position = position;
			LastPosition = position;
			Normal = normal;
		}

		public static int Size()
		{
			return 36;
		}
	}
}
