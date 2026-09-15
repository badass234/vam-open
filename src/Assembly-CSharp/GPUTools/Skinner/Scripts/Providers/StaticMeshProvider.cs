using System;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Skinner.Scripts.Abstract;
using UnityEngine;

namespace GPUTools.Skinner.Scripts.Providers
{
	[Serializable]
	public class StaticMeshProvider : IMeshProvider
	{
		[SerializeField]
		public MeshFilter MeshFilter;

		private GpuBuffer<Matrix4x4> toWorldMatricesBuffer;

		private GpuBuffer<Matrix4x4> oldToWorldMatricesBuffer;

		public Matrix4x4 ToWorldMatrix => MeshFilter.transform.localToWorldMatrix;

		public GpuBuffer<Matrix4x4> ToWorldMatricesBuffer
		{
			get
			{
				UpdateToWorldMatrices();
				return toWorldMatricesBuffer;
			}
		}

		public GpuBuffer<Vector3> NormalsBuffer => null;

		public GpuBuffer<Vector3> PreCalculatedVerticesBuffer => null;

		public Mesh Mesh
		{
			get
			{
				if (MeshFilter != null)
				{
					if (Application.isPlaying)
					{
						return MeshFilter.mesh;
					}
					return MeshFilter.sharedMesh;
				}
				return null;
			}
		}

		public Mesh MeshForImport
		{
			get
			{
				if (MeshFilter != null)
				{
					if (Application.isPlaying)
					{
						return MeshFilter.mesh;
					}
					return MeshFilter.sharedMesh;
				}
				return null;
			}
		}

		public bool Validate(bool log)
		{
			if (log)
			{
			}
			return MeshFilter != null;
		}

		private void UpdateToWorldMatrices()
		{
			if (toWorldMatricesBuffer == null)
			{
				toWorldMatricesBuffer = new GpuBuffer<Matrix4x4>(1, 64);
			}
			toWorldMatricesBuffer.Data[0] = MeshFilter.transform.localToWorldMatrix;
			toWorldMatricesBuffer.PushData();
		}

		public void Dispatch()
		{
			UpdateToWorldMatrices();
		}

		public void Dispose()
		{
			if (toWorldMatricesBuffer != null)
			{
				toWorldMatricesBuffer.Dispose();
			}
		}
	}
}
