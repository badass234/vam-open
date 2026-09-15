using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace VikingCrewTools.UI
{
	public class SpeechBubbleManager : MonoBehaviour
	{
		public enum SpeechbubbleType
		{
			NORMAL,
			SERIOUS,
			ANGRY,
			THINKING
		}

		[Serializable]
		public class SpeechbubblePrefab
		{
			public SpeechbubbleType type;

			public GameObject prefab;
		}

		[Header("Default settings:")]
		[FormerlySerializedAs("defaultColor")]
		[SerializeField]
		private Color _defaultColor = Color.white;

		[FormerlySerializedAs("defaultTimeToLive")]
		[SerializeField]
		private float _defaultTimeToLive = 1f;

		[FormerlySerializedAs("is2D")]
		[SerializeField]
		private bool _is2D = true;

		[Tooltip("If you want to change the size of your speechbubbles in a scene without having to change the prefabs then change this value")]
		[FormerlySerializedAs("sizeMultiplier")]
		[SerializeField]
		private float _sizeMultiplier = 1f;

		[Tooltip("If you want to use different managers, for example if you want to have one manager for allies and one for enemies in order to style their speech bubbles differently set this to false. Note that you will need to keep track of a reference some other way in that case.")]
		[SerializeField]
		private bool _isSingleton = true;

		[Header("Prefabs mapping to each type:")]
		[FormerlySerializedAs("prefabs")]
		[SerializeField]
		private List<SpeechbubblePrefab> _prefabs;

		private Dictionary<SpeechbubbleType, GameObject> _prefabsDict = new Dictionary<SpeechbubbleType, GameObject>();

		private Dictionary<SpeechbubbleType, Queue<SpeechBubbleBehaviour>> _speechBubbleQueueDict = new Dictionary<SpeechbubbleType, Queue<SpeechBubbleBehaviour>>();

		[SerializeField]
		[Tooltip("Will use main camera if left as null")]
		private Camera _cam;

		private static SpeechBubbleManager _instance;

		public static SpeechBubbleManager Instance => _instance;

		public Camera Cam
		{
			get
			{
				return _cam;
			}
			set
			{
				_cam = value;
				foreach (Queue<SpeechBubbleBehaviour> value2 in _speechBubbleQueueDict.Values)
				{
					foreach (SpeechBubbleBehaviour item in value2)
					{
						item.Cam = _cam;
					}
				}
			}
		}

		protected void Awake()
		{
			if (_cam == null)
			{
				_cam = Camera.main;
			}
			if (_isSingleton)
			{
				_instance = this;
			}
			_prefabsDict.Clear();
			_speechBubbleQueueDict.Clear();
			foreach (SpeechbubblePrefab prefab in _prefabs)
			{
				_prefabsDict.Add(prefab.type, prefab.prefab);
				_speechBubbleQueueDict.Add(prefab.type, new Queue<SpeechBubbleBehaviour>());
			}
		}

		private IEnumerator DelaySpeechBubble(float delay, Transform objectToFollow, string text, SpeechbubbleType type, float timeToLive, Color color, Vector3 offset)
		{
			yield return new WaitForSeconds(delay);
			if ((bool)objectToFollow)
			{
				AddSpeechBubble(objectToFollow, text, type, timeToLive, color, offset);
			}
		}

		public SpeechBubbleBehaviour AddSpeechBubble(Vector3 position, string text, SpeechbubbleType type = SpeechbubbleType.NORMAL, float timeToLive = 0f, Color color = default(Color))
		{
			if (timeToLive == 0f)
			{
				timeToLive = _defaultTimeToLive;
			}
			if (color == default(Color))
			{
				color = _defaultColor;
			}
			SpeechBubbleBehaviour bubble = GetBubble(type);
			bubble.Setup(position, text, timeToLive, color, Cam);
			_speechBubbleQueueDict[type].Enqueue(bubble);
			return bubble;
		}

		public SpeechBubbleBehaviour AddSpeechBubble(Transform objectToFollow, string text, SpeechbubbleType type = SpeechbubbleType.NORMAL, float timeToLive = 0f, Color color = default(Color), Vector3 offset = default(Vector3))
		{
			if (timeToLive == 0f)
			{
				timeToLive = _defaultTimeToLive;
			}
			if (color == default(Color))
			{
				color = _defaultColor;
			}
			SpeechBubbleBehaviour bubble = GetBubble(type);
			bubble.Setup(objectToFollow, offset, text, timeToLive, color, Cam);
			_speechBubbleQueueDict[type].Enqueue(bubble);
			return bubble;
		}

		public void AddDelayedSpeechBubble(float delay, Transform objectToFollow, string text, SpeechbubbleType type = SpeechbubbleType.NORMAL, float timeToLive = 0f, Color color = default(Color), Vector3 offset = default(Vector3))
		{
			StartCoroutine(DelaySpeechBubble(delay, objectToFollow, text, type, timeToLive, color, offset));
		}

		private SpeechBubbleBehaviour GetBubble(SpeechbubbleType type = SpeechbubbleType.NORMAL)
		{
			SpeechBubbleBehaviour speechBubbleBehaviour;
			if (_speechBubbleQueueDict[type].Count == 0 || _speechBubbleQueueDict[type].Peek().gameObject.activeInHierarchy)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(GetPrefab(type));
				gameObject.transform.SetParent(base.transform);
				gameObject.transform.localScale = _sizeMultiplier * GetPrefab(type).transform.localScale;
				speechBubbleBehaviour = gameObject.GetComponent<SpeechBubbleBehaviour>();
				if (!_is2D)
				{
					Canvas canvas = gameObject.AddComponent<Canvas>();
					canvas.renderMode = RenderMode.WorldSpace;
					canvas.overrideSorting = true;
				}
			}
			else
			{
				speechBubbleBehaviour = _speechBubbleQueueDict[type].Dequeue();
			}
			speechBubbleBehaviour.transform.SetAsLastSibling();
			return speechBubbleBehaviour;
		}

		private GameObject GetPrefab(SpeechbubbleType type)
		{
			return _prefabsDict[type];
		}

		public SpeechbubbleType GetRandomSpeechbubbleType()
		{
			return _prefabs[UnityEngine.Random.Range(0, _prefabs.Count)].type;
		}

		public void Clear()
		{
			foreach (KeyValuePair<SpeechbubbleType, Queue<SpeechBubbleBehaviour>> item in _speechBubbleQueueDict)
			{
				foreach (SpeechBubbleBehaviour item2 in item.Value)
				{
					item2.Clear();
				}
			}
		}
	}
}
