using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTCommon
{
	public static class RuntimeEditorApplication
	{
		private static List<RuntimeEditorWindow> m_windows;

		private static RuntimeEditorWindow m_pointerOverWindow;

		private static RuntimeEditorWindow m_activeWindow;

		private static int m_activeCameraIndex;

		private static bool m_isOpened;

		private static bool m_isPlayModeStateChanging;

		private static bool m_isPlaying;

		public static RuntimeEditorWindow PointerOverWindow => m_pointerOverWindow;

		public static RuntimeWindowType PointerOverWindowType
		{
			get
			{
				if (m_pointerOverWindow == null)
				{
					return RuntimeWindowType.None;
				}
				return m_pointerOverWindow.WindowType;
			}
		}

		public static RuntimeEditorWindow ActiveWindow => m_activeWindow;

		public static RuntimeWindowType ActiveWindowType
		{
			get
			{
				if (m_activeWindow == null)
				{
					return RuntimeWindowType.None;
				}
				return m_activeWindow.WindowType;
			}
		}

		public static Camera[] GameCameras { get; set; }

		public static Camera[] SceneCameras { get; set; }

		public static int ActiveSceneCameraIndex
		{
			get
			{
				return m_activeCameraIndex;
			}
			set
			{
				if (m_activeCameraIndex != value)
				{
					m_activeCameraIndex = value;
					if (RuntimeEditorApplication.ActiveSceneCameraChanged != null)
					{
						RuntimeEditorApplication.ActiveSceneCameraChanged();
					}
				}
			}
		}

		public static Camera ActiveSceneCamera
		{
			get
			{
				if (SceneCameras == null || SceneCameras.Length == 0)
				{
					return null;
				}
				return SceneCameras[ActiveSceneCameraIndex];
			}
		}

		public static bool IsOpened
		{
			get
			{
				return m_isOpened;
			}
			set
			{
				if (m_isOpened != value)
				{
					m_isOpened = value;
					if (!m_isOpened)
					{
						ActivateWindow(GetWindow(RuntimeWindowType.GameView));
					}
					if (RuntimeEditorApplication.IsOpenedChanged != null)
					{
						RuntimeEditorApplication.IsOpenedChanged();
					}
				}
			}
		}

		public static bool IsPlaymodeStateChanging => m_isPlayModeStateChanging;

		public static bool IsPlaying
		{
			get
			{
				return m_isPlaying;
			}
			set
			{
				if (m_isPlaying != value)
				{
					m_isPlaying = value;
					m_isPlayModeStateChanging = true;
					if (RuntimeEditorApplication.PlaymodeStateChanging != null)
					{
						RuntimeEditorApplication.PlaymodeStateChanging();
					}
					if (RuntimeEditorApplication.PlaymodeStateChanged != null)
					{
						RuntimeEditorApplication.PlaymodeStateChanged();
					}
					m_isPlayModeStateChanging = false;
				}
			}
		}

		public static event RuntimeEditorEvent PlaymodeStateChanging;

		public static event RuntimeEditorEvent PlaymodeStateChanged;

		public static event RuntimeEditorEvent ActiveWindowChanged;

		public static event RuntimeEditorEvent PointerOverWindowChanged;

		public static event RuntimeEditorEvent IsOpenedChanged;

		public static event RuntimeEditorEvent ActiveSceneCameraChanged;

		public static event RuntimeEditorEvent SaveSelectedObjectsRequired;

		static RuntimeEditorApplication()
		{
			m_windows = new List<RuntimeEditorWindow>();
			Reset();
		}

		public static void SaveSelectedObjects()
		{
			if (RuntimeEditorApplication.SaveSelectedObjectsRequired != null)
			{
				RuntimeEditorApplication.SaveSelectedObjectsRequired();
			}
		}

		public static void Reset()
		{
			m_windows = new List<RuntimeEditorWindow>();
			m_pointerOverWindow = null;
			m_activeWindow = null;
			m_activeCameraIndex = 0;
			GameCameras = null;
			SceneCameras = null;
			m_isOpened = false;
			m_isPlaying = false;
			RuntimeSelection.objects = null;
			RuntimeUndo.Reset();
			RuntimeTools.Reset();
		}

		public static RuntimeEditorWindow GetWindow(RuntimeWindowType type)
		{
			return m_windows.Where((RuntimeEditorWindow wnd) => wnd != null && wnd.WindowType == type).FirstOrDefault();
		}

		public static void ActivateWindow(RuntimeEditorWindow window)
		{
			if (m_activeWindow != window)
			{
				m_activeWindow = window;
				if (RuntimeEditorApplication.ActiveWindowChanged != null)
				{
					RuntimeEditorApplication.ActiveWindowChanged();
				}
			}
		}

		public static void ActivateWindow(RuntimeWindowType type)
		{
			ActivateWindow(GetWindow(type));
		}

		public static void PointerEnter(RuntimeEditorWindow window)
		{
			if (m_pointerOverWindow != window)
			{
				m_pointerOverWindow = window;
				if (RuntimeEditorApplication.PointerOverWindowChanged != null)
				{
					RuntimeEditorApplication.PointerOverWindowChanged();
				}
			}
		}

		public static void PointerExit(RuntimeEditorWindow window)
		{
			if (m_pointerOverWindow == window && m_pointerOverWindow != null)
			{
				m_pointerOverWindow = null;
				if (RuntimeEditorApplication.PointerOverWindowChanged != null)
				{
					RuntimeEditorApplication.PointerOverWindowChanged();
				}
			}
		}

		public static bool IsPointerOverWindow(RuntimeWindowType type)
		{
			return PointerOverWindowType == type;
		}

		public static bool IsPointerOverWindow(RuntimeEditorWindow window)
		{
			return m_pointerOverWindow == window;
		}

		public static bool IsActiveWindow(RuntimeWindowType type)
		{
			return ActiveWindowType == type;
		}

		public static bool IsActiveWindow(RuntimeEditorWindow window)
		{
			return m_activeWindow == window;
		}

		public static void AddWindow(RuntimeEditorWindow window)
		{
			m_windows.Add(window);
		}

		public static void RemoveWindow(RuntimeEditorWindow window)
		{
			if (m_windows != null)
			{
				m_windows.Remove(window);
			}
		}
	}
}
