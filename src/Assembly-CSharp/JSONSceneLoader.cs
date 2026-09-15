using System.Collections.Generic;
using System.IO;
using MVR.FileManagement;
using UnityEngine;

public class JSONSceneLoader : JSONStorable
{
	protected JSONStorableActionSceneFilePath loadSceneJSONAction;

	protected JSONStorableActionSceneFilePath mergeLoadSceneJSONAction;

	public Transform embedSceneContainer;

	protected Dictionary<string, JSONEmbed> embedSceneNameToScene;

	protected JSONStorableString embedSceneNameJSON;

	protected JSONStorableAction loadEmbedScene;

	protected JSONStorableAction loadEmbedSceneMerge;

	protected void LoadScene(string path)
	{
		if (!(SuperController.singleton != null))
		{
			return;
		}
		if (SuperController.singleton.gameMode == SuperController.GameMode.Play)
		{
			if (FileManager.FileExists(path))
			{
				SuperController.singleton.Load(path);
				return;
			}
			string fileName = Path.GetFileName(path);
			JSONEmbed value;
			if (embedSceneNameToScene.TryGetValue(fileName, out value))
			{
				SuperController.singleton.LoadFromJSONEmbed(value);
			}
		}
		else
		{
			SuperController.LogError("Scene load triggered while in edit mode. Scene load triggers only apply in Play mode");
		}
	}

	protected void MergeLoadScene(string path)
	{
		if (!(SuperController.singleton != null))
		{
			return;
		}
		if (SuperController.singleton.gameMode == SuperController.GameMode.Play)
		{
			if (FileManager.FileExists(path))
			{
				SuperController.singleton.LoadMerge(path);
				return;
			}
			string fileName = Path.GetFileName(path);
			JSONEmbed value;
			if (embedSceneNameToScene.TryGetValue(fileName, out value))
			{
				SuperController.singleton.LoadFromJSONEmbed(value, true);
			}
		}
		else
		{
			SuperController.LogError("Merge scene load triggered while in edit mode. Scene load triggers only apply in Play mode");
		}
	}

	protected void LoadEmbedScene()
	{
		JSONEmbed value;
		if (embedSceneNameJSON != null && embedSceneNameJSON.val != string.Empty && embedSceneNameToScene.TryGetValue(embedSceneNameJSON.val, out value))
		{
			SuperController.singleton.LoadFromJSONEmbed(value);
		}
	}

	protected void LoadEmbedSceneMerge()
	{
		JSONEmbed value;
		if (embedSceneNameJSON != null && embedSceneNameJSON.val != string.Empty && embedSceneNameToScene.TryGetValue(embedSceneNameJSON.val, out value))
		{
			SuperController.singleton.LoadFromJSONEmbed(value, true);
		}
	}

	protected void Init()
	{
		loadSceneJSONAction = new JSONStorableActionSceneFilePath("LoadScene", LoadScene);
		RegisterSceneFilePathAction(loadSceneJSONAction);
		mergeLoadSceneJSONAction = new JSONStorableActionSceneFilePath("MergeLoadScene", MergeLoadScene);
		RegisterSceneFilePathAction(mergeLoadSceneJSONAction);
		embedSceneNameJSON = new JSONStorableString("embedSceneName", string.Empty);
		RegisterString(embedSceneNameJSON);
		loadEmbedScene = new JSONStorableAction("LoadEmbedScene", LoadEmbedScene);
		RegisterAction(loadEmbedScene);
		loadEmbedSceneMerge = new JSONStorableAction("LoadEmbedSceneMerge", LoadEmbedSceneMerge);
		RegisterAction(loadEmbedSceneMerge);
		embedSceneNameToScene = new Dictionary<string, JSONEmbed>();
		if (embedSceneContainer != null)
		{
			JSONEmbed[] componentsInChildren = embedSceneContainer.GetComponentsInChildren<JSONEmbed>();
			JSONEmbed[] array = componentsInChildren;
			foreach (JSONEmbed jSONEmbed in array)
			{
				embedSceneNameToScene.Add(jSONEmbed.name, jSONEmbed);
			}
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
			InitUI();
			InitUIAlt();
		}
	}
}
