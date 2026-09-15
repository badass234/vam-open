using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GPUTools.ClothDemo.Scripts
{
	public class OpenSceneButton : MonoBehaviour
	{
		[SerializeField]
		private string sceneName;

		public void Start()
		{
			GetComponent<Button>().onClick.AddListener(OnClick);
		}

		private void OnClick()
		{
			SceneManager.LoadScene(sceneName);
		}
	}
}
