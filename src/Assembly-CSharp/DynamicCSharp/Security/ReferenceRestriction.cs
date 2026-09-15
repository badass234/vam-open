using System;
using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

namespace DynamicCSharp.Security
{
	[Serializable]
	public sealed class ReferenceRestriction : Restriction
	{
		[SerializeField]
		private string referenceName = string.Empty;

		public string RestrictedName => referenceName;

		public override string Message => $"The references assembly '{referenceName}' is prohibited and cannot be referenced";

		public override RestrictionMode Mode => DynamicCSharp.Settings.assemblyRestrictionMode;

		public ReferenceRestriction(string referenceName)
		{
			this.referenceName = referenceName;
		}

		public override bool Verify(ModuleDefinition module)
		{
			if (string.IsNullOrEmpty(referenceName))
			{
				return true;
			}
			IEnumerable<AssemblyNameReference> assemblyReferences = module.AssemblyReferences;
			foreach (AssemblyNameReference item in assemblyReferences)
			{
				if (string.Compare(referenceName, item.Name + ".dll") == 0)
				{
					return false;
				}
			}
			return true;
		}
	}
}
