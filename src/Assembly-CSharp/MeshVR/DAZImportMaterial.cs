using System.IO;
using System.Text.RegularExpressions;
using SimpleJSON;
using UnityEngine;

namespace MeshVR
{
	public class DAZImportMaterial
	{
		public string name;

		public Vector2 uvScale;

		public Vector2 uvOffset;

		public bool hasDiffuseMap;

		public bool ignoreDiffuseColor;

		public Color diffuseColor = Color.white;

		public float diffuseStrength = 1f;

		public string diffuseTexturePath;

		public Texture2D diffuseTexture;

		public bool isTransparent;

		public bool hasAlphaMap;

		public float alphaStrength = 1f;

		public string alphaTexturePath;

		public Texture2D alphaTexture;

		public bool useSpecularAsGlossMap;

		public bool hasSpecularColorMap;

		public bool ignoreSpecularColor;

		public Color specularColor = Color.white;

		public float fresnelStrength = 0.5f;

		public string specularColorTexturePath;

		public Texture2D specularColorTexture;

		public bool hasSpecularStrengthMap;

		public float specularStrength = 1f;

		public string specularStrengthTexturePath;

		public Texture2D specularStrengthTexture;

		public bool hasGlossMap;

		public float gloss = 0.5f;

		public float glossWeight = 1f;

		public string glossTexturePath;

		public string roughnessTexturePath;

		public Texture2D glossTexture;

		public bool copyBumpAsSpecularColorMap;

		public bool forceBumpAsNormalMap;

		public bool hasBumpMap;

		public float bumpStrength = 1f;

		public float bumpiness = 1f;

		public string bumpTexturePath;

		public Texture2D bumpTexture;

		public bool hasNormalMap;

		public string normalTexturePath;

		public Texture2D normalTexture;

		public bool hasSubsurfaceColorMap;

		public Color subsurfaceColor = Color.white;

		public string subsurfaceColorTexturePath;

		public Texture2D subsurfaceColorTexture;

		public bool hasSubsurfaceStrengthMap;

		public float subsurfaceStrength;

		public string subsurfaceStrengthTexturePath;

		public Texture2D subsurfaceStrengthTexture;

		public bool hasTranslucencyColorMap;

		public Color translucencyColor = Color.white;

		public string translucencyColorTexturePath;

		public Texture2D translucencyColorTexture;

		public bool hasTranslucencyStrengthMap;

		public float translucencyStrength;

		public string translucencyStrengthTexturePath;

		public Texture2D translucencyStrengthTexture;

		public bool hasReflectionColor;

		public float reflectionStrength;

		public string standardShader;

		public string glossShader;

		public string normalMapShader;

		public string transparentShader;

		public string reflTransparentShader;

		public string transparentNormalMapShader;

		protected Color ProcessColorNode(JSONNode colorNode)
		{
			float asFloat = colorNode[0].AsFloat;
			float asFloat2 = colorNode[1].AsFloat;
			float asFloat3 = colorNode[2].AsFloat;
			return new Color(asFloat, asFloat2, asFloat3);
		}

		protected Texture2D CopyAndImportImage(DAZImport importObject, string MaterialFolder, string path, bool isNormalMap = false, bool isTransparency = false, bool isBumpMap = false, float bumpStrength = 1f, bool isGlossMap = false, bool forceLinear = false, string invertWithSuffix = null, string addSuffix = "")
		{
			if (path == "null")
			{
				return null;
			}
			path = DAZImport.DAZurlFix(path);
			string text = Regex.Replace(path, ".*/", string.Empty);
			string text2;
			if (addSuffix == string.Empty)
			{
				text2 = MaterialFolder + "/" + text;
			}
			else
			{
				string extension = Path.GetExtension(text);
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
				text2 = MaterialFolder + "/" + fileNameWithoutExtension + addSuffix + extension;
			}
			Texture2D texture2D = null;
			bool flag = invertWithSuffix != null;
			string path2;
			if (flag && !Application.isPlaying)
			{
				string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(text2);
				path2 = MaterialFolder + "/" + fileNameWithoutExtension2 + invertWithSuffix + ".png";
			}
			else
			{
				path2 = text2;
			}
			if (!File.Exists(text2) || !File.Exists(path2))
			{
				string text3 = importObject.DetermineFilePath(path);
				if (!File.Exists(text3))
				{
					Debug.LogError("Could not find referenced texture " + path + " in DAZ library. Skipping");
					return null;
				}
				File.Copy(text3, text2);
			}
			if (Application.isPlaying && ImageLoaderThreaded.singleton != null)
			{
				ImageLoaderThreaded.QueuedImage queuedImage = new ImageLoaderThreaded.QueuedImage();
				queuedImage.imgPath = text2;
				queuedImage.createMipMaps = true;
				queuedImage.isNormalMap = isNormalMap;
				queuedImage.isThumbnail = false;
				queuedImage.linear = isNormalMap || isBumpMap || isGlossMap || forceLinear;
				queuedImage.createNormalFromBump = isBumpMap;
				queuedImage.bumpStrength = 0.5f * bumpStrength;
				queuedImage.createAlphaFromGrayscale = isTransparency;
				queuedImage.compress = !isNormalMap && !isBumpMap;
				queuedImage.invert = flag;
				ImageLoaderThreaded.singleton.ProcessImageImmediate(queuedImage);
				if (isBumpMap && queuedImage.tex != null)
				{
					text2 = Regex.Replace(text2, "\\.[a-zA-Z]+$", "GenNM.png");
					byte[] bytes = queuedImage.tex.EncodeToPNG();
					File.WriteAllBytes(text2, bytes);
				}
				if (queuedImage.hadError)
				{
					SuperController.LogError("Error during process of image " + queuedImage.imgPath + ": " + queuedImage.errorText);
				}
				else
				{
					texture2D = queuedImage.tex;
					if (texture2D != null)
					{
						texture2D.name = text2;
						importObject.SetTextureSourcePath(texture2D, text2);
					}
					else
					{
						SuperController.LogError("Unexpected null texture from " + queuedImage.imgPath);
					}
				}
			}
			return texture2D;
		}

		public void ProcessJSON(JSONNode sm)
		{
			JSONNode jSONNode = sm["diffuse"]["channel"];
			if (jSONNode != null)
			{
				if (jSONNode["current_value"] != null && !ignoreDiffuseColor)
				{
					diffuseColor = ProcessColorNode(jSONNode["current_value"]);
				}
				if (jSONNode["image_file"] != null)
				{
					diffuseTexturePath = jSONNode["image_file"];
					hasDiffuseMap = true;
				}
			}
			jSONNode = sm["diffuse_strength"]["channel"];
			if (jSONNode != null)
			{
				if (jSONNode["current_value"] != null)
				{
					diffuseStrength = jSONNode["current_value"].AsFloat;
				}
				if (jSONNode["image_file"] != null)
				{
					Debug.LogError("Don't support diffuse_strength texture map");
				}
			}
			jSONNode = sm["transparency"]["channel"];
			if (jSONNode != null)
			{
				if (jSONNode["current_value"] != null)
				{
					alphaStrength = jSONNode["current_value"].AsFloat;
					if (alphaStrength != 1f)
					{
						isTransparent = true;
					}
				}
				if (jSONNode["image_file"] != null)
				{
					alphaTexturePath = jSONNode["image_file"];
					hasAlphaMap = true;
					isTransparent = true;
				}
			}
			jSONNode = sm["specular"]["channel"];
			if (jSONNode != null)
			{
				if (jSONNode["current_value"] != null && !ignoreSpecularColor)
				{
					specularColor = ProcessColorNode(jSONNode["current_value"]);
				}
				if (jSONNode["image_file"] != null)
				{
					if (useSpecularAsGlossMap)
					{
						glossTexturePath = jSONNode["image_file"];
						hasGlossMap = true;
					}
					specularColorTexturePath = jSONNode["image_file"];
					hasSpecularColorMap = true;
				}
			}
			jSONNode = sm["specular_strength"]["channel"];
			if (jSONNode != null)
			{
				if (jSONNode["current_value"] != null)
				{
					specularStrength = jSONNode["current_value"].AsFloat;
				}
				if (jSONNode["image_file"] != null)
				{
					if (useSpecularAsGlossMap)
					{
						glossTexturePath = jSONNode["image_file"];
						hasGlossMap = true;
					}
					specularStrengthTexturePath = jSONNode["image_file"];
					hasSpecularStrengthMap = true;
				}
			}
			jSONNode = sm["glossiness"]["channel"];
			if (jSONNode != null)
			{
				if (jSONNode["current_value"] != null)
				{
					gloss = jSONNode["current_value"].AsFloat;
				}
				if (jSONNode["image_file"] != null)
				{
					glossTexturePath = jSONNode["image_file"];
					hasGlossMap = true;
				}
			}
			jSONNode = sm["bump"]["channel"];
			if (jSONNode != null)
			{
				if (jSONNode["current_value"] != null)
				{
					bumpStrength = jSONNode["current_value"].AsFloat;
				}
				if (jSONNode["image_file"] != null)
				{
					bumpTexturePath = jSONNode["image_file"];
					hasBumpMap = true;
				}
			}
			jSONNode = sm["normal"]["channel"];
			if (jSONNode != null && jSONNode["image_file"] != null && (!hasBumpMap || !forceBumpAsNormalMap))
			{
				normalTexturePath = jSONNode["image_file"];
				hasNormalMap = true;
			}
			JSONNode jSONNode2 = sm["u_scale"]["channel"]["current_value"];
			uvScale.x = 1f;
			if (jSONNode2 != null)
			{
				uvScale.x = jSONNode2.AsFloat;
			}
			JSONNode jSONNode3 = sm["v_scale"]["channel"]["current_value"];
			uvScale.y = 1f;
			if (jSONNode3 != null)
			{
				uvScale.y = jSONNode3.AsFloat;
			}
			JSONNode jSONNode4 = sm["u_offset"]["channel"]["current_value"];
			uvOffset.x = 0f;
			if (jSONNode4 != null)
			{
				uvOffset.x = jSONNode4.AsFloat;
			}
			JSONNode jSONNode5 = sm["v_offset"]["channel"]["current_value"];
			uvOffset.y = 0f;
			if (jSONNode5 != null)
			{
				uvOffset.y = jSONNode5.AsFloat;
			}
			JSONNode jSONNode6 = null;
			if (sm["extra"] != null)
			{
				foreach (JSONNode item in sm["extra"].AsArray)
				{
					string text = item["type"];
					if (text == "studio_material_channels")
					{
						jSONNode6 = item["channels"];
						if (jSONNode6 != null)
						{
							break;
						}
					}
				}
			}
			if (!(jSONNode6 != null))
			{
				return;
			}
			foreach (JSONNode item2 in jSONNode6.AsArray)
			{
				string text2 = item2["channel"]["id"];
				JSONNode jSONNode9 = item2["channel"]["visible"];
				if (text2 == null || (jSONNode9 != null && !jSONNode9.AsBool))
				{
					continue;
				}
				switch (text2)
				{
				case "Diffuse Color":
					jSONNode = item2["channel"];
					if (jSONNode["current_value"] != null && !ignoreDiffuseColor)
					{
						diffuseColor = ProcessColorNode(jSONNode["current_value"]);
					}
					if (jSONNode["image_file"] != null)
					{
						diffuseTexturePath = jSONNode["image_file"];
						hasDiffuseMap = true;
					}
					break;
				case "Diffuse Strength":
					jSONNode = item2["channel"];
					if (jSONNode["current_value"] != null)
					{
						diffuseStrength = jSONNode["current_value"].AsFloat;
					}
					if (jSONNode["image_file"] != null)
					{
						Debug.LogError("Don't currently support Diffuse Strength texture map");
					}
					break;
				case "Cutout Opacity":
					jSONNode = item2["channel"];
					if (jSONNode["current_value"] != null && jSONNode["current_value"].AsFloat != 1f)
					{
						isTransparent = true;
					}
					if (jSONNode["image_file"] != null)
					{
						alphaTexturePath = jSONNode["image_file"];
						hasAlphaMap = true;
						isTransparent = true;
					}
					break;
				case "Refraction Weight":
					jSONNode = item2["channel"];
					if (jSONNode["current_value"] != null && jSONNode["current_value"].AsFloat > 0f)
					{
						isTransparent = true;
					}
					break;
				case "Specular Color":
					jSONNode = item2["channel"];
					if (jSONNode["current_value"] != null && !ignoreSpecularColor)
					{
						specularColor = ProcessColorNode(jSONNode["current_value"]);
					}
					if (jSONNode["image_file"] != null)
					{
						if (useSpecularAsGlossMap)
						{
							glossTexturePath = jSONNode["image_file"];
							hasGlossMap = true;
						}
						specularColorTexturePath = jSONNode["image_file"];
						hasSpecularColorMap = true;
					}
					break;
				case "Glossy Color":
					jSONNode = item2["channel"];
					if (jSONNode["current_value"] != null && !ignoreSpecularColor)
					{
						specularColor = ProcessColorNode(jSONNode["current_value"]);
					}
					if (jSONNode["image_file"] != null)
					{
						if (useSpecularAsGlossMap)
						{
							glossTexturePath = jSONNode["image_file"];
							hasGlossMap = true;
						}
						specularColorTexturePath = jSONNode["image_file"];
						hasSpecularColorMap = true;
					}
					break;
				case "Glossy Layered Weight":
				case "Glossy Weight":
					jSONNode = item2["channel"];
					if (jSONNode["current_value"] != null)
					{
						glossWeight = jSONNode["current_value"].AsFloat;
					}
					if (jSONNode["image_file"] != null)
					{
						glossTexturePath = jSONNode["image_file"];
						hasGlossMap = true;
					}
					break;
				case "Glossiness":
					jSONNode = item2["channel"];
					if (jSONNode["current_value"] != null)
					{
						gloss = jSONNode["current_value"].AsFloat;
					}
					if (jSONNode["image_file"] != null)
					{
						glossTexturePath = jSONNode["image_file"];
						hasGlossMap = true;
					}
					break;
				case "Glossy Roughness":
					jSONNode = item2["channel"];
					if (jSONNode["current_value"] != null)
					{
						glossWeight = jSONNode["current_value"].AsFloat;
					}
					if (jSONNode["image_file"] != null)
					{
						roughnessTexturePath = jSONNode["image_file"];
						glossWeight = 1f;
						hasGlossMap = true;
					}
					break;
				case "Bump Strength":
					jSONNode = item2["channel"];
					if (jSONNode["current_value"] != null)
					{
						bumpStrength = jSONNode["current_value"].AsFloat;
					}
					if (jSONNode["image_file"] != null)
					{
						bumpTexturePath = jSONNode["image_file"];
						hasBumpMap = true;
					}
					break;
				case "Normal Map":
					jSONNode = item2["channel"];
					if (jSONNode["image_file"] != null && (!hasBumpMap || !forceBumpAsNormalMap))
					{
						normalTexturePath = jSONNode["image_file"];
						hasNormalMap = true;
					}
					break;
				case "Subsurface Color":
					jSONNode = item2["channel"];
					if (jSONNode["current_value"] != null)
					{
					}
					if (jSONNode["image_file"] != null)
					{
						subsurfaceColorTexturePath = jSONNode["image_file"];
						hasSubsurfaceColorMap = true;
					}
					break;
				case "Subsurface Strength":
					jSONNode = item2["channel"];
					if (jSONNode["current_value"] != null)
					{
						subsurfaceStrength = jSONNode["current_value"].AsFloat;
					}
					if (jSONNode["image_file"] != null)
					{
						subsurfaceStrengthTexturePath = jSONNode["image_file"];
						hasSubsurfaceStrengthMap = true;
					}
					break;
				case "Reflection Color":
					hasReflectionColor = true;
					break;
				case "Reflection Strength":
					jSONNode = item2["channel"];
					if (jSONNode["current_value"] != null)
					{
						reflectionStrength = jSONNode["current_value"].AsFloat;
					}
					break;
				case "Translucency Color":
					jSONNode = item2["channel"];
					if (jSONNode["current_value"] != null)
					{
						translucencyColor = ProcessColorNode(jSONNode["current_value"]);
					}
					if (jSONNode["image_file"] != null)
					{
						translucencyColorTexturePath = jSONNode["image_file"];
						hasTranslucencyColorMap = true;
					}
					break;
				case "Translucency Strength":
					jSONNode = item2["channel"];
					if (jSONNode["current_value"] != null)
					{
						translucencyStrength = jSONNode["current_value"].AsFloat;
					}
					if (jSONNode["image_file"] != null)
					{
						translucencyStrengthTexturePath = jSONNode["image_file"];
						hasTranslucencyStrengthMap = true;
					}
					break;
				}
			}
		}

		public void Report()
		{
			string text = "Material " + name + " import report:";
			if (hasDiffuseMap)
			{
				text = text + " Has Diffuse Map " + diffuseTexturePath;
			}
			if (hasAlphaMap)
			{
				text = text + " Has Alpha Map " + alphaTexturePath;
			}
			if (hasSpecularColorMap)
			{
				text = text + " Has Specular Color Map " + specularColorTexturePath;
			}
			if (hasSpecularStrengthMap)
			{
				text = text + " Has Specular Strength Map " + specularStrengthTexturePath;
			}
			if (hasGlossMap)
			{
				text = text + " Has Gloss Map " + glossTexturePath;
			}
			if (hasBumpMap)
			{
				text = text + " Has Bump Map " + bumpTexturePath;
			}
			if (hasNormalMap)
			{
				text = text + " Has Normal Map " + normalTexturePath;
			}
			if (hasSubsurfaceColorMap)
			{
				text = text + " Has Subsurface Color Map " + subsurfaceColorTexturePath;
			}
			if (hasSubsurfaceStrengthMap)
			{
				text = text + " Has Subsurface Strength Map " + subsurfaceStrengthTexturePath;
			}
			if (hasTranslucencyColorMap)
			{
				text = text + " Has Translucency Color Map " + translucencyColorTexturePath;
			}
			if (hasTranslucencyStrengthMap)
			{
				text = text + " Has Translucency Strength Map " + translucencyStrengthTexturePath;
			}
		}

		public void ImportImages(DAZImport importObject, string MaterialFolder)
		{
			if (diffuseTexturePath != null)
			{
				diffuseTexture = CopyAndImportImage(importObject, MaterialFolder, diffuseTexturePath, false, false, false, 1f, false, false, null, string.Empty);
			}
			if (alphaTexturePath != null)
			{
				alphaTexture = CopyAndImportImage(importObject, MaterialFolder, alphaTexturePath, false, true, false, 1f, false, false, null, string.Empty);
			}
			if (specularColorTexturePath != null)
			{
				specularColorTexture = CopyAndImportImage(importObject, MaterialFolder, specularColorTexturePath, false, false, false, 1f, false, false, null, string.Empty);
			}
			if (specularStrengthTexturePath != null)
			{
				specularColorTexture = CopyAndImportImage(importObject, MaterialFolder, specularStrengthTexturePath, false, false, false, 1f, false, true, null, string.Empty);
			}
			if (glossTexturePath != null)
			{
				glossTexture = CopyAndImportImage(importObject, MaterialFolder, glossTexturePath, false, false, false, 1f, true, false, null, string.Empty);
			}
			if (roughnessTexturePath != null)
			{
				glossTexture = CopyAndImportImage(importObject, MaterialFolder, roughnessTexturePath, false, false, false, 1f, true, false, "Inverted", string.Empty);
			}
			if (bumpTexturePath != null)
			{
				if (copyBumpAsSpecularColorMap)
				{
					specularColorTexture = CopyAndImportImage(importObject, MaterialFolder, bumpTexturePath, false, false, false, 1f, false, true, null, "AsSpec");
				}
				bumpTexture = CopyAndImportImage(importObject, MaterialFolder, bumpTexturePath, false, false, true, bumpStrength, false, false, null, string.Empty);
			}
			if (normalTexturePath != null)
			{
				normalTexture = CopyAndImportImage(importObject, MaterialFolder, normalTexturePath, true, false, false, 1f, false, false, null, string.Empty);
			}
			if (subsurfaceColorTexturePath != null)
			{
				subsurfaceColorTexture = CopyAndImportImage(importObject, MaterialFolder, subsurfaceColorTexturePath, false, false, false, 1f, false, false, null, string.Empty);
			}
			if (subsurfaceStrengthTexturePath != null)
			{
				subsurfaceStrengthTexture = CopyAndImportImage(importObject, MaterialFolder, subsurfaceStrengthTexturePath, false, false, false, 1f, false, true, null, string.Empty);
			}
			if (translucencyColorTexturePath != null)
			{
				translucencyColorTexture = CopyAndImportImage(importObject, MaterialFolder, translucencyColorTexturePath, false, false, false, 1f, false, false, null, string.Empty);
			}
			if (translucencyStrengthTexturePath != null)
			{
				translucencyStrengthTexture = CopyAndImportImage(importObject, MaterialFolder, translucencyStrengthTexturePath, false, false, false, 1f, false, true, null, string.Empty);
			}
		}

		public Material CreateMaterialTypeMVR()
		{
			string text = ((isTransparent && hasReflectionColor && reflectionStrength > 0f) ? reflTransparentShader : (isTransparent ? ((!hasNormalMap && !hasBumpMap) ? transparentShader : transparentNormalMapShader) : ((hasNormalMap || hasBumpMap) ? normalMapShader : ((!hasGlossMap) ? standardShader : glossShader))));
			if (text == null)
			{
				Debug.LogError("Shader names not properly set on DAZImportMaterial object");
				return null;
			}
			Shader shader = Shader.Find(text);
			if (shader == null)
			{
				Debug.LogError("Could not find shader " + text + ". Can't import material");
				return null;
			}
			Material material = new Material(shader);
			if (material == null)
			{
				Debug.LogError("Failed to create material with shader " + shader.name);
			}
			else
			{
				material.name = name;
				if (isTransparent)
				{
					diffuseColor.a = alphaStrength;
				}
				if (material.HasProperty("_Color"))
				{
					Color value = diffuseColor;
					if (!ignoreDiffuseColor)
					{
						value *= diffuseStrength;
						value.a = diffuseColor.a;
						value.r = Mathf.Clamp01(value.r);
						value.g = Mathf.Clamp01(value.g);
						value.b = Mathf.Clamp01(value.b);
					}
					material.SetColor("_Color", value);
				}
				bool flag = false;
				if (material.HasProperty("_AlphaTex") && isTransparent && alphaTexture != null)
				{
					flag = true;
					material.SetTexture("_AlphaTex", alphaTexture);
					material.SetTextureScale("_AlphaTex", uvScale);
					material.SetTextureOffset("_AlphaTex", uvOffset);
				}
				if (material.HasProperty("_MainTex"))
				{
					if (diffuseTexture != null)
					{
						material.SetTexture("_MainTex", diffuseTexture);
						if (!flag && isTransparent && alphaTexture != null)
						{
							Debug.LogError("Alpha texture " + alphaTexturePath + " has gone unused because shader does not support separate alpha texture and a diffuse texture also exists");
						}
						material.SetTextureScale("_MainTex", uvScale);
						material.SetTextureOffset("_MainTex", uvOffset);
					}
					else if (!flag && isTransparent && alphaTexture != null)
					{
						material.SetTexture("_MainTex", alphaTexture);
						material.SetTextureScale("_MainTex", uvScale);
						material.SetTextureOffset("_MainTex", uvOffset);
					}
				}
				if (material.HasProperty("_SpecInt"))
				{
					material.SetFloat("_SpecInt", specularStrength);
				}
				if (material.HasProperty("_SpecColor"))
				{
					material.SetColor("_SpecColor", specularColor);
				}
				if (material.HasProperty("_SpecTex"))
				{
					if (specularColorTexture != null)
					{
						material.SetTexture("_SpecTex", specularColorTexture);
						material.SetTextureScale("_SpecTex", uvScale);
						material.SetTextureOffset("_SpecTex", uvOffset);
						if (specularStrengthTexture != null)
						{
							Debug.LogError("Have both specular color and specular strength textures. Only using specular color texture");
						}
					}
					else if (specularStrengthTexture != null)
					{
						material.SetTexture("_SpecTex", specularStrengthTexture);
						material.SetTextureScale("_SpecTex", uvScale);
						material.SetTextureOffset("_SpecTex", uvOffset);
					}
				}
				if (material.HasProperty("_Shininess"))
				{
					float value2 = 2f + gloss * glossWeight * 6f;
					value2 = Mathf.Clamp(value2, 2f, 8f);
					material.SetFloat("_Shininess", value2);
				}
				if (material.HasProperty("_Fresnel"))
				{
					material.SetFloat("_Fresnel", fresnelStrength);
				}
				if (material.HasProperty("_GlossTex"))
				{
					material.SetTexture("_GlossTex", glossTexture);
					material.SetTextureScale("_GlossTex", uvScale);
					material.SetTextureOffset("_GlossTex", uvOffset);
				}
				else if (glossTexture != null)
				{
					Debug.LogError("Found gloss texture, but shader " + text + " does not support it");
				}
				if (material.HasProperty("_BumpMap"))
				{
					if (normalTexture != null)
					{
						material.SetTexture("_BumpMap", normalTexture);
						material.SetTextureScale("_BumpMap", uvScale);
						material.SetTextureOffset("_BumpMap", uvOffset);
					}
					else if (bumpTexture != null)
					{
						material.SetTexture("_BumpMap", bumpTexture);
						material.SetTextureScale("_BumpMap", uvScale);
						material.SetTextureOffset("_BumpMap", uvOffset);
					}
				}
				else if (normalTexture != null || bumpTexture != null)
				{
					Debug.LogError("Found normal texture, but shader does not support normal mapping");
				}
				if (material.HasProperty("_DiffuseBumpiness"))
				{
					material.SetFloat("_DiffuseBumpiness", bumpiness);
				}
				if (material.HasProperty("_SpecularBumpiness"))
				{
					material.SetFloat("_SpecularBumpiness", bumpiness);
				}
				if (material.HasProperty("_DetailMap"))
				{
					if (normalTexture != null && bumpTexture != null)
					{
						material.SetTexture("_DetailMap", bumpTexture);
						material.SetTextureScale("_DetailMap", uvScale);
						material.SetTextureOffset("_DetailMap", uvOffset);
					}
				}
				else if (normalTexture != null && bumpTexture != null)
				{
					Debug.LogError("Found both normal and bump texture, but shader does not support detail map");
				}
				if (material.HasProperty("_SubdermisColor"))
				{
					material.SetColor("_SubdermisColor", subsurfaceColor);
				}
				if (material.HasProperty("_SubdermisTex"))
				{
					if (subsurfaceColorTexture != null)
					{
						material.SetTexture("_SubdermisTex", subsurfaceColorTexture);
						material.SetTextureScale("_SubdermisTex", uvScale);
						material.SetTextureOffset("_SubdermisTex", uvOffset);
					}
				}
				else if (subsurfaceColorTexture != null)
				{
					Debug.LogError("Found subsurface color texture, but shader does not support using it");
				}
				if (material.HasProperty("_Subdermis"))
				{
					material.SetFloat("_Subdermis", subsurfaceStrength);
				}
				if (subsurfaceStrengthTexture != null)
				{
					Debug.LogError("Found subsurface strength texture, but shader does not support using it");
				}
				if (material.HasProperty("_TranslucencyColor"))
				{
					material.SetColor("_TranslucencyColor", translucencyColor);
				}
				if (material.HasProperty("_TranslucencyTex"))
				{
					if (translucencyColorTexture != null)
					{
						material.SetTexture("_TranslucencyTex", translucencyColorTexture);
						material.SetTextureScale("_TranslucencyTex", uvScale);
						material.SetTextureOffset("_TranslucencyTex", uvOffset);
					}
				}
				else if (translucencyColorTexture != null)
				{
					Debug.LogError("Found translucency color texture, but shader does not support using it");
				}
				if (translucencyStrengthTexture != null)
				{
					Debug.LogError("Found translucency strength texture, but shader does not support using it");
				}
			}
			return material;
		}

		public Material CreateMaterialTypeStandard(Shader s)
		{
			Material material = new Material(s);
			if (material == null)
			{
				Debug.LogError("Failed to create material with shader " + s.name);
			}
			else
			{
				Debug.LogError("TODO CreateMaterialTypeStandard");
			}
			return material;
		}

		public Material CreateMaterialTypeHDRP(Shader s)
		{
			Material material = new Material(s);
			if (material == null)
			{
				Debug.LogError("Failed to create material with shader " + s.name);
			}
			else
			{
				Debug.LogError("TODO CreateMaterialTypeHDRP");
			}
			return material;
		}
	}
}
