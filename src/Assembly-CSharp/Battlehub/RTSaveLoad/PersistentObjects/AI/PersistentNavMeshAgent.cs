using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.AI;

namespace Battlehub.RTSaveLoad.PersistentObjects.AI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentNavMeshAgent : PersistentBehaviour
	{
		public Vector3 destination;

		public float stoppingDistance;

		public Vector3 velocity;

		public Vector3 nextPosition;

		public float baseOffset;

		public bool autoTraverseOffMeshLink;

		public bool autoBraking;

		public bool autoRepath;

		public bool isStopped;

		public NavMeshPath path;

		public int areaMask;

		public float speed;

		public float angularSpeed;

		public float acceleration;

		public bool updatePosition;

		public bool updateRotation;

		public bool updateUpAxis;

		public float radius;

		public float height;

		public uint obstacleAvoidanceType;

		public int avoidancePriority;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			NavMeshAgent navMeshAgent = (NavMeshAgent)obj;
			navMeshAgent.destination = destination;
			navMeshAgent.stoppingDistance = stoppingDistance;
			navMeshAgent.velocity = velocity;
			navMeshAgent.nextPosition = nextPosition;
			navMeshAgent.baseOffset = baseOffset;
			navMeshAgent.autoTraverseOffMeshLink = autoTraverseOffMeshLink;
			navMeshAgent.autoBraking = autoBraking;
			navMeshAgent.autoRepath = autoRepath;
			navMeshAgent.isStopped = isStopped;
			navMeshAgent.path = path;
			navMeshAgent.areaMask = areaMask;
			navMeshAgent.speed = speed;
			navMeshAgent.angularSpeed = angularSpeed;
			navMeshAgent.acceleration = acceleration;
			navMeshAgent.updatePosition = updatePosition;
			navMeshAgent.updateRotation = updateRotation;
			navMeshAgent.updateUpAxis = updateUpAxis;
			navMeshAgent.radius = radius;
			navMeshAgent.height = height;
			navMeshAgent.obstacleAvoidanceType = (ObstacleAvoidanceType)obstacleAvoidanceType;
			navMeshAgent.avoidancePriority = avoidancePriority;
			return navMeshAgent;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				NavMeshAgent navMeshAgent = (NavMeshAgent)obj;
				destination = navMeshAgent.destination;
				stoppingDistance = navMeshAgent.stoppingDistance;
				velocity = navMeshAgent.velocity;
				nextPosition = navMeshAgent.nextPosition;
				baseOffset = navMeshAgent.baseOffset;
				autoTraverseOffMeshLink = navMeshAgent.autoTraverseOffMeshLink;
				autoBraking = navMeshAgent.autoBraking;
				autoRepath = navMeshAgent.autoRepath;
				isStopped = navMeshAgent.isStopped;
				path = navMeshAgent.path;
				areaMask = navMeshAgent.areaMask;
				speed = navMeshAgent.speed;
				angularSpeed = navMeshAgent.angularSpeed;
				acceleration = navMeshAgent.acceleration;
				updatePosition = navMeshAgent.updatePosition;
				updateRotation = navMeshAgent.updateRotation;
				updateUpAxis = navMeshAgent.updateUpAxis;
				radius = navMeshAgent.radius;
				height = navMeshAgent.height;
				obstacleAvoidanceType = (uint)navMeshAgent.obstacleAvoidanceType;
				avoidancePriority = navMeshAgent.avoidancePriority;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
