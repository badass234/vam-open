using UnityEngine;

namespace Leap.Unity.Space
{
	public interface IRadialTransformer : ITransformer
	{
		Vector4 GetVectorRepresentation(Transform element);
	}
}
