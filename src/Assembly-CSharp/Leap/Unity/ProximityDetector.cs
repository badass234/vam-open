using System.Collections;
using System.Collections.Generic;
using Leap.Unity.Attributes;
using UnityEngine;

namespace Leap.Unity
{
	public class ProximityDetector : Detector
	{
		[Tooltip("Dispatched when close enough to a target.")]
		public ProximityEvent OnProximity;

		[Units("seconds")]
		[MinValue(0f)]
		[Tooltip("The interval in seconds at which to check this detector's conditions.")]
		public float Period = 0.1f;

		[Header("Detector Targets")]
		[Tooltip("The list of target objects.")]
		[DisableIf("UseLayersNotList", true, null)]
		public GameObject[] TargetObjects;

		[Tooltip("Objects with this tag are added to the list of targets.")]
		[DisableIf("UseLayersNotList", true, null)]
		public string TagName = string.Empty;

		[Tooltip("Use a Layer instead of the target list.")]
		public bool UseLayersNotList;

		[Tooltip("The Layer containing the objects to check.")]
		[DisableIf("UseLayersNotList", false, null)]
		public LayerMask Layer;

		[Header("Distance Settings")]
		[Tooltip("The target distance in meters to activate the detector.")]
		[MinValue(0f)]
		public float OnDistance = 0.01f;

		[Tooltip("The distance in meters at which to deactivate the detector.")]
		public float OffDistance = 0.015f;

		[Header("")]
		[Tooltip("Draw this detector's Gizmos, if any. (Gizmos must be on in Unity edtor, too.)")]
		public bool ShowGizmos = true;

		private IEnumerator proximityWatcherCoroutine;

		private GameObject _currentObj;

		public GameObject CurrentObject => _currentObj;

		protected virtual void OnValidate()
		{
			if (OffDistance < OnDistance)
			{
				OffDistance = OnDistance;
			}
		}

		private void Awake()
		{
			proximityWatcherCoroutine = proximityWatcher();
			if (TagName != string.Empty)
			{
				GameObject[] array = GameObject.FindGameObjectsWithTag(TagName);
				List<GameObject> list = new List<GameObject>(array.Length + TargetObjects.Length);
				for (int i = 0; i < TargetObjects.Length; i++)
				{
					list.Add(TargetObjects[i]);
				}
				for (int j = 0; j < array.Length; j++)
				{
					list.Add(array[j]);
				}
				TargetObjects = list.ToArray();
			}
		}

		private void OnEnable()
		{
			StopCoroutine(proximityWatcherCoroutine);
			StartCoroutine(proximityWatcherCoroutine);
		}

		private void OnDisable()
		{
			StopCoroutine(proximityWatcherCoroutine);
			Deactivate();
		}

		private IEnumerator proximityWatcher()
		{
			bool proximityState = false;
			while (true)
			{
				float onSquared = OnDistance * OnDistance;
				float offSquared = OffDistance * OffDistance;
				if (_currentObj != null)
				{
					if (distanceSquared(_currentObj) > offSquared)
					{
						_currentObj = null;
						proximityState = false;
					}
				}
				else if (UseLayersNotList)
				{
					Collider[] array = Physics.OverlapSphere(base.transform.position, OnDistance, Layer);
					if (array.Length > 0)
					{
						_currentObj = array[0].gameObject;
						proximityState = true;
						OnProximity.Invoke(_currentObj);
					}
				}
				else
				{
					for (int i = 0; i < TargetObjects.Length; i++)
					{
						GameObject gameObject = TargetObjects[i];
						if (distanceSquared(gameObject) < onSquared)
						{
							_currentObj = gameObject;
							proximityState = true;
							OnProximity.Invoke(_currentObj);
							break;
						}
					}
				}
				if (proximityState)
				{
					Activate();
				}
				else
				{
					Deactivate();
				}
				yield return new WaitForSeconds(Period);
			}
		}

		private float distanceSquared(GameObject target)
		{
			Collider component = target.GetComponent<Collider>();
			Vector3 vector = ((!(component != null)) ? target.transform.position : component.ClosestPointOnBounds(base.transform.position));
			return (vector - base.transform.position).sqrMagnitude;
		}
	}
}
