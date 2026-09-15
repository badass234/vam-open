using System;
using System.Collections;
using Leap.Unity.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace Leap.Unity.Animation
{
	public class TransformTweenBehaviour : MonoBehaviour
	{
		[Serializable]
		public class FloatEvent : UnityEvent<float>
		{
		}

		public enum EventType
		{
			OnLeaveStart = 110,
			OnReachEnd = 120,
			OnLeaveEnd = 130,
			OnReachStart = 140
		}

		[Tooltip("The transform to which to apply the tweened properties.")]
		public Transform targetTransform;

		[Tooltip("The transform whose position/rotation/localScale provide the start state of the tween.")]
		public Transform startTransform;

		[Tooltip("The transform whose position/rotation/localScale provide the end state of the tween.")]
		public Transform endTransform;

		public bool startAtEnd;

		[Header("Tween Settings")]
		public bool tweenLocalPosition = true;

		public bool tweenLocalRotation = true;

		public bool tweenLocalScale = true;

		[MinValue(0.001f)]
		public float tweenDuration = 0.25f;

		public SmoothType tweenSmoothType = SmoothType.Smooth;

		public Action<float> OnProgress = delegate
		{
		};

		public Action OnLeaveStart = delegate
		{
		};

		public Action OnReachEnd = delegate
		{
		};

		public Action OnLeaveEnd = delegate
		{
		};

		public Action OnReachStart = delegate
		{
		};

		private Tween _tween;

		private Coroutine _playTweenAfterDelayCoroutine;

		private Direction _curDelayedDirection = Direction.Backward;

		[SerializeField]
		private EnumEventTable _eventTable;

		public Tween tween
		{
			get
			{
				return _tween;
			}
			set
			{
				_tween = value;
			}
		}

		private void OnValidate()
		{
			if (targetTransform != null)
			{
				if (startTransform == targetTransform)
				{
					Debug.LogError("The start transform of the TransformTweenBehaviour should be a different transform than the target transform; the start transform provides starting position/rotation/scale information for the tween.", base.gameObject);
				}
				else if (endTransform == targetTransform)
				{
					Debug.LogError("The end transform of the TransformTweenBehaviour should be a different transform than the target transform; the end transform provides ending position/rotation/scale information for the tween.", base.gameObject);
				}
			}
		}

		private void Awake()
		{
			initUnityEvents();
			_tween = Tween.Persistent().OverTime(tweenDuration).Smooth(tweenSmoothType);
			if (tweenLocalPosition)
			{
				_tween = _tween.Target(targetTransform).LocalPosition(startTransform, endTransform);
			}
			if (tweenLocalRotation)
			{
				_tween = _tween.Target(targetTransform).LocalRotation(startTransform, endTransform);
			}
			if (tweenLocalScale)
			{
				_tween = _tween.Target(targetTransform).LocalScale(startTransform, endTransform);
			}
			_tween.OnProgress(OnProgress);
			_tween.OnLeaveStart(OnLeaveStart);
			_tween.OnReachEnd(OnReachEnd);
			_tween.OnLeaveEnd(OnLeaveEnd);
			_tween.OnReachStart(OnReachStart);
			if (startAtEnd)
			{
				_tween.progress = 0.9999999f;
				_tween.Play(Direction.Forward);
			}
			else
			{
				_tween.progress = 1E-07f;
				_tween.Play(Direction.Backward);
			}
		}

		private void OnDestroy()
		{
			if (_tween.isValid)
			{
				_tween.Release();
			}
		}

		public void PlayTween()
		{
			PlayTween(Direction.Forward, 0f);
		}

		public void PlayTween(Direction tweenDirection = Direction.Forward, float afterDelay = 0f)
		{
			if (_playTweenAfterDelayCoroutine != null && tweenDirection != _curDelayedDirection)
			{
				StopCoroutine(_playTweenAfterDelayCoroutine);
				_curDelayedDirection = tweenDirection;
			}
			_playTweenAfterDelayCoroutine = StartCoroutine(playAfterDelay(tweenDirection, afterDelay));
		}

		private IEnumerator playAfterDelay(Direction tweenDirection, float delay)
		{
			yield return new WaitForSeconds(delay);
			tween.Play(tweenDirection);
		}

		public void PlayForward()
		{
			PlayTween(Direction.Forward, 0f);
		}

		public void PlayBackward()
		{
			PlayTween(Direction.Backward);
		}

		public void PlayForwardAfterDelay(float delay = 0f)
		{
			PlayTween(Direction.Forward, delay);
		}

		public void PlayBackwardAfterDelay(float delay = 0f)
		{
			PlayTween(Direction.Backward, delay);
		}

		public void StopTween()
		{
			tween.Stop();
		}

		public void SetTargetToStart()
		{
			setTargetTo(startTransform);
		}

		public void SetTargetToEnd()
		{
			setTargetTo(endTransform);
		}

		private void setTargetTo(Transform t)
		{
			if (targetTransform != null && t != null)
			{
				if (tweenLocalPosition)
				{
					targetTransform.localPosition = t.localPosition;
				}
				if (tweenLocalRotation)
				{
					targetTransform.localRotation = t.localRotation;
				}
				if (tweenLocalScale)
				{
					targetTransform.localScale = t.localScale;
				}
			}
		}

		private void initUnityEvents()
		{
			setupCallback(ref OnLeaveStart, EventType.OnLeaveStart);
			setupCallback(ref OnReachEnd, EventType.OnReachEnd);
			setupCallback(ref OnLeaveEnd, EventType.OnLeaveEnd);
			setupCallback(ref OnReachStart, EventType.OnReachStart);
		}

		private void setupCallback(ref Action action, EventType type)
		{
			action = (Action)Delegate.Combine(action, (Action)delegate
			{
				_eventTable.Invoke((int)type);
			});
		}

		private void setupCallback<T>(ref Action<T> action, EventType type)
		{
			action = (Action<T>)Delegate.Combine(action, (Action<T>)delegate
			{
				_eventTable.Invoke((int)type);
			});
		}
	}
}
