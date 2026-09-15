using System;
using System.Collections.Generic;
using Battlehub.RTSaveLoad.PersistentObjects.EventSystems;
using ProtoBuf;
using UnityEngine;
using UnityEngine.UI;

namespace Battlehub.RTSaveLoad.PersistentObjects.UI
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentCanvasScaler : PersistentUIBehaviour
	{
		public uint uiScaleMode;

		public float referencePixelsPerUnit;

		public float scaleFactor;

		public Vector2 referenceResolution;

		public uint screenMatchMode;

		public float matchWidthOrHeight;

		public uint physicalUnit;

		public float fallbackScreenDPI;

		public float defaultSpriteDPI;

		public float dynamicPixelsPerUnit;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			CanvasScaler canvasScaler = (CanvasScaler)obj;
			canvasScaler.uiScaleMode = (CanvasScaler.ScaleMode)uiScaleMode;
			canvasScaler.referencePixelsPerUnit = referencePixelsPerUnit;
			canvasScaler.scaleFactor = scaleFactor;
			canvasScaler.referenceResolution = referenceResolution;
			canvasScaler.screenMatchMode = (CanvasScaler.ScreenMatchMode)screenMatchMode;
			canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
			canvasScaler.physicalUnit = (CanvasScaler.Unit)physicalUnit;
			canvasScaler.fallbackScreenDPI = fallbackScreenDPI;
			canvasScaler.defaultSpriteDPI = defaultSpriteDPI;
			canvasScaler.dynamicPixelsPerUnit = dynamicPixelsPerUnit;
			return canvasScaler;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				CanvasScaler canvasScaler = (CanvasScaler)obj;
				uiScaleMode = (uint)canvasScaler.uiScaleMode;
				referencePixelsPerUnit = canvasScaler.referencePixelsPerUnit;
				scaleFactor = canvasScaler.scaleFactor;
				referenceResolution = canvasScaler.referenceResolution;
				screenMatchMode = (uint)canvasScaler.screenMatchMode;
				matchWidthOrHeight = canvasScaler.matchWidthOrHeight;
				physicalUnit = (uint)canvasScaler.physicalUnit;
				fallbackScreenDPI = canvasScaler.fallbackScreenDPI;
				defaultSpriteDPI = canvasScaler.defaultSpriteDPI;
				dynamicPixelsPerUnit = canvasScaler.dynamicPixelsPerUnit;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
