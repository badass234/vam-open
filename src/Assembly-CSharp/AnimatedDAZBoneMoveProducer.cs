using UnityEngine;

public class AnimatedDAZBoneMoveProducer : MoveProducer
{
	public bool useOrientationOffset = true;

	protected DAZBone dazBone;

	protected bool _wasInit;

	public Transform orientationTransform { get; protected set; }

	protected override void Init()
	{
		if (_wasInit)
		{
			return;
		}
		_wasInit = true;
		base.Init();
		if (_receiver != null && _receiver.followWhenOff != null)
		{
			dazBone = _receiver.followWhenOff.GetComponent<DAZBone>();
			if (dazBone != null)
			{
				GameObject gameObject = new GameObject(base.name + "_orientation");
				orientationTransform = gameObject.transform;
				orientationTransform.SetParent(base.transform);
				orientationTransform.localPosition = Vector3.zero;
				dazBone.Init();
				Quaternion rotation = base.transform.rotation;
				base.transform.rotation = Quaternion.identity;
				orientationTransform.rotation = dazBone.startingRotationRelativeToRoot;
				base.transform.rotation = rotation;
			}
		}
	}

	protected override void SetCurrentPositionAndRotation()
	{
		if (useOrientationOffset && orientationTransform != null)
		{
			_currentPosition = orientationTransform.position;
			_currentRotation = orientationTransform.rotation;
		}
		else
		{
			_currentPosition = base.transform.position;
			_currentRotation = base.transform.rotation;
		}
	}
}
