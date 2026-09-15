using System;
using System.Collections.Generic;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Hair.Scripts.Geometry.Abstract;
using GPUTools.Hair.Scripts.Geometry.Tools;
using GPUTools.Hair.Scripts.Types;
using GPUTools.Hair.Scripts.Utils;
using GPUTools.Skinner.Scripts.Providers;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Geometry.Import
{
	[ExecuteInEditMode]
	public class HairGeometryImporter : GeometryProviderBase
	{
		[SerializeField]
		public bool DebugDraw = true;

		[SerializeField]
		public int Segments = 5;

		[SerializeField]
		public HairGroupsProvider HairGroupsProvider = new HairGroupsProvider();

		[SerializeField]
		public MeshProvider ScalpProvider = new MeshProvider();

		[SerializeField]
		public int[] Indices;

		[SerializeField]
		public Bounds Bounds;

		public override bool Validate(bool log)
		{
			if (Indices == null || Indices.Length == 0)
			{
				if (log)
				{
					Debug.LogError("Provider does not have any generated hair geometry");
				}
				return false;
			}
			return ValidateImpl(log);
		}

		private bool ValidateImpl(bool log)
		{
			if (!ScalpProvider.Validate(false))
			{
				if (log)
				{
					Debug.LogError("Scalp field is null");
				}
				return false;
			}
			return HairGroupsProvider.Validate(log);
		}

		public void Process()
		{
			if (ValidateImpl(true))
			{
				HairGroupsProvider.Process(ScalpProvider.ToWorldMatrix.inverse);
				Indices = ProcessIndices();
			}
		}

		public override void Dispatch()
		{
			ScalpProvider.Dispatch();
		}

		private void OnDestroy()
		{
			ScalpProvider.Dispose();
		}

		private int[] ProcessMap()
		{
			float accuracy = ScalpProcessingTools.MiddleDistanceBetweenPoints(ScalpProvider.Mesh) * 0.1f;
			if (ScalpProvider.Type == ScalpMeshType.Skinned || ScalpProvider.Type == ScalpMeshType.PreCalc)
			{
				List<Vector3> scalpVertices = ScalpProvider.Mesh.vertices.ToList();
				return ScalpProcessingTools.HairRootToScalpIndices(scalpVertices, HairGroupsProvider.Vertices, GetSegmentsNum(), accuracy).ToArray();
			}
			return new int[HairGroupsProvider.Vertices.Count / GetSegmentsNum()];
		}

		private int[] ProcessIndices()
		{
			float accuracy = ScalpProcessingTools.MiddleDistanceBetweenPoints(ScalpProvider.Mesh) * 0.1f;
			List<int> scalpIndices = ScalpProvider.Mesh.GetIndices(0).ToList();
			List<Vector3> scalpVertices = ScalpProvider.Mesh.vertices.ToList();
			return ScalpProcessingTools.ProcessIndices(scalpIndices, scalpVertices, HairGroupsProvider.VerticesGroups, GetSegmentsNum(), accuracy).ToArray();
		}

		public override GpuBuffer<Matrix4x4> GetTransformsBuffer()
		{
			return ScalpProvider.ToWorldMatricesBuffer;
		}

		public override GpuBuffer<Vector3> GetNormalsBuffer()
		{
			return ScalpProvider.NormalsBuffer;
		}

		public override Matrix4x4 GetToWorldMatrix()
		{
			return ScalpProvider.ToWorldMatrix;
		}

		public override int[] GetHairRootToScalpMap()
		{
			return ProcessMap();
		}

		public override Bounds GetBounds()
		{
			return base.transform.TransformBounds(Bounds);
		}

		public override int GetSegmentsNum()
		{
			return Segments;
		}

		public override int GetStandsNum()
		{
			return HairGroupsProvider.Vertices.Count / Segments;
		}

		public override int[] GetIndices()
		{
			return Indices;
		}

		public override List<Vector3> GetVertices()
		{
			return HairGroupsProvider.Vertices;
		}

		public override void SetVertices(List<Vector3> verts)
		{
			throw new NotImplementedException();
		}

		public override List<float> GetRigidities()
		{
			return null;
		}

		public override void SetRigidities(List<float> rigidities)
		{
			throw new NotImplementedException();
		}

		public override void CalculateNearbyVertexGroups()
		{
			throw new NotImplementedException();
		}

		public override List<Vector4ListContainer> GetNearbyVertexGroups()
		{
			throw new NotImplementedException();
		}

		public override List<Color> GetColors()
		{
			return HairGroupsProvider.Colors;
		}

		private void OnDrawGizmos()
		{
			if (!DebugDraw || GetVertices() == null || !ValidateImpl(false))
			{
				return;
			}
			Matrix4x4 toWorldMatrix = ScalpProvider.ToWorldMatrix;
			List<Vector3> vertices = GetVertices();
			for (int i = 1; i < vertices.Count; i++)
			{
				if (i % Segments != 0)
				{
					Vector3 from = toWorldMatrix.MultiplyPoint3x4(vertices[i - 1]);
					Vector3 to = toWorldMatrix.MultiplyPoint3x4(vertices[i]);
					Gizmos.DrawLine(from, to);
				}
			}
			Bounds bounds = GetBounds();
			Gizmos.DrawWireCube(bounds.center, bounds.size);
		}
	}
}
