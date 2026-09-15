using System;
using System.Collections;
using System.Collections.Generic;
using UltimateGameTools.MeshSimplifier;
using UnityEngine;

public class SimplifyMeshPreview : MonoBehaviour
{
	[Serializable]
	public class ShowcaseObject
	{
		public MeshSimplify m_meshSimplify;

		public Vector3 m_position;

		public Vector3 m_angles;

		public Vector3 m_rotationAxis = Vector3.up;

		public string m_description;
	}

	public ShowcaseObject[] ShowcaseObjects;

	public Material WireframeMaterial;

	public float MouseSensitvity = 0.3f;

	public float MouseReleaseSpeed = 3f;

	private Dictionary<GameObject, Material[]> m_objectMaterials;

	private MeshSimplify m_selectedMeshSimplify;

	private int m_nSelectedIndex = -1;

	private bool m_bWireframe;

	private float m_fRotationSpeed = 10f;

	private float m_fLastMouseX;

	private Mesh m_newMesh;

	private int m_nLastProgress = -1;

	private string m_strLastTitle = string.Empty;

	private string m_strLastMessage = string.Empty;

	private float m_fVertexAmount = 1f;

	private bool m_bGUIEnabled = true;

	private void Start()
	{
		if (ShowcaseObjects != null && ShowcaseObjects.Length > 0)
		{
			for (int i = 0; i < ShowcaseObjects.Length; i++)
			{
				ShowcaseObjects[i].m_description = ShowcaseObjects[i].m_description.Replace("\\n", Environment.NewLine);
			}
			SetActiveObject(0);
		}
		Simplifier.CoroutineFrameMiliseconds = 20;
	}

	private void Progress(string strTitle, string strMessage, float fT)
	{
		int num = Mathf.RoundToInt(fT * 100f);
		if (num != m_nLastProgress || m_strLastTitle != strTitle || m_strLastMessage != strMessage)
		{
			m_strLastTitle = strTitle;
			m_strLastMessage = strMessage;
			m_nLastProgress = num;
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F1))
		{
			m_bGUIEnabled = !m_bGUIEnabled;
		}
		if (Input.GetKeyDown(KeyCode.W))
		{
			m_bWireframe = !m_bWireframe;
			SetWireframe(m_bWireframe);
		}
		if (m_selectedMeshSimplify != null)
		{
			if (Input.GetMouseButton(0) && Input.mousePosition.y > 100f)
			{
				Vector3 eulerAngles = ShowcaseObjects[m_nSelectedIndex].m_rotationAxis * (0f - (Input.mousePosition.x - m_fLastMouseX) * MouseSensitvity);
				m_selectedMeshSimplify.transform.Rotate(eulerAngles, Space.Self);
			}
			else if (Input.GetMouseButtonUp(0) && Input.mousePosition.y > 100f)
			{
				m_fRotationSpeed = (0f - (Input.mousePosition.x - m_fLastMouseX)) * MouseReleaseSpeed;
			}
			else
			{
				Vector3 eulerAngles2 = ShowcaseObjects[m_nSelectedIndex].m_rotationAxis * (m_fRotationSpeed * Time.deltaTime);
				m_selectedMeshSimplify.transform.Rotate(eulerAngles2, Space.Self);
			}
		}
		m_fLastMouseX = Input.mousePosition.x;
	}

	private void OnGUI()
	{
		int num = 150;
		int num2 = 50;
		int num3 = 20;
		int num4 = 10;
		Rect rect = new Rect(Screen.width / 2 - num / 2, 0f, num, num2);
		Rect screenRect = new Rect(rect.x + (float)num3, rect.y + (float)num4, num - num3 * 2, num2 - num4 * 2);
		GUILayout.BeginArea(screenRect);
		if (GUILayout.Button("Exit"))
		{
			Application.Quit();
		}
		GUILayout.EndArea();
		if (!m_bGUIEnabled)
		{
			return;
		}
		int num5 = 400;
		if (ShowcaseObjects == null)
		{
			return;
		}
		bool flag = true;
		if (!string.IsNullOrEmpty(m_strLastTitle) && !string.IsNullOrEmpty(m_strLastMessage))
		{
			flag = false;
		}
		GUI.Box(new Rect(0f, 0f, num5 + 10, 400f), string.Empty);
		GUILayout.Label("Select model:", GUILayout.Width(num5));
		GUILayout.BeginHorizontal();
		for (int i = 0; i < ShowcaseObjects.Length; i++)
		{
			if (GUILayout.Button(ShowcaseObjects[i].m_meshSimplify.name) && flag)
			{
				if (m_selectedMeshSimplify != null)
				{
					UnityEngine.Object.DestroyImmediate(m_selectedMeshSimplify.gameObject);
				}
				SetActiveObject(i);
			}
		}
		GUILayout.EndHorizontal();
		if (!(m_selectedMeshSimplify != null))
		{
			return;
		}
		GUILayout.Space(20f);
		GUILayout.Label(ShowcaseObjects[m_nSelectedIndex].m_description);
		GUILayout.Space(20f);
		GUI.changed = false;
		m_bWireframe = GUILayout.Toggle(m_bWireframe, "Show wireframe");
		if (GUI.changed && m_selectedMeshSimplify != null)
		{
			SetWireframe(m_bWireframe);
		}
		GUILayout.Space(20f);
		int simplifiedVertexCount = m_selectedMeshSimplify.GetSimplifiedVertexCount(true);
		int originalVertexCount = m_selectedMeshSimplify.GetOriginalVertexCount(true);
		GUILayout.Label("Vertex count: " + simplifiedVertexCount + "/" + originalVertexCount + " " + Mathf.RoundToInt((float)simplifiedVertexCount / (float)originalVertexCount * 100f).ToString() + "% from original");
		GUILayout.Space(20f);
		if (!string.IsNullOrEmpty(m_strLastTitle) && !string.IsNullOrEmpty(m_strLastMessage))
		{
			GUILayout.Label(m_strLastTitle + ": " + m_strLastMessage, GUILayout.MaxWidth(num5));
			GUI.color = Color.blue;
			Rect lastRect = GUILayoutUtility.GetLastRect();
			GUI.Box(new Rect(10f, lastRect.yMax + 5f, 204f, 24f), string.Empty);
			GUI.Box(new Rect(12f, lastRect.yMax + 7f, m_nLastProgress * 2, 20f), string.Empty);
			return;
		}
		GUILayout.Label("Vertices: " + (m_fVertexAmount * 100f).ToString("0.00") + "%");
		m_fVertexAmount = GUILayout.HorizontalSlider(m_fVertexAmount, 0f, 1f, GUILayout.Width(200f));
		GUILayout.BeginHorizontal();
		GUILayout.Space(3f);
		if (GUILayout.Button("Compute simplified mesh", GUILayout.Width(200f)))
		{
			StartCoroutine(ComputeMeshWithVertices(m_fVertexAmount));
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	private void SetActiveObject(int index)
	{
		m_nSelectedIndex = index;
		MeshSimplify meshSimplify = UnityEngine.Object.Instantiate(ShowcaseObjects[index].m_meshSimplify);
		meshSimplify.transform.position = ShowcaseObjects[index].m_position;
		meshSimplify.transform.rotation = Quaternion.Euler(ShowcaseObjects[index].m_angles);
		m_selectedMeshSimplify = meshSimplify;
		m_objectMaterials = new Dictionary<GameObject, Material[]>();
		AddMaterials(meshSimplify.gameObject, m_objectMaterials);
		m_bWireframe = false;
	}

	private void AddMaterials(GameObject theGameObject, Dictionary<GameObject, Material[]> dicMaterials)
	{
		Renderer component = theGameObject.GetComponent<Renderer>();
		MeshSimplify component2 = theGameObject.GetComponent<MeshSimplify>();
		if (component != null && component.sharedMaterials != null && component2 != null)
		{
			dicMaterials.Add(theGameObject, component.sharedMaterials);
		}
		for (int i = 0; i < theGameObject.transform.childCount; i++)
		{
			AddMaterials(theGameObject.transform.GetChild(i).gameObject, dicMaterials);
		}
	}

	private void SetWireframe(bool bEnabled)
	{
		m_bWireframe = bEnabled;
		foreach (KeyValuePair<GameObject, Material[]> objectMaterial in m_objectMaterials)
		{
			Renderer component = objectMaterial.Key.GetComponent<Renderer>();
			if (bEnabled)
			{
				Material[] array = new Material[objectMaterial.Value.Length];
				for (int i = 0; i < objectMaterial.Value.Length; i++)
				{
					array[i] = WireframeMaterial;
				}
				component.sharedMaterials = array;
			}
			else
			{
				component.sharedMaterials = objectMaterial.Value;
			}
		}
	}

	private IEnumerator ComputeMeshWithVertices(float fAmount)
	{
		foreach (KeyValuePair<GameObject, Material[]> pair in m_objectMaterials)
		{
			MeshSimplify meshSimplify = pair.Key.GetComponent<MeshSimplify>();
			MeshFilter meshFilter = pair.Key.GetComponent<MeshFilter>();
			SkinnedMeshRenderer skin = pair.Key.GetComponent<SkinnedMeshRenderer>();
			if (!meshSimplify || (!(meshFilter != null) && !(skin != null)))
			{
				continue;
			}
			Mesh newMesh = null;
			if (meshFilter != null)
			{
				newMesh = UnityEngine.Object.Instantiate(meshFilter.sharedMesh);
			}
			else if (skin != null)
			{
				newMesh = UnityEngine.Object.Instantiate(skin.sharedMesh);
			}
			if (meshSimplify.GetMeshSimplifier() != null)
			{
				meshSimplify.GetMeshSimplifier().CoroutineEnded = false;
				StartCoroutine(meshSimplify.GetMeshSimplifier().ComputeMeshWithVertexCount(pair.Key, newMesh, Mathf.RoundToInt(fAmount * (float)meshSimplify.GetMeshSimplifier().GetOriginalMeshUniqueVertexCount()), meshSimplify.name, Progress));
				while (!meshSimplify.GetMeshSimplifier().CoroutineEnded)
				{
					yield return null;
				}
				if (meshFilter != null)
				{
					meshFilter.mesh = newMesh;
				}
				else if (skin != null)
				{
					skin.sharedMesh = newMesh;
				}
				meshSimplify.m_simplifiedMesh = newMesh;
			}
		}
		m_strLastTitle = string.Empty;
		m_strLastMessage = string.Empty;
		m_nLastProgress = 0;
	}
}
