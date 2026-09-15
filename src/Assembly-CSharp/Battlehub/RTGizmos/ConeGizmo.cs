using System;
using UnityEngine;

namespace Battlehub.RTGizmos
{
	public abstract class ConeGizmo : BaseGizmo
	{
		private Vector3[] m_coneHandlesPositions;

		private Vector3[] m_coneHandesNormals;

		protected abstract float Radius { get; set; }

		protected abstract float Height { get; set; }

		private Vector3 Scale => new Vector3(Mathf.Max(Mathf.Abs(Radius), 0.001f), Mathf.Max(Mathf.Abs(Radius), 0.001f), 1f);

		protected override Matrix4x4 HandlesTransform => Matrix4x4.TRS(Target.TransformPoint(Vector3.forward * Height), Target.rotation, Scale);

		protected override Vector3[] HandlesPositions => m_coneHandlesPositions;

		protected override Vector3[] HandlesNormals => m_coneHandesNormals;

		protected override void AwakeOverride()
		{
			base.AwakeOverride();
			m_coneHandlesPositions = RuntimeGizmos.GetConeHandlesPositions();
			m_coneHandesNormals = RuntimeGizmos.GetConeHandlesNormals();
		}

		protected override bool OnDrag(int index, Vector3 offset)
		{
			float num = Math.Sign(Vector3.Dot(offset.normalized, HandlesNormals[index]));
			if (index == 0)
			{
				Height += offset.magnitude * num;
			}
			else
			{
				Radius += offset.magnitude * num;
			}
			return true;
		}

		protected override bool HitOverride(int index, Vector3 vertex, Vector3 normal)
		{
			if (index == 0 && Mathf.Abs(Radius) < 0.0001f)
			{
				return false;
			}
			return true;
		}

		protected override void DrawOverride()
		{
			base.DrawOverride();
			RuntimeGizmos.DrawConeHandles(Target.TransformPoint(Vector3.forward * Height), Target.rotation, Scale, HandlesColor);
			RuntimeGizmos.DrawWireConeGL(Height, Radius, Target.position, Target.rotation, Vector3.one, LineColor);
			if (base.IsDragging)
			{
				RuntimeGizmos.DrawSelection(Target.TransformPoint(Vector3.forward * Height + Vector3.Scale(HandlesPositions[base.DragIndex], Scale)), Target.rotation, Scale, SelectionColor);
			}
		}
	}
}
