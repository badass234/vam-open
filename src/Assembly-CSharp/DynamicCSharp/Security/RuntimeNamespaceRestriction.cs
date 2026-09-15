using System;
using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

namespace DynamicCSharp.Security
{
	[Serializable]
	public sealed class RuntimeNamespaceRestriction : Restriction
	{
		[SerializeField]
		private string namespaceName = string.Empty;

		[SerializeField]
		private string[] typeExceptions;

		private HashSet<string> typeExceptionsHash;

		public string RestrictedNamespace => namespaceName;

		public override string Message => $"The namespace '{namespaceName}' is prohibited and cannot be referenced";

		public override RestrictionMode Mode => DynamicCSharp.Settings.namespaceRestrictionMode;

		public RuntimeNamespaceRestriction(string restrictedName)
		{
			namespaceName = restrictedName;
		}

		private bool IsTypeException(string type)
		{
			if (typeExceptions != null)
			{
				if (typeExceptionsHash == null)
				{
					typeExceptionsHash = new HashSet<string>();
					string[] array = typeExceptions;
					foreach (string item in array)
					{
						typeExceptionsHash.Add(item);
					}
				}
				return typeExceptionsHash.Contains(type);
			}
			return false;
		}

		public override bool Verify(ModuleDefinition module)
		{
			if (string.IsNullOrEmpty(namespaceName))
			{
				return true;
			}
			IEnumerable<TypeReference> typeReferences = module.GetTypeReferences();
			foreach (TypeReference item in typeReferences)
			{
				if (!IsTypeException(item.FullName))
				{
					string @namespace = item.Namespace;
					if (string.Compare(namespaceName, @namespace) == 0)
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
