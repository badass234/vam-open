using UnityEngine;

namespace DynamicCSharp.Demo
{
	public class ScriptCoroutineExample : MonoBehaviour
	{
		private ScriptDomain domain;

		private string sourceCode = "using UnityEngine;using System.Collections;class Test : MonoBehaviour{   public IEnumerator TestMethod()   {       for(int i = 0; i < 10; i++)       {           Debug.Log(\"Hello World - From loaded behaviour code\");           yield return new WaitForSeconds(1);       }   }}";

		private void Start()
		{
			bool initCompiler = true;
			domain = ScriptDomain.CreateDomain("ModDomain", initCompiler);
			ScriptType scriptType = domain.CompileAndLoadScriptSource(sourceCode);
			ScriptProxy scriptProxy = scriptType.CreateInstance(base.gameObject);
			scriptProxy.supportCoroutines = true;
			scriptProxy.Call("TestMethod");
		}
	}
}
