using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DynamicCSharp.Demo
{
	public sealed class TankManagerAsync : MonoBehaviour
	{
		private ScriptDomain domain;

		private Vector2 startPosition;

		private Quaternion startRotation;

		private string initialText = string.Empty;

		private int counter;

		private float timer;

		private const string newTemplate = "BlankTemplate";

		private const string exampleTemplate = "ExampleTemplate";

		public GameObject bulletObject;

		public GameObject tankObject;

		public Text statusText;

		public void Awake()
		{
			if (statusText != null)
			{
				initialText = statusText.text;
			}
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
				StartCoroutine(RunTankScript(ui.codeEditor.text));
			});
		}

		public void Update()
		{
			if (!(statusText != null))
			{
				return;
			}
			if (domain.CompilerService.IsCompiling)
			{
				if (Time.time > timer + 0.1f)
				{
					timer = Time.time;
					counter++;
					if (counter > 3)
					{
						counter = 0;
					}
					statusText.text = "Compiling";
					for (int i = 0; i < counter; i++)
					{
						statusText.text += '.';
					}
				}
			}
			else
			{
				statusText.text = initialText;
			}
		}

		public IEnumerator RunTankScript(string source)
		{
			TankController old = tankObject.GetComponent<TankController>();
			if (old != null)
			{
				UnityEngine.Object.Destroy(old);
			}
			RespawnTank();
			AsyncCompileLoadOperation task = domain.CompileAndLoadScriptSourcesAsync(source);
			yield return task;
			if (!task.IsSuccessful)
			{
				Debug.LogError("Compile failed");
				yield break;
			}
			ScriptType type = task.MainType;
			if (type.IsSubtypeOf<TankController>())
			{
				ScriptProxy scriptProxy = type.CreateInstance(tankObject);
				if (scriptProxy == null)
				{
					Debug.LogError($"Failed to create an instance of '{type.RawType}'");
					yield break;
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
