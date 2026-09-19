using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.GZip;
using MVR;
using MVR.FileManagement;
using Microsoft.Win32;
using SimpleJSON;
using UnityEngine;

namespace MeshVR
{
	[ExecuteInEditMode]
	public class DAZImport : ObjectAllocator
	{
		public enum ImportType
		{
			SingleObject,
			SkinnedSingleObject,
			Environment,
			Character,
			Clothing,
			Hair,
			HairScalp
		}

		public enum Gender
		{
			Neutral,
			Female,
			Male
		}

		public delegate void ImportCallback();

		public ImportType importType;

		protected const float geoScale = 0.01f;

		public string DAZLibraryDirectory = string.Empty;

		public string alternateDAZLibraryDirectory = string.Empty;

		[NonSerialized]
		public List<string> registryDAZLibraryDirectories;

		[NonSerialized]
		public string DAZSceneDufFile;

		public string DAZSceneModifierOverrideDufFile;

		public string modifierOverrideUrl;

		public bool createMaterials = true;

		public bool useSpecularAsGlossMap;

		public bool copyBumpAsSpecularColorMap;

		public bool forceBumpAsNormalMap;

		public bool replaceExistingMaterials;

		public bool combineToSingleMaterial;

		public bool materialOptionsDeregisterOnDisable;

		public bool combineMaterials;

		public Gender importGender;

		public bool createMorphBank;

		public ComputeShader GPUSkinCompute;

		public ComputeShader GPUMeshCompute;

		[HideInInspector]
		public string standardShader = "Custom/Subsurface/Cull";

		[HideInInspector]
		public string glossShader = "Custom/Subsurface/GlossCull";

		[HideInInspector]
		public string normalMapShader = "Custom/Subsurface/GlossNMCull";

		[HideInInspector]
		public string transparentShader = "Custom/Subsurface/Transparent";

		[HideInInspector]
		public string reflTransparentShader = "Custom/Subsurface/Transparent";

		[HideInInspector]
		public string transparentNormalMapShader = "Custom/Subsurface/TransparentGlossNMNoCullSeparateAlpha";

		[NonSerialized]
		public string subsurfaceStandardShader = "Custom/Subsurface/Cull";

		[NonSerialized]
		public string subsurfaceNoCullStandardShader = "Custom/Subsurface/NoCull";

		[NonSerialized]
		public string subsurfaceGlossMapShader = "Custom/Subsurface/GlossCull";

		[NonSerialized]
		public string subsurfaceNoCullGlossMapShader = "Custom/Subsurface/GlossNoCull";

		[NonSerialized]
		public string subsurfaceNormalMapShader = "Custom/Subsurface/GlossNMCull";

		[NonSerialized]
		public string subsurfaceNoCullNormalMapShader = "Custom/Subsurface/GlossNMNoCull";

		[NonSerialized]
		public string subsurfaceTransparentShader = "Custom/Subsurface/TransparentSeparateAlpha";

		[NonSerialized]
		public string subsurfaceReflTransparentShader = "Custom/Subsurface/TransparentSeparateAlpha";

		[NonSerialized]
		public string subsurfaceTransparentNormalMapShader = "Custom/Subsurface/TransparentGlossNMNoCullSeparateAlpha";

		[NonSerialized]
		public string subsurfaceMarmosetReflTransparentShader = "Marmoset/Transparent/Simple Glass/Specular IBL";

		[NonSerialized]
		public string subsurfaceAlphaAdjustShader = "Custom/Subsurface/TransparentGlossNMNoCullSeparateAlpha";

		[NonSerialized]
		public string hairShader = "Custom/Hair/MainSeparateAlphaLayer1";

		[NonSerialized]
		public string hairScalpShader = "Custom/Hair/MainSeparateAlphaLayerScalp";

		[NonSerialized]
		public string clothingShader = "Custom/Subsurface/TransparentGlossNMNoCullSeparateAlpha";

		public string MaterialCreationDirectory = "Assets/VaMAssets/Import/Environments";

		public string RuntimeMaterialCreationDirectory = "Custom/Atoms";

		public bool overrideMaterialFolderName;

		public string GeneratedMeshAssetDirectory = "Assets/VaMAssets/Generated/Atoms";

		public string MaterialOverrideFolderName = "Generic";

		private string MaterialFolder;

		protected Dictionary<Texture2D, string> textureToSourcePath;

		public Color subdermisColor = Color.white;

		public float bumpiness = 1f;

		public bool forceDiffuseColor;

		public Color diffuseColor = Color.white;

		public bool forceSpecularColor;

		public Color specularColor = Color.white;

		public bool createSkinsAndNodes = true;

		public bool createSkins = true;

		public Transform skinToWrapToTransform;

		public DAZSkinV2 skinToWrapTo;

		public bool wrapOnImport = true;

		public bool wrapToMorphedVertices;

		public bool createSkinWrap = true;

		public float skinWrapSurfaceOffset = 0.0003f;

		public float skinWrapAdditionalThicknessMultiplier = 0.001f;

		public int skinWrapSmoothOuterLoops = 1;

		public bool createMaterialOptions = true;

		public bool addParentNameToMaterialOptions;

		public bool connectMaterialUI = true;

		public UIMultiConnector materialUIConnector;

		public UIConnectorMaster materialUIConnectorMaster;

		public bool addMaterialTabs = true;

		public RectTransform materialUITab;

		public Color materialUITabColor;

		public string materialUITabAddBeforeTabName;

		public bool nestObjects;

		public bool embedMeshAndSkinOnNodes;

		public DAZSkinV2.PhysicsType skinPhysicsType;

		public Transform container;

		public Transform nodePrefab;

		public Transform controlPrefab;

		public Transform UIPrefab;

		public Transform UIPrefabNoPhysics;

		public Transform controlContainer;

		public Transform UIContainer;

		private Dictionary<string, JSONNode> DAZ_uv_library;

		private Dictionary<string, DAZUVMap> DAZ_uv_map;

		private Dictionary<string, Material> DAZ_material_map;

		private Dictionary<string, JSONNode> DAZ_geometry_library;

		private Dictionary<string, JSONNode> DAZ_modifier_library;

		private Dictionary<string, JSONNode> DAZ_node_library;

		private Dictionary<string, JSONNode> DAZ_material_library;

		private Dictionary<string, Transform> sceneNodeIDToTransform;

		private Dictionary<string, string> graftIDToMainID;

		private Dictionary<string, string> geometryIDToNodeID;

		private Dictionary<string, string> sceneGeometryIDToSceneNodeID;

		private DAZSkinV2[] dazSkins;

		public bool useSceneLabelsInsteadOfIds;

		public bool skipImportOfExisting;

		public string[] sceneNodeIds;

		public string[] sceneNodeLabels;

		public bool[] sceneNodeIdImport;

		public bool[] sceneNodeIdControllable;

		public bool[] sceneNodeIdIsPhysicsObj;

		public bool[] sceneNodeIdFloorLock;

		public string importStatus;

		public bool isImporting;

		public bool shouldCreateSkinsAndNodes
		{
			get
			{
				if (importType == ImportType.Environment || importType == ImportType.Character || importType == ImportType.SkinnedSingleObject)
				{
					return createSkinsAndNodes;
				}
				return false;
			}
		}

		public bool shouldCreateSkins
		{
			get
			{
				if (importType == ImportType.SkinnedSingleObject || importType == ImportType.Character)
				{
					return createSkins;
				}
				return false;
			}
		}

		public bool shouldCreateSkinWrap
		{
			get
			{
				if (importType == ImportType.Clothing || importType == ImportType.HairScalp)
				{
					return createSkinWrap;
				}
				return false;
			}
		}

		public string GetDefaultDAZContentPath()
		{
			if (UserPreferences.singleton != null && UserPreferences.singleton.DAZDefaultContentFolder != null && UserPreferences.singleton.DAZDefaultContentFolder != string.Empty)
			{
				return UserPreferences.singleton.DAZDefaultContentFolder;
			}
			if (registryDAZLibraryDirectories != null && registryDAZLibraryDirectories.Count > 0)
			{
				return registryDAZLibraryDirectories[0];
			}
			return null;
		}

		public string DetermineFilePath(string pathkey)
		{
			string text = null;
			bool flag = false;
			if (DAZLibraryDirectory != null && DAZLibraryDirectory != string.Empty)
			{
				text = DAZLibraryDirectory + pathkey;
				flag = File.Exists(text);
			}
			if (!flag && alternateDAZLibraryDirectory != null && alternateDAZLibraryDirectory != string.Empty)
			{
				text = alternateDAZLibraryDirectory + pathkey;
				flag = File.Exists(text);
			}
			if (!flag && UserPreferences.singleton != null && UserPreferences.singleton.DAZExtraLibraryFolder != null && UserPreferences.singleton.DAZExtraLibraryFolder != string.Empty)
			{
				text = UserPreferences.singleton.DAZExtraLibraryFolder + "/" + pathkey;
				flag = File.Exists(text);
			}
			if (!flag && registryDAZLibraryDirectories != null)
			{
				foreach (string registryDAZLibraryDirectory in registryDAZLibraryDirectories)
				{
					text = registryDAZLibraryDirectory + "/" + pathkey;
					flag = File.Exists(text);
					if (flag)
					{
						break;
					}
				}
			}
			if (flag)
			{
				return text;
			}
			return null;
		}

		public string GetTextureSourcePath(Texture2D tex)
		{
			string value = null;
			if (textureToSourcePath == null)
			{
				textureToSourcePath = new Dictionary<Texture2D, string>();
			}
			textureToSourcePath.TryGetValue(tex, out value);
			return value;
		}

		public void SetTextureSourcePath(Texture2D tex, string path)
		{
			if (textureToSourcePath == null)
			{
				textureToSourcePath = new Dictionary<Texture2D, string>();
			}
			if (textureToSourcePath.ContainsKey(tex))
			{
				textureToSourcePath.Remove(tex);
			}
			textureToSourcePath.Add(tex, path);
		}

		public static JSONNode ReadJSON(string path)
		{
			JSONNode result = null;
			try
			{
				FileManager.AssertNotCalledFromPlugin();
				if (FileChecker.IsGzipped(path))
				{
					using (FileEntryStream fileEntryStream = FileManager.OpenStream(path))
					{
						using (Stream stream = new GZipInputStream(fileEntryStream.Stream))
						{
							using (StreamReader streamReader = new StreamReader(stream))
							{
								string aJSON = streamReader.ReadToEnd();
								result = JSON.Parse(aJSON);
							}
						}
					}
				}
				else
				{
					using (FileEntryStreamReader fileEntryStreamReader = FileManager.OpenStreamReader(path))
					{
						string aJSON2 = fileEntryStreamReader.ReadToEnd();
						result = JSON.Parse(aJSON2);
					}
				}
			}
			catch (Exception ex)
			{
				SuperController.LogError("Exception during ReadJSON " + path + " " + ex.Message);
			}
			return result;
		}

		private List<string> ProcessDAZLibraries(string url, JSONNode jl)
		{
			if (DAZ_node_library == null)
			{
				DAZ_node_library = new Dictionary<string, JSONNode>();
			}
			JSONNode jSONNode = jl["node_library"];
			if (jSONNode != null)
			{
				foreach (JSONNode item in jSONNode.AsArray)
				{
					string text = item["id"];
					string key = url + "#" + text;
					if (url == string.Empty && DAZ_node_library.ContainsKey(key))
					{
						DAZ_node_library.Remove(key);
					}
					DAZ_node_library.Add(key, item);
				}
			}
			if (DAZ_geometry_library == null)
			{
				DAZ_geometry_library = new Dictionary<string, JSONNode>();
			}
			JSONNode jSONNode3 = jl["geometry_library"];
			if (jSONNode3 != null)
			{
				foreach (JSONNode item2 in jSONNode3.AsArray)
				{
					string text2 = item2["id"];
					string key2 = url + "#" + text2;
					if (url == string.Empty && DAZ_geometry_library.ContainsKey(key2))
					{
						DAZ_geometry_library.Remove(key2);
					}
					DAZ_geometry_library.Add(key2, item2);
				}
			}
			if (DAZ_modifier_library == null)
			{
				DAZ_modifier_library = new Dictionary<string, JSONNode>();
			}
			List<string> list = new List<string>();
			JSONNode jSONNode5 = jl["modifier_library"];
			if (jSONNode5 != null)
			{
				foreach (JSONNode item3 in jSONNode5.AsArray)
				{
					string text3 = item3["id"];
					string text4 = url + "#" + text3;
					list.Add(text4);
					if (url == string.Empty && DAZ_modifier_library.ContainsKey(text4))
					{
						DAZ_modifier_library.Remove(text4);
					}
					DAZ_modifier_library.Add(text4, item3);
				}
			}
			if (DAZ_uv_library == null)
			{
				DAZ_uv_library = new Dictionary<string, JSONNode>();
			}
			JSONNode jSONNode7 = jl["uv_set_library"];
			if (jSONNode7 != null)
			{
				foreach (JSONNode item4 in jSONNode7.AsArray)
				{
					string text5 = item4["id"];
					string key3 = url + "#" + text5;
					if (url == string.Empty && DAZ_uv_library.ContainsKey(key3))
					{
						DAZ_uv_library.Remove(key3);
					}
					DAZ_uv_library.Add(key3, item4);
				}
			}
			if (DAZ_material_library == null)
			{
				DAZ_material_library = new Dictionary<string, JSONNode>();
			}
			JSONNode jSONNode9 = jl["material_library"];
			if (jSONNode9 != null)
			{
				foreach (JSONNode item5 in jSONNode9.AsArray)
				{
					string text6 = item5["id"];
					string key4 = url + "#" + text6;
					if (url == string.Empty && DAZ_material_library.ContainsKey(key4))
					{
						DAZ_material_library.Remove(key4);
					}
					DAZ_material_library.Add(key4, item5);
				}
			}
			return list;
		}

		private List<string> ProcessAltModifiers(string url, JSONNode jl)
		{
			if (DAZ_modifier_library == null)
			{
				DAZ_modifier_library = new Dictionary<string, JSONNode>();
			}
			List<string> list = new List<string>();
			JSONNode jSONNode = jl["modifier_library"];
			if (jSONNode != null)
			{
				foreach (JSONNode item in jSONNode.AsArray)
				{
					string text = item["id"];
					string text2 = url + "#" + text;
					list.Add(text2);
					if (DAZ_modifier_library.ContainsKey(text2))
					{
						DAZ_modifier_library.Remove(text2);
					}
					DAZ_modifier_library.Add(text2, item);
				}
			}
			return list;
		}

		public static string ReplaceHex(Match m)
		{
			string value = m.Value.Replace("%", string.Empty);
			uint value2 = Convert.ToUInt32(value, 16);
			return Convert.ToChar(value2).ToString();
		}

		public static string DAZurlFix(string url)
		{
			MatchEvaluator evaluator = ReplaceHex;
			return Regex.Replace(url, "%([0-9A-Z][0-9A-Z])", evaluator);
		}

		public static string DAZurlToPathKey(string url)
		{
			url = DAZurlFix(url);
			return Regex.Replace(url, "#.*", string.Empty);
		}

		public static string DAZurlToId(string url)
		{
			url = DAZurlFix(url);
			return Regex.Replace(url, ".*#", string.Empty);
		}

		public List<string> ReadDAZdsf(string url)
		{
			string text = DAZurlToPathKey(url);
			List<string> list = null;
			if (text != string.Empty)
			{
				string text2 = DetermineFilePath(text);
				if (text2 != null)
				{
					JSONNode jl = ReadJSON(text2);
					return ProcessDAZLibraries(text, jl);
				}
				throw new Exception("File " + text + " could not be found in libraries. Check DAZ and alternate library paths for correctness.");
			}
			throw new Exception("Could not get path key from url " + url);
		}

		private JSONNode GetMaterial(string url)
		{
			if (DAZ_material_library == null)
			{
				DAZ_material_library = new Dictionary<string, JSONNode>();
			}
			url = DAZurlFix(url);
			if (!DAZ_material_library.ContainsKey(url))
			{
				ReadDAZdsf(url);
			}
			JSONNode value = null;
			if (!DAZ_material_library.TryGetValue(url, out value))
			{
				throw new Exception("Could not find material at " + url);
			}
			return value;
		}

		private JSONNode GetUV(string url)
		{
			if (DAZ_uv_library == null)
			{
				DAZ_uv_library = new Dictionary<string, JSONNode>();
			}
			url = DAZurlFix(url);
			if (!DAZ_uv_library.ContainsKey(url))
			{
				ReadDAZdsf(url);
			}
			JSONNode value;
			if (!DAZ_uv_library.TryGetValue(url, out value))
			{
				throw new Exception("Could not find uv at " + url);
			}
			return value;
		}

		private JSONNode GetGeometry(string url)
		{
			if (DAZ_geometry_library == null)
			{
				DAZ_geometry_library = new Dictionary<string, JSONNode>();
			}
			url = DAZurlFix(url);
			if (!DAZ_geometry_library.ContainsKey(url))
			{
				ReadDAZdsf(url);
			}
			JSONNode value;
			if (!DAZ_geometry_library.TryGetValue(url, out value))
			{
				throw new Exception("Could not find geometry at " + url);
			}
			return value;
		}

		private JSONNode GetNode(string url)
		{
			if (DAZ_node_library == null)
			{
				DAZ_node_library = new Dictionary<string, JSONNode>();
			}
			url = DAZurlFix(url);
			if (!DAZ_node_library.ContainsKey(url))
			{
				ReadDAZdsf(url);
			}
			JSONNode value;
			if (!DAZ_node_library.TryGetValue(url, out value))
			{
				throw new Exception("Could not find node at " + url);
			}
			return value;
		}

		private JSONNode GetModifier(string url)
		{
			if (DAZ_modifier_library == null)
			{
				DAZ_modifier_library = new Dictionary<string, JSONNode>();
			}
			url = DAZurlFix(url);
			if (!DAZ_modifier_library.ContainsKey(url))
			{
				ReadDAZdsf(url);
			}
			JSONNode value;
			if (!DAZ_modifier_library.TryGetValue(url, out value))
			{
				throw new Exception("Could not find modifier at " + url);
			}
			return value;
		}

		private DAZUVMap ProcessUV(string uvurl)
		{
			DAZUVMap dAZUVMap;
			if (uvurl == null)
			{
				dAZUVMap = new DAZUVMap();
			}
			else
			{
				if (DAZ_uv_map.TryGetValue(uvurl, out dAZUVMap))
				{
					return dAZUVMap;
				}
				dAZUVMap = new DAZUVMap();
				DAZ_uv_map.Add(uvurl, dAZUVMap);
				JSONNode uV = GetUV(uvurl);
				if (uV != null)
				{
					dAZUVMap.Import(uV);
				}
			}
			return dAZUVMap;
		}

		private DAZMesh GetDAZMeshBySceneGeometryId(string sceneGeometryId)
		{
			DAZMesh[] components = GetComponents<DAZMesh>();
			DAZMesh dAZMesh = null;
			DAZMesh[] array = components;
			foreach (DAZMesh dAZMesh2 in array)
			{
				if (dAZMesh2.sceneGeometryId == sceneGeometryId)
				{
					dAZMesh = dAZMesh2;
					break;
				}
			}
			if (dAZMesh == null && container != null && embedMeshAndSkinOnNodes)
			{
				components = container.GetComponentsInChildren<DAZMesh>();
				DAZMesh[] array2 = components;
				foreach (DAZMesh dAZMesh3 in array2)
				{
					if (dAZMesh3.sceneGeometryId == sceneGeometryId)
					{
						dAZMesh = dAZMesh3;
						break;
					}
				}
			}
			return dAZMesh;
		}

		private DAZMesh GetDAZMeshByGeometryId(string geometryId)
		{
			DAZMesh[] components = GetComponents<DAZMesh>();
			DAZMesh dAZMesh = null;
			DAZMesh[] array = components;
			foreach (DAZMesh dAZMesh2 in array)
			{
				if (dAZMesh2.geometryId == geometryId)
				{
					dAZMesh = dAZMesh2;
					break;
				}
			}
			if (dAZMesh == null && container != null && embedMeshAndSkinOnNodes)
			{
				components = container.GetComponentsInChildren<DAZMesh>();
				DAZMesh[] array2 = components;
				foreach (DAZMesh dAZMesh3 in array2)
				{
					if (dAZMesh3.geometryId == geometryId)
					{
						dAZMesh = dAZMesh3;
						break;
					}
				}
			}
			return dAZMesh;
		}

		private DAZMesh GetDAZMeshByNodeId(string nodeId)
		{
			DAZMesh[] components = GetComponents<DAZMesh>();
			DAZMesh dAZMesh = null;
			DAZMesh[] array = components;
			foreach (DAZMesh dAZMesh2 in array)
			{
				if (dAZMesh2.nodeId == nodeId)
				{
					dAZMesh = dAZMesh2;
					break;
				}
			}
			if (dAZMesh == null && container != null && embedMeshAndSkinOnNodes)
			{
				components = container.GetComponentsInChildren<DAZMesh>();
				DAZMesh[] array2 = components;
				foreach (DAZMesh dAZMesh3 in array2)
				{
					if (dAZMesh3.nodeId == nodeId)
					{
						dAZMesh = dAZMesh3;
						break;
					}
				}
			}
			return dAZMesh;
		}

		private DAZMesh GetDAZMeshBySceneNodeId(string sceneNodeId)
		{
			DAZMesh[] components = GetComponents<DAZMesh>();
			DAZMesh dAZMesh = null;
			DAZMesh[] array = components;
			foreach (DAZMesh dAZMesh2 in array)
			{
				if (dAZMesh2.sceneNodeId == sceneNodeId)
				{
					dAZMesh = dAZMesh2;
					break;
				}
			}
			if (dAZMesh == null && container != null && embedMeshAndSkinOnNodes)
			{
				components = container.GetComponentsInChildren<DAZMesh>();
				DAZMesh[] array2 = components;
				foreach (DAZMesh dAZMesh3 in array2)
				{
					if (dAZMesh3.sceneNodeId == sceneNodeId)
					{
						dAZMesh = dAZMesh3;
						break;
					}
				}
			}
			return dAZMesh;
		}

		private DAZMesh CreateDAZMesh(string sceneGeometryId, string geometryId, string sceneNodeId, string nodeId, GameObject meshContainer)
		{
			DAZMesh dAZMesh = meshContainer.AddComponent<DAZMesh>();
			dAZMesh.sceneGeometryId = sceneGeometryId;
			dAZMesh.geometryId = geometryId;
			dAZMesh.nodeId = nodeId;
			dAZMesh.sceneNodeId = sceneNodeId;
			if ((!shouldCreateSkinsAndNodes || !shouldCreateSkins) && !shouldCreateSkinWrap)
			{
				dAZMesh.drawMorphedUVMappedMesh = true;
			}
			if (shouldCreateSkinWrap)
			{
				dAZMesh.drawInEditorWhenNotPlaying = true;
			}
			return dAZMesh;
		}

		private DAZMesh GetOrCreateDAZMesh(string sceneGeometryId, string geometryId, string sceneNodeId, string nodeId, GameObject meshContainer)
		{
			DAZMesh dAZMesh = GetDAZMeshBySceneGeometryId(sceneGeometryId);
			if (dAZMesh == null)
			{
				dAZMesh = CreateDAZMesh(sceneGeometryId, geometryId, sceneNodeId, nodeId, meshContainer);
			}
			else
			{
				dAZMesh.sceneNodeId = sceneNodeId;
				dAZMesh.nodeId = nodeId;
			}
			return dAZMesh;
		}

		private DAZSkinV2 GetDAZSkin(string skinId, GameObject skinContainer)
		{
			DAZSkinV2[] components = skinContainer.GetComponents<DAZSkinV2>();
			DAZSkinV2 result = null;
			DAZSkinV2[] array = components;
			foreach (DAZSkinV2 dAZSkinV in array)
			{
				if (dAZSkinV.skinId == skinId)
				{
					result = dAZSkinV;
					break;
				}
			}
			return result;
		}

		/// <summary>
		/// In VaM these two references come from the prefab; the rip has no compute shaders, so they
		/// are recovered here instead of relying on the serialized (empty) fields.
		/// </summary>
		private void EnsureGPUComputeShaders()
		{
			if (GPUSkinCompute == null)
			{
				GPUSkinCompute = VamComputeShaderProvider.SkinShader;
			}
			if (GPUMeshCompute == null)
			{
				GPUMeshCompute = VamComputeShaderProvider.MeshShader;
			}
		}

		private DAZSkinV2 CreateDAZSkin(string skinId, string skinUrl, GameObject skinContainer)
		{
			DAZSkinV2 dAZSkinV = skinContainer.AddComponent<DAZSkinV2>();
			dAZSkinV.skinId = skinId;
			dAZSkinV.skinUrl = DAZurlFix(skinUrl);
			dAZSkinV.physicsType = skinPhysicsType;
			EnsureGPUComputeShaders();
			if (Application.isPlaying)
			{
				if (GPUSkinCompute != null)
				{
					dAZSkinV.GPUSkinner = GPUSkinCompute;
				}
				if (GPUMeshCompute != null)
				{
					dAZSkinV.GPUMeshCompute = GPUMeshCompute;
				}
			}
			return dAZSkinV;
		}

		private DAZSkinV2 GetOrCreateDAZSkin(string skinId, string skinUrl, GameObject skinContainer)
		{
			DAZSkinV2 dAZSkinV = GetDAZSkin(skinId, skinContainer);
			if (dAZSkinV == null)
			{
				dAZSkinV = CreateDAZSkin(skinId, skinUrl, skinContainer);
			}
			return dAZSkinV;
		}

		private DAZSkinWrap GetDAZSkinWrap(DAZMesh dazMesh)
		{
			DAZSkinWrap[] components = dazMesh.gameObject.GetComponents<DAZSkinWrap>();
			DAZSkinWrap result = null;
			DAZSkinWrap[] array = components;
			foreach (DAZSkinWrap dAZSkinWrap in array)
			{
				if (dAZSkinWrap.dazMesh == dazMesh)
				{
					result = dAZSkinWrap;
					break;
				}
			}
			return result;
		}

		private DAZSkinWrap CreateDAZSkinWrap(DAZMesh dazMesh)
		{
			DAZSkinWrap dAZSkinWrap = dazMesh.gameObject.AddComponent<DAZSkinWrap>();
			dAZSkinWrap.skinTransform = skinToWrapToTransform;
			dAZSkinWrap.skin = skinToWrapTo;
			dAZSkinWrap.dazMesh = dazMesh;
			EnsureGPUComputeShaders();
			if (Application.isPlaying)
			{
				if (GPUSkinCompute != null)
				{
					dAZSkinWrap.GPUSkinWrapper = GPUSkinCompute;
				}
				if (GPUMeshCompute != null)
				{
					dAZSkinWrap.GPUMeshCompute = GPUMeshCompute;
				}
			}
			return dAZSkinWrap;
		}

		private DAZSkinWrap GetOrCreateDAZSkinWrap(DAZMesh dazMesh)
		{
			DAZSkinWrap dAZSkinWrap = GetDAZSkinWrap(dazMesh);
			if (dAZSkinWrap == null)
			{
				dAZSkinWrap = CreateDAZSkinWrap(dazMesh);
			}
			return dAZSkinWrap;
		}

		private DAZMesh ProcessGeometry(string geourl, string sceneGeometryId, DAZUVMap[] uvmaplist, string sceneNodeId, string nodeId, GameObject meshContainer)
		{
			JSONNode geometry = GetGeometry(geourl);
			string geometryId = geometry["id"];
			DAZMesh orCreateDAZMesh = GetOrCreateDAZMesh(sceneGeometryId, geometryId, sceneNodeId, nodeId, meshContainer);
			if (orCreateDAZMesh != null)
			{
				orCreateDAZMesh.Import(geometry, uvmaplist[0], DAZ_material_map, true);
			}
			return orCreateDAZMesh;
		}

		private void SetPositionRotation(JSONNode jn, JSONNode sn, Transform t)
		{
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			foreach (JSONNode item in jn["center_point"].AsArray)
			{
				switch (item["id"])
				{
				case "x":
					zero.x = item["value"].AsFloat * -0.01f;
					break;
				case "y":
					zero.y = item["value"].AsFloat * 0.01f;
					break;
				case "z":
					zero.z = item["value"].AsFloat * 0.01f;
					break;
				}
			}
			foreach (JSONNode item2 in sn["center_point"].AsArray)
			{
				switch (item2["id"])
				{
				case "x":
					zero.x = item2["current_value"].AsFloat * -0.01f;
					break;
				case "y":
					zero.y = item2["current_value"].AsFloat * 0.01f;
					break;
				case "z":
					zero.z = item2["current_value"].AsFloat * 0.01f;
					break;
				}
			}
			foreach (JSONNode item3 in jn["orientation"].AsArray)
			{
				switch (item3["id"])
				{
				case "x":
					zero2.x = item3["value"].AsFloat;
					break;
				case "y":
					zero2.y = 0f - item3["value"].AsFloat;
					break;
				case "z":
					zero2.z = 0f - item3["value"].AsFloat;
					break;
				}
			}
			foreach (JSONNode item4 in sn["orientation"].AsArray)
			{
				switch (item4["id"])
				{
				case "x":
					zero2.x = item4["current_value"].AsFloat;
					break;
				case "y":
					zero2.y = 0f - item4["current_value"].AsFloat;
					break;
				case "z":
					zero2.z = 0f - item4["current_value"].AsFloat;
					break;
				}
			}
			t.position = zero;
			t.rotation = Quaternion.Euler(zero2);
		}

		private bool NodeExists(JSONNode sn, bool isRoot)
		{
			string url = DAZurlFix(sn["url"]);
			JSONNode node = GetNode(url);
			string text = node["name"];
			if (isRoot)
			{
				text = ((!useSceneLabelsInsteadOfIds) ? ((string)sn["id"]) : ((string)sn["label"]));
			}
			Transform transform = container.Find(text);
			if (transform != null)
			{
				return true;
			}
			return false;
		}

		private string ProcessNodeCreation(JSONNode sn, bool isRoot)
		{
			string url = DAZurlFix(sn["url"]);
			string text = sn["id"];
			JSONNode node = GetNode(url);
			string result = node["id"];
			if (sn["conform_target"] != null)
			{
				string text2 = DAZurlToId(sn["conform_target"]);
				if (graftIDToMainID == null)
				{
					graftIDToMainID = new Dictionary<string, string>();
				}
				graftIDToMainID.Add(text, text2);
				result = text2;
			}
			else if (shouldCreateSkinsAndNodes)
			{
				if (sceneNodeIDToTransform == null)
				{
					sceneNodeIDToTransform = new Dictionary<string, Transform>();
				}
				Transform value2;
				if (sn["parent"] != null)
				{
					string text3 = DAZurlToId(sn["parent"]);
					string value;
					if (graftIDToMainID != null && graftIDToMainID.TryGetValue(text3, out value))
					{
						text3 = value;
					}
					if (!sceneNodeIDToTransform.TryGetValue(text3, out value2))
					{
						Debug.LogWarning("Could not find parent transform " + text3);
						value2 = container;
					}
				}
				else
				{
					value2 = container;
				}
				string text4 = node["name"];
				if (isRoot)
				{
					text4 = ((!useSceneLabelsInsteadOfIds) ? text : ((string)sn["label"]));
				}
				GameObject gameObject = null;
				Transform transform = value2.Find(text4);
				if (transform == null)
				{
					transform = container.Find(text4);
				}
				if (transform != null)
				{
					gameObject = transform.gameObject;
				}
				if (gameObject == null)
				{
					if (isRoot && nodePrefab != null)
					{
						if (Application.isPlaying)
						{
							gameObject = UnityEngine.Object.Instantiate(nodePrefab.gameObject);
						}
						gameObject.name = text4;
						gameObject.transform.parent = value2;
						Transform transform2 = gameObject.transform.Find("object");
						if (transform2 != null)
						{
							sceneNodeIDToTransform.Add(text, transform2.transform);
						}
						else
						{
							sceneNodeIDToTransform.Add(text, gameObject.transform);
						}
					}
					else
					{
						gameObject = new GameObject(text4);
						gameObject.transform.parent = value2;
						sceneNodeIDToTransform.Add(text, gameObject.transform);
					}
				}
				else
				{
					gameObject.transform.parent = value2;
					if (isRoot && nodePrefab != null)
					{
						Transform transform3 = gameObject.transform.Find("object");
						if (transform3 != null)
						{
							sceneNodeIDToTransform.Add(text, transform3.transform);
						}
						else
						{
							sceneNodeIDToTransform.Add(text, gameObject.transform);
						}
					}
					else
					{
						sceneNodeIDToTransform.Add(text, gameObject.transform);
					}
				}
				if (isRoot)
				{
					SetPositionRotation(node, sn, gameObject.transform);
				}
				else if (shouldCreateSkins)
				{
					DAZBone dAZBone = gameObject.GetComponent<DAZBone>();
					if (dAZBone == null)
					{
						dAZBone = gameObject.AddComponent<DAZBone>();
					}
					dAZBone.ImportNode(node, importGender == Gender.Male);
					dAZBone.exclude = skinPhysicsType != DAZSkinV2.PhysicsType.None;
				}
				else
				{
					SetPositionRotation(node, sn, gameObject.transform);
				}
			}
			return result;
		}

		private void ProcessNodeTransform(JSONNode sn, bool isRoot)
		{
			string url = DAZurlFix(sn["url"]);
			string text = sn["id"];
			JSONNode node = GetNode(url);
			string text2 = node["id"];
			Transform value2;
			if (sn["parent"] != null)
			{
				string text3 = DAZurlToId(sn["parent"]);
				string value;
				if (graftIDToMainID != null && graftIDToMainID.TryGetValue(text3, out value))
				{
					text3 = value;
				}
				if (!sceneNodeIDToTransform.TryGetValue(text3, out value2))
				{
					Debug.LogWarning("Could not find parent transform " + text3);
					value2 = container;
				}
			}
			else
			{
				value2 = container;
			}
			string text4 = node["name"];
			if (isRoot)
			{
				text4 = ((!useSceneLabelsInsteadOfIds) ? text : ((string)sn["label"]));
			}
			GameObject gameObject = null;
			foreach (Transform item in value2)
			{
				if (item.name == text4)
				{
					gameObject = item.gameObject;
					break;
				}
			}
			if (gameObject == null)
			{
				return;
			}
			Vector3 zero = Vector3.zero;
			JSONNode jSONNode = sn["translation"];
			if (jSONNode == null)
			{
				jSONNode = node["translation"];
			}
			if (jSONNode != null)
			{
				foreach (JSONNode item2 in jSONNode.AsArray)
				{
					switch (item2["id"])
					{
					case "x":
						zero.x = item2["current_value"].AsFloat * -0.01f;
						break;
					case "y":
						zero.y = item2["current_value"].AsFloat * 0.01f;
						break;
					case "z":
						zero.z = item2["current_value"].AsFloat * 0.01f;
						break;
					}
				}
			}
			string text5 = sn["rotation_order"];
			if (text5 == null)
			{
				text5 = node["rotation_order"];
			}
			Quaternion2Angles.RotationOrder rotationOrder = Quaternion2Angles.RotationOrder.XYZ;
			if (text5 != null)
			{
				switch (text5)
				{
				case "XYZ":
					rotationOrder = Quaternion2Angles.RotationOrder.ZYX;
					break;
				case "XZY":
					rotationOrder = Quaternion2Angles.RotationOrder.YZX;
					break;
				case "YXZ":
					rotationOrder = Quaternion2Angles.RotationOrder.ZXY;
					break;
				case "YZX":
					rotationOrder = Quaternion2Angles.RotationOrder.XZY;
					break;
				case "ZXY":
					rotationOrder = Quaternion2Angles.RotationOrder.YXZ;
					break;
				case "ZYX":
					rotationOrder = Quaternion2Angles.RotationOrder.XYZ;
					break;
				default:
					Debug.LogError("Bad rotation order in json: " + text5);
					rotationOrder = Quaternion2Angles.RotationOrder.XYZ;
					break;
				}
			}
			Vector3 zero2 = Vector3.zero;
			JSONNode jSONNode3 = sn["rotation"];
			if (jSONNode3 == null)
			{
				jSONNode3 = node["rotation"];
			}
			if (jSONNode3 != null)
			{
				foreach (JSONNode item3 in jSONNode3.AsArray)
				{
					switch (item3["id"])
					{
					case "x":
						zero2.x = item3["current_value"].AsFloat;
						break;
					case "y":
						zero2.y = 0f - item3["current_value"].AsFloat;
						break;
					case "z":
						zero2.z = 0f - item3["current_value"].AsFloat;
						break;
					}
				}
			}
			Quaternion identity = Quaternion.identity;
			switch (rotationOrder)
			{
			case Quaternion2Angles.RotationOrder.XYZ:
				identity *= Quaternion.Euler(zero2.x, 0f, 0f);
				identity *= Quaternion.Euler(0f, zero2.y, 0f);
				identity *= Quaternion.Euler(0f, 0f, zero2.z);
				break;
			case Quaternion2Angles.RotationOrder.XZY:
				identity *= Quaternion.Euler(zero2.x, 0f, 0f);
				identity *= Quaternion.Euler(0f, 0f, zero2.z);
				identity *= Quaternion.Euler(0f, zero2.y, 0f);
				break;
			case Quaternion2Angles.RotationOrder.YXZ:
				identity *= Quaternion.Euler(0f, zero2.y, 0f);
				identity *= Quaternion.Euler(zero2.x, 0f, 0f);
				identity *= Quaternion.Euler(0f, 0f, zero2.z);
				break;
			case Quaternion2Angles.RotationOrder.YZX:
				identity *= Quaternion.Euler(0f, zero2.y, 0f);
				identity *= Quaternion.Euler(0f, 0f, zero2.z);
				identity *= Quaternion.Euler(zero2.x, 0f, 0f);
				break;
			case Quaternion2Angles.RotationOrder.ZXY:
				identity *= Quaternion.Euler(0f, 0f, zero2.z);
				identity *= Quaternion.Euler(zero2.x, 0f, 0f);
				identity *= Quaternion.Euler(0f, zero2.y, 0f);
				break;
			case Quaternion2Angles.RotationOrder.ZYX:
				identity *= Quaternion.Euler(0f, 0f, zero2.z);
				identity *= Quaternion.Euler(0f, zero2.y, 0f);
				identity *= Quaternion.Euler(zero2.x, 0f, 0f);
				break;
			}
			if (isRoot)
			{
				gameObject.transform.localPosition += zero;
				gameObject.transform.localRotation *= identity;
				if (!nestObjects)
				{
					gameObject.transform.parent = container;
				}
			}
			else if (shouldCreateSkins)
			{
				DAZBone component = gameObject.GetComponent<DAZBone>();
				if (component != null)
				{
					component.presetLocalTranslation = zero;
					component.presetLocalRotation = identity.eulerAngles;
				}
			}
			else
			{
				gameObject.transform.localPosition += zero;
				gameObject.transform.localRotation *= identity;
			}
		}

		public DAZMorph ProcessMorph(string modurl, string sceneId = null)
		{
			JSONNode modifier = GetModifier(modurl);
			if (modifier != null)
			{
				string text = DAZurlToId(modifier["parent"]);
				if (modifier["formulas"].Count > 0)
				{
					foreach (JSONNode item in modifier["formulas"].AsArray)
					{
						string input = item["output"];
						string text2 = Regex.Replace(input, "^.*\\?", string.Empty);
						if (text2 == "value")
						{
							string input2 = Regex.Replace(input, "^.*:", string.Empty);
							input2 = Regex.Replace(input2, "\\?.*", string.Empty);
							if (Regex.IsMatch(input2, "^/"))
							{
								ProcessMorph(input2, sceneId);
							}
						}
					}
				}
				DAZMorph dAZMorph = new DAZMorph();
				dAZMorph.Import(modifier);
				DAZMesh dAZMesh;
				if (sceneId != null)
				{
					dAZMesh = GetDAZMeshBySceneGeometryId(sceneId);
					if (dAZMesh == null)
					{
						dAZMesh = GetDAZMeshBySceneNodeId(sceneId);
					}
				}
				else
				{
					dAZMesh = GetDAZMeshByGeometryId(text);
					if (dAZMesh == null)
					{
						dAZMesh = GetDAZMeshByNodeId(text);
					}
				}
				if (dAZMesh != null)
				{
					if (dAZMesh.morphBank == null && createMorphBank)
					{
						DAZMorphBank dAZMorphBank = dAZMesh.gameObject.AddComponent<DAZMorphBank>();
						dAZMorphBank.connectedMesh = dAZMesh;
						dAZMesh.morphBank = dAZMorphBank;
					}
					if (dAZMesh.morphBank != null)
					{
						dAZMesh.morphBank.AddMorphUsingSubBanks(dAZMorph);
					}
				}
				else if (sceneId != null)
				{
					Debug.LogWarning("Could not find scene id " + sceneId + " when processing morph " + dAZMorph.morphName);
				}
				else
				{
					Debug.LogWarning("Could not find base id " + text + " when processing morph " + dAZMorph.morphName);
				}
				return dAZMorph;
			}
			Debug.LogError("Could not process morph " + modurl);
			return null;
		}

		private DAZSkinV2 ProcessSkin(JSONNode sn, GameObject skinContainer)
		{
			string text = sn["url"];
			string skinId = sn["id"];
			JSONNode modifier = GetModifier(text);
			DAZSkinV2 orCreateDAZSkin = GetOrCreateDAZSkin(skinId, text, skinContainer);
			if (orCreateDAZSkin != null)
			{
				string text2 = (orCreateDAZSkin.sceneGeometryId = DAZurlToId(sn["parent"]));
				string value;
				if (sceneGeometryIDToSceneNodeID.TryGetValue(text2, out value))
				{
					string value2;
					if (graftIDToMainID.TryGetValue(value, out value2))
					{
						value = value2;
					}
					Transform value3;
					if (sceneNodeIDToTransform.TryGetValue(value, out value3))
					{
						DAZBones component = value3.GetComponent<DAZBones>();
						if (component != null)
						{
							orCreateDAZSkin.root = component;
						}
						orCreateDAZSkin.Import(modifier);
					}
					else
					{
						Debug.LogError("Could not find root bone " + value + " during ProcessSkin for geometry " + text2);
					}
				}
			}
			return orCreateDAZSkin;
		}

		private bool ProcessMaterial(JSONNode sm)
		{
			if (createMaterials && MaterialFolder != null && MaterialFolder != string.Empty)
			{
				string text = sm["id"];
				JSONNode material = GetMaterial(sm["url"]);
				string text2 = sm["groups"][0];
				if (text == null || !(material != null))
				{
					return false;
				}
				Material material2 = null;
				string text3 = MaterialFolder + "/" + text + ".mat";
				if (material2 == null)
				{
					DAZImportMaterial dAZImportMaterial = new DAZImportMaterial();
					dAZImportMaterial.subsurfaceColor = subdermisColor;
					if (forceDiffuseColor)
					{
						dAZImportMaterial.ignoreDiffuseColor = true;
						dAZImportMaterial.diffuseColor = diffuseColor;
					}
					if (forceSpecularColor)
					{
						dAZImportMaterial.ignoreSpecularColor = true;
						dAZImportMaterial.specularColor = specularColor;
					}
					dAZImportMaterial.bumpiness = bumpiness;
					dAZImportMaterial.name = text;
					dAZImportMaterial.useSpecularAsGlossMap = useSpecularAsGlossMap;
					dAZImportMaterial.copyBumpAsSpecularColorMap = copyBumpAsSpecularColorMap;
					dAZImportMaterial.forceBumpAsNormalMap = forceBumpAsNormalMap;
					dAZImportMaterial.ProcessJSON(material);
					dAZImportMaterial.ProcessJSON(sm);
					dAZImportMaterial.Report();
					dAZImportMaterial.ImportImages(this, MaterialFolder);
					dAZImportMaterial.standardShader = standardShader;
					dAZImportMaterial.glossShader = glossShader;
					dAZImportMaterial.normalMapShader = normalMapShader;
					dAZImportMaterial.transparentShader = transparentShader;
					dAZImportMaterial.reflTransparentShader = reflTransparentShader;
					dAZImportMaterial.transparentNormalMapShader = transparentNormalMapShader;
					material2 = dAZImportMaterial.CreateMaterialTypeMVR();
					RegisterAllocatedObject(material2);
				}
				if (material2 != null && text2 != null && !DAZ_material_map.ContainsKey(text2))
				{
					DAZ_material_map.Add(text2, material2);
				}
			}
			return true;
		}

		public void ClearPrescan()
		{
			sceneNodeIds = null;
			sceneNodeLabels = null;
			sceneNodeIdImport = null;
			sceneNodeIdControllable = null;
			sceneNodeIdIsPhysicsObj = null;
			sceneNodeIdFloorLock = null;
		}

		public void PrescanDuf()
		{
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			if (DAZSceneDufFile != null && DAZSceneDufFile != string.Empty)
			{
				JSONNode jSONNode = ReadJSON(DAZSceneDufFile);
				JSONNode jSONNode2 = jSONNode["scene"]["nodes"];
				foreach (JSONNode item in jSONNode2.AsArray)
				{
					if (item["geometries"] != null)
					{
						list.Add(item["id"]);
						list2.Add(item["label"]);
					}
				}
			}
			sceneNodeIds = list.ToArray();
			sceneNodeLabels = list2.ToArray();
			if (sceneNodeIdImport == null || sceneNodeIdImport.Length != sceneNodeIds.Length)
			{
				sceneNodeIdImport = new bool[sceneNodeIds.Length];
				for (int i = 0; i < sceneNodeIds.Length; i++)
				{
					sceneNodeIdImport[i] = true;
				}
			}
			if (sceneNodeIdControllable == null || sceneNodeIdControllable.Length != sceneNodeIds.Length)
			{
				sceneNodeIdControllable = new bool[sceneNodeIds.Length];
				for (int j = 0; j < sceneNodeIds.Length; j++)
				{
					sceneNodeIdControllable[j] = true;
				}
			}
			if (sceneNodeIdIsPhysicsObj == null || sceneNodeIdIsPhysicsObj.Length != sceneNodeIds.Length)
			{
				sceneNodeIdIsPhysicsObj = new bool[sceneNodeIds.Length];
			}
			if (sceneNodeIdFloorLock == null || sceneNodeIdFloorLock.Length != sceneNodeIds.Length)
			{
				sceneNodeIdFloorLock = new bool[sceneNodeIds.Length];
				for (int k = 0; k < sceneNodeIds.Length; k++)
				{
					sceneNodeIdFloorLock[k] = true;
				}
			}
		}

		public void SetRegistryLibPaths()
		{
			registryDAZLibraryDirectories = new List<string>();
			// Registry.GetValue returns null when the whole key is missing, which is the normal
			// state on a machine without DAZ Studio installed; unboxing that null is what threw.
			object numContentDirs = Registry.GetValue("HKEY_CURRENT_USER\\Software\\DAZ\\Studio4", "NumContentDirs", 0);
			int num = 0;
			if (numContentDirs != null)
			{
				num = (int)numContentDirs;
			}
			for (int i = 0; i < num; i++)
			{
				string text = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\DAZ\\Studio4", "ContentDir" + i, null);
				if (text != null && text != string.Empty)
				{
					registryDAZLibraryDirectories.Add(text);
				}
			}
		}

		protected string GetMaterialSignature(Material m)
		{
			string text = string.Empty;
			if (m.HasProperty("_Color"))
			{
				text = text + ":C:" + m.GetColor("_Color").ToString();
			}
			if (m.HasProperty("_SpecColor"))
			{
				text = text + ":SC:" + m.GetColor("_SpecColor").ToString();
			}
			if (m.HasProperty("_SubdermisColor"))
			{
				text = text + ":SDC:" + m.GetColor("_SubdermisColor").ToString();
			}
			if (m.HasProperty("_AlphaAdjust"))
			{
				text = text + ":AA:" + m.GetFloat("_AlphaAdjust").ToString("F3");
			}
			if (m.HasProperty("_DiffOffset"))
			{
				text = text + ":DO:" + m.GetFloat("_DiffOffset").ToString("F3");
			}
			if (m.HasProperty("_SpecOffset"))
			{
				text = text + ":SO:" + m.GetFloat("_SpecOffset").ToString("F3");
			}
			if (m.HasProperty("_GlossOffset"))
			{
				text = text + ":GO:" + m.GetFloat("_GlossOffset").ToString("F3");
			}
			if (m.HasProperty("_SpecInt"))
			{
				text = text + ":SI:" + m.GetFloat("_SpecInt").ToString("F3");
			}
			if (m.HasProperty("_Shininess"))
			{
				text = text + ":SH:" + m.GetFloat("_Shininess").ToString("F3");
			}
			if (m.HasProperty("_Fresnel"))
			{
				text = text + ":SF:" + m.GetFloat("_Fresnel").ToString("F3");
			}
			if (m.HasProperty("_IBLFilter"))
			{
				text = text + ":IF:" + m.GetFloat("_IBLFilter").ToString("F3");
			}
			if (m.HasProperty("_DiffuseBumpiness"))
			{
				text = text + ":DB:" + m.GetFloat("_DiffuseBumpiness").ToString("F3");
			}
			if (m.HasProperty("_SpecularBumpiness"))
			{
				text = text + ":SB:" + m.GetFloat("_SpecularBumpiness").ToString("F3");
			}
			if (m.HasProperty("_MainTex"))
			{
				Texture texture = m.GetTexture("_MainTex");
				if (texture != null)
				{
					text = text + ":MT:" + texture.name;
				}
			}
			if (m.HasProperty("_SpecTex"))
			{
				Texture texture2 = m.GetTexture("_SpecTex");
				if (texture2 != null)
				{
					text = text + ":ST:" + texture2.name;
				}
			}
			if (m.HasProperty("_GlossTex"))
			{
				Texture texture3 = m.GetTexture("_GlossTex");
				if (texture3 != null)
				{
					text = text + ":GT:" + texture3.name;
				}
			}
			if (m.HasProperty("_AlphaTex"))
			{
				Texture texture4 = m.GetTexture("_AlphaTex");
				if (texture4 != null)
				{
					text = text + ":AT:" + texture4.name;
				}
			}
			if (m.HasProperty("_BumpMap"))
			{
				Texture texture5 = m.GetTexture("_BumpMap");
				if (texture5 != null)
				{
					text = text + ":BM:" + texture5.name;
				}
			}
			if (m.HasProperty("_DetailMap"))
			{
				Texture texture6 = m.GetTexture("_DetailMap");
				if (texture6 != null)
				{
					text = text + ":DM:" + texture6.name;
				}
			}
			if (m.HasProperty("_DecalTex"))
			{
				Texture texture7 = m.GetTexture("_DecalTex");
				if (texture7 != null)
				{
					text = text + ":DT:" + texture7.name;
				}
			}
			return text;
		}

		protected bool CompareMaterials(Material m1, Material m2)
		{
			if (m1.HasProperty("_Color") && !m2.HasProperty("_Color"))
			{
				return false;
			}
			return true;
		}

		public void CreateMaterialOptionsUI(MaterialOptions mo, string tabName)
		{
			if (materialUIConnector != null)
			{
				materialUIConnector.AddConnector(mo);
			}
			if (materialUIConnectorMaster != null && addMaterialTabs)
			{
				TabbedUIBuilder.Tab tab = new TabbedUIBuilder.Tab();
				tab.name = tabName;
				tab.prefab = materialUITab;
				tab.color = materialUITabColor;
				materialUIConnectorMaster.AddTab(tab, materialUITabAddBeforeTabName);
			}
		}

		public void CreateMaterialOptions(GameObject go, int numMats, string[] materialNames, Material[] materials, Type type, string customTextureFolder, bool createWithExclude = false)
		{
			MaterialOptions[] components = go.GetComponents<MaterialOptions>();
			int num = components.Length;
			int num2 = numMats;
			Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
			if (combineToSingleMaterial)
			{
				num2 = 1;
				List<int> list = new List<int>();
				for (int i = 0; i < materials.Length; i++)
				{
					list.Add(i);
				}
				dictionary.Add(0, list);
			}
			else if (combineMaterials)
			{
				Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
				int num3 = 0;
				for (int j = 0; j < materials.Length; j++)
				{
					string materialSignature = GetMaterialSignature(materials[j]);
					int value;
					if (dictionary2.TryGetValue(materialSignature, out value))
					{
						List<int> value2;
						if (dictionary.TryGetValue(value, out value2))
						{
							value2.Add(j);
						}
					}
					else
					{
						dictionary2.Add(materialSignature, num3);
						List<int> list2 = new List<int>();
						list2.Add(j);
						dictionary.Add(num3, list2);
						num3++;
					}
				}
				num2 = num3;
			}
			else
			{
				for (int k = 0; k < materials.Length; k++)
				{
					List<int> list3 = new List<int>();
					list3.Add(k);
					dictionary.Add(k, list3);
				}
				num2 = numMats;
			}
			if (num == num2)
			{
				return;
			}
			int num4 = num2 - num;
			if (num4 > 0)
			{
				for (int l = 0; l < num4; l++)
				{
					go.AddComponent(type);
				}
			}
			components = go.GetComponents<MaterialOptions>();
			UIMultiConnector uIMultiConnector = null;
			UIConnectorMaster uIConnectorMaster = null;
			ConfigurableJoint component = go.GetComponent<ConfigurableJoint>();
			if (component != null && component.connectedBody != null)
			{
				FreeControllerV3 component2 = component.connectedBody.GetComponent<FreeControllerV3>();
				if (component2 != null && component2.UITransforms != null && component2.UITransforms.Length > 0)
				{
					Transform transform = component2.UITransforms[0];
					if (transform != null)
					{
						uIMultiConnector = transform.GetComponent<UIMultiConnector>();
						uIConnectorMaster = transform.GetComponent<UIConnectorMaster>();
					}
				}
			}
			if (uIMultiConnector == null)
			{
				uIMultiConnector = materialUIConnector;
			}
			if (uIConnectorMaster == null)
			{
				uIConnectorMaster = materialUIConnectorMaster;
			}
			for (int m = 0; m < components.Length; m++)
			{
				string text = ((!combineMaterials && !combineToSingleMaterial) ? materialNames[m] : ((components.Length != 1) ? ("Combined" + (m + 1)) : "Combined"));
				components[m].deregisterOnDisable = materialOptionsDeregisterOnDisable;
				if (connectMaterialUI)
				{
					if (uIMultiConnector != null)
					{
						uIMultiConnector.AddConnector(components[m]);
					}
					if (uIConnectorMaster != null && addMaterialTabs)
					{
						TabbedUIBuilder.Tab tab = new TabbedUIBuilder.Tab();
						tab.name = text;
						tab.prefab = materialUITab;
						tab.color = materialUITabColor;
						uIConnectorMaster.AddTab(tab, materialUITabAddBeforeTabName);
					}
				}
				components[m].exclude = createWithExclude;
				if (addParentNameToMaterialOptions)
				{
					components[m].overrideId = "+parent+Material" + text;
				}
				else
				{
					components[m].overrideId = "+Material" + text;
				}
				List<int> value3;
				if (dictionary.TryGetValue(m, out value3))
				{
					components[m].paramMaterialSlots = value3.ToArray();
					components[m].materialForDefaults = materials[value3[0]];
					if (components[m].textureGroup1 == null)
					{
						components[m].textureGroup1 = new MaterialOptionTextureGroup();
					}
					components[m].textureGroup1.materialSlots = value3.ToArray();
					if (Application.isPlaying)
					{
						components[m].customTextureFolder = customTextureFolder;
					}
					components[m].SetStartingValues(textureToSourcePath);
				}
			}
		}

		public void ClearMaterialConnectors()
		{
			if (connectMaterialUI)
			{
				if (materialUIConnector != null)
				{
					materialUIConnector.ClearConnectors();
				}
				if (materialUIConnectorMaster != null)
				{
					materialUIConnectorMaster.ClearRuntimeTabs();
				}
			}
		}

		public IEnumerator ImportDufMorphsCo(ImportCallback callback = null)
		{
			isImporting = true;
			importStatus = "Initialization";
			yield return null;
			SetRegistryLibPaths();
			DAZ_modifier_library = new Dictionary<string, JSONNode>();
			DAZ_geometry_library = new Dictionary<string, JSONNode>();
			DAZ_node_library = new Dictionary<string, JSONNode>();
			DAZ_uv_library = new Dictionary<string, JSONNode>();
			if (DAZSceneDufFile == null || !(DAZSceneDufFile != string.Empty))
			{
				yield break;
			}
			JSONNode djn = null;
			string dufname = Regex.Replace(DAZSceneDufFile, ".*/", string.Empty);
			dufname = Regex.Replace(dufname, "\\.duf", string.Empty);
			try
			{
				djn = ReadJSON(DAZSceneDufFile);
			}
			catch (Exception ex)
			{
				SuperController.LogError("Exception during DAZ import: " + ex.Message);
				importStatus = "Import failed " + ex.Message;
				isImporting = false;
				callback?.Invoke();
				yield break;
			}
			yield return null;
			JSONNode JSONSceneModifiers = djn["scene"]["modifiers"];
			if (!(JSONSceneModifiers != null))
			{
				yield break;
			}
			foreach (JSONNode item in JSONSceneModifiers.AsArray)
			{
				string text = item["channel"]["type"];
				if (!(text == "float"))
				{
					continue;
				}
				string sceneId = DAZurlToId(item["parent"]);
				DAZMorph dAZMorph = ProcessMorph(item["url"], sceneId);
				if (dAZMorph != null && !dAZMorph.preserveValueOnReimport)
				{
					float num = item["channel"]["current_value"].AsFloat;
					if (num <= 0.001f && num >= -0.001f)
					{
						num = 0f;
					}
					dAZMorph.importValue = num;
					dAZMorph.morphValue = num;
				}
			}
		}

		public IEnumerator ImportDufCo(ImportCallback callback = null)
		{
			isImporting = true;
			importStatus = "Initialization";
			yield return null;
			SetRegistryLibPaths();
			Dictionary<string, bool> sceneNodeIdToImport = new Dictionary<string, bool>();
			Dictionary<string, bool> sceneNodeIdToControllable = new Dictionary<string, bool>();
			Dictionary<string, bool> sceneNodeIdToPhysicsEnabled = new Dictionary<string, bool>();
			Dictionary<string, bool> sceneNodeIdToFloorLock = new Dictionary<string, bool>();
			if (sceneNodeIds != null)
			{
				for (int i = 0; i < sceneNodeIds.Length; i++)
				{
					string key = sceneNodeIds[i];
					sceneNodeIdToImport.Add(key, sceneNodeIdImport[i]);
					sceneNodeIdToControllable.Add(key, sceneNodeIdControllable[i]);
					sceneNodeIdToPhysicsEnabled.Add(key, sceneNodeIdIsPhysicsObj[i]);
					sceneNodeIdToFloorLock.Add(key, sceneNodeIdFloorLock[i]);
				}
			}
			if (container == null)
			{
				container = base.transform;
			}
			DAZ_modifier_library = new Dictionary<string, JSONNode>();
			DAZ_geometry_library = new Dictionary<string, JSONNode>();
			DAZ_node_library = new Dictionary<string, JSONNode>();
			DAZ_uv_library = new Dictionary<string, JSONNode>();
			DAZ_uv_map = new Dictionary<string, DAZUVMap>();
			DAZ_material_map = new Dictionary<string, Material>();
			sceneNodeIDToTransform = new Dictionary<string, Transform>();
			graftIDToMainID = new Dictionary<string, string>();
			if (DAZSceneDufFile != null && DAZSceneDufFile != string.Empty)
			{
				JSONNode djn = null;
				string dufname = Regex.Replace(DAZSceneDufFile, ".*/", string.Empty);
				dufname = Regex.Replace(dufname, "\\.duf", string.Empty);
				try
				{
					djn = ReadJSON(DAZSceneDufFile);
					string text = ((!Application.isPlaying) ? MaterialCreationDirectory : ((importType != ImportType.Character && importType != ImportType.Clothing && importType != ImportType.Hair && importType != ImportType.HairScalp) ? RuntimeMaterialCreationDirectory : (RuntimeMaterialCreationDirectory + "/" + importGender)));
					if (overrideMaterialFolderName)
					{
						MaterialFolder = text + "/" + MaterialOverrideFolderName;
					}
					else
					{
						MaterialFolder = text + "/" + dufname;
					}
					if (Application.isPlaying)
					{
						if (ImageLoaderThreaded.singleton != null)
						{
							ImageLoaderThreaded.singleton.PurgeAllImmediateTextures();
						}
						Directory.CreateDirectory(MaterialFolder);
					}
					ClearMaterialConnectors();
					importStatus = "Reading libraries";
					ProcessDAZLibraries(string.Empty, djn);
				}
				catch (Exception ex)
				{
					SuperController.LogError("Exception during DAZ import: " + ex.Message);
					importStatus = "Import failed " + ex.Message;
					isImporting = false;
					callback?.Invoke();
					yield break;
				}
				yield return null;
				JSONNode JSONSceneMaterials = null;
				Dictionary<string, List<DAZUVMap>> materialMap = new Dictionary<string, List<DAZUVMap>>();
				try
				{
					JSONSceneMaterials = djn["scene"]["materials"];
					foreach (JSONNode item in JSONSceneMaterials.AsArray)
					{
						string key2 = DAZurlToId(item["geometry"]);
						string uvurl = item["uv_set"];
						DAZUVMap dAZUVMap = ProcessUV(uvurl);
						if (dAZUVMap != null)
						{
							List<DAZUVMap> value;
							if (!materialMap.TryGetValue(key2, out value))
							{
								value = new List<DAZUVMap>();
								materialMap.Add(key2, value);
							}
							value.Add(dAZUVMap);
						}
						else
						{
							importStatus = "Error during process of UV maps";
						}
					}
				}
				catch (Exception ex2)
				{
					SuperController.LogError("Exception during DAZ import: " + ex2.Message);
					importStatus = "Import failed " + ex2.Message;
					isImporting = false;
					callback?.Invoke();
					yield break;
				}
				int numMaterials = JSONSceneMaterials.Count;
				int ind = 1;
				IEnumerator enumerator2 = JSONSceneMaterials.AsArray.GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						JSONNode sm = (JSONNode)enumerator2.Current;
						try
						{
							importStatus = "Import material " + ind + " of " + numMaterials;
							float num2 = (float)ind * 1f / (float)numMaterials;
							if (!ProcessMaterial(sm))
							{
								importStatus = "Error during process of material";
							}
						}
						catch (Exception ex3)
						{
							SuperController.LogError("Exception during DAZ import: " + ex3);
							importStatus = "Import failed " + ex3.Message;
							isImporting = false;
							callback?.Invoke();
							yield break;
						}
						ind++;
						yield return null;
					}
				}
				finally
				{
					IDisposable disposable;
					IDisposable disposable2 = (disposable = enumerator2 as IDisposable);
					if (disposable != null)
					{
						disposable2.Dispose();
					}
				}
				try
				{
					JSONNode jSONNode2 = djn["scene"]["modifiers"];
					importStatus = "Process geometry";
					JSONNode jSONNode3 = djn["scene"]["nodes"];
					sceneGeometryIDToSceneNodeID = new Dictionary<string, string>();
					DAZSkinWrap dAZSkinWrap = null;
					Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
					Dictionary<string, Transform> dictionary2 = new Dictionary<string, Transform>();
					foreach (JSONNode item2 in jSONNode3.AsArray)
					{
						string text2 = item2["id"];
						string text3 = ((!useSceneLabelsInsteadOfIds) ? text2 : ((string)item2["label"]));
						bool flag = false;
						if (item2["geometries"] != null)
						{
							flag = true;
						}
						bool value2 = true;
						if (sceneNodeIdToImport.ContainsKey(text2))
						{
							sceneNodeIdToImport.TryGetValue(text2, out value2);
						}
						if (flag && skipImportOfExisting && NodeExists(item2, flag))
						{
							value2 = false;
						}
						if (!value2)
						{
							continue;
						}
						string nodeId = ProcessNodeCreation(item2, flag);
						bool value3 = false;
						bool value4 = false;
						if (flag)
						{
							Transform transform = base.transform;
							Transform value5;
							if (sceneNodeIDToTransform.TryGetValue(text2, out value5))
							{
								if (embedMeshAndSkinOnNodes)
								{
									if (item2["conform_target"] != null)
									{
										string sceneNodeId = DAZurlToId(item2["conform_target"]);
										DAZMesh dAZMeshBySceneNodeId = GetDAZMeshBySceneNodeId(sceneNodeId);
										transform = ((!(dAZMeshBySceneNodeId != null)) ? value5 : dAZMeshBySceneNodeId.transform);
									}
									else
									{
										transform = value5;
									}
								}
								dictionary.Add(text2, transform);
								if (shouldCreateSkins)
								{
									DAZBones component = value5.GetComponent<DAZBones>();
									if (component == null)
									{
										value5.gameObject.AddComponent<DAZBones>();
									}
								}
								GameObject gameObject = null;
								bool flag2 = false;
								bool value6 = false;
								sceneNodeIdToControllable.TryGetValue(text2, out value6);
								if (value6)
								{
									if (controlContainer != null && controlPrefab != null)
									{
										string text4 = text3 + "Control";
										Transform transform2 = controlContainer.Find(text4);
										if (transform2 != null)
										{
											gameObject = transform2.gameObject;
										}
										else
										{
											if (Application.isPlaying)
											{
												gameObject = UnityEngine.Object.Instantiate(controlPrefab.gameObject);
											}
											gameObject.name = text4;
											gameObject.transform.parent = controlContainer;
											gameObject.transform.position = transform.position;
											gameObject.transform.rotation = transform.rotation;
										}
										dictionary2.Add(text2, gameObject.transform);
										Rigidbody component2 = gameObject.GetComponent<Rigidbody>();
										Rigidbody component3 = transform.GetComponent<Rigidbody>();
										if (component3 != null && sceneNodeIdToPhysicsEnabled.TryGetValue(text2, out value3))
										{
											component3.isKinematic = !value3;
										}
										ConfigurableJoint component4 = transform.GetComponent<ConfigurableJoint>();
										if (component4 != null && component2 != null)
										{
											flag2 = true;
											component4.connectedBody = component2;
										}
										sceneNodeIdToFloorLock.TryGetValue(text2, out value4);
									}
									if (UIContainer != null && UIPrefab != null)
									{
										GameObject gameObject2 = null;
										string text5 = text3 + "UI";
										Transform transform3 = UIContainer.Find(text5);
										if (transform3 != null)
										{
											GameObject gameObject3 = transform3.gameObject;
										}
										else
										{
											if (Application.isPlaying)
											{
												gameObject2 = ((value3 || !(UIPrefabNoPhysics != null)) ? UnityEngine.Object.Instantiate(UIPrefab.gameObject) : UnityEngine.Object.Instantiate(UIPrefabNoPhysics.gameObject));
											}
											gameObject2.name = text5;
											gameObject2.transform.parent = UIContainer;
											if (gameObject != null)
											{
												FreeControllerV3 component5 = gameObject.GetComponent<FreeControllerV3>();
												if (component5 != null)
												{
													if (flag2)
													{
														component5.followWhenOff = transform;
													}
													if (value3)
													{
														component5.startingPositionState = FreeControllerV3.PositionState.Off;
														component5.startingRotationState = FreeControllerV3.RotationState.Off;
													}
													if (value4)
													{
														component5.yLock = true;
														component5.xRotLock = true;
														component5.zRotLock = true;
													}
													component5.UITransforms = new Transform[1];
													component5.UITransforms[0] = gameObject2.transform;
													UIConnector uIConnector = gameObject2.AddComponent<UIConnector>();
													uIConnector.receiverTransform = component5.transform;
													uIConnector.receiver = component5;
													uIConnector.storeid = component5.storeId;
												}
												MotionAnimationControl component6 = gameObject.GetComponent<MotionAnimationControl>();
												if (component6 != null)
												{
													UIConnector uIConnector2 = gameObject2.AddComponent<UIConnector>();
													uIConnector2.receiverTransform = component6.transform;
													uIConnector2.receiver = component6;
													uIConnector2.storeid = component6.storeId;
												}
												SetTransformScale component7 = transform.GetComponent<SetTransformScale>();
												if (component7 != null)
												{
													UIConnector uIConnector3 = gameObject2.AddComponent<UIConnector>();
													uIConnector3.receiverTransform = component7.transform;
													uIConnector3.receiver = component7;
													uIConnector3.storeid = component7.storeId;
												}
												PhysicsMaterialControl component8 = transform.GetComponent<PhysicsMaterialControl>();
												if (component8 != null)
												{
													UIConnector uIConnector4 = gameObject2.AddComponent<UIConnector>();
													uIConnector4.receiverTransform = component8.transform;
													uIConnector4.receiver = component8;
													uIConnector4.storeid = component8.storeId;
												}
											}
										}
									}
								}
								else
								{
									ConfigurableJoint component9 = transform.GetComponent<ConfigurableJoint>();
									if (component9 != null)
									{
										UnityEngine.Object.DestroyImmediate(component9);
									}
									Rigidbody component10 = transform.GetComponent<Rigidbody>();
									if (component10 != null)
									{
										UnityEngine.Object.DestroyImmediate(component10);
									}
									ForceReceiver component11 = transform.GetComponent<ForceReceiver>();
									if (component11 != null)
									{
										UnityEngine.Object.DestroyImmediate(component11);
									}
									PhysicsMaterialControl component12 = transform.GetComponent<PhysicsMaterialControl>();
									if (component12 != null)
									{
										UnityEngine.Object.DestroyImmediate(component12);
									}
								}
							}
							DAZMesh dAZMesh = null;
							foreach (JSONNode item3 in item2["geometries"].AsArray)
							{
								string text6 = item3["url"];
								if (text6 != null)
								{
									string text7 = item3["id"];
									sceneGeometryIDToSceneNodeID.Add(text7, text2);
									List<DAZUVMap> value7;
									if (materialMap.TryGetValue(text7, out value7))
									{
										dAZMesh = ProcessGeometry(text6, text7, value7.ToArray(), text2, nodeId, transform.gameObject);
										if (dAZMesh != null)
										{
											if (shouldCreateSkinWrap)
											{
												if (!Application.isPlaying)
												{
													dAZMesh.SaveMeshAsset();
												}
												dAZMesh.createMeshFilterAndRenderer = true;
												MeshRenderer component13 = dAZMesh.GetComponent<MeshRenderer>();
												if (component13 != null)
												{
													component13.enabled = false;
												}
												dAZMesh.drawMorphedUVMappedMesh = false;
												dAZMesh.enabled = false;
												DAZSkinWrap orCreateDAZSkinWrap = GetOrCreateDAZSkinWrap(dAZMesh);
												if (orCreateDAZSkinWrap != null)
												{
													dAZSkinWrap = orCreateDAZSkinWrap;
													dAZSkinWrap.surfaceOffset = skinWrapSurfaceOffset;
													dAZSkinWrap.additionalThicknessMultiplier = skinWrapAdditionalThicknessMultiplier;
													dAZSkinWrap.smoothOuterLoops = skinWrapSmoothOuterLoops;
													orCreateDAZSkinWrap.CopyMaterials();
													if (createMaterialOptions)
													{
														CreateMaterialOptions(orCreateDAZSkinWrap.gameObject, orCreateDAZSkinWrap.numMaterials, orCreateDAZSkinWrap.materialNames, orCreateDAZSkinWrap.GPUmaterials, typeof(DAZSkinWrapMaterialOptions), MaterialFolder, Application.isPlaying);
													}
												}
											}
											else if (!shouldCreateSkins)
											{
												if (createMaterialOptions)
												{
													CreateMaterialOptions(dAZMesh.gameObject, dAZMesh.numMaterials, dAZMesh.materialNames, dAZMesh.materials, typeof(MaterialOptions), MaterialFolder, Application.isPlaying);
												}
												if (!Application.isPlaying)
												{
													dAZMesh.SaveMeshAsset();
												}
												dAZMesh.createMeshFilterAndRenderer = true;
												dAZMesh.drawMorphedUVMappedMesh = false;
												dAZMesh.createMeshCollider = true;
												if (value3)
												{
													dAZMesh.useConvexCollider = true;
												}
												else
												{
													dAZMesh.useConvexCollider = false;
												}
												dAZMesh.enabled = false;
											}
										}
									}
									else
									{
										Debug.LogError("Could not find materials for " + text7);
									}
								}
								else
								{
									Debug.LogError("Could not find geometries url");
								}
								if (dAZSkinWrap != null && !embedMeshAndSkinOnNodes)
								{
									break;
								}
							}
							if (item2["conform_target"] != null)
							{
								string sceneNodeId2 = DAZurlToId(item2["conform_target"]);
								DAZMesh dAZMeshBySceneNodeId2 = GetDAZMeshBySceneNodeId(sceneNodeId2);
								if (dAZMeshBySceneNodeId2 != null && dAZMesh != null)
								{
									dAZMesh.graftTo = dAZMeshBySceneNodeId2;
								}
							}
						}
						if (dAZSkinWrap != null && !embedMeshAndSkinOnNodes)
						{
							break;
						}
					}
					if (shouldCreateSkinsAndNodes)
					{
						foreach (JSONNode item4 in jSONNode3.AsArray)
						{
							string key3 = item4["id"];
							bool flag3 = false;
							if (item4["geometries"] != null)
							{
								flag3 = true;
							}
							bool value8 = true;
							if (sceneNodeIdToImport.ContainsKey(key3))
							{
								sceneNodeIdToImport.TryGetValue(key3, out value8);
							}
							if (flag3 && skipImportOfExisting && NodeExists(item4, flag3))
							{
								value8 = false;
							}
							if (value8)
							{
								ProcessNodeTransform(item4, flag3);
							}
						}
						foreach (JSONNode item5 in jSONNode3.AsArray)
						{
							string key4 = item5["id"];
							Transform value9;
							Transform value10;
							if (dictionary2.TryGetValue(key4, out value9) && dictionary.TryGetValue(key4, out value10))
							{
								value9.position = value10.position;
								value9.rotation = value10.rotation;
							}
						}
					}
					if (jSONNode2 != null)
					{
						foreach (JSONNode item6 in jSONNode2.AsArray)
						{
							string text8 = item6["channel"]["type"];
							if (!(text8 == "float"))
							{
								continue;
							}
							string sceneId = DAZurlToId(item6["parent"]);
							DAZMorph dAZMorph = ProcessMorph(item6["url"], sceneId);
							if (dAZMorph != null && !dAZMorph.preserveValueOnReimport)
							{
								float num = item6["channel"]["current_value"].AsFloat;
								if (num <= 0.001f && num >= -0.001f)
								{
									num = 0f;
								}
								dAZMorph.importValue = num;
								dAZMorph.morphValue = num;
							}
						}
					}
					if (container != null)
					{
						DAZBones[] componentsInChildren = container.GetComponentsInChildren<DAZBones>();
						foreach (DAZBones dAZBones in componentsInChildren)
						{
							dAZBones.Reset();
						}
					}
					DAZMesh[] components = GetComponents<DAZMesh>();
					foreach (DAZMesh dAZMesh2 in components)
					{
						dAZMesh2.ReInitMorphs();
					}
					if (wrapOnImport)
					{
						DAZSkinWrap[] components2 = GetComponents<DAZSkinWrap>();
						foreach (DAZSkinWrap dAZSkinWrap2 in components2)
						{
							if (dAZSkinWrap2.wrapStore == null || dAZSkinWrap2.wrapStore.wrapVertices == null)
							{
								dAZSkinWrap2.wrapToMorphedVertices = wrapToMorphedVertices;
								dAZSkinWrap2.Wrap();
							}
						}
					}
					DAZSkinWrap component14 = GetComponent<DAZSkinWrap>();
					if (component14 != null)
					{
						component14.draw = true;
						if (Application.isPlaying)
						{
							DAZSkinWrapControl component15 = GetComponent<DAZSkinWrapControl>();
							if (component15 != null)
							{
								component15.wrap = component14;
							}
						}
					}
					DAZMergedMesh[] components3 = GetComponents<DAZMergedMesh>();
					foreach (DAZMergedMesh dAZMergedMesh in components3)
					{
						dAZMergedMesh.ManualUpdate();
					}
					if (DAZSceneModifierOverrideDufFile != null && DAZSceneModifierOverrideDufFile != string.Empty)
					{
						JSONNode jl = ReadJSON(DAZSceneModifierOverrideDufFile);
						string url = DAZurlFix(modifierOverrideUrl);
						ProcessAltModifiers(url, jl);
					}
					if (shouldCreateSkinsAndNodes && shouldCreateSkins && container != null && jSONNode2 != null)
					{
						foreach (JSONNode item7 in jSONNode2.AsArray)
						{
							string text9 = item7["extra"][0]["type"];
							if (!(text9 == "skin_settings"))
							{
								continue;
							}
							string skinId = item7["id"];
							string skinUrl = item7["url"];
							string text10 = DAZurlToId(item7["parent"]);
							Transform transform4 = base.transform;
							if (embedMeshAndSkinOnNodes && text10 != null)
							{
								DAZMesh dAZMeshBySceneGeometryId = GetDAZMeshBySceneGeometryId(text10);
								if (dAZMeshBySceneGeometryId != null)
								{
									transform4 = dAZMeshBySceneGeometryId.transform;
								}
								else
								{
									Debug.LogWarning("Could not find DAZMesh with scene geometry ID " + text10);
									transform4 = base.transform;
								}
							}
							DAZSkinV2 orCreateDAZSkin = GetOrCreateDAZSkin(skinId, skinUrl, transform4.gameObject);
							orCreateDAZSkin.ImportStart();
							foreach (JSONNode item8 in jSONNode3.AsArray)
							{
								string url2 = DAZurlFix(item8["url"]);
								JSONNode node = GetNode(url2);
								orCreateDAZSkin.ImportNode(node, url2);
							}
							ProcessSkin(item7, transform4.gameObject);
							if (createMaterialOptions)
							{
								CreateMaterialOptions(orCreateDAZSkin.gameObject, orCreateDAZSkin.numMaterials, orCreateDAZSkin.materialNames, orCreateDAZSkin.GPUmaterials, typeof(DAZCharacterMaterialOptions), MaterialFolder, Application.isPlaying);
							}
						}
						foreach (JSONNode item9 in jSONNode2.AsArray)
						{
							string text11 = item9["extra"][0]["type"];
							if (!(text11 == "skin_settings"))
							{
								continue;
							}
							string skinId2 = item9["id"];
							string text13 = (string)item9["url"];
							string text12 = DAZurlToId(item9["parent"]);
							Transform transform5 = base.transform;
							if (embedMeshAndSkinOnNodes && text12 != null)
							{
								DAZMesh dAZMeshBySceneGeometryId2 = GetDAZMeshBySceneGeometryId(text12);
								if (dAZMeshBySceneGeometryId2 != null)
								{
									transform5 = dAZMeshBySceneGeometryId2.transform;
								}
								else
								{
									Debug.LogWarning("Could not find DAZMesh with scene geometry ID " + text12);
									transform5 = base.transform;
								}
							}
							DAZSkinV2 dAZSkin = GetDAZSkin(skinId2, transform5.gameObject);
							if (dAZSkin.dazMesh.graftTo != null)
							{
								DAZMergedMesh dAZMergedMesh2 = transform5.GetComponent<DAZMergedMesh>();
								if (dAZMergedMesh2 == null)
								{
									dAZMergedMesh2 = transform5.gameObject.AddComponent<DAZMergedMesh>();
								}
								dAZMergedMesh2.Merge();
								DAZMergedSkinV2 dAZMergedSkinV = transform5.GetComponent<DAZMergedSkinV2>();
								if (dAZMergedSkinV == null)
								{
									dAZMergedSkinV = transform5.gameObject.AddComponent<DAZMergedSkinV2>();
								}
								dAZMergedSkinV.root = dAZSkin.root;
								dAZMergedSkinV.Merge();
								dAZMergedSkinV.skin = true;
								dAZMergedSkinV.useSmoothing = true;
								dAZMergedSkinV.useSmoothVertsForNormalTangentRecalc = true;
								dAZMergedSkinV.skinMethod = DAZSkinV2.SkinMethod.CPUAndGPU;
								dAZMergedSkinV.CopyMaterials();
							}
						}
					}
					if (materialUIConnectorMaster != null)
					{
						materialUIConnectorMaster.Rebuild();
					}
				}
				catch (Exception ex4)
				{
					SuperController.LogError("Exception during DAZ import: " + ex4.ToString());
					importStatus = "Import failed " + ex4.Message;
					isImporting = false;
					callback?.Invoke();
					yield break;
				}
			}
			DAZDynamic dd = GetComponent<DAZDynamic>();
			if (dd != null)
			{
				dd.RefreshStorables();
			}
			importStatus = "Import complete";
			isImporting = false;
			callback?.Invoke();
		}

		public void ImportDufMorphs()
		{
			IEnumerator enumerator = ImportDufMorphsCo();
			while (enumerator.MoveNext())
			{
			}
		}

		public void ImportDuf()
		{
			IEnumerator enumerator = ImportDufCo();
			while (enumerator.MoveNext())
			{
			}
		}

		protected void OnEnable()
		{
			SetRegistryLibPaths();
		}
	}
}
