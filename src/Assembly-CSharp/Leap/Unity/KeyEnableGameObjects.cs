using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity
{
	public class KeyEnableGameObjects : MonoBehaviour
	{
		public List<GameObject> targets;

		[Header("Controls")]
		public KeyCode unlockHold = KeyCode.RightShift;

		public KeyCode toggle = KeyCode.T;

		private void Update()
		{
			if ((unlockHold == KeyCode.None || Input.GetKey(unlockHold)) && Input.GetKeyDown(toggle))
			{
				for (int i = 0; i < targets.Count; i++)
				{
					targets[i].SetActive(!targets[i].activeSelf);
				}
			}
		}
	}
}
