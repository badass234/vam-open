using UnityEngine;

[ExecuteInEditMode]
public class ProjectorMatrix : MonoBehaviour
{
	public enum matrixName
	{
		_projectiveMatrWaves,
		_projectiveMatrCausticScale
	}

	public matrixName GlobalMatrixName;

	public Transform ProjectiveTranform;

	public bool UpdateOnStart;

	public bool CanUpdate = true;

	private Transform t;

	private void Start()
	{
		t = base.transform;
		if (UpdateOnStart)
		{
			UpdateMatrix();
		}
	}

	private void Update()
	{
		if (!UpdateOnStart)
		{
			UpdateMatrix();
		}
	}

	public void UpdateMatrix()
	{
		if (CanUpdate && ProjectiveTranform != null)
		{
			Shader.SetGlobalMatrix(GlobalMatrixName.ToString(), ProjectiveTranform.worldToLocalMatrix * t.localToWorldMatrix);
		}
	}
}
