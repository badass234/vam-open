using System;
using System.Linq.Expressions;
using System.Reflection;
using Battlehub.RTCommon;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTGizmos
{
	public class SkinnedMeshRendererGizmo : BoxGizmo
	{
		[SerializeField]
		private SkinnedMeshRenderer m_skinnedMeshRenderer;

		private static readonly Bounds m_zeroBounds = default(Bounds);

		protected override Bounds Bounds
		{
			get
			{
				if (m_skinnedMeshRenderer == null)
				{
					return m_zeroBounds;
				}
				return m_skinnedMeshRenderer.localBounds;
			}
			set
			{
				if (m_skinnedMeshRenderer != null)
				{
					m_skinnedMeshRenderer.localBounds = value;
				}
			}
		}

		protected override void AwakeOverride()
		{
			if (m_skinnedMeshRenderer == null)
			{
				m_skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
			}
			if (m_skinnedMeshRenderer == null)
			{
				Debug.LogError("Set SkinnedMeshRenderer");
			}
			else
			{
				Target = m_skinnedMeshRenderer.rootBone;
			}
			base.AwakeOverride();
		}

		protected override void RecordOverride()
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(SkinnedMeshRenderer), "x");
			base.RecordOverride();
			RuntimeUndo.RecordValue(m_skinnedMeshRenderer, Strong.PropertyInfo(Expression.Lambda<Func<SkinnedMeshRenderer, Bounds>>(Expression.Property(parameterExpression, typeof(UnityEngine.SkinnedMeshRenderer).GetProperty("localBounds").GetGetMethod()), new ParameterExpression[1] { parameterExpression })));
		}

		private void Reset()
		{
			LineColor = new Color(0.75f, 0.75f, 0.75f, 0.75f);
			HandlesColor = new Color(0.9f, 0.9f, 0.9f, 0.95f);
			SelectionColor = new Color(1f, 1f, 0f, 1f);
		}
	}
}
