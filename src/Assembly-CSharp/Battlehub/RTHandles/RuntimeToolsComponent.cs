using Battlehub.RTCommon;
using UnityEngine;

namespace Battlehub.RTHandles
{
	public class RuntimeToolsComponent : MonoBehaviour
	{
		public KeyCode ViewToolKey = KeyCode.Q;

		public KeyCode MoveToolKey = KeyCode.W;

		public KeyCode RotateToolKey = KeyCode.E;

		public KeyCode ScaleToolKey = KeyCode.R;

		public KeyCode PivotRotationKey = KeyCode.X;

		public KeyCode PivotModeKey = KeyCode.Z;

		private void Awake()
		{
			UnityEditorToolsListener.ToolChanged += OnUnityEditorToolChanged;
		}

		private void Start()
		{
			RuntimeTools.Current = RuntimeTool.Move;
		}

		private void OnDestroy()
		{
			UnityEditorToolsListener.ToolChanged -= OnUnityEditorToolChanged;
		}

		private void Update()
		{
			if (RuntimeTools.ActiveTool != null)
			{
				return;
			}
			bool flag = RuntimeEditorApplication.IsActiveWindow(RuntimeWindowType.GameView);
			if (RuntimeTools.IsViewing || flag)
			{
				return;
			}
			if (InputController.GetKeyDown(ViewToolKey))
			{
				RuntimeTools.Current = RuntimeTool.View;
			}
			else if (InputController.GetKeyDown(MoveToolKey))
			{
				RuntimeTools.Current = RuntimeTool.Move;
			}
			else if (InputController.GetKeyDown(RotateToolKey))
			{
				RuntimeTools.Current = RuntimeTool.Rotate;
			}
			else if (InputController.GetKeyDown(ScaleToolKey))
			{
				RuntimeTools.Current = RuntimeTool.Scale;
			}
			if (InputController.GetKeyDown(PivotRotationKey))
			{
				if (RuntimeTools.PivotRotation == RuntimePivotRotation.Local)
				{
					RuntimeTools.PivotRotation = RuntimePivotRotation.Global;
				}
				else
				{
					RuntimeTools.PivotRotation = RuntimePivotRotation.Local;
				}
			}
			if (InputController.GetKeyDown(PivotModeKey) && !InputController.GetKey(KeyCode.LeftControl) && !InputController.GetKey(KeyCode.LeftShift))
			{
				if (RuntimeTools.PivotMode == RuntimePivotMode.Center)
				{
					RuntimeTools.PivotMode = RuntimePivotMode.Pivot;
				}
				else
				{
					RuntimeTools.PivotMode = RuntimePivotMode.Center;
				}
			}
		}

		private void OnUnityEditorToolChanged()
		{
		}
	}
}
