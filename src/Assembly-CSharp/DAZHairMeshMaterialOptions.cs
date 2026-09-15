using System.Collections.Generic;
using UnityEngine;

public class DAZHairMeshMaterialOptions : MaterialOptions
{
	[HideInInspector]
	[SerializeField]
	protected DAZHairMesh _mesh;

	public DAZHairMesh mesh
	{
		get
		{
			if (_mesh == null)
			{
				DAZHairMesh[] components = GetComponents<DAZHairMesh>();
				DAZHairMesh[] array = components;
				foreach (DAZHairMesh dAZHairMesh in array)
				{
					if (dAZHairMesh.enabled)
					{
						_mesh = dAZHairMesh;
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

	protected override void SetMaterialParam(string name, float value)
	{
		if (_mesh != null)
		{
			Material hairMaterialRuntime = _mesh.hairMaterialRuntime;
			if (hairMaterialRuntime != null && hairMaterialRuntime.HasProperty(name))
			{
				hairMaterialRuntime.SetFloat(name, value);
			}
		}
	}

	protected override void SetMaterialColor(string name, Color c)
	{
		if (_mesh != null)
		{
			Material hairMaterialRuntime = _mesh.hairMaterialRuntime;
			if (hairMaterialRuntime != null && hairMaterialRuntime.HasProperty(name))
			{
				hairMaterialRuntime.SetColor(name, c);
			}
		}
	}

	protected override void SetMaterialTexture(int slot, string propName, Texture texture)
	{
		if (_mesh != null)
		{
			Material hairMaterialRuntime = _mesh.hairMaterialRuntime;
			SetMaterialTexture(hairMaterialRuntime, propName, texture);
		}
	}

	protected override void SetMaterialTextureScale(int slot, string propName, Vector2 scale)
	{
		if (_mesh != null)
		{
			Material hairMaterialRuntime = _mesh.hairMaterialRuntime;
			SetMaterialTextureScale(hairMaterialRuntime, propName, scale);
		}
	}

	protected override void SetMaterialTextureOffset(int slot, string propName, Vector2 offset)
	{
		if (_mesh != null)
		{
			Material hairMaterialRuntime = _mesh.hairMaterialRuntime;
			SetMaterialTextureOffset(hairMaterialRuntime, propName, offset);
		}
	}

	public override void SetStartingValues(Dictionary<Texture2D, string> textureToSourcePath)
	{
		if (_mesh != null)
		{
			_mesh.MaterialInit();
			if (materialForDefaults == null)
			{
				materialForDefaults = _mesh.hairMaterial;
			}
		}
		base.SetStartingValues(textureToSourcePath);
	}
}
