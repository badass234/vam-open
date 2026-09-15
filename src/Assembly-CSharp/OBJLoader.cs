using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class OBJLoader
{
	private struct OBJFace
	{
		public string materialName;

		public string meshName;

		public int[] indexes;
	}

	public static bool splitByMaterial = false;

	public static string[] searchPaths = new string[2]
	{
		string.Empty,
		"%FileName%_Textures" + Path.DirectorySeparatorChar
	};

	public static Vector3 ParseVectorFromCMPS(string[] cmps)
	{
		float x = float.Parse(cmps[1]);
		float y = float.Parse(cmps[2]);
		if (cmps.Length == 4)
		{
			float z = float.Parse(cmps[3]);
			return new Vector3(x, y, z);
		}
		return new Vector2(x, y);
	}

	public static Color ParseColorFromCMPS(string[] cmps, float scalar = 1f)
	{
		float r = float.Parse(cmps[1]) * scalar;
		float g = float.Parse(cmps[2]) * scalar;
		float b = float.Parse(cmps[3]) * scalar;
		return new Color(r, g, b);
	}

	public static string OBJGetFilePath(string path, string basePath, string fileName)
	{
		string[] array = searchPaths;
		foreach (string text in array)
		{
			string text2 = text.Replace("%FileName%", fileName);
			if (File.Exists(basePath + text2 + path))
			{
				return basePath + text2 + path;
			}
			if (File.Exists(path))
			{
				return path;
			}
		}
		return null;
	}

	public static Material[] LoadMTLFile(string fn)
	{
		Material material = null;
		List<Material> list = new List<Material>();
		FileInfo fileInfo = new FileInfo(fn);
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fn);
		string basePath = fileInfo.Directory.FullName + Path.DirectorySeparatorChar;
		string[] array = File.ReadAllLines(fn);
		foreach (string text in array)
		{
			string text2 = text.Trim().Replace("  ", " ");
			string[] array2 = text2.Split(' ');
			string text3 = text2.Remove(0, text2.IndexOf(' ') + 1);
			if (array2[0] == "newmtl")
			{
				if (material != null)
				{
					list.Add(material);
				}
				material = new Material(Shader.Find("Standard (Specular setup)"));
				material.name = text3;
			}
			else if (array2[0] == "Kd")
			{
				material.SetColor("_Color", ParseColorFromCMPS(array2));
			}
			else if (array2[0] == "map_Kd")
			{
				string text4 = OBJGetFilePath(text3, basePath, fileNameWithoutExtension);
				if (text4 != null)
				{
					material.SetTexture("_MainTex", TextureLoader.LoadTexture(text4));
				}
			}
			else if (array2[0] == "map_Bump")
			{
				string text5 = OBJGetFilePath(text3, basePath, fileNameWithoutExtension);
				if (text5 != null)
				{
					material.SetTexture("_BumpMap", TextureLoader.LoadTexture(text5, true));
					material.EnableKeyword("_NORMALMAP");
				}
			}
			else if (array2[0] == "Ks")
			{
				material.SetColor("_SpecColor", ParseColorFromCMPS(array2));
			}
			else if (array2[0] == "Ka")
			{
				material.SetColor("_EmissionColor", ParseColorFromCMPS(array2, 0.05f));
				material.EnableKeyword("_EMISSION");
			}
			else if (array2[0] == "d")
			{
				float num = float.Parse(array2[1]);
				if (num < 1f)
				{
					Color color = material.color;
					color.a = num;
					material.SetColor("_Color", color);
					material.SetFloat("_Mode", 3f);
					material.SetInt("_SrcBlend", 5);
					material.SetInt("_DstBlend", 10);
					material.SetInt("_ZWrite", 0);
					material.DisableKeyword("_ALPHATEST_ON");
					material.EnableKeyword("_ALPHABLEND_ON");
					material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
					material.renderQueue = 3000;
				}
			}
			else if (array2[0] == "Ns")
			{
				float num2 = float.Parse(array2[1]);
				num2 /= 1000f;
				material.SetFloat("_Glossiness", num2);
			}
		}
		if (material != null)
		{
			list.Add(material);
		}
		return list.ToArray();
	}

	public static GameObject LoadOBJFile(string fn)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fn);
		bool flag = false;
		List<Vector3> list = new List<Vector3>();
		List<Vector3> list2 = new List<Vector3>();
		List<Vector2> list3 = new List<Vector2>();
		List<Vector3> list4 = new List<Vector3>();
		List<Vector3> list5 = new List<Vector3>();
		List<Vector2> list6 = new List<Vector2>();
		List<string> list7 = new List<string>();
		List<string> list8 = new List<string>();
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		List<OBJFace> list9 = new List<OBJFace>();
		string text = string.Empty;
		string text2 = "default";
		Material[] array = null;
		FileInfo fileInfo = new FileInfo(fn);
		string[] array2 = File.ReadAllLines(fn);
		foreach (string text3 in array2)
		{
			if (text3.Length <= 0 || text3[0] == '#')
			{
				continue;
			}
			string text4 = text3.Trim().Replace("  ", " ");
			string[] array3 = text4.Split(' ');
			string text5 = text4.Remove(0, text4.IndexOf(' ') + 1);
			if (array3[0] == "mtllib")
			{
				string text6 = OBJGetFilePath(text5, fileInfo.Directory.FullName + Path.DirectorySeparatorChar, fileNameWithoutExtension);
				if (text6 != null)
				{
					array = LoadMTLFile(text6);
				}
			}
			else if ((array3[0] == "g" || array3[0] == "o") && !splitByMaterial)
			{
				text2 = text5;
				if (!list8.Contains(text2))
				{
					list8.Add(text2);
				}
			}
			else if (array3[0] == "usemtl")
			{
				text = text5;
				if (!list7.Contains(text))
				{
					list7.Add(text);
				}
				if (splitByMaterial && !list8.Contains(text))
				{
					list8.Add(text);
				}
			}
			else if (array3[0] == "v")
			{
				list.Add(ParseVectorFromCMPS(array3));
			}
			else if (array3[0] == "vn")
			{
				list2.Add(ParseVectorFromCMPS(array3));
			}
			else if (array3[0] == "vt")
			{
				list3.Add(ParseVectorFromCMPS(array3));
			}
			else
			{
				if (!(array3[0] == "f"))
				{
					continue;
				}
				int[] array4 = new int[array3.Length - 1];
				for (int j = 1; j < array3.Length; j++)
				{
					string text7 = array3[j];
					int num = -1;
					int num2 = -1;
					int num3 = -1;
					if (text7.Contains("//"))
					{
						string[] array5 = text7.Split('/');
						num = int.Parse(array5[0]) - 1;
						num2 = int.Parse(array5[2]) - 1;
					}
					else if (text7.Count((char x) => x == '/') == 2)
					{
						string[] array6 = text7.Split('/');
						num = int.Parse(array6[0]) - 1;
						num3 = int.Parse(array6[1]) - 1;
						num2 = int.Parse(array6[2]) - 1;
					}
					else if (!text7.Contains("/"))
					{
						num = int.Parse(text7) - 1;
					}
					else
					{
						string[] array7 = text7.Split('/');
						num = int.Parse(array7[0]) - 1;
						num3 = int.Parse(array7[1]) - 1;
					}
					string key = num + "|" + num2 + "|" + num3;
					if (dictionary.ContainsKey(key))
					{
						array4[j - 1] = dictionary[key];
						continue;
					}
					array4[j - 1] = dictionary.Count;
					dictionary[key] = dictionary.Count;
					list4.Add(list[num]);
					if (num2 < 0 || num2 > list2.Count - 1)
					{
						list5.Add(Vector3.zero);
					}
					else
					{
						flag = true;
						list5.Add(list2[num2]);
					}
					if (num3 < 0 || num3 > list3.Count - 1)
					{
						list6.Add(Vector2.zero);
					}
					else
					{
						list6.Add(list3[num3]);
					}
				}
				if (array4.Length < 5 && array4.Length >= 3)
				{
					OBJFace item = default(OBJFace);
					item.materialName = text;
					item.indexes = new int[3]
					{
						array4[0],
						array4[1],
						array4[2]
					};
					item.meshName = ((!splitByMaterial) ? text2 : text);
					list9.Add(item);
					if (array4.Length > 3)
					{
						OBJFace item2 = default(OBJFace);
						item2.materialName = text;
						item2.meshName = ((!splitByMaterial) ? text2 : text);
						item2.indexes = new int[3]
						{
							array4[2],
							array4[3],
							array4[0]
						};
						list9.Add(item2);
					}
				}
			}
		}
		if (list8.Count == 0)
		{
			list8.Add("default");
		}
		GameObject gameObject = new GameObject(fileNameWithoutExtension);
		foreach (string obj in list8)
		{
			GameObject gameObject2 = new GameObject(obj);
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.transform.localScale = new Vector3(-1f, 1f, 1f);
			Mesh mesh = new Mesh();
			mesh.name = obj;
			List<Vector3> list10 = new List<Vector3>();
			List<Vector3> list11 = new List<Vector3>();
			List<Vector2> list12 = new List<Vector2>();
			List<int[]> list13 = new List<int[]>();
			Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
			List<string> meshMaterialNames = new List<string>();
			OBJFace[] source = list9.Where((OBJFace x) => x.meshName == obj).ToArray();
			foreach (string mn in list7)
			{
				OBJFace[] array8 = source.Where((OBJFace x) => x.materialName == mn).ToArray();
				if (array8.Length <= 0)
				{
					continue;
				}
				int[] array9 = new int[0];
				OBJFace[] array10 = array8;
				for (int k = 0; k < array10.Length; k++)
				{
					OBJFace oBJFace = array10[k];
					int num4 = array9.Length;
					Array.Resize(ref array9, num4 + oBJFace.indexes.Length);
					Array.Copy(oBJFace.indexes, 0, array9, num4, oBJFace.indexes.Length);
				}
				meshMaterialNames.Add(mn);
				if (mesh.subMeshCount != meshMaterialNames.Count)
				{
					mesh.subMeshCount = meshMaterialNames.Count;
				}
				for (int l = 0; l < array9.Length; l++)
				{
					int num5 = array9[l];
					if (dictionary2.ContainsKey(num5))
					{
						array9[l] = dictionary2[num5];
						continue;
					}
					list10.Add(list4[num5]);
					list11.Add(list5[num5]);
					list12.Add(list6[num5]);
					dictionary2[num5] = list10.Count - 1;
					array9[l] = dictionary2[num5];
				}
				list13.Add(array9);
			}
			mesh.vertices = list10.ToArray();
			mesh.normals = list11.ToArray();
			mesh.uv = list12.ToArray();
			for (int m = 0; m < list13.Count; m++)
			{
				mesh.SetTriangles(list13[m], m);
			}
			if (!flag)
			{
				mesh.RecalculateNormals();
			}
			mesh.RecalculateBounds();
			MeshFilter meshFilter = gameObject2.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = gameObject2.AddComponent<MeshRenderer>();
			Material[] array11 = new Material[meshMaterialNames.Count];
			for (int n = 0; n < meshMaterialNames.Count; n++)
			{
				if (array == null)
				{
					array11[n] = new Material(Shader.Find("Standard (Specular setup)"));
				}
				else
				{
					Material material = Array.Find(array, (Material x) => x.name == meshMaterialNames[n]);
					if (material == null)
					{
						array11[n] = new Material(Shader.Find("Standard (Specular setup)"));
					}
					else
					{
						array11[n] = material;
					}
				}
				array11[n].name = meshMaterialNames[n];
			}
			meshRenderer.materials = array11;
			meshFilter.mesh = mesh;
		}
		return gameObject;
	}
}
