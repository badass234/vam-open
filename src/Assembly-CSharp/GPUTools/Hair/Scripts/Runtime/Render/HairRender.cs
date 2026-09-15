using GPUTools.Hair.Scripts.Runtime.Data;
using GPUTools.Hair.Scripts.Utils;
using UnityEngine;
using UnityEngine.Rendering;

namespace GPUTools.Hair.Scripts.Runtime.Render
{
	public class HairRender : MonoBehaviour
	{
		private Mesh mesh;

		private HairDataFacade data;

		private MeshRenderer rend;

		public bool IsVisible => rend.isVisible;

		private void Awake()
		{
			mesh = new Mesh();
			rend = base.gameObject.AddComponent<MeshRenderer>();
			base.gameObject.AddComponent<MeshFilter>().mesh = mesh;
		}

		public void Initialize(HairDataFacade data)
		{
			this.data = data;
			InitializeMaterial();
			InitializeMesh();
		}

		public void InitializeMesh()
		{
			mesh.triangles = null;
			mesh.vertices = new Vector3[(int)data.Size.x];
			mesh.SetIndices(data.Indices, MeshTopology.Triangles, 0);
		}

		private void InitializeMaterial()
		{
			if (data.material != null)
			{
				rend.material = data.material;
			}
			else
			{
				rend.material = Resources.Load<Material>("Materials/Hair");
			}
			if (data.StyleMode)
			{
				if (data.BarycentricsFixed != null)
				{
					rend.material.SetBuffer("_Barycentrics", data.BarycentricsFixed.ComputeBuffer);
				}
				else
				{
					rend.material.SetBuffer("_Barycentrics", null);
				}
			}
			else if (data.Barycentrics != null)
			{
				rend.material.SetBuffer("_Barycentrics", data.Barycentrics.ComputeBuffer);
			}
			else
			{
				rend.material.SetBuffer("_Barycentrics", null);
			}
			if (data.TessRenderParticles != null)
			{
				rend.material.SetBuffer("_Particles", data.TessRenderParticles.ComputeBuffer);
			}
			else
			{
				rend.material.SetBuffer("_Particles", null);
			}
		}

		public void Dispatch()
		{
			UpdateBounds();
			UpdateMaterial();
			UpdateRenderer();
		}

		public Shader GetShader()
		{
			if (rend != null && rend.material != null)
			{
				return rend.material.shader;
			}
			if (data.material != null)
			{
				return data.material.shader;
			}
			return null;
		}

		public void SetShader(Shader s)
		{
			if (rend.material.shader != s)
			{
				rend.material.shader = s;
			}
		}

		private void UpdateBounds()
		{
			mesh.bounds = base.transform.InverseTransformBounds(data.Bounds);
		}

		private void UpdateMaterial()
		{
			if (data.StyleMode)
			{
				if (data.BarycentricsFixed != null)
				{
					rend.material.SetBuffer("_Barycentrics", data.BarycentricsFixed.ComputeBuffer);
				}
				else
				{
					rend.material.SetBuffer("_Barycentrics", null);
				}
			}
			else if (data.Barycentrics != null)
			{
				rend.material.SetBuffer("_Barycentrics", data.Barycentrics.ComputeBuffer);
			}
			else
			{
				rend.material.SetBuffer("_Barycentrics", null);
			}
			if (data.TessRenderParticles != null)
			{
				rend.material.SetBuffer("_Particles", data.TessRenderParticles.ComputeBuffer);
			}
			else
			{
				rend.material.SetBuffer("_Particles", null);
			}
			rend.material.SetVector("_LightCenter", data.LightCenter);
			if (data.StyleMode)
			{
				Vector2 vector = default(Vector2);
				vector.x = 4f;
				vector.y = data.TessFactor.y;
				rend.material.SetFloat("_RandomBarycentric", 0f);
				rend.material.SetVector("_TessFactor", vector);
				rend.material.SetFloat("_StandWidth", 0.001f * data.WorldScale);
				rend.material.SetFloat("_MaxSpread", 1f);
			}
			else
			{
				rend.material.SetFloat("_RandomBarycentric", 1f);
				rend.material.SetVector("_TessFactor", data.TessFactor);
				rend.material.SetFloat("_StandWidth", data.StandWidth * data.WorldScale);
				rend.material.SetFloat("_MaxSpread", data.MaxSpread * data.WorldScale);
			}
			rend.material.SetFloat("_SpecularShift", data.SpecularShift);
			rend.material.SetFloat("_PrimarySpecular", data.PrimarySpecular);
			rend.material.SetFloat("_SecondarySpecular", data.SecondarySpecular);
			rend.material.SetColor("_SpecularColor", data.SpecularColor);
			rend.material.SetFloat("_Diffuse", data.Diffuse);
			rend.material.SetFloat("_FresnelPower", data.FresnelPower);
			rend.material.SetFloat("_FresnelAtten", data.FresnelAttenuation);
			rend.material.SetVector("_WavinessAxis", data.WavinessAxis);
			rend.material.SetVector("_Length", data.Length);
			rend.material.SetFloat("_Volume", data.Volume);
			rend.material.SetVector("_Size", data.Size);
			rend.material.SetFloat("_RandomTexColorPower", data.RandomTexColorPower);
			rend.material.SetFloat("_RandomTexColorOffset", data.RandomTexColorOffset);
			rend.material.SetFloat("_IBLFactor", data.IBLFactor);
		}

		private void UpdateRenderer()
		{
			rend.shadowCastingMode = (data.CastShadows ? ShadowCastingMode.On : ShadowCastingMode.Off);
			rend.receiveShadows = data.ReseiveShadows;
		}
	}
}
