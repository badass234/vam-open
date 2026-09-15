using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentSkinnedMeshRenderer : PersistentRenderer
	{
		public long[] bones;

		public long rootBone;

		public uint quality;

		public long sharedMesh;

		public bool updateWhenOffscreen;

		public bool skinnedMotionVectors;

		public Bounds localBounds;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)obj;
			skinnedMeshRenderer.bones = Resolve<Transform, UnityEngine.Object>(bones, objects);
			skinnedMeshRenderer.rootBone = (Transform)objects.Get(rootBone);
			skinnedMeshRenderer.quality = (SkinQuality)quality;
			skinnedMeshRenderer.sharedMesh = (Mesh)objects.Get(sharedMesh);
			skinnedMeshRenderer.skinnedMotionVectors = skinnedMotionVectors;
			skinnedMeshRenderer.localBounds = localBounds;
			return skinnedMeshRenderer;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)obj;
				bones = skinnedMeshRenderer.bones.GetMappedInstanceID();
				rootBone = skinnedMeshRenderer.rootBone.GetMappedInstanceID();
				quality = (uint)skinnedMeshRenderer.quality;
				sharedMesh = skinnedMeshRenderer.sharedMesh.GetMappedInstanceID();
				updateWhenOffscreen = skinnedMeshRenderer.updateWhenOffscreen;
				skinnedMotionVectors = skinnedMeshRenderer.skinnedMotionVectors;
				localBounds = skinnedMeshRenderer.localBounds;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependencies(bones, dependencies, objects, allowNulls);
			AddDependency(rootBone, dependencies, objects, allowNulls);
			AddDependency(sharedMesh, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)obj;
				AddDependencies(skinnedMeshRenderer.bones, dependencies);
				AddDependency(skinnedMeshRenderer.rootBone, dependencies);
				AddDependency(skinnedMeshRenderer.sharedMesh, dependencies);
			}
		}
	}
}
