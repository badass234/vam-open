using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentMaterial : PersistentObject
	{
		public RTShaderPropertyType[] m_propertyTypes;

		public string[] m_propertyNames;

		public DataContract[] m_propertyValues;

		public string[] m_keywords;

		public long shader;

		public Color color;

		public long mainTexture;

		public Vector2 mainTextureOffset;

		public Vector2 mainTextureScale;

		public int renderQueue;

		public string[] shaderKeywords;

		public uint globalIlluminationFlags;

		public bool enableInstancing;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			Material material = (Material)obj;
			material.shader = (Shader)objects.Get(shader);
			if (material.HasProperty("_Color"))
			{
				material.color = color;
			}
			if (material.HasProperty("_MainTex"))
			{
				material.mainTexture = (Texture)objects.Get(mainTexture);
				material.mainTextureOffset = mainTextureOffset;
				material.mainTextureScale = mainTextureScale;
			}
			material.renderQueue = renderQueue;
			material.shaderKeywords = shaderKeywords;
			material.globalIlluminationFlags = (MaterialGlobalIlluminationFlags)globalIlluminationFlags;
			material.enableInstancing = enableInstancing;
			if (m_keywords != null)
			{
				string[] keywords = m_keywords;
				foreach (string keyword in keywords)
				{
					material.EnableKeyword(keyword);
				}
			}
			if (m_propertyNames != null)
			{
				for (int j = 0; j < m_propertyNames.Length; j++)
				{
					string text = m_propertyNames[j];
					switch (m_propertyTypes[j])
					{
					case RTShaderPropertyType.Color:
						if (m_propertyValues[j].AsPrimitive.ValueBase is Color)
						{
							material.SetColor(text, (Color)m_propertyValues[j].AsPrimitive.ValueBase);
						}
						break;
					case RTShaderPropertyType.Float:
						if (m_propertyValues[j].AsPrimitive.ValueBase is float)
						{
							material.SetFloat(text, (float)m_propertyValues[j].AsPrimitive.ValueBase);
						}
						break;
					case RTShaderPropertyType.Range:
						if (m_propertyValues[j].AsPrimitive.ValueBase is float)
						{
							material.SetFloat(text, (float)m_propertyValues[j].AsPrimitive.ValueBase);
						}
						break;
					case RTShaderPropertyType.TexEnv:
						if (m_propertyValues[j].AsPrimitive.ValueBase is long)
						{
							material.SetTexture(text, objects.Get((long)m_propertyValues[j].AsPrimitive.ValueBase) as Texture);
						}
						break;
					case RTShaderPropertyType.Vector:
						if (m_propertyValues[j].AsPrimitive.ValueBase is Vector4)
						{
							material.SetVector(text, (Vector4)m_propertyValues[j].AsPrimitive.ValueBase);
						}
						break;
					}
				}
			}
			return material;
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
			AddDependency(shader, dependencies, objects, allowNulls);
			AddDependency(mainTexture, dependencies, objects, allowNulls);
			if (m_propertyValues == null)
			{
				return;
			}
			for (int i = 0; i < m_propertyValues.Length; i++)
			{
				RTShaderPropertyType rTShaderPropertyType = m_propertyTypes[i];
				if (rTShaderPropertyType == RTShaderPropertyType.TexEnv && m_propertyValues[i].AsPrimitive.ValueBase is long)
				{
					AddDependency((long)m_propertyValues[i].AsPrimitive.ValueBase, dependencies, objects, allowNulls);
				}
			}
		}

		protected override void GetDependencies(Dictionary<long, UnityEngine.Object> dependencies, object obj)
		{
			base.GetDependencies(dependencies, obj);
			if (obj == null)
			{
				return;
			}
			Material material = (Material)obj;
			AddDependency(material.shader, dependencies);
			if (material.HasProperty("_MainTex"))
			{
				AddDependency(material.mainTexture, dependencies);
			}
			RuntimeShaderInfo runtimeShaderInfo = null;
			IRuntimeShaderUtil shaderUtil = Dependencies.ShaderUtil;
			if (shaderUtil != null)
			{
				runtimeShaderInfo = shaderUtil.GetShaderInfo(material.shader);
			}
			if (runtimeShaderInfo == null)
			{
				return;
			}
			for (int i = 0; i < runtimeShaderInfo.PropertyCount; i++)
			{
				string text = runtimeShaderInfo.PropertyNames[i];
				RTShaderPropertyType rTShaderPropertyType = runtimeShaderInfo.PropertyTypes[i];
				if (rTShaderPropertyType == RTShaderPropertyType.TexEnv)
				{
					AddDependency(material.GetTexture(text), dependencies);
				}
			}
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj == null)
			{
				return;
			}
			Material material = (Material)obj;
			shader = material.shader.GetMappedInstanceID();
			if (material.HasProperty("_Color"))
			{
				color = material.color;
			}
			if (material.HasProperty("_MainTex"))
			{
				mainTexture = material.mainTexture.GetMappedInstanceID();
				mainTextureOffset = material.mainTextureOffset;
				mainTextureScale = material.mainTextureScale;
			}
			renderQueue = material.renderQueue;
			shaderKeywords = material.shaderKeywords;
			globalIlluminationFlags = (uint)material.globalIlluminationFlags;
			enableInstancing = material.enableInstancing;
			if (material.shader == null)
			{
				return;
			}
			RuntimeShaderInfo runtimeShaderInfo = null;
			IRuntimeShaderUtil shaderUtil = Dependencies.ShaderUtil;
			if (shaderUtil != null)
			{
				runtimeShaderInfo = shaderUtil.GetShaderInfo(material.shader);
			}
			if (runtimeShaderInfo == null)
			{
				return;
			}
			m_propertyNames = new string[runtimeShaderInfo.PropertyCount];
			m_propertyTypes = new RTShaderPropertyType[runtimeShaderInfo.PropertyCount];
			m_propertyValues = new DataContract[runtimeShaderInfo.PropertyCount];
			for (int i = 0; i < runtimeShaderInfo.PropertyCount; i++)
			{
				string text = runtimeShaderInfo.PropertyNames[i];
				RTShaderPropertyType rTShaderPropertyType = runtimeShaderInfo.PropertyTypes[i];
				m_propertyNames[i] = text;
				m_propertyTypes[i] = rTShaderPropertyType;
				switch (rTShaderPropertyType)
				{
				case RTShaderPropertyType.Color:
					m_propertyValues[i] = new DataContract(PrimitiveContract.Create(material.GetColor(text)));
					break;
				case RTShaderPropertyType.Float:
					m_propertyValues[i] = new DataContract(PrimitiveContract.Create(material.GetFloat(text)));
					break;
				case RTShaderPropertyType.Range:
					m_propertyValues[i] = new DataContract(PrimitiveContract.Create(material.GetFloat(text)));
					break;
				case RTShaderPropertyType.TexEnv:
					m_propertyValues[i] = new DataContract(PrimitiveContract.Create(material.GetTexture(text).GetMappedInstanceID()));
					break;
				case RTShaderPropertyType.Vector:
					m_propertyValues[i] = new DataContract(PrimitiveContract.Create(material.GetVector(text)));
					break;
				case RTShaderPropertyType.Unknown:
					m_propertyValues[i] = null;
					break;
				}
			}
			m_keywords = material.shaderKeywords;
		}
	}
}
