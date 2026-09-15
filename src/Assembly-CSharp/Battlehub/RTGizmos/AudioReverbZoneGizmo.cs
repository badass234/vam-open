using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Battlehub.RTCommon;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTGizmos
{
	public class AudioReverbZoneGizmo : SphereGizmo
	{
		[SerializeField]
		private AudioReverbZone m_source;

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
				m_source = GetComponent<AudioReverbZone>();
			}
			if (m_source == null)
			{
				Debug.LogError("Set AudioSource");
			}
			if (base.gameObject.GetComponents<AudioReverbZoneGizmo>().Count((AudioReverbZoneGizmo a) => a.m_source == m_source) == 1)
			{
				AudioReverbZoneGizmo audioReverbZoneGizmo = base.gameObject.AddComponent<AudioReverbZoneGizmo>();
				audioReverbZoneGizmo.LineColor = LineColor;
				audioReverbZoneGizmo.HandlesColor = HandlesColor;
				audioReverbZoneGizmo.SelectionColor = SelectionColor;
				audioReverbZoneGizmo.SelectionMargin = SelectionMargin;
				audioReverbZoneGizmo.EnableUndo = EnableUndo;
				audioReverbZoneGizmo.m_max = !m_max;
			}
			base.AwakeOverride();
		}

		protected override void RecordOverride()
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(AudioReverbZone), "x");
			ParameterExpression parameterExpression2 = Expression.Parameter(typeof(AudioReverbZone), "x");
			base.RecordOverride();
			RuntimeUndo.RecordValue(m_source, Strong.PropertyInfo(Expression.Lambda<Func<AudioReverbZone, float>>(Expression.Property(parameterExpression, typeof(UnityEngine.AudioReverbZone).GetProperty("minDistance").GetGetMethod()), new ParameterExpression[1] { parameterExpression })));
			RuntimeUndo.RecordValue(m_source, Strong.PropertyInfo(Expression.Lambda<Func<AudioReverbZone, float>>(Expression.Property(parameterExpression2, typeof(UnityEngine.AudioReverbZone).GetProperty("maxDistance").GetGetMethod()), new ParameterExpression[1] { parameterExpression2 })));
		}

		private void Reset()
		{
			LineColor = new Color(0.375f, 0.75f, 1f, 0.5f);
			HandlesColor = new Color(0.375f, 0.75f, 1f, 0.5f);
			SelectionColor = new Color(1f, 1f, 0f, 1f);
		}
	}
}
