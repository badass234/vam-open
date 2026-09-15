using MVR;
using UnityEngine;

public class DelayedSkinMeshRenderer : ObjectAllocator
{
	public SkinnedMeshRenderer skinnedMeshRenderer;

	private Mesh mesh1;

	private Mesh mesh2;

	private Mesh mesh3;

	private Mesh skinMesh;

	private Mesh drawMesh;

	protected Matrix4x4 lastMatrix;

	public int delayCount = 1;

	private bool wasInit;

	private void LateUpdate()
	{
		Init();
		if (skinnedMeshRenderer != null)
		{
			if (skinMesh == mesh1)
			{
				skinMesh = mesh2;
				if (delayCount == 1)
				{
					drawMesh = mesh1;
				}
				else
				{
					drawMesh = mesh3;
				}
			}
			else if (skinMesh == mesh2)
			{
				skinMesh = mesh3;
				if (delayCount == 1)
				{
					drawMesh = mesh2;
				}
				else
				{
					drawMesh = mesh1;
				}
			}
			else
			{
				skinMesh = mesh1;
				if (delayCount == 1)
				{
					drawMesh = mesh3;
				}
				else
				{
					drawMesh = mesh2;
				}
			}
			skinnedMeshRenderer.BakeMesh(skinMesh);
			for (int i = 0; i < skinnedMeshRenderer.materials.Length; i++)
			{
				Material material = skinnedMeshRenderer.materials[i];
				if (material != null)
				{
					Graphics.DrawMesh(drawMesh, lastMatrix, material, base.gameObject.layer, null, i, null, skinnedMeshRenderer.shadowCastingMode, skinnedMeshRenderer.receiveShadows);
				}
			}
		}
		lastMatrix = base.transform.localToWorldMatrix;
	}

	private void Init()
	{
		if (!wasInit && skinnedMeshRenderer != null)
		{
			wasInit = true;
			lastMatrix = base.transform.localToWorldMatrix;
			skinnedMeshRenderer.enabled = false;
			mesh1 = Object.Instantiate(skinnedMeshRenderer.sharedMesh);
			RegisterAllocatedObject(mesh1);
			mesh2 = Object.Instantiate(skinnedMeshRenderer.sharedMesh);
			RegisterAllocatedObject(mesh2);
			mesh3 = Object.Instantiate(skinnedMeshRenderer.sharedMesh);
			RegisterAllocatedObject(mesh3);
		}
	}
}
