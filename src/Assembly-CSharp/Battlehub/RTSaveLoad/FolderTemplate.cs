using System.Collections.Generic;
using System.Linq;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	[ExecuteInEditMode]
	public class FolderTemplate : MonoBehaviour
	{
		[EnumFlags]
		public AssetTypeHint TypeHint = AssetTypeHint.All;

		public Object[] Objects;

		private string m_name;

		private void Awake()
		{
			if (Application.isPlaying)
			{
				base.enabled = false;
			}
		}

		private void OnTransformParentChanged()
		{
			FixName();
		}

		private void Update()
		{
			if (m_name != base.name)
			{
				FixName();
				m_name = base.name;
			}
		}

		private void FixName()
		{
			FolderTemplate[] siblings = GetSiblings(this);
			base.name = PathHelper.RemoveInvalidFineNameCharacters(base.name);
			base.name = PathHelper.GetUniqueName(base.name, siblings.Select((FolderTemplate s) => s.name).ToArray());
			m_name = base.name;
		}

		private static FolderTemplate[] GetSiblings(FolderTemplate template)
		{
			if (template.transform.parent == null)
			{
				return new FolderTemplate[0];
			}
			FolderTemplate component = template.transform.parent.GetComponent<FolderTemplate>();
			if (component == null)
			{
				return new FolderTemplate[0];
			}
			List<FolderTemplate> list = new List<FolderTemplate>();
			foreach (Transform item in component.transform)
			{
				FolderTemplate component2 = item.GetComponent<FolderTemplate>();
				if (component2 != null && component2 != template)
				{
					list.Add(component2);
				}
			}
			return list.ToArray();
		}
	}
}
