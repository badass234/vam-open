using UnityEngine;

namespace DynamicCSharp.Demo
{
	public class LoadAssemblyExample : MonoBehaviour
	{
		private ScriptDomain domain;

		private void Start()
		{
			bool initCompiler = true;
			domain = ScriptDomain.CreateDomain("ModDomain", initCompiler);
			ScriptAssembly scriptAssembly = domain.LoadAssembly("ModAssembly.dll");
			ScriptType[] array = scriptAssembly.FindAllTypes();
			foreach (ScriptType scriptType in array)
			{
				Debug.Log(scriptType.ToString());
			}
		}
	}
}
