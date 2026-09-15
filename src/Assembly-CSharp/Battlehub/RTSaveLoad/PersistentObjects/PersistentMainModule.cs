using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentMainModule : PersistentData
	{
		public float duration;

		public bool loop;

		public bool prewarm;

		public PersistentMinMaxCurve startDelay;

		public float startDelayMultiplier;

		public PersistentMinMaxCurve startLifetime;

		public float startLifetimeMultiplier;

		public PersistentMinMaxCurve startSpeed;

		public float startSpeedMultiplier;

		public bool startSize3D;

		public PersistentMinMaxCurve startSize;

		public float startSizeMultiplier;

		public PersistentMinMaxCurve startSizeX;

		public float startSizeXMultiplier;

		public PersistentMinMaxCurve startSizeY;

		public float startSizeYMultiplier;

		public PersistentMinMaxCurve startSizeZ;

		public float startSizeZMultiplier;

		public bool startRotation3D;

		public PersistentMinMaxCurve startRotation;

		public float startRotationMultiplier;

		public PersistentMinMaxCurve startRotationX;

		public float startRotationXMultiplier;

		public PersistentMinMaxCurve startRotationY;

		public float startRotationYMultiplier;

		public PersistentMinMaxCurve startRotationZ;

		public float startRotationZMultiplier;

		public float randomizeRotationDirection;

		public PersistentMinMaxGradient startColor;

		public PersistentMinMaxCurve gravityModifier;

		public float gravityModifierMultiplier;

		public uint simulationSpace;

		public long customSimulationSpace;

		public float simulationSpeed;

		public uint scalingMode;

		public bool playOnAwake;

		public int maxParticles;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.MainModule mainModule = (ParticleSystem.MainModule)obj;
			mainModule.duration = duration;
			mainModule.loop = loop;
			mainModule.prewarm = prewarm;
			mainModule.startDelay = Write(mainModule.startDelay, startDelay, objects);
			mainModule.startDelayMultiplier = startDelayMultiplier;
			mainModule.startLifetime = Write(mainModule.startLifetime, startLifetime, objects);
			mainModule.startLifetimeMultiplier = startLifetimeMultiplier;
			mainModule.startSpeed = Write(mainModule.startSpeed, startSpeed, objects);
			mainModule.startSpeedMultiplier = startSpeedMultiplier;
			mainModule.startSize3D = startSize3D;
			mainModule.startSize = Write(mainModule.startSize, startSize, objects);
			mainModule.startSizeMultiplier = startSizeMultiplier;
			mainModule.startSizeX = Write(mainModule.startSizeX, startSizeX, objects);
			mainModule.startSizeXMultiplier = startSizeXMultiplier;
			mainModule.startSizeY = Write(mainModule.startSizeY, startSizeY, objects);
			mainModule.startSizeYMultiplier = startSizeYMultiplier;
			mainModule.startSizeZ = Write(mainModule.startSizeZ, startSizeZ, objects);
			mainModule.startSizeZMultiplier = startSizeZMultiplier;
			mainModule.startRotation3D = startRotation3D;
			mainModule.startRotation = Write(mainModule.startRotation, startRotation, objects);
			mainModule.startRotationMultiplier = startRotationMultiplier;
			mainModule.startRotationX = Write(mainModule.startRotationX, startRotationX, objects);
			mainModule.startRotationXMultiplier = startRotationXMultiplier;
			mainModule.startRotationY = Write(mainModule.startRotationY, startRotationY, objects);
			mainModule.startRotationYMultiplier = startRotationYMultiplier;
			mainModule.startRotationZ = Write(mainModule.startRotationZ, startRotationZ, objects);
			mainModule.startRotationZMultiplier = startRotationZMultiplier;
			mainModule.startColor = Write(mainModule.startColor, startColor, objects);
			mainModule.gravityModifier = Write(mainModule.gravityModifier, gravityModifier, objects);
			mainModule.gravityModifierMultiplier = gravityModifierMultiplier;
			mainModule.simulationSpace = (ParticleSystemSimulationSpace)simulationSpace;
			mainModule.customSimulationSpace = (Transform)objects.Get(customSimulationSpace);
			mainModule.simulationSpeed = simulationSpeed;
			mainModule.scalingMode = (ParticleSystemScalingMode)scalingMode;
			mainModule.playOnAwake = playOnAwake;
			mainModule.maxParticles = maxParticles;
			return mainModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.MainModule mainModule = (ParticleSystem.MainModule)obj;
				duration = mainModule.duration;
				loop = mainModule.loop;
				prewarm = mainModule.prewarm;
				startDelay = Read(startDelay, mainModule.startDelay);
				startDelayMultiplier = mainModule.startDelayMultiplier;
				startLifetime = Read(startLifetime, mainModule.startLifetime);
				startLifetimeMultiplier = mainModule.startLifetimeMultiplier;
				startSpeed = Read(startSpeed, mainModule.startSpeed);
				startSpeedMultiplier = mainModule.startSpeedMultiplier;
				startSize3D = mainModule.startSize3D;
				startSize = Read(startSize, mainModule.startSize);
				startSizeMultiplier = mainModule.startSizeMultiplier;
				startSizeX = Read(startSizeX, mainModule.startSizeX);
				startSizeXMultiplier = mainModule.startSizeXMultiplier;
				startSizeY = Read(startSizeY, mainModule.startSizeY);
				startSizeYMultiplier = mainModule.startSizeYMultiplier;
				startSizeZ = Read(startSizeZ, mainModule.startSizeZ);
				startSizeZMultiplier = mainModule.startSizeZMultiplier;
				startRotation3D = mainModule.startRotation3D;
				startRotation = Read(startRotation, mainModule.startRotation);
				startRotationMultiplier = mainModule.startRotationMultiplier;
				startRotationX = Read(startRotationX, mainModule.startRotationX);
				startRotationXMultiplier = mainModule.startRotationXMultiplier;
				startRotationY = Read(startRotationY, mainModule.startRotationY);
				startRotationYMultiplier = mainModule.startRotationYMultiplier;
				startRotationZ = Read(startRotationZ, mainModule.startRotationZ);
				startRotationZMultiplier = mainModule.startRotationZMultiplier;
				startColor = Read(startColor, mainModule.startColor);
				gravityModifier = Read(gravityModifier, mainModule.gravityModifier);
				gravityModifierMultiplier = mainModule.gravityModifierMultiplier;
				simulationSpace = (uint)mainModule.simulationSpace;
				customSimulationSpace = mainModule.customSimulationSpace.GetMappedInstanceID();
				simulationSpeed = mainModule.simulationSpeed;
				scalingMode = (uint)mainModule.scalingMode;
				playOnAwake = mainModule.playOnAwake;
				maxParticles = mainModule.maxParticles;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			FindDependencies(startDelay, dependencies, objects, allowNulls);
			FindDependencies(startLifetime, dependencies, objects, allowNulls);
			FindDependencies(startSpeed, dependencies, objects, allowNulls);
			FindDependencies(startSize, dependencies, objects, allowNulls);
			FindDependencies(startSizeX, dependencies, objects, allowNulls);
			FindDependencies(startSizeY, dependencies, objects, allowNulls);
			FindDependencies(startSizeZ, dependencies, objects, allowNulls);
			FindDependencies(startRotation, dependencies, objects, allowNulls);
			FindDependencies(startRotationX, dependencies, objects, allowNulls);
			FindDependencies(startRotationY, dependencies, objects, allowNulls);
			FindDependencies(startRotationZ, dependencies, objects, allowNulls);
			FindDependencies(startColor, dependencies, objects, allowNulls);
			FindDependencies(gravityModifier, dependencies, objects, allowNulls);
			AddDependency(customSimulationSpace, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				ParticleSystem.MainModule mainModule = (ParticleSystem.MainModule)obj;
				GetDependencies(startDelay, mainModule.startDelay, dependencies);
				GetDependencies(startLifetime, mainModule.startLifetime, dependencies);
				GetDependencies(startSpeed, mainModule.startSpeed, dependencies);
				GetDependencies(startSize, mainModule.startSize, dependencies);
				GetDependencies(startSizeX, mainModule.startSizeX, dependencies);
				GetDependencies(startSizeY, mainModule.startSizeY, dependencies);
				GetDependencies(startSizeZ, mainModule.startSizeZ, dependencies);
				GetDependencies(startRotation, mainModule.startRotation, dependencies);
				GetDependencies(startRotationX, mainModule.startRotationX, dependencies);
				GetDependencies(startRotationY, mainModule.startRotationY, dependencies);
				GetDependencies(startRotationZ, mainModule.startRotationZ, dependencies);
				GetDependencies(startColor, mainModule.startColor, dependencies);
				GetDependencies(gravityModifier, mainModule.gravityModifier, dependencies);
				AddDependency(mainModule.customSimulationSpace, dependencies);
			}
		}
	}
}
