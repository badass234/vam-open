using System;
using System.Linq.Expressions;
using System.Reflection;
using Battlehub.RTCommon;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTGizmos
{
	public class PointLightGizmo : SphereGizmo
	{
		[SerializeField]
		private Light m_light;

		protected override Vector3 Center
		{
			get
			{
				return Vector3.zero;
			}
			set
			{
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
			if (m_light.type != LightType.Point)
			{
				Debug.LogWarning("m_light.Type != LightType.Point");
			}
			base.AwakeOverride();
		}

		protected override void RecordOverride()
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(Light), "x");
			base.RecordOverride();
			RuntimeUndo.RecordValue(m_light, Strong.PropertyInfo(Expression.Lambda<Func<Light, float>>(Expression.Property(parameterExpression, typeof(UnityEngine.Light).GetProperty("range").GetGetMethod()), new ParameterExpression[1] { parameterExpression })));
		}

		protected override bool OnDrag(int index, Vector3 offset)
		{
			Radius += offset.magnitude * Mathf.Sign(Vector3.Dot(offset, HandlesNormals[index]));
			if (Radius < 0f)
			{
				Radius = 0f;
				return false;
			}
			return true;
		}

		private void Reset()
		{
			LineColor = new Color(1f, 1f, 0.5f, 0.5f);
			HandlesColor = new Color(1f, 1f, 0.35f, 0.95f);
			SelectionColor = new Color(1f, 1f, 0f, 1f);
		}
	}
}
