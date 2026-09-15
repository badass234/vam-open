using System.Collections.Generic;
using MVR;
using UnityEngine;

[ExecuteInEditMode]
public class SetAnchorFromVertex : PhysicsSimulatorJSONStorable
{
	[HideInInspector]
	public Transform skinTransform;

	protected DAZSkinV2 _skin;

	[HideInInspector]
	public int subMeshSelection;

	public int targetVertex = -1;

	public bool doUpdate = true;

	public ConfigurableJoint joint;

	public Rigidbody jointRB;

	protected bool initialLocalRotationWasInit;

	protected Quaternion initialLocalRotation;

	public bool setX = true;

	public bool setY = true;

	public bool setZ = true;

	public bool showHandles = true;

	public bool showBackfaceHandles;

	public float handleSize = 0.0002f;

	protected Dictionary<int, int> _uvVertToBaseVertDict;

	public Vector3 target;

	protected Vector3 currentAnchor;

	protected Vector3 newAnchor;

	protected DAZBone dazBone;

	protected Matrix4x4 connectedBodyWorldToLocalMatrix;

	protected bool detectedPhysicsCorruptionOnThread;

	protected string physicsCorruptionType = string.Empty;

	public DAZSkinV2 skin
	{
		get
		{
			return _skin;
		}
		set
		{
			if (_skin != value)
			{
				_skin = value;
				InitSkin();
			}
		}
	}

	public bool isEnabled { get; protected set; }

	protected Dictionary<int, int> uvVertToBaseVertDict
	{
		get
		{
			if (_uvVertToBaseVertDict == null)
			{
				if (skin != null && skin.dazMesh != null)
				{
					_uvVertToBaseVertDict = skin.dazMesh.uvVertToBaseVert;
				}
				else
				{
					_uvVertToBaseVertDict = new Dictionary<int, int>();
				}
			}
			return _uvVertToBaseVertDict;
		}
	}

	public void ClickVertex(int vid)
	{
		if (targetVertex == vid)
		{
			targetVertex = -1;
		}
		else
		{
			targetVertex = vid;
		}
	}

	public int GetBaseVertex(int vid)
	{
		int value;
		if (skin != null && skin.dazMesh != null && uvVertToBaseVertDict.TryGetValue(vid, out value))
		{
			vid = value;
		}
		return vid;
	}

	public bool IsBaseVertex(int vid)
	{
		if (skin != null && skin.dazMesh != null)
		{
			return !uvVertToBaseVertDict.ContainsKey(vid);
		}
		return true;
	}

	protected override void SyncCollisionEnabled()
	{
		base.SyncCollisionEnabled();
		if (joint != null)
		{
			Rigidbody component = joint.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.detectCollisions = _collisionEnabled && !_resetSimulation;
			}
		}
	}

	protected override void SyncUseInterpolation()
	{
		base.SyncUseInterpolation();
		if (!(joint != null))
		{
			return;
		}
		Rigidbody component = joint.GetComponent<Rigidbody>();
		if (component != null)
		{
			if (_useInterpolation)
			{
				component.interpolation = RigidbodyInterpolation.Interpolate;
			}
			else
			{
				component.interpolation = RigidbodyInterpolation.None;
			}
		}
	}

	public void PrepThreadUpdate(bool isForThread = true)
	{
		if (dazBone == null && isForThread)
		{
			connectedBodyWorldToLocalMatrix = joint.connectedBody.transform.worldToLocalMatrix;
		}
		currentAnchor = joint.connectedAnchor;
	}

	public void DoThreadUpdate(Vector3[] vertsToUse, bool isRunningOnThread = true)
	{
		Vector3 v = vertsToUse[targetVertex];
		if (NaNUtils.IsVector3Valid(v))
		{
			target = v;
		}
		else
		{
			detectedPhysicsCorruptionOnThread = true;
			physicsCorruptionType = "Vertex";
		}
		if (isRunningOnThread)
		{
			if (dazBone != null)
			{
				if (NaNUtils.IsMatrixValid(dazBone.worldToLocalMatrix))
				{
					newAnchor = dazBone.worldToLocalMatrix.MultiplyPoint3x4(target);
				}
				else
				{
					detectedPhysicsCorruptionOnThread = true;
					physicsCorruptionType = "Matrix";
				}
			}
			else if (NaNUtils.IsMatrixValid(connectedBodyWorldToLocalMatrix))
			{
				newAnchor = connectedBodyWorldToLocalMatrix.MultiplyPoint3x4(target);
			}
			else
			{
				detectedPhysicsCorruptionOnThread = true;
				physicsCorruptionType = "Matrix";
			}
		}
		else
		{
			newAnchor = joint.connectedBody.transform.InverseTransformPoint(target);
		}
		if (!setX)
		{
			newAnchor.x = currentAnchor.x;
		}
		if (!setY)
		{
			newAnchor.y = currentAnchor.y;
		}
		if (!setZ)
		{
			newAnchor.z = currentAnchor.z;
		}
	}

	public void FinishThreadUpdate()
	{
		if (detectedPhysicsCorruptionOnThread)
		{
			if (containingAtom != null)
			{
				containingAtom.AlertPhysicsCorruption("SetAnchorFromVertex " + physicsCorruptionType + " " + base.name);
			}
			detectedPhysicsCorruptionOnThread = false;
		}
		SyncAnchorToTarget();
	}

	protected void SyncAnchorToTarget()
	{
		joint.connectedAnchor = newAnchor;
		if (Application.isPlaying)
		{
			if (_resetSimulation)
			{
				base.transform.localPosition = newAnchor;
				base.transform.localRotation = initialLocalRotation;
				if (jointRB != null)
				{
					jointRB.velocity = Vector3.zero;
					jointRB.angularVelocity = Vector3.zero;
				}
			}
		}
		else
		{
			base.transform.localPosition = newAnchor;
		}
	}

	protected void InitSkin()
	{
		if (_skin != null && Application.isPlaying)
		{
			if (!_skin.postSkinVerts[targetVertex])
			{
				_skin.postSkinVerts[targetVertex] = true;
				skin.postSkinVertsChanged = true;
			}
			if (!initialLocalRotationWasInit)
			{
				initialLocalRotation = base.transform.localRotation;
				initialLocalRotationWasInit = true;
			}
			if (dazBone == null)
			{
				dazBone = joint.connectedBody.transform.GetComponent<DAZBone>();
			}
		}
	}

	protected override void Update()
	{
		if (!doUpdate)
		{
			return;
		}
		base.Update();
		if (joint != null && _skin != null && targetVertex != -1)
		{
			bool flag = true;
			Vector3[] array;
			if (Application.isPlaying)
			{
				array = skin.rawSkinnedVerts;
				flag = skin.postSkinVertsReady[targetVertex];
			}
			else
			{
				array = skin.dazMesh.morphedUVVertices;
			}
			if (array != null && targetVertex < array.Length && flag)
			{
				PrepThreadUpdate(false);
				DoThreadUpdate(array, false);
				FinishThreadUpdate();
			}
		}
	}

	protected void OnEnable()
	{
		isEnabled = true;
	}

	protected void OnDisable()
	{
		isEnabled = false;
	}
}
