using System.Collections;
using UnityEngine;

public class ConfigurableJointReconnector : ScaleChangeReceiver
{
	public bool resetRelativePositionAndRotationOnEnable;

	public bool createJointOnInit;

	public Rigidbody rigidBodyToConnect;

	public int numResetFrames = 10;

	private bool wasInit;

	private ConfigurableJoint joint;

	private Vector3 startingRelativePosition;

	private Quaternion startingRelativeRotation;

	public bool controlRelativePositionAndRotation;

	protected Vector3 _controlledRelativePosition;

	protected Quaternion _controlledRelativeRotation;

	private Vector3 relativePosition
	{
		get
		{
			if (controlRelativePositionAndRotation)
			{
				return controlledRelativePosition;
			}
			return startingRelativePosition;
		}
		set
		{
			if (controlRelativePositionAndRotation)
			{
				controlledRelativePosition = value;
			}
			else
			{
				startingRelativePosition = value;
			}
		}
	}

	private Quaternion relativeRotation
	{
		get
		{
			if (controlRelativePositionAndRotation)
			{
				return controlledRelativeRotation;
			}
			return startingRelativeRotation;
		}
		set
		{
			if (controlRelativePositionAndRotation)
			{
				controlledRelativeRotation = value;
			}
			else
			{
				startingRelativeRotation = value;
			}
		}
	}

	public Vector3 controlledRelativePosition
	{
		get
		{
			return _controlledRelativePosition;
		}
		set
		{
			if (_controlledRelativePosition != value)
			{
				_controlledRelativePosition = value;
				if (base.gameObject.activeInHierarchy && base.enabled)
				{
					Reconnect();
				}
			}
		}
	}

	public Quaternion controlledRelativeRotation
	{
		get
		{
			return _controlledRelativeRotation;
		}
		set
		{
			if (_controlledRelativeRotation != value)
			{
				_controlledRelativeRotation = value;
				if (base.gameObject.activeInHierarchy && base.enabled)
				{
					Reconnect();
				}
			}
		}
	}

	public override void ScaleChanged(float scale)
	{
		base.ScaleChanged(scale);
		if (base.isActiveAndEnabled)
		{
			Reconnect();
		}
	}

	private IEnumerator RepeatingReset()
	{
		for (int i = 0; i < numResetFrames; i++)
		{
			ResetToOrigin();
			yield return null;
		}
	}

	private void ResetToOrigin()
	{
		Rigidbody connectedBody = joint.connectedBody;
		if (connectedBody != null)
		{
			base.transform.position = connectedBody.transform.localToWorldMatrix.MultiplyPoint3x4(relativePosition);
			base.transform.rotation = connectedBody.transform.rotation * relativeRotation;
		}
	}

	public void Reconnect()
	{
		if (!wasInit)
		{
			wasInit = true;
			joint = GetComponent<ConfigurableJoint>();
			if (joint != null)
			{
				if (createJointOnInit && rigidBodyToConnect != null)
				{
					joint.connectedBody = null;
					base.transform.position = rigidBodyToConnect.transform.position;
					base.transform.rotation = rigidBodyToConnect.transform.rotation;
					joint.connectedBody = rigidBodyToConnect;
					startingRelativePosition = Vector3.zero;
					startingRelativeRotation = Quaternion.identity;
				}
				else
				{
					Rigidbody connectedBody = joint.connectedBody;
					if (connectedBody != null)
					{
						startingRelativePosition = connectedBody.transform.worldToLocalMatrix.MultiplyPoint3x4(base.transform.position);
						startingRelativeRotation = Quaternion.Inverse(connectedBody.transform.rotation) * base.transform.rotation;
					}
				}
			}
		}
		if (!(joint != null))
		{
			return;
		}
		Vector3 position = base.transform.position;
		Quaternion rotation = base.transform.rotation;
		Rigidbody connectedBody2 = joint.connectedBody;
		if (connectedBody2 != null)
		{
			joint.connectedBody = null;
			if (connectedBody2 != null)
			{
				base.transform.position = connectedBody2.transform.localToWorldMatrix.MultiplyPoint3x4(relativePosition);
				base.transform.rotation = connectedBody2.transform.rotation * relativeRotation;
			}
			joint.connectedBody = connectedBody2;
			if (resetRelativePositionAndRotationOnEnable)
			{
				StopAllCoroutines();
				StartCoroutine(RepeatingReset());
			}
			else
			{
				base.transform.position = position;
				base.transform.rotation = rotation;
			}
		}
	}

	private void OnEnable()
	{
		Reconnect();
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	private void Awake()
	{
		Reconnect();
	}
}
