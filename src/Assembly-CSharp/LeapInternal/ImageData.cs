using Leap;

namespace LeapInternal
{
	public class ImageData
	{
		private LEAP_IMAGE_PROPERTIES _properties;

		private object _object;

		public Image.CameraType camera { get; protected set; }

		public eLeapImageType type => _properties.type;

		public eLeapImageFormat format => _properties.format;

		public uint bpp => _properties.bpp;

		public uint width => _properties.width;

		public uint height => _properties.height;

		public float RayScaleX => _properties.x_scale;

		public float RayScaleY => _properties.y_scale;

		public float RayOffsetX => _properties.x_offset;

		public float RayOffsetY => _properties.y_offset;

		public byte[] AsByteArray => _object as byte[];

		public float[] AsFloatArray => _object as float[];

		public uint byteOffset { get; protected set; }

		public int DistortionSize => LeapC.DistortionSize;

		public ulong DistortionMatrixKey { get; protected set; }

		public DistortionData DistortionData { get; protected set; }

		public ImageData(Image.CameraType camera, LEAP_IMAGE image, DistortionData distortionData)
		{
			this.camera = camera;
			_properties = image.properties;
			DistortionMatrixKey = image.matrix_version;
			DistortionData = distortionData;
			_object = MemoryManager.GetPinnedObject(image.data);
			byteOffset = image.offset;
		}
	}
}
