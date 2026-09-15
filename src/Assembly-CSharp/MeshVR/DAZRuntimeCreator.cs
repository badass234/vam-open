using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using GPUTools.Cloth.Scripts;
using GPUTools.Cloth.Scripts.Types;
using GPUTools.Hair.Scripts;
using GPUTools.Hair.Scripts.Geometry.Create;
using GPUTools.Skinner.Scripts.Providers;
using MVR.FileManagement;
using MVR.FileManagementSecure;
using UnityEngine;
using UnityEngine.UI;

namespace MeshVR
{
	public class DAZRuntimeCreator : PresetManagerControl
	{
		protected DAZImport di;

		protected DAZDynamic dd;

		public DAZImport dazImport;

		protected JSONStorableAction clearAction;

		protected JSONStorableAction cancelAction;

		protected JSONStorableAction importDufAction;

		protected string _dufStoreName;

		protected JSONStorableUrl dufFileJSON;

		protected Text importMessageText;

		protected Text importVertexCountText;

		protected JSONStorableBool combineMaterialsJSON;

		protected JSONStorableBool wrapToMorphedVerticesJSON;

		protected Text simVertexCountText;

		protected Text simJointCountText;

		protected Text simNearbyJointCountText;

		protected Texture2D clothSimTexture;

		protected RawImage clothSimTextureRawImage;

		protected JSONStorableUrl clothSimTextureFileJSON;

		protected bool clothSimUseIndividualSimTextures;

		protected JSONStorableBool clothSimUseIndividualSimTexturesJSON;

		protected JSONStorableAction setUniformClothSimTextureAction;

		protected JSONStorableFloat uniformClothSimTextureValueJSON;

		protected JSONStorableBool clothSimCreateNearbyJointsJSON;

		protected JSONStorableFloat clothSimNearbyJointsDistanceJSON;

		protected Thread processGeometryThread;

		protected ClothSettings clothSettingsForThread;

		protected string threadError;

		protected bool isGeneratingClothSim;

		protected bool abortGenerateClothSim;

		protected JSONStorableAction generateClothSimAction;

		protected JSONStorableAction cancelGenerateClothSimAction;

		public RuntimeHairGeometryCreator hairCreator;

		public string customScalpChoiceName;

		protected string startingScalpChoiceName;

		protected ObjectChoice[] scalpChoices;

		protected List<string> scalpChoiceNames;

		protected Dictionary<string, ObjectChoice> scalpChoiceNameToObjectChoice;

		protected JSONStorableStringChooser scalpChooserJSON;

		protected JSONStorableFloat segmentsJSON;

		protected JSONStorableFloat segmentsLengthJSON;

		protected bool scalpMaskEditMode;

		protected JSONStorableFloat scalpMaskSelectableSizeJSON;

		public Transform selectablePrefab;

		protected List<Selectable> createdSelectables;

		protected bool _scalpMaskEditModeHideBackfaces = true;

		protected JSONStorableBool scalpMaskEditModeHideBackfacesJSON;

		protected JSONStorableAction scalpMaskClearAllAction;

		protected JSONStorableAction scalpMaskSetAllAction;

		protected JSONStorableAction startScalpMaskEditModeAction;

		protected JSONStorableAction cancelScalpMaskEditModeAction;

		protected JSONStorableAction finishScalpMaskEditModeAction;

		protected bool isGeneratingHairSim;

		protected bool abortGenerateHairSim;

		protected JSONStorableAction generateHairSimAction;

		protected JSONStorableAction cancelGenerateHairSimAction;

		protected JSONStorableString storeFolderNameJSON;

		protected JSONStorableBool autoSetStoreFolderNameFromDufJSON;

		protected JSONStorableString packageNameJSON;

		protected JSONStorableAction clearPackageAction;

		protected JSONStorableString storeNameJSON;

		protected JSONStorableString displayNameJSON;

		protected JSONStorableBool autoSetStoreNameFromDufJSON;

		protected string _creatorName;

		protected JSONStorableString creatorNameJSON;

		protected JSONStorableString storedCreatorNameJSON;

		protected RectTransform tagsPanel;

		public Transform tagTogglePrefab;

		protected RectTransform regionTagsContent;

		protected RectTransform typeTagsContent;

		protected RectTransform otherTagsContent;

		protected static bool tagsPanelOpen;

		public string[] regionTags;

		public string[] maleTypeTags;

		public string[] femaleTypeTags;

		protected HashSet<string> otherTags = new HashSet<string>();

		protected HashSet<string> tagsSet = new HashSet<string>();

		private bool ignoreTagFromToggleCallback;

		protected Dictionary<string, Toggle> tagToToggle = new Dictionary<string, Toggle>();

		protected JSONStorableString tagsJSON;

		protected JSONStorableUrl storeBrowsePathJSON;

		protected JSONStorableAction storeAction;

		protected JSONStorableAction loadAction;

		protected Text creatorStatusText;

		protected Text creatorStatusTextAlt;

		protected RawImage thumbnailRawImage;

		protected JSONStorableAction getScreenshotAction;

		protected bool isWrapping;

		public ObjectChoice CurrentScalpChoice { get; private set; }

		protected bool dataCreatedFromImport
		{
			set
			{
				if (creatorNameJSON != null)
				{
					creatorNameJSON.interactable = !value;
				}
			}
		}

		protected void ClearParams()
		{
			dufFileJSON.val = string.Empty;
			storeBrowsePathJSON.val = string.Empty;
			storeFolderNameJSON.val = string.Empty;
			storeNameJSON.val = string.Empty;
			packageNameJSON.val = string.Empty;
			storedCreatorNameJSON.val = string.Empty;
			displayNameJSON.val = string.Empty;
			tagsJSON.val = string.Empty;
			if (clothSimTextureFileJSON != null)
			{
				clothSimTextureFileJSON.val = string.Empty;
			}
			if (importMessageText != null)
			{
				importMessageText.text = "Select DUF scene file to import";
			}
			ResetCreatorName();
		}

		protected void ClearObjects()
		{
			if (dd != null)
			{
				dd.Clear();
			}
			if (hairCreator != null)
			{
				hairCreator.Clear();
			}
			DAZMorphBank component = GetComponent<DAZMorphBank>();
			if (component != null)
			{
				UnityEngine.Object.Destroy(component);
			}
			foreach (Transform item in base.transform)
			{
				DAZMorphSubBank component2 = item.GetComponent<DAZMorphSubBank>();
				if (component2 != null)
				{
					UnityEngine.Object.Destroy(item.gameObject);
				}
			}
			if (importMessageText != null)
			{
				importMessageText.text = string.Empty;
			}
			if (importVertexCountText != null)
			{
				importVertexCountText.text = string.Empty;
			}
			if (simVertexCountText != null)
			{
				simVertexCountText.text = string.Empty;
			}
			if (simJointCountText != null)
			{
				simJointCountText.text = string.Empty;
			}
			if (simNearbyJointCountText != null)
			{
				simNearbyJointCountText.text = string.Empty;
			}
			SyncHairCountTexts();
			SyncOtherTags();
			dataCreatedFromImport = false;
		}

		protected void ClearAll()
		{
			isWrapping = false;
			StopAllCoroutines();
			AbortProcessGeometryThreaded();
			isGeneratingClothSim = false;
			ClearObjects();
			ClearParams();
			SyncOtherTags();
			SyncUI();
		}

		protected void Cancel()
		{
			ClearAll();
			SetCreatorStatus("Cancelled");
		}

		protected void ImportCallback()
		{
			SetCreatorStatus(di.importStatus);
			if (importVertexCountText != null)
			{
				DAZMesh component = di.GetComponent<DAZMesh>();
				if (component != null)
				{
					int numUVVertices = component.numUVVertices;
					importVertexCountText.text = numUVVertices.ToString();
					if (numUVVertices > 50000)
					{
						importMessageText.text = "Vertex count very high. Recommend decimation (<50000 for wrap and <25000 for sim) & reimport";
						importVertexCountText.color = Color.red;
					}
					else if (numUVVertices > 25000)
					{
						importMessageText.text = "Vertex count high. Recommend decimation (<50000 for wrap and <25000 for sim) & reimport";
						importVertexCountText.color = Color.yellow;
					}
					else
					{
						importMessageText.text = "Import complete. Vertex count in range.";
						importVertexCountText.color = Color.green;
					}
				}
			}
			SyncSimTextureLoadedHandlers();
		}

		protected void ImportDuf()
		{
			if (di != null)
			{
				ClearObjects();
				DAZCharacterRun componentInParent = GetComponentInParent<DAZCharacterRun>();
				if (componentInParent != null)
				{
					componentInParent.doSetMergedVerts = true;
				}
				if (IsHair() && customScalpChoiceName != null)
				{
					scalpChooserJSON.val = customScalpChoiceName;
				}
				StartCoroutine(di.ImportDufCo(ImportCallback));
				dataCreatedFromImport = true;
			}
		}

		protected void DufFileBeginBrowse(JSONStorableUrl jsurl)
		{
			if (!(di != null))
			{
				return;
			}
			string defaultDAZContentPath = di.GetDefaultDAZContentPath();
			List<ShortCut> list = new List<ShortCut>();
			if (defaultDAZContentPath != null && defaultDAZContentPath != string.Empty)
			{
				ShortCut shortCut = new ShortCut();
				shortCut.path = defaultDAZContentPath.Replace('/', '\\');
				shortCut.displayName = shortCut.path;
				list.Add(shortCut);
			}
			if (di.registryDAZLibraryDirectories != null)
			{
				foreach (string registryDAZLibraryDirectory in di.registryDAZLibraryDirectories)
				{
					if (registryDAZLibraryDirectory != defaultDAZContentPath)
					{
						ShortCut shortCut = new ShortCut();
						shortCut.path = registryDAZLibraryDirectory.Replace('/', '\\');
						shortCut.displayName = shortCut.path;
						list.Add(shortCut);
					}
				}
			}
			jsurl.shortCuts = list;
		}

		protected void SyncDufFile(string url)
		{
			_dufStoreName = null;
			if (url != null && url != string.Empty)
			{
				_dufStoreName = Regex.Replace(url, ".*/", string.Empty);
				_dufStoreName = Regex.Replace(_dufStoreName, "\\.duf", string.Empty);
			}
			SyncStoreFolderNameToDuf();
			SyncStoreNameToDuf();
			if (di != null)
			{
				di.DAZSceneDufFile = url;
			}
		}

		protected void SyncCombineMaterials(bool b)
		{
			if (di != null)
			{
				di.combineMaterials = b;
			}
		}

		protected void SyncWrapToMorphedVertices(bool b)
		{
			if (di != null)
			{
				di.wrapToMorphedVertices = b;
			}
		}

		public bool IsClothing()
		{
			if (dd.itemType == PresetManager.ItemType.ClothingFemale || dd.itemType == PresetManager.ItemType.ClothingMale || dd.itemType == PresetManager.ItemType.ClothingNeutral)
			{
				return true;
			}
			return false;
		}

		protected void SyncClothSimTextures(ClothSettings cs)
		{
			if (!(cs != null))
			{
				return;
			}
			cs.EditorTexture = clothSimTexture;
			if (clothSimUseIndividualSimTextures)
			{
				cs.EditorType = ClothEditorType.Provider;
			}
			else if (clothSimTexture == null)
			{
				cs.EditorType = ClothEditorType.None;
			}
			else
			{
				cs.EditorType = ClothEditorType.Texture;
			}
			if (cs.GeometryData != null)
			{
				cs.GeometryData.ResetParticlesBlend();
			}
			if (cs.builder != null)
			{
				if (cs.builder.physicsBlend != null)
				{
					cs.builder.physicsBlend.UpdateSettings();
				}
				cs.Reset();
			}
		}

		protected void SetClothSimTexture(Texture2D tex)
		{
			clothSimTexture = tex;
			ClothSettings component = GetComponent<ClothSettings>();
			SyncClothSimTextures(component);
		}

		protected void LoadSimTextureCallback(ImageLoaderThreaded.QueuedImage qi)
		{
			SetClothSimTexture(qi.tex);
		}

		protected void BeginClothSimTextureBrowse(JSONStorableUrl jsurl)
		{
			string storeFolderPath = dd.GetStoreFolderPath(false);
			if (!FileManager.IsDirectoryInPackage(storeFolderPath) && !storeFolderPath.Contains(":") && FileManager.IsSecureWritePath(storeFolderPath) && !FileManager.DirectoryExists(storeFolderPath))
			{
				FileManager.CreateDirectory(storeFolderPath);
			}
			jsurl.suggestedPath = storeFolderPath;
			jsurl.shortCuts = FileManager.GetShortCutsForDirectory(storeFolderPath);
		}

		protected void SyncClothSimTextureFile(string url)
		{
			if (FileManager.FileExists(url))
			{
				if (clothSimTextureRawImage != null)
				{
					LoadThumbnailImage(url, clothSimTextureRawImage, true);
				}
				ImageLoaderThreaded.QueuedImage queuedImage = new ImageLoaderThreaded.QueuedImage();
				queuedImage.imgPath = url;
				queuedImage.callback = LoadSimTextureCallback;
				queuedImage.forceReload = true;
				ImageLoaderThreaded.singleton.QueueImage(queuedImage);
			}
			else
			{
				SetClothSimTexture(null);
				clothSimTextureRawImage.texture = null;
			}
		}

		protected void SyncClothSimUseIndividualSimTextures(bool b)
		{
			clothSimUseIndividualSimTextures = b;
			ClothSettings component = GetComponent<ClothSettings>();
			SyncClothSimTextures(component);
		}

		protected void SyncSimTextureLoadedHandlers()
		{
			DAZSkinWrapMaterialOptions[] components = GetComponents<DAZSkinWrapMaterialOptions>();
			DAZSkinWrapMaterialOptions[] array = components;
			foreach (DAZSkinWrapMaterialOptions dAZSkinWrapMaterialOptions in array)
			{
				dAZSkinWrapMaterialOptions.simTextureLoadedHandlers = (DAZSkinWrapMaterialOptions.SimTextureLoaded)Delegate.Combine(dAZSkinWrapMaterialOptions.simTextureLoadedHandlers, new DAZSkinWrapMaterialOptions.SimTextureLoaded(IndividualSimTextureUpdated));
			}
		}

		protected void IndividualSimTextureUpdated()
		{
			if (clothSimUseIndividualSimTextures)
			{
				ClothSettings component = GetComponent<ClothSettings>();
				SyncClothSimTextures(component);
			}
		}

		protected void SetUniformClothSimTexture(float uniformVal)
		{
			int num = 4;
			int num2 = 4;
			Color color = new Color(uniformVal, 0f, 0f);
			Texture2D texture2D = new Texture2D(num, num2);
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					texture2D.SetPixel(i, j, color);
				}
			}
			texture2D.Apply();
			SetClothSimTexture(texture2D);
			clothSimTextureRawImage.texture = texture2D;
		}

		protected void SetUniformClothSimTexture()
		{
			if (uniformClothSimTextureValueJSON != null)
			{
				SetUniformClothSimTexture(uniformClothSimTextureValueJSON.val);
			}
		}

		protected void SyncClothSimCreateNearbyJoints(bool b)
		{
			ClothSettings component = GetComponent<ClothSettings>();
			if (component != null)
			{
				component.CreateNearbyJoints = b;
			}
		}

		protected void SyncClothSimNearbyJointsDistance(float f)
		{
			ClothSettings component = GetComponent<ClothSettings>();
			if (component != null)
			{
				component.NearbyJointsMaxDistance = f;
			}
		}

		protected void AbortProcessGeometryThreaded(bool wait = true)
		{
			if (processGeometryThread != null && processGeometryThread.IsAlive)
			{
				clothSettingsForThread.CancelProcessGeometryThreaded();
				if (wait)
				{
					while (processGeometryThread.IsAlive)
					{
						Thread.Sleep(0);
					}
				}
			}
			abortGenerateClothSim = true;
		}

		protected void ProcessGeometryThreaded()
		{
			try
			{
				clothSettingsForThread.ProcessGeometryThreaded();
			}
			catch (ThreadAbortException ex)
			{
				Debug.LogError("Thread aborted " + ex);
			}
			catch (Exception ex2)
			{
				threadError = "Exception on thread while generating sim data " + ex2;
			}
		}

		protected IEnumerator GenerateClothSimCo(ClothSettings cs)
		{
			yield return null;
			if (cs != null)
			{
				if (simVertexCountText != null)
				{
					simVertexCountText.text = string.Empty;
				}
				if (simJointCountText != null)
				{
					simJointCountText.text = string.Empty;
				}
				if (simNearbyJointCountText != null)
				{
					simNearbyJointCountText.text = string.Empty;
				}
				SetCreatorStatus("Process Main Thread Sim Data Creation");
				cs.ProcessGeometryMainThread();
				SetCreatorStatus("Starting Sim Data Creation Thread");
				clothSettingsForThread = cs;
				threadError = null;
				processGeometryThread = new Thread(ProcessGeometryThreaded);
				processGeometryThread.Start();
				if (abortGenerateClothSim)
				{
					SetCreatorStatus("Sim Data Creation Aborted");
					isGeneratingClothSim = false;
					yield break;
				}
				while (processGeometryThread.IsAlive)
				{
					if (cs.GeometryData != null && cs.GeometryData.status != null && cs.GeometryData.status != string.Empty)
					{
						SetCreatorStatus(cs.GeometryData.status);
					}
					yield return null;
				}
				if (threadError != null)
				{
					SetCreatorStatus(threadError);
					isGeneratingClothSim = false;
					yield break;
				}
				if (abortGenerateClothSim)
				{
					SetCreatorStatus("Sim Data Creation Aborted");
					isGeneratingClothSim = false;
					yield break;
				}
				if (cs.GeometryData != null && cs.GeometryData.Particles != null)
				{
					if (simVertexCountText != null)
					{
						int num = cs.GeometryData.Particles.Length;
						simVertexCountText.text = num.ToString();
						if (num > 25000)
						{
							simVertexCountText.color = Color.red;
						}
						else
						{
							simVertexCountText.color = Color.green;
						}
					}
					if (simJointCountText != null)
					{
						int num2 = cs.GeometryData.StiffnessJointGroups.Sum((Int2ListContainer container) => container.List.Count);
						simJointCountText.text = num2.ToString();
					}
					if (simNearbyJointCountText != null)
					{
						int num3 = cs.GeometryData.NearbyJointGroups.Sum((Int2ListContainer container) => container.List.Count);
						simNearbyJointCountText.text = num3.ToString();
					}
					SetCreatorStatus("Sim Data Creation Finished");
				}
				else
				{
					SetCreatorStatus("Sim Data Creation Failed");
				}
				SyncClothSimTextures(cs);
			}
			isGeneratingClothSim = false;
		}

		protected void GenerateClothSim()
		{
			if (!isGeneratingClothSim && dd != null)
			{
				isGeneratingClothSim = true;
				abortGenerateClothSim = false;
				ClothSettings clothSettings = dd.CreateNewClothSettings();
				clothSettings.CreateNearbyJoints = clothSimCreateNearbyJointsJSON.val;
				clothSettings.NearbyJointsMaxDistance = clothSimNearbyJointsDistanceJSON.val;
				StartCoroutine(GenerateClothSimCo(clothSettings));
			}
		}

		protected void CancelGenerateClothSim()
		{
			AbortProcessGeometryThreaded();
		}

		public bool IsHair()
		{
			if (hairCreator != null)
			{
				return true;
			}
			return false;
		}

		protected void SyncHairCountTexts()
		{
			if (!IsHair())
			{
				return;
			}
			if (simVertexCountText != null)
			{
				int[] hairRootToScalpMap = hairCreator.GetHairRootToScalpMap();
				if (hairRootToScalpMap != null)
				{
					simVertexCountText.text = hairRootToScalpMap.Length.ToString();
				}
				else
				{
					simVertexCountText.text = "0";
				}
			}
			if (simJointCountText != null)
			{
				List<Vector3> vertices = hairCreator.GetVertices();
				if (vertices != null)
				{
					simJointCountText.text = vertices.Count.ToString();
				}
				else
				{
					simJointCountText.text = "0";
				}
			}
			HairSimControl component = GetComponent<HairSimControl>();
			if (component != null)
			{
				component.SyncStyleText();
			}
		}

		public void Rebuild()
		{
			HairSettings component = GetComponent<HairSettings>();
			if (component != null && component.HairBuidCommand != null)
			{
				component.HairBuidCommand.RebuildHair();
			}
		}

		protected void InitScalpChoices()
		{
			scalpChoiceNames = new List<string>();
			scalpChoices = GetComponentsInChildren<ObjectChoice>(true);
			scalpChoiceNameToObjectChoice = new Dictionary<string, ObjectChoice>();
			ObjectChoice[] array = scalpChoices;
			foreach (ObjectChoice objectChoice in array)
			{
				scalpChoiceNames.Add(objectChoice.displayName);
				scalpChoiceNameToObjectChoice.Add(objectChoice.displayName, objectChoice);
				if (objectChoice.gameObject.activeSelf)
				{
					CurrentScalpChoice = objectChoice;
					startingScalpChoiceName = CurrentScalpChoice.displayName;
				}
			}
		}

		protected ObjectChoice SyncScalpChoiceGameObject(string s)
		{
			ObjectChoice value;
			if (scalpChoiceNameToObjectChoice.TryGetValue(s, out value))
			{
				if (CurrentScalpChoice != null)
				{
					CurrentScalpChoice.gameObject.SetActive(false);
				}
				CurrentScalpChoice = value;
				value.gameObject.SetActive(false);
				value.gameObject.SetActive(true);
				return value;
			}
			return null;
		}

		protected void SyncScalpChoice(string s)
		{
			ObjectChoice objectChoice = SyncScalpChoiceGameObject(s);
			if (objectChoice != null)
			{
				PreCalcMeshProvider component = objectChoice.GetComponent<PreCalcMeshProvider>();
				hairCreator.ScalpProvider = component;
				Rebuild();
			}
		}

		protected void SyncSegments(float f)
		{
			if (IsHair())
			{
				hairCreator.Segments = (int)f;
				Rebuild();
			}
		}

		protected void SyncSegmentsLength(float f)
		{
			if (IsHair())
			{
				hairCreator.SegmentLength = f;
			}
		}

		protected void SyncScalpMaskSelectableSize(float f)
		{
			if (createdSelectables == null)
			{
				return;
			}
			foreach (Selectable createdSelectable in createdSelectables)
			{
				GameObject gameObject = createdSelectable.gameObject;
				gameObject.transform.localScale = Vector3.one * f;
			}
		}

		protected void ClearSelectables()
		{
			if (createdSelectables == null)
			{
				return;
			}
			Selector.RemoveAll();
			foreach (Selectable createdSelectable in createdSelectables)
			{
				GameObject obj = createdSelectable.gameObject;
				UnityEngine.Object.Destroy(obj);
			}
			Selector.hideBackfaces = false;
			createdSelectables = null;
		}

		protected void SelectionChanged(int uid, bool b)
		{
			if (hairCreator != null)
			{
				hairCreator.strandsMaskWorking.vertices[uid] = !b;
			}
		}

		protected void SyncSelectables()
		{
			if (!IsHair() || createdSelectables == null)
			{
				return;
			}
			bool[] vertices = hairCreator.strandsMaskWorking.vertices;
			foreach (Selectable createdSelectable in createdSelectables)
			{
				createdSelectable.isSelected = !vertices[createdSelectable.id];
			}
		}

		protected void CreateSelectables()
		{
			if (!IsHair() || !(selectablePrefab != null))
			{
				return;
			}
			createdSelectables = new List<Selectable>();
			bool[] vertices = hairCreator.strandsMaskWorking.vertices;
			bool[] enabledIndices = hairCreator.enabledIndices;
			Vector3[] vertices2 = hairCreator.ScalpProvider.Mesh.vertices;
			Vector3[] normals = hairCreator.ScalpProvider.Mesh.normals;
			for (int i = 0; i < vertices.Length; i++)
			{
				if (enabledIndices[i])
				{
					Transform transform = UnityEngine.Object.Instantiate(selectablePrefab);
					transform.localScale = Vector3.one * scalpMaskSelectableSizeJSON.val;
					Selectable component = transform.GetComponent<Selectable>();
					if (component != null)
					{
						createdSelectables.Add(component);
						component.id = i;
						component.isSelected = !vertices[i];
						component.selectionChanged = (Selectable.SelectionChanged)Delegate.Combine(component.selectionChanged, new Selectable.SelectionChanged(SelectionChanged));
					}
					transform.position = vertices2[i];
					transform.LookAt(transform.position + normals[i]);
				}
			}
			Selector.hideBackfaces = _scalpMaskEditModeHideBackfaces;
		}

		protected void SyncScalpMaskEditModeHideBackfaces(bool b)
		{
			_scalpMaskEditModeHideBackfaces = b;
			if (scalpMaskEditMode)
			{
				Selector.hideBackfaces = _scalpMaskEditModeHideBackfaces;
			}
		}

		protected void SetHairRenderEnabled(bool b)
		{
			HairSettings component = GetComponent<HairSettings>();
			if (component != null && component.HairBuidCommand != null && component.HairBuidCommand.render != null)
			{
				MeshRenderer component2 = component.HairBuidCommand.render.GetComponent<MeshRenderer>();
				if (component2 != null)
				{
					component2.enabled = b;
				}
			}
		}

		protected void SyncScalpMaskButtons()
		{
			if (startScalpMaskEditModeAction.button != null)
			{
				startScalpMaskEditModeAction.button.interactable = !scalpMaskEditMode;
			}
			if (cancelScalpMaskEditModeAction.button != null)
			{
				cancelScalpMaskEditModeAction.button.interactable = scalpMaskEditMode;
			}
			if (finishScalpMaskEditModeAction.button != null)
			{
				finishScalpMaskEditModeAction.button.interactable = scalpMaskEditMode;
			}
			if (scalpMaskClearAllAction.button != null)
			{
				scalpMaskClearAllAction.button.interactable = scalpMaskEditMode;
			}
			if (scalpMaskSetAllAction.button != null)
			{
				scalpMaskSetAllAction.button.interactable = scalpMaskEditMode;
			}
		}

		protected void SyncScalpMaskTool()
		{
			HairSimControlTools componentInParent = GetComponentInParent<HairSimControlTools>();
			if (componentInParent != null)
			{
				componentInParent.SetScalpMaskToolVisible(scalpMaskEditMode);
				componentInParent.SetOnlyToolsControllable(scalpMaskEditMode);
			}
		}

		public void ScalpMaskClearAll()
		{
			if (IsHair())
			{
				hairCreator.MaskClearAll();
				SyncSelectables();
			}
		}

		public void ScalpMaskSetAll()
		{
			if (IsHair())
			{
				hairCreator.MaskSetAll();
				SyncSelectables();
			}
		}

		public void StartScalpMaskEditMode()
		{
			if (IsHair())
			{
				scalpMaskEditMode = true;
				SyncScalpMaskButtons();
				SyncScalpMaskTool();
				SetHairRenderEnabled(false);
				DAZSkinV2.staticDraw = true;
				DAZSkinWrap.staticDraw = true;
				ClearSelectables();
				hairCreator.SetWorkingMaskToCurrentMask();
				CreateSelectables();
				Selector.Activate();
				HairSimControl component = GetComponent<HairSimControl>();
				if (component != null)
				{
					component.CancelStyleMode();
				}
				if (SuperController.singleton != null)
				{
					SuperController.singleton.SelectModeCustomWithVRTargetControl("Select scalp vertices that will have hair strands. White = Has Strand. Red = No Strand. Mouse: Click - Toggles vertex. Click+Drag - Add. LeftCtrl+Click+Drag - Remove. VR: Grab Mask Tool, Select Mode, and Move Over Scalp");
				}
			}
		}

		public void CancelScalpMaskEditMode()
		{
			if (IsHair())
			{
				ClearSelectables();
				Selector.Deactivate();
				scalpMaskEditMode = false;
				SyncScalpMaskButtons();
				SyncScalpMaskTool();
				SetHairRenderEnabled(true);
				DAZSkinV2.staticDraw = false;
				DAZSkinWrap.staticDraw = false;
				if (SuperController.singleton != null)
				{
					SuperController.singleton.SelectModeOff();
				}
			}
		}

		public void FinishScalpMaskEditMode()
		{
			if (IsHair())
			{
				ClearSelectables();
				Selector.Deactivate();
				scalpMaskEditMode = false;
				SyncScalpMaskButtons();
				SyncScalpMaskTool();
				SetHairRenderEnabled(true);
				DAZSkinV2.staticDraw = false;
				DAZSkinWrap.staticDraw = false;
				hairCreator.ApplyMaskChanges();
				HairSimControl component = GetComponent<HairSimControl>();
				if (component != null)
				{
					component.ClearStyleJoints();
				}
				Rebuild();
				if (SuperController.singleton != null)
				{
					SuperController.singleton.SelectModeOff();
				}
			}
		}

		protected void GenerateHairSim()
		{
			if (!isGeneratingHairSim && dd != null)
			{
				abortGenerateHairSim = false;
				hairCreator.ClearAllStrands();
				hairCreator.GenerateAll();
				SyncHairCountTexts();
				Rebuild();
				HairSimControl component = GetComponent<HairSimControl>();
				if (component != null)
				{
					component.StartStyleMode();
				}
			}
		}

		protected void CancelGenerateHairSim()
		{
		}

		protected void SyncDazImportMaterialFolder()
		{
			if (di != null)
			{
				string text = string.Empty;
				if (creatorNameJSON != null && creatorNameJSON.val != null && creatorNameJSON.val != string.Empty)
				{
					text = text + creatorNameJSON.val + "/";
				}
				if (storeFolderNameJSON != null && storeFolderNameJSON.val != null && storeFolderNameJSON.val != string.Empty)
				{
					text += storeFolderNameJSON.val;
				}
				if (text != string.Empty)
				{
					di.MaterialOverrideFolderName = text;
					di.overrideMaterialFolderName = true;
				}
				else
				{
					di.overrideMaterialFolderName = false;
				}
			}
		}

		protected void SyncStoreFolderNameToDuf()
		{
			if (autoSetStoreFolderNameFromDufJSON.val && _dufStoreName != null && _dufStoreName != string.Empty)
			{
				storeFolderNameJSON.val = _dufStoreName;
			}
		}

		protected void SyncStoreFolderName(string sname)
		{
			SyncDazImportMaterialFolder();
			ClearPackage();
			if (dd != null)
			{
				dd.storeFolderName = sname;
				dd.SyncMaterialOptions();
			}
			SyncPresetUI();
			SyncStoreFolderNameToDuf();
			SyncUI();
		}

		protected void SyncAutoSetStoreFolderNameFromDuf(bool b)
		{
			SyncStoreFolderNameToDuf();
		}

		protected void SyncUID()
		{
			if (dd != null)
			{
				dd.uid = dd.creatorName + ":" + dd.storeName;
			}
		}

		protected void SyncPackageName(string pname)
		{
			if (dd != null)
			{
				dd.package = pname;
				dd.SyncMaterialOptions();
			}
			SyncPresetUI();
			SyncUI();
		}

		protected void ClearPackage()
		{
			packageNameJSON.val = string.Empty;
		}

		protected void SyncStoreNameToDuf()
		{
			if (autoSetStoreNameFromDufJSON.val && _dufStoreName != null && _dufStoreName != string.Empty)
			{
				storeNameJSON.val = _dufStoreName;
			}
		}

		protected void SyncStoreName(string sname)
		{
			if (dd != null)
			{
				dd.storeName = sname;
			}
			SyncUID();
			SyncPresetUI();
			SyncStoreNameToDuf();
			SyncUI();
		}

		protected void SyncDisplayName(string dname)
		{
			if (dd != null)
			{
				dd.displayName = dname;
			}
		}

		protected void SyncAutoSetStoreNameFromDuf(bool b)
		{
			SyncStoreNameToDuf();
		}

		protected void ResetCreatorName()
		{
			string text = "Anonymous";
			if (UserPreferences.singleton != null)
			{
				text = UserPreferences.singleton.creatorName;
			}
			if (creatorNameJSON != null)
			{
				creatorNameJSON.val = text;
			}
			else
			{
				SyncCreatorName(text);
			}
		}

		protected void SyncCreatorName(string s)
		{
			_creatorName = s;
			SyncDazImportMaterialFolder();
			if (dd != null)
			{
				dd.creatorName = s;
			}
			SyncUID();
			SyncUI();
		}

		protected void OpenTagsPanel()
		{
			if (tagsPanel != null)
			{
				tagsPanelOpen = true;
				tagsPanel.gameObject.SetActive(true);
			}
		}

		protected void CloseTagsPanel()
		{
			if (tagsPanel != null)
			{
				tagsPanelOpen = false;
				tagsPanel.gameObject.SetActive(false);
			}
		}

		protected void SyncOtherTags()
		{
			DAZClothingItemControl component = GetComponent<DAZClothingItemControl>();
			if (component != null)
			{
				otherTags = component.GetAllClothingOtherTags();
			}
			else
			{
				DAZHairGroupControl component2 = GetComponent<DAZHairGroupControl>();
				if (component2 != null)
				{
					otherTags = component2.GetAllHairOtherTags();
				}
			}
			SyncOtherTagsUI();
		}

		protected void SyncTagsSetToTags()
		{
			if (tagsJSON != null && tagsJSON.val != string.Empty)
			{
				string[] collection = tagsJSON.val.Split(',');
				tagsSet = new HashSet<string>(collection);
			}
			else
			{
				tagsSet = new HashSet<string>();
			}
		}

		protected void SyncTagsToTagsSet()
		{
			string[] array = new string[tagsSet.Count];
			tagsSet.CopyTo(array);
			tagsJSON.valNoCallback = string.Join(",", array);
			SyncDDTagsToJSON();
		}

		protected void SyncTagFromToggle(string tag, bool isEnabled)
		{
			if (!ignoreTagFromToggleCallback)
			{
				if (isEnabled)
				{
					tagsSet.Add(tag);
				}
				else
				{
					tagsSet.Remove(tag);
				}
				SyncTagsToTagsSet();
			}
		}

		protected void SyncTagTogglesToTags()
		{
			ignoreTagFromToggleCallback = true;
			foreach (KeyValuePair<string, Toggle> item in tagToToggle)
			{
				if (tagsSet.Contains(item.Key))
				{
					if (item.Value != null)
					{
						item.Value.isOn = true;
					}
				}
				else if (item.Value != null)
				{
					item.Value.isOn = false;
				}
			}
			ignoreTagFromToggleCallback = false;
		}

		protected void CreateTagToggle(string tag, Transform parent)
		{
			Transform transform = UnityEngine.Object.Instantiate(tagTogglePrefab);
			Text componentInChildren = transform.GetComponentInChildren<Text>();
			componentInChildren.text = tag;
			Toggle componentInChildren2 = transform.GetComponentInChildren<Toggle>();
			componentInChildren2.onValueChanged.AddListener(delegate(bool b)
			{
				SyncTagFromToggle(tag, b);
			});
			tagToToggle.Remove(tag);
			tagToToggle.Add(tag, componentInChildren2);
			transform.SetParent(parent, false);
		}

		protected void SyncOtherTagsUI()
		{
			if (!(tagTogglePrefab != null) || !(otherTagsContent != null))
			{
				return;
			}
			foreach (Transform item in otherTagsContent)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
			List<string> list = otherTags.ToList();
			list.Sort();
			foreach (string item2 in list)
			{
				CreateTagToggle(item2, otherTagsContent);
			}
			SyncTagTogglesToTags();
		}

		protected void InitTagsUI()
		{
			tagToToggle = new Dictionary<string, Toggle>();
			if (tagTogglePrefab != null)
			{
				if (regionTags != null && regionTagsContent != null)
				{
					List<string> list = new List<string>(regionTags);
					list.Sort();
					foreach (string item in list)
					{
						CreateTagToggle(item, regionTagsContent);
					}
				}
				if (pm != null)
				{
					if (pm.itemType == PresetManager.ItemType.ClothingFemale)
					{
						if (femaleTypeTags != null && typeTagsContent != null)
						{
							List<string> list2 = new List<string>(femaleTypeTags);
							list2.Sort();
							foreach (string item2 in list2)
							{
								CreateTagToggle(item2, typeTagsContent);
							}
						}
					}
					else if (pm.itemType == PresetManager.ItemType.ClothingMale && maleTypeTags != null && typeTagsContent != null)
					{
						List<string> list3 = new List<string>(maleTypeTags);
						list3.Sort();
						foreach (string item3 in list3)
						{
							CreateTagToggle(item3, typeTagsContent);
						}
					}
				}
				SyncOtherTagsUI();
			}
			SyncTagTogglesToTags();
		}

		protected void SyncDDTagsToJSON()
		{
			if (dd != null)
			{
				dd.tags = tagsJSON.val;
			}
		}

		protected void SyncTagsFromJSON(string tags)
		{
			string input = tags.Trim();
			input = Regex.Replace(input, ",\\s+", ",");
			input = Regex.Replace(input, "\\s+,", ",");
			if (input != tags)
			{
				tagsJSON.valNoCallback = input;
			}
			SyncTagsSetToTags();
			SyncTagTogglesToTags();
			SyncDDTagsToJSON();
		}

		protected void SyncBrowsePath(string url)
		{
			if (url != null && url != string.Empty)
			{
				string[] array = dd.PathToNames(url);
				storeFolderNameJSON.val = array[0];
				creatorNameJSON.val = array[1];
				storeNameJSON.val = array[2];
				packageNameJSON.val = array[3];
				Load();
			}
		}

		public void LoadFromPath(string path)
		{
			storeBrowsePathJSON.val = path;
		}

		protected void BeginBrowseCreator(JSONStorableUrl jsurl)
		{
			if (pm != null)
			{
				string storeRootPath = dd.GetStoreRootPath(false);
				string input = storeRootPath;
				input = Regex.Replace(input, "/$", string.Empty);
				jsurl.suggestedPath = input;
				List<ShortCut> shortCutsForDirectory = FileManager.GetShortCutsForDirectory(storeRootPath, false, false, true, true);
				jsurl.shortCuts = shortCutsForDirectory;
			}
		}

		protected void Store()
		{
			if (!(dd != null))
			{
				return;
			}
			bool flag = dd.CheckStoreExistance();
			DAZClothingItemControl component = GetComponent<DAZClothingItemControl>();
			DAZHairGroupControl component2 = GetComponent<DAZHairGroupControl>();
			bool flag2 = true;
			if (!flag)
			{
				if (component != null)
				{
					flag2 = component.IsClothingUIDAvailable(dd.uid);
				}
				else if (component2 != null)
				{
					flag2 = component2.IsHairItemUIDAvailable(dd.uid);
				}
			}
			if (flag2)
			{
				dd.isRealItem = true;
				if (component != null && component.clothingItem != null)
				{
					dd.isRealItem = component.clothingItem.isRealItem;
				}
				if (component2 != null && component2.hairItem != null)
				{
					dd.isRealItem = component2.hairItem.isRealItem;
				}
				if (dd.Store())
				{
					if (storedCreatorNameJSON != null)
					{
						storedCreatorNameJSON.val = dd.storedCreatorName;
					}
					SetCreatorStatus("Store to " + dd.storeName + " complete");
					SyncPresetUI();
					if (component != null)
					{
						component.RefreshClothingItems();
					}
					else if (component2 != null)
					{
						component2.RefreshHairItems();
					}
					SyncOtherTags();
				}
				else
				{
					string text = "Store to " + dd.storeName + " failed";
					SuperController.LogError(text);
					SetCreatorStatus(text);
				}
			}
			else
			{
				string text2 = "Store to " + dd.storeName + " failed. UID " + dd.uid + " is already being used";
				SuperController.LogError(text2);
				SetCreatorStatus(text2);
			}
			SyncUI();
		}

		private IEnumerator LoadDelay()
		{
			yield return null;
			DAZClothingItemControl itemControl = GetComponent<DAZClothingItemControl>();
			if (itemControl != null)
			{
				itemControl.ResetIsRealClothingItem();
			}
			if (dd.Load(true))
			{
				if (storedCreatorNameJSON != null)
				{
					storedCreatorNameJSON.val = dd.storedCreatorName;
				}
				if (displayNameJSON != null)
				{
					displayNameJSON.valNoCallback = dd.displayName;
				}
				if (tagsJSON != null)
				{
					tagsJSON.valNoCallback = dd.tags;
					SyncTagsSetToTags();
					SyncTagTogglesToTags();
				}
				if (di != null && di.materialUIConnectorMaster != null)
				{
					di.materialUIConnectorMaster.Rebuild();
				}
				if (IsHair())
				{
					segmentsJSON.valNoCallback = hairCreator.Segments;
					segmentsLengthJSON.valNoCallback = hairCreator.SegmentLength;
					scalpChooserJSON.valNoCallback = hairCreator.ScalpProviderName;
					SyncScalpChoiceGameObject(hairCreator.ScalpProviderName);
				}
				LoadThumbnailImage(dd.GetStorePathBase() + ".jpg");
				SetCreatorStatus("Load from " + dd.storeName + " complete");
				SyncSimTextureLoadedHandlers();
			}
			else
			{
				ClearObjects();
				SetCreatorStatus("Load from " + dd.storeName + " failed");
			}
			yield return null;
		}

		protected void Load()
		{
			if (dd != null)
			{
				ClearObjects();
				StartCoroutine(LoadDelay());
			}
		}

		protected virtual void SetCreatorStatus(string status)
		{
			if (creatorStatusText != null)
			{
				creatorStatusText.text = status;
			}
			if (creatorStatusTextAlt != null)
			{
				creatorStatusTextAlt.text = status;
			}
		}

		protected void LoadThumbnailImage(string imgPath, RawImage rawImage, bool forceReload = false)
		{
			if (rawImage != null && FileManager.FileExists(imgPath))
			{
				ImageLoaderThreaded.QueuedImage queuedImage = new ImageLoaderThreaded.QueuedImage();
				queuedImage.imgPath = imgPath;
				queuedImage.width = 512;
				queuedImage.height = 512;
				queuedImage.setSize = true;
				queuedImage.fillBackground = true;
				queuedImage.forceReload = forceReload;
				queuedImage.rawImageToLoad = rawImage;
				ImageLoaderThreaded.singleton.QueueThumbnail(queuedImage);
			}
			DAZClothingItemControl component = GetComponent<DAZClothingItemControl>();
			if (component != null)
			{
				component.RefreshClothingItemThumbnail(imgPath);
				return;
			}
			DAZHairGroupControl component2 = GetComponent<DAZHairGroupControl>();
			if (component2 != null)
			{
				component2.RefreshHairItemThumbnail(imgPath);
			}
		}

		protected void LoadThumbnailImage(string imgPath)
		{
			LoadThumbnailImage(imgPath, thumbnailRawImage);
		}

		protected void GetScreenshot()
		{
			if (dd != null)
			{
				dd.GetScreenshot(LoadThumbnailImage);
			}
		}

		protected override void Init()
		{
			base.Init();
			if (dazImport != null)
			{
				di = dazImport;
			}
			else
			{
				di = GetComponent<DAZImport>();
			}
			dd = GetComponent<DAZDynamic>();
			dd.ignoreExclude = true;
			string suggestPath = null;
			if (di != null)
			{
				di.SetRegistryLibPaths();
				suggestPath = di.GetDefaultDAZContentPath();
				if (IsHair())
				{
					di.combineToSingleMaterial = true;
				}
				else
				{
					di.combineMaterials = true;
				}
			}
			dufFileJSON = new JSONStorableUrl("dufFile", string.Empty, SyncDufFile, "duf", suggestPath);
			dufFileJSON.suggestedPathGroup = "DAZRuntimeCreator";
			dufFileJSON.beginBrowseWithObjectCallback = DufFileBeginBrowse;
			dufFileJSON.isStorable = false;
			dufFileJSON.isRestorable = false;
			RegisterUrl(dufFileJSON);
			clearAction = new JSONStorableAction("Clear", ClearAll);
			RegisterAction(clearAction);
			cancelAction = new JSONStorableAction("Cancel", Cancel);
			RegisterAction(cancelAction);
			combineMaterialsJSON = new JSONStorableBool("combineMaterials", true, SyncCombineMaterials);
			combineMaterialsJSON.isStorable = false;
			combineMaterialsJSON.isRestorable = false;
			RegisterBool(combineMaterialsJSON);
			wrapToMorphedVerticesJSON = new JSONStorableBool("wrapToMorphedVertices", false, SyncWrapToMorphedVertices);
			wrapToMorphedVerticesJSON.isStorable = false;
			wrapToMorphedVerticesJSON.isRestorable = false;
			RegisterBool(wrapToMorphedVerticesJSON);
			importDufAction = new JSONStorableAction("Import", ImportDuf);
			RegisterAction(importDufAction);
			if (IsClothing())
			{
				generateClothSimAction = new JSONStorableAction("CreateClothSim", GenerateClothSim);
				RegisterAction(generateClothSimAction);
				cancelGenerateClothSimAction = new JSONStorableAction("CancelCreateClothSim", CancelGenerateClothSim);
				RegisterAction(cancelGenerateClothSimAction);
				uniformClothSimTextureValueJSON = new JSONStorableFloat("uniformClothSimTextureValue", 0.95f, 0f, 1f);
				uniformClothSimTextureValueJSON.isStorable = false;
				uniformClothSimTextureValueJSON.isRestorable = false;
				RegisterFloat(uniformClothSimTextureValueJSON);
				setUniformClothSimTextureAction = new JSONStorableAction("SetUniformTexture", SetUniformClothSimTexture);
				RegisterAction(setUniformClothSimTextureAction);
				clothSimUseIndividualSimTexturesJSON = new JSONStorableBool("clothSimUseIndividualSimTextures", clothSimUseIndividualSimTextures, SyncClothSimUseIndividualSimTextures);
				clothSimUseIndividualSimTexturesJSON.isStorable = false;
				clothSimUseIndividualSimTexturesJSON.isRestorable = false;
				RegisterBool(clothSimUseIndividualSimTexturesJSON);
				clothSimCreateNearbyJointsJSON = new JSONStorableBool("clothSimCreateNearbyJoints", false, SyncClothSimCreateNearbyJoints);
				clothSimCreateNearbyJointsJSON.isStorable = false;
				clothSimCreateNearbyJointsJSON.isRestorable = false;
				RegisterBool(clothSimCreateNearbyJointsJSON);
				clothSimNearbyJointsDistanceJSON = new JSONStorableFloat("clothSimNearbyJointsDistance", 0.002f, SyncClothSimNearbyJointsDistance, 0.0001f, 0.01f);
				clothSimNearbyJointsDistanceJSON.isStorable = false;
				clothSimNearbyJointsDistanceJSON.isRestorable = false;
				RegisterFloat(clothSimNearbyJointsDistanceJSON);
			}
			if (IsHair())
			{
				InitScalpChoices();
				scalpChooserJSON = new JSONStorableStringChooser("scalpChoice", scalpChoiceNames, startingScalpChoiceName, "Scalp Choice", SyncScalpChoice);
				scalpChooserJSON.isStorable = false;
				scalpChooserJSON.isRestorable = false;
				RegisterStringChooser(scalpChooserJSON);
				generateHairSimAction = new JSONStorableAction("CreateHairSim", GenerateHairSim);
				RegisterAction(generateHairSimAction);
				cancelGenerateHairSimAction = new JSONStorableAction("CancelCreateHairSim", CancelGenerateHairSim);
				RegisterAction(cancelGenerateHairSimAction);
				segmentsJSON = new JSONStorableFloat("segments", hairCreator.Segments, SyncSegments, 2f, 50f);
				segmentsJSON.isStorable = false;
				segmentsJSON.isRestorable = false;
				RegisterFloat(segmentsJSON);
				segmentsLengthJSON = new JSONStorableFloat("segmentsLength", hairCreator.SegmentLength, SyncSegmentsLength, 0.0001f, 0.03f);
				segmentsLengthJSON.isStorable = false;
				segmentsLengthJSON.isRestorable = false;
				RegisterFloat(segmentsLengthJSON);
				scalpMaskSelectableSizeJSON = new JSONStorableFloat("scalpMaskSelectableSize", 0.002f, SyncScalpMaskSelectableSize, 0.0002f, 0.004f);
				scalpMaskSelectableSizeJSON.isStorable = false;
				scalpMaskSelectableSizeJSON.isRestorable = false;
				RegisterFloat(scalpMaskSelectableSizeJSON);
				scalpMaskClearAllAction = new JSONStorableAction("ScalpMaskClearAll", ScalpMaskClearAll);
				RegisterAction(scalpMaskClearAllAction);
				scalpMaskSetAllAction = new JSONStorableAction("ScalpMaskSetAll", ScalpMaskSetAll);
				RegisterAction(scalpMaskSetAllAction);
				startScalpMaskEditModeAction = new JSONStorableAction("StartScalpMaskEditMode", StartScalpMaskEditMode);
				RegisterAction(startScalpMaskEditModeAction);
				cancelScalpMaskEditModeAction = new JSONStorableAction("CancelScalpMaskEditMode", CancelScalpMaskEditMode);
				RegisterAction(cancelScalpMaskEditModeAction);
				finishScalpMaskEditModeAction = new JSONStorableAction("FinishScalpMaskEditMode", FinishScalpMaskEditMode);
				RegisterAction(finishScalpMaskEditModeAction);
				scalpMaskEditModeHideBackfacesJSON = new JSONStorableBool("ScalpMaskEditModeHideBackfaces", _scalpMaskEditModeHideBackfaces, SyncScalpMaskEditModeHideBackfaces);
				scalpMaskEditModeHideBackfacesJSON.isStorable = false;
				scalpMaskEditModeHideBackfacesJSON.isRestorable = false;
				RegisterBool(scalpMaskEditModeHideBackfacesJSON);
			}
			autoSetStoreFolderNameFromDufJSON = new JSONStorableBool("autoSetStoreFolderNameFromDuf", true, SyncAutoSetStoreFolderNameFromDuf);
			autoSetStoreFolderNameFromDufJSON.isStorable = false;
			autoSetStoreFolderNameFromDufJSON.isRestorable = false;
			RegisterBool(autoSetStoreFolderNameFromDufJSON);
			storeFolderNameJSON = new JSONStorableString("storeFolderName", string.Empty, SyncStoreFolderName);
			storeFolderNameJSON.isStorable = false;
			storeFolderNameJSON.isRestorable = false;
			storeFolderNameJSON.enableOnChange = true;
			RegisterString(storeFolderNameJSON);
			autoSetStoreNameFromDufJSON = new JSONStorableBool("autoSetStoreNameFromDuf", true, SyncAutoSetStoreNameFromDuf);
			autoSetStoreNameFromDufJSON.isStorable = false;
			autoSetStoreNameFromDufJSON.isRestorable = false;
			RegisterBool(autoSetStoreNameFromDufJSON);
			packageNameJSON = new JSONStorableString("packageName", string.Empty, SyncPackageName);
			packageNameJSON.isStorable = false;
			packageNameJSON.isRestorable = false;
			RegisterString(packageNameJSON);
			clearPackageAction = new JSONStorableAction("ClearPackage", ClearPackage);
			RegisterAction(clearPackageAction);
			storeNameJSON = new JSONStorableString("storeName", string.Empty, SyncStoreName);
			storeNameJSON.isStorable = false;
			storeNameJSON.isRestorable = false;
			storeNameJSON.enableOnChange = true;
			RegisterString(storeNameJSON);
			displayNameJSON = new JSONStorableString("displayName", string.Empty, SyncDisplayName);
			displayNameJSON.isStorable = false;
			displayNameJSON.isRestorable = false;
			RegisterString(displayNameJSON);
			ResetCreatorName();
			creatorNameJSON = new JSONStorableString("creatorName", _creatorName, SyncCreatorName);
			creatorNameJSON.isStorable = false;
			creatorNameJSON.isRestorable = false;
			creatorNameJSON.enableOnChange = true;
			RegisterString(creatorNameJSON);
			storedCreatorNameJSON = new JSONStorableString("storedCreatorName", dd.storedCreatorName);
			storedCreatorNameJSON.isStorable = false;
			storedCreatorNameJSON.isRestorable = false;
			tagsJSON = new JSONStorableString("tags", string.Empty, SyncTagsFromJSON);
			tagsJSON.isStorable = false;
			tagsJSON.isRestorable = false;
			RegisterString(tagsJSON);
			storeAction = new JSONStorableAction("Store", Store);
			RegisterAction(storeAction);
			loadAction = new JSONStorableAction("Load", Load);
			RegisterAction(loadAction);
			getScreenshotAction = new JSONStorableAction("GetScreenshot", GetScreenshot);
			RegisterAction(getScreenshotAction);
		}

		protected void StartInit()
		{
			string suggestPath = null;
			if (dd != null)
			{
				suggestPath = dd.GetStoreRootPath(false);
			}
			storeBrowsePathJSON = new JSONStorableUrl("storeBrowsePath", string.Empty, SyncBrowsePath, "vam", suggestPath, true);
			storeBrowsePathJSON.beginBrowseWithObjectCallback = BeginBrowseCreator;
			storeBrowsePathJSON.allowFullComputerBrowse = false;
			storeBrowsePathJSON.allowBrowseAboveSuggestedPath = false;
			storeBrowsePathJSON.isStorable = false;
			storeBrowsePathJSON.isRestorable = false;
			RegisterUrl(storeBrowsePathJSON);
			if (IsClothing())
			{
				clothSimTextureFileJSON = new JSONStorableUrl("clothSimTextureFile", string.Empty, SyncClothSimTextureFile, "png|jpg", suggestPath, true);
				clothSimTextureFileJSON.allowFullComputerBrowse = false;
				clothSimTextureFileJSON.allowBrowseAboveSuggestedPath = true;
				clothSimTextureFileJSON.beginBrowseWithObjectCallback = BeginClothSimTextureBrowse;
				clothSimTextureFileJSON.isStorable = false;
				clothSimTextureFileJSON.isRestorable = false;
				RegisterUrl(clothSimTextureFileJSON);
			}
			SyncOtherTags();
		}

		protected void SyncLoadButton()
		{
			if (!(dd != null))
			{
				return;
			}
			if (dd.CheckStoreExistance())
			{
				if (loadAction != null && loadAction.dynamicButton != null && loadAction.dynamicButton.button != null)
				{
					loadAction.dynamicButton.button.interactable = true;
				}
			}
			else if (loadAction != null && loadAction.dynamicButton != null && loadAction.dynamicButton.button != null)
			{
				loadAction.dynamicButton.button.interactable = false;
			}
		}

		protected void SyncStoreButton()
		{
			if (!(dd != null))
			{
				return;
			}
			if (!isWrapping && dd.CheckReadyForStore())
			{
				if (dd.CheckStoreExistance())
				{
					if (storeAction != null && storeAction.dynamicButton != null)
					{
						storeAction.dynamicButton.buttonColor = Color.red;
						if (storeAction.dynamicButton.button != null)
						{
							storeAction.dynamicButton.button.interactable = true;
						}
						if (storeAction.dynamicButton.buttonText != null)
						{
							storeAction.dynamicButton.buttonText.text = "Overwrite Existing Item";
						}
					}
				}
				else if (storeAction != null && storeAction.dynamicButton != null)
				{
					storeAction.dynamicButton.buttonColor = Color.green;
					if (storeAction.dynamicButton.button != null)
					{
						storeAction.dynamicButton.button.interactable = true;
					}
					if (storeAction.dynamicButton.buttonText != null)
					{
						storeAction.dynamicButton.buttonText.text = "Create New Item";
					}
				}
			}
			else if (dd.IsInPackage())
			{
				if (storeAction != null && storeAction.dynamicButton != null)
				{
					storeAction.dynamicButton.buttonColor = Color.gray;
					if (storeAction.dynamicButton.button != null)
					{
						storeAction.dynamicButton.button.interactable = false;
					}
					if (storeAction.dynamicButton.buttonText != null)
					{
						storeAction.dynamicButton.buttonText.text = "Create New Item Disabled Due To Being In Package. Clear Package To Make Local Copy.";
					}
				}
			}
			else if (storeAction != null && storeAction.dynamicButton != null)
			{
				storeAction.dynamicButton.buttonColor = Color.gray;
				if (storeAction.dynamicButton.button != null)
				{
					storeAction.dynamicButton.button.interactable = false;
				}
				if (storeAction.dynamicButton.buttonText != null)
				{
					storeAction.dynamicButton.buttonText.text = "Create New Item Disabled Until All Fields Are Set";
				}
			}
		}

		protected void SyncUI()
		{
			SyncLoadButton();
			SyncStoreButton();
		}

		protected override void InitUI(Transform t, bool isAlt)
		{
			base.InitUI(t, isAlt);
			if (!(t != null))
			{
				return;
			}
			DAZRuntimeCreatorUI componentInChildren = t.GetComponentInChildren<DAZRuntimeCreatorUI>();
			if (!(componentInChildren != null))
			{
				return;
			}
			dufFileJSON.RegisterText(componentInChildren.dufFileUrlText, isAlt);
			dufFileJSON.RegisterFileBrowseButton(componentInChildren.dufFileBrowseButton, isAlt);
			clearAction.RegisterButton(componentInChildren.clearButton, isAlt);
			cancelAction.RegisterButton(componentInChildren.cancelButton, isAlt);
			combineMaterialsJSON.RegisterToggle(componentInChildren.combineMaterialsToggle, isAlt);
			wrapToMorphedVerticesJSON.RegisterToggle(componentInChildren.wrapToMorphedVerticesToggle, isAlt);
			importDufAction.RegisterButton(componentInChildren.importButton, isAlt);
			if (IsClothing())
			{
				generateClothSimAction.RegisterButton(componentInChildren.generateSimButton, isAlt);
				cancelGenerateClothSimAction.RegisterButton(componentInChildren.cancelGenerateSimButton, isAlt);
				setUniformClothSimTextureAction.RegisterButton(componentInChildren.setUniformClothSimTextureButton, isAlt);
				uniformClothSimTextureValueJSON.RegisterSlider(componentInChildren.uniformClothSimTextureValueSlider, isAlt);
				clothSimTextureFileJSON.RegisterClearButton(componentInChildren.clearClothSimTextureButton, isAlt);
				clothSimTextureFileJSON.RegisterFileBrowseButton(componentInChildren.selectClothSimTextureButton, isAlt);
				clothSimUseIndividualSimTexturesJSON.RegisterToggle(componentInChildren.clothSimUseIndividualSimTexturesToggle, isAlt);
				clothSimCreateNearbyJointsJSON.RegisterToggle(componentInChildren.clothSimCreateNearbyJointsToggle, isAlt);
				clothSimNearbyJointsDistanceJSON.RegisterSlider(componentInChildren.clothSimNearbyJointsDistanceSlider, isAlt);
			}
			if (IsHair())
			{
				ObjectChoice[] array = scalpChoices;
				foreach (ObjectChoice objectChoice in array)
				{
					JSONStorable[] componentsInChildren = objectChoice.GetComponentsInChildren<JSONStorable>(true);
					JSONStorable[] array2 = componentsInChildren;
					foreach (JSONStorable jSONStorable in array2)
					{
						if (isAlt)
						{
							jSONStorable.SetUIAlt(t);
						}
						else
						{
							jSONStorable.SetUI(t);
						}
					}
				}
				scalpChooserJSON.RegisterPopup(componentInChildren.scalpChooserPopup, isAlt);
				generateHairSimAction.RegisterButton(componentInChildren.generateSimButton, isAlt);
				cancelGenerateHairSimAction.RegisterButton(componentInChildren.cancelGenerateSimButton, isAlt);
				segmentsJSON.RegisterSlider(componentInChildren.hairSimSegmentsSlider, isAlt);
				segmentsLengthJSON.RegisterSlider(componentInChildren.hairSimSegmentsLengthSlider, isAlt);
				scalpMaskSelectableSizeJSON.RegisterSlider(componentInChildren.scalpMaskSelectableSizeSlider, isAlt);
				scalpMaskSetAllAction.RegisterButton(componentInChildren.scalpMaskSetAllButton, isAlt);
				scalpMaskClearAllAction.RegisterButton(componentInChildren.scalpMaskClearAllButton, isAlt);
				startScalpMaskEditModeAction.RegisterButton(componentInChildren.startScalpMaskEditModeButton, isAlt);
				cancelScalpMaskEditModeAction.RegisterButton(componentInChildren.cancelScalpMaskEditModeButton, isAlt);
				finishScalpMaskEditModeAction.RegisterButton(componentInChildren.finishScalpMaskEditModeButton, isAlt);
				scalpMaskEditModeHideBackfacesJSON.RegisterToggle(componentInChildren.scalpMaskEditModeHideBackfacesToggle, isAlt);
				SyncScalpMaskButtons();
			}
			autoSetStoreFolderNameFromDufJSON.RegisterToggle(componentInChildren.autoSetStoreFolderNameToDufToggle, isAlt);
			storeFolderNameJSON.RegisterInputField(componentInChildren.storeFolderNameField, isAlt);
			autoSetStoreNameFromDufJSON.RegisterToggle(componentInChildren.autoSetStoreNameToDufToggle, isAlt);
			packageNameJSON.RegisterText(componentInChildren.packageNameText, isAlt);
			clearPackageAction.RegisterButton(componentInChildren.clearPackageButton, isAlt);
			storeNameJSON.RegisterInputField(componentInChildren.storeNameField, isAlt);
			displayNameJSON.RegisterInputField(componentInChildren.displayNameField, isAlt);
			creatorNameJSON.RegisterInputField(componentInChildren.creatorNameField, isAlt);
			storedCreatorNameJSON.RegisterText(componentInChildren.storedCreatorNameText, isAlt);
			tagsJSON.RegisterInputField(componentInChildren.tagsField, isAlt);
			storeBrowsePathJSON.RegisterFileBrowseButton(componentInChildren.browseStoreButton, isAlt);
			storeAction.RegisterButton(componentInChildren.storeButton, isAlt);
			loadAction.RegisterButton(componentInChildren.loadButton, isAlt);
			getScreenshotAction.RegisterButton(componentInChildren.getScreenshotButton, isAlt);
			if (isAlt)
			{
				creatorStatusTextAlt = componentInChildren.creatorStatusText;
			}
			else
			{
				creatorStatusText = componentInChildren.creatorStatusText;
				thumbnailRawImage = componentInChildren.thumbnailRawImage;
				importMessageText = componentInChildren.importMessageText;
				importVertexCountText = componentInChildren.importVertexCountText;
				simVertexCountText = componentInChildren.simVertexCountText;
				simJointCountText = componentInChildren.simJointCountText;
				simNearbyJointCountText = componentInChildren.simNearbyJointCountText;
				if (IsClothing())
				{
					clothSimTextureRawImage = componentInChildren.clothSimTextureRawImage;
					ClothSimControl component = GetComponent<ClothSimControl>();
					if (component != null && component.simEnabledJSON != null)
					{
						component.simEnabledJSON.toggleAlt = componentInChildren.clothSimEnabledToggle;
					}
					DAZClothingItemControl component2 = GetComponent<DAZClothingItemControl>();
					if (component2 != null && component2.disableAnatomyJSON != null)
					{
						component2.disableAnatomyJSON.toggleAlt = componentInChildren.disableAnatomyToggle;
					}
				}
				else if (IsHair())
				{
					DAZHairGroupControl component3 = GetComponent<DAZHairGroupControl>();
					if (component3 != null && component3.disableAnatomyJSON != null)
					{
						component3.disableAnatomyJSON.toggleAlt = componentInChildren.disableAnatomyToggle;
					}
				}
				tagsPanel = componentInChildren.tagsPanel;
				regionTagsContent = componentInChildren.regionTagsContent;
				typeTagsContent = componentInChildren.typeTagsContent;
				otherTagsContent = componentInChildren.otherTagsContent;
				if (componentInChildren.openTagsPanelButton != null)
				{
					componentInChildren.openTagsPanelButton.onClick.AddListener(OpenTagsPanel);
				}
				if (componentInChildren.closeTagsPanelButton != null)
				{
					componentInChildren.closeTagsPanelButton.onClick.AddListener(CloseTagsPanel);
				}
				InitTagsUI();
			}
			if (tagsPanelOpen)
			{
				OpenTagsPanel();
			}
			SyncUI();
		}

		private void Update()
		{
			if (di != null && di.isImporting)
			{
				SetCreatorStatus(di.importStatus);
			}
			bool flag = false;
			DAZSkinWrap[] components = di.GetComponents<DAZSkinWrap>();
			DAZSkinWrap[] array = components;
			foreach (DAZSkinWrap dAZSkinWrap in array)
			{
				if (dAZSkinWrap.IsWrapping)
				{
					flag = true;
					isWrapping = true;
					SetCreatorStatus(dAZSkinWrap.WrapStatus);
					SyncStoreButton();
				}
			}
			if (isWrapping && !flag)
			{
				isWrapping = false;
				SetCreatorStatus("Wrapping Complete");
				SyncStoreButton();
				if (IsHair() && customScalpChoiceName != null)
				{
					SyncScalpChoice(customScalpChoiceName);
				}
			}
		}

		private void Start()
		{
			StartInit();
		}

		private void OnEnable()
		{
		}

		private void OnDisable()
		{
			DAZCharacterRun componentInParent = GetComponentInParent<DAZCharacterRun>();
			if (componentInParent != null)
			{
				componentInParent.doSetMergedVerts = false;
			}
			if (scalpMaskEditMode)
			{
				CancelScalpMaskEditMode();
			}
		}

		private void OnDestroy()
		{
			AbortProcessGeometryThreaded(false);
		}
	}
}
