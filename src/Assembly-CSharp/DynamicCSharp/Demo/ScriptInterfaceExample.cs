using UnityEngine;

namespace DynamicCSharp.Demo
{
	public class ScriptInterfaceExample : MonoBehaviour
	{
		private ScriptDomain domain;

		private string sourceCode = "using UnityEngine;using DynamicCSharp.Demo;class Test : IExampleInterface{   public void SayHello()   {       Debug.Log(\"Hello - From loaded code\");   }   public void SayGoodbye()   {       Debug.Log(\"Goodbye - From loaded code\");   }}";

		private void Start()
		{
			bool initCompiler = true;
			domain = ScriptDomain.CreateDomain("ModDomain", initCompiler);
			ScriptType scriptType = domain.CompileAndLoadScriptSource(sourceCode);
			ScriptProxy scriptProxy = scriptType.CreateInstance();
			IExampleInterface instanceAs = scriptProxy.GetInstanceAs<IExampleInterface>(true);
			instanceAs.SayHello();
			instanceAs.SayGoodbye();
		}
	}
}
