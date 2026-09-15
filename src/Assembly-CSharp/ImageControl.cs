using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Core;
using MVR.FileManagement;
using MVR.FileManagementSecure;
using OldMoatGames;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ImageControl : JSONStorableTriggerHandler
{
	public GameObject nonWhitelistSiteObject;

	public Text nonWhitelistSiteText;

	public GameObject webDisabledObject;

	protected bool notActiveOnSync;

	public Transform targetTransformForScale;

	public Transform transformForScaling;

	protected JSONStorableUrl urlJSON;

	[SerializeField]
	protected string _url;

	public bool matchScaleToImageRatio = true;

	protected Texture2D currentTexture;

	public Texture2D blankTexture;

	public RawImage rawImage;

	public bool alsoSetSpecularTexture = true;

	protected Texture currentVideoTexture;

	protected VideoPlayer videoPlayer;

	protected string currentTempVideo;

	protected JSONStorableBool playVideoWhenReadyJSON;

	[SerializeField]
	protected bool _playVideoWhenReady = true;

	protected JSONStorableBool useAnamorphicVideoAspectRatioJSON;

	protected bool _useAnamorphicVideoAspectRatio;

	public GameObject videoIsLoadingIndicator;

	public GameObject videoHadErrorIndicator;

	protected JSONStorableBool videoIsLoadingJSON;

	protected JSONStorableBool videoIsReadyJSON;

	protected JSONStorableBool videoHadErrorJSON;

	protected JSONStorableBool loopVideoJSON;

	[SerializeField]
	protected bool _loopVideo;

	protected JSONStorableFloat playbackTimeJSON;

	protected bool wasPlaying;

	protected JSONStorableAction playVideoAction;

	protected JSONStorableAction pauseVideoAction;

	protected JSONStorableAction stopVideoAction;

	protected JSONStorableAction seekToVideoStartAction;

	public Trigger videoReadyTrigger;

	public Trigger videoStartedTrigger;

	public Trigger videoStoppedTrigger;

	protected Texture2D createdTexture;

	protected Texture2D registeredTexture;

	protected bool _allowTiling;

	protected JSONStorableBool allowTilingJSON;

	protected bool wasWebEnabled;

	public string url
	{
		get
		{
			return _url;
		}
		set
		{
			if (urlJSON != null)
			{
				urlJSON.val = value;
			}
			if (_url != value)
			{
				SyncUrl(value);
			}
		}
	}

	public bool playVideoWhenReady
	{
		get
		{
			return _playVideoWhenReady;
		}
		set
		{
			if (playVideoWhenReadyJSON != null)
			{
				playVideoWhenReadyJSON.val = value;
			}
			else
			{
				SyncPlayVideoWhenReady(value);
			}
		}
	}

	public bool useAnamorphicVideoAspectRatio
	{
		get
		{
			return _useAnamorphicVideoAspectRatio;
		}
		set
		{
			if (useAnamorphicVideoAspectRatioJSON != null)
			{
				useAnamorphicVideoAspectRatioJSON.val = value;
			}
			else
			{
				SyncUseAnamorphicVideoAspectRatio(value);
			}
		}
	}

	public bool loopVideo
	{
		get
		{
			return _loopVideo;
		}
		set
		{
			if (loopVideoJSON != null)
			{
				loopVideoJSON.val = value;
			}
			else
			{
				SyncLoopVideo(value);
			}
		}
	}

	public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
	{
		JSONClass jSON = base.GetJSON(includePhysical, includeAppearance, forceStore);
		if (includePhysical || forceStore)
		{
			if (videoReadyTrigger != null)
			{
				needsStore = true;
				jSON["videoReadyTrigger"] = videoReadyTrigger.GetJSON(base.subScenePrefix);
			}
			if (videoStartedTrigger != null)
			{
				needsStore = true;
				jSON["videoStartedTrigger"] = videoStartedTrigger.GetJSON(base.subScenePrefix);
			}
			if (videoStoppedTrigger != null)
			{
				needsStore = true;
				jSON["videoStoppedTrigger"] = videoStoppedTrigger.GetJSON(base.subScenePrefix);
			}
		}
		return jSON;
	}

	public override void LateRestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, bool setMissingToDefault = true)
	{
		base.LateRestoreFromJSON(jc, restorePhysical, restoreAppearance, setMissingToDefault);
		if (base.physicalLocked || !restorePhysical)
		{
			return;
		}
		if (!IsCustomPhysicalParamLocked("videoReadyTrigger"))
		{
			if (jc["videoReadyTrigger"] != null)
			{
				JSONClass asObject = jc["videoReadyTrigger"].AsObject;
				if (asObject != null)
				{
					videoReadyTrigger.RestoreFromJSON(asObject, base.subScenePrefix, base.mergeRestore);
				}
			}
			else if (setMissingToDefault)
			{
				videoReadyTrigger.RestoreFromJSON(new JSONClass());
			}
		}
		if (!IsCustomPhysicalParamLocked("videoStartedTrigger"))
		{
			if (jc["videoStartedTrigger"] != null)
			{
				JSONClass asObject2 = jc["videoStartedTrigger"].AsObject;
				if (asObject2 != null)
				{
					videoStartedTrigger.RestoreFromJSON(asObject2, base.subScenePrefix, base.mergeRestore);
				}
			}
			else if (setMissingToDefault)
			{
				videoStartedTrigger.RestoreFromJSON(new JSONClass());
			}
		}
		if (IsCustomPhysicalParamLocked("videoStoppedTrigger"))
		{
			return;
		}
		if (jc["videoStoppedTrigger"] != null)
		{
			JSONClass asObject3 = jc["videoStoppedTrigger"].AsObject;
			if (asObject3 != null)
			{
				videoStoppedTrigger.RestoreFromJSON(asObject3, base.subScenePrefix, base.mergeRestore);
			}
		}
		else if (setMissingToDefault)
		{
			videoStoppedTrigger.RestoreFromJSON(new JSONClass());
		}
	}

	public override void Validate()
	{
		base.Validate();
		if (videoReadyTrigger != null)
		{
			videoReadyTrigger.Validate();
		}
		if (videoStartedTrigger != null)
		{
			videoStartedTrigger.Validate();
		}
		if (videoStoppedTrigger != null)
		{
			videoStoppedTrigger.Validate();
		}
	}

	public void StartSyncUrl()
	{
		if (_url != null && _url != string.Empty)
		{
			bool flag = false;
			if (Regex.IsMatch(url, "^http"))
			{
				if (UserPreferences.singleton == null)
				{
					flag = true;
				}
				else if (UserPreferences.singleton.enableWebMisc)
				{
					if (UserPreferences.singleton.CheckWhitelistDomain(_url))
					{
						flag = true;
					}
					else
					{
						if (nonWhitelistSiteObject != null)
						{
							nonWhitelistSiteObject.SetActive(true);
						}
						if (nonWhitelistSiteText != null)
						{
							nonWhitelistSiteText.text = _url;
						}
						SuperController.LogError("Attempted to load image from URL " + _url + " which is not on whitelist", true, !UserPreferences.singleton.hideDisabledWebMessages);
					}
				}
				else if (!UserPreferences.singleton.hideDisabledWebMessages)
				{
					if (webDisabledObject != null)
					{
						webDisabledObject.SetActive(true);
					}
					SuperController.LogError("Attempted to load http URL image when web load option is disabled. To enable, see User Preferences -> Web Security tab");
					SuperController.singleton.ShowMainHUDAuto();
					SuperController.singleton.SetActiveUI("MainMenu");
					SuperController.singleton.SetMainMenuTab("TabUserPrefs");
					SuperController.singleton.SetUserPrefsTab("TabSecurity");
				}
				else if (webDisabledObject != null)
				{
					webDisabledObject.SetActive(true);
				}
			}
			else
			{
				flag = true;
				if (webDisabledObject != null)
				{
					webDisabledObject.SetActive(false);
				}
			}
			if (!flag)
			{
				return;
			}
			if (nonWhitelistSiteObject != null)
			{
				nonWhitelistSiteObject.SetActive(false);
			}
			if (base.gameObject.activeInHierarchy)
			{
				if (_url.EndsWith(".avi") || _url.EndsWith(".mp4"))
				{
					ClearImage();
					SyncVideo();
					return;
				}
				DisableVideoPlayer();
				AnimatedGifPlayer component = GetComponent<AnimatedGifPlayer>();
				if (_url.EndsWith(".gif"))
				{
					if (component != null)
					{
						component.enabled = true;
						component.FileName = url;
						component.Init();
					}
				}
				else if (_url.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || _url.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase) || _url.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
				{
					if (component != null)
					{
						component.enabled = false;
					}
					if (_url.StartsWith("http") || _url.StartsWith("file") || ImageLoaderThreaded.singleton == null)
					{
						StartCoroutine(SyncImage());
						return;
					}
					ImageLoaderThreaded.QueuedImage queuedImage = new ImageLoaderThreaded.QueuedImage();
					queuedImage.imgPath = _url;
					queuedImage.createMipMaps = true;
					queuedImage.callback = OnImageLoaded;
					ImageLoaderThreaded.singleton.QueueImage(queuedImage);
				}
				else
				{
					if (component != null)
					{
						component.enabled = false;
					}
					ClearImage();
					SyncVideo();
				}
			}
			else
			{
				notActiveOnSync = true;
			}
		}
		else
		{
			DisableVideoPlayer();
			AnimatedGifPlayer component2 = GetComponent<AnimatedGifPlayer>();
			if (component2 != null)
			{
				component2.enabled = false;
			}
			ClearImage();
		}
	}

	protected void SyncUrl(string s)
	{
		_url = s;
		StartSyncUrl();
	}

	protected void BeginBrowse(JSONStorableUrl jsurl)
	{
		List<ShortCut> shortCutsForDirectory = FileManager.GetShortCutsForDirectory("Custom/Images", true, true, true, true);
		ShortCut shortCut = new ShortCut();
		shortCut.displayName = "Root";
		shortCut.path = Path.GetFullPath(".");
		shortCutsForDirectory.Insert(0, shortCut);
		jsurl.shortCuts = shortCutsForDirectory;
	}

	public void SyncImageRatio(Texture tex, bool isVideo = false)
	{
		if (!matchScaleToImageRatio)
		{
			return;
		}
		float num = 1f;
		float num2 = 1f;
		float num3 = 1f;
		if (targetTransformForScale != null)
		{
			num2 = targetTransformForScale.localScale.x;
			num3 = targetTransformForScale.localScale.y;
			num = num2 / num3;
		}
		Vector3 localScale = default(Vector3);
		localScale.z = 1f;
		if (tex != null)
		{
			float num4 = tex.width;
			float num5 = tex.height;
			if (isVideo && _useAnamorphicVideoAspectRatio)
			{
				num4 *= 1.33f;
			}
			float num6 = num4 / num5;
			if (num6 > num)
			{
				localScale.x = num2;
				localScale.y = num2 / num6;
			}
			else
			{
				localScale.x = num6 * num3;
				localScale.y = num3;
			}
		}
		else
		{
			localScale.x = num2;
			localScale.y = num3;
		}
		if (transformForScaling != null)
		{
			transformForScaling.localScale = localScale;
		}
		else
		{
			base.transform.localScale = localScale;
		}
	}

	public void SyncTexture(Texture2D tex)
	{
		SyncImageRatio(tex);
		currentTexture = tex;
		SyncAllowTiling();
		if (rawImage != null)
		{
			rawImage.texture = tex;
			return;
		}
		MeshRenderer[] components = GetComponents<MeshRenderer>();
		MeshRenderer[] array = components;
		foreach (MeshRenderer meshRenderer in array)
		{
			Material[] array2 = ((!Application.isPlaying) ? meshRenderer.sharedMaterials : meshRenderer.materials);
			Material material = array2[0];
			if (material.HasProperty("_MainTex"))
			{
				material.SetTexture("_MainTex", tex);
			}
			if (alsoSetSpecularTexture && material.HasProperty("_SpecTex"))
			{
				material.SetTexture("_SpecTex", tex);
			}
		}
		GetComponent<Renderer>().material.mainTexture = tex;
	}

	protected void SyncVideoTexture()
	{
		if (!(videoPlayer.texture != null) || !(videoPlayer.texture != currentVideoTexture))
		{
			return;
		}
		currentVideoTexture = videoPlayer.texture;
		SyncImageRatio(currentVideoTexture, true);
		if (!(rawImage == null) || !alsoSetSpecularTexture)
		{
			return;
		}
		MeshRenderer[] components = GetComponents<MeshRenderer>();
		MeshRenderer[] array = components;
		foreach (MeshRenderer meshRenderer in array)
		{
			Material[] array2 = ((!Application.isPlaying) ? meshRenderer.sharedMaterials : meshRenderer.materials);
			Material material = array2[0];
			if (material.HasProperty("_SpecTex"))
			{
				material.SetTexture("_SpecTex", currentVideoTexture);
			}
		}
	}

	protected string GetVideoCacheFileName(string path)
	{
		string result = null;
		FileEntry fileEntry = FileManager.GetFileEntry(path);
		if (fileEntry != null)
		{
			string text = fileEntry.Size.ToString();
			string text2 = fileEntry.LastWriteTime.ToFileTime().ToString();
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
			string extension = Path.GetExtension(path);
			fileNameWithoutExtension = fileNameWithoutExtension.Replace('.', '_');
			result = fileNameWithoutExtension + "_" + text + "_" + text2 + extension;
		}
		return result;
	}

	protected string GetVideoCachePath(string path)
	{
		string result = null;
		string videoCacheDir = CacheManager.GetVideoCacheDir();
		if (videoCacheDir != null)
		{
			string text = videoCacheDir + "/";
			string videoCacheFileName = GetVideoCacheFileName(path);
			if (videoCacheFileName != null)
			{
				result = text + videoCacheFileName;
			}
		}
		return result;
	}

	protected void DeleteTempVideo()
	{
		if (currentTempVideo != null)
		{
			File.Delete(currentTempVideo);
			currentTempVideo = null;
		}
	}

	protected string MakeTempVideo(string inputPath)
	{
		string result = null;
		try
		{
			Directory.CreateDirectory("Temp");
			DeleteTempVideo();
			currentTempVideo = "Temp/" + GetVideoCacheFileName(inputPath);
			FileManager.CopyFile(inputPath, currentTempVideo);
			result = currentTempVideo;
		}
		catch (Exception ex)
		{
			SuperController.LogError("Exception during creation of temporary copy of video " + inputPath + ": " + ex);
		}
		return result;
	}

	protected void SyncVideo()
	{
		if (!(videoPlayer != null))
		{
			return;
		}
		videoPlayer.enabled = true;
		string text = _url;
		if (FileManager.IsFileInPackage(text))
		{
			if (CacheManager.CachingEnabled)
			{
				try
				{
					string videoCachePath = GetVideoCachePath(text);
					if (videoCachePath == null)
					{
						throw new Exception("Unable to get cache path");
					}
					if (FileManager.FileExists(videoCachePath))
					{
						text = videoCachePath;
					}
					else
					{
						byte[] buffer = new byte[4096];
						using (FileEntryStream fileEntryStream = FileManager.OpenStream(text))
						{
							using (FileStream destination = File.Open(videoCachePath, FileMode.Create))
							{
								StreamUtils.Copy(fileEntryStream.Stream, destination, buffer);
								text = videoCachePath;
							}
						}
					}
				}
				catch (Exception ex)
				{
					SuperController.LogError("Exception during cache handling video " + text + ": " + ex);
					return;
				}
			}
			else
			{
				string text2 = MakeTempVideo(text);
				if (text2 == null)
				{
					return;
				}
				text = text2;
			}
		}
		if (File.Exists(text))
		{
			text = Path.GetFullPath(text);
		}
		videoPlayer.url = text;
		videoHadErrorJSON.val = false;
		videoIsLoadingJSON.val = true;
		videoPlayer.Prepare();
	}

	public bool IsVideoReady()
	{
		if (videoPlayer != null && videoPlayer.enabled)
		{
			return videoPlayer.isPrepared && videoPlayer.frameCount != 0;
		}
		return false;
	}

	public bool IsVideoPlaying()
	{
		if (videoPlayer != null && videoPlayer.enabled)
		{
			return videoPlayer.isPlaying;
		}
		return false;
	}

	protected void SyncPlayVideoWhenReady(bool b)
	{
		_playVideoWhenReady = b;
	}

	protected void SyncUseAnamorphicVideoAspectRatio(bool b)
	{
		_useAnamorphicVideoAspectRatio = b;
		if (videoPlayer != null && videoPlayer.isPrepared)
		{
			SyncImageRatio(currentVideoTexture, true);
		}
		else
		{
			SyncImageRatio(currentTexture);
		}
	}

	protected void SyncLoopVideo(bool b)
	{
		_loopVideo = b;
		if (videoPlayer != null)
		{
			videoPlayer.isLooping = _loopVideo;
		}
	}

	protected void SyncPlaybackTime(float f)
	{
		if (videoPlayer != null)
		{
			videoPlayer.time = f;
		}
	}

	protected void UpdateVideo()
	{
		if (!(videoPlayer != null))
		{
			return;
		}
		bool flag = videoPlayer.isPrepared && videoPlayer.frameCount != 0;
		if (videoIsReadyJSON != null)
		{
			videoIsReadyJSON.val = flag;
		}
		if (videoReadyTrigger != null)
		{
			videoReadyTrigger.active = flag;
		}
		if (videoStoppedTrigger != null && wasPlaying && !videoPlayer.isPlaying)
		{
			videoStoppedTrigger.active = true;
			videoStoppedTrigger.active = false;
		}
		wasPlaying = videoPlayer.isPlaying;
		if (playVideoAction != null && playVideoAction.button != null)
		{
			playVideoAction.button.gameObject.SetActive(!videoPlayer.isPlaying);
		}
		if (pauseVideoAction != null && pauseVideoAction.button != null)
		{
			pauseVideoAction.button.gameObject.SetActive(videoPlayer.isPlaying);
		}
		if (playbackTimeJSON != null)
		{
			float max = 0f;
			if (videoPlayer.frameRate > 0f)
			{
				max = (float)videoPlayer.frameCount / videoPlayer.frameRate;
			}
			playbackTimeJSON.max = max;
			if (videoPlayer.isPlaying)
			{
				playbackTimeJSON.valNoCallback = (float)videoPlayer.time;
			}
			playbackTimeJSON.SetInteractble(videoPlayer.canSetTime);
		}
	}

	public void PlayVideo()
	{
		if (videoPlayer != null)
		{
			if (!videoPlayer.isPrepared)
			{
				videoHadErrorJSON.val = false;
				videoIsLoadingJSON.val = true;
			}
			videoPlayer.Play();
		}
	}

	public void PauseVideo()
	{
		if (videoPlayer != null)
		{
			videoPlayer.Pause();
		}
	}

	public void StopVideo()
	{
		if (videoPlayer != null)
		{
			if (videoPlayer.canSetTime)
			{
				playbackTimeJSON.val = 0f;
				videoPlayer.Pause();
			}
			else
			{
				videoPlayer.Stop();
			}
		}
	}

	public void SeekToVideoStart()
	{
		if (videoPlayer != null)
		{
			if (videoPlayer.canSetTime)
			{
				playbackTimeJSON.val = 0f;
			}
			else if (videoPlayer.isPlaying)
			{
				videoPlayer.Stop();
				videoPlayer.Play();
			}
			else
			{
				videoPlayer.Stop();
			}
		}
	}

	protected void InitVideoPlayer()
	{
		videoPlayer = GetComponent<VideoPlayer>();
		if (videoPlayer != null)
		{
			videoPlayer.enabled = false;
			videoPlayer.errorReceived += VideoPlayer_errorReceived;
			videoPlayer.prepareCompleted += VideoPlayer_prepareCompleted;
			videoPlayer.started += VideoPlayer_started;
			videoPlayer.seekCompleted += VideoPlayer_seekCompleted;
		}
	}

	protected void VideoPlayer_seekCompleted(VideoPlayer source)
	{
	}

	protected void VideoPlayer_started(VideoPlayer source)
	{
		if (videoStartedTrigger != null)
		{
			videoStartedTrigger.active = true;
			videoStartedTrigger.active = false;
		}
	}

	protected void VideoPlayer_prepareCompleted(VideoPlayer source)
	{
		videoIsLoadingJSON.val = false;
		SyncVideoTexture();
		if (_playVideoWhenReady)
		{
			videoPlayer.Play();
		}
	}

	protected void VideoPlayer_errorReceived(VideoPlayer source, string message)
	{
		videoIsLoadingJSON.val = false;
		videoHadErrorJSON.val = true;
		SuperController.LogError("Error with video " + message);
	}

	protected void DisableVideoPlayer()
	{
		if (videoPlayer != null)
		{
			videoPlayer.enabled = false;
		}
		videoIsLoadingJSON.val = false;
		videoHadErrorJSON.val = false;
	}

	protected void OnAtomRename(string oldid, string newid)
	{
		if (videoReadyTrigger != null)
		{
			videoReadyTrigger.SyncAtomNames();
		}
		if (videoStartedTrigger != null)
		{
			videoStartedTrigger.SyncAtomNames();
		}
		if (videoStoppedTrigger != null)
		{
			videoStoppedTrigger.SyncAtomNames();
		}
	}

	protected void ClearImage()
	{
		if (blankTexture != null)
		{
			SyncTexture(blankTexture);
		}
		else
		{
			SyncTexture(null);
		}
	}

	private IEnumerator SyncImage()
	{
		Texture2D tex = new Texture2D(4, 4, TextureFormat.DXT5, true);
		string urltoload = _url;
		if (!Regex.IsMatch(urltoload, "^http") && !Regex.IsMatch(urltoload, "^file"))
		{
			urltoload = ((!urltoload.Contains(":/")) ? ("file:///.\\" + urltoload) : ("file:///" + urltoload));
		}
		WWW www = new WWW(urltoload);
		yield return www;
		if (www.error == null || www.error == string.Empty)
		{
			www.LoadImageIntoTexture(tex);
			if (createdTexture != null)
			{
				UnityEngine.Object.Destroy(createdTexture);
			}
			createdTexture = tex;
			SyncTexture(tex);
		}
		else
		{
			SuperController.LogError("Could not load image at " + urltoload + " Error: " + www.error);
		}
	}

	protected void OnImageLoaded(ImageLoaderThreaded.QueuedImage qi)
	{
		if (qi.tex != null && this != null)
		{
			SyncTexture(qi.tex);
			ImageLoaderThreaded.singleton.RegisterTextureUse(qi.tex);
			if (registeredTexture != null)
			{
				ImageLoaderThreaded.singleton.DeregisterTextureUse(registeredTexture);
			}
			registeredTexture = qi.tex;
		}
	}

	protected void SyncAllowTiling()
	{
		if (currentTexture != null)
		{
			if (_allowTiling)
			{
				currentTexture.wrapMode = TextureWrapMode.Repeat;
			}
			else
			{
				currentTexture.wrapMode = TextureWrapMode.Clamp;
			}
		}
	}

	protected void SyncAllowTiling(bool b)
	{
		_allowTiling = b;
		SyncAllowTiling();
	}

	protected void Init()
	{
		videoReadyTrigger = new Trigger();
		videoReadyTrigger.handler = this;
		videoStartedTrigger = new Trigger();
		videoStartedTrigger.handler = this;
		videoStoppedTrigger = new Trigger();
		videoStoppedTrigger.handler = this;
		InitVideoPlayer();
		if (!FileManager.DirectoryExists("Custom/Images"))
		{
			FileManager.CreateDirectory("Custom/Images");
		}
		if ((bool)SuperController.singleton)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Combine(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRename));
		}
		allowTilingJSON = new JSONStorableBool("allowTiling", _allowTiling, SyncAllowTiling);
		RegisterBool(allowTilingJSON);
		playVideoWhenReadyJSON = new JSONStorableBool("playVideoWhenReady", _playVideoWhenReady, SyncPlayVideoWhenReady);
		RegisterBool(playVideoWhenReadyJSON);
		useAnamorphicVideoAspectRatioJSON = new JSONStorableBool("useAnamorphicVideoAspectRatio", _useAnamorphicVideoAspectRatio, SyncUseAnamorphicVideoAspectRatio);
		RegisterBool(useAnamorphicVideoAspectRatioJSON);
		videoIsReadyJSON = new JSONStorableBool("videoIsReady", false);
		videoIsLoadingJSON = new JSONStorableBool("videoIsLoading", false);
		videoIsLoadingJSON.RegisterIndicator(videoIsLoadingIndicator, true);
		videoHadErrorJSON = new JSONStorableBool("videoHadError", false);
		videoHadErrorJSON.RegisterIndicator(videoHadErrorIndicator, true);
		loopVideoJSON = new JSONStorableBool("loopVideo", _loopVideo, SyncLoopVideo);
		SyncLoopVideo(_loopVideo);
		RegisterBool(loopVideoJSON);
		playbackTimeJSON = new JSONStorableFloat("playbackTime", 0f, SyncPlaybackTime, 0f, 0f);
		playbackTimeJSON.isRestorable = false;
		playbackTimeJSON.isStorable = false;
		RegisterFloat(playbackTimeJSON);
		playVideoAction = new JSONStorableAction("PlayVideo", PlayVideo);
		RegisterAction(playVideoAction);
		pauseVideoAction = new JSONStorableAction("PauseVideo", PauseVideo);
		RegisterAction(pauseVideoAction);
		stopVideoAction = new JSONStorableAction("StopVideo", StopVideo);
		RegisterAction(stopVideoAction);
		seekToVideoStartAction = new JSONStorableAction("SeekToVideoStart", SeekToVideoStart);
		RegisterAction(seekToVideoStartAction);
		if (videoPlayer != null)
		{
			urlJSON = new JSONStorableUrl("url", _url, SyncUrl, "jpg|jpeg|png|gif|avi|mp4", "Custom/Images");
		}
		else
		{
			urlJSON = new JSONStorableUrl("url", _url, SyncUrl, "jpg|jpeg|png|gif", "Custom/Images");
		}
		urlJSON.beginBrowseWithObjectCallback = BeginBrowse;
		urlJSON.disableOnEndEdit = true;
		RegisterUrl(urlJSON);
	}

	protected override void InitUI(Transform t, bool isAlt)
	{
		if (t != null)
		{
			ImageControlUI componentInChildren = t.GetComponentInChildren<ImageControlUI>(true);
			if (componentInChildren != null)
			{
				urlJSON.RegisterInputField(componentInChildren.urlInputField, isAlt);
				urlJSON.RegisterInputFieldAction(componentInChildren.urlInputFieldAction, isAlt);
				urlJSON.RegisterCopyToClipboardButton(componentInChildren.copyToClipboardButton, isAlt);
				urlJSON.RegisterCopyFromClipboardButton(componentInChildren.copyFromClipboardButton, isAlt);
				urlJSON.RegisterFileBrowseButton(componentInChildren.fileBrowseButton, isAlt);
				urlJSON.RegisterSetValToInputFieldButton(componentInChildren.loadButton, isAlt);
				urlJSON.RegisterClearValButton(componentInChildren.clearUrlButton, isAlt);
				allowTilingJSON.RegisterToggle(componentInChildren.allowImageTilingToggle, isAlt);
				playVideoWhenReadyJSON.RegisterToggle(componentInChildren.playVideoWhenReadyToggle, isAlt);
				useAnamorphicVideoAspectRatioJSON.RegisterToggle(componentInChildren.useAnamorphicVideoAspectRatioToggle, isAlt);
				videoIsReadyJSON.RegisterIndicator(componentInChildren.videoIsReadyIndicator, isAlt);
				videoIsLoadingJSON.RegisterIndicator(componentInChildren.videoIsLoadingIndicator, isAlt);
				videoHadErrorJSON.RegisterIndicator(componentInChildren.videoHadErrorIndicator, isAlt);
				loopVideoJSON.RegisterToggle(componentInChildren.loopVideoToggle, isAlt);
				playbackTimeJSON.RegisterSlider(componentInChildren.playbackTimeSilder, isAlt);
				playVideoAction.RegisterButton(componentInChildren.playVideoButton, isAlt);
				pauseVideoAction.RegisterButton(componentInChildren.pauseVideoButton, isAlt);
				stopVideoAction.RegisterButton(componentInChildren.stopVideoButton, isAlt);
				seekToVideoStartAction.RegisterButton(componentInChildren.seekToVideoStartButton, isAlt);
			}
			VideoReadyTriggerUI componentInChildren2 = t.GetComponentInChildren<VideoReadyTriggerUI>(true);
			if (componentInChildren2 != null)
			{
				videoReadyTrigger.triggerActionsParent = componentInChildren2.transform;
				videoReadyTrigger.triggerPanel = componentInChildren2.transform;
				videoReadyTrigger.triggerActionsPanel = componentInChildren2.transform;
				videoReadyTrigger.InitTriggerUI();
				videoReadyTrigger.InitTriggerActionsUI();
			}
			VideoStartedTriggerUI componentInChildren3 = t.GetComponentInChildren<VideoStartedTriggerUI>(true);
			if (componentInChildren3 != null)
			{
				videoStartedTrigger.triggerActionsParent = componentInChildren3.transform;
				videoStartedTrigger.triggerPanel = componentInChildren3.transform;
				videoStartedTrigger.triggerActionsPanel = componentInChildren3.transform;
				videoStartedTrigger.InitTriggerUI();
				videoStartedTrigger.InitTriggerActionsUI();
			}
			VideoStoppedTriggerUI componentInChildren4 = t.GetComponentInChildren<VideoStoppedTriggerUI>(true);
			if (componentInChildren4 != null)
			{
				videoStoppedTrigger.triggerActionsParent = componentInChildren4.transform;
				videoStoppedTrigger.triggerPanel = componentInChildren4.transform;
				videoStoppedTrigger.triggerActionsPanel = componentInChildren4.transform;
				videoStoppedTrigger.InitTriggerUI();
				videoStoppedTrigger.InitTriggerActionsUI();
			}
		}
	}

	protected void CheckEnable()
	{
		bool flag = true;
		if (UserPreferences.singleton != null)
		{
			flag = UserPreferences.singleton.enableWebMisc;
		}
		if (!wasWebEnabled && flag)
		{
			SyncUrl(_url);
		}
		wasWebEnabled = flag;
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

	protected void OnEnable()
	{
		if (notActiveOnSync)
		{
			notActiveOnSync = false;
			StartSyncUrl();
		}
	}

	protected void OnDisable()
	{
	}

	protected void Update()
	{
		CheckEnable();
		if (videoReadyTrigger != null)
		{
			videoReadyTrigger.Update();
		}
		if (videoStartedTrigger != null)
		{
			videoStartedTrigger.Update();
		}
		if (videoStoppedTrigger != null)
		{
			videoStoppedTrigger.Update();
		}
		UpdateVideo();
	}

	protected void OnDestroy()
	{
		DeleteTempVideo();
		if (createdTexture != null)
		{
			UnityEngine.Object.Destroy(createdTexture);
		}
		if (registeredTexture != null)
		{
			ImageLoaderThreaded.singleton.DeregisterTextureUse(registeredTexture);
		}
		if (videoReadyTrigger != null)
		{
			videoReadyTrigger.Remove();
		}
		if (videoStartedTrigger != null)
		{
			videoStartedTrigger.Remove();
		}
		if (videoStoppedTrigger != null)
		{
			videoStoppedTrigger.Remove();
		}
		if ((bool)SuperController.singleton)
		{
			SuperController singleton = SuperController.singleton;
			singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Remove(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRename));
		}
	}

	public void Start()
	{
		StartSyncUrl();
	}
}
