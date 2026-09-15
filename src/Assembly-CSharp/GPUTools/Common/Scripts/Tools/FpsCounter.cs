using UnityEngine;
using UnityEngine.UI;

namespace GPUTools.Common.Scripts.Tools
{
	public class FpsCounter : MonoBehaviour
	{
		[SerializeField]
		private Text textField;

		private float deltaTime;

		private void Update()
		{
			deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
			float num = deltaTime * 1000f;
			float num2 = 1f / deltaTime;
			textField.text = $"{num:0.0} ms ({num2:0.} fps)";
		}
	}
}
