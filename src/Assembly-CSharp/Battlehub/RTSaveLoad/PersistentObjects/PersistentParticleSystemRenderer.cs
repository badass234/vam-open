using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentParticleSystemRenderer : PersistentRenderer
	{
		public uint renderMode;

		public float lengthScale;

		public float velocityScale;

		public float cameraVelocityScale;

		public float normalDirection;

		public uint alignment;

		public Vector3 pivot;

		public uint sortMode;

		public float sortingFudge;

		public float minParticleSize;

		public float maxParticleSize;

		public long mesh;

		public long trailMaterial;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystemRenderer particleSystemRenderer = (ParticleSystemRenderer)obj;
			particleSystemRenderer.renderMode = (ParticleSystemRenderMode)renderMode;
			particleSystemRenderer.lengthScale = lengthScale;
			particleSystemRenderer.velocityScale = velocityScale;
			particleSystemRenderer.cameraVelocityScale = cameraVelocityScale;
			particleSystemRenderer.normalDirection = normalDirection;
			particleSystemRenderer.alignment = (ParticleSystemRenderSpace)alignment;
			particleSystemRenderer.pivot = pivot;
			particleSystemRenderer.sortMode = (ParticleSystemSortMode)sortMode;
			particleSystemRenderer.sortingFudge = sortingFudge;
			particleSystemRenderer.minParticleSize = minParticleSize;
			particleSystemRenderer.maxParticleSize = maxParticleSize;
			particleSystemRenderer.mesh = (Mesh)objects.Get(mesh);
			particleSystemRenderer.trailMaterial = (Material)objects.Get(trailMaterial);
			return particleSystemRenderer;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystemRenderer particleSystemRenderer = (ParticleSystemRenderer)obj;
				renderMode = (uint)particleSystemRenderer.renderMode;
				lengthScale = particleSystemRenderer.lengthScale;
				velocityScale = particleSystemRenderer.velocityScale;
				cameraVelocityScale = particleSystemRenderer.cameraVelocityScale;
				normalDirection = particleSystemRenderer.normalDirection;
				alignment = (uint)particleSystemRenderer.alignment;
				pivot = particleSystemRenderer.pivot;
				sortMode = (uint)particleSystemRenderer.sortMode;
				sortingFudge = particleSystemRenderer.sortingFudge;
				minParticleSize = particleSystemRenderer.minParticleSize;
				maxParticleSize = particleSystemRenderer.maxParticleSize;
				mesh = particleSystemRenderer.mesh.GetMappedInstanceID();
				trailMaterial = particleSystemRenderer.trailMaterial.GetMappedInstanceID();
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(mesh, dependencies, objects, allowNulls);
			AddDependency(trailMaterial, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				ParticleSystemRenderer particleSystemRenderer = (ParticleSystemRenderer)obj;
				AddDependency(particleSystemRenderer.mesh, dependencies);
				AddDependency(particleSystemRenderer.trailMaterial, dependencies);
			}
		}
	}
}
