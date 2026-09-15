using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using SimpleJSON;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
	public Text statusText;

	public Button startButton;

	public Button startButtonAlt;

	public Slider fullProgressSlider;

	public Slider individualProgressSlider;

	public string[] preloadScenes;

	public string sceneCheckFile;

	public string sceneCheckFilePathPrefix;

	public string sceneName;

	public bool loadOnStart;

	public bool activateWhenLoaded = true;

	public bool loadAsync = true;

	protected bool isLoading;

	protected bool isLoadingMainScene;

	protected AsyncOperation async;

	protected AsyncOperation mainAsync;

	public Material progressMaterial;

	public string progressMaterialFieldName;

	protected float progressTarget;

	protected float progress;

	public float progressMaxSpeed = 0.005f;

	public RectTransform contentLevelParent;

	public Transform contentLevelTogglePrefab;

	public ToggleGroup contentLevelToggleGroup;

	public float contentLevelToggleStartY = 100f;

	public float contentLevelToggleBuffer = 5f;

	public Transform singleSceneBanner;

	public Transform singleSceneText;

	public Transform multiSceneBanner;

	public Transform multiSceneText;

	public Transform keyIssueBanner;

	protected Dictionary<string, string> keyNameToKeyVal;

	protected string firstSceneName;

	protected bool singleScene;

	public Text keyInputField;

	public Text keyEntryStatus;

	public void AddKey()
	{
		if (sceneCheckFile == null || !(sceneCheckFile != string.Empty) || !(keyInputField != null))
		{
			return;
		}
		JSONClass jSONClass;
		if (File.Exists(sceneCheckFile))
		{
			StreamReader streamReader = new StreamReader(sceneCheckFile);
			string aJSON = streamReader.ReadToEnd();
			streamReader.Close();
			JSONNode jSONNode = JSON.Parse(aJSON);
			if (jSONNode == null)
			{
				jSONClass = new JSONClass();
			}
			else
			{
				jSONClass = jSONNode.AsObject;
				if (jSONClass == null)
				{
					jSONClass = new JSONClass();
				}
			}
		}
		else
		{
			jSONClass = new JSONClass();
		}
		string text = keyInputField.text;
		if (text != null && text.Length > 0)
		{
			string text2 = string.Empty;
			string text3;
			switch (text[0])
			{
			case 'F':
			case 'f':
				text3 = "Free";
				text2 = "F";
				break;
			case 'T':
			case 't':
				text3 = "Teaser";
				text2 = "T";
				break;
			case 'E':
			case 'e':
				text3 = "Entertainer";
				text2 = "E";
				break;
			case 'C':
			case 'c':
				text3 = "Creator";
				text2 = "C";
				break;
			default:
				text3 = "Unknown";
				break;
			}
			if (text3 != "Unknown")
			{
				SHA256 shaHash = SHA256.Create();
				string sha256Hash = GetSha256Hash(shaHash, text3 + text, 3);
				int buildIndexByScenePath = SceneUtility.GetBuildIndexByScenePath(sceneCheckFilePathPrefix + "/" + text2 + sha256Hash);
				if (buildIndexByScenePath != -1)
				{
					if (keyEntryStatus != null)
					{
						keyEntryStatus.text = "Key accepted";
					}
					jSONClass[text].AsBool = true;
					string value = jSONClass.ToString(string.Empty);
					try
					{
						StreamWriter streamWriter = new StreamWriter(sceneCheckFile);
						streamWriter.Write(value);
						streamWriter.Close();
					}
					catch (Exception ex)
					{
						if (keyEntryStatus != null)
						{
							keyEntryStatus.text = "Exception while storing key " + ex.Message;
						}
					}
					GenerateContentLevelToggles();
				}
				else if (keyEntryStatus != null)
				{
					keyEntryStatus.text = "Invalid key";
				}
			}
			else if (keyEntryStatus != null)
			{
				keyEntryStatus.text = "Invalid key";
			}
		}
		else if (keyEntryStatus != null)
		{
			keyEntryStatus.text = "Invalid key";
		}
	}

	public void OutputSceneNameForKey(string keyval)
	{
		char c = keyval[0];
		string text = string.Empty;
		string empty = string.Empty;
		switch (c)
		{
		case 'F':
		case 'f':
			empty = "Free";
			text = "F";
			break;
		case 'T':
		case 't':
			empty = "Teaser";
			text = "T";
			break;
		case 'E':
		case 'e':
			empty = "Entertainer";
			text = "E";
			break;
		case 'C':
		case 'c':
			empty = "Creator";
			text = "C";
			break;
		default:
			empty = "Unknown";
			break;
		}
		SHA256 shaHash = SHA256.Create();
		string sha256Hash = GetSha256Hash(shaHash, empty + keyval, 3);
		string text2 = text + sha256Hash;
		Debug.Log("Scene name for key " + keyval + " is " + text2);
	}

	protected void GenerateContentLevelToggles()
	{
		if (sceneCheckFile != null && sceneCheckFile != string.Empty && contentLevelParent != null && contentLevelTogglePrefab != null)
		{
			foreach (Transform item in contentLevelParent)
			{
				if (item.name != "Label")
				{
					UnityEngine.Object.Destroy(item.gameObject);
				}
			}
			contentLevelParent.gameObject.SetActive(false);
			keyNameToKeyVal = new Dictionary<string, string>();
			try
			{
				if (File.Exists(sceneCheckFile))
				{
					StreamReader streamReader = new StreamReader(sceneCheckFile);
					string aJSON = streamReader.ReadToEnd();
					streamReader.Close();
					JSONNode jSONNode = JSON.Parse(aJSON);
					if (jSONNode != null)
					{
						JSONClass asObject = jSONNode.AsObject;
						if (asObject != null)
						{
							float num = contentLevelToggleStartY;
							int num2 = 0;
							Toggle toggle = null;
							firstSceneName = null;
							List<string> list = new List<string>(asObject.Keys);
							list.Sort(delegate(string s1, string s2)
							{
								char c = s1[0];
								char c2 = s2[0];
								if (c == c2)
								{
									return 0;
								}
								switch (c)
								{
								case 'F':
									return -1;
								case 'T':
									if (c2 == 'F')
									{
										return 1;
									}
									return -1;
								case 'E':
									if (c2 == 'F' || c2 == 'T')
									{
										return 1;
									}
									return -1;
								default:
									return 1;
								}
							});
							Vector2 anchoredPosition = default(Vector2);
							foreach (string item2 in list)
							{
								char c3 = item2[0];
								string text = string.Empty;
								string text2;
								switch (c3)
								{
								case 'F':
								case 'f':
									text2 = "Free";
									text = "F";
									break;
								case 'T':
								case 't':
									text2 = "Teaser";
									text = "T";
									break;
								case 'E':
								case 'e':
									text2 = "Entertainer";
									text = "E";
									break;
								case 'C':
								case 'c':
									text2 = "Creator";
									text = "C";
									break;
								default:
									text2 = "Unknown";
									break;
								}
								if (text2 != null && item2 != null && text2 != "Unknown")
								{
									SHA256 shaHash = SHA256.Create();
									string sha256Hash = GetSha256Hash(shaHash, text2 + item2, 3);
									string value = text + sha256Hash;
									if (firstSceneName == null)
									{
										firstSceneName = value;
									}
									num2++;
									keyNameToKeyVal.Add(text2, value);
									anchoredPosition.x = 0f;
									anchoredPosition.y = num;
									Transform transform2 = UnityEngine.Object.Instantiate(contentLevelTogglePrefab);
									transform2.SetParent(contentLevelParent, false);
									RectTransform component = transform2.GetComponent<RectTransform>();
									component.anchoredPosition = anchoredPosition;
									num += component.sizeDelta.y + contentLevelToggleBuffer;
									Toggle componentInChildren = transform2.GetComponentInChildren<Toggle>();
									if (componentInChildren != null)
									{
										toggle = componentInChildren;
										componentInChildren.isOn = false;
										componentInChildren.group = contentLevelToggleGroup;
									}
									Text componentInChildren2 = transform2.GetComponentInChildren<Text>();
									if (componentInChildren2 != null)
									{
										componentInChildren2.text = text2;
									}
								}
								else
								{
									Debug.LogError("Invalid key file");
									if (keyEntryStatus != null)
									{
										keyEntryStatus.text = "Invalid key file";
									}
									if (keyIssueBanner != null)
									{
										keyIssueBanner.gameObject.SetActive(true);
									}
								}
							}
							if (toggle != null)
							{
								toggle.isOn = true;
							}
							if (num2 > 0)
							{
								float y = num + contentLevelToggleBuffer;
								Vector2 sizeDelta = contentLevelParent.sizeDelta;
								sizeDelta.y = y;
								contentLevelParent.sizeDelta = sizeDelta;
							}
							if (num2 == 1)
							{
								singleScene = true;
								if (singleSceneBanner != null)
								{
									singleSceneBanner.gameObject.SetActive(true);
								}
								if (multiSceneBanner != null)
								{
									multiSceneBanner.gameObject.SetActive(false);
								}
								if (singleSceneText != null)
								{
									singleSceneText.gameObject.SetActive(true);
								}
								if (multiSceneText != null)
								{
									multiSceneText.gameObject.SetActive(false);
								}
							}
							else if (num2 >= 1)
							{
								singleScene = false;
								contentLevelParent.gameObject.SetActive(true);
								if (singleSceneBanner != null)
								{
									singleSceneBanner.gameObject.SetActive(false);
								}
								if (multiSceneBanner != null)
								{
									multiSceneBanner.gameObject.SetActive(true);
								}
								if (singleSceneText != null)
								{
									singleSceneText.gameObject.SetActive(false);
								}
								if (multiSceneText != null)
								{
									multiSceneText.gameObject.SetActive(true);
								}
							}
							else
							{
								Debug.LogError("No valid keys found in keys file");
								if (keyEntryStatus != null)
								{
									keyEntryStatus.text = "No valid keys found in keys file";
								}
								if (keyIssueBanner != null)
								{
									keyIssueBanner.gameObject.SetActive(true);
								}
							}
						}
						else
						{
							Debug.LogError("Invalid key file");
							if (keyEntryStatus != null)
							{
								keyEntryStatus.text = "Invalid key file";
							}
							if (keyIssueBanner != null)
							{
								keyIssueBanner.gameObject.SetActive(true);
							}
						}
					}
					else
					{
						Debug.LogError("Invalid key file");
						if (keyEntryStatus != null)
						{
							keyEntryStatus.text = "Invalid key file";
						}
						if (keyIssueBanner != null)
						{
							keyIssueBanner.gameObject.SetActive(true);
						}
					}
				}
				else
				{
					Debug.LogError("Key file missing");
					if (keyEntryStatus != null)
					{
						keyEntryStatus.text = "Key file missing";
					}
					if (keyIssueBanner != null)
					{
						keyIssueBanner.gameObject.SetActive(true);
					}
				}
				return;
			}
			catch (Exception ex)
			{
				if (keyEntryStatus != null)
				{
					keyEntryStatus.text = "Exception while processing key file " + ex.Message;
				}
				if (keyIssueBanner != null)
				{
					keyIssueBanner.gameObject.SetActive(true);
				}
				return;
			}
		}
		if (contentLevelParent != null)
		{
			contentLevelParent.gameObject.SetActive(false);
		}
	}

	private static string GetSha256Hash(SHA256 shaHash, string input, int length)
	{
		byte[] array = shaHash.ComputeHash(Encoding.UTF8.GetBytes(input));
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length && i < length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	protected string GetLoadSceneName()
	{
		if (sceneCheckFile != null && sceneCheckFile != string.Empty)
		{
			if (singleScene)
			{
				return firstSceneName;
			}
			if (contentLevelToggleGroup != null)
			{
				foreach (Toggle item in contentLevelToggleGroup.ActiveToggles())
				{
					Text componentInChildren = item.GetComponentInChildren<Text>();
					if (componentInChildren != null)
					{
						string text = componentInChildren.text;
						string value;
						if (keyNameToKeyVal.TryGetValue(text, out value))
						{
							return sceneCheckFilePathPrefix + "/" + value;
						}
					}
				}
			}
			return null;
		}
		return sceneName;
	}

	private IEnumerator LoadMainScene()
	{
		if (startButton != null)
		{
			startButton.gameObject.SetActive(false);
		}
		if (startButtonAlt != null)
		{
			startButtonAlt.gameObject.SetActive(false);
		}
		string loadSceneName = GetLoadSceneName();
		if (loadSceneName != null)
		{
			if (contentLevelParent != null)
			{
				contentLevelParent.gameObject.SetActive(false);
			}
			mainAsync = SceneManager.LoadSceneAsync(loadSceneName, LoadSceneMode.Single);
			yield return async;
			yield break;
		}
		Debug.LogError("Load scene name was null. Check key file");
		if (keyEntryStatus != null)
		{
			keyEntryStatus.text = "Invalid key file";
		}
		if (keyIssueBanner != null)
		{
			keyIssueBanner.gameObject.SetActive(true);
		}
	}

	private IEnumerator LoadMergeScenesAsync()
	{
		isLoading = true;
		yield return null;
		if (fullProgressSlider != null)
		{
			fullProgressSlider.gameObject.SetActive(true);
			fullProgressSlider.value = 0f;
		}
		if (individualProgressSlider != null)
		{
			individualProgressSlider.gameObject.SetActive(true);
			individualProgressSlider.value = 0f;
		}
		if (progressMaterial != null && progressMaterial.HasProperty(progressMaterialFieldName))
		{
			progressMaterial.SetFloat(progressMaterialFieldName, 0f);
		}
		int fullLength = preloadScenes.Length;
		for (int i = 0; i < preloadScenes.Length; i++)
		{
			async = SceneManager.LoadSceneAsync(preloadScenes[i], LoadSceneMode.Additive);
			yield return async;
			progressTarget = (float)(i + 1) / (float)fullLength;
		}
		progressTarget = 1f;
		isLoading = false;
	}

	protected void LoadScene()
	{
		if (startButton != null)
		{
			startButton.interactable = false;
		}
		if (startButtonAlt != null)
		{
			startButtonAlt.interactable = false;
		}
		if (loadAsync)
		{
			StartCoroutine(LoadMergeScenesAsync());
			return;
		}
		for (int i = 0; i < preloadScenes.Length; i++)
		{
			SceneManager.LoadScene(preloadScenes[i], LoadSceneMode.Additive);
		}
		string loadSceneName = GetLoadSceneName();
		if (loadSceneName != null)
		{
			SceneManager.LoadScene(loadSceneName, LoadSceneMode.Single);
		}
	}

	protected void ActivateScene()
	{
		StartCoroutine(LoadMainScene());
	}

	private void Start()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		string text = string.Empty;
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i] == "-benchmark")
			{
				text = commandLineArgs[i + 1];
			}
		}
		if (text != string.Empty)
		{
			SceneManager.LoadScene(text, LoadSceneMode.Single);
			return;
		}
		if (loadOnStart)
		{
			if (startButton != null)
			{
				startButton.gameObject.SetActive(true);
				startButton.onClick.AddListener(ActivateScene);
				startButton.interactable = false;
			}
			if (startButtonAlt != null)
			{
				startButtonAlt.gameObject.SetActive(true);
				startButtonAlt.onClick.AddListener(ActivateScene);
				startButtonAlt.interactable = false;
			}
			LoadScene();
		}
		else
		{
			if (startButton != null)
			{
				startButton.gameObject.SetActive(true);
				startButton.onClick.AddListener(LoadScene);
			}
			if (startButtonAlt != null)
			{
				startButtonAlt.gameObject.SetActive(true);
				startButtonAlt.onClick.AddListener(LoadScene);
			}
		}
		GenerateContentLevelToggles();
	}

	private void Update()
	{
		if (isLoading && individualProgressSlider != null)
		{
			individualProgressSlider.value = async.progress * 100f;
		}
		if (progressTarget > progress)
		{
			progress += progressMaxSpeed;
			if (progress > progressTarget)
			{
				progress = progressTarget;
			}
		}
		if (progress == 1f)
		{
			progress = progressTarget;
			if (activateWhenLoaded)
			{
				ActivateScene();
			}
			else
			{
				if (startButton != null)
				{
					startButton.interactable = true;
				}
				if (startButtonAlt != null)
				{
					startButtonAlt.interactable = true;
				}
			}
		}
		if (fullProgressSlider != null)
		{
			fullProgressSlider.value = progress * 100f;
		}
		if (progressMaterial != null)
		{
			if (mainAsync != null)
			{
				progressMaterial.SetFloat(progressMaterialFieldName, 1f - mainAsync.progress);
			}
			else if (progressMaterial.HasProperty(progressMaterialFieldName))
			{
				progressMaterial.SetFloat(progressMaterialFieldName, progress);
			}
		}
	}
}
