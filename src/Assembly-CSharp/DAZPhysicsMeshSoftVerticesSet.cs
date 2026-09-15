using System;
using System.Collections.Generic;
using GPUTools.Physics.Scripts.Behaviours;
using UnityEngine;

[Serializable]
public class DAZPhysicsMeshSoftVerticesSet
{
	[SerializeField]
	protected string _uid;

	public int targetVertex = -1;

	public int anchorVertex = -1;

	public bool autoInfluenceAnchor;

	public int[] influenceVertices;

	public int highlightVertex;

	[SerializeField]
	protected List<string> _links;

	public float springMultiplier = 1f;

	public float sizeMultiplier = 1f;

	public float limitMultiplier = 1f;

	public bool forceLookAtReference;

	[NonSerialized]
	public Vector3 lastPosition;

	[NonSerialized]
	public Vector3 lastPositionThreaded;

	[NonSerialized]
	public Vector3 currentPosition;

	[NonSerialized]
	public Vector3 lastKinematicPosition;

	[NonSerialized]
	public Vector3 lastKinematicPositionThreaded;

	[NonSerialized]
	public Vector3 currentKinematicPosition;

	[NonSerialized]
	public float interpFactor;

	[NonSerialized]
	public Transform kinematicTransform;

	[NonSerialized]
	public Rigidbody kinematicRB;

	[NonSerialized]
	public Transform jointTransform;

	[NonSerialized]
	public Transform jointTrackerTransform;

	[NonSerialized]
	public Rigidbody jointRB;

	[NonSerialized]
	public ConfigurableJoint joint;

	[NonSerialized]
	public Collider jointCollider;

	[NonSerialized]
	public Collider jointCollider2;

	[NonSerialized]
	public float[] influenceVerticesDistances;

	[NonSerialized]
	public float[] influenceVerticesWeights;

	[NonSerialized]
	public SpringJoint[] linkJoints;

	[NonSerialized]
	public float[] linkJointDistances;

	[NonSerialized]
	public Vector3 initialTargetPosition;

	[NonSerialized]
	public bool linkTargetPositionDirty;

	[NonSerialized]
	public Vector3 jointTargetPosition;

	[NonSerialized]
	public Vector3 jointTargetVelocity;

	[NonSerialized]
	public Vector3 lastJointTargetPosition;

	[NonSerialized]
	public Quaternion jointTargetLookAt;

	[NonSerialized]
	public Vector3 primaryMove;

	[NonSerialized]
	public Vector3 move;

	[NonSerialized]
	public float threadedColliderRadius;

	[NonSerialized]
	public float threadedColliderHeight;

	[NonSerialized]
	public Vector3 threadedColliderCenter;

	[NonSerialized]
	public float threadedCollider2Radius;

	[NonSerialized]
	public float threadedCollider2Height;

	[NonSerialized]
	public Vector3 threadedCollider2Center;

	[NonSerialized]
	public CapsuleLineSphereCollider capsuleLineSphereCollider;

	[NonSerialized]
	public CapsuleLineSphereCollider capsuleLineSphereCollider2;

	public string uid
	{
		get
		{
			if (_uid == null || _uid == string.Empty)
			{
				_uid = Guid.NewGuid().ToString();
			}
			return _uid;
		}
	}

	public List<string> links
	{
		get
		{
			if (_links == null)
			{
				_links = new List<string>();
			}
			return _links;
		}
	}

	public DAZPhysicsMeshSoftVerticesSet()
	{
		_uid = Guid.NewGuid().ToString();
		influenceVertices = new int[0];
		_links = new List<string>();
	}

	public void AddInfluenceVertex(int vid)
	{
		int[] array = new int[influenceVertices.Length + 1];
		bool flag = false;
		for (int i = 0; i < influenceVertices.Length; i++)
		{
			if (influenceVertices[i] == vid)
			{
				flag = true;
				break;
			}
			array[i] = influenceVertices[i];
		}
		if (!flag)
		{
			array[influenceVertices.Length] = vid;
			influenceVertices = array;
		}
	}

	public void RemoveInfluenceVertex(int vid)
	{
		int[] array = new int[influenceVertices.Length - 1];
		bool flag = false;
		int num = 0;
		for (int i = 0; i < influenceVertices.Length; i++)
		{
			if (influenceVertices[i] == vid)
			{
				flag = true;
				continue;
			}
			array[num] = influenceVertices[i];
			num++;
		}
		if (flag)
		{
			influenceVertices = array;
		}
	}
}
