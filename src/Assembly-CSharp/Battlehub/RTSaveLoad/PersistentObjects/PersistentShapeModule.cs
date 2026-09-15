using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentShapeModule : PersistentData
	{
		public bool enabled;

		public uint shapeType;

		public float randomDirectionAmount;

		public float sphericalDirectionAmount;

		public bool alignToDirection;

		public float radius;

		public uint radiusMode;

		public float radiusSpread;

		public PersistentMinMaxCurve radiusSpeed;

		public float radiusSpeedMultiplier;

		public float angle;

		public float length;

		public Vector3 box;

		public uint meshShapeType;

		public long mesh;

		public long meshRenderer;

		public long skinnedMeshRenderer;

		public bool useMeshMaterialIndex;

		public int meshMaterialIndex;

		public bool useMeshColors;

		public float normalOffset;

		public float meshScale;

		public float arc;

		public uint arcMode;

		public float arcSpread;

		public PersistentMinMaxCurve arcSpeed;

		public float arcSpeedMultiplier;

		public Vector3 scale;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.ShapeModule shapeModule = (ParticleSystem.ShapeModule)obj;
			shapeModule.enabled = enabled;
			shapeModule.shapeType = (ParticleSystemShapeType)shapeType;
			shapeModule.randomDirectionAmount = randomDirectionAmount;
			shapeModule.sphericalDirectionAmount = sphericalDirectionAmount;
			shapeModule.alignToDirection = alignToDirection;
			shapeModule.radius = radius;
			shapeModule.radiusMode = (ParticleSystemShapeMultiModeValue)radiusMode;
			shapeModule.radiusSpread = radiusSpread;
			shapeModule.radiusSpeed = Write(shapeModule.radiusSpeed, radiusSpeed, objects);
			shapeModule.radiusSpeedMultiplier = radiusSpeedMultiplier;
			shapeModule.angle = angle;
			shapeModule.length = length;
			shapeModule.scale = box;
			shapeModule.meshShapeType = (ParticleSystemMeshShapeType)meshShapeType;
			shapeModule.mesh = (Mesh)objects.Get(mesh);
			shapeModule.meshRenderer = (MeshRenderer)objects.Get(meshRenderer);
			shapeModule.skinnedMeshRenderer = (SkinnedMeshRenderer)objects.Get(skinnedMeshRenderer);
			shapeModule.useMeshMaterialIndex = useMeshMaterialIndex;
			shapeModule.meshMaterialIndex = meshMaterialIndex;
			shapeModule.useMeshColors = useMeshColors;
			shapeModule.normalOffset = normalOffset;
			shapeModule.scale = scale;
			shapeModule.arc = arc;
			shapeModule.arcMode = (ParticleSystemShapeMultiModeValue)arcMode;
			shapeModule.arcSpread = arcSpread;
			shapeModule.arcSpeed = Write(shapeModule.arcSpeed, arcSpeed, objects);
			shapeModule.arcSpeedMultiplier = arcSpeedMultiplier;
			return shapeModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.ShapeModule shapeModule = (ParticleSystem.ShapeModule)obj;
				enabled = shapeModule.enabled;
				shapeType = (uint)shapeModule.shapeType;
				randomDirectionAmount = shapeModule.randomDirectionAmount;
				sphericalDirectionAmount = shapeModule.sphericalDirectionAmount;
				alignToDirection = shapeModule.alignToDirection;
				radius = shapeModule.radius;
				radiusMode = (uint)shapeModule.radiusMode;
				radiusSpread = shapeModule.radiusSpread;
				radiusSpeed = Read(radiusSpeed, shapeModule.radiusSpeed);
				radiusSpeedMultiplier = shapeModule.radiusSpeedMultiplier;
				angle = shapeModule.angle;
				length = shapeModule.length;
				box = shapeModule.scale;
				meshShapeType = (uint)shapeModule.meshShapeType;
				mesh = shapeModule.mesh.GetMappedInstanceID();
				meshRenderer = shapeModule.meshRenderer.GetMappedInstanceID();
				skinnedMeshRenderer = shapeModule.skinnedMeshRenderer.GetMappedInstanceID();
				useMeshMaterialIndex = shapeModule.useMeshMaterialIndex;
				meshMaterialIndex = shapeModule.meshMaterialIndex;
				useMeshColors = shapeModule.useMeshColors;
				normalOffset = shapeModule.normalOffset;
				scale = shapeModule.scale;
				arc = shapeModule.arc;
				arcMode = (uint)shapeModule.arcMode;
				arcSpread = shapeModule.arcSpread;
				arcSpeed = Read(arcSpeed, shapeModule.arcSpeed);
				arcSpeedMultiplier = shapeModule.arcSpeedMultiplier;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			FindDependencies(radiusSpeed, dependencies, objects, allowNulls);
			AddDependency(mesh, dependencies, objects, allowNulls);
			AddDependency(meshRenderer, dependencies, objects, allowNulls);
			AddDependency(skinnedMeshRenderer, dependencies, objects, allowNulls);
			FindDependencies(arcSpeed, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				ParticleSystem.ShapeModule shapeModule = (ParticleSystem.ShapeModule)obj;
				GetDependencies(radiusSpeed, shapeModule.radiusSpeed, dependencies);
				AddDependency(shapeModule.mesh, dependencies);
				AddDependency(shapeModule.meshRenderer, dependencies);
				AddDependency(shapeModule.skinnedMeshRenderer, dependencies);
				GetDependencies(arcSpeed, shapeModule.arcSpeed, dependencies);
			}
		}
	}
}
