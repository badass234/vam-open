using UnityEngine;

public class DAZMeshData : ScriptableObject
{
	public Vector3[] baseVertices;

	public MeshPoly[] basePolyList;

	public DAZVertexMap[] baseVerticesToUVVertices;

	public Vector3[] UVVertices;

	public MeshPoly[] UVPolyList;

	public Vector2[] OrigUV;
}
