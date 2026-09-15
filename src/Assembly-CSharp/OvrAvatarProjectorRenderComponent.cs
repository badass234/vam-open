using System.Collections.Generic;
using UnityEngine;

public class OvrAvatarProjectorRenderComponent : OvrAvatarRenderComponent
{
	private Material material;

	internal void InitializeProjectorRender(ovrAvatarRenderPart_ProjectorRender render, Shader shader, OvrAvatarRenderComponent target)
	{
		if (shader == null)
		{
			shader = Shader.Find("OvrAvatar/AvatarSurfaceShader");
		}
		material = CreateAvatarMaterial(base.gameObject.name + "_projector", shader);
		material.EnableKeyword("PROJECTOR_ON");
		Renderer component = target.GetComponent<Renderer>();
		if (component != null)
		{
			List<Material> list = new List<Material>(component.sharedMaterials);
			list.Add(material);
			component.sharedMaterials = list.ToArray();
		}
	}

	internal void UpdateProjectorRender(OvrAvatarComponent component, ovrAvatarRenderPart_ProjectorRender render)
	{
		OvrAvatar.ConvertTransform(render.localTransform, base.transform);
		material.SetMatrix("_ProjectorWorldToLocal", base.transform.worldToLocalMatrix);
		component.UpdateAvatarMaterial(material, render.materialState);
	}

	private void OnDrawGizmos()
	{
		Vector3 from = base.transform.localToWorldMatrix.MultiplyPoint(new Vector3(-1f, -1f, -1f));
		Vector3 vector = base.transform.localToWorldMatrix.MultiplyPoint(new Vector3(1f, -1f, -1f));
		Vector3 vector2 = base.transform.localToWorldMatrix.MultiplyPoint(new Vector3(-1f, 1f, -1f));
		Vector3 vector3 = base.transform.localToWorldMatrix.MultiplyPoint(new Vector3(1f, 1f, -1f));
		Vector3 vector4 = base.transform.localToWorldMatrix.MultiplyPoint(new Vector3(-1f, -1f, 1f));
		Vector3 vector5 = base.transform.localToWorldMatrix.MultiplyPoint(new Vector3(1f, -1f, 1f));
		Vector3 vector6 = base.transform.localToWorldMatrix.MultiplyPoint(new Vector3(-1f, 1f, 1f));
		Vector3 to = base.transform.localToWorldMatrix.MultiplyPoint(new Vector3(1f, 1f, 1f));
		Gizmos.color = Color.gray;
		Gizmos.DrawLine(from, vector);
		Gizmos.DrawLine(from, vector2);
		Gizmos.DrawLine(vector2, vector3);
		Gizmos.DrawLine(vector, vector3);
		Gizmos.DrawLine(from, vector4);
		Gizmos.DrawLine(vector, vector5);
		Gizmos.DrawLine(vector2, vector6);
		Gizmos.DrawLine(vector3, to);
		Gizmos.DrawLine(vector4, vector5);
		Gizmos.DrawLine(vector4, vector6);
		Gizmos.DrawLine(vector6, to);
		Gizmos.DrawLine(vector5, to);
	}
}
