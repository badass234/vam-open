using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentCanvas : PersistentBehaviour
	{
		public uint renderMode;

		public long worldCamera;

		public float scaleFactor;

		public float referencePixelsPerUnit;

		public bool overridePixelPerfect;

		public bool pixelPerfect;

		public float planeDistance;

		public bool overrideSorting;

		public int sortingOrder;

		public int targetDisplay;

		public float normalizedSortingGridSize;

		public int sortingLayerID;

		public uint additionalShaderChannels;

		public string sortingLayerName;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Canvas canvas = (Canvas)obj;
			canvas.renderMode = (RenderMode)renderMode;
			canvas.worldCamera = (Camera)objects.Get(worldCamera);
			canvas.scaleFactor = scaleFactor;
			canvas.referencePixelsPerUnit = referencePixelsPerUnit;
			canvas.overridePixelPerfect = overridePixelPerfect;
			canvas.pixelPerfect = pixelPerfect;
			canvas.planeDistance = planeDistance;
			canvas.overrideSorting = overrideSorting;
			canvas.sortingOrder = sortingOrder;
			canvas.targetDisplay = targetDisplay;
			canvas.normalizedSortingGridSize = normalizedSortingGridSize;
			canvas.sortingLayerID = sortingLayerID;
			canvas.additionalShaderChannels = (AdditionalCanvasShaderChannels)additionalShaderChannels;
			canvas.sortingLayerName = sortingLayerName;
			return canvas;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				Canvas canvas = (Canvas)obj;
				renderMode = (uint)canvas.renderMode;
				worldCamera = canvas.worldCamera.GetMappedInstanceID();
				scaleFactor = canvas.scaleFactor;
				referencePixelsPerUnit = canvas.referencePixelsPerUnit;
				overridePixelPerfect = canvas.overridePixelPerfect;
				pixelPerfect = canvas.pixelPerfect;
				planeDistance = canvas.planeDistance;
				overrideSorting = canvas.overrideSorting;
				sortingOrder = canvas.sortingOrder;
				targetDisplay = canvas.targetDisplay;
				normalizedSortingGridSize = canvas.normalizedSortingGridSize;
				sortingLayerID = canvas.sortingLayerID;
				additionalShaderChannels = (uint)canvas.additionalShaderChannels;
				sortingLayerName = canvas.sortingLayerName;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(worldCamera, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				Canvas canvas = (Canvas)obj;
				AddDependency(canvas.worldCamera, dependencies);
			}
		}
	}
}
