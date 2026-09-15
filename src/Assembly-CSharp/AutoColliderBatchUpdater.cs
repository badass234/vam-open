using System.Collections.Generic;
using MVR;
using UnityEngine;

public class AutoColliderBatchUpdater : JSONStorable
{
	protected AutoCollider[] _autoColliders;

	protected AutoCollider[] _autoCollidersWithJointsAndBones;

	protected AutoCollider[] _autoCollidersWithJoints;

	protected AutoCollider[] _autoCollidersWithoutJoints;

	public bool clumpUpdate;

	public DAZSkinV2 skin;

	public int numControlledColliders;

	protected bool _on = true;

	private bool _resetSimulation;

	protected List<AsyncFlag> waitResumeSimulationFlags;

	protected const float squaredThreshold = 4E-06f;

	protected bool morphsChanged;

	public bool isEnabled;

	protected AsyncFlag enableResetFlag;

	protected int pauseCountdown;

	public AutoCollider[] autoColliders => _autoColliders;

	public bool on
	{
		get
		{
			return _on;
		}
		set
		{
			if (_on != value)
			{
				_on = value;
			}
		}
	}

	public bool resetSimulation
	{
		get
		{
			return _resetSimulation;
		}
		set
		{
			if (_resetSimulation != value)
			{
				_resetSimulation = value;
				if (_autoColliders == null)
				{
					UpdateAutoColliders();
				}
				AutoCollider[] array = _autoColliders;
				foreach (AutoCollider autoCollider in array)
				{
					autoCollider.resetSimulation = value;
				}
			}
		}
	}

	public void UpdateAutoColliders()
	{
		AutoCollider[] componentsInChildren = GetComponentsInChildren<AutoCollider>();
		List<AutoCollider> list = new List<AutoCollider>();
		List<AutoCollider> list2 = new List<AutoCollider>();
		List<AutoCollider> list3 = new List<AutoCollider>();
		List<AutoCollider> list4 = new List<AutoCollider>();
		AutoCollider[] array = componentsInChildren;
		foreach (AutoCollider autoCollider in array)
		{
			if (!autoCollider.allowBatchUpdate)
			{
				continue;
			}
			autoCollider.enabled = false;
			autoCollider.resetSimulation = _resetSimulation;
			list.Add(autoCollider);
			if (autoCollider.joint != null)
			{
				if (autoCollider.bone != null)
				{
					list3.Add(autoCollider);
				}
				else
				{
					list2.Add(autoCollider);
				}
			}
			else
			{
				list4.Add(autoCollider);
			}
		}
		_autoColliders = list.ToArray();
		_autoCollidersWithJointsAndBones = list3.ToArray();
		_autoCollidersWithJoints = list2.ToArray();
		_autoCollidersWithoutJoints = list4.ToArray();
		numControlledColliders = _autoColliders.Length;
	}

	protected void CheckResumeSimulation()
	{
		if (waitResumeSimulationFlags == null)
		{
			waitResumeSimulationFlags = new List<AsyncFlag>();
		}
		bool flag = false;
		if (waitResumeSimulationFlags.Count > 0)
		{
			List<AsyncFlag> list = new List<AsyncFlag>();
			foreach (AsyncFlag waitResumeSimulationFlag in waitResumeSimulationFlags)
			{
				if (waitResumeSimulationFlag.Raised)
				{
					list.Add(waitResumeSimulationFlag);
					flag = true;
				}
			}
			foreach (AsyncFlag item in list)
			{
				waitResumeSimulationFlags.Remove(item);
			}
		}
		if (waitResumeSimulationFlags.Count > 0)
		{
			resetSimulation = true;
		}
		else if (flag)
		{
			resetSimulation = false;
		}
	}

	public void ResetSimulation(AsyncFlag waitFor)
	{
		if (waitResumeSimulationFlags == null)
		{
			waitResumeSimulationFlags = new List<AsyncFlag>();
		}
		waitResumeSimulationFlags.Add(waitFor);
		resetSimulation = true;
	}

	public void UpdateSizeThreadedFast(Vector3[] verts, Vector3[] norms)
	{
		if (_autoColliders != null && _autoColliders.Length > 0)
		{
			AutoCollider[] array = _autoColliders;
			foreach (AutoCollider autoCollider in array)
			{
				autoCollider.AutoColliderSizeSetFast(verts);
				autoCollider.UpdateHardTransformPositionFast(verts, norms);
			}
		}
	}

	public void UpdateAnchorsThreadedFast(Vector3[] verts, Vector3[] norms)
	{
		if (_autoColliders == null || _autoColliders.Length <= 0)
		{
			return;
		}
		AutoCollider[] array = _autoColliders;
		foreach (AutoCollider autoCollider in array)
		{
			if (autoCollider.centerJoint)
			{
				if (autoCollider.lookAtOption == AutoCollider.LookAtOption.Opposite && autoCollider.oppositeVertex != -1)
				{
					autoCollider.anchorTarget = (verts[autoCollider.targetVertex] + verts[autoCollider.oppositeVertex]) * 0.5f;
				}
				else if (autoCollider.lookAtOption == AutoCollider.LookAtOption.AnchorCenters && autoCollider.anchorVertex1 != -1 && autoCollider.anchorVertex2 != -1)
				{
					autoCollider.anchorTarget = (verts[autoCollider.anchorVertex1] + verts[autoCollider.anchorVertex2]) * 0.5f;
				}
				else if (autoCollider.lookAtOption == AutoCollider.LookAtOption.VertexNormal)
				{
					if (autoCollider.colliderOrient == AutoCollider.ColliderOrient.Look)
					{
						float num = autoCollider.colliderLength * 0.5f * autoCollider.scale;
						if (num < autoCollider.colliderRadius * autoCollider.scale)
						{
							num = autoCollider.colliderRadius * autoCollider.scale;
						}
						autoCollider.anchorTarget = verts[autoCollider.targetVertex] + norms[autoCollider.targetVertex] * (0f - num);
					}
					else
					{
						autoCollider.anchorTarget = verts[autoCollider.targetVertex] + norms[autoCollider.targetVertex] * (0f - autoCollider.colliderRadius) * autoCollider.scale;
					}
				}
			}
			else
			{
				autoCollider.anchorTarget = verts[autoCollider.targetVertex];
			}
			if (autoCollider.bone != null)
			{
				Vector3 vector = autoCollider.bone.worldToLocalMatrix.MultiplyPoint3x4(autoCollider.anchorTarget);
				float sqrMagnitude = (vector - autoCollider.transformedAnchorTarget).sqrMagnitude;
				if (sqrMagnitude >= 4E-06f)
				{
					autoCollider.transformedAnchorTarget = autoCollider.bone.worldToLocalMatrix.MultiplyPoint3x4(autoCollider.anchorTarget);
					autoCollider.transformedAnchorTargetDirty = true;
				}
				else
				{
					autoCollider.transformedAnchorTargetDirty = false;
				}
			}
		}
	}

	public void CheckPhysicsCorruption()
	{
		if (_autoCollidersWithJointsAndBones != null && _autoCollidersWithJointsAndBones.Length > 0)
		{
			AutoCollider autoCollider = _autoCollidersWithJointsAndBones[0];
			Vector3 position = autoCollider.jointTransform.position;
			if (!NaNUtils.IsVector3Valid(position) && containingAtom != null)
			{
				containingAtom.AlertPhysicsCorruption("AutoCollider invalid joint position");
			}
		}
	}

	public void UpdateThreadedFinish(Vector3[] verts, Vector3[] norms)
	{
		if (_resetSimulation)
		{
			AutoCollider[] array = _autoColliders;
			foreach (AutoCollider autoCollider in array)
			{
				if (autoCollider.joint != null)
				{
					if (autoCollider.bone != null)
					{
						autoCollider.joint.connectedAnchor = autoCollider.transformedAnchorTarget;
					}
					else
					{
						autoCollider.joint.connectedAnchor = autoCollider.backForceRigidbody.transform.InverseTransformPoint(autoCollider.anchorTarget);
					}
					autoCollider.ResetJointPhysics();
				}
				if (autoCollider.colliderDirty)
				{
					autoCollider.AutoColliderSizeSetFinishFast();
				}
			}
			return;
		}
		AutoCollider[] autoCollidersWithJointsAndBones = _autoCollidersWithJointsAndBones;
		foreach (AutoCollider autoCollider2 in autoCollidersWithJointsAndBones)
		{
			if (autoCollider2.transformedAnchorTargetDirty)
			{
				autoCollider2.joint.connectedAnchor = autoCollider2.transformedAnchorTarget;
			}
			if (autoCollider2.colliderDirty)
			{
				autoCollider2.AutoColliderSizeSetFinishFast();
			}
		}
		AutoCollider[] autoCollidersWithJoints = _autoCollidersWithJoints;
		foreach (AutoCollider autoCollider3 in autoCollidersWithJoints)
		{
			autoCollider3.joint.connectedAnchor = autoCollider3.backForceRigidbody.transform.InverseTransformPoint(autoCollider3.anchorTarget);
			if (autoCollider3.colliderDirty)
			{
				autoCollider3.AutoColliderSizeSetFinishFast();
			}
		}
		AutoCollider[] autoCollidersWithoutJoints = _autoCollidersWithoutJoints;
		foreach (AutoCollider autoCollider4 in autoCollidersWithoutJoints)
		{
			if (autoCollider4.colliderDirty)
			{
				autoCollider4.AutoColliderSizeSetFinishFast();
			}
		}
	}

	protected void UpdateAnchors()
	{
		if (_autoColliders == null || _autoColliders.Length <= 0 || !(skin != null))
		{
			return;
		}
		Vector3[] rawSkinnedVerts = skin.rawSkinnedVerts;
		Vector3[] postSkinNormals = skin.postSkinNormals;
		AutoCollider[] array = _autoColliders;
		foreach (AutoCollider autoCollider in array)
		{
			if (autoCollider.joint != null && !_resetSimulation)
			{
				if (autoCollider.centerJoint)
				{
					if (autoCollider.lookAtOption == AutoCollider.LookAtOption.Opposite && autoCollider.oppositeVertex != -1)
					{
						autoCollider.anchorTarget = (rawSkinnedVerts[autoCollider.targetVertex] + rawSkinnedVerts[autoCollider.oppositeVertex]) * 0.5f;
						autoCollider.joint.connectedAnchor = autoCollider.backForceRigidbody.transform.InverseTransformPoint(autoCollider.anchorTarget);
					}
					else if (autoCollider.lookAtOption == AutoCollider.LookAtOption.AnchorCenters && autoCollider.anchorVertex1 != -1 && autoCollider.anchorVertex2 != -1)
					{
						autoCollider.anchorTarget = (rawSkinnedVerts[autoCollider.anchorVertex1] + rawSkinnedVerts[autoCollider.anchorVertex2]) * 0.5f;
						autoCollider.joint.connectedAnchor = autoCollider.backForceRigidbody.transform.InverseTransformPoint(autoCollider.anchorTarget);
					}
					else if (autoCollider.lookAtOption == AutoCollider.LookAtOption.VertexNormal)
					{
						if (autoCollider.colliderOrient == AutoCollider.ColliderOrient.Look)
						{
							float num = autoCollider.colliderLength * 0.5f;
							if (num < autoCollider.colliderRadius)
							{
								num = autoCollider.colliderRadius;
							}
							autoCollider.anchorTarget = rawSkinnedVerts[autoCollider.targetVertex] + postSkinNormals[autoCollider.targetVertex] * (0f - num);
							autoCollider.joint.connectedAnchor = autoCollider.backForceRigidbody.transform.InverseTransformPoint(autoCollider.anchorTarget);
						}
						else
						{
							autoCollider.anchorTarget = rawSkinnedVerts[autoCollider.targetVertex] + postSkinNormals[autoCollider.targetVertex] * (0f - autoCollider.colliderRadius);
							autoCollider.joint.connectedAnchor = autoCollider.backForceRigidbody.transform.InverseTransformPoint(autoCollider.anchorTarget);
						}
					}
				}
				else
				{
					autoCollider.anchorTarget = rawSkinnedVerts[autoCollider.targetVertex];
					autoCollider.joint.connectedAnchor = autoCollider.backForceRigidbody.transform.InverseTransformPoint(autoCollider.anchorTarget);
				}
			}
			if (autoCollider.debug)
			{
				MyDebug.DrawWireCube(autoCollider.anchorTarget, 0.005f, Color.blue);
			}
		}
	}

	protected void ResetJointPhysics()
	{
		UpdateAnchors();
		if (_autoColliders != null && _autoColliders.Length > 0)
		{
			AutoCollider[] array = _autoColliders;
			foreach (AutoCollider autoCollider in array)
			{
				autoCollider.ResetJointPhysics();
			}
		}
	}

	private void OnEnable()
	{
		UpdateAutoColliders();
		AutoCollider[] array = _autoColliders;
		foreach (AutoCollider autoCollider in array)
		{
			autoCollider.enabled = false;
			autoCollider.Init();
		}
		isEnabled = true;
		enableResetFlag = new AsyncFlag("EnableResetFlag");
		ResetSimulation(enableResetFlag);
		pauseCountdown = 10;
	}

	private void OnDisable()
	{
		isEnabled = false;
		AutoCollider[] array = _autoColliders;
		foreach (AutoCollider autoCollider in array)
		{
			autoCollider.enabled = true;
		}
		if (enableResetFlag != null)
		{
			enableResetFlag.Raise();
			pauseCountdown = 0;
		}
	}

	private void Update()
	{
		if (Application.isPlaying)
		{
			if (enableResetFlag != null && !enableResetFlag.Raised)
			{
				pauseCountdown--;
				if (pauseCountdown <= 0)
				{
					enableResetFlag.Raise();
				}
			}
			CheckResumeSimulation();
		}
		if (skin != null && (skin.dazMesh.visibleNonPoseVerticesChangedLastFrame || skin.dazMesh.visibleNonPoseVerticesChangedThisFrame))
		{
			morphsChanged = true;
		}
	}

	private void FixedUpdate()
	{
		if (!_on || !clumpUpdate)
		{
			return;
		}
		if (resetSimulation)
		{
			ResetJointPhysics();
			return;
		}
		UpdateAnchors();
		if (morphsChanged)
		{
			morphsChanged = false;
			AutoCollider[] array = _autoColliders;
			foreach (AutoCollider autoCollider in array)
			{
				autoCollider.AutoColliderSizeSet();
			}
		}
	}
}
