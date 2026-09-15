using UnityEngine;

public class ChildOrderFlip : MonoBehaviour
{
	public Transform[] transformsToFlip;

	[SerializeField]
	protected bool _flipped;

	public bool flipped
	{
		get
		{
			return _flipped;
		}
		set
		{
			if (_flipped != value)
			{
				_flipped = value;
				Flip();
			}
		}
	}

	protected void Flip()
	{
		Transform[] array = transformsToFlip;
		foreach (Transform transform in array)
		{
			int childCount = transform.childCount;
			for (int j = 0; j < childCount; j++)
			{
				Transform child = transform.GetChild(childCount - 1);
				child.SetSiblingIndex(j);
			}
		}
	}
}
