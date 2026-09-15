using UnityEngine;

namespace Leap.Unity.Space
{
	[DisallowMultipleComponent]
	public class LeapSpaceAnchor : MonoBehaviour
	{
		[HideInInspector]
		public LeapSpaceAnchor parent;

		[HideInInspector]
		public LeapSpace space;

		public ITransformer transformer;

		protected virtual void OnEnable()
		{
		}

		protected virtual void OnDisable()
		{
		}

		public void RecalculateParentAnchor()
		{
			if (this is LeapSpace)
			{
				parent = this;
			}
			else
			{
				parent = GetAnchor(base.transform.parent);
			}
		}

		public static LeapSpaceAnchor GetAnchor(Transform root)
		{
			LeapSpaceAnchor component;
			while (true)
			{
				if (root == null)
				{
					return null;
				}
				component = root.GetComponent<LeapSpaceAnchor>();
				if (component != null && component.enabled)
				{
					break;
				}
				root = root.parent;
			}
			return component;
		}
	}
}
