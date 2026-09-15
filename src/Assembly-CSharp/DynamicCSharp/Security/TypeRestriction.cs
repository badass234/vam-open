using System;
using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

namespace DynamicCSharp.Security
{
	[Serializable]
	public sealed class TypeRestriction : Restriction
	{
		[SerializeField]
		private string typeName = string.Empty;

		public string RestrictedType => typeName;

		public override string Message => $"The type '{typeName}' is prohibited and cannot be referenced";

		public override RestrictionMode Mode => RestrictionMode.Exclusive;

		public TypeRestriction(string restrictedName)
		{
			typeName = restrictedName;
		}

		public override bool Verify(ModuleDefinition module)
		{
			if (string.IsNullOrEmpty(typeName))
			{
				return true;
			}
			IEnumerable<TypeReference> typeReferences = module.GetTypeReferences();
			foreach (TypeReference item in typeReferences)
			{
				string fullName = item.FullName;
				if (string.Compare(typeName, fullName) == 0)
				{
					return false;
				}
			}
			return true;
		}
	}
}
