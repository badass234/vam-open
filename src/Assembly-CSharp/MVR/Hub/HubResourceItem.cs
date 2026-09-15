using System;
using System.Text.RegularExpressions;
using MVR.FileManagement;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

namespace MVR.Hub
{
	public class HubResourceItem
	{
		protected HubBrowse browser;

		protected string resource_id;

		protected string discussion_thread_id;

		protected JSONStorableString titleJSON;

		protected JSONStorableString tagLineJSON;

		protected JSONStorableString versionNumberJSON;

		protected JSONStorableString payTypeJSON;

		protected JSONStorableString categoryJSON;

		protected JSONStorableAction payTypeAndCategorySelectAction;

		protected JSONStorableString creatorJSON;

		protected JSONStorableAction creatorSelectAction;

		protected RawImage creatorIconImage;

		protected Texture2D creatorIconTexture;

		protected bool useQueueImmediate;

		protected ImageLoaderThreaded.QueuedImage creatorIconQueuedImage;

		protected JSONStorableUrl creatorIconUrlJSON;

		protected RawImage thumbnailImage;

		protected Texture2D thumbnailTexture;

		protected ImageLoaderThreaded.QueuedImage thumbnailQueuedImage;

		protected JSONStorableUrl thumbnailUrlJSON;

		protected JSONStorableBool hubDownloadableJSON;

		protected JSONStorableBool hubHostedJSON;

		protected JSONStorableString dependencyCountJSON;

		protected JSONStorableBool hasDependenciesJSON;

		protected JSONStorableBool hasLicenseJSON;

		protected JSONStorableString licenseTypeJSON;

		protected Image licenseColorImage;

		protected JSONStorableString downloadCountJSON;

		protected JSONStorableString ratingsCountJSON;

		protected JSONStorableFloat ratingJSON;

		protected JSONStorableString lastUpdateJSON;

		protected JSONArray varFilesJSONArray;

		protected JSONStorableBool inLibraryJSON;

		protected JSONStorableBool updateAvailableJSON;

		protected JSONStorableString updateMsgJSON;

		protected static Color redColor = new Color(1f, 0.48f, 0.48f);

		protected static Color yellowColor = new Color(1f, 1f, 0.5f);

		protected static Color greenColor = new Color(0.52f, 1f, 0.52f);

		protected JSONStorableAction openDetailAction;

		public string ResourceId => resource_id;

		public string Title => titleJSON.val;

		public string TagLine => tagLineJSON.val;

		public string VersionNumber => versionNumberJSON.val;

		public string PayType => payTypeJSON.val;

		public string Category => categoryJSON.val;

		public string Creator => creatorJSON.val;

		public int DownloadCount
		{
			get
			{
				int result;
				if (int.TryParse(downloadCountJSON.val, out result))
				{
					return result;
				}
				return 0;
			}
		}

		public int RatingsCount
		{
			get
			{
				int result;
				if (int.TryParse(ratingsCountJSON.val, out result))
				{
					return result;
				}
				return 0;
			}
		}

		public float Rating => ratingJSON.val;

		public DateTime LastUpdateTimestamp { get; protected set; }

		public HubResourceItem(JSONClass resource, HubBrowse hubBrowse, bool queueImagesImmediate = false)
		{
			useQueueImmediate = queueImagesImmediate;
			browser = hubBrowse;
			resource_id = resource["resource_id"];
			discussion_thread_id = resource["discussion_thread_id"];
			string startingValue = resource["title"];
			string startingValue2 = resource["tag_line"];
			string startingValue3 = resource["version_string"];
			string startingValue4 = resource["category"];
			string startingValue5 = resource["type"];
			string startingValue6 = resource["username"];
			string text = resource["icon_url"];
			string text2 = resource["image_url"];
			bool asBool = resource["hubDownloadable"].AsBool;
			bool asBool2 = resource["hubHosted"].AsBool;
			int asInt = resource["dependency_count"].AsInt;
			bool startingValue7 = asInt > 0;
			string startingValue8 = resource["download_count"];
			string startingValue9 = resource["rating_count"];
			float asFloat = resource["rating_avg"].AsFloat;
			int asInt2 = resource["last_update"].AsInt;
			LastUpdateTimestamp = UnixTimeStampToDateTime(asInt2);
			string startingValue10 = ((!((DateTime.Now - LastUpdateTimestamp).TotalDays > 7.0)) ? LastUpdateTimestamp.ToString("dddd h:mm tt") : LastUpdateTimestamp.ToString("MMM d, yyyy"));
			varFilesJSONArray = resource["hubFiles"].AsArray;
			titleJSON = new JSONStorableString("title", startingValue);
			tagLineJSON = new JSONStorableString("tagLine", startingValue2);
			versionNumberJSON = new JSONStorableString("versionNumber", startingValue3);
			payTypeJSON = new JSONStorableString("payType", startingValue4);
			categoryJSON = new JSONStorableString("category", startingValue5);
			payTypeAndCategorySelectAction = new JSONStorableAction("PayTypeAndCategorySelect", PayTypeAndCategorySelect);
			creatorJSON = new JSONStorableString("creator", startingValue6);
			creatorSelectAction = new JSONStorableAction("CreatorSelect", CreatorSelect);
			creatorIconUrlJSON = new JSONStorableUrl("creatorIconUrl", text, SyncCreatorIconUrl);
			SyncCreatorIconUrl(text);
			thumbnailUrlJSON = new JSONStorableUrl("thumbnailUrl", text2, SyncThumbnailUrl);
			SyncThumbnailUrl(text2);
			hubDownloadableJSON = new JSONStorableBool("hubDownloadable", asBool);
			hubHostedJSON = new JSONStorableBool("hubHosted", asBool2);
			hasDependenciesJSON = new JSONStorableBool("hasDependencies", startingValue7);
			dependencyCountJSON = new JSONStorableString("dependencyCount", asInt + " Hub-Hosted Dependencies");
			downloadCountJSON = new JSONStorableString("downloadCount", startingValue8);
			ratingsCountJSON = new JSONStorableString("ratingsCount", startingValue9);
			ratingJSON = new JSONStorableFloat("rating", asFloat, 0f, 5f, true, false);
			lastUpdateJSON = new JSONStorableString("lastUpdate", startingValue10);
			openDetailAction = new JSONStorableAction("OpenDetail", OpenDetail);
			inLibraryJSON = new JSONStorableBool("inLibrary", false);
			updateAvailableJSON = new JSONStorableBool("updateAvailable", false);
			updateMsgJSON = new JSONStorableString("updateMsg", "Update Available");
			licenseTypeJSON = new JSONStorableString("licenseType", string.Empty);
			hasLicenseJSON = new JSONStorableBool("hasLicense", false);
		}

		protected void PayTypeAndCategorySelect()
		{
			browser.SetPayTypeAndCategoryFilter(payTypeJSON.val, categoryJSON.val);
		}

		protected void CreatorSelect()
		{
			browser.CreatorFilterOnly = creatorJSON.val;
		}

		protected void SyncCreatorIconTexture(ImageLoaderThreaded.QueuedImage qi)
		{
			creatorIconTexture = qi.tex;
			if (creatorIconImage != null && creatorIconTexture != null)
			{
				creatorIconImage.texture = creatorIconTexture;
			}
		}

		protected void SyncCreatorIconUrl(string url)
		{
			if (ImageLoaderThreaded.singleton != null && url != null && url != string.Empty)
			{
				ImageLoaderThreaded.QueuedImage queuedImage = new ImageLoaderThreaded.QueuedImage();
				queuedImage.imgPath = url;
				queuedImage.callback = SyncCreatorIconTexture;
				creatorIconQueuedImage = queuedImage;
				if (useQueueImmediate)
				{
					ImageLoaderThreaded.singleton.QueueThumbnailImmediate(queuedImage);
				}
				else
				{
					ImageLoaderThreaded.singleton.QueueThumbnail(queuedImage);
				}
			}
		}

		protected void SyncThumbnailTexture(ImageLoaderThreaded.QueuedImage qi)
		{
			thumbnailTexture = qi.tex;
			if (thumbnailImage != null && thumbnailTexture != null)
			{
				thumbnailImage.texture = thumbnailTexture;
			}
		}

		protected void SyncThumbnailUrl(string url)
		{
			if (ImageLoaderThreaded.singleton != null && url != null && url != string.Empty)
			{
				ImageLoaderThreaded.QueuedImage queuedImage = new ImageLoaderThreaded.QueuedImage();
				queuedImage.imgPath = url;
				queuedImage.callback = SyncThumbnailTexture;
				thumbnailQueuedImage = queuedImage;
				if (useQueueImmediate)
				{
					ImageLoaderThreaded.singleton.QueueThumbnailImmediate(queuedImage);
				}
				else
				{
					ImageLoaderThreaded.singleton.QueueThumbnail(queuedImage);
				}
			}
		}

		protected DateTime UnixTimeStampToDateTime(int unixTimeStamp)
		{
			return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimeStamp).ToLocalTime();
		}

		protected void SyncLicenseColor()
		{
			if (licenseColorImage != null && licenseTypeJSON.val != string.Empty)
			{
				switch (licenseTypeJSON.val)
				{
				case "FC":
				case "CC BY":
				case "CC BY-SA":
					licenseColorImage.color = greenColor;
					break;
				case "CC BY-ND":
					licenseColorImage.color = yellowColor;
					break;
				default:
					licenseColorImage.color = redColor;
					break;
				}
			}
		}

		public virtual void Refresh()
		{
			if (hubDownloadableJSON.val && varFilesJSONArray != null && varFilesJSONArray.Count > 0)
			{
				bool val = true;
				bool val2 = false;
				string text = string.Empty;
				bool val3 = false;
				foreach (JSONNode item in varFilesJSONArray)
				{
					string text2 = item["filename"];
					if (text2 == null)
					{
						continue;
					}
					text = item["licenseType"];
					val3 = text != null;
					text2 = Regex.Replace(text2, ".var$", string.Empty);
					string packageGroupUid = Regex.Replace(text2, "(.*)\\..*", "$1");
					string s = Regex.Replace(text2, ".*\\.([0-9]+)$", "$1");
					int result;
					if (!int.TryParse(s, out result))
					{
						continue;
					}
					VarPackage package = FileManager.GetPackage(text2);
					if (package == null)
					{
						VarPackageGroup packageGroup = FileManager.GetPackageGroup(packageGroupUid);
						if (packageGroup == null || packageGroup.NewestPackage == null)
						{
							val = false;
							break;
						}
						if (packageGroup.NewestPackage.Version < result)
						{
							val2 = true;
							updateMsgJSON.val = "Update Available " + packageGroup.NewestEnabledPackage.Version + " -> " + result;
						}
					}
				}
				inLibraryJSON.val = val;
				updateAvailableJSON.val = val2;
				licenseTypeJSON.val = text;
				hasLicenseJSON.val = val3;
				SyncLicenseColor();
			}
			else
			{
				inLibraryJSON.val = false;
				updateAvailableJSON.val = false;
				licenseTypeJSON.val = string.Empty;
				hasLicenseJSON.val = false;
			}
		}

		public void OpenDetail()
		{
			browser.OpenDetail(resource_id);
		}

		public void Hide()
		{
			if (creatorIconQueuedImage != null)
			{
				creatorIconQueuedImage.cancel = true;
			}
			if (thumbnailQueuedImage != null)
			{
				thumbnailQueuedImage.cancel = true;
			}
		}

		public void Show()
		{
			if (creatorIconQueuedImage != null && !creatorIconQueuedImage.preprocessed)
			{
				creatorIconQueuedImage.cancel = false;
				if (useQueueImmediate)
				{
					ImageLoaderThreaded.singleton.QueueThumbnailImmediate(creatorIconQueuedImage);
				}
				else
				{
					ImageLoaderThreaded.singleton.QueueThumbnail(creatorIconQueuedImage);
				}
			}
			if (thumbnailQueuedImage != null && !thumbnailQueuedImage.preprocessed)
			{
				thumbnailQueuedImage.cancel = false;
				if (useQueueImmediate)
				{
					ImageLoaderThreaded.singleton.QueueThumbnailImmediate(thumbnailQueuedImage);
				}
				else
				{
					ImageLoaderThreaded.singleton.QueueThumbnail(thumbnailQueuedImage);
				}
			}
		}

		public void Destroy()
		{
			if (creatorIconQueuedImage != null)
			{
				creatorIconQueuedImage.cancel = true;
			}
			if (thumbnailQueuedImage != null)
			{
				thumbnailQueuedImage.cancel = true;
			}
		}

		public virtual void RegisterUI(HubResourceItemUI ui)
		{
			if (ui != null)
			{
				ui.connectedItem = this;
				titleJSON.text = ui.titleText;
				tagLineJSON.text = ui.tagLineText;
				versionNumberJSON.text = ui.versionText;
				payTypeJSON.text = ui.payTypeText;
				categoryJSON.text = ui.categoryText;
				payTypeAndCategorySelectAction.button = ui.payTypeAndCategorySelectButton;
				creatorSelectAction.button = ui.creatorSelectButton;
				creatorJSON.text = ui.creatorText;
				creatorIconImage = ui.creatorIconImage;
				if (creatorIconImage != null && creatorIconTexture != null)
				{
					creatorIconImage.texture = creatorIconTexture;
				}
				thumbnailImage = ui.thumbnailImage;
				if (thumbnailImage != null && thumbnailTexture != null)
				{
					thumbnailImage.texture = thumbnailTexture;
				}
				hubDownloadableJSON.indicator = ui.hubDownloadableIndicator;
				hubDownloadableJSON.negativeIndicator = ui.hubDownloadableNegativeIndicator;
				hubHostedJSON.indicator = ui.hubHostedIndicator;
				hubHostedJSON.negativeIndicator = ui.hubHostedNegativeIndicator;
				hasDependenciesJSON.indicator = ui.hasDependenciesIndicator;
				hasDependenciesJSON.negativeIndicator = ui.hasDependenciesNegativeIndicator;
				dependencyCountJSON.text = ui.dependencyCountText;
				downloadCountJSON.text = ui.downloadCountText;
				ratingsCountJSON.text = ui.ratingsCountText;
				ratingJSON.slider = ui.ratingSlider;
				lastUpdateJSON.text = ui.lastUpdateText;
				openDetailAction.button = ui.openDetailButton;
				inLibraryJSON.indicator = ui.inLibraryIndicator;
				licenseColorImage = ui.licenseColorImage;
				updateAvailableJSON.indicator = ui.updateAvailableIndicator;
				updateMsgJSON.text = ui.updateMsgText;
				hasLicenseJSON.indicator = ui.hasLicenseIndicator;
				licenseTypeJSON.text = ui.licenseTypeText;
				SyncLicenseColor();
			}
		}
	}
}
