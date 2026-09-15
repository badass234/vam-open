using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentParticle : PersistentData
	{
		public Vector3 position;

		public Vector3 velocity;

		public float remainingLifetime;

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
			ParticleSystem.Particle particle = (ParticleSystem.Particle)obj;
			particle.position = position;
			particle.velocity = velocity;
			particle.remainingLifetime = remainingLifetime;
			particle.startLifetime = startLifetime;
			particle.startSize = startSize;
			particle.startSize3D = startSize3D;
			particle.axisOfRotation = axisOfRotation;
			particle.rotation = rotation;
			particle.rotation3D = rotation3D;
			particle.angularVelocity = angularVelocity;
			particle.angularVelocity3D = angularVelocity3D;
			particle.startColor = startColor;
			particle.randomSeed = randomSeed;
			return particle;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.Particle particle = (ParticleSystem.Particle)obj;
				position = particle.position;
				velocity = particle.velocity;
				remainingLifetime = particle.remainingLifetime;
				startLifetime = particle.startLifetime;
				startSize = particle.startSize;
				startSize3D = particle.startSize3D;
				axisOfRotation = particle.axisOfRotation;
				rotation = particle.rotation;
				rotation3D = particle.rotation3D;
				angularVelocity = particle.angularVelocity;
				angularVelocity3D = particle.angularVelocity3D;
				startColor = particle.startColor;
				randomSeed = particle.randomSeed;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
