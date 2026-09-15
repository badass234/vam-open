using UnityEngine;
using UnityEngine.UI;

namespace MeshVR
{
	public class DAZRuntimeCreatorUI : PresetManagerControlUI
	{
		public Text dufFileUrlText;

		public Button dufFileBrowseButton;

		public Button clearButton;

		public Button importButton;

		public Button cancelButton;

		public Text importVertexCountText;

		public Toggle combineMaterialsToggle;

		public Toggle wrapToMorphedVerticesToggle;

		public Toggle disableAnatomyToggle;

		public Text importMessageText;

		public Text simVertexCountText;

		public Text simJointCountText;

		public Text simNearbyJointCountText;

		public Button generateSimButton;

		public Button cancelGenerateSimButton;

		public Button selectClothSimTextureButton;

		public Button clearClothSimTextureButton;

		public Slider uniformClothSimTextureValueSlider;

		public Button setUniformClothSimTextureButton;

		public RawImage clothSimTextureRawImage;

		public Toggle clothSimUseIndividualSimTexturesToggle;

		public Toggle clothSimEnabledToggle;

		public Toggle clothSimCreateNearbyJointsToggle;

		public Slider clothSimNearbyJointsDistanceSlider;

		public UIPopup scalpChooserPopup;

		public Slider hairSimSegmentsSlider;

		public Slider hairSimSegmentsLengthSlider;

		public Slider scalpMaskSelectableSizeSlider;

		public Button scalpMaskSetAllButton;

		public Button scalpMaskClearAllButton;

		public Button startScalpMaskEditModeButton;

		public Button cancelScalpMaskEditModeButton;

		public Button finishScalpMaskEditModeButton;

		public Toggle scalpMaskEditModeHideBackfacesToggle;

		public Toggle autoSetStoreFolderNameToDufToggle;

		public InputField storeFolderNameField;

		public Toggle autoSetStoreNameToDufToggle;

		public Text packageNameText;

		public Button clearPackageButton;

		public InputField storeNameField;

		public InputField displayNameField;

		public InputField creatorNameField;

		public InputField tagsField;

		public RectTransform tagsPanel;

		public RectTransform regionTagsContent;

		public RectTransform typeTagsContent;

		public RectTransform otherTagsContent;

		public Button openTagsPanelButton;

		public Button closeTagsPanelButton;

		public Button browseStoreButton;

		public UIDynamicButton storeButton;

		public UIDynamicButton loadButton;

		public Button getScreenshotButton;

		public RawImage thumbnailRawImage;

		public Text storedCreatorNameText;

		public Text creatorStatusText;
	}
}
