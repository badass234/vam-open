using UnityEngine;

public class ExtendedCapsuleCollider
{
	public CapsuleCollider collider;

	public Vector3 endPoint1;

	public Vector3 endPoint2;

	public Vector3 endPoint1Local;

	public Vector3 endPoint2Local;

	public Vector3 localCenter;

	public float unscaledRadius;

	public float radius;

	public float radiusSquared;

	public float unscaledLength;

	public float length;

	public float oneOverLength;

	public float unscaledHalfLength;

	public float halfLength;

	public float lengthSquared;

	public float oneOverLengthSquared;

	public void UpdateEndpoints()
	{
		endPoint1 = collider.transform.TransformPoint(endPoint1Local);
		endPoint2 = collider.transform.TransformPoint(endPoint2Local);
	}

	public void RecalculateVars()
	{
		if (collider != null)
		{
			float x = collider.transform.lossyScale.x;
			localCenter = collider.center;
			unscaledRadius = collider.radius;
			radius = unscaledRadius * x;
			radiusSquared = radius * radius;
			unscaledLength = collider.height - 2f * unscaledRadius;
			length = unscaledLength * x;
			oneOverLength = 1f / length;
			halfLength = length * 0.5f;
			unscaledHalfLength = unscaledLength * 0.5f;
			lengthSquared = length * length;
			oneOverLengthSquared = 1f / lengthSquared;
			switch (collider.direction)
			{
			case 0:
				endPoint1Local.x = localCenter.x + unscaledHalfLength;
				endPoint1Local.y = localCenter.y;
				endPoint1Local.z = localCenter.z;
				endPoint2Local.x = localCenter.x - unscaledHalfLength;
				endPoint2Local.y = localCenter.y;
				endPoint2Local.z = localCenter.z;
				break;
			case 1:
				endPoint1Local.x = localCenter.x;
				endPoint1Local.y = localCenter.y + unscaledHalfLength;
				endPoint1Local.z = localCenter.z;
				endPoint2Local.x = localCenter.x;
				endPoint2Local.y = localCenter.y - unscaledHalfLength;
				endPoint2Local.z = localCenter.z;
				break;
			case 2:
				endPoint1Local.x = localCenter.x;
				endPoint1Local.y = localCenter.y;
				endPoint1Local.z = localCenter.z + unscaledHalfLength;
				endPoint2Local.x = localCenter.x;
				endPoint2Local.y = localCenter.y;
				endPoint2Local.z = localCenter.z - unscaledHalfLength;
				break;
			}
		}
	}
}
