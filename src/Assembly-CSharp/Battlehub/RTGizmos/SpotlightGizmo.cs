using System;
using System.Linq.Expressions;
using System.Reflection;
using Battlehub.RTCommon;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTGizmos
{
	public class SpotlightGizmo : ConeGizmo
	{
		[SerializeField]
		private Light m_light;

		protected override float Height
		{
			get
			{
				return m_light.range;
			}
			set
			{
				if (m_light != null)
				{
					m_light.range = value;
				}
			}
		}

		protected override float Radius
		{
			get
			{
				if (m_light == null)
				{
					return 0f;
				}
				return m_light.range * Mathf.Tan((float)Math.PI / 180f * m_light.spotAngle / 2f);
			}
			set
			{
				if (m_light != null)
				{
					m_light.spotAngle = Mathf.Atan2(value, m_light.range) * 57.29578f * 2f;
				}
			}
		}

		protected override void AwakeOverride()
		{
			if (m_light == null)
			{
				m_light = GetComponent<Light>();
			}
			if (m_light == null)
			{
				Debug.LogError("Set Light");
			}
			if (m_light.type != 0)
			{
				Debug.LogWarning("m_light.Type != LightType.Spot");
			}
			base.AwakeOverride();
		}

		protected override void RecordOverride()
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(Light), "x");
			ParameterExpression parameterExpression2 = Expression.Parameter(typeof(Light), "x");
			base.RecordOverride();
			RuntimeUndo.RecordValue(m_light, Strong.PropertyInfo(Expression.Lambda<Func<Light, float>>(Expression.Property(parameterExpression, typeof(UnityEngine.Light).GetProperty("range").GetGetMethod()), new ParameterExpression[1] { parameterExpression })));
			RuntimeUndo.RecordValue(m_light, Strong.PropertyInfo(Expression.Lambda<Func<Light, float>>(Expression.Property(parameterExpression2, typeof(UnityEngine.Light).GetProperty("spotAngle").GetGetMethod()), new ParameterExpression[1] { parameterExpression2 })));
		}

		private void Reset()
		{
			LineColor = new Color(1f, 1f, 0.5f, 0.5f);
			HandlesColor = new Color(1f, 1f, 0.35f, 0.95f);
			SelectionColor = new Color(1f, 1f, 0f, 1f);
		}
	}
}
