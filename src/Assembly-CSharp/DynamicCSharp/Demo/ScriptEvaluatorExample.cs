using UnityEngine;

namespace DynamicCSharp.Demo
{
	public class ScriptEvaluatorExample
	{
		private ScriptDomain domain;

		private ScriptEvaluator evaluator;

		private void Start()
		{
			domain = ScriptDomain.CreateDomain("EvalDomain", true);
			evaluator = new ScriptEvaluator(domain);
			evaluator.AddUsing("UnityEngine");
		}

		private void onGUI()
		{
			if (GUILayout.Button("EvalMath"))
			{
				Debug.Log(evaluator.Eval("return 6 * 3 + 20;"));
			}
			if (GUILayout.Button("EvalLoop"))
			{
				evaluator.Eval("for(int i = 0; i < 5; i++) Debug.Log(\"Hello World \" + i);");
			}
			if (GUILayout.Button("EvalVar"))
			{
				evaluator.BindVar("floatValue", 23.5f);
				evaluator.Eval("Debug.Log(floatValue + 4f);");
			}
			if (GUILayout.Button("EvalRefVar"))
			{
				Variable<float> message = evaluator.BindVar("floatValue", 12.3f);
				evaluator.Eval("floatValue *= 2;");
				Debug.Log(message);
			}
			if (GUILayout.Button("EvalDelegate"))
			{
				evaluator.BindDelegate("callback", delegate
				{
					Debug.Log("Hello from callback");
				});
				evaluator.Eval("callback();");
			}
		}
	}
}
