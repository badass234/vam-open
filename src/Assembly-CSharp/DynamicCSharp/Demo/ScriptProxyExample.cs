using UnityEngine;

namespace DynamicCSharp.Demo
{
	public class ScriptProxyExample : MonoBehaviour
	{
		private ScriptDomain domain;

		private string sourceCode = "using UnityEngine;class Test{   public void TestMethod()   {       Debug.Log(\"Hello World - From loaded code\");   }}";

		private void Start()
		{
			bool initCompiler = true;
			domain = ScriptDomain.CreateDomain("ModDomain", initCompiler);
			ScriptType scriptType = domain.CompileAndLoadScriptSource(sourceCode);
			ScriptProxy scriptProxy = scriptType.CreateInstance();
			scriptProxy.Call("TestMethod");
		}
	}
}
