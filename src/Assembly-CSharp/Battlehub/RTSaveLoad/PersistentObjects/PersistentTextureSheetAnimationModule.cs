using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Rendering;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentTextureSheetAnimationModule : PersistentData
	{
		public bool enabled;

		public int numTilesX;

		public int numTilesY;

		public uint animation;

		public bool useRandomRow;

		public PersistentMinMaxCurve frameOverTime;

		public float frameOverTimeMultiplier;

		public PersistentMinMaxCurve startFrame;

		public float startFrameMultiplier;

		public int cycleCount;

		public int rowIndex;

		public uint uvChannelMask;

		public float flipU;

		public float flipV;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule = (ParticleSystem.TextureSheetAnimationModule)obj;
			textureSheetAnimationModule.enabled = enabled;
			textureSheetAnimationModule.numTilesX = numTilesX;
			textureSheetAnimationModule.numTilesY = numTilesY;
			textureSheetAnimationModule.animation = (ParticleSystemAnimationType)animation;
			textureSheetAnimationModule.useRandomRow = useRandomRow;
			textureSheetAnimationModule.frameOverTime = Write(textureSheetAnimationModule.frameOverTime, frameOverTime, objects);
			textureSheetAnimationModule.frameOverTimeMultiplier = frameOverTimeMultiplier;
			textureSheetAnimationModule.startFrame = Write(textureSheetAnimationModule.startFrame, startFrame, objects);
			textureSheetAnimationModule.startFrameMultiplier = startFrameMultiplier;
			textureSheetAnimationModule.cycleCount = cycleCount;
			textureSheetAnimationModule.rowIndex = rowIndex;
			textureSheetAnimationModule.uvChannelMask = (UVChannelFlags)uvChannelMask;
			textureSheetAnimationModule.flipU = flipU;
			textureSheetAnimationModule.flipV = flipV;
			return textureSheetAnimationModule;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule = (ParticleSystem.TextureSheetAnimationModule)obj;
				enabled = textureSheetAnimationModule.enabled;
				numTilesX = textureSheetAnimationModule.numTilesX;
				numTilesY = textureSheetAnimationModule.numTilesY;
				animation = (uint)textureSheetAnimationModule.animation;
				useRandomRow = textureSheetAnimationModule.useRandomRow;
				frameOverTime = Read(frameOverTime, textureSheetAnimationModule.frameOverTime);
				frameOverTimeMultiplier = textureSheetAnimationModule.frameOverTimeMultiplier;
				startFrame = Read(startFrame, textureSheetAnimationModule.startFrame);
				startFrameMultiplier = textureSheetAnimationModule.startFrameMultiplier;
				cycleCount = textureSheetAnimationModule.cycleCount;
				rowIndex = textureSheetAnimationModule.rowIndex;
				uvChannelMask = (uint)textureSheetAnimationModule.uvChannelMask;
				flipU = textureSheetAnimationModule.flipU;
				flipV = textureSheetAnimationModule.flipV;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			FindDependencies(frameOverTime, dependencies, objects, allowNulls);
			FindDependencies(startFrame, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				ParticleSystem.TextureSheetAnimationModule textureSheetAnimationModule = (ParticleSystem.TextureSheetAnimationModule)obj;
				GetDependencies(frameOverTime, textureSheetAnimationModule.frameOverTime, dependencies);
				GetDependencies(startFrame, textureSheetAnimationModule.startFrame, dependencies);
			}
		}
	}
}
