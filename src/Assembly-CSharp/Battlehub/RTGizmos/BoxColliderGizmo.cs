using System;
using System.Linq.Expressions;
using System.Reflection;
using Battlehub.RTCommon;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTGizmos
{
	public class BoxColliderGizmo : BoxGizmo
	{
		[SerializeField]
		private BoxCollider m_collider;

		private static readonly Bounds m_zeroBounds = default(Bounds);

		protected override Bounds Bounds
		{
			get
			{
				if (m_collider == null)
				{
					return m_zeroBounds;
				}
				return new Bounds(m_collider.center, m_collider.size);
			}
			set
			{
				if (m_collider != null)
				{
					m_collider.center = value.center;
					m_collider.size = value.extents * 2f;
				}
			}
		}

		protected override void AwakeOverride()
		{
			if (m_collider == null)
			{
				m_collider = GetComponent<BoxCollider>();
			}
			if (m_collider == null)
			{
				Debug.LogError("Set Collider");
			}
			base.AwakeOverride();
		}

		protected override void RecordOverride()
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(BoxCollider), "x");
			ParameterExpression parameterExpression2 = Expression.Parameter(typeof(BoxCollider), "x");
			base.RecordOverride();
			RuntimeUndo.RecordValue(m_collider, Strong.PropertyInfo(Expression.Lambda<Func<BoxCollider, Vector3>>(Expression.Property(parameterExpression, typeof(UnityEngine.BoxCollider).GetProperty("center").GetGetMethod()), new ParameterExpression[1] { parameterExpression })));
			RuntimeUndo.RecordValue(m_collider, Strong.PropertyInfo(Expression.Lambda<Func<BoxCollider, Vector3>>(Expression.Property(parameterExpression2, typeof(UnityEngine.BoxCollider).GetProperty("size").GetGetMethod()), new ParameterExpression[1] { parameterExpression2 })));
		}
	}
}
