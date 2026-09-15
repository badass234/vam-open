using Battlehub.RTHandles;
using UnityEngine;

public class FCPositionHandle : PositionHandle
{
	protected FreeControllerV3 _controller;

	public FCRotationHandle priorityHandle;

	private Rigidbody rb;

	protected bool isDragging;

	public FreeControllerV3 controller
	{
		get
		{
			return _controller;
		}
		set
		{
			if (_controller != value)
			{
				_controller = value;
				if (!isDragging && _controller != null)
				{
					base.transform.position = _controller.transform.position;
				}
			}
		}
	}

	public bool HasSelectedAxis => SelectedAxis != RuntimeHandleAxis.None;

	protected override bool OnBeginDrag()
	{
		if (_controller != null && base.OnBeginDrag() && _controller.canGrabPosition && !_controller.isGrabbing)
		{
			isDragging = true;
			_controller.isGrabbing = true;
			_controller.SelectLinkToRigidbody(rb, FreeControllerV3.SelectLinkState.PositionAndRotation);
			return true;
		}
		return false;
	}

	protected override void OnDrop()
	{
		base.OnDrop();
		if (isDragging)
		{
			isDragging = false;
			if (_controller != null)
			{
				_controller.isGrabbing = false;
				_controller.RestorePreLinkState();
			}
		}
	}

	public void ForceDrop()
	{
		OnRuntimeToolChanged();
	}

	public void ForceDeselectAxis()
	{
		SelectedAxis = RuntimeHandleAxis.None;
	}

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	protected override void UpdateOverride()
	{
		base.UpdateOverride();
		if (priorityHandle != null)
		{
			if (isDragging)
			{
				priorityHandle.ForceDeselectAxis();
			}
			else if (priorityHandle.HasSelectedAxis)
			{
				SelectedAxis = RuntimeHandleAxis.None;
			}
		}
		if (!isDragging && _controller != null)
		{
			base.transform.position = _controller.transform.position;
		}
	}

	protected override void DrawOverride()
	{
		if (_controller != null && _controller.canGrabPosition)
		{
			base.DrawOverride();
		}
	}
}
