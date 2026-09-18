using System.Collections.Generic;
using System.Security.Cryptography;

namespace Mono.CSharp
{
	public class ParserSession
	{
		private MD5 md5;

		public readonly char[] StreamReaderBuffer = new char[4096];

		public readonly Dictionary<char[], string>[] Identifiers = new Dictionary<char[], string>[513];

		public readonly List<Parameter> ParametersStack = new List<Parameter>(4);

		public readonly char[] IDBuilder = new char[512];

		public readonly char[] NumberBuilder = new char[512];

		public LocationsBag LocationsBag { get; set; }

		public bool UseJayGlobalArrays { get; set; }

		public LocatedToken[] LocatedTokens { get; set; }

		public MD5 GetChecksumAlgorithm()
		{
			return md5 ?? (md5 = MD5.Create());
		}
	}
}
