using System;
using UnityEngine;

[Serializable]
public class LightMapDataContainerObject : ScriptableObject
{
	public int[] lightmapIndexes;

	public Vector4[] lightmapOffsetScales;
}
