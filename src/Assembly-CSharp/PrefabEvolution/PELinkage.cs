using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrefabEvolution
{
	[Serializable]
	public class PELinkage
	{
		[Serializable]
		public class Link
		{
			public int LIIF;

			public UnityEngine.Object InstanceTarget;

			public override string ToString()
			{
				return $"[Link]{LIIF}:{InstanceTarget}";
			}
		}

		public List<Link> Links = Utils.Create<List<Link>>();

		public Link this[int liif]
		{
			get
			{
				for (int i = 0; i < Links.Count; i++)
				{
					Link link = Links[i];
					if (link.LIIF == liif)
					{
						return link;
					}
				}
				return null;
			}
		}

		public Link this[Link link]
		{
			get
			{
				if (link == null)
				{
					return null;
				}
				return this[link.LIIF];
			}
		}

		public Link this[UnityEngine.Object obj]
		{
			get
			{
				for (int i = 0; i < Links.Count; i++)
				{
					Link link = Links[i];
					if (link.InstanceTarget == obj)
					{
						return link;
					}
				}
				return null;
			}
		}

		public UnityEngine.Object GetPrefabObject(GameObject prefab, UnityEngine.Object instanceObject)
		{
			PEPrefabScript component = prefab.GetComponent<PEPrefabScript>();
			Link link = this[instanceObject];
			if (link == null)
			{
				return null;
			}
			return component.Links[link.LIIF]?.InstanceTarget;
		}
	}
}
