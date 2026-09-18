using System;
using System.IO;
using System.Text;

namespace Mono.CSharp
{
	public class SeekableStreamReader : IDisposable
	{
		public const int DefaultReadAheadSize = 2048;

		private StreamReader reader;

		private Stream stream;

		private char[] buffer;

		private int read_ahead_length;

		private int buffer_start;

		private int char_count;

		private int pos;

		public int Position
		{
			get
			{
				return buffer_start + pos;
			}
			set
			{
				if (value < buffer_start)
				{
					InitializeStream(read_ahead_length);
					reader = new StreamReader(stream, reader.CurrentEncoding, true);
				}
				while (value > buffer_start + char_count)
				{
					pos = char_count;
					if (!ReadBuffer())
					{
						throw new InternalErrorException("Seek beyond end of file: " + (buffer_start + char_count - value));
					}
				}
				pos = value - buffer_start;
			}
		}

		public SeekableStreamReader(Stream stream, Encoding encoding, char[] sharedBuffer = null)
		{
			this.stream = stream;
			buffer = sharedBuffer;
			InitializeStream(2048);
			reader = new StreamReader(stream, encoding, true);
		}

		public void Dispose()
		{
			reader.Dispose();
		}

		private void InitializeStream(int read_length_inc)
		{
			read_ahead_length += read_length_inc;
			int num = read_ahead_length * 2;
			if (buffer == null || buffer.Length < num)
			{
				buffer = new char[num];
			}
			stream.Position = 0L;
			buffer_start = (char_count = (pos = 0));
		}

		private bool ReadBuffer()
		{
			int num = buffer.Length - char_count;
			if (num <= read_ahead_length)
			{
				int num2 = read_ahead_length - num;
				Array.Copy(buffer, num2, buffer, 0, char_count - num2);
				pos -= num2;
				char_count -= num2;
				buffer_start += num2;
				num += num2;
			}
			char_count += reader.Read(buffer, char_count, num);
			return pos < char_count;
		}

		public char[] ReadChars(int fromPosition, int toPosition)
		{
			char[] array = new char[toPosition - fromPosition];
			if (buffer_start <= fromPosition && toPosition <= buffer_start + buffer.Length)
			{
				Array.Copy(buffer, fromPosition - buffer_start, array, 0, array.Length);
				return array;
			}
			throw new NotImplementedException();
		}

		public int Peek()
		{
			if (pos >= char_count && !ReadBuffer())
			{
				return -1;
			}
			return buffer[pos];
		}

		public int Read()
		{
			if (pos >= char_count && !ReadBuffer())
			{
				return -1;
			}
			return buffer[pos++];
		}
	}
}
