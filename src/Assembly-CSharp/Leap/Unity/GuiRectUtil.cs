using UnityEngine;

namespace Leap.Unity
{
	public static class GuiRectUtil
	{
		public static Vector3 Corner00(this Rect rect)
		{
			return new Vector3(rect.x, rect.y);
		}

		public static Vector3 Corner10(this Rect rect)
		{
			return new Vector3(rect.x + rect.width, rect.y);
		}

		public static Vector3 Corner01(this Rect rect)
		{
			return new Vector3(rect.x, rect.y + rect.height);
		}

		public static Vector3 Corner11(this Rect rect)
		{
			return new Vector3(rect.x + rect.width, rect.y + rect.height);
		}

		public static Rect Encapsulate(this Rect rect, Vector2 point)
		{
			if (point.x < rect.x)
			{
				rect.width += rect.x - point.x;
				rect.x = point.x;
			}
			else if (point.x > rect.x + rect.width)
			{
				rect.width = point.x - rect.x;
			}
			if (point.y < rect.y)
			{
				rect.height += rect.y - point.y;
				rect.y = point.y;
			}
			else if (point.y > rect.y + rect.height)
			{
				rect.height = point.y - rect.y;
			}
			return rect;
		}

		public static void SplitHorizontally(this Rect rect, out Rect left, out Rect right)
		{
			left = rect;
			left.width /= 2f;
			right = left;
			right.x += right.width;
		}

		public static void SplitHorizontallyWithRight(this Rect rect, out Rect left, out Rect right, float rightWidth)
		{
			left = rect;
			left.width -= rightWidth;
			right = left;
			right.x += right.width;
			right.width = rightWidth;
		}

		public static Rect NextLine(this Rect rect)
		{
			rect.y += rect.height;
			return rect;
		}

		public static Rect FromRight(this Rect rect, float width)
		{
			rect.x = rect.width - width;
			rect.width = width;
			return rect;
		}
	}
}
