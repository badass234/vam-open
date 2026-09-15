using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentCharacterController : PersistentCollider
	{
		public float radius;

		public float height;

		public Vector3 center;

		public float slopeLimit;

		public float stepOffset;

		public float skinWidth;

		public float minMoveDistance;

		public bool detectCollisions;

		public bool enableOverlapRecovery;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			CharacterController characterController = (CharacterController)obj;
			characterController.radius = radius;
			characterController.height = height;
			characterController.center = center;
			characterController.slopeLimit = slopeLimit;
			characterController.stepOffset = stepOffset;
			characterController.skinWidth = skinWidth;
			characterController.minMoveDistance = minMoveDistance;
			characterController.detectCollisions = detectCollisions;
			characterController.enableOverlapRecovery = enableOverlapRecovery;
			return characterController;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				CharacterController characterController = (CharacterController)obj;
				radius = characterController.radius;
				height = characterController.height;
				center = characterController.center;
				slopeLimit = characterController.slopeLimit;
				stepOffset = characterController.stepOffset;
				skinWidth = characterController.skinWidth;
				minMoveDistance = characterController.minMoveDistance;
				detectCollisions = characterController.detectCollisions;
				enableOverlapRecovery = characterController.enableOverlapRecovery;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
