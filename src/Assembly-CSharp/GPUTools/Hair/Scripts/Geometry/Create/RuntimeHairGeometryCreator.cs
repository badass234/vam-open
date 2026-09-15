using System;
using System.Collections.Generic;
using System.IO;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Hair.Scripts.Geometry.Abstract;
using GPUTools.Hair.Scripts.Geometry.Tools;
using GPUTools.Hair.Scripts.Types;
using GPUTools.Hair.Scripts.Utils;
using GPUTools.Skinner.Scripts.Providers;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Geometry.Create
{
	public class RuntimeHairGeometryCreator : GeometryProviderBase
	{
		[Serializable]
		public class ScalpMask
		{
			public string name;

			public bool[] vertices;

			public ScalpMask(int vertexCount)
			{
				vertices = new bool[vertexCount];
				name = string.Empty;
			}

			public void CopyVerticesFrom(ScalpMask otherMask)
			{
				if (vertices.Length == otherMask.vertices.Length)
				{
					for (int i = 0; i < vertices.Length; i++)
					{
						vertices[i] = otherMask.vertices[i];
					}
				}
				else
				{
					Debug.LogError("Attempted to copy mask vertices from a mask that has different vertex count");
				}
			}

			public void StoreToBinaryWriter(BinaryWriter binWriter)
			{
				binWriter.Write(name);
				binWriter.Write(vertices.Length);
				for (int i = 0; i < vertices.Length; i++)
				{
					binWriter.Write(vertices[i]);
				}
			}

			public void LoadFromBinaryReader(BinaryReader binReader)
			{
				name = binReader.ReadString();
				int num = binReader.ReadInt32();
				vertices = new bool[num];
				for (int i = 0; i < num; i++)
				{
					vertices[i] = binReader.ReadBoolean();
				}
			}

			public void ClearAll()
			{
				for (int i = 0; i < vertices.Length; i++)
				{
					vertices[i] = false;
				}
			}

			public void SetAll()
			{
				for (int i = 0; i < vertices.Length; i++)
				{
					vertices[i] = true;
				}
			}
		}

		[Serializable]
		public class Strand
		{
			public int scalpIndex;

			public List<Vector3> vertices;

			public Strand(int index)
			{
				scalpIndex = index;
			}

			public void StoreToBinaryWriter(BinaryWriter binWriter)
			{
				binWriter.Write(scalpIndex);
				if (vertices != null)
				{
					binWriter.Write(vertices.Count);
					for (int i = 0; i < vertices.Count; i++)
					{
						binWriter.Write(vertices[i].x);
						binWriter.Write(vertices[i].y);
						binWriter.Write(vertices[i].z);
					}
				}
				else
				{
					binWriter.Write(0);
				}
			}

			public void LoadFromBinaryReader(BinaryReader binReader)
			{
				scalpIndex = binReader.ReadInt32();
				int num = binReader.ReadInt32();
				if (num == 0)
				{
					vertices = null;
					return;
				}
				vertices = new List<Vector3>();
				Vector3 item = default(Vector3);
				for (int i = 0; i < num; i++)
				{
					item.x = binReader.ReadSingle();
					item.y = binReader.ReadSingle();
					item.z = binReader.ReadSingle();
					vertices.Add(item);
				}
			}
		}

		protected struct VertDistance
		{
			public int vert;

			public float distance;
		}

		protected bool _usingAuxData;

		[SerializeField]
		protected int _Segments = 5;

		[SerializeField]
		protected float _SegmentLength = 0.02f;

		protected string _ScalpProviderName;

		protected bool[] _enabledIndices;

		public Transform[] ScalpProviders;

		[SerializeField]
		protected PreCalcMeshProvider _ScalpProvider;

		public Bounds Bounds;

		public float NearbyVertexSearchMinDistance;

		public float NearbyVertexSearchDistance = 0.01f;

		public int MaxNearbyVertsPerVert = 4;

		[NonSerialized]
		public ScalpMask strandsMask;

		[NonSerialized]
		public ScalpMask strandsMaskWorking;

		[NonSerialized]
		public Strand[] strands;

		private int[] indices;

		private List<Vector3> vertices;

		private List<Vector3> verticesLoaded;

		private List<Color> colors;

		private int[] hairRootToScalpIndices;

		private List<Vector4ListContainer> nearbyVertexGroups;

		private List<Vector4ListContainer> nearbyVertexGroupsLoaded;

		private List<float> rigidities;

		private List<float> rigiditiesLoaded;

		public bool DebugDraw;

		public bool DebugDrawUnselectedGroups = true;

		private bool isProcessed;

		public string status = string.Empty;

		protected bool cancelCalculateNearbyVertexGroups;

		protected Matrix4x4 toWorldMatrix;

		public bool usingAuxData => _usingAuxData;

		public int Segments
		{
			get
			{
				return _Segments;
			}
			set
			{
				if (_Segments != value)
				{
					_Segments = value;
					Clear();
				}
			}
		}

		public float SegmentLength
		{
			get
			{
				return _SegmentLength;
			}
			set
			{
				if (_SegmentLength != value)
				{
					_SegmentLength = value;
				}
			}
		}

		public string ScalpProviderName => _ScalpProviderName;

		public bool[] enabledIndices => _enabledIndices;

		public PreCalcMeshProvider ScalpProvider
		{
			get
			{
				return _ScalpProvider;
			}
			set
			{
				if (_ScalpProvider != value)
				{
					_ScalpProvider = value;
					SyncToScalpProvider();
					ClearNearbyVertexGroups();
					GenerateOutput();
				}
			}
		}

		public void StoreToBinaryWriter(BinaryWriter binWriter)
		{
			binWriter.Write("RuntimeHairGeometryCreator");
			if (rigidities == null)
			{
				binWriter.Write("1.0");
			}
			else
			{
				binWriter.Write("1.1");
			}
			binWriter.Write(_ScalpProviderName);
			binWriter.Write(_Segments);
			binWriter.Write(SegmentLength);
			strandsMask.StoreToBinaryWriter(binWriter);
			binWriter.Write(strands.Length);
			for (int i = 0; i < strands.Length; i++)
			{
				strands[i].StoreToBinaryWriter(binWriter);
			}
			binWriter.Write(indices.Length);
			for (int j = 0; j < indices.Length; j++)
			{
				binWriter.Write(indices[j]);
			}
			binWriter.Write(vertices.Count);
			for (int k = 0; k < vertices.Count; k++)
			{
				binWriter.Write(vertices[k].x);
				binWriter.Write(vertices[k].y);
				binWriter.Write(vertices[k].z);
			}
			if (rigidities != null)
			{
				binWriter.Write(rigidities.Count);
				for (int l = 0; l < rigidities.Count; l++)
				{
					binWriter.Write(rigidities[l]);
				}
			}
			binWriter.Write(hairRootToScalpIndices.Length);
			for (int m = 0; m < hairRootToScalpIndices.Length; m++)
			{
				binWriter.Write(hairRootToScalpIndices[m]);
			}
			if (nearbyVertexGroups != null)
			{
				binWriter.Write(nearbyVertexGroups.Count);
				for (int n = 0; n < nearbyVertexGroups.Count; n++)
				{
					List<Vector4> list = nearbyVertexGroups[n].List;
					binWriter.Write(list.Count);
					for (int num = 0; num < list.Count; num++)
					{
						binWriter.Write(list[num].x);
						binWriter.Write(list[num].y);
						binWriter.Write(list[num].z);
						binWriter.Write(list[num].w);
					}
				}
			}
			else
			{
				binWriter.Write(0);
			}
		}

		public void StoreAuxToBinaryWriter(BinaryWriter binWriter)
		{
			_usingAuxData = true;
			binWriter.Write("RuntimeHairGeometryCreatorAux");
			if (rigidities == null)
			{
				binWriter.Write("1.0");
			}
			else
			{
				binWriter.Write("1.1");
			}
			binWriter.Write(vertices.Count);
			for (int i = 0; i < vertices.Count; i++)
			{
				binWriter.Write(vertices[i].x);
				binWriter.Write(vertices[i].y);
				binWriter.Write(vertices[i].z);
			}
			if (rigidities != null)
			{
				binWriter.Write(rigidities.Count);
				for (int j = 0; j < rigidities.Count; j++)
				{
					binWriter.Write(rigidities[j]);
				}
			}
			if (nearbyVertexGroups != null)
			{
				binWriter.Write(nearbyVertexGroups.Count);
				for (int k = 0; k < nearbyVertexGroups.Count; k++)
				{
					List<Vector4> list = nearbyVertexGroups[k].List;
					binWriter.Write(list.Count);
					for (int l = 0; l < list.Count; l++)
					{
						binWriter.Write(list[l].x);
						binWriter.Write(list[l].y);
						binWriter.Write(list[l].z);
						binWriter.Write(list[l].w);
					}
				}
			}
			else
			{
				binWriter.Write(0);
			}
		}

		public void LoadFromBinaryReader(BinaryReader binReader)
		{
			_usingAuxData = false;
			string text = binReader.ReadString();
			if (text != "RuntimeHairGeometryCreator")
			{
				throw new Exception("Invalid binary format for binary data passed to RuntimeHairGeometryCreator");
			}
			string text2 = binReader.ReadString();
			bool flag = false;
			if (text2 != "1.0" && text2 != "1.1")
			{
				throw new Exception("Invalid schema version " + text2 + " for binary data passed to RuntimeHairGeometryCreator");
			}
			if (text2 == "1.1")
			{
				flag = true;
			}
			_ScalpProviderName = binReader.ReadString();
			bool flag2 = false;
			Transform[] scalpProviders = ScalpProviders;
			foreach (Transform transform in scalpProviders)
			{
				if (transform.name == _ScalpProviderName)
				{
					transform.gameObject.SetActive(true);
					PreCalcMeshProvider component = transform.GetComponent<PreCalcMeshProvider>();
					if (component != null)
					{
						flag2 = true;
						ScalpProvider = component;
					}
				}
				else
				{
					transform.gameObject.SetActive(false);
				}
			}
			if (!flag2)
			{
				ScalpProvider = null;
				throw new Exception("Could not find scalp provider " + _ScalpProviderName);
			}
			_Segments = binReader.ReadInt32();
			SegmentLength = binReader.ReadSingle();
			strandsMask.LoadFromBinaryReader(binReader);
			int num = binReader.ReadInt32();
			int num2 = ((!_ScalpProvider.useBaseMesh) ? _ScalpProvider.Mesh.vertexCount : _ScalpProvider.BaseMesh.vertexCount);
			if (num != num2)
			{
				throw new Exception("Binary hair data not compatible with chosen scalp mesh");
			}
			for (int j = 0; j < num; j++)
			{
				strands[j].LoadFromBinaryReader(binReader);
			}
			int num3 = binReader.ReadInt32();
			indices = new int[num3];
			for (int k = 0; k < num3; k++)
			{
				indices[k] = binReader.ReadInt32();
			}
			int num4 = binReader.ReadInt32();
			vertices = new List<Vector3>();
			Vector3 item = default(Vector3);
			for (int l = 0; l < num4; l++)
			{
				item.x = binReader.ReadSingle();
				item.y = binReader.ReadSingle();
				item.z = binReader.ReadSingle();
				vertices.Add(item);
			}
			verticesLoaded = vertices;
			if (flag)
			{
				int num5 = binReader.ReadInt32();
				if ((float)num5 == 0f)
				{
					rigidities = null;
				}
				else
				{
					rigidities = new List<float>();
					for (int m = 0; m < num5; m++)
					{
						float item2 = binReader.ReadSingle();
						rigidities.Add(item2);
					}
				}
			}
			else
			{
				rigidities = null;
			}
			rigiditiesLoaded = rigidities;
			int num6 = binReader.ReadInt32();
			hairRootToScalpIndices = new int[num6];
			for (int n = 0; n < num6; n++)
			{
				hairRootToScalpIndices[n] = binReader.ReadInt32();
			}
			int num7 = binReader.ReadInt32();
			nearbyVertexGroups = new List<Vector4ListContainer>();
			Vector4 item3 = default(Vector4);
			for (int num8 = 0; num8 < num7; num8++)
			{
				Vector4ListContainer vector4ListContainer = new Vector4ListContainer();
				List<Vector4> list = (vector4ListContainer.List = new List<Vector4>());
				nearbyVertexGroups.Add(vector4ListContainer);
				int num9 = binReader.ReadInt32();
				for (int num10 = 0; num10 < num9; num10++)
				{
					item3.x = binReader.ReadSingle();
					item3.y = binReader.ReadSingle();
					item3.z = binReader.ReadSingle();
					item3.w = binReader.ReadSingle();
					list.Add(item3);
				}
			}
			nearbyVertexGroupsLoaded = nearbyVertexGroups;
		}

		public void RevertToLoadedData()
		{
			if (_usingAuxData && verticesLoaded != null)
			{
				_usingAuxData = false;
				vertices = verticesLoaded;
				rigidities = rigiditiesLoaded;
				nearbyVertexGroups = nearbyVertexGroupsLoaded;
			}
		}

		public void LoadAuxFromBinaryReader(BinaryReader binReader)
		{
			_usingAuxData = true;
			string text = binReader.ReadString();
			if (text != "RuntimeHairGeometryCreatorAux")
			{
				throw new Exception("Invalid aux binary format for binary data passed to RuntimeHairGeometryCreator");
			}
			string text2 = binReader.ReadString();
			bool flag = false;
			if (text2 != "1.0" && text2 != "1.1")
			{
				throw new Exception("Invalid schema version " + text2 + " for binary data passed to RuntimeHairGeometryCreator");
			}
			if (text2 == "1.1")
			{
				flag = true;
			}
			int num = binReader.ReadInt32();
			vertices = new List<Vector3>();
			Vector3 item = default(Vector3);
			for (int i = 0; i < num; i++)
			{
				item.x = binReader.ReadSingle();
				item.y = binReader.ReadSingle();
				item.z = binReader.ReadSingle();
				vertices.Add(item);
			}
			if (flag)
			{
				int num2 = binReader.ReadInt32();
				if ((float)num2 == 0f)
				{
					rigidities = null;
				}
				else
				{
					rigidities = new List<float>();
					for (int j = 0; j < num2; j++)
					{
						float item2 = binReader.ReadSingle();
						rigidities.Add(item2);
					}
				}
			}
			else
			{
				rigidities = null;
			}
			int num3 = binReader.ReadInt32();
			nearbyVertexGroups = new List<Vector4ListContainer>();
			Vector4 item3 = default(Vector4);
			for (int k = 0; k < num3; k++)
			{
				Vector4ListContainer vector4ListContainer = new Vector4ListContainer();
				List<Vector4> list = (vector4ListContainer.List = new List<Vector4>());
				nearbyVertexGroups.Add(vector4ListContainer);
				int num4 = binReader.ReadInt32();
				for (int l = 0; l < num4; l++)
				{
					item3.x = binReader.ReadSingle();
					item3.y = binReader.ReadSingle();
					item3.z = binReader.ReadSingle();
					item3.w = binReader.ReadSingle();
					list.Add(item3);
				}
			}
		}

		protected void SyncToScalpProvider()
		{
			if (!Application.isPlaying || !(_ScalpProvider != null))
			{
				return;
			}
			_ScalpProviderName = _ScalpProvider.name;
			int vertexCount;
			if (_ScalpProvider.useBaseMesh)
			{
				if (_ScalpProvider.BaseMesh == null)
				{
					return;
				}
				vertexCount = _ScalpProvider.BaseMesh.vertexCount;
			}
			else
			{
				if (_ScalpProvider.Mesh == null)
				{
					return;
				}
				vertexCount = _ScalpProvider.Mesh.vertexCount;
			}
			HashSet<int> hashSet = new HashSet<int>();
			if (_ScalpProvider.materialsToUse != null && _ScalpProvider.materialsToUse.Length > 0)
			{
				for (int i = 0; i < _ScalpProvider.materialsToUse.Length; i++)
				{
					int[] array = ((!_ScalpProvider.useBaseMesh) ? _ScalpProvider.Mesh.GetIndices(_ScalpProvider.materialsToUse[i]) : _ScalpProvider.BaseMesh.GetIndices(_ScalpProvider.materialsToUse[i]));
					for (int j = 0; j < array.Length; j++)
					{
						hashSet.Add(array[j]);
					}
				}
			}
			else
			{
				int subMeshCount = _ScalpProvider.Mesh.subMeshCount;
				for (int k = 0; k < subMeshCount; k++)
				{
					int[] array2 = ((!_ScalpProvider.useBaseMesh) ? _ScalpProvider.Mesh.GetIndices(k) : _ScalpProvider.BaseMesh.GetIndices(k));
					for (int l = 0; l < array2.Length; l++)
					{
						hashSet.Add(array2[l]);
					}
				}
			}
			_enabledIndices = new bool[vertexCount];
			for (int m = 0; m < vertexCount; m++)
			{
				_enabledIndices[m] = hashSet.Contains(m);
			}
			strandsMask = new ScalpMask(vertexCount);
			strandsMaskWorking = new ScalpMask(vertexCount);
			strands = new Strand[vertexCount];
			for (int n = 0; n < vertexCount; n++)
			{
				strands[n] = new Strand(n);
			}
		}

		private void Awake()
		{
			SyncToScalpProvider();
			if (Application.isPlaying && !isProcessed)
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
			ClearNearbyVertexGroups();
		}

		public bool IsDirty()
		{
			return !isProcessed;
		}

		public void MaskClearAll()
		{
			if (strandsMaskWorking != null)
			{
				strandsMaskWorking.ClearAll();
			}
			ApplyMaskChanges();
		}

		public void MaskSetAll()
		{
			if (strandsMaskWorking != null)
			{
				strandsMaskWorking.SetAll();
			}
			ApplyMaskChanges();
		}

		public void SetWorkingMaskToCurrentMask()
		{
			if (strandsMask != null)
			{
				strandsMaskWorking.CopyVerticesFrom(strandsMask);
			}
		}

		public void ApplyMaskChanges()
		{
			if (strandsMask != null)
			{
				strandsMask.CopyVerticesFrom(strandsMaskWorking);
				GenerateAll();
			}
		}

		public void Clear()
		{
			ClearAllStrands();
			ClearNearbyVertexGroups();
			GenerateOutput();
		}

		public void ClearAllStrands()
		{
			if (strands != null)
			{
				for (int i = 0; i < strands.Length; i++)
				{
					strands[i].vertices = null;
				}
			}
		}

		public void GenerateAll()
		{
			GenerateStrands();
			ClearNearbyVertexGroups();
			GenerateOutput();
		}

		protected void GenerateStrands()
		{
			if (!(_ScalpProvider != null))
			{
				return;
			}
			Mesh mesh = ((!_ScalpProvider.useBaseMesh) ? _ScalpProvider.Mesh : _ScalpProvider.BaseMesh);
			if (!(mesh != null))
			{
				return;
			}
			Vector3[] array = mesh.vertices;
			Vector3[] normals = mesh.normals;
			for (int i = 0; i < strands.Length; i++)
			{
				if (strandsMask.vertices[i] || !_enabledIndices[i])
				{
					strands[i].vertices = null;
				}
				else if (strands[i].vertices == null || strands[i].vertices.Count != Segments)
				{
					strands[i].vertices = new List<Vector3>();
					strands[i].vertices.Add(array[i]);
					for (int j = 1; j < Segments; j++)
					{
						strands[i].vertices.Add(array[i] + normals[i] * j * SegmentLength);
					}
				}
			}
		}

		public void GenerateOutput()
		{
			if (!(ScalpProvider != null) || strands == null)
			{
				return;
			}
			Mesh mesh = ((!_ScalpProvider.useBaseMesh) ? _ScalpProvider.Mesh : _ScalpProvider.BaseMesh);
			if (!(mesh != null))
			{
				return;
			}
			List<int> list = new List<int>();
			vertices = new List<Vector3>();
			rigidities = null;
			colors = new List<Color>();
			for (int i = 0; i < strands.Length; i++)
			{
				if (strands[i].vertices != null)
				{
					list.Add(i);
					colors.Add(Color.black);
					vertices.AddRange(strands[i].vertices);
				}
			}
			hairRootToScalpIndices = list.ToArray();
			List<List<Vector3>> list2 = new List<List<Vector3>>();
			list2.Add(vertices);
			float accuracy = ScalpProcessingTools.MiddleDistanceBetweenPoints(mesh) * 0.1f;
			List<int> list3 = new List<int>();
			if (_ScalpProvider.materialsToUse != null && _ScalpProvider.materialsToUse.Length > 0)
			{
				for (int j = 0; j < _ScalpProvider.materialsToUse.Length; j++)
				{
					int[] array = mesh.GetIndices(_ScalpProvider.materialsToUse[j]);
					for (int k = 0; k < array.Length; k++)
					{
						list3.Add(array[k]);
					}
				}
			}
			else
			{
				int subMeshCount = _ScalpProvider.Mesh.subMeshCount;
				for (int l = 0; l < subMeshCount; l++)
				{
					int[] array2 = mesh.GetIndices(l);
					for (int m = 0; m < array2.Length; m++)
					{
						list3.Add(array2[m]);
					}
				}
			}
			indices = ScalpProcessingTools.ProcessIndices(list3, mesh.vertices.ToList(), list2, Segments, accuracy).ToArray();
		}

		public void Process()
		{
			if (!(_ScalpProvider == null) && _ScalpProvider.Validate(true))
			{
				GenerateOutput();
				isProcessed = true;
			}
		}

		public override void Dispatch()
		{
			if (_ScalpProvider != null)
			{
				_ScalpProvider.provideToWorldMatrices = true;
				_ScalpProvider.Dispatch();
			}
		}

		public override bool Validate(bool log)
		{
			if (_ScalpProvider != null)
			{
				return _ScalpProvider.Validate(log);
			}
			return false;
		}

		private void OnDestroy()
		{
			if (_ScalpProvider != null)
			{
				_ScalpProvider.Dispose();
			}
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
			int num = 0;
			for (int i = 0; i < hairRootToScalpIndices.Length; i++)
			{
				int num2 = hairRootToScalpIndices[i];
				List<Vector3> list = new List<Vector3>();
				strands[num2].vertices = list;
				for (int j = 0; j < Segments; j++)
				{
					list.Add(vertices[num]);
					num++;
				}
			}
		}

		public override List<float> GetRigidities()
		{
			return rigidities;
		}

		public override void SetRigidities(List<float> rigiditiesList)
		{
			rigidities = rigiditiesList;
		}

		private bool AddToHashSet(HashSet<Vector4> set, int i1, int i2, float distance, float distanceRatio)
		{
			if (i1 == -1 || i2 == -1)
			{
				return false;
			}
			return set.Add((i1 <= i2) ? new Vector4(i2, i1, distance, distanceRatio) : new Vector4(i1, i2, distance, distanceRatio));
		}

		public void ClearNearbyVertexGroups()
		{
			nearbyVertexGroups = new List<Vector4ListContainer>();
			Vector4ListContainer item = new Vector4ListContainer();
			nearbyVertexGroups.Add(item);
		}

		public void CancelCalculateNearbyVertexGroups()
		{
			cancelCalculateNearbyVertexGroups = true;
		}

		public void PrepareCalculateNearbyVertexGroups()
		{
			toWorldMatrix = GetToWorldMatrix();
		}

		public override void CalculateNearbyVertexGroups()
		{
			cancelCalculateNearbyVertexGroups = false;
			nearbyVertexGroups = new List<Vector4ListContainer>();
			status = "Preparing vertices";
			List<Vector3> list = new List<Vector3>();
			foreach (Vector3 vertex in vertices)
			{
				Vector3 item = toWorldMatrix.MultiplyPoint3x4(vertex);
				list.Add(item);
			}
			HashSet<Vector4> hashSet = new HashSet<Vector4>();
			HashSet<int> hashSet2 = new HashSet<int>();
			bool flag = MaxNearbyVertsPerVert == 1;
			if (cancelCalculateNearbyVertexGroups)
			{
				return;
			}
			VertDistance item2 = default(VertDistance);
			for (int i = 0; i < list.Count; i++)
			{
				if (i % Segments == 0 || (flag && hashSet2.Contains(i)))
				{
					continue;
				}
				int num = i / Segments;
				List<VertDistance> list2 = new List<VertDistance>();
				for (int j = 0; j < list.Count; j++)
				{
					if (j % Segments == 0 || (flag && hashSet2.Contains(j)))
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
					if (num4 >= MaxNearbyVertsPerVert)
					{
						break;
					}
					if (flag)
					{
						hashSet2.Add(i);
						hashSet2.Add(item5.vert);
					}
					AddToHashSet(hashSet, i, item5.vert, item5.distance, (NearbyVertexSearchDistance - item5.distance) / NearbyVertexSearchDistance);
					num4++;
				}
				status = "Processed " + i + " of " + list.Count + " vertices";
				if (cancelCalculateNearbyVertexGroups)
				{
					return;
				}
			}
			Debug.Log("Found " + hashSet.Count + " nearby vertex pairs");
			List<Vector4> list3 = hashSet.ToList();
			List<HashSet<int>> list4 = new List<HashSet<int>>();
			status = "Converting to vertex groups";
			foreach (Vector4 item6 in list3)
			{
				bool flag2 = false;
				int item3 = (int)item6.x;
				int item4 = (int)item6.y;
				for (int k = 0; k < list4.Count; k++)
				{
					HashSet<int> hashSet3 = list4[k];
					if (!hashSet3.Contains(item3) && !hashSet3.Contains(item4))
					{
						flag2 = true;
						hashSet3.Add(item3);
						hashSet3.Add(item4);
						Vector4ListContainer vector4ListContainer = nearbyVertexGroups[k];
						vector4ListContainer.List.Add(item6);
						break;
					}
				}
				if (!flag2)
				{
					HashSet<int> hashSet4 = new HashSet<int>();
					list4.Add(hashSet4);
					hashSet4.Add(item3);
					hashSet4.Add(item4);
					Vector4ListContainer vector4ListContainer2 = new Vector4ListContainer();
					vector4ListContainer2.List.Add(item6);
					nearbyVertexGroups.Add(vector4ListContainer2);
				}
				if (cancelCalculateNearbyVertexGroups)
				{
					nearbyVertexGroups = new List<Vector4ListContainer>();
					return;
				}
			}
			status = "Complete";
			Debug.Log("Created " + nearbyVertexGroups.Count + " nearby vertex pair groups");
		}

		public override List<Vector4ListContainer> GetNearbyVertexGroups()
		{
			return nearbyVertexGroups;
		}

		public override List<Color> GetColors()
		{
			return colors;
		}

		public override Matrix4x4 GetToWorldMatrix()
		{
			if (_ScalpProvider != null)
			{
				return _ScalpProvider.ToWorldMatrix;
			}
			return Matrix4x4.identity;
		}

		public override GpuBuffer<Matrix4x4> GetTransformsBuffer()
		{
			if (_ScalpProvider != null)
			{
				return _ScalpProvider.ToWorldMatricesBuffer;
			}
			return null;
		}

		public override GpuBuffer<Vector3> GetNormalsBuffer()
		{
			if (_ScalpProvider != null)
			{
				return _ScalpProvider.NormalsBuffer;
			}
			return null;
		}

		public override int[] GetHairRootToScalpMap()
		{
			return hairRootToScalpIndices;
		}

		private void Update()
		{
		}

		private void OnDrawGizmos()
		{
			if (DebugDraw && _ScalpProvider.Validate(false))
			{
				Bounds bounds = GetBounds();
				Gizmos.DrawWireCube(bounds.center, bounds.size);
			}
		}
	}
}
