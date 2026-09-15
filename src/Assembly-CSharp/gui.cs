using UnityEngine;

public class gui : MonoBehaviour
{
	public Material mat;

	public Material mat1;

	private string tname = "DAY";

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(10f, 10f, 150f, 100f), base.name))
		{
			if (tname == "DAY")
			{
				RenderSettings.skybox = new Material(mat);
				tname = "NIGHT";
				GameObject.Find("sun").GetComponent<Light>().intensity = 0.1f;
			}
			else
			{
				RenderSettings.skybox = new Material(mat1);
				tname = "DAY";
				GameObject.Find("sun").GetComponent<Light>().intensity = 0f;
			}
			MonoBehaviour.print(mat);
		}
	}
}
