using UnityEngine;

namespace MeshVR
{
	public class Selectable : MonoBehaviour
	{
		public delegate void SelectionChanged(int uid, bool b);

		public SelectionChanged selectionChanged;

		public int id;

		private bool _isSelected;

		private bool _isHidden;

		private Renderer render;

		private Collider collide;

		public bool isSelected
		{
			get
			{
				return _isSelected;
			}
			set
			{
				if (_isSelected != value)
				{
					_isSelected = value;
					Renderer componentInChildren = GetComponentInChildren<Renderer>();
					if (componentInChildren != null)
					{
						componentInChildren.material.color = ((!value) ? Color.red : Color.white);
					}
					if (selectionChanged != null)
					{
						selectionChanged(id, _isSelected);
					}
				}
			}
		}

		public bool isHidden
		{
			get
			{
				return _isHidden;
			}
			set
			{
				if (_isHidden != value)
				{
					_isHidden = value;
					if (render != null)
					{
						render.enabled = !_isHidden;
					}
					if (collide != null)
					{
						collide.enabled = !_isHidden;
					}
				}
			}
		}

		private void Awake()
		{
			render = GetComponent<Renderer>();
			collide = GetComponent<Collider>();
		}

		private void OnEnable()
		{
			Selector.selectables.Add(this);
		}

		private void OnDisable()
		{
			Selector.selectables.Remove(this);
		}

		private void OnDestroy()
		{
			selectionChanged = null;
		}

		private void OnTriggerEnter(Collider other)
		{
			SelectableSelect component = other.GetComponent<SelectableSelect>();
			if (component != null && component.enabled)
			{
				isSelected = true;
				return;
			}
			SelectableUnselect component2 = other.GetComponent<SelectableUnselect>();
			if (component2 != null && component2.enabled)
			{
				isSelected = false;
			}
		}
	}
}
