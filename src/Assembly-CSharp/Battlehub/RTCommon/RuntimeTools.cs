using UnityEngine;
using UnityEngine.EventSystems;

namespace Battlehub.RTCommon
{
	public static class RuntimeTools
	{
		private static RuntimeTool m_current;

		private static RuntimePivotMode m_pivotMode;

		private static RuntimePivotRotation m_pivotRotation;

		private static bool m_isViewing;

		private static bool m_showSelectionGizmos;

		private static bool m_showGizmos;

		private static bool m_autoFocus;

		private static bool m_unitSnapping;

		private static bool m_isSnapping;

		private static SnappingMode m_snappingMode;

		private static GameObject m_spawnPrefab;

		public static LockObject m_lockAxes;

		public static bool IsViewing
		{
			get
			{
				return m_isViewing;
			}
			set
			{
				if (m_isViewing != value)
				{
					m_isViewing = value;
					if (m_isViewing)
					{
						ActiveTool = null;
					}
					if (RuntimeTools.IsViewingChanged != null)
					{
						RuntimeTools.IsViewingChanged();
					}
				}
			}
		}

		public static bool ShowSelectionGizmos
		{
			get
			{
				return m_showSelectionGizmos;
			}
			set
			{
				if (m_showSelectionGizmos != value)
				{
					m_showSelectionGizmos = value;
					if (RuntimeTools.ShowSelectionGizmosChanged != null)
					{
						RuntimeTools.ShowSelectionGizmosChanged();
					}
				}
			}
		}

		public static bool ShowGizmos
		{
			get
			{
				return m_showGizmos;
			}
			set
			{
				if (m_showGizmos != value)
				{
					m_showGizmos = value;
					if (RuntimeTools.ShowGizmosChanged != null)
					{
						RuntimeTools.ShowGizmosChanged();
					}
				}
			}
		}

		public static bool AutoFocus
		{
			get
			{
				return m_autoFocus;
			}
			set
			{
				if (m_autoFocus != value)
				{
					m_autoFocus = value;
					if (RuntimeTools.AutoFocusChanged != null)
					{
						RuntimeTools.AutoFocusChanged();
					}
				}
			}
		}

		public static bool UnitSnapping
		{
			get
			{
				return m_unitSnapping;
			}
			set
			{
				if (m_unitSnapping != value)
				{
					m_unitSnapping = value;
					if (RuntimeTools.UnitSnappingChanged != null)
					{
						RuntimeTools.UnitSnappingChanged();
					}
				}
			}
		}

		public static bool IsSnapping
		{
			get
			{
				return m_isSnapping;
			}
			set
			{
				if (m_isSnapping != value)
				{
					m_isSnapping = value;
					if (RuntimeTools.IsSnappingChanged != null)
					{
						RuntimeTools.IsSnappingChanged();
					}
				}
			}
		}

		public static SnappingMode SnappingMode
		{
			get
			{
				return m_snappingMode;
			}
			set
			{
				if (m_snappingMode != value)
				{
					m_snappingMode = value;
					if (RuntimeTools.SnappingModeChanged != null)
					{
						RuntimeTools.SnappingModeChanged();
					}
				}
			}
		}

		public static GameObject SpawnPrefab
		{
			get
			{
				return m_spawnPrefab;
			}
			set
			{
				if (m_spawnPrefab != value)
				{
					GameObject spawnPrefab = m_spawnPrefab;
					m_spawnPrefab = value;
					if (RuntimeTools.SpawnPrefabChanged != null)
					{
						RuntimeTools.SpawnPrefabChanged(spawnPrefab);
					}
				}
			}
		}

		public static Object ActiveTool { get; set; }

		public static LockObject LockAxes
		{
			get
			{
				return m_lockAxes;
			}
			set
			{
				if (m_lockAxes != value)
				{
					m_lockAxes = value;
					if (RuntimeTools.LockAxesChanged != null)
					{
						RuntimeTools.LockAxesChanged();
					}
				}
			}
		}

		public static RuntimeTool Current
		{
			get
			{
				return m_current;
			}
			set
			{
				if (m_current != value)
				{
					m_current = value;
					if (RuntimeTools.ToolChanged != null)
					{
						RuntimeTools.ToolChanged();
					}
				}
			}
		}

		public static RuntimePivotRotation PivotRotation
		{
			get
			{
				return m_pivotRotation;
			}
			set
			{
				if (m_pivotRotation != value)
				{
					m_pivotRotation = value;
					if (RuntimeTools.PivotRotationChanged != null)
					{
						RuntimeTools.PivotRotationChanged();
					}
				}
			}
		}

		public static RuntimePivotMode PivotMode
		{
			get
			{
				return m_pivotMode;
			}
			set
			{
				if (m_pivotMode != value)
				{
					m_pivotMode = value;
					if (RuntimeTools.PivotModeChanged != null)
					{
						RuntimeTools.PivotModeChanged();
					}
				}
			}
		}

		public static bool DrawSelectionGizmoRay { get; set; }

		public static event RuntimeToolsEvent ToolChanged;

		public static event RuntimeToolsEvent PivotRotationChanged;

		public static event RuntimeToolsEvent PivotModeChanged;

		public static event SpawnPrefabChanged SpawnPrefabChanged;

		public static event RuntimeToolsEvent IsViewingChanged;

		public static event RuntimeToolsEvent ShowSelectionGizmosChanged;

		public static event RuntimeToolsEvent ShowGizmosChanged;

		public static event RuntimeToolsEvent AutoFocusChanged;

		public static event RuntimeToolsEvent UnitSnappingChanged;

		public static event RuntimeToolsEvent IsSnappingChanged;

		public static event RuntimeToolsEvent SnappingModeChanged;

		public static event RuntimeToolsEvent LockAxesChanged;

		static RuntimeTools()
		{
			m_snappingMode = SnappingMode.Vertex;
			Reset();
		}

		public static void Reset()
		{
			ActiveTool = null;
			LockAxes = null;
			m_isViewing = false;
			m_isSnapping = false;
			m_showSelectionGizmos = true;
			m_showGizmos = true;
			m_unitSnapping = false;
			m_pivotMode = RuntimePivotMode.Center;
			SpawnPrefab = null;
		}

		public static bool IsPointerOverGameObject()
		{
			return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
		}
	}
}
