using UnityEngine;

namespace Battlehub.RTCommon
{
	public class RuntimeUndoComponent : MonoBehaviour
	{
		public KeyCode UndoKey = KeyCode.Z;

		public KeyCode RedoKey = KeyCode.Y;

		public KeyCode RuntimeModifierKey = KeyCode.LeftControl;

		public KeyCode EditorModifierKey = KeyCode.LeftShift;

		private static RuntimeUndoComponent m_instance;

		public KeyCode ModifierKey => RuntimeModifierKey;

		public static bool IsInitialized => m_instance != null;

		private void Awake()
		{
			if (m_instance == null)
			{
				m_instance = this;
			}
		}

		private void OnDestroy()
		{
			if (m_instance == this)
			{
				m_instance = null;
			}
		}

		private void Update()
		{
			if (InputController.GetKeyDown(UndoKey) && InputController.GetKey(ModifierKey))
			{
				RuntimeUndo.Undo();
			}
			else if (InputController.GetKeyDown(RedoKey) && InputController.GetKey(ModifierKey))
			{
				RuntimeUndo.Redo();
			}
		}
	}
}
