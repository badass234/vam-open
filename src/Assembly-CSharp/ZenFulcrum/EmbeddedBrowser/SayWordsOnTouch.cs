using System;
using System.Collections;
using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser
{
	public class SayWordsOnTouch : MonoBehaviour
	{
		[Serializable]
		public class Verse
		{
			public float delay;

			[Multiline]
			public string textHTML;

			public float dwellTime = 5f;
		}

		public Verse[] thingsToSay;

		private bool triggered;

		private bool stillTriggered;

		public float extraLeaveRange;

		public static int ActiveSpeakers { get; private set; }

		public void OnTriggerEnter(Collider other)
		{
			if (triggered)
			{
				return;
			}
			PlayerInventory component = other.GetComponent<PlayerInventory>();
			if ((bool)component)
			{
				triggered = true;
				stillTriggered = true;
				ActiveSpeakers++;
				StartCoroutine(SayStuff());
				BoxCollider component2 = GetComponent<BoxCollider>();
				if ((bool)component2)
				{
					Vector3 size = component2.size;
					size.x += extraLeaveRange * 2f;
					size.y += extraLeaveRange * 2f;
					size.z += extraLeaveRange * 2f;
					component2.size = size;
				}
			}
		}

		private IEnumerator SayStuff()
		{
			for (int idx = 0; idx < thingsToSay.Length; idx++)
			{
				if (!stillTriggered)
				{
					break;
				}
				yield return new WaitForSeconds(thingsToSay[idx].delay);
				if (!stillTriggered)
				{
					break;
				}
				HUDManager.Instance.Say(thingsToSay[idx].textHTML, thingsToSay[idx].dwellTime);
			}
			ActiveSpeakers--;
			UnityEngine.Object.Destroy(base.gameObject);
		}

		public void OnTriggerExit(Collider other)
		{
			PlayerInventory component = other.GetComponent<PlayerInventory>();
			if ((bool)component)
			{
				stillTriggered = false;
			}
		}
	}
}
