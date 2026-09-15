using System;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Skinner.Scripts.Abstract;
using GPUTools.Skinner.Scripts.Kernels;
using GPUTools.Skinner.Scripts.Utils;
using UnityEngine;

namespace GPUTools.Skinner.Scripts.Providers
{
	[Serializable]
	public class SkinnedMeshProvider : IMeshProvider
	{
		[SerializeField]
		public SkinnedMeshRenderer SkinnedMeshRenderer;

		private GPUSkinnerPro gpuSkinner;

		public GpuBuffer<Matrix4x4> ToWorldMatricesBuffer
		{
			get
			{
				UpdateToWorldMatricesBufferGPU();
				return gpuSkinner.TransformMatricesBuffer;
			}
		}

		public GpuBuffer<Vector3> NormalsBuffer => null;

		public Matrix4x4 ToWorldMatrix => MeshSkinUtils.CreateToWorldMatrix(SkinnedMeshRenderer);

		public GpuBuffer<Vector3> PreCalculatedVerticesBuffer => null;

		public Mesh Mesh => SkinnedMeshRenderer.sharedMesh;

		public Mesh MeshForImport => SkinnedMeshRenderer.sharedMesh;

		public bool Validate(bool log)
		{
			if (log)
			{
			}
			return SkinnedMeshRenderer != null && SkinnedMeshRenderer.sharedMesh != null;
		}

		private void UpdateToWorldMatricesBufferGPU()
		{
			if (gpuSkinner == null)
			{
				gpuSkinner = new GPUSkinnerPro(SkinnedMeshRenderer);
			}
		}

		public void Dispatch()
		{
			if (gpuSkinner != null)
			{
				gpuSkinner.Dispatch();
			}
		}

		public void Dispose()
		{
			if (gpuSkinner != null)
			{
				gpuSkinner.Dispose();
			}
		}
	}
}
