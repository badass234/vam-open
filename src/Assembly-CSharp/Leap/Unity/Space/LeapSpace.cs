using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity.Space
{
	public abstract class LeapSpace : LeapSpaceAnchor
	{
		private static List<LeapSpace> _enabledSpaces = new List<LeapSpace>();

		private List<LeapSpaceAnchor> _anchors = new List<LeapSpaceAnchor>();

		public static List<LeapSpace> allEnabled => _enabledSpaces;

		public List<LeapSpaceAnchor> anchors => _anchors;

		protected override void OnEnable()
		{
			base.OnEnable();
			_enabledSpaces.Add(this);
			RebuildHierarchy();
			RecalculateTransformers();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			_enabledSpaces.Remove(this);
			for (int i = 0; i < _anchors.Count; i++)
			{
				_anchors[i].space = null;
				_anchors[i].transformer = null;
			}
		}

		public void RebuildHierarchy()
		{
			_anchors.Clear();
			rebuildHierarchyRecursively(base.transform);
		}

		public void RecalculateTransformers()
		{
			transformer = CosntructBaseTransformer();
			for (int i = 1; i < _anchors.Count; i++)
			{
				LeapSpaceAnchor leapSpaceAnchor = _anchors[i];
				LeapSpaceAnchor leapSpaceAnchor2 = leapSpaceAnchor.parent;
				UpdateTransformer(leapSpaceAnchor.transformer, leapSpaceAnchor2.transformer);
			}
		}

		public abstract Hash GetSettingHash();

		protected abstract ITransformer CosntructBaseTransformer();

		protected abstract ITransformer ConstructTransformer(LeapSpaceAnchor anchor);

		protected abstract void UpdateTransformer(ITransformer transformer, ITransformer parent);

		private void rebuildHierarchyRecursively(Transform root)
		{
			LeapSpaceAnchor component = root.GetComponent<LeapSpaceAnchor>();
			if (component != null && component.enabled)
			{
				component.space = this;
				component.RecalculateParentAnchor();
				component.transformer = ConstructTransformer(component);
				_anchors.Add(component);
			}
			int childCount = root.childCount;
			for (int i = 0; i < childCount; i++)
			{
				rebuildHierarchyRecursively(root.GetChild(i));
			}
		}
	}
}
