using GPUTools.Common.Scripts.PL.Tools;
using UnityEngine;

namespace GPUTools.Skinner.Scripts.Providers
{
	public class PreCalcMeshProviderHolder : PreCalcMeshProvider
	{
		protected PreCalcMeshProvider _provider;

		public PreCalcMeshProvider provider
		{
			get
			{
				return _provider;
			}
			set
			{
				if (_provider != value)
				{
					_provider = value;
				}
			}
		}

		public override Matrix4x4 ToWorldMatrix
		{
			get
			{
				if (provider != null)
				{
					return provider.ToWorldMatrix;
				}
				return Matrix4x4.identity;
			}
		}

		public override GpuBuffer<Matrix4x4> ToWorldMatricesBuffer
		{
			get
			{
				if (provider != null)
				{
					return provider.ToWorldMatricesBuffer;
				}
				return null;
			}
		}

		public override GpuBuffer<Vector3> PreCalculatedVerticesBuffer
		{
			get
			{
				if (provider != null)
				{
					return provider.PreCalculatedVerticesBuffer;
				}
				return null;
			}
		}

		public override GpuBuffer<Vector3> NormalsBuffer
		{
			get
			{
				if (provider != null)
				{
					return provider.NormalsBuffer;
				}
				return null;
			}
		}

		public override Mesh Mesh
		{
			get
			{
				if (provider != null)
				{
					return provider.Mesh;
				}
				return null;
			}
		}

		public override Mesh BaseMesh
		{
			get
			{
				if (provider != null)
				{
					return provider.BaseMesh;
				}
				return null;
			}
		}

		public override Mesh MeshForImport
		{
			get
			{
				if (provider != null)
				{
					return provider.MeshForImport;
				}
				return null;
			}
		}

		public override Color[] VertexSimColors
		{
			get
			{
				if (provider != null)
				{
					return provider.VertexSimColors;
				}
				return null;
			}
		}

		public override bool Validate(bool log)
		{
			if (provider != null)
			{
				return provider.Validate(log);
			}
			return false;
		}

		public override void Stop()
		{
			if (provider != null)
			{
				provider.Stop();
			}
		}

		public override void Dispatch()
		{
			if (provider != null)
			{
				provider.provideToWorldMatrices = provideToWorldMatrices;
				provider.Dispatch();
			}
		}

		public override void PostProcessDispatch(ComputeBuffer finalVerts)
		{
			if (provider != null)
			{
				provider.PostProcessDispatch(finalVerts);
			}
		}

		public override void Dispose()
		{
			if (provider != null)
			{
				provider.Dispose();
			}
		}
	}
}
