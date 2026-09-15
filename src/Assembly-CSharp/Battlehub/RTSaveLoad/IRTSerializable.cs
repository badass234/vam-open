using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	public interface IRTSerializable
	{
		void Serialize();

		void Deserialize(Dictionary<long, Object> dependencies);

		void GetDependencies(Dictionary<long, Object> dependencies);

		void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls);
	}
}
