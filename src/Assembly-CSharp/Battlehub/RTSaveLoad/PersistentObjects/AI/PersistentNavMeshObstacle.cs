using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.AI;

namespace Battlehub.RTSaveLoad.PersistentObjects.AI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentNavMeshObstacle : PersistentBehaviour
	{
		public float height;

		public float radius;

		public Vector3 velocity;

		public bool carving;

		public bool carveOnlyStationary;

		public float carvingMoveThreshold;

		public float carvingTimeToStationary;

		public uint shape;

		public Vector3 center;

		public Vector3 size;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			NavMeshObstacle navMeshObstacle = (NavMeshObstacle)obj;
			navMeshObstacle.height = height;
			navMeshObstacle.radius = radius;
			navMeshObstacle.velocity = velocity;
			navMeshObstacle.carving = carving;
			navMeshObstacle.carveOnlyStationary = carveOnlyStationary;
			navMeshObstacle.carvingMoveThreshold = carvingMoveThreshold;
			navMeshObstacle.carvingTimeToStationary = carvingTimeToStationary;
			navMeshObstacle.shape = (NavMeshObstacleShape)shape;
			navMeshObstacle.center = center;
			navMeshObstacle.size = size;
			return navMeshObstacle;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				NavMeshObstacle navMeshObstacle = (NavMeshObstacle)obj;
				height = navMeshObstacle.height;
				radius = navMeshObstacle.radius;
				velocity = navMeshObstacle.velocity;
				carving = navMeshObstacle.carving;
				carveOnlyStationary = navMeshObstacle.carveOnlyStationary;
				carvingMoveThreshold = navMeshObstacle.carvingMoveThreshold;
				carvingTimeToStationary = navMeshObstacle.carvingTimeToStationary;
				shape = (uint)navMeshObstacle.shape;
				center = navMeshObstacle.center;
				size = navMeshObstacle.size;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
