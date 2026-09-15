using System;
using System.IO;

namespace OldMoatGames
{
	public class GifDecoder
	{
		public enum Status
		{
			StatusOk,
			StatusFormatError,
			StatusOpenError
		}

		public class GifFrame
		{
			public byte[] Image;

			public float Delay;

			public GifFrame(byte[] im, float del)
			{
				Image = im;
				Delay = del;
			}
		}

		private Stream _inStream;

		private Status _status;

		private int _width;

		private int _height;

		private bool _gctFlag;

		private int _gctSize;

		private int _loopCount = 1;

		private int[] _gct;

		private int[] _lct;

		private int[] _act;

		private int _bgIndex;

		private int _bgColor;

		private int _lastBgColor;

		private bool _lctFlag;

		private bool _interlace;

		private int _lctSize;

		private int _ix;

		private int _iy;

		private int _iw;

		private int _ih;

		private int[] _image;

		private byte[] _bimage;

		private readonly byte[] _block = new byte[256];

		private int _blockSize;

		private int _dispose;

		private int _lastDispose;

		private bool _transparency;

		private float _delay;

		private int _transIndex;

		private long _imageDataOffset;

		private const int MaxStackSize = 4096;

		private short[] _prefix;

		private byte[] _suffix;

		private byte[] _pixelStack;

		private byte[] _pixels;

		private GifFrame _currentFrame;

		private int _frameCount;

		public int NumberOfFrames { get; private set; }

		public bool AllFramesRead { get; private set; }

		public float GetDelayCurrentFrame()
		{
			return _currentFrame.Delay;
		}

		public int GetFrameCount()
		{
			return _frameCount;
		}

		public int GetLoopCount()
		{
			return _loopCount;
		}

		public GifFrame GetCurrentFrame()
		{
			return _currentFrame;
		}

		public int GetFrameWidth()
		{
			return _width;
		}

		public int GetFrameHeight()
		{
			return _height;
		}

		public Status Read(Stream inStream)
		{
			Init();
			if (inStream != null)
			{
				_inStream = inStream;
				ReadHeader();
				if (Error())
				{
					_status = Status.StatusFormatError;
				}
			}
			else
			{
				_status = Status.StatusOpenError;
			}
			return _status;
		}

		public void Reset()
		{
			_inStream.Position = 0L;
			Read(_inStream);
		}

		public void Close()
		{
			_inStream.Close();
		}

		private bool Error()
		{
			return _status != Status.StatusOk;
		}

		public void ReadNextFrame(bool loop)
		{
			while (!Error())
			{
				switch (Read())
				{
				case 44:
					ReadImage();
					return;
				case 33:
					switch (Read())
					{
					case 249:
						ReadGraphicControlExt();
						break;
					case 255:
					{
						ReadBlock();
						string text = string.Empty;
						for (int i = 0; i < 11; i++)
						{
							text += (char)_block[i];
						}
						if (text.Equals("NETSCAPE2.0"))
						{
							ReadNetscapeExt();
						}
						else
						{
							Skip();
						}
						break;
					}
					default:
						Skip();
						break;
					}
					break;
				case 59:
					NumberOfFrames = _frameCount;
					if (loop)
					{
						RewindReader();
						break;
					}
					AllFramesRead = true;
					return;
				default:
					_status = Status.StatusFormatError;
					break;
				case 0:
					break;
				}
			}
		}

		private void RewindReader()
		{
			_frameCount = 0;
			AllFramesRead = false;
			_inStream.Position = _imageDataOffset;
		}

		private void SetPixels()
		{
			if (_lastDispose > 0 && _lastDispose == 2)
			{
				int num = ((!_transparency) ? _lastBgColor : 0);
				for (int i = 0; i < _image.Length; i++)
				{
					_image[i] = num;
				}
			}
			int num2 = 1;
			int num3 = 8;
			int num4 = 0;
			for (int j = 0; j < _ih; j++)
			{
				int num5 = j;
				if (_interlace)
				{
					if (num4 >= _ih)
					{
						num2++;
						switch (num2)
						{
						case 2:
							num4 = 4;
							break;
						case 3:
							num4 = 2;
							num3 = 4;
							break;
						case 4:
							num4 = 1;
							num3 = 2;
							break;
						}
					}
					num5 = num4;
					num4 += num3;
				}
				num5 += _iy;
				if (num5 >= _height)
				{
					continue;
				}
				int num6 = j * _iw;
				int num7 = _height - num5 - 1;
				int k = num7 * _width + _ix;
				for (int num8 = k + _iw; k < num8; k++)
				{
					int num9 = _act[_pixels[num6++] & 0xFF];
					if (num9 != 0)
					{
						_image[k] = num9;
					}
				}
			}
		}

		private void DecodeImageData()
		{
			int num = _iw * _ih;
			if (_pixels == null || _pixels.Length < num)
			{
				_pixels = new byte[num];
			}
			if (_prefix == null)
			{
				_prefix = new short[4096];
			}
			if (_suffix == null)
			{
				_suffix = new byte[4096];
			}
			if (_pixelStack == null)
			{
				_pixelStack = new byte[4097];
			}
			int num2 = Read();
			int num3 = 1 << num2;
			int num4 = num3 + 1;
			int num5 = num3 + 2;
			int num6 = -1;
			int num7 = num2 + 1;
			int num8 = (1 << num7) - 1;
			for (int i = 0; i < num3; i++)
			{
				_prefix[i] = 0;
				_suffix[i] = (byte)i;
			}
			int j;
			int num12;
			int num11;
			int num10;
			int num9;
			int num13 = (j = (num12 = (num11 = (num10 = (num9 = 0)))));
			int k = 0;
			while (k < num)
			{
				if (num10 == 0)
				{
					for (; j < num7; j += 8)
					{
						if (num12 == 0)
						{
							num12 = ReadBlock();
							num9 = 0;
						}
						num13 += (_block[num9++] & 0xFF) << j;
						num12--;
					}
					int i = num13 & num8;
					num13 >>= num7;
					j -= num7;
					if (i > num5 || i == num4)
					{
						break;
					}
					if (i == num3)
					{
						num7 = num2 + 1;
						num8 = (1 << num7) - 1;
						num5 = num3 + 2;
						num6 = -1;
						continue;
					}
					if (num6 == -1)
					{
						_pixelStack[num10++] = _suffix[i];
						num6 = i;
						num11 = i;
						continue;
					}
					int num14 = i;
					if (i == num5)
					{
						_pixelStack[num10++] = (byte)num11;
						i = num6;
					}
					while (i > num3)
					{
						_pixelStack[num10++] = _suffix[i];
						i = _prefix[i];
					}
					num11 = _suffix[i] & 0xFF;
					if (num5 >= 4096)
					{
						break;
					}
					_pixelStack[num10++] = (byte)num11;
					_prefix[num5] = (short)num6;
					_suffix[num5] = (byte)num11;
					num5++;
					if ((num5 & num8) == 0 && num5 < 4096)
					{
						num7++;
						num8 += num5;
					}
					num6 = num14;
				}
				num10--;
				_pixels[k++] = _pixelStack[num10];
			}
			for (; k < num; k++)
			{
				_pixels[k] = 0;
			}
		}

		private void Init()
		{
			_status = Status.StatusOk;
			_frameCount = 0;
			_currentFrame = null;
			AllFramesRead = false;
			_gct = null;
			_lct = null;
		}

		private int Read()
		{
			int result = 0;
			try
			{
				result = _inStream.ReadByte();
			}
			catch (IOException)
			{
				_status = Status.StatusFormatError;
			}
			return result;
		}

		private int ReadBlock()
		{
			_blockSize = Read();
			int i = 0;
			if (_blockSize <= 0)
			{
				return i;
			}
			try
			{
				int num;
				for (; i < _blockSize; i += num)
				{
					num = _inStream.Read(_block, i, _blockSize - i);
					if (num == -1)
					{
						break;
					}
				}
			}
			catch (IOException)
			{
			}
			if (i < _blockSize)
			{
				_status = Status.StatusFormatError;
			}
			return i;
		}

		private int[] ReadColorTable(int ncolors)
		{
			int num = 3 * ncolors;
			int[] array = null;
			byte[] array2 = new byte[num];
			int num2 = 0;
			try
			{
				num2 = _inStream.Read(array2, 0, array2.Length);
			}
			catch (IOException)
			{
			}
			if (num2 < num)
			{
				_status = Status.StatusFormatError;
			}
			else
			{
				array = new int[256];
				int num3 = 0;
				int num4 = 0;
				while (num3 < ncolors)
				{
					uint num5 = array2[num4++];
					uint num6 = (uint)(array2[num4++] & 0xFF);
					uint num7 = (uint)(array2[num4++] & 0xFF);
					array[num3++] = (int)(0xFF000000u | (num7 << 16) | (num6 << 8) | num5);
				}
			}
			return array;
		}

		private void ReadGraphicControlExt()
		{
			Read();
			int num = Read();
			_dispose = (num & 0x1C) >> 2;
			if (_dispose == 0)
			{
				_dispose = 1;
			}
			_transparency = (num & 1) != 0;
			_delay = (float)ReadShort() / 100f;
			_transIndex = Read();
			Read();
		}

		private void ReadHeader()
		{
			string text = string.Empty;
			for (int i = 0; i < 6; i++)
			{
				text += (char)Read();
			}
			if (!text.StartsWith("GIF"))
			{
				_status = Status.StatusFormatError;
				return;
			}
			ReadLsd();
			if (_gctFlag && !Error())
			{
				_gct = ReadColorTable(_gctSize);
				_bgColor = _gct[_bgIndex];
			}
			_imageDataOffset = _inStream.Position;
		}

		private void ReadImage()
		{
			_ix = ReadShort();
			_iy = ReadShort();
			_iw = ReadShort();
			_ih = ReadShort();
			int num = Read();
			_lctFlag = (num & 0x80) != 0;
			_interlace = (num & 0x40) != 0;
			_lctSize = 2 << (num & 7);
			if (_lctFlag)
			{
				_lct = ReadColorTable(_lctSize);
				_act = _lct;
			}
			else
			{
				_act = _gct;
				if (_bgIndex == _transIndex)
				{
					_bgColor = 0;
				}
			}
			int num2 = 0;
			if (_transparency)
			{
				num2 = _act[_transIndex];
				_act[_transIndex] = 0;
			}
			if (_act == null)
			{
				_status = Status.StatusFormatError;
			}
			if (Error())
			{
				return;
			}
			DecodeImageData();
			Skip();
			if (!Error())
			{
				if (_image == null)
				{
					_image = new int[_width * _height];
				}
				if (_bimage == null)
				{
					_bimage = new byte[_width * _height * 4];
				}
				SetPixels();
				Buffer.BlockCopy(_image, 0, _bimage, 0, _bimage.Length);
				_currentFrame = new GifFrame(_bimage, _delay);
				_frameCount++;
				if (_transparency)
				{
					_act[_transIndex] = num2;
				}
				ResetFrame();
			}
		}

		private void ReadLsd()
		{
			_width = ReadShort();
			_height = ReadShort();
			int num = Read();
			_gctFlag = (num & 0x80) != 0;
			_gctSize = 2 << (num & 7);
			_bgIndex = Read();
			Read();
		}

		private void ReadNetscapeExt()
		{
			do
			{
				ReadBlock();
				if (_block[0] == 1)
				{
					int num = _block[1] & 0xFF;
					int num2 = _block[2] & 0xFF;
					_loopCount = (num2 << 8) | num;
				}
			}
			while (_blockSize > 0 && !Error());
		}

		private int ReadShort()
		{
			return Read() | (Read() << 8);
		}

		private void ResetFrame()
		{
			_lastDispose = _dispose;
			_lastBgColor = _bgColor;
			_lct = null;
		}

		private void Skip()
		{
			do
			{
				ReadBlock();
			}
			while (_blockSize > 0 && !Error());
		}
	}
}
