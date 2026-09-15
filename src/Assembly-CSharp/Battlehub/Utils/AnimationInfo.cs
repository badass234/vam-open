using System;
using UnityEngine;

namespace Battlehub.Utils
{
	public abstract class AnimationInfo<TObj, TValue> : IAnimationInfo
	{
		private float m_duration;

		private float m_t;

		private TObj m_target;

		private TValue m_from;

		private TValue m_to;

		private AnimationCallback<TObj, TValue> m_callback;

		private Func<float, float> m_easingFunction;

		float IAnimationInfo.Duration => m_duration;

		float IAnimationInfo.T
		{
			get
			{
				return m_t;
			}
			set
			{
				m_t = value;
				if (m_t < 0f)
				{
					m_t = 0f;
				}
				if (!float.IsNaN(m_t))
				{
					bool flag = m_t >= m_duration;
					float t = ((!flag) ? m_easingFunction(m_t / m_duration) : 1f);
					TValue value2 = Lerp(m_from, m_to, t);
					m_callback(m_target, value2, m_t, flag);
				}
			}
		}

		public bool InProgress => m_t > 0f && m_t < m_duration;

		public AnimationInfo(TValue from, TValue to, float duration, Func<float, float> easingFunction, AnimationCallback<TObj, TValue> callback, TObj target)
		{
			if (callback == null)
			{
				throw new ArgumentNullException("callback");
			}
			if (easingFunction == null)
			{
				throw new ArgumentNullException("easingFunction");
			}
			m_target = target;
			m_from = from;
			m_to = to;
			m_duration = duration;
			m_callback = callback;
			m_easingFunction = easingFunction;
		}

		public static float EaseLinear(float t)
		{
			return t;
		}

		public static float EaseInQuad(float t)
		{
			return t * t;
		}

		public static float EaseOutQuad(float t)
		{
			return t * (2f - t);
		}

		public static float EaseInOutQuad(float t)
		{
			return (!((double)t < 0.5)) ? (-1f + (4f - 2f * t) * t) : (2f * t * t);
		}

		public static float EaseInCubic(float t)
		{
			return t * t * t;
		}

		public static float EaseOutCubic(float t)
		{
			return (t -= 1f) * t * t + 1f;
		}

		public static float EaseInOutCubic(float t)
		{
			return (!((double)t < 0.5)) ? ((t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f) : (4f * t * t * t);
		}

		public static float EaseInQuart(float t)
		{
			return t * t * t * t;
		}

		public static float EaseOutQuart(float t)
		{
			return 1f - (t -= 1f) * t * t * t;
		}

		public static float EaseInOutQuart(float t)
		{
			return (!((double)t < 0.5)) ? (1f - 8f * (t -= 1f) * t * t * t) : (8f * t * t * t * t);
		}

		public static float ElasticEaseIn(float t)
		{
			return Mathf.Sin((float)Math.PI * 26f * t) * Mathf.Pow(2f, 10f * (t - 1f));
		}

		public static float ElasticEaseOut(float t)
		{
			return Mathf.Sin((float)Math.PI * -26f * (t + 1f)) * Mathf.Pow(2f, -10f * t) + 1f;
		}

		public static float ElasticEaseInOut(float t)
		{
			if ((double)t < 0.5)
			{
				return 0.5f * Mathf.Sin((float)Math.PI * 26f * (2f * t)) * Mathf.Pow(2f, 10f * (2f * t - 1f));
			}
			return 0.5f * (Mathf.Sin((float)Math.PI * -26f * (2f * t - 1f + 1f)) * Mathf.Pow(2f, -10f * (2f * t - 1f)) + 2f);
		}

		protected abstract TValue Lerp(TValue from, TValue to, float t);

		public void Abort()
		{
			m_t = float.NaN;
		}
	}
}
