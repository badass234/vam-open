using System;
using System.Collections.Generic;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Hair.Scripts.Geometry.Abstract;
using GPUTools.Hair.Scripts.Geometry.MayaImport.Data;
using GPUTools.Hair.Scripts.Geometry.MayaImport.Debug;
using GPUTools.Hair.Scripts.Types;
using GPUTools.Hair.Scripts.Utils;
using GPUTools.Skinner.Scripts.Providers;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Geometry.MayaImport
{
	public class MayaHairGeometryImporter : GeometryProviderBase
	{
		[SerializeField]
		public bool DebugDraw = true;

		[SerializeField]
		public Texture2D RegionsTexture;

		[SerializeField]
		public MeshProvider ScalpProvider;

		[SerializeField]
		public GameObject HairContainer;

		[SerializeField]
		public float RegionThresholdDistance = 0.5f;

		[SerializeField]
		public MayaHairData Data = new MayaHairData();

		[SerializeField]
		public Bounds Bounds;

		public void Process()
		{
			if (ValidateImpl(true))
			{
				new CacheMayaHairData(this).Cache();
				Data.Validate(true);
			}
		}

		public override void Dispatch()
		{
			ScalpProvider.Dispatch();
		}

		public bool ValidateHairContainer(bool log)
		{
			if (HairContainer == null)
			{
				if (log)
				{
				}
				return false;
			}
			return true;
		}

		public override bool Validate(bool log)
		{
			return ValidateImpl(log) && Data.Validate(log);
		}

		private bool ValidateImpl(bool log)
		{
			return ScalpProvider.Validate(log) && ValidateHairContainer(log);
		}

		public override Bounds GetBounds()
		{
			return base.transform.TransformBounds(Bounds);
		}

		public override int GetSegmentsNum()
		{
			return Data.Segments;
		}

		public override int GetStandsNum()
		{
			return Data.Vertices.Count / Data.Segments;
		}

		public override int[] GetIndices()
		{
			return Data.Indices;
		}

		public override List<Vector3> GetVertices()
		{
			return Data.Vertices;
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
			return new Color[Data.Vertices.Count].ToList();
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
			return Data.HairRootToScalpMap;
		}

		private void OnDrawGizmos()
		{
			if (DebugDraw && Validate(false))
			{
				MayaImporterDebugDraw.Draw(this);
			}
		}
	}
}
