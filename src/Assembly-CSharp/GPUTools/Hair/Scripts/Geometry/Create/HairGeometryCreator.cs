using System;
using System.Collections.Generic;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Hair.Scripts.Geometry.Abstract;
using GPUTools.Hair.Scripts.Geometry.Tools;
using GPUTools.Hair.Scripts.Types;
using GPUTools.Hair.Scripts.Utils;
using GPUTools.Skinner.Scripts.Providers;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Geometry.Create
{
	[Serializable]
	public class HairGeometryCreator : GeometryProviderBase
	{
		protected struct VertDistance
		{
			public int vert;

			public float distance;
		}

		[SerializeField]
		public bool DebugDraw;

		[SerializeField]
		public bool DebugDrawUnselectedGroups = true;

		[SerializeField]
		public int Segments = 5;

		[SerializeField]
		public GeometryBrush Brush = new GeometryBrush();

		[SerializeField]
		public MeshProvider ScalpProvider = new MeshProvider();

		[SerializeField]
		public List<GameObject> ColliderProviders = new List<GameObject>();

		[SerializeField]
		public CreatorGeometry Geomery = new CreatorGeometry();

		[SerializeField]
		public Bounds Bounds;

		[SerializeField]
		public float NearbyVertexSearchMinDistance;

		[SerializeField]
		public float NearbyVertexSearchDistance = 0.01f;

		[SerializeField]
		public int MaxNearbyVertsPerVert = 2;

		[SerializeField]
		private int[] indices;

		[SerializeField]
		private List<Vector3> vertices;

		[SerializeField]
		private List<Color> colors;

		[SerializeField]
		private int[] hairRootToScalpIndices;

		[SerializeField]
		public List<Vector4ListContainer> nearbyVertexGroups;

		[SerializeField]
		private bool isProcessed;

		private void Awake()
		{
			if (!isProcessed && Application.isPlaying)
			{
				Process();
			}
		}

		public void Optimize()
		{
			Process();
		}

		public void SetDirty()
		{
			isProcessed = false;
		}

		public void ClearNearbyVertices()
		{
			nearbyVertexGroups = null;
			isProcessed = false;
		}

		public bool IsDirty()
		{
			return !isProcessed;
		}

		public void Process()
		{
			Debug.Log("Hair Geometry Creator Process() called");
			if (!ScalpProvider.Validate(true))
			{
				return;
			}
			List<List<Vector3>> list = new List<List<Vector3>>();
			List<Vector3> list2 = new List<Vector3>();
			List<Color> list3 = new List<Color>();
			foreach (GeometryGroupData item in Geomery.List)
			{
				list.Add(item.Vertices);
				list2.AddRange(item.Vertices);
				list3.AddRange(item.Colors);
			}
			vertices = list2;
			colors = list3;
			Mesh mesh = ScalpProvider.Mesh;
			float accuracy = ScalpProcessingTools.MiddleDistanceBetweenPoints(mesh) * 0.1f;
			indices = ScalpProcessingTools.ProcessIndices(mesh.GetIndices(0).ToList(), mesh.vertices.ToList(), list, Segments, accuracy).ToArray();
			if (ScalpProvider.Type == ScalpMeshType.Skinned)
			{
				hairRootToScalpIndices = ScalpProcessingTools.HairRootToScalpIndices(mesh.vertices.ToList(), vertices, Segments, accuracy).ToArray();
			}
			else if (ScalpProvider.Type == ScalpMeshType.PreCalc)
			{
				hairRootToScalpIndices = ScalpProcessingTools.HairRootToScalpIndices(mesh.vertices.ToList(), vertices, Segments, accuracy).ToArray();
			}
			else
			{
				hairRootToScalpIndices = new int[vertices.Count / GetSegmentsNum()];
			}
			CalculateNearbyVertexGroups();
			isProcessed = true;
		}

		public override void Dispatch()
		{
			if (ScalpProvider.Type == ScalpMeshType.PreCalc && ScalpProvider.PreCalcProvider != null)
			{
				ScalpProvider.PreCalcProvider.provideToWorldMatrices = true;
			}
			ScalpProvider.Dispatch();
		}

		public override bool Validate(bool log)
		{
			return ScalpProvider.Validate(log) && Geomery.Validate(log);
		}

		private void OnDestroy()
		{
			ScalpProvider.Dispose();
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
			return vertices.Count / Segments;
		}

		public override int[] GetIndices()
		{
			return indices;
		}

		public override List<Vector3> GetVertices()
		{
			return vertices;
		}

		public override void SetVertices(List<Vector3> verts)
		{
			vertices = verts;
		}

		public override List<float> GetRigidities()
		{
			return null;
		}

		public override void SetRigidities(List<float> rigidities)
		{
			throw new NotImplementedException();
		}

		private bool AddToHashSet(HashSet<Vector4> set, int i1, int i2, float distance, float distanceRatio)
		{
			if (i1 == -1 || i2 == -1)
			{
				return false;
			}
			return set.Add((i1 <= i2) ? new Vector4(i2, i1, distance, distanceRatio) : new Vector4(i1, i2, distance, distanceRatio));
		}

		public override void CalculateNearbyVertexGroups()
		{
			nearbyVertexGroups = new List<Vector4ListContainer>();
			Matrix4x4 toWorldMatrix = GetToWorldMatrix();
			List<Vector3> list = new List<Vector3>();
			foreach (Vector3 vertex in vertices)
			{
				Vector3 item = toWorldMatrix.MultiplyPoint3x4(vertex);
				list.Add(item);
			}
			HashSet<Vector4> hashSet = new HashSet<Vector4>();
			VertDistance item2 = default(VertDistance);
			for (int i = 0; i < list.Count; i++)
			{
				if (i % Segments == 0)
				{
					continue;
				}
				int num = i / Segments;
				List<VertDistance> list2 = new List<VertDistance>();
				for (int j = 0; j < list.Count; j++)
				{
					if (j % Segments == 0)
					{
						continue;
					}
					int num2 = j / Segments;
					if (num != num2)
					{
						float num3 = Vector3.Distance(list[i], list[j]);
						if (num3 < NearbyVertexSearchDistance && num3 > NearbyVertexSearchMinDistance)
						{
							item2.vert = j;
							item2.distance = num3;
							list2.Add(item2);
						}
					}
				}
				list2.Sort((VertDistance vd1, VertDistance vd2) => vd1.distance.CompareTo(vd2.distance));
				int num4 = 0;
				foreach (VertDistance item5 in list2)
				{
					if (num4 > MaxNearbyVertsPerVert)
					{
						break;
					}
					AddToHashSet(hashSet, i, item5.vert, item5.distance, (NearbyVertexSearchDistance - item5.distance) / NearbyVertexSearchDistance);
					num4++;
				}
			}
			Debug.Log("Found " + hashSet.Count + " nearby vertex pairs");
			List<Vector4> list3 = hashSet.ToList();
			List<HashSet<int>> list4 = new List<HashSet<int>>();
			foreach (Vector4 item6 in list3)
			{
				bool flag = false;
				int item3 = (int)item6.x;
				int item4 = (int)item6.y;
				for (int k = 0; k < list4.Count; k++)
				{
					HashSet<int> hashSet2 = list4[k];
					if (!hashSet2.Contains(item3) && !hashSet2.Contains(item4))
					{
						flag = true;
						hashSet2.Add(item3);
						hashSet2.Add(item4);
						Vector4ListContainer vector4ListContainer = nearbyVertexGroups[k];
						vector4ListContainer.List.Add(item6);
						break;
					}
				}
				if (!flag)
				{
					HashSet<int> hashSet3 = new HashSet<int>();
					list4.Add(hashSet3);
					hashSet3.Add(item3);
					hashSet3.Add(item4);
					Vector4ListContainer vector4ListContainer2 = new Vector4ListContainer();
					vector4ListContainer2.List.Add(item6);
					nearbyVertexGroups.Add(vector4ListContainer2);
				}
			}
			Debug.Log("Created " + nearbyVertexGroups.Count + " nearby vertex pair groups");
		}

		public override List<Vector4ListContainer> GetNearbyVertexGroups()
		{
			if (nearbyVertexGroups == null || nearbyVertexGroups.Count == 0)
			{
				Debug.Log("Vertex groups not precalculated. Must build at runtime which is slow");
				CalculateNearbyVertexGroups();
			}
			return nearbyVertexGroups;
		}

		public override List<Color> GetColors()
		{
			return colors;
		}

		public override Matrix4x4 GetToWorldMatrix()
		{
			return ScalpProvider.ToWorldMatrix;
		}

		public override GpuBuffer<Matrix4x4> GetTransformsBuffer()
		{
			return ScalpProvider.ToWorldMatricesBuffer;
		}

		public override GpuBuffer<Vector3> GetNormalsBuffer()
		{
			return ScalpProvider.NormalsBuffer;
		}

		public override int[] GetHairRootToScalpMap()
		{
			return hairRootToScalpIndices;
		}

		private void OnDrawGizmos()
		{
			if (!DebugDraw || !ScalpProvider.Validate(false))
			{
				return;
			}
			foreach (GeometryGroupData item in Geomery.List)
			{
				bool flag = Geomery.Selected == item;
				if (flag || DebugDrawUnselectedGroups)
				{
					item.OnDrawGizmos(Segments, flag, ScalpProvider.ToWorldMatrix);
				}
			}
			Bounds bounds = GetBounds();
			Gizmos.DrawWireCube(bounds.center, bounds.size);
		}
	}
}
