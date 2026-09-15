using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class OBJExporter : MonoBehaviour
{
	public bool generateMaterials = true;

	public bool exportTextures = true;

	public bool exportTextureNames;

	private string lastExportFolder;

	public void Export(string exportPath, Mesh mesh, Vector3[] vertices, Vector3[] normals, Material[] mats, Dictionary<int, bool> enabledMats)
	{
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		FileInfo fileInfo = new FileInfo(exportPath);
		lastExportFolder = fileInfo.Directory.FullName;
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(exportPath);
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		if (generateMaterials)
		{
			stringBuilder.AppendLine("mtllib " + fileNameWithoutExtension + ".mtl");
		}
		string text = mesh.name;
		if (generateMaterials)
		{
			for (int i = 0; i < mats.Length; i++)
			{
				if (enabledMats.ContainsKey(i))
				{
					Material material = mats[i];
					if (!dictionary.ContainsKey(material.name))
					{
						dictionary[material.name] = true;
						stringBuilder2.Append(MaterialToString(material));
						stringBuilder2.AppendLine();
					}
				}
			}
		}
		foreach (Vector3 vector in vertices)
		{
			Vector3 vector2 = vector;
			vector2.x *= -1f;
			stringBuilder.AppendLine("v " + vector2.x + " " + vector2.y + " " + vector2.z);
		}
		foreach (Vector3 vector3 in normals)
		{
			Vector3 vector4 = vector3;
			vector4.x *= -1f;
			stringBuilder.AppendLine("vn " + vector4.x + " " + vector4.y + " " + vector4.z);
		}
		Vector2[] uv = mesh.uv;
		for (int l = 0; l < uv.Length; l++)
		{
			Vector2 vector5 = uv[l];
			stringBuilder.AppendLine("vt " + vector5.x + " " + vector5.y);
		}
		for (int m = 0; m < mesh.subMeshCount; m++)
		{
			if (enabledMats.ContainsKey(m))
			{
				if (m < mats.Length)
				{
					string text2 = mats[m].name;
					stringBuilder.AppendLine("usemtl " + text2);
				}
				else
				{
					stringBuilder.AppendLine("usemtl " + text + "_missing" + m);
				}
				int[] triangles = mesh.GetTriangles(m);
				for (int n = 0; n < triangles.Length; n += 3)
				{
					int index = triangles[n] + 1;
					int index2 = triangles[n + 1] + 1;
					int index3 = triangles[n + 2] + 1;
					stringBuilder.AppendLine("f " + ConstructOBJString(index3) + " " + ConstructOBJString(index2) + " " + ConstructOBJString(index));
				}
			}
		}
		File.WriteAllText(exportPath, stringBuilder.ToString());
		if (generateMaterials)
		{
			File.WriteAllText(fileInfo.Directory.FullName + "\\" + fileNameWithoutExtension + ".mtl", stringBuilder2.ToString());
		}
	}

	private string TryExportTexture(string propertyName, Material m)
	{
		if (m.HasProperty(propertyName))
		{
			Texture texture = m.GetTexture(propertyName);
			if (texture != null)
			{
				return ExportTexture((Texture2D)texture);
			}
		}
		return "false";
	}

	private string ExportTexture(Texture2D t)
	{
		try
		{
			string text = lastExportFolder + "\\" + t.name + ".png";
			Texture2D texture2D = new Texture2D(t.width, t.height, TextureFormat.ARGB32, false);
			t.GetRawTextureData();
			texture2D.SetPixels(t.GetPixels());
			File.WriteAllBytes(text, texture2D.EncodeToPNG());
			return text;
		}
		catch (Exception ex)
		{
			Debug.Log("Could not export texture : " + t.name + ": " + ex.Message);
			return "null";
		}
	}

	private string ConstructOBJString(int index)
	{
		string text = index.ToString();
		return text + "/" + text + "/" + text;
	}

	private string MaterialToString(Material m)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("newmtl " + m.name);
		if (m.HasProperty("_Color"))
		{
			stringBuilder.AppendLine("Kd " + m.color.r + " " + m.color.g + " " + m.color.b);
			if (m.color.a < 1f)
			{
				stringBuilder.AppendLine("Tr " + (1f - m.color.a));
				stringBuilder.AppendLine("d " + m.color.a);
			}
		}
		if (m.HasProperty("_SpecColor"))
		{
			Color color = m.GetColor("_SpecColor");
			stringBuilder.AppendLine("Ks " + color.r + " " + color.g + " " + color.b);
		}
		if (exportTextureNames && m.HasProperty("_MainTex"))
		{
			Texture texture = m.GetTexture("_MainTex");
			if (texture != null)
			{
				stringBuilder.AppendLine("map_Kd " + texture.name);
			}
		}
		if (exportTextures)
		{
			string text = TryExportTexture("_MainTex", m);
			if (text != "false")
			{
				stringBuilder.AppendLine("map_Kd " + text);
			}
			text = TryExportTexture("_SpecMap", m);
			if (text != "false")
			{
				stringBuilder.AppendLine("map_Ks " + text);
			}
			text = TryExportTexture("_BumpMap", m);
			if (text != "false")
			{
				stringBuilder.AppendLine("map_Bump " + text);
			}
		}
		stringBuilder.AppendLine("illum 2");
		return stringBuilder.ToString();
	}
}
