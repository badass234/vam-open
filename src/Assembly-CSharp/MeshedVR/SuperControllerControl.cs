using UnityEngine;

namespace MeshedVR
{
	internal class SuperControllerControl : MonoBehaviour
	{
		public void SetToLastUI()
		{
			if (SuperController.singleton != null)
			{
				SuperController.singleton.SetToLastActiveUI();
			}
		}
	}
}
