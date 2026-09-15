using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using GPUTools.Cloth.Scripts;
using GPUTools.Cloth.Scripts.Geometry.Data;
using GPUTools.Cloth.Scripts.Runtime.Physics;
using GPUTools.Hair.Scripts;
using GPUTools.Hair.Scripts.Geometry.Create;
using GPUTools.Skinner.Scripts.Providers;
using MVR.FileManagement;
using SimpleJSON;
using UnityEngine;

namespace MeshVR
{
	public class DAZDynamic : PresetManager
	{
		public string displayName;

		protected string _uid;

		protected string[] _tagsArray;

		protected string _tags = string.Empty;

		public Transform binaryStorableBucket;

		public bool allowZeroBinaryStorables;

		public bool isRealItem = true;

		public string uid
		{
			get
			{
				return _uid;
			}
			set
			{
				if (_uid != value)
				{
					_uid = value;
					if (_uid == null || _uid == string.Empty)
					{
						base.name = "NoUID";
					}
					else
					{
						base.name = value;
					}
				}
			}
		}

		public string[] tagsArray
		{
			get
			{
				return _tagsArray;
			}
			protected set
			{
				_tagsArray = value;
			}
		}

		public string tags
		{
			get
			{
				return _tags;
			}
			set
			{
				string text = value;
				if (text != null)
				{
					text = text.Trim();
					text = Regex.Replace(text, ",\\s+", ",");
					text = Regex.Replace(text, "\\s+,", ",");
					text = text.ToLower();
				}
				if (_tags != text)
				{
					_tags = text;
					if (_tags == null || _tags == string.Empty)
					{
						tagsArray = new string[0];
						return;
					}
					tagsArray = _tags.Split(',');
				}
			}
		}

		public bool CheckMatchTag(string tag)
		{
			string text = tag.ToLower();
			if (_tagsArray != null)
			{
				string[] array = _tagsArray;
				foreach (string text2 in array)
				{
					if (text2.ToLower() == text)
					{
						return true;
					}
				}
			}
			return false;
		}

		public string GetMetaStorePath()
		{
			string result = null;
			if (CheckReadyForLoad())
			{
				string storeFolderPath = GetStoreFolderPath();
				result = storeFolderPath + storeName + ".vam";
			}
			return result;
		}

		public void Delete()
		{
			if (!CheckReadyForLoad())
			{
				return;
			}
			if (package == string.Empty)
			{
				string storeFolderPath = GetStoreFolderPath();
				string path = storeFolderPath + storeName + ".vab";
				string path2 = storeFolderPath + storeName + ".vam";
				string path3 = storeFolderPath + storeName + ".vaj";
				try
				{
					FileManager.AssertNotCalledFromPlugin();
					Clear();
					if (Application.isPlaying)
					{
						FileManager.CreateDirectory(storeFolderPath);
					}
					FileManager.DeleteFile(path);
					FileManager.DeleteFile(path2);
					FileManager.DeleteFile(path3);
					string[] files = FileManager.GetFiles(storeFolderPath, storeName + "_*.vap");
					string[] array = files;
					foreach (string path4 in array)
					{
						FileManager.DeleteFile(path4);
					}
					string[] files2 = FileManager.GetFiles(storeFolderPath, storeName + "_*.vap.fav");
					string[] array2 = files2;
					foreach (string path5 in array2)
					{
						FileManager.DeleteFile(path5);
					}
					return;
				}
				catch (Exception ex)
				{
					SuperController.LogError("Exception while deleting " + storeName + ": " + ex);
					return;
				}
			}
			SuperController.LogError("Tried to delete package item " + storeName);
		}

		protected override void StorePresetBinary()
		{
			string storeFolderPath = GetStoreFolderPath(false);
			if (storeFolderPath == null || !(storeFolderPath != string.Empty) || storeName == null || !(storeName != string.Empty) || _presetName == null || !(_presetName != string.Empty))
			{
				return;
			}
			if (!IsPresetInPackage())
			{
				string text = storeFolderPath + presetSubPath + storeName + "_" + presetSubName + ".vapb";
				if (itemType != ItemType.HairFemale && itemType != ItemType.HairMale && itemType != ItemType.HairNeutral)
				{
					return;
				}
				RuntimeHairGeometryCreator component = GetComponent<RuntimeHairGeometryCreator>();
				if (!(component != null))
				{
					return;
				}
				try
				{
					using (FileStream output = FileManager.OpenStreamForCreate(text))
					{
						using (BinaryWriter binWriter = new BinaryWriter(output))
						{
							component.StoreAuxToBinaryWriter(binWriter);
							return;
						}
					}
				}
				catch (Exception ex)
				{
					SuperController.LogError("Exception while storing to " + text + " " + ex);
					return;
				}
			}
			SuperController.LogError("Attempted to store a preset binary into a package. Cannot store");
		}

		public override void RestorePresetBinary()
		{
			string storeFolderPath = GetStoreFolderPath(false);
			if (storeFolderPath == null || !(storeFolderPath != string.Empty) || storeName == null || !(storeName != string.Empty))
			{
				return;
			}
			RuntimeHairGeometryCreator component = GetComponent<RuntimeHairGeometryCreator>();
			if (!(component != null))
			{
				return;
			}
			if (_presetName != null && _presetName != string.Empty)
			{
				string text = presetPackagePath + storeFolderPath + presetSubPath + storeName + "_" + presetSubName + ".vapb";
				if (itemType != ItemType.HairFemale && itemType != ItemType.HairMale && itemType != ItemType.HairNeutral)
				{
					return;
				}
				if (FileManager.FileExists(text))
				{
					try
					{
						using (FileEntryStream fileEntryStream = FileManager.OpenStream(text, true))
						{
							using (BinaryReader binReader = new BinaryReader(fileEntryStream.Stream))
							{
								component.LoadAuxFromBinaryReader(binReader);
								HairSettings component2 = GetComponent<HairSettings>();
								if (component2 != null && component2.HairBuidCommand != null)
								{
									component2.HairBuidCommand.RebuildHair();
								}
								return;
							}
						}
					}
					catch (Exception ex)
					{
						SuperController.LogError("Exception while loading " + text + " " + ex);
						return;
					}
				}
				if (component.usingAuxData)
				{
					component.RevertToLoadedData();
					HairSettings component3 = GetComponent<HairSettings>();
					if (component3 != null && component3.HairBuidCommand != null)
					{
						component3.HairBuidCommand.RebuildHair();
					}
				}
			}
			else if (component.usingAuxData)
			{
				component.RevertToLoadedData();
				HairSettings component4 = GetComponent<HairSettings>();
				if (component4 != null && component4.HairBuidCommand != null)
				{
					component4.HairBuidCommand.RebuildHair();
				}
			}
		}

		public void Clear()
		{
			ClothSettings component = GetComponent<ClothSettings>();
			if ((bool)component)
			{
				component.enabled = false;
				UnityEngine.Object.Destroy(component);
			}
			ClothPhysics component2 = GetComponent<ClothPhysics>();
			if ((bool)component2)
			{
				UnityEngine.Object.Destroy(component2);
			}
			IBinaryStorable[] components = binaryStorableBucket.GetComponents<IBinaryStorable>();
			IBinaryStorable[] array = components;
			foreach (IBinaryStorable binaryStorable in array)
			{
				if (binaryStorable is Component)
				{
					UnityEngine.Object.Destroy(binaryStorable as Component);
				}
			}
			MeshRenderer component3 = GetComponent<MeshRenderer>();
			if (component3 != null)
			{
				UnityEngine.Object.Destroy(component3);
			}
			MeshFilter component4 = GetComponent<MeshFilter>();
			if (component4 != null)
			{
				UnityEngine.Object.Destroy(component4);
			}
			RefreshStorables();
		}

		public bool CheckReadyForStore()
		{
			bool result = false;
			if (itemType != 0)
			{
				string storeFolderPath = GetStoreFolderPath();
				if (IsInPackage())
				{
					VarPackage varPackage = FileManager.GetPackage(package);
					if (!varPackage.IsSimulated)
					{
						return false;
					}
				}
				if (storeFolderPath != null && storeFolderPath != string.Empty && storeName != null && storeName != string.Empty)
				{
					IBinaryStorable[] components = binaryStorableBucket.GetComponents<IBinaryStorable>();
					if (allowZeroBinaryStorables || components.Length > 0)
					{
						result = true;
					}
				}
			}
			return result;
		}

		public bool CheckReadyForLoad()
		{
			bool result = false;
			if (itemType != 0)
			{
				string storeFolderPath = GetStoreFolderPath();
				if (storeFolderPath != null && storeFolderPath != string.Empty && storeName != null && storeName != string.Empty)
				{
					string path = storeFolderPath + storeName + ".vam";
					string path2 = storeFolderPath + storeName + ".vaj";
					if (FileManager.FileExists(path) && FileManager.FileExists(path2))
					{
						result = true;
					}
				}
			}
			return result;
		}

		public bool CheckStoreExistance()
		{
			bool result = false;
			if (itemType != 0)
			{
				string storeFolderPath = GetStoreFolderPath();
				if (storeFolderPath != null && storeFolderPath != string.Empty && storeName != null && storeName != string.Empty)
				{
					string path = storeFolderPath + storeName + ".vaj";
					result = FileManager.FileExists(path);
				}
			}
			return result;
		}

		public bool Store()
		{
			if (itemType != 0)
			{
				if (CheckReadyForStore())
				{
					string storeFolderPath = GetStoreFolderPath();
					string text = storeFolderPath + storeName + ".vab";
					string text2 = storeFolderPath + storeName + ".vam";
					string text3 = storeFolderPath + storeName + ".vaj";
					JSONClass jSONClass = new JSONClass();
					jSONClass["itemType"] = itemType.ToString();
					jSONClass["uid"] = _uid;
					jSONClass["displayName"] = displayName;
					jSONClass["creatorName"] = creatorName;
					jSONClass["tags"] = tags;
					jSONClass["isRealItem"].AsBool = isRealItem;
					storedCreatorName = creatorName;
					JSONClass jSONClass2 = new JSONClass();
					jSONClass2["components"] = new JSONArray();
					IBinaryStorable[] components = binaryStorableBucket.GetComponents<IBinaryStorable>();
					IBinaryStorable[] array = components;
					foreach (IBinaryStorable binaryStorable in array)
					{
						JSONClass jSONClass3 = new JSONClass();
						jSONClass3["type"] = binaryStorable.GetType().ToString();
						jSONClass2["components"].Add(jSONClass3);
					}
					try
					{
						FileManager.AssertNotCalledFromPlugin();
						if (Application.isPlaying)
						{
							FileManager.CreateDirectory(storeFolderPath);
						}
						using (FileStream output = FileManager.OpenStreamForCreate(text))
						{
							using (BinaryWriter binaryWriter = new BinaryWriter(output))
							{
								binaryWriter.Write("DynamicStore");
								binaryWriter.Write("1.0");
								IBinaryStorable[] array2 = components;
								foreach (IBinaryStorable binaryStorable2 in array2)
								{
									binaryStorable2.StoreToBinaryWriter(binaryWriter);
								}
								if (itemType == ItemType.ClothingFemale || itemType == ItemType.ClothingMale || itemType == ItemType.ClothingNeutral)
								{
									ClothSettings component = GetComponent<ClothSettings>();
									if (component != null)
									{
										ClothGeometryData geometryData = component.GeometryData;
										if (geometryData != null && geometryData.IsProcessed)
										{
											binaryWriter.Write(true);
											geometryData.StoreToBinaryWriter(binaryWriter);
										}
										else
										{
											binaryWriter.Write(false);
										}
									}
									else
									{
										binaryWriter.Write(false);
									}
								}
								else if (itemType == ItemType.HairFemale || itemType == ItemType.HairMale || itemType == ItemType.HairNeutral)
								{
									RuntimeHairGeometryCreator component2 = GetComponent<RuntimeHairGeometryCreator>();
									if (component2 != null)
									{
										binaryWriter.Write(true);
										component2.StoreToBinaryWriter(binaryWriter);
									}
									else
									{
										binaryWriter.Write(false);
									}
								}
							}
						}
					}
					catch (Exception ex)
					{
						SuperController.LogError("Exception while storing to " + text + " " + ex);
						return false;
					}
					RefreshStorables();
					FileManager.SetSaveDirFromFilePath(text3);
					StoreStorables(jSONClass2, true);
					try
					{
						using (StreamWriter streamWriter = FileManager.OpenStreamWriter(text2))
						{
							StringBuilder stringBuilder = new StringBuilder(1000);
							jSONClass.ToString(string.Empty, stringBuilder);
							streamWriter.Write(stringBuilder.ToString());
						}
					}
					catch (Exception ex2)
					{
						SuperController.LogError("Exception while storing to " + text2 + " " + ex2);
						return false;
					}
					try
					{
						using (StreamWriter streamWriter2 = FileManager.OpenStreamWriter(text3))
						{
							StringBuilder stringBuilder2 = new StringBuilder(10000);
							jSONClass2.ToString(string.Empty, stringBuilder2);
							streamWriter2.Write(stringBuilder2.ToString());
						}
					}
					catch (Exception ex3)
					{
						SuperController.LogError("Exception while storing to " + text3 + " " + ex3);
						return false;
					}
					return true;
				}
				SuperController.LogError("Not ready for store. Store root or name not set");
				return false;
			}
			SuperController.LogError("Item type set to None. Cannot store");
			return false;
		}

		public void GetScreenshot(SuperController.ScreenShotCallback callback = null)
		{
			if (itemType == ItemType.None)
			{
				return;
			}
			string storeFolderPath = GetStoreFolderPath();
			if (storeFolderPath != null)
			{
				string path = storeFolderPath + storeName + ".vam";
				if (FileManager.FileExists(path))
				{
					string saveName = storeFolderPath + storeName + ".jpg";
					SuperController.singleton.DoSaveScreenshot(saveName, callback);
				}
				else
				{
					SuperController.LogError("Screenshot only works after store");
				}
			}
		}

		public ClothSettings CreateNewClothSettings()
		{
			ClothSettings component = GetComponent<ClothSettings>();
			if (component != null)
			{
				component.enabled = false;
				UnityEngine.Object.Destroy(component);
			}
			component = base.gameObject.AddComponent<ClothSettings>();
			component.enabled = false;
			DAZSkinWrap component2 = GetComponent<DAZSkinWrap>();
			if (component2 != null)
			{
				component.MeshProvider.Type = ScalpMeshType.PreCalc;
				component.MeshProvider.PreCalcProvider = component2;
			}
			ClothSimControl component3 = GetComponent<ClothSimControl>();
			if (component3 != null)
			{
				component3.clothSettings = component;
				component3.SetSimEnabled(false);
			}
			return component;
		}

		public void SetDefaultsFromCurrent()
		{
			if (storables == null)
			{
				return;
			}
			foreach (Storable storable2 in storables)
			{
				JSONStorable storable = storable2.storable;
				if (ignoreExclude || !storable.exclude)
				{
					storable.SetDefaultsFromCurrent();
				}
			}
		}

		public bool Load(bool createWithExclude = false)
		{
			if (itemType != 0)
			{
				if (CheckReadyForLoad())
				{
					string storeFolderPath = GetStoreFolderPath();
					string text = storeFolderPath + storeName + ".vab";
					string text2 = storeFolderPath + storeName + ".vam";
					string text3 = storeFolderPath + storeName + ".vaj";
					if (IsInPackage())
					{
						VarPackage varPackage = FileManager.GetPackage(package);
						PackageInfo component = GetComponent<PackageInfo>();
						if (varPackage != null && component != null)
						{
							component.SetPackage(varPackage);
						}
					}
					string empty = string.Empty;
					try
					{
						empty = FileManager.ReadAllText(text2, true);
					}
					catch (Exception ex)
					{
						SuperController.LogError("Exception while loading " + text2 + " " + ex);
						return false;
					}
					JSONClass asObject = JSON.Parse(empty).AsObject;
					string empty2 = string.Empty;
					try
					{
						empty2 = FileManager.ReadAllText(text3, true);
					}
					catch (Exception ex2)
					{
						SuperController.LogError("Exception while loading " + text3 + " " + ex2);
						return false;
					}
					JSONClass asObject2 = JSON.Parse(empty2).AsObject;
					if (asObject != null && asObject2 != null)
					{
						Assembly assembly = GetType().Assembly;
						string text4 = asObject["itemType"];
						if (text4 != null)
						{
							try
							{
								itemType = (ItemType)Enum.Parse(typeof(ItemType), text4);
							}
							catch (ArgumentException)
							{
								SuperController.LogError("Attempted to set itemType to " + text4 + " which is not a valid item type");
							}
						}
						if (asObject["uid"] != null)
						{
							uid = asObject["uid"];
						}
						if (asObject["displayName"] != null)
						{
							displayName = asObject["displayName"];
						}
						else
						{
							displayName = Regex.Replace(storeName, ".*:", string.Empty);
						}
						if (asObject["creatorName"] != null)
						{
							storedCreatorName = asObject["creatorName"];
						}
						if (asObject["tags"] != null)
						{
							tags = asObject["tags"];
						}
						else
						{
							tags = string.Empty;
						}
						if (asObject["isRealItem"] != null)
						{
							isRealItem = asObject["isRealItem"].AsBool;
						}
						else
						{
							isRealItem = true;
						}
						if (asObject2["components"] != null)
						{
							foreach (JSONClass item in asObject2["components"].AsArray)
							{
								string text5 = item["type"];
								if (text5 != null)
								{
									Type type = assembly.GetType(text5);
									if (type != null)
									{
										binaryStorableBucket.gameObject.AddComponent(type);
									}
								}
							}
						}
					}
					DAZImport dAZImport = null;
					DAZMesh dAZMesh = null;
					DAZSkinWrap[] array = null;
					if (FileManager.FileExists(text))
					{
						IBinaryStorable[] components = binaryStorableBucket.GetComponents<IBinaryStorable>();
						try
						{
							using (FileEntryStream fileEntryStream = FileManager.OpenStream(text, true))
							{
								using (BinaryReader binaryReader = new BinaryReader(fileEntryStream.Stream))
								{
									string text6 = binaryReader.ReadString();
									if (text6 != "DynamicStore")
									{
										SuperController.LogError("Binary file " + text + " corrupted. Cannot read");
										Clear();
										return false;
									}
									string text7 = binaryReader.ReadString();
									if (text7 != "1.0")
									{
										SuperController.LogError("Binary schema " + text7 + " is not compatible with this version of software");
										Clear();
										return false;
									}
									IBinaryStorable[] array2 = components;
									foreach (IBinaryStorable binaryStorable in array2)
									{
										if (!binaryStorable.LoadFromBinaryReader(binaryReader))
										{
											Clear();
											return false;
										}
									}
									dAZImport = binaryStorableBucket.GetComponent<DAZImport>();
									dAZMesh = binaryStorableBucket.GetComponent<DAZMesh>();
									array = binaryStorableBucket.GetComponents<DAZSkinWrap>();
									DAZSkinWrap[] array3 = array;
									foreach (DAZSkinWrap dAZSkinWrap in array3)
									{
										dAZSkinWrap.dazMesh = dAZMesh;
										dAZSkinWrap.CopyMaterials();
										if (dAZImport != null)
										{
											dAZSkinWrap.skinTransform = dAZImport.skinToWrapToTransform;
											dAZSkinWrap.skin = dAZImport.skinToWrapTo;
											if (dAZImport.GPUSkinCompute != null)
											{
												dAZSkinWrap.GPUSkinWrapper = dAZImport.GPUSkinCompute;
											}
											if (dAZImport.GPUMeshCompute != null)
											{
												dAZSkinWrap.GPUMeshCompute = dAZImport.GPUMeshCompute;
											}
										}
									}
									if (itemType == ItemType.ClothingFemale || itemType == ItemType.ClothingMale || itemType == ItemType.ClothingNeutral)
									{
										if (binaryReader.ReadBoolean())
										{
											ClothSettings clothSettings = CreateNewClothSettings();
											if (clothSettings != null)
											{
												ClothGeometryData clothGeometryData = new ClothGeometryData();
												if (!clothGeometryData.LoadFromBinaryReader(binaryReader))
												{
													Clear();
													return false;
												}
												clothGeometryData.IsProcessed = true;
												clothSettings.GeometryData = clothGeometryData;
											}
										}
									}
									else if ((itemType == ItemType.HairFemale || itemType == ItemType.HairMale || itemType == ItemType.HairNeutral) && binaryReader.ReadBoolean())
									{
										RuntimeHairGeometryCreator component2 = GetComponent<RuntimeHairGeometryCreator>();
										if (component2 != null)
										{
											component2.LoadFromBinaryReader(binaryReader);
										}
										HairSettings component3 = GetComponent<HairSettings>();
										if (component3 != null && component3.HairBuidCommand != null)
										{
											component3.HairBuidCommand.RebuildHair();
										}
									}
								}
							}
						}
						catch (Exception ex4)
						{
							SuperController.LogError("Exception while loading " + text + " " + ex4);
							return false;
						}
						MaterialOptions[] components2 = binaryStorableBucket.GetComponents<MaterialOptions>();
						if (dAZImport != null)
						{
							dAZImport.ClearMaterialConnectors();
						}
						DAZSkinWrap component4 = binaryStorableBucket.GetComponent<DAZSkinWrap>();
						if (component4 != null)
						{
							component4.draw = true;
							if (dAZImport != null)
							{
							}
							DAZSkinWrapControl component5 = binaryStorableBucket.GetComponent<DAZSkinWrapControl>();
							if (component5 != null)
							{
								component5.wrap = component4;
							}
						}
						else if (dAZMesh != null)
						{
							dAZMesh.createMeshFilterAndRenderer = true;
							if (!(dAZImport != null))
							{
							}
						}
						if (dAZImport != null)
						{
							string text8 = null;
							DirectoryEntry directoryEntry = FileManager.GetDirectoryEntry(storeFolderPath);
							if (directoryEntry != null)
							{
								text8 = directoryEntry.Path;
							}
							for (int k = 0; k < components2.Length; k++)
							{
								components2[k].exclude = createWithExclude;
								string tabName;
								if (components2[k].overrideId == null || !(components2[k].overrideId != string.Empty))
								{
									tabName = ((components2.Length != 1) ? ("Combined" + (k + 1)) : "Combined");
								}
								else
								{
									tabName = Regex.Replace(components2[k].overrideId, "^\\+parent", string.Empty);
									tabName = Regex.Replace(components2[k].overrideId, "^\\+Material", string.Empty);
								}
								dAZImport.CreateMaterialOptionsUI(components2[k], tabName);
								components2[k].SetStartingValues();
								if (text8 != null)
								{
									components2[k].SetCustomTextureFolder(text8);
								}
							}
						}
						if (includeChildrenMaterialOptions)
						{
							SyncMaterialOptions();
						}
						DAZClothSettingsSimTextureReloader component6 = GetComponent<DAZClothSettingsSimTextureReloader>();
						if (component6 != null)
						{
							component6.SyncSkinWrapMaterialOptions();
						}
						FileManager.PushLoadDirFromFilePath(text3);
						RefreshStorables();
						RestoreStorables(asObject2);
						FileManager.PopLoadDir();
						SetDefaultsFromCurrent();
						return true;
					}
					SuperController.LogError("Could not load binary store " + text);
					Clear();
					return false;
				}
				SuperController.LogError("Not ready for load. Invalid load path or params");
				return false;
			}
			SuperController.LogError("Item type set to None. Cannot load");
			return false;
		}

		protected override void Awake()
		{
			base.Awake();
			if (binaryStorableBucket == null)
			{
				binaryStorableBucket = base.transform;
			}
		}
	}
}
