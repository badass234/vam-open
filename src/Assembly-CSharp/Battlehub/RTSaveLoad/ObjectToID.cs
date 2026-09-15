using System;
using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	[Serializable]
	public struct ObjectToID
	{
		[HideInInspector]
		public string Name;

		public UnityEngine.Object Object;

		[ReadOnly]
		public int Id;

		public ObjectToID(UnityEngine.Object obj, int id)
		{
			Name = obj.name;
			Object = obj;
			Id = id;
		}
	}
}
