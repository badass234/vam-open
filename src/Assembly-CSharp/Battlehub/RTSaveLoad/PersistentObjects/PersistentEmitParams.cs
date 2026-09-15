using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentEmitParams : PersistentData
	{
		public Vector3 position;

		public bool applyShapeToPosition;

		public Vector3 velocity;

		public float startLifetime;

		public float startSize;

		public Vector3 startSize3D;

		public Vector3 axisOfRotation;

		public float rotation;

		public Vector3 rotation3D;

		public float angularVelocity;

		public Vector3 angularVelocity3D;

		public Color32 startColor;

		public uint randomSeed;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.EmitParams emitParams = (ParticleSystem.EmitParams)obj;
			emitParams.position = position;
			emitParams.applyShapeToPosition = applyShapeToPosition;
			emitParams.velocity = velocity;
			emitParams.startLifetime = startLifetime;
			emitParams.startSize = startSize;
			emitParams.startSize3D = startSize3D;
			emitParams.axisOfRotation = axisOfRotation;
			emitParams.rotation = rotation;
			emitParams.rotation3D = rotation3D;
			emitParams.angularVelocity = angularVelocity;
			emitParams.angularVelocity3D = angularVelocity3D;
			emitParams.startColor = startColor;
			emitParams.randomSeed = randomSeed;
			return emitParams;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.EmitParams emitParams = (ParticleSystem.EmitParams)obj;
				position = emitParams.position;
				applyShapeToPosition = emitParams.applyShapeToPosition;
				velocity = emitParams.velocity;
				startLifetime = emitParams.startLifetime;
				startSize = emitParams.startSize;
				startSize3D = emitParams.startSize3D;
				axisOfRotation = emitParams.axisOfRotation;
				rotation = emitParams.rotation;
				rotation3D = emitParams.rotation3D;
				angularVelocity = emitParams.angularVelocity;
				angularVelocity3D = emitParams.angularVelocity3D;
				startColor = emitParams.startColor;
				randomSeed = emitParams.randomSeed;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
