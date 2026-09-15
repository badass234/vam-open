using UnityEngine;
using UnityEngine.UI;

public class HierarchicalAssignUIMaterial : MonoBehaviour
{
	public Material UIMaterial;

	private void Start()
	{
		if (UIMaterial != null)
		{
			Image[] componentsInChildren = GetComponentsInChildren<Image>(true);
			Image[] array = componentsInChildren;
			foreach (Image image in array)
			{
				image.material = UIMaterial;
			}
			Text[] componentsInChildren2 = GetComponentsInChildren<Text>(true);
			Text[] array2 = componentsInChildren2;
			foreach (Text text in array2)
			{
				text.material = UIMaterial;
			}
		}
	}
}
