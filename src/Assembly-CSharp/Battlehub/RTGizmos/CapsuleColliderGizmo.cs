using System;
using System.Linq.Expressions;
using System.Reflection;
using Battlehub.RTCommon;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTGizmos
{
	public class CapsuleColliderGizmo : CapsuleGizmo
	{
		[SerializeField]
		private CapsuleCollider m_collider;

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

		protected override float Height
		{
			get
			{
				if (m_collider == null)
				{
					return 0f;
				}
				return m_collider.height;
			}
			set
			{
				if (m_collider != null)
				{
					m_collider.height = value;
				}
			}
		}

		protected override int Direction
		{
			get
			{
				if (m_collider == null)
				{
					return 0;
				}
				return m_collider.direction;
			}
			set
			{
				if (m_collider != null)
				{
					m_collider.direction = value;
				}
			}
		}

		protected override void AwakeOverride()
		{
			if (m_collider == null)
			{
				m_collider = GetComponent<CapsuleCollider>();
			}
			if (m_collider == null)
			{
				Debug.LogError("Set Collider");
			}
			base.AwakeOverride();
		}

		protected override void RecordOverride()
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(CapsuleCollider), "x");
			ParameterExpression parameterExpression2 = Expression.Parameter(typeof(CapsuleCollider), "x");
			ParameterExpression parameterExpression3 = Expression.Parameter(typeof(CapsuleCollider), "x");
			base.RecordOverride();
			RuntimeUndo.RecordValue(m_collider, Strong.PropertyInfo(Expression.Lambda<Func<CapsuleCollider, Vector3>>(Expression.Property(parameterExpression, typeof(UnityEngine.CapsuleCollider).GetProperty("center").GetGetMethod()), new ParameterExpression[1] { parameterExpression })));
			RuntimeUndo.RecordValue(m_collider, Strong.PropertyInfo(Expression.Lambda<Func<CapsuleCollider, float>>(Expression.Property(parameterExpression2, typeof(UnityEngine.CapsuleCollider).GetProperty("height").GetGetMethod()), new ParameterExpression[1] { parameterExpression2 })));
			RuntimeUndo.RecordValue(m_collider, Strong.PropertyInfo(Expression.Lambda<Func<CapsuleCollider, int>>(Expression.Property(parameterExpression3, typeof(UnityEngine.CapsuleCollider).GetProperty("direction").GetGetMethod()), new ParameterExpression[1] { parameterExpression3 })));
		}
	}
}
