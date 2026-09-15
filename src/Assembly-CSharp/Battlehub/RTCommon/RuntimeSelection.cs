using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTCommon
{
	public class RuntimeSelection
	{
		private static HashSet<Object> m_selectionHS;

		protected static Object m_activeObject;

		protected static Object[] m_objects;

		public static GameObject activeGameObject
		{
			get
			{
				return activeObject as GameObject;
			}
			set
			{
				activeObject = value;
			}
		}

		public static Object activeObject
		{
			get
			{
				return m_activeObject;
			}
			set
			{
				if (m_activeObject != value || (value != null && m_objects != null && m_objects.Length > 1))
				{
					RuntimeUndo.RecordSelection();
					m_activeObject = value;
					Object[] unselectedObjects = m_objects;
					if (m_activeObject != null)
					{
						m_objects = new Object[1] { value };
					}
					else
					{
						m_objects = new Object[0];
					}
					UpdateHS();
					RuntimeUndo.RecordSelection();
					RaiseSelectionChanged(unselectedObjects);
				}
			}
		}

		public static Object[] objects
		{
			get
			{
				return m_objects;
			}
			set
			{
				if (IsSelectionChanged(value))
				{
					RuntimeUndo.RecordSelection();
					SetObjects(value);
					RuntimeUndo.RecordSelection();
				}
			}
		}

		public static GameObject[] gameObjects
		{
			get
			{
				if (m_objects == null)
				{
					return null;
				}
				return m_objects.OfType<GameObject>().ToArray();
			}
		}

		public static Transform activeTransform
		{
			get
			{
				if (m_activeObject == null)
				{
					return null;
				}
				if (m_activeObject is GameObject)
				{
					return ((GameObject)m_activeObject).transform;
				}
				return null;
			}
		}

		public static event RuntimeSelectionChanged SelectionChanged;

		protected static void RaiseSelectionChanged(Object[] unselectedObjects)
		{
			if (RuntimeSelection.SelectionChanged != null)
			{
				RuntimeSelection.SelectionChanged(unselectedObjects);
			}
		}

		public static bool IsSelected(Object obj)
		{
			if (m_selectionHS == null)
			{
				return false;
			}
			return m_selectionHS.Contains(obj);
		}

		private static void UpdateHS()
		{
			if (m_objects != null)
			{
				m_selectionHS = new HashSet<Object>(m_objects);
			}
			else
			{
				m_selectionHS = null;
			}
		}

		private static bool IsSelectionChanged(Object[] value)
		{
			if (m_objects == value)
			{
				return false;
			}
			if (m_objects == null)
			{
				return value.Length != 0;
			}
			if (value == null)
			{
				return m_objects.Length != 0;
			}
			if (m_objects.Length != value.Length)
			{
				return true;
			}
			for (int i = 0; i < m_objects.Length; i++)
			{
				if (m_objects[i] != value[i])
				{
					return true;
				}
			}
			return false;
		}

		protected static void SetObjects(Object[] value)
		{
			if (!IsSelectionChanged(value))
			{
				return;
			}
			Object[] unselectedObjects = m_objects;
			if (value == null)
			{
				m_objects = null;
				m_activeObject = null;
			}
			else
			{
				m_objects = value.ToArray();
				if (m_activeObject == null || !m_objects.Contains(m_activeObject))
				{
					m_activeObject = m_objects.OfType<Object>().FirstOrDefault();
				}
			}
			UpdateHS();
			RaiseSelectionChanged(unselectedObjects);
		}

		public static void Select(Object activeObject, Object[] selection)
		{
			if (IsSelectionChanged(selection))
			{
				RuntimeUndo.RecordSelection();
				m_activeObject = activeObject;
				SetObjects(selection);
				RuntimeUndo.RecordSelection();
			}
		}
	}
}
