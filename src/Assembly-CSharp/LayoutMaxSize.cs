using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
[RequireComponent(typeof(RectTransform))]
public class LayoutMaxSize : LayoutGroup
{
	public bool adjustHeight;

	public bool adjustWidth;

	private float lastX;

	private float lastY;

	public override void SetLayoutHorizontal()
	{
		UpdateMaxSizes();
	}

	public override void SetLayoutVertical()
	{
		UpdateMaxSizes();
	}

	public override void CalculateLayoutInputVertical()
	{
		UpdateMaxSizes();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		UpdateMaxSizes();
	}

	protected override void OnTransformChildrenChanged()
	{
		base.OnTransformChildrenChanged();
		UpdateMaxSizes();
	}

	private void Update()
	{
		UpdateMaxSizes();
	}

	private void UpdateMaxSizes()
	{
		if (base.transform.childCount <= 0)
		{
			return;
		}
		if (adjustHeight)
		{
			bool flag = true;
			float num = 0f;
			float num2 = 0f;
			for (int i = 0; i < base.transform.childCount; i++)
			{
				RectTransform component = base.transform.GetChild(i).GetComponent<RectTransform>();
				if (!(component == null))
				{
					Vector3 localPosition = component.localPosition;
					Vector2 sizeDelta = component.sizeDelta;
					Vector2 pivot = component.pivot;
					if (flag)
					{
						num = localPosition.y + sizeDelta.y * (1f - pivot.y);
						num2 = localPosition.y - sizeDelta.y * pivot.y;
					}
					else
					{
						num = Mathf.Max(num, localPosition.y + sizeDelta.y * (1f - pivot.y));
						num2 = Mathf.Min(num2, localPosition.y - sizeDelta.y * pivot.y);
					}
					flag = false;
				}
			}
			float num3 = Mathf.Abs(num - num2);
			SetLayoutInputForAxis(num3, num3, 0f, 1);
			if (num3 != lastY)
			{
				SetDirty();
			}
			lastY = num3;
		}
		if (!adjustWidth)
		{
			return;
		}
		bool flag2 = true;
		float num4 = 0f;
		float num5 = 0f;
		for (int j = 0; j < base.transform.childCount; j++)
		{
			RectTransform component2 = base.transform.GetChild(j).GetComponent<RectTransform>();
			if (!(component2 == null))
			{
				Vector3 localPosition2 = component2.localPosition;
				Vector2 sizeDelta2 = component2.sizeDelta;
				Vector2 pivot2 = component2.pivot;
				if (flag2)
				{
					num4 = localPosition2.x + sizeDelta2.x * (1f - pivot2.x);
					num5 = localPosition2.x - sizeDelta2.x * pivot2.x;
				}
				else
				{
					num4 = Mathf.Max(num4, localPosition2.x + sizeDelta2.x * (1f - pivot2.x));
					num5 = Mathf.Min(num5, localPosition2.x - sizeDelta2.x * pivot2.x);
				}
				flag2 = false;
			}
		}
		float num6 = Mathf.Abs(num4 - num5);
		SetLayoutInputForAxis(num6, num6, 0f, 0);
		if (num6 != lastX)
		{
			SetDirty();
		}
		lastX = num6;
	}
}
