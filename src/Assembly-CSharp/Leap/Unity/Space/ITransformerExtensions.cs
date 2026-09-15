using UnityEngine;

namespace Leap.Unity.Space
{
	public static class ITransformerExtensions
	{
		public static void WorldSpaceUnwarp(this ITransformer transformer, Vector3 worldWarpedPosition, Quaternion worldWarpedRotation, out Vector3 worldRectilinearPosition, out Quaternion worldRectilinearRotation)
		{
			Transform transform = transformer.anchor.space.transform;
			Vector3 vector = transform.InverseTransformPoint(worldWarpedPosition);
			Quaternion localWarpedRot = transform.InverseTransformRotation(worldWarpedRotation);
			Vector3 position = transformer.InverseTransformPoint(vector);
			worldRectilinearPosition = transform.TransformPoint(position);
			Quaternion rotation = transformer.InverseTransformRotation(vector, localWarpedRot);
			worldRectilinearRotation = transform.TransformRotation(rotation);
		}
	}
}
