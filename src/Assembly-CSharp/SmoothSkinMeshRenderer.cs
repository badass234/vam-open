using MVR;
using UnityEngine;

[ExecuteInEditMode]
public class SmoothSkinMeshRenderer : ObjectAllocator
{
	public SkinnedMeshRenderer skinnedMeshRenderer;

	private Mesh mesh;

	private MeshSmooth meshSmooth;

	private MeshRenderer mr;

	public Vector3[] inputVerts;

	public Vector3[] smoothedVerts;

	private bool wasInit;

	private void Update()
	{
		Init();
		if (!(skinnedMeshRenderer != null))
		{
			return;
		}
		skinnedMeshRenderer.BakeMesh(mesh);
		inputVerts = mesh.vertices;
		meshSmooth.LaplacianSmooth(inputVerts, smoothedVerts);
		mesh.vertices = smoothedVerts;
		for (int i = 0; i < mesh.subMeshCount; i++)
		{
			Material material = skinnedMeshRenderer.sharedMaterials[i];
			if (material != null)
			{
				Graphics.DrawMesh(mesh, base.transform.localToWorldMatrix, material, base.gameObject.layer, null, i, null, skinnedMeshRenderer.shadowCastingMode, skinnedMeshRenderer.receiveShadows);
			}
		}
	}

	private void Init()
	{
		if (!wasInit && skinnedMeshRenderer != null)
		{
			wasInit = true;
			mesh = Object.Instantiate(skinnedMeshRenderer.sharedMesh);
			RegisterAllocatedObject(mesh);
			smoothedVerts = new Vector3[mesh.vertices.Length];
			meshSmooth = new MeshSmooth(mesh.vertices, mesh.triangles);
		}
	}

	private void OnEnable()
	{
		wasInit = false;
		Init();
	}

	private void Start()
	{
		wasInit = false;
		Init();
	}
}
