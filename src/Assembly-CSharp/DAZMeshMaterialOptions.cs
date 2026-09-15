using System.Collections.Generic;
using UnityEngine;

public class DAZMeshMaterialOptions : MaterialOptions
{
	[HideInInspector]
	public bool useAllMeshes;

	[HideInInspector]
	[SerializeField]
	protected DAZMesh _mesh;

	[HideInInspector]
	public bool useSimpleMaterial;

	public DAZMesh mesh
	{
		get
		{
			if (_mesh == null)
			{
				DAZMesh[] components = GetComponents<DAZMesh>();
				DAZMesh[] array = components;
				foreach (DAZMesh dAZMesh in array)
				{
					if (dAZMesh.enabled && dAZMesh.drawMorphedUVMappedMesh)
					{
						_mesh = dAZMesh;
					}
				}
			}
			return _mesh;
		}
		set
		{
			if (_mesh != value)
			{
				_mesh = value;
				SetAllParameters();
			}
		}
	}

	protected void SetMaterialHideForMesh(DAZMesh dmesh, bool b)
	{
		if (!(dmesh != null))
		{
			return;
		}
		dmesh.InitMaterials();
		if (useSimpleMaterial)
		{
			Material simpleMaterial = dmesh.simpleMaterial;
			SetMaterialHide(simpleMaterial, b);
		}
		else
		{
			if (paramMaterialSlots == null)
			{
				return;
			}
			for (int i = 0; i < paramMaterialSlots.Length; i++)
			{
				int num = paramMaterialSlots[i];
				if (num < dmesh.numMaterials)
				{
					Material m = dmesh.materials[num];
					SetMaterialHide(m, b);
				}
			}
		}
	}

	protected override void SetMaterialHide(bool b)
	{
		if (useAllMeshes)
		{
			DAZMesh[] components = GetComponents<DAZMesh>();
			DAZMesh[] array = components;
			foreach (DAZMesh dAZMesh in array)
			{
				if (dAZMesh.enabled && dAZMesh.drawMorphedUVMappedMesh)
				{
					SetMaterialHideForMesh(dAZMesh, b);
				}
			}
		}
		else
		{
			SetMaterialHideForMesh(_mesh, b);
		}
	}

	protected void SetMaterialRenderQueueForMesh(DAZMesh dmesh, int q)
	{
		if (!(dmesh != null))
		{
			return;
		}
		dmesh.InitMaterials();
		if (useSimpleMaterial)
		{
			Material simpleMaterial = dmesh.simpleMaterial;
			simpleMaterial.renderQueue = q;
		}
		else
		{
			if (paramMaterialSlots == null)
			{
				return;
			}
			for (int i = 0; i < paramMaterialSlots.Length; i++)
			{
				int num = paramMaterialSlots[i];
				if (num < dmesh.numMaterials)
				{
					Material material = dmesh.materials[num];
					material.renderQueue = q;
				}
			}
		}
	}

	protected override void SetMaterialRenderQueue(int q)
	{
		if (useAllMeshes)
		{
			DAZMesh[] components = GetComponents<DAZMesh>();
			DAZMesh[] array = components;
			foreach (DAZMesh dAZMesh in array)
			{
				if (dAZMesh.enabled && dAZMesh.drawMorphedUVMappedMesh)
				{
					SetMaterialRenderQueueForMesh(dAZMesh, q);
				}
			}
		}
		else
		{
			SetMaterialRenderQueueForMesh(_mesh, q);
		}
	}

	protected void SetMaterialParamForMesh(DAZMesh dmesh, string name, float value)
	{
		if (!(dmesh != null))
		{
			return;
		}
		dmesh.InitMaterials();
		if (useSimpleMaterial)
		{
			Material simpleMaterial = dmesh.simpleMaterial;
			if (simpleMaterial.HasProperty(name))
			{
				simpleMaterial.SetFloat(name, value);
			}
		}
		else
		{
			if (paramMaterialSlots == null)
			{
				return;
			}
			for (int i = 0; i < paramMaterialSlots.Length; i++)
			{
				int num = paramMaterialSlots[i];
				if (num < dmesh.numMaterials)
				{
					Material material = dmesh.materials[num];
					if (material != null && material.HasProperty(name))
					{
						material.SetFloat(name, value);
					}
				}
			}
		}
	}

	protected override void SetMaterialParam(string name, float value)
	{
		if (useAllMeshes)
		{
			DAZMesh[] components = GetComponents<DAZMesh>();
			DAZMesh[] array = components;
			foreach (DAZMesh dAZMesh in array)
			{
				if (dAZMesh.enabled && dAZMesh.drawMorphedUVMappedMesh)
				{
					SetMaterialParamForMesh(dAZMesh, name, value);
				}
			}
		}
		else
		{
			SetMaterialParamForMesh(_mesh, name, value);
		}
	}

	protected void SetMaterialColorForMesh(DAZMesh dmesh, string name, Color c)
	{
		if (!(dmesh != null))
		{
			return;
		}
		dmesh.InitMaterials();
		if (useSimpleMaterial)
		{
			Material simpleMaterial = dmesh.simpleMaterial;
			if (simpleMaterial.HasProperty(name))
			{
				simpleMaterial.SetColor(name, c);
			}
		}
		else
		{
			if (paramMaterialSlots == null)
			{
				return;
			}
			for (int i = 0; i < paramMaterialSlots.Length; i++)
			{
				int num = paramMaterialSlots[i];
				if (num < dmesh.numMaterials)
				{
					Material material = dmesh.materials[num];
					if (material != null && material.HasProperty(name))
					{
						material.SetColor(name, c);
					}
				}
			}
		}
	}

	protected override void SetMaterialColor(string name, Color c)
	{
		if (useAllMeshes)
		{
			DAZMesh[] components = GetComponents<DAZMesh>();
			DAZMesh[] array = components;
			foreach (DAZMesh dAZMesh in array)
			{
				if (dAZMesh.enabled && dAZMesh.drawMorphedUVMappedMesh)
				{
					SetMaterialColorForMesh(dAZMesh, name, c);
				}
			}
		}
		else
		{
			SetMaterialColorForMesh(_mesh, name, c);
		}
	}

	protected override void SetMaterialTexture(int slot, string propName, Texture texture)
	{
		if (useAllMeshes)
		{
			DAZMesh[] components = GetComponents<DAZMesh>();
			DAZMesh[] array = components;
			foreach (DAZMesh dAZMesh in array)
			{
				if (dAZMesh != null)
				{
					dAZMesh.InitMaterials();
					if (slot < dAZMesh.numMaterials)
					{
						Material m = dAZMesh.materials[slot];
						SetMaterialTexture(m, propName, texture);
					}
				}
			}
		}
		else if (mesh != null)
		{
			mesh.InitMaterials();
			if (slot < mesh.numMaterials)
			{
				Material m2 = mesh.materials[slot];
				SetMaterialTexture(m2, propName, texture);
			}
		}
	}

	protected override void SetMaterialTexture2(int slot, string propName, Texture texture)
	{
		Debug.LogError("SetMaterialTexture2 for DAZMeshMaterialOptions should not be used");
	}

	protected override void SetMaterialTextureScale(int slot, string propName, Vector2 scale)
	{
		if (useAllMeshes)
		{
			DAZMesh[] components = GetComponents<DAZMesh>();
			DAZMesh[] array = components;
			foreach (DAZMesh dAZMesh in array)
			{
				if (dAZMesh != null)
				{
					dAZMesh.InitMaterials();
					if (slot < dAZMesh.numMaterials)
					{
						Material m = dAZMesh.materials[slot];
						SetMaterialTextureScale(m, propName, scale);
					}
				}
			}
		}
		else if (mesh != null)
		{
			mesh.InitMaterials();
			if (slot < mesh.numMaterials)
			{
				Material m2 = mesh.materials[slot];
				SetMaterialTextureScale(m2, propName, scale);
			}
		}
	}

	protected override void SetMaterialTextureScale2(int slot, string propName, Vector2 scale)
	{
		Debug.LogError("SetMaterialTextureScale2 for DAZMeshMaterialOptions should not be used");
	}

	protected override void SetMaterialTextureOffset(int slot, string propName, Vector2 offset)
	{
		if (useAllMeshes)
		{
			DAZMesh[] components = GetComponents<DAZMesh>();
			DAZMesh[] array = components;
			foreach (DAZMesh dAZMesh in array)
			{
				if (dAZMesh != null)
				{
					dAZMesh.InitMaterials();
					if (slot < dAZMesh.numMaterials)
					{
						Material m = dAZMesh.materials[slot];
						SetMaterialTextureOffset(m, propName, offset);
					}
				}
			}
		}
		else if (mesh != null)
		{
			mesh.InitMaterials();
			if (slot < mesh.numMaterials)
			{
				Material m2 = mesh.materials[slot];
				SetMaterialTextureOffset(m2, propName, offset);
			}
		}
	}

	protected override void SetMaterialTextureOffset2(int slot, string propName, Vector2 offset)
	{
		Debug.LogError("SetMaterialTextureOffset2 for DAZMeshMaterialOptions should not be used");
	}

	public override Mesh GetMesh()
	{
		Mesh result = null;
		if (_mesh != null)
		{
			result = _mesh.morphedUVMappedMesh;
		}
		return result;
	}

	public override void SetStartingValues(Dictionary<Texture2D, string> textureToSourcePath)
	{
		if (materialForDefaults == null)
		{
			if (useAllMeshes)
			{
				DAZMesh[] components = GetComponents<DAZMesh>();
				if (components.Length > 0)
				{
					_mesh = components[0];
				}
				else
				{
					_mesh = null;
				}
			}
			if (mesh != null)
			{
				mesh.InitMaterials();
				if (paramMaterialSlots != null && paramMaterialSlots.Length > 0)
				{
					int num = paramMaterialSlots[0];
					if (num < mesh.numMaterials)
					{
						materialForDefaults = mesh.materials[num];
					}
				}
			}
		}
		base.SetStartingValues(textureToSourcePath);
	}
}
