using System;
using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

namespace DynamicCSharp.Security
{
	[Serializable]
	public sealed class MemberRestriction : Restriction
	{
		[SerializeField]
		private string memberName = string.Empty;

		public string RestrictedMember => memberName;

		public override string Message => $"The member '{memberName}' is prohibited and cannot be referenced";

		public override RestrictionMode Mode => RestrictionMode.Exclusive;

		public MemberRestriction(string restrictedName)
		{
			memberName = restrictedName;
		}

		public override bool Verify(ModuleDefinition module)
		{
			if (string.IsNullOrEmpty(memberName))
			{
				return true;
			}
			IEnumerable<MemberReference> memberReferences = module.GetMemberReferences();
			foreach (MemberReference item in memberReferences)
			{
				string fullName = item.DeclaringType.FullName;
				string name = item.Name;
				string strB = fullName + "::" + name;
				if (string.Compare(memberName, strB) == 0)
				{
					return false;
				}
			}
			return true;
		}
	}
}
