using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	public interface IRuntimeShaderUtil
	{
		RuntimeShaderInfo GetShaderInfo(Shader shader);
	}
}
