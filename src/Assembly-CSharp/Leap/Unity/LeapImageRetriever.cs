using System;
using System.Collections;
using Leap.Unity.Query;
using LeapInternal;
using UnityEngine;
using UnityEngine.Serialization;

namespace Leap.Unity
{
	[RequireComponent(typeof(Camera))]
	[RequireComponent(typeof(LeapServiceProvider))]
	public class LeapImageRetriever : MonoBehaviour
	{
		public class LeapTextureData
		{
			private Texture2D _combinedTexture;

			private byte[] _intermediateArray;

			public Texture2D CombinedTexture => _combinedTexture;

			public bool CheckStale(Image image)
			{
				if (_combinedTexture == null || _intermediateArray == null)
				{
					return true;
				}
				if (image.Width != _combinedTexture.width || image.Height * 2 != _combinedTexture.height)
				{
					return true;
				}
				if (_combinedTexture.format != getTextureFormat(image))
				{
					return true;
				}
				return false;
			}

			public void Reconstruct(Image image, string globalShaderName, string pixelSizeName)
			{
				int width = image.Width;
				int num = image.Height * 2;
				TextureFormat textureFormat = getTextureFormat(image);
				if (_combinedTexture != null)
				{
					UnityEngine.Object.DestroyImmediate(_combinedTexture);
				}
				_combinedTexture = new Texture2D(width, num, textureFormat, false, true);
				_combinedTexture.wrapMode = TextureWrapMode.Clamp;
				_combinedTexture.filterMode = FilterMode.Bilinear;
				_combinedTexture.name = globalShaderName;
				_combinedTexture.hideFlags = HideFlags.DontSave;
				_intermediateArray = new byte[width * num * bytesPerPixel(textureFormat)];
				Shader.SetGlobalTexture(globalShaderName, _combinedTexture);
				Shader.SetGlobalVector(pixelSizeName, new Vector2(1f / (float)image.Width, 1f / (float)image.Height));
			}

			public void UpdateTexture(Image image)
			{
				_combinedTexture.LoadRawTextureData(image.Data(Image.CameraType.LEFT));
				_combinedTexture.Apply();
			}

			private TextureFormat getTextureFormat(Image image)
			{
				if (image.Format == Image.FormatType.INFRARED)
				{
					return TextureFormat.Alpha8;
				}
				throw new Exception(string.Concat("Unexpected image format ", image.Format, "!"));
			}

			private int bytesPerPixel(TextureFormat format)
			{
				if (format == TextureFormat.Alpha8)
				{
					return 1;
				}
				throw new Exception("Unexpected texture format " + format);
			}
		}

		public class LeapDistortionData
		{
			private Texture2D _combinedTexture;

			public Texture2D CombinedTexture => _combinedTexture;

			public bool CheckStale()
			{
				return _combinedTexture == null;
			}

			public void Reconstruct(Image image, string shaderName)
			{
				int num = image.DistortionWidth / 2;
				int num2 = image.DistortionHeight * 2;
				if (_combinedTexture != null)
				{
					UnityEngine.Object.DestroyImmediate(_combinedTexture);
				}
				Color32[] array = new Color32[num * num2];
				_combinedTexture = new Texture2D(num, num2, TextureFormat.RGBA32, false, true);
				_combinedTexture.filterMode = FilterMode.Bilinear;
				_combinedTexture.wrapMode = TextureWrapMode.Clamp;
				_combinedTexture.hideFlags = HideFlags.DontSave;
				addDistortionData(image, array, 0);
				_combinedTexture.SetPixels32(array);
				_combinedTexture.Apply();
				Shader.SetGlobalTexture(shaderName, _combinedTexture);
			}

			private void addDistortionData(Image image, Color32[] colors, int startIndex)
			{
				float[] array = image.Distortion(Image.CameraType.LEFT).Query().Concat(image.Distortion(Image.CameraType.RIGHT))
					.ToArray();
				for (int i = 0; i < array.Length; i += 2)
				{
					byte @byte;
					byte byte2;
					encodeFloat(array[i], out @byte, out byte2);
					byte byte3;
					byte byte4;
					encodeFloat(array[i + 1], out byte3, out byte4);
					colors[i / 2 + startIndex] = new Color32(@byte, byte2, byte3, byte4);
				}
			}

			private void encodeFloat(float value, out byte byte0, out byte byte1)
			{
				value = (value + 0.6f) / 2.3f;
				float num = value;
				float num2 = value * 255f;
				num -= (float)(int)num;
				num2 -= (float)(int)num2;
				num -= 0.003921569f * num2;
				byte0 = (byte)(num * 256f);
				byte1 = (byte)(num2 * 256f);
			}
		}

		public class EyeTextureData
		{
			private const string GLOBAL_RAW_TEXTURE_NAME = "_LeapGlobalRawTexture";

			private const string GLOBAL_DISTORTION_TEXTURE_NAME = "_LeapGlobalDistortion";

			private const string GLOBAL_RAW_PIXEL_SIZE_NAME = "_LeapGlobalRawPixelSize";

			public readonly LeapTextureData TextureData;

			public readonly LeapDistortionData Distortion;

			private bool _isStale;

			public EyeTextureData()
			{
				TextureData = new LeapTextureData();
				Distortion = new LeapDistortionData();
			}

			public static void ResetGlobalShaderValues()
			{
				Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false, false);
				texture2D.name = "EmptyTexture";
				texture2D.hideFlags = HideFlags.DontSave;
				texture2D.SetPixel(0, 0, new Color(0f, 0f, 0f, 0f));
				Shader.SetGlobalTexture("_LeapGlobalRawTexture", texture2D);
				Shader.SetGlobalTexture("_LeapGlobalDistortion", texture2D);
			}

			public bool CheckStale(Image image)
			{
				return TextureData.CheckStale(image) || Distortion.CheckStale() || _isStale;
			}

			public void MarkStale()
			{
				_isStale = true;
			}

			public void Reconstruct(Image image)
			{
				TextureData.Reconstruct(image, "_LeapGlobalRawTexture", "_LeapGlobalRawPixelSize");
				Distortion.Reconstruct(image, "_LeapGlobalDistortion");
				_isStale = false;
			}

			public void UpdateTextures(Image image)
			{
				TextureData.UpdateTexture(image);
			}
		}

		public const string GLOBAL_COLOR_SPACE_GAMMA_NAME = "_LeapGlobalColorSpaceGamma";

		public const string GLOBAL_GAMMA_CORRECTION_EXPONENT_NAME = "_LeapGlobalGammaCorrectionExponent";

		public const string GLOBAL_CAMERA_PROJECTION_NAME = "_LeapGlobalProjection";

		public const int IMAGE_WARNING_WAIT = 10;

		public const int LEFT_IMAGE_INDEX = 0;

		public const int RIGHT_IMAGE_INDEX = 1;

		public const float IMAGE_SETTING_POLL_RATE = 2f;

		[SerializeField]
		[FormerlySerializedAs("gammaCorrection")]
		private float _gammaCorrection = 1f;

		private LeapServiceProvider _provider;

		private EyeTextureData _eyeTextureData = new EyeTextureData();

		protected ProduceConsumeBuffer<Image> _imageQueue = new ProduceConsumeBuffer<Image>(32);

		protected Image _currentImage;

		private Coroutine _serviceCoroutine;

		public EyeTextureData TextureData => _eyeTextureData;

		private void Awake()
		{
			_provider = GetComponent<LeapServiceProvider>();
			if (_provider == null)
			{
				_provider = GetComponentInChildren<LeapServiceProvider>();
			}
			MemoryManager.EnablePooling = true;
			ApplyGammaCorrectionValues();
		}

		private void OnEnable()
		{
			subscribeToService();
		}

		private void OnDisable()
		{
			unsubscribeFromService();
		}

		private void OnDestroy()
		{
			StopAllCoroutines();
			Controller leapController = _provider.GetLeapController();
			if (leapController != null)
			{
				_provider.GetLeapController().DistortionChange -= onDistortionChange;
			}
		}

		private void LateUpdate()
		{
			Frame currentFrame = _provider.CurrentFrame;
			_currentImage = null;
			Image t;
			while (_imageQueue.TryPeek(out t) && t.SequenceId <= currentFrame.Id)
			{
				_currentImage = t;
				_imageQueue.TryDequeue();
			}
		}

		private void OnPreRender()
		{
			if (_currentImage != null)
			{
				if (_eyeTextureData.CheckStale(_currentImage))
				{
					_eyeTextureData.Reconstruct(_currentImage);
				}
				_eyeTextureData.UpdateTextures(_currentImage);
			}
		}

		private void subscribeToService()
		{
			if (_serviceCoroutine == null)
			{
				_serviceCoroutine = StartCoroutine(serviceCoroutine());
			}
		}

		private void unsubscribeFromService()
		{
			if (_serviceCoroutine != null)
			{
				StopCoroutine(_serviceCoroutine);
				_serviceCoroutine = null;
			}
			Controller leapController = _provider.GetLeapController();
			if (leapController != null)
			{
				leapController.ClearPolicy(Controller.PolicyFlag.POLICY_IMAGES);
				leapController.ImageReady -= onImageReady;
				leapController.DistortionChange -= onDistortionChange;
			}
		}

		private IEnumerator serviceCoroutine()
		{
			Controller controller = null;
			do
			{
				controller = _provider.GetLeapController();
				yield return null;
			}
			while (controller == null);
			controller.SetPolicy(Controller.PolicyFlag.POLICY_IMAGES);
			controller.ImageReady += onImageReady;
			controller.DistortionChange += onDistortionChange;
		}

		private void onImageReady(object sender, ImageEventArgs args)
		{
			Image image = args.image;
			_imageQueue.TryEnqueue(image);
		}

		public void ApplyGammaCorrectionValues()
		{
			float value = 1f;
			if (QualitySettings.activeColorSpace != ColorSpace.Linear)
			{
				value = 0f - Mathf.Log10(Mathf.GammaToLinearSpace(0.1f));
			}
			Shader.SetGlobalFloat("_LeapGlobalColorSpaceGamma", value);
			Shader.SetGlobalFloat("_LeapGlobalGammaCorrectionExponent", 1f / _gammaCorrection);
		}

		private void onDistortionChange(object sender, LeapEventArgs args)
		{
			_eyeTextureData.MarkStale();
		}
	}
}
