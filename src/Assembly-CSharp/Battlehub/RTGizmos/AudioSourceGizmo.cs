using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Battlehub.RTCommon;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTGizmos
{
	public class AudioSourceGizmo : SphereGizmo
	{
		[SerializeField]
		private AudioSource m_source;

		[SerializeField]
		[HideInInspector]
		private bool m_max = true;

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
				if (m_source == null)
				{
					return 0f;
				}
				if (m_max)
				{
					return m_source.maxDistance;
				}
				return m_source.minDistance;
			}
			set
			{
				if (m_source != null)
				{
					if (m_max)
					{
						m_source.maxDistance = value;
					}
					else
					{
						m_source.minDistance = value;
					}
				}
			}
		}

		protected override void AwakeOverride()
		{
			if (m_source == null)
			{
				m_source = GetComponent<AudioSource>();
			}
			if (m_source == null)
			{
				Debug.LogError("Set AudioSource");
			}
			if (base.gameObject.GetComponents<AudioSourceGizmo>().Count((AudioSourceGizmo a) => a.m_source == m_source) == 1)
			{
				AudioSourceGizmo audioSourceGizmo = base.gameObject.AddComponent<AudioSourceGizmo>();
				audioSourceGizmo.LineColor = LineColor;
				audioSourceGizmo.HandlesColor = HandlesColor;
				audioSourceGizmo.SelectionColor = SelectionColor;
				audioSourceGizmo.SelectionMargin = SelectionMargin;
				audioSourceGizmo.EnableUndo = EnableUndo;
				audioSourceGizmo.m_max = !m_max;
			}
			base.AwakeOverride();
		}

		protected override void RecordOverride()
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(AudioSource), "x");
			ParameterExpression parameterExpression2 = Expression.Parameter(typeof(AudioSource), "x");
			base.RecordOverride();
			RuntimeUndo.RecordValue(m_source, Strong.PropertyInfo(Expression.Lambda<Func<AudioSource, float>>(Expression.Property(parameterExpression, typeof(UnityEngine.AudioSource).GetProperty("minDistance").GetGetMethod()), new ParameterExpression[1] { parameterExpression })));
			RuntimeUndo.RecordValue(m_source, Strong.PropertyInfo(Expression.Lambda<Func<AudioSource, float>>(Expression.Property(parameterExpression2, typeof(UnityEngine.AudioSource).GetProperty("maxDistance").GetGetMethod()), new ParameterExpression[1] { parameterExpression2 })));
		}

		private void Reset()
		{
			LineColor = new Color(0.375f, 0.75f, 1f, 0.5f);
			HandlesColor = new Color(0.375f, 0.75f, 1f, 0.5f);
			SelectionColor = new Color(1f, 1f, 0f, 1f);
		}
	}
}
