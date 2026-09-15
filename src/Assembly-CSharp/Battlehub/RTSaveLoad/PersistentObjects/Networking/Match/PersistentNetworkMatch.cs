using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Networking.Match;

namespace Battlehub.RTSaveLoad.PersistentObjects.Networking.Match
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentNetworkMatch : PersistentMonoBehaviour
	{
		public Uri baseUri;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			NetworkMatch networkMatch = (NetworkMatch)obj;
			networkMatch.baseUri = baseUri;
			return networkMatch;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				NetworkMatch networkMatch = (NetworkMatch)obj;
				baseUri = networkMatch.baseUri;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
