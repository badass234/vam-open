using System;
using System.Reflection;

namespace Mono.CSharp
{
	public class AssemblyResource : IEquatable<AssemblyResource>
	{
		public ResourceAttributes Attributes { get; private set; }

		public string Name { get; private set; }

		public string FileName { get; private set; }

		public bool IsEmbeded { get; set; }

		public AssemblyResource(string fileName, string name)
			: this(fileName, name, false)
		{
		}

		public AssemblyResource(string fileName, string name, bool isPrivate)
		{
			FileName = fileName;
			Name = name;
			Attributes = ((!isPrivate) ? ResourceAttributes.Public : ResourceAttributes.Private);
		}

		public bool Equals(AssemblyResource other)
		{
			return Name == other.Name;
		}
	}
}
