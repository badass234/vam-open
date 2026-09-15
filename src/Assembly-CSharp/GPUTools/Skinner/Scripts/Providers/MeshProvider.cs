using System;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Skinner.Scripts.Abstract;
using UnityEngine;

namespace GPUTools.Skinner.Scripts.Providers
{
	[Serializable]
	public class MeshProvider : IMeshProvider
	{
		public ScalpMeshType Type;

		[SerializeField]
		public SkinnedMeshProvider SkinnedProvider = new SkinnedMeshProvider();

		[SerializeField]
		public StaticMeshProvider StaticProvider = new StaticMeshProvider();

		[SerializeField]
		public PreCalcMeshProvider PreCalcProvider;

		public Matrix4x4 ToWorldMatrix => GetCurrentProvider()?.ToWorldMatrix ?? Matrix4x4.identity;

		public GpuBuffer<Matrix4x4> ToWorldMatricesBuffer => GetCurrentProvider()?.ToWorldMatricesBuffer;

		public GpuBuffer<Vector3> PreCalculatedVerticesBuffer => GetCurrentProvider()?.PreCalculatedVerticesBuffer;

		public GpuBuffer<Vector3> NormalsBuffer => GetCurrentProvider()?.NormalsBuffer;

		public Mesh Mesh => GetCurrentProvider()?.Mesh;

		public Color[] SimColors
		{
			get
			{
				if (Type == ScalpMeshType.PreCalc && PreCalcProvider != null)
				{
					return PreCalcProvider.VertexSimColors;
				}
				return null;
			}
		}

		public Mesh MeshForImport => GetCurrentProvider()?.MeshForImport;

		public bool Validate(bool log)
		{
			return GetCurrentProvider()?.Validate(log) ?? false;
		}

		public void Stop()
		{
			if (PreCalcProvider != null)
			{
				PreCalcProvider.Stop();
			}
		}

		public void Dispatch()
		{
			GetCurrentProvider()?.Dispatch();
		}

		public void Dispose()
		{
			GetCurrentProvider()?.Dispose();
		}

		private IMeshProvider GetCurrentProvider()
		{
			if (Type == ScalpMeshType.Static)
			{
				return StaticProvider;
			}
			if (Type == ScalpMeshType.PreCalc)
			{
				return PreCalcProvider;
			}
			return SkinnedProvider;
		}
	}
}
