using System;
using LeapInternal;

namespace Leap
{
	public class Image
	{
		public enum FormatType
		{
			INFRARED,
			IBRG
		}

		public enum ImageType
		{
			DEFAULT,
			RAW
		}

		public enum CameraType
		{
			LEFT,
			RIGHT
		}

		private ImageData leftImage;

		private ImageData rightImage;

		private long frameId;

		private long timestamp;

		public uint NumBytes => leftImage.width * leftImage.height * leftImage.bpp;

		public long SequenceId => frameId;

		public int Width => (int)leftImage.width;

		public int Height => (int)leftImage.height;

		public int BytesPerPixel => (int)leftImage.bpp;

		public FormatType Format
		{
			get
			{
				switch (leftImage.format)
				{
				case eLeapImageFormat.eLeapImageType_IR:
					return FormatType.INFRARED;
				case eLeapImageFormat.eLeapImageType_RGBIr_Bayer:
					return FormatType.IBRG;
				default:
					return FormatType.INFRARED;
				}
			}
		}

		public ImageType Type
		{
			get
			{
				switch (leftImage.type)
				{
				case eLeapImageType.eLeapImageType_Default:
					return ImageType.DEFAULT;
				case eLeapImageType.eLeapImageType_Raw:
					return ImageType.RAW;
				default:
					return ImageType.DEFAULT;
				}
			}
		}

		public int DistortionWidth => leftImage.DistortionSize * 2;

		public int DistortionHeight => leftImage.DistortionSize;

		public long Timestamp => timestamp;

		public Image(long frameId, long timestamp, ImageData leftImage, ImageData rightImage)
		{
			if (leftImage == null || rightImage == null)
			{
				throw new ArgumentNullException("images");
			}
			if (leftImage.type != rightImage.type || leftImage.format != rightImage.format || leftImage.width != rightImage.width || leftImage.height != rightImage.height || leftImage.bpp != rightImage.bpp || leftImage.DistortionSize != rightImage.DistortionSize)
			{
				throw new ArgumentException("image mismatch");
			}
			this.frameId = frameId;
			this.timestamp = timestamp;
			this.leftImage = leftImage;
			this.rightImage = rightImage;
		}

		private ImageData imageData(CameraType camera)
		{
			return (camera != 0) ? rightImage : leftImage;
		}

		public byte[] Data(CameraType camera)
		{
			if (camera != 0 && camera != CameraType.RIGHT)
			{
				return null;
			}
			return imageData(camera).AsByteArray;
		}

		public uint ByteOffset(CameraType camera)
		{
			if (camera != 0 && camera != CameraType.RIGHT)
			{
				return 0u;
			}
			return imageData(camera).byteOffset;
		}

		public float[] Distortion(CameraType camera)
		{
			if (camera != 0 && camera != CameraType.RIGHT)
			{
				return null;
			}
			return imageData(camera).DistortionData.Data;
		}

		public Vector PixelToRectilinear(CameraType camera, Vector pixel)
		{
			return Connection.GetConnection().PixelToRectilinear(camera, pixel);
		}

		public Vector RectilinearToPixel(CameraType camera, Vector ray)
		{
			return Connection.GetConnection().RectilinearToPixel(camera, ray);
		}

		public bool Equals(Image other)
		{
			return frameId == other.frameId && Type == other.Type && Timestamp == other.Timestamp;
		}

		public override string ToString()
		{
			return string.Concat("Image sequence", frameId, ", format: ", Format, ", type: ", Type);
		}

		public float RayOffsetX(CameraType camera)
		{
			if (camera != 0 && camera != CameraType.RIGHT)
			{
				return 0f;
			}
			return imageData(camera).RayOffsetX;
		}

		public float RayOffsetY(CameraType camera)
		{
			if (camera != 0 && camera != CameraType.RIGHT)
			{
				return 0f;
			}
			return imageData(camera).RayOffsetY;
		}

		public float RayScaleX(CameraType camera)
		{
			if (camera != 0 && camera != CameraType.RIGHT)
			{
				return 0f;
			}
			return imageData(camera).RayScaleX;
		}

		public float RayScaleY(CameraType camera)
		{
			if (camera != 0 && camera != CameraType.RIGHT)
			{
				return 0f;
			}
			return imageData(camera).RayScaleY;
		}
	}
}
