using UnityEngine;

internal class OVRMRForegroundCameraManager : MonoBehaviour
{
	public GameObject clipPlaneGameObj;

	private Material clipPlaneMaterial;

	private void OnPreRender()
	{
		if ((bool)clipPlaneGameObj)
		{
			if (clipPlaneMaterial == null)
			{
				clipPlaneMaterial = clipPlaneGameObj.GetComponent<MeshRenderer>().material;
			}
			clipPlaneGameObj.GetComponent<MeshRenderer>().material.SetFloat("_Visible", 1f);
		}
	}

	private void OnPostRender()
	{
		if ((bool)clipPlaneGameObj)
		{
			clipPlaneGameObj.GetComponent<MeshRenderer>().material.SetFloat("_Visible", 0f);
		}
	}
}
