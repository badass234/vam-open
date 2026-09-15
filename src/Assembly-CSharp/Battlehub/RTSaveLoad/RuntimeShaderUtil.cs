using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	public class RuntimeShaderUtil : IRuntimeShaderUtil
	{
		private static readonly Dictionary<string, TextAsset[]> m_textAssets = new Dictionary<string, TextAsset[]>();

		private const string Path = "/Battlehub/RTSaveLoad/ShaderInfo";

		public static string GetPath(bool resourcesFolder)
		{
			return "/Battlehub/RTSaveLoad/ShaderInfo" + ((!resourcesFolder) ? string.Empty : "/Resources");
		}

		public static long FileNameToInstanceID(string fileName)
		{
			int num = fileName.LastIndexOf("_");
			if (num == -1)
			{
				return 0L;
			}
			long result;
			if (long.TryParse(fileName.Substring(num + 1).Replace(".txt", string.Empty), out result))
			{
				return result;
			}
			return 0L;
		}

		public static string GetShaderInfoFileName(Shader shader, bool withoutExtension = false)
		{
			return string.Format("rt_shader_{0}_{1}" + ((!withoutExtension) ? ".txt" : string.Empty), shader.name.Replace("/", "__"), shader.GetMappedInstanceID().ToString());
		}

		public static void AddExtra(string key, TextAsset[] textAssets)
		{
			if (!m_textAssets.ContainsKey(key))
			{
				m_textAssets.Add(key, textAssets);
			}
		}

		public static void RemoveExtra(string key)
		{
			m_textAssets.Remove(key);
		}

		public RuntimeShaderInfo GetShaderInfo(Shader shader)
		{
			if (shader == null)
			{
				throw new ArgumentNullException("shader");
			}
			string shaderName = GetShaderInfoFileName(shader, true);
			TextAsset textAsset = Resources.Load<TextAsset>(shaderName);
			if (textAsset == null)
			{
				foreach (TextAsset[] value in m_textAssets.Values)
				{
					textAsset = value.Where((TextAsset t) => t.name == shaderName).FirstOrDefault();
					if (textAsset != null)
					{
						break;
					}
				}
			}
			if (textAsset == null)
			{
				Debug.LogFormat("Shader {0} is not found", shaderName);
				return null;
			}
			return JsonUtility.FromJson<RuntimeShaderInfo>(textAsset.text);
		}
	}
}
