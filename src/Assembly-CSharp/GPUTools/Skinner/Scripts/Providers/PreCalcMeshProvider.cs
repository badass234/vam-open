using System;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Skinner.Scripts.Abstract;
using UnityEngine;

namespace GPUTools.Skinner.Scripts.Providers
{
	[Serializable]
	public class PreCalcMeshProvider : MonoBehaviour, IMeshProvider
	{
		public bool provideToWorldMatrices;

		public bool useBaseMesh;

		public int[] materialsToUse;

		public bool drawInPostProcess = true;

		public virtual Matrix4x4 ToWorldMatrix => base.transform.localToWorldMatrix;

		public virtual GpuBuffer<Matrix4x4> ToWorldMatricesBuffer => null;

		public virtual GpuBuffer<Vector3> PreCalculatedVerticesBuffer { get; protected set; }

		public virtual GpuBuffer<Vector3> NormalsBuffer { get; protected set; }

		public virtual Mesh Mesh => null;

		public virtual Mesh BaseMesh => null;

		public virtual Mesh MeshForImport => null;

		public virtual Color[] VertexSimColors => null;

		public virtual bool Validate(bool log)
		{
			if (log)
			{
			}
			return Mesh != null;
		}

		public virtual void Stop()
		{
		}

		public virtual void Dispatch()
		{
		}

		public virtual void PostProcessDispatch(ComputeBuffer finalVerts)
		{
		}

		public virtual void Dispose()
		{
		}
	}
}
