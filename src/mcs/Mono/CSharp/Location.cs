using System;
using System.Collections.Generic;

namespace Mono.CSharp
{
	public struct Location : IEquatable<Location>
	{
		private struct Checkpoint
		{
			public readonly int LineOffset;

			public readonly int File;

			public Checkpoint(int file, int line)
			{
				File = file;
				LineOffset = line - line % 256;
			}
		}

		private readonly int token;

		private const int column_bits = 8;

		private const int line_delta_bits = 8;

		private const int checkpoint_bits = 16;

		private const int column_mask = 255;

		private const int max_column = 255;

		private static List<SourceFile> source_list;

		private static Checkpoint[] checkpoints;

		private static int checkpoint_index;

		public static readonly Location Null;

		public static bool InEmacs;

		public bool IsNull => token == 0;

		public string Name
		{
			get
			{
				int file = File;
				if (token == 0 || file <= 0)
				{
					return null;
				}
				return source_list[file - 1].Name;
			}
		}

		public string NameFullPath
		{
			get
			{
				int file = File;
				if (token == 0 || file <= 0)
				{
					return null;
				}
				return source_list[file - 1].FullPathName;
			}
		}

		private int CheckpointIndex => (token >> 16) & 0xFFFF;

		public int Row
		{
			get
			{
				if (token == 0)
				{
					return 1;
				}
				return checkpoints[CheckpointIndex].LineOffset + ((token >> 8) & 0xFF);
			}
		}

		public int Column
		{
			get
			{
				if (token == 0)
				{
					return 1;
				}
				return token & 0xFF;
			}
		}

		public int File
		{
			get
			{
				if (token == 0)
				{
					return 0;
				}
				if (checkpoints.Length <= CheckpointIndex)
				{
					throw new Exception($"Should not happen. Token is {token:X04}, checkpoints are {checkpoints.Length}, index is {CheckpointIndex}");
				}
				return checkpoints[CheckpointIndex].File;
			}
		}

		public SourceFile SourceFile
		{
			get
			{
				int file = File;
				if (file == 0)
				{
					return null;
				}
				return source_list[file - 1];
			}
		}

		static Location()
		{
			Reset();
		}

		public static void Reset()
		{
			source_list = new List<SourceFile>();
			checkpoint_index = 0;
		}

		public static void AddFile(SourceFile file)
		{
			source_list.Add(file);
		}

		public static void Initialize(List<SourceFile> files)
		{
			source_list.AddRange(files.ToArray());
			checkpoints = new Checkpoint[System.Math.Max(1, source_list.Count * 2)];
			if (checkpoints.Length != 0)
			{
				checkpoints[0] = new Checkpoint(0, 0);
			}
		}

		public Location(SourceFile file, int row, int column)
		{
			if (row <= 0)
			{
				token = 0;
				return;
			}
			if (column > 255)
			{
				column = 255;
			}
			long num = -1L;
			long num2 = 0L;
			int num3 = file?.Index ?? 0;
			int num4 = ((checkpoint_index < 10) ? checkpoint_index : 10);
			for (int i = 0; i < num4; i++)
			{
				int lineOffset = checkpoints[checkpoint_index - i].LineOffset;
				num2 = row - lineOffset;
				if (num2 >= 0 && num2 < 256 && checkpoints[checkpoint_index - i].File == num3)
				{
					num = checkpoint_index - i;
					break;
				}
			}
			if (num == -1)
			{
				AddCheckpoint(num3, row);
				num = checkpoint_index;
				num2 = row % 256;
			}
			long num5 = column + (num2 << 8) + (num << 16);
			token = (int)((num5 <= uint.MaxValue) ? num5 : 0);
		}

		public static Location operator -(Location loc, int columns)
		{
			return new Location(loc.SourceFile, loc.Row, loc.Column - columns);
		}

		private static void AddCheckpoint(int file, int row)
		{
			if (checkpoints.Length == ++checkpoint_index)
			{
				Array.Resize(ref checkpoints, checkpoint_index * 2);
			}
			checkpoints[checkpoint_index] = new Checkpoint(file, row);
		}

		private string FormatLocation(string fileName)
		{
			if (InEmacs)
			{
				return fileName + "(" + Row + "):";
			}
			return fileName + "(" + Row + "," + Column + ((Column == 255) ? "+):" : "):");
		}

		public override string ToString()
		{
			return FormatLocation(Name);
		}

		public string ToStringFullName()
		{
			return FormatLocation(NameFullPath);
		}

		public bool Equals(Location other)
		{
			return token == other.token;
		}
	}
}
