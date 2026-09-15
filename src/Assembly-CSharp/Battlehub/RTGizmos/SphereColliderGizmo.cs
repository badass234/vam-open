using System;
using System.Linq.Expressions;
using System.Reflection;
using Battlehub.RTCommon;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTGizmos
{
	public class SphereColliderGizmo : SphereGizmo
	{
		[SerializeField]
		private SphereCollider m_collider;

		protected override Vector3 Center
		{
			get
			{
				if (m_collider == null)
				{
					return Vector3.zero;
				}
				return m_collider.center;
			}
			set
			{
				if (m_collider != null)
				{
					m_collider.center = value;
				}
			}
		}

		protected override float Radius
		{
			get
			{
				if (m_collider == null)
				{
					return 0f;
				}
				return m_collider.radius;
			}
			set
			{
				if (m_collider != null)
				{
					m_collider.radius = value;
				}
			}
		}

		protected override void AwakeOverride()
		{
			if (m_collider == null)
			{
				m_collider = GetComponent<SphereCollider>();
			}
			if (m_collider == null)
			{
				Debug.LogError("Set Collider");
			}
			base.AwakeOverride();
		}

		protected override void RecordOverride()
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(SphereCollider), "x");
			ParameterExpression parameterExpression2 = Expression.Parameter(typeof(SphereCollider), "x");
			base.RecordOverride();
			RuntimeUndo.RecordValue(m_collider, Strong.PropertyInfo(Expression.Lambda<Func<SphereCollider, Vector3>>(Expression.Property(parameterExpression, typeof(UnityEngine.SphereCollider).GetProperty("center").GetGetMethod()), new ParameterExpression[1] { parameterExpression })));
			RuntimeUndo.RecordValue(m_collider, Strong.PropertyInfo(Expression.Lambda<Func<SphereCollider, float>>(Expression.Property(parameterExpression2, typeof(UnityEngine.SphereCollider).GetProperty("radius").GetGetMethod()), new ParameterExpression[1] { parameterExpression2 })));
		}
	}
}
