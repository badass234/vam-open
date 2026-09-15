using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentLineRenderer : PersistentRenderer
	{
		public float startWidth;

		public float endWidth;

		public PersistentAnimationCurve widthCurve;

		public float widthMultiplier;

		public Color startColor;

		public Color endColor;

		public PersistentGradient colorGradient;

		public int positionCount;

		public bool useWorldSpace;

		public bool loop;

		public int numCornerVertices;

		public int numCapVertices;

		public uint textureMode;

		public uint alignment;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			LineRenderer lineRenderer = (LineRenderer)obj;
			lineRenderer.startWidth = startWidth;
			lineRenderer.endWidth = endWidth;
			lineRenderer.widthCurve = Write(lineRenderer.widthCurve, widthCurve, objects);
			lineRenderer.widthMultiplier = widthMultiplier;
			lineRenderer.startColor = startColor;
			lineRenderer.endColor = endColor;
			lineRenderer.colorGradient = Write(lineRenderer.colorGradient, colorGradient, objects);
			lineRenderer.positionCount = positionCount;
			lineRenderer.useWorldSpace = useWorldSpace;
			lineRenderer.loop = loop;
			lineRenderer.numCornerVertices = numCornerVertices;
			lineRenderer.numCapVertices = numCapVertices;
			lineRenderer.textureMode = (LineTextureMode)textureMode;
			lineRenderer.alignment = (LineAlignment)alignment;
			return lineRenderer;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				LineRenderer lineRenderer = (LineRenderer)obj;
				startWidth = lineRenderer.startWidth;
				endWidth = lineRenderer.endWidth;
				widthCurve = Read(widthCurve, lineRenderer.widthCurve);
				widthMultiplier = lineRenderer.widthMultiplier;
				startColor = lineRenderer.startColor;
				endColor = lineRenderer.endColor;
				colorGradient = Read(colorGradient, lineRenderer.colorGradient);
				positionCount = lineRenderer.positionCount;
				useWorldSpace = lineRenderer.useWorldSpace;
				loop = lineRenderer.loop;
				numCornerVertices = lineRenderer.numCornerVertices;
				numCapVertices = lineRenderer.numCapVertices;
				textureMode = (uint)lineRenderer.textureMode;
				alignment = (uint)lineRenderer.alignment;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			FindDependencies(widthCurve, dependencies, objects, allowNulls);
			FindDependencies(colorGradient, dependencies, objects, allowNulls);
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj != null)
			{
				LineRenderer lineRenderer = (LineRenderer)obj;
				GetDependencies(widthCurve, lineRenderer.widthCurve, dependencies);
				GetDependencies(colorGradient, lineRenderer.colorGradient, dependencies);
			}
		}
	}
}
