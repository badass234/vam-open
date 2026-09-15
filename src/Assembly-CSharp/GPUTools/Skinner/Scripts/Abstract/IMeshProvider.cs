using GPUTools.Common.Scripts.PL.Tools;
using UnityEngine;

namespace GPUTools.Skinner.Scripts.Abstract
{
	public interface IMeshProvider
	{
		Matrix4x4 ToWorldMatrix { get; }

		GpuBuffer<Matrix4x4> ToWorldMatricesBuffer { get; }

		GpuBuffer<Vector3> PreCalculatedVerticesBuffer { get; }

		GpuBuffer<Vector3> NormalsBuffer { get; }

		Mesh Mesh { get; }

		Mesh MeshForImport { get; }

		bool Validate(bool log);

		void Dispatch();

		void Dispose();
	}
}
