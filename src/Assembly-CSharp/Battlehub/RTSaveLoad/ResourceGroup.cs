using Battlehub.Utils;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	public class ResourceGroup : MonoBehaviour
	{
		[ReadOnly]
		public string Guid;

		public ObjectToID[] Mapping;
	}
}
