using System;
using UnityEngine;

namespace DynamicCSharp.Demo
{
	public sealed class TankManager : MonoBehaviour
	{
		private ScriptDomain domain;

		private Vector2 startPosition;

		private Quaternion startRotation;

		private const string newTemplate = "BlankTemplate";

		private const string exampleTemplate = "ExampleTemplate";

		public GameObject bulletObject;

		public GameObject tankObject;

		public void Awake()
		{
			domain = ScriptDomain.CreateDomain("ScriptDomain", true);
			startPosition = tankObject.transform.position;
			startRotation = tankObject.transform.rotation;
			CodeUI.onNewClicked = (Action<CodeUI>)Delegate.Combine(CodeUI.onNewClicked, (Action<CodeUI>)delegate(CodeUI ui)
			{
				ui.codeEditor.text = Resources.Load<TextAsset>("BlankTemplate").text;
			});
			CodeUI.onLoadClicked = (Action<CodeUI>)Delegate.Combine(CodeUI.onLoadClicked, (Action<CodeUI>)delegate(CodeUI ui)
			{
				ui.codeEditor.text = Resources.Load<TextAsset>("ExampleTemplate").text;
			});
			CodeUI.onCompileClicked = (Action<CodeUI>)Delegate.Combine(CodeUI.onCompileClicked, (Action<CodeUI>)delegate(CodeUI ui)
			{
				RunTankScript(ui.codeEditor.text);
			});
		}

		public void RunTankScript(string source)
		{
			TankController component = tankObject.GetComponent<TankController>();
			if (component != null)
			{
				UnityEngine.Object.Destroy(component);
			}
			RespawnTank();
			ScriptType scriptType = domain.CompileAndLoadScriptSource(source);
			if (scriptType == null)
			{
				Debug.LogError("Compile failed");
			}
			else if (scriptType.IsSubtypeOf<TankController>())
			{
				ScriptProxy scriptProxy = scriptType.CreateInstance(tankObject);
				if (scriptProxy == null)
				{
					Debug.LogError($"Failed to create an instance of '{scriptType.RawType}'");
					return;
				}
				scriptProxy.Fields["bulletObject"] = bulletObject;
				scriptProxy.Call("RunTank");
			}
			else
			{
				Debug.LogError("The script must inherit from 'TankController'");
			}
		}

		public void RespawnTank()
		{
			tankObject.transform.position = startPosition;
			tankObject.transform.rotation = startRotation;
		}
	}
}
