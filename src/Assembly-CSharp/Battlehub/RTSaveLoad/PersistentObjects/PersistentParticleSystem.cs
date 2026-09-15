using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentParticleSystem : PersistentComponent
	{
		public float time;

		public uint randomSeed;

		public bool useAutoRandomSeed;

		public PersistentEmissionModule emission;

		public PersistentCollisionModule collision;

		public PersistentTriggerModule trigger;

		public PersistentShapeModule shape;

		public PersistentVelocityOverLifetimeModule velocityOverLifetime;

		public PersistentLimitVelocityOverLifetimeModule limitVelocityOverLifetime;

		public PersistentInheritVelocityModule inheritVelocity;

		public PersistentForceOverLifetimeModule forceOverLifetime;

		public PersistentColorOverLifetimeModule colorOverLifetime;

		public PersistentColorBySpeedModule colorBySpeed;

		public PersistentSizeOverLifetimeModule sizeOverLifetime;

		public PersistentSizeBySpeedModule sizeBySpeed;

		public PersistentRotationOverLifetimeModule rotationOverLifetime;

		public PersistentRotationBySpeedModule rotationBySpeed;

		public PersistentExternalForcesModule externalForces;

		public PersistentSubEmittersModule subEmitters;

		public PersistentTextureSheetAnimationModule textureSheetAnimation;

		public PersistentLightsModule lights;

		public PersistentMainModule main;

		public PersistentNoiseModule noise;

		public PersistentTrailModule trails;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem particleSystem = (ParticleSystem)obj;
			particleSystem.time = time;
			particleSystem.randomSeed = randomSeed;
			particleSystem.useAutoRandomSeed = useAutoRandomSeed;
			Write(particleSystem.emission, emission, objects);
			Write(particleSystem.collision, collision, objects);
			Write(particleSystem.trigger, trigger, objects);
			Write(particleSystem.shape, shape, objects);
			Write(particleSystem.velocityOverLifetime, velocityOverLifetime, objects);
			Write(particleSystem.limitVelocityOverLifetime, limitVelocityOverLifetime, objects);
			Write(particleSystem.inheritVelocity, inheritVelocity, objects);
			Write(particleSystem.forceOverLifetime, forceOverLifetime, objects);
			Write(particleSystem.colorOverLifetime, colorOverLifetime, objects);
			Write(particleSystem.colorBySpeed, colorBySpeed, objects);
			Write(particleSystem.sizeOverLifetime, sizeOverLifetime, objects);
			Write(particleSystem.sizeBySpeed, sizeBySpeed, objects);
			Write(particleSystem.rotationOverLifetime, rotationOverLifetime, objects);
			Write(particleSystem.rotationBySpeed, rotationBySpeed, objects);
			Write(particleSystem.externalForces, externalForces, objects);
			Write(particleSystem.subEmitters, subEmitters, objects);
			Write(particleSystem.textureSheetAnimation, textureSheetAnimation, objects);
			Write(particleSystem.lights, lights, objects);
			Write(particleSystem.main, main, objects);
			Write(particleSystem.noise, noise, objects);
			Write(particleSystem.trails, trails, objects);
			return particleSystem;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem particleSystem = (ParticleSystem)obj;
				time = particleSystem.time;
				randomSeed = particleSystem.randomSeed;
				useAutoRandomSeed = particleSystem.useAutoRandomSeed;
				emission = Read(emission, particleSystem.emission);
				collision = Read(collision, particleSystem.collision);
				trigger = Read(trigger, particleSystem.trigger);
				shape = Read(shape, particleSystem.shape);
				velocityOverLifetime = Read(velocityOverLifetime, particleSystem.velocityOverLifetime);
				limitVelocityOverLifetime = Read(limitVelocityOverLifetime, particleSystem.limitVelocityOverLifetime);
				inheritVelocity = Read(inheritVelocity, particleSystem.inheritVelocity);
				forceOverLifetime = Read(forceOverLifetime, particleSystem.forceOverLifetime);
				colorOverLifetime = Read(colorOverLifetime, particleSystem.colorOverLifetime);
				colorBySpeed = Read(colorBySpeed, particleSystem.colorBySpeed);
				sizeOverLifetime = Read(sizeOverLifetime, particleSystem.sizeOverLifetime);
				sizeBySpeed = Read(sizeBySpeed, particleSystem.sizeBySpeed);
				rotationOverLifetime = Read(rotationOverLifetime, particleSystem.rotationOverLifetime);
				rotationBySpeed = Read(rotationBySpeed, particleSystem.rotationBySpeed);
				externalForces = Read(externalForces, particleSystem.externalForces);
				subEmitters = Read(subEmitters, particleSystem.subEmitters);
				textureSheetAnimation = Read(textureSheetAnimation, particleSystem.textureSheetAnimation);
				lights = Read(lights, particleSystem.lights);
				main = Read(main, particleSystem.main);
				noise = Read(noise, particleSystem.noise);
				trails = Read(trails, particleSystem.trails);
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			FindDependencies(emission, dependencies, objects, allowNulls);
			FindDependencies(collision, dependencies, objects, allowNulls);
			FindDependencies(trigger, dependencies, objects, allowNulls);
			FindDependencies(shape, dependencies, objects, allowNulls);
			FindDependencies(velocityOverLifetime, dependencies, objects, allowNulls);
			FindDependencies(limitVelocityOverLifetime, dependencies, objects, allowNulls);
			FindDependencies(inheritVelocity, dependencies, objects, allowNulls);
			FindDependencies(forceOverLifetime, dependencies, objects, allowNulls);
			FindDependencies(colorOverLifetime, dependencies, objects, allowNulls);
			FindDependencies(colorBySpeed, dependencies, objects, allowNulls);
			FindDependencies(sizeOverLifetime, dependencies, objects, allowNulls);
			FindDependencies(sizeBySpeed, dependencies, objects, allowNulls);
			FindDependencies(rotationOverLifetime, dependencies, objects, allowNulls);
			FindDependencies(rotationBySpeed, dependencies, objects, allowNulls);
			FindDependencies(externalForces, dependencies, objects, allowNulls);
			FindDependencies(subEmitters, dependencies, objects, allowNulls);
			FindDependencies(textureSheetAnimation, dependencies, objects, allowNulls);
			FindDependencies(lights, dependencies, objects, allowNulls);
			FindDependencies(main, dependencies, objects, allowNulls);
			FindDependencies(noise, dependencies, objects, allowNulls);
			FindDependencies(trails, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				ParticleSystem particleSystem = (ParticleSystem)obj;
				GetDependencies(emission, particleSystem.emission, dependencies);
				GetDependencies(collision, particleSystem.collision, dependencies);
				GetDependencies(trigger, particleSystem.trigger, dependencies);
				GetDependencies(shape, particleSystem.shape, dependencies);
				GetDependencies(velocityOverLifetime, particleSystem.velocityOverLifetime, dependencies);
				GetDependencies(limitVelocityOverLifetime, particleSystem.limitVelocityOverLifetime, dependencies);
				GetDependencies(inheritVelocity, particleSystem.inheritVelocity, dependencies);
				GetDependencies(forceOverLifetime, particleSystem.forceOverLifetime, dependencies);
				GetDependencies(colorOverLifetime, particleSystem.colorOverLifetime, dependencies);
				GetDependencies(colorBySpeed, particleSystem.colorBySpeed, dependencies);
				GetDependencies(sizeOverLifetime, particleSystem.sizeOverLifetime, dependencies);
				GetDependencies(sizeBySpeed, particleSystem.sizeBySpeed, dependencies);
				GetDependencies(rotationOverLifetime, particleSystem.rotationOverLifetime, dependencies);
				GetDependencies(rotationBySpeed, particleSystem.rotationBySpeed, dependencies);
				GetDependencies(externalForces, particleSystem.externalForces, dependencies);
				GetDependencies(subEmitters, particleSystem.subEmitters, dependencies);
				GetDependencies(textureSheetAnimation, particleSystem.textureSheetAnimation, dependencies);
				GetDependencies(lights, particleSystem.lights, dependencies);
				GetDependencies(main, particleSystem.main, dependencies);
				GetDependencies(noise, particleSystem.noise, dependencies);
				GetDependencies(trails, particleSystem.trails, dependencies);
			}
		}
	}
}
