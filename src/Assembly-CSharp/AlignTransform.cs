using UnityEngine;

public class AlignTransform : MonoBehaviour
{
	public Transform alignTo;

	public bool delayFrame;

	protected Vector3 lastPosition;

	protected Quaternion lastRotation;

	private void Update()
	{
		if (alignTo != null)
		{
			if (delayFrame)
			{
				base.transform.position = lastPosition;
				base.transform.rotation = lastRotation;
				lastPosition = alignTo.position;
				lastRotation = alignTo.rotation;
			}
			else
			{
				base.transform.position = alignTo.position;
				base.transform.rotation = alignTo.rotation;
			}
		}
	}
}
