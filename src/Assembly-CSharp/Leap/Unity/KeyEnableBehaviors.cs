using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity
{
	public class KeyEnableBehaviors : MonoBehaviour
	{
		public List<Behaviour> targets;

		[Header("Controls")]
		public KeyCode unlockHold;

		public KeyCode toggle = KeyCode.Space;

		private void Update()
		{
			if ((unlockHold == KeyCode.None || Input.GetKey(unlockHold)) && Input.GetKeyDown(toggle))
			{
				for (int i = 0; i < targets.Count; i++)
				{
					targets[i].enabled = !targets[i].enabled;
				}
			}
		}
	}
}
