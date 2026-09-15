using UnityEngine;

namespace Battlehub.RTCommon
{
	public class Test : MonoBehaviour
	{
		private void Awake()
		{
			RuntimeSelection.SelectionChanged += OnSelectionChanged;
		}

		private void OnDestroy()
		{
			RuntimeSelection.SelectionChanged -= OnSelectionChanged;
		}

		private void OnSelectionChanged(Object[] unselectedObjects)
		{
			Object[] objects = RuntimeSelection.objects;
		}
	}
}
