using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentCanvasRenderer : PersistentComponent
	{
		public bool hasPopInstruction;

		public int materialCount;

		public int popMaterialCount;

		public bool cull;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			CanvasRenderer canvasRenderer = (CanvasRenderer)obj;
			canvasRenderer.hasPopInstruction = hasPopInstruction;
			canvasRenderer.materialCount = materialCount;
			canvasRenderer.popMaterialCount = popMaterialCount;
			canvasRenderer.cull = cull;
			return canvasRenderer;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				CanvasRenderer canvasRenderer = (CanvasRenderer)obj;
				hasPopInstruction = canvasRenderer.hasPopInstruction;
				materialCount = canvasRenderer.materialCount;
				popMaterialCount = canvasRenderer.popMaterialCount;
				cull = canvasRenderer.cull;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
