using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace ZenFulcrum.EmbeddedBrowser
{
	public class ActionTimer : MonoBehaviour
	{
		[Serializable]
		public class TimedAction
		{
			public float delay;

			public UnityEvent action;
		}

		public TimedAction[] thingsToDo;

		private bool triggered;

		public void OnTriggerEnter(Collider other)
		{
			if (!triggered)
			{
				PlayerInventory component = other.GetComponent<PlayerInventory>();
				if ((bool)component)
				{
					triggered = true;
					StartCoroutine(DoThings());
				}
			}
		}

		private IEnumerator DoThings()
		{
			for (int idx = 0; idx < thingsToDo.Length; idx++)
			{
				yield return new WaitForSeconds(thingsToDo[idx].delay);
				thingsToDo[idx].action.Invoke();
			}
		}
	}
}
