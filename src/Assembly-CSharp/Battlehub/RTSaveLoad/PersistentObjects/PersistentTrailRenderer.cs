using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentTrailRenderer : PersistentRenderer
	{
		public float time;

		public float startWidth;

		public float endWidth;

		public PersistentAnimationCurve widthCurve;

		public float widthMultiplier;

		public Color startColor;

		public Color endColor;

		public PersistentGradient colorGradient;

		public bool autodestruct;

		public int numCornerVertices;

		public int numCapVertices;

		public float minVertexDistance;

		public uint textureMode;

		public uint alignment;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			TrailRenderer trailRenderer = (TrailRenderer)obj;
			trailRenderer.time = time;
			trailRenderer.startWidth = startWidth;
			trailRenderer.endWidth = endWidth;
			trailRenderer.widthCurve = Write(trailRenderer.widthCurve, widthCurve, objects);
			trailRenderer.widthMultiplier = widthMultiplier;
			trailRenderer.startColor = startColor;
			trailRenderer.endColor = endColor;
			trailRenderer.colorGradient = Write(trailRenderer.colorGradient, colorGradient, objects);
			trailRenderer.autodestruct = autodestruct;
			trailRenderer.numCornerVertices = numCornerVertices;
			trailRenderer.numCapVertices = numCapVertices;
			trailRenderer.minVertexDistance = minVertexDistance;
			trailRenderer.textureMode = (LineTextureMode)textureMode;
			trailRenderer.alignment = (LineAlignment)alignment;
			return trailRenderer;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				TrailRenderer trailRenderer = (TrailRenderer)obj;
				time = trailRenderer.time;
				startWidth = trailRenderer.startWidth;
				endWidth = trailRenderer.endWidth;
				widthCurve = Read(widthCurve, trailRenderer.widthCurve);
				widthMultiplier = trailRenderer.widthMultiplier;
				startColor = trailRenderer.startColor;
				endColor = trailRenderer.endColor;
				colorGradient = Read(colorGradient, trailRenderer.colorGradient);
				autodestruct = trailRenderer.autodestruct;
				numCornerVertices = trailRenderer.numCornerVertices;
				numCapVertices = trailRenderer.numCapVertices;
				minVertexDistance = trailRenderer.minVertexDistance;
				textureMode = (uint)trailRenderer.textureMode;
				alignment = (uint)trailRenderer.alignment;
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
				TrailRenderer trailRenderer = (TrailRenderer)obj;
				GetDependencies(widthCurve, trailRenderer.widthCurve, dependencies);
				GetDependencies(colorGradient, trailRenderer.colorGradient, dependencies);
			}
		}
	}
}
