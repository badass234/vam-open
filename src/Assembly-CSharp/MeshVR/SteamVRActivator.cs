using System.Collections;
using UnityEngine;
using Valve.VR;

namespace MeshVR
{
	public class SteamVRActivator : MonoBehaviour
	{
		public SteamVR_Behaviour_Pose trackedObject;

		public GameObject[] objectsToActivateOnTracking;

		public bool isActive
		{
			get
			{
				if (trackedObject != null)
				{
					return trackedObject.isActive;
				}
				return base.gameObject.activeInHierarchy;
			}
		}

		public bool isPoseValid
		{
			get
			{
				if (trackedObject != null)
				{
					return trackedObject.isValid;
				}
				return false;
			}
		}

		protected void Activate()
		{
			GameObject[] array = objectsToActivateOnTracking;
			foreach (GameObject gameObject in array)
			{
				if (gameObject != null)
				{
					gameObject.SetActive(true);
				}
			}
		}

		protected void Deactivate()
		{
			GameObject[] array = objectsToActivateOnTracking;
			foreach (GameObject gameObject in array)
			{
				if (gameObject != null)
				{
					gameObject.SetActive(false);
				}
			}
		}

		protected void OnConnectedChanged(SteamVR_Behaviour_Pose pose, SteamVR_Input_Sources sources, bool connected)
		{
			if (connected)
			{
				Debug.Log(string.Concat("Device ", sources, " connected"));
				Activate();
			}
			else
			{
				Debug.Log(string.Concat("Device ", sources, " disconnected"));
				Deactivate();
			}
		}

		protected virtual void Awake()
		{
			if (trackedObject == null)
			{
				trackedObject = base.gameObject.GetComponent<SteamVR_Behaviour_Pose>();
			}
			if (trackedObject != null)
			{
				trackedObject.onConnectedChanged.AddListener(OnConnectedChanged);
			}
		}

		protected virtual IEnumerator Start()
		{
			while (!isPoseValid)
			{
				yield return null;
			}
			Activate();
		}
	}
}
