using UnityEngine;

namespace DynamicCSharp.Demo
{
	public class ScriptProxyBehaviourExample : MonoBehaviour
	{
		private ScriptDomain domain;

		private string sourceCode = "using UnityEngine;class Test : MonoBehaviour{   public void Awake()   {      Debug.Log(\"Hello world - From loaded behaviour 'Awake'\");   }   public void TestMethod()   {       Debug.Log(\"Hello World - From loaded behaviour code\");   }}";

		private void Start()
		{
			bool initCompiler = true;
			domain = ScriptDomain.CreateDomain("ModDomain", initCompiler);
			ScriptType scriptType = domain.CompileAndLoadScriptSource(sourceCode);
			ScriptProxy scriptProxy = scriptType.CreateInstance(base.gameObject);
			scriptProxy.Call("TestMethod");
		}
	}
}
