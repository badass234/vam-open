using UnityEngine;

public class DemoControl : MonoBehaviour
{
	public Animator anim;

	public Transform styles;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void ToggleAnimation()
	{
		if (anim.isActiveAndEnabled)
		{
			anim.enabled = false;
		}
		else
		{
			anim.enabled = true;
		}
	}

	public void NextStyle()
	{
		int num = 0;
		for (int i = 0; i < styles.childCount; i++)
		{
			Transform child = styles.GetChild(i);
			if (child.gameObject.activeSelf)
			{
				num = i;
				break;
			}
		}
		int num2 = num + 1;
		if (num2 >= styles.childCount)
		{
			num2 = 0;
		}
		for (int j = 0; j < styles.childCount; j++)
		{
			Transform child2 = styles.GetChild(j);
			child2.gameObject.SetActive(false);
		}
		Transform child3 = styles.GetChild(num2);
		child3.gameObject.SetActive(true);
	}
}
