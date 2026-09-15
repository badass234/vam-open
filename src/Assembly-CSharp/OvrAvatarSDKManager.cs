using System;
using System.Collections.Generic;
using Oculus.Avatar;
using UnityEngine;

public class OvrAvatarSDKManager : MonoBehaviour
{
	private static OvrAvatarSDKManager _instance;

	private Dictionary<ulong, HashSet<specificationCallback>> specificationCallbacks;

	private Dictionary<ulong, HashSet<assetLoadedCallback>> assetLoadedCallbacks;

	private Dictionary<ulong, OvrAvatarAsset> assetCache;

	public static OvrAvatarSDKManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = UnityEngine.Object.FindObjectOfType<OvrAvatarSDKManager>();
				if (_instance == null)
				{
					GameObject gameObject = new GameObject("OvrAvatarSDKManager");
					_instance = gameObject.AddComponent<OvrAvatarSDKManager>();
					_instance.Initialize();
				}
			}
			return _instance;
		}
	}

	private void Initialize()
	{
		string text = OvrAvatarSettings.AppID;
		if (text == string.Empty)
		{
			Debug.Log("No Oculus Rift App ID has been provided. Go to Oculus Avatar > Edit Configuration to supply one", OvrAvatarSettings.Instance);
			text = "0";
		}
		CAPI.ovrAvatar_Initialize(text);
		specificationCallbacks = new Dictionary<ulong, HashSet<specificationCallback>>();
		assetLoadedCallbacks = new Dictionary<ulong, HashSet<assetLoadedCallback>>();
		assetCache = new Dictionary<ulong, OvrAvatarAsset>();
	}

	private void OnDestroy()
	{
		CAPI.ovrAvatar_Shutdown();
	}

	private void Update()
	{
		IntPtr intPtr = CAPI.ovrAvatarMessage_Pop();
		if (intPtr == IntPtr.Zero)
		{
			return;
		}
		ovrAvatarMessageType ovrAvatarMessageType2 = CAPI.ovrAvatarMessage_GetType(intPtr);
		switch (ovrAvatarMessageType2)
		{
		case ovrAvatarMessageType.AssetLoaded:
		{
			ovrAvatarMessage_AssetLoaded ovrAvatarMessage_AssetLoaded2 = CAPI.ovrAvatarMessage_GetAssetLoaded(intPtr);
			IntPtr asset = ovrAvatarMessage_AssetLoaded2.asset;
			ulong assetID = ovrAvatarMessage_AssetLoaded2.assetID;
			ovrAvatarAssetType ovrAvatarAssetType2 = CAPI.ovrAvatarAsset_GetType(asset);
			OvrAvatarAsset ovrAvatarAsset;
			switch (ovrAvatarAssetType2)
			{
			case ovrAvatarAssetType.Mesh:
				ovrAvatarAsset = new OvrAvatarAssetMesh(assetID, asset);
				break;
			case ovrAvatarAssetType.Texture:
				ovrAvatarAsset = new OvrAvatarAssetTexture(assetID, asset);
				break;
			case ovrAvatarAssetType.Material:
				ovrAvatarAsset = new OvrAvatarAssetMaterial(assetID, asset);
				break;
			default:
				throw new NotImplementedException($"Unsupported asset type format {ovrAvatarAssetType2.ToString()}");
			}
			HashSet<assetLoadedCallback> value2;
			if (assetLoadedCallbacks.TryGetValue(ovrAvatarMessage_AssetLoaded2.assetID, out value2))
			{
				assetCache.Add(assetID, ovrAvatarAsset);
				foreach (assetLoadedCallback item in value2)
				{
					item(ovrAvatarAsset);
				}
				assetLoadedCallbacks.Remove(ovrAvatarMessage_AssetLoaded2.assetID);
			}
			else
			{
				Debug.LogWarning("Loaded an asset with no owner: " + ovrAvatarMessage_AssetLoaded2.assetID);
			}
			break;
		}
		case ovrAvatarMessageType.AvatarSpecification:
		{
			ovrAvatarMessage_AvatarSpecification ovrAvatarMessage_AvatarSpecification2 = CAPI.ovrAvatarMessage_GetAvatarSpecification(intPtr);
			HashSet<specificationCallback> value;
			if (specificationCallbacks.TryGetValue(ovrAvatarMessage_AvatarSpecification2.oculusUserID, out value))
			{
				foreach (specificationCallback item2 in value)
				{
					item2(ovrAvatarMessage_AvatarSpecification2.avatarSpec);
				}
				specificationCallbacks.Remove(ovrAvatarMessage_AvatarSpecification2.oculusUserID);
			}
			else
			{
				Debug.LogWarning("Error, got an avatar specification callback from a user id we don't have a record for: " + ovrAvatarMessage_AvatarSpecification2.oculusUserID);
			}
			break;
		}
		default:
			throw new NotImplementedException("Unhandled ovrAvatarMessageType: " + ovrAvatarMessageType2);
		}
		CAPI.ovrAvatarMessage_Free(intPtr);
	}

	public void RequestAvatarSpecification(ulong userId, specificationCallback callback)
	{
		HashSet<specificationCallback> value;
		if (!specificationCallbacks.TryGetValue(userId, out value))
		{
			value = new HashSet<specificationCallback>();
			specificationCallbacks.Add(userId, value);
			IntPtr specificationRequest = CAPI.ovrAvatarSpecificationRequest_Create(userId);
			CAPI.ovrAvatar_RequestAvatarSpecificationFromSpecRequest(specificationRequest);
			CAPI.ovrAvatarSpecificationRequest_Destroy(specificationRequest);
		}
		value.Add(callback);
	}

	public void BeginLoadingAsset(ulong assetId, assetLoadedCallback callback)
	{
		HashSet<assetLoadedCallback> value;
		if (!assetLoadedCallbacks.TryGetValue(assetId, out value))
		{
			value = new HashSet<assetLoadedCallback>();
			assetLoadedCallbacks.Add(assetId, value);
			CAPI.ovrAvatarAsset_BeginLoading(assetId);
		}
		if (value.Add(callback))
		{
			value.Add(callback);
		}
	}

	public OvrAvatarAsset GetAsset(ulong assetId)
	{
		OvrAvatarAsset value;
		if (assetCache.TryGetValue(assetId, out value))
		{
			return value;
		}
		return null;
	}
}
