using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DAZMergedMesh : DAZMesh
{
	public enum GraftMethod
	{
		Closest,
		Boundary,
		ClosestPoly,
		ClosestVertAndPoly
	}

	public enum GraftSymmetryAxis
	{
		X,
		Y,
		Z
	}

	[Serializable]
	protected class FreeVertGraftWeight
	{
		public int freeVert;

		public int graftVert;

		public float weight;

		public int graftPoly;

		public float graftVertToPolyRatio;
	}

	public DAZMesh targetMesh;

	public DAZMesh graftMesh;

	public DAZMesh graft2Mesh;

	[SerializeField]
	private bool hasGraft2;

	public bool staticMesh;

	public DAZMergedMesh copyGraftOptionsFromMesh;

	public GraftSymmetryAxis graftSymmetryAxis;

	public bool useGraftSymmetry;

	public float graftSymmetryDistance = 0.001f;

	public bool graftToCenterlineVerts;

	public GraftMethod graftMethod = GraftMethod.Boundary;

	private bool graftFactorsDirty;

	[SerializeField]
	private float _graftXFactor = 1f;

	[SerializeField]
	private float _graftYFactor = 1f;

	[SerializeField]
	private float _graftZFactor = 1f;

	public string[] graftMeshMorphNamesForGrafting;

	public float[] graftMeshMorphValuesForGrafting;

	public bool drawGraftMorphedMesh;

	[SerializeField]
	private float[] _graftWeights;

	[SerializeField]
	private float[] _graft2Weights;

	[SerializeField]
	private bool[] _graftIsFreeVert;

	[SerializeField]
	private bool[] _graft2IsFreeVert;

	[SerializeField]
	private FreeVertGraftWeight[] _freeVertGraftWeights;

	public float freeVertexDistance = 0.1f;

	public int numTargetBaseVertices;

	public int numGraftBaseVertices;

	public int numGraftUVVertices;

	public int numGraft2BaseVertices;

	public int numTargetUVVertices;

	public int startGraftVertIndex;

	public int startGraft2VertIndex;

	private Vector3[] _graftMovements;

	private Vector3[] _graft2Movements;

	private Vector3[] _graftMovements2;

	private Vector3[] _graft2Movements2;

	private Mesh _graftMorphedMesh;

	private Vector3[] _graftMorphedMeshVertices;

	protected bool isPlaying;

	protected Vector3[] _threadedMorphedUVVertices;

	protected Vector3[] _threadedVisibleMorphedUVVertices;

	protected Vector3[] _threadedMorphedBaseVertices;

	protected bool _threadedVerticesChangedThisFrame;

	protected bool _threadedVisibleNonPoseVerticesChangedThisFrame;

	protected bool _targetMeshVerticesChangedThisFrame;

	protected bool _targetMeshVisibleNonPoseVerticesChangedThisFrame;

	protected bool _targetMeshVisibleNonPoseVerticesChangedLastFrame;

	protected bool _graftMeshVerticesChangedThisFrame;

	protected bool _graftMeshVisibleNonPoseVerticesChangedThisFrame;

	protected bool _graftMeshVisibleNonPoseVerticesChangedLastFrame;

	protected bool _graft2MeshVerticesChangedThisFrame;

	protected bool _graft2MeshVisibleNonPoseVerticesChangedThisFrame;

	protected bool _graft2MeshVisibleNonPoseVerticesChangedLastFrame;

	private float avgxdiff1;

	private float avgydiff1;

	private float avgzdiff1;

	private float avgxdiff2;

	private float avgydiff2;

	private float avgzdiff2;

	public bool has2ndGraft => hasGraft2;

	public float graftXFactor
	{
		get
		{
			return _graftXFactor;
		}
		set
		{
			if (_graftXFactor != value)
			{
				_graftXFactor = value;
				graftFactorsDirty = true;
			}
		}
	}

	public float graftYFactor
	{
		get
		{
			return _graftYFactor;
		}
		set
		{
			if (_graftYFactor != value)
			{
				_graftYFactor = value;
				graftFactorsDirty = true;
			}
		}
	}

	public float graftZFactor
	{
		get
		{
			return _graftZFactor;
		}
		set
		{
			if (_graftZFactor != value)
			{
				_graftZFactor = value;
				graftFactorsDirty = true;
			}
		}
	}

	public Vector3[] threadedMorphedUVVertices => _threadedMorphedUVVertices;

	public void CopyGraftOptions()
	{
		if (copyGraftOptionsFromMesh != null)
		{
			graftMethod = copyGraftOptionsFromMesh.graftMethod;
			graftSymmetryAxis = copyGraftOptionsFromMesh.graftSymmetryAxis;
			useGraftSymmetry = copyGraftOptionsFromMesh.useGraftSymmetry;
			graftSymmetryDistance = copyGraftOptionsFromMesh.graftSymmetryDistance;
			graftToCenterlineVerts = copyGraftOptionsFromMesh.graftToCenterlineVerts;
			freeVertexDistance = copyGraftOptionsFromMesh.freeVertexDistance;
			graftXFactor = copyGraftOptionsFromMesh.graftXFactor;
			graftYFactor = copyGraftOptionsFromMesh.graftYFactor;
			graftZFactor = copyGraftOptionsFromMesh.graftZFactor;
			graftMeshMorphNamesForGrafting = new string[copyGraftOptionsFromMesh.graftMeshMorphNamesForGrafting.Length];
			for (int i = 0; i < copyGraftOptionsFromMesh.graftMeshMorphNamesForGrafting.Length; i++)
			{
				graftMeshMorphNamesForGrafting[i] = copyGraftOptionsFromMesh.graftMeshMorphNamesForGrafting[i];
			}
			graftMeshMorphValuesForGrafting = new float[copyGraftOptionsFromMesh.graftMeshMorphValuesForGrafting.Length];
			for (int j = 0; j < copyGraftOptionsFromMesh.graftMeshMorphValuesForGrafting.Length; j++)
			{
				graftMeshMorphValuesForGrafting[j] = copyGraftOptionsFromMesh.graftMeshMorphValuesForGrafting[j];
			}
		}
	}

	public override void DeriveMeshes()
	{
		base.DeriveMeshes();
		_threadedMorphedBaseVertices = (Vector3[])base.baseVertices.Clone();
		_threadedMorphedUVVertices = (Vector3[])base.UVVertices.Clone();
		_threadedVisibleMorphedUVVertices = (Vector3[])base.UVVertices.Clone();
		_graftMovements = new Vector3[numGraftBaseVertices];
		_graftMovements2 = new Vector3[numGraftBaseVertices];
		if (hasGraft2)
		{
			_graft2Movements = new Vector3[numGraft2BaseVertices];
			_graft2Movements2 = new Vector3[numGraft2BaseVertices];
		}
		UpdateVertices(true);
		if (graftMesh != null)
		{
			Vector3[] array = graftMesh.baseVertices;
			_graftMorphedMeshVertices = new Vector3[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				_graftMorphedMeshVertices[i] = array[i];
			}
			Mesh mesh = graftMesh.baseMesh;
			_graftMorphedMesh = new Mesh();
			RegisterAllocatedObject(_graftMorphedMesh);
			_graftMorphedMesh.vertices = _graftMorphedMeshVertices;
			_graftMorphedMesh.subMeshCount = graftMesh.numMaterials;
			for (int j = 0; j < graftMesh.numMaterials; j++)
			{
				_graftMorphedMesh.SetIndices(mesh.GetIndices(j), MeshTopology.Triangles, j);
			}
			_graftMorphedMesh.RecalculateBounds();
			_graftMorphedMesh.normals = mesh.normals;
		}
	}

	public void CalculateFreeVertGraftWeightsViaClosest()
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		DAZMeshGraftVertexPair[] vertexPairs = graftMesh.meshGraft.vertexPairs;
		foreach (DAZMeshGraftVertexPair dAZMeshGraftVertexPair in vertexPairs)
		{
			dictionary.Add(dAZMeshGraftVertexPair.vertexNum, dAZMeshGraftVertexPair.graftToVertexNum);
		}
		MeshPoly[] array = targetMesh.basePolyList;
		Vector3[] array2 = targetMesh.baseVertices;
		Vector3[] array3 = graftMesh.baseVertices;
		Vector3[] array4 = new Vector3[array3.Length];
		Vector3[] array5 = new Vector3[array.Length];
		List<int> list = new List<int>();
		int[] hiddenPolys = graftMesh.meshGraft.hiddenPolys;
		foreach (int num in hiddenPolys)
		{
			int[] vertices = array[num].vertices;
			foreach (int item in vertices)
			{
				list.Add(item);
			}
		}
		for (int l = 0; l < array3.Length; l++)
		{
			array4[l] = array3[l];
		}
		if (graftMeshMorphNamesForGrafting != null && graftMesh.morphBank != null)
		{
			if (graftMeshMorphNamesForGrafting.Length == graftMeshMorphValuesForGrafting.Length)
			{
				for (int m = 0; m < graftMeshMorphNamesForGrafting.Length; m++)
				{
					float num2 = graftMeshMorphValuesForGrafting[m];
					DAZMorph builtInMorph = graftMesh.morphBank.GetBuiltInMorph(graftMeshMorphNamesForGrafting[m]);
					if (builtInMorph != null && builtInMorph.deltas.Length > 0)
					{
						DAZMorphVertex[] deltas = builtInMorph.deltas;
						foreach (DAZMorphVertex dAZMorphVertex in deltas)
						{
							Vector3 vector = dAZMorphVertex.delta * num2;
							array4[dAZMorphVertex.vertex] += vector;
						}
					}
					else
					{
						Debug.LogError("Could not find graft morph " + graftMeshMorphNamesForGrafting[m]);
					}
				}
			}
			else
			{
				Debug.LogError("Graft mesh morph names and morph values are not same length");
			}
		}
		int[] hiddenPolys2 = graftMesh.meshGraft.hiddenPolys;
		foreach (int num4 in hiddenPolys2)
		{
			int[] vertices2 = array[num4].vertices;
			float num5 = 0f;
			float num6 = 0f;
			float num7 = 0f;
			int num8 = vertices2.Length;
			float num9 = 1f / (float)num8;
			for (int num10 = 0; num10 < num8; num10++)
			{
				int num11 = vertices2[num10];
				num5 += array2[num11].x * num9;
				num6 += array2[num11].y * num9;
				num7 += array2[num11].z * num9;
			}
			array5[num4].x = num5;
			array5[num4].y = num6;
			array5[num4].z = num7;
		}
		_freeVertGraftWeights = new FreeVertGraftWeight[numGraftBaseVertices];
		for (int num12 = 0; num12 < numGraftBaseVertices; num12++)
		{
			float num13 = 1000000f;
			float num14 = 1000000f;
			int graftVert = -1;
			int graftPoly = -1;
			int[] hiddenPolys3 = graftMesh.meshGraft.hiddenPolys;
			foreach (int num16 in hiddenPolys3)
			{
				Vector3 vector2 = array5[num16];
				float magnitude = (vector2 - array4[num12]).magnitude;
				if (magnitude < num14)
				{
					graftPoly = num16;
					num14 = magnitude;
				}
			}
			float num17 = 0f;
			switch (graftSymmetryAxis)
			{
			case GraftSymmetryAxis.X:
				num17 = Mathf.Abs(array4[num12].x);
				break;
			case GraftSymmetryAxis.Y:
				num17 = Mathf.Abs(array4[num12].y);
				break;
			case GraftSymmetryAxis.Z:
				num17 = Mathf.Abs(array4[num12].z);
				break;
			}
			if (useGraftSymmetry)
			{
				if (num17 < graftSymmetryDistance)
				{
					foreach (int item2 in list)
					{
						Vector3 vector3 = array2[item2];
						float num18 = 0f;
						switch (graftSymmetryAxis)
						{
						case GraftSymmetryAxis.X:
							num18 = Mathf.Abs(vector3.x);
							break;
						case GraftSymmetryAxis.Y:
							num18 = Mathf.Abs(vector3.y);
							break;
						case GraftSymmetryAxis.Z:
							num18 = Mathf.Abs(vector3.z);
							break;
						}
						if (num18 < graftSymmetryDistance)
						{
							float magnitude2 = (vector3 - array4[num12]).magnitude;
							if (magnitude2 < num13)
							{
								graftVert = item2;
								num13 = magnitude2;
							}
						}
					}
				}
				else
				{
					foreach (int item3 in list)
					{
						Vector3 vector4 = array2[item3];
						float num19 = 0f;
						switch (graftSymmetryAxis)
						{
						case GraftSymmetryAxis.X:
							num19 = Mathf.Abs(vector4.x);
							break;
						case GraftSymmetryAxis.Y:
							num19 = Mathf.Abs(vector4.y);
							break;
						case GraftSymmetryAxis.Z:
							num19 = Mathf.Abs(vector4.z);
							break;
						}
						if (graftToCenterlineVerts || (!graftToCenterlineVerts && num19 > graftSymmetryDistance))
						{
							float magnitude3 = (vector4 - array4[num12]).magnitude;
							if (magnitude3 < num13)
							{
								graftVert = item3;
								num13 = magnitude3;
							}
						}
					}
				}
			}
			else
			{
				foreach (int item4 in list)
				{
					Vector3 vector5 = array2[item4];
					float magnitude4 = (vector5 - array4[num12]).magnitude;
					if (magnitude4 < num13)
					{
						graftVert = item4;
						num13 = magnitude4;
					}
				}
			}
			FreeVertGraftWeight freeVertGraftWeight = new FreeVertGraftWeight();
			freeVertGraftWeight.freeVert = num12;
			if (freeVertexDistance == 0f)
			{
				freeVertGraftWeight.weight = 0f;
			}
			else
			{
				float weight = Mathf.Clamp01(1f - num13 / freeVertexDistance);
				freeVertGraftWeight.weight = weight;
			}
			freeVertGraftWeight.graftVert = graftVert;
			freeVertGraftWeight.graftPoly = graftPoly;
			if (useGraftSymmetry)
			{
				switch (graftSymmetryAxis)
				{
				case GraftSymmetryAxis.X:
					num17 = Mathf.Abs(array4[num12].x);
					break;
				case GraftSymmetryAxis.Y:
					num17 = Mathf.Abs(array4[num12].y);
					break;
				case GraftSymmetryAxis.Z:
					num17 = Mathf.Abs(array4[num12].z);
					break;
				}
				if (num17 < graftSymmetryDistance)
				{
					freeVertGraftWeight.graftVertToPolyRatio = 0f;
				}
				else
				{
					freeVertGraftWeight.graftVertToPolyRatio = num13 / (num13 + num14);
				}
			}
			else
			{
				freeVertGraftWeight.graftVertToPolyRatio = num13 / (num13 + num14);
			}
			_freeVertGraftWeights[num12] = freeVertGraftWeight;
		}
	}

	public void Merge()
	{
		base.meshData = null;
		meshSmooth = null;
		DAZMesh[] components = GetComponents<DAZMesh>();
		bool flag = false;
		hasGraft2 = false;
		DAZMesh[] array = components;
		foreach (DAZMesh dAZMesh in array)
		{
			if (dAZMesh.meshGraft == null || !(dAZMesh.graftTo != null) || !(dAZMesh != this))
			{
				continue;
			}
			if (flag)
			{
				hasGraft2 = true;
				graft2Mesh = dAZMesh;
				if (dAZMesh.graftTo != targetMesh)
				{
					Debug.LogError("2nd graft mesh " + dAZMesh.geometryId + " uses a different target mesh " + dAZMesh.graftTo.geometryId + " than 1st graft mesh " + targetMesh.geometryId);
					Debug.LogError("Merge aborted");
					return;
				}
				geometryId = geometryId + ":" + graft2Mesh.geometryId;
			}
			else
			{
				flag = true;
				graftMesh = dAZMesh;
				targetMesh = dAZMesh.graftTo;
				geometryId = targetMesh.geometryId + ":" + graftMesh.geometryId;
			}
		}
		if (!(targetMesh != null) || !(graftMesh != null))
		{
			return;
		}
		Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
		int[] hiddenPolys = graftMesh.meshGraft.hiddenPolys;
		foreach (int key in hiddenPolys)
		{
			dictionary.Add(key, true);
		}
		if (hasGraft2)
		{
			int[] hiddenPolys2 = graft2Mesh.meshGraft.hiddenPolys;
			foreach (int key2 in hiddenPolys2)
			{
				dictionary.Add(key2, true);
			}
		}
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		DAZMeshGraftVertexPair[] vertexPairs = graftMesh.meshGraft.vertexPairs;
		foreach (DAZMeshGraftVertexPair dAZMeshGraftVertexPair in vertexPairs)
		{
			dictionary2.Add(dAZMeshGraftVertexPair.vertexNum, dAZMeshGraftVertexPair.graftToVertexNum);
		}
		Dictionary<int, int> dictionary3 = new Dictionary<int, int>();
		if (hasGraft2)
		{
			DAZMeshGraftVertexPair[] vertexPairs2 = graft2Mesh.meshGraft.vertexPairs;
			foreach (DAZMeshGraftVertexPair dAZMeshGraftVertexPair2 in vertexPairs2)
			{
				dictionary3.Add(dAZMeshGraftVertexPair2.vertexNum, dAZMeshGraftVertexPair2.graftToVertexNum);
			}
		}
		Dictionary<int, int> dictionary4 = new Dictionary<int, int>();
		List<DAZVertexMap> list = new List<DAZVertexMap>();
		numTargetBaseVertices = targetMesh.numBaseVertices;
		numTargetUVVertices = targetMesh.numUVVertices;
		numGraftBaseVertices = graftMesh.numBaseVertices;
		if (hasGraft2)
		{
			numGraft2BaseVertices = graft2Mesh.numBaseVertices;
		}
		else
		{
			numGraft2BaseVertices = 0;
		}
		numGraftUVVertices = graftMesh.numUVVertices;
		startGraftVertIndex = numTargetBaseVertices;
		if (hasGraft2)
		{
			startGraft2VertIndex = numTargetBaseVertices + numGraftBaseVertices;
		}
		if (hasGraft2)
		{
			_numBaseVertices = numTargetBaseVertices + numGraftBaseVertices + numGraft2BaseVertices;
		}
		else
		{
			_numBaseVertices = numTargetBaseVertices + numGraftBaseVertices;
		}
		_baseVertices = new Vector3[_numBaseVertices];
		Vector3[] array2 = targetMesh.baseVertices;
		for (int n = 0; n < numTargetBaseVertices; n++)
		{
			_baseVertices[n] = array2[n];
		}
		Vector3[] array3 = graftMesh.baseVertices;
		DAZMeshGraftVertexPair[] vertexPairs3 = graftMesh.meshGraft.vertexPairs;
		int num = vertexPairs3.Length;
		_graftWeights = new float[numGraftBaseVertices * num];
		_graftIsFreeVert = new bool[numGraftBaseVertices];
		CalculateFreeVertGraftWeightsViaClosest();
		for (int num2 = 0; num2 < numGraftBaseVertices; num2++)
		{
			int value;
			if (dictionary2.TryGetValue(num2, out value))
			{
				_graftIsFreeVert[num2] = false;
				DAZVertexMap dAZVertexMap = new DAZVertexMap();
				dAZVertexMap.fromvert = value;
				dAZVertexMap.tovert = startGraftVertIndex + num2;
				list.Add(dAZVertexMap);
				dictionary4.Add(dAZVertexMap.tovert, dAZVertexMap.fromvert);
				_baseVertices[startGraftVertIndex + num2] = array2[value];
				continue;
			}
			_graftIsFreeVert[num2] = true;
			_baseVertices[startGraftVertIndex + num2] = array3[num2];
			float num3 = 0f;
			for (int num4 = 0; num4 < num; num4++)
			{
				int vertexNum = vertexPairs3[num4].vertexNum;
				float num5 = array3[num2].x - array3[vertexNum].x;
				float num6 = array3[num2].y - array3[vertexNum].y;
				float num7 = array3[num2].z - array3[vertexNum].z;
				float num8 = num5 * num5 + num6 * num6 + num7 * num7;
				float num9 = num8 * num8;
				num3 += num9;
				int num10 = num4 * numGraftBaseVertices + num2;
				_graftWeights[num10] = num9;
			}
			float num11 = 0f;
			for (int num12 = 0; num12 < num; num12++)
			{
				int num13 = num12 * numGraftBaseVertices + num2;
				float num14 = num3 / _graftWeights[num13];
				_graftWeights[num13] = num14;
				num11 += num14;
			}
			float num15 = 1f / num11;
			for (int num16 = 0; num16 < num; num16++)
			{
				int num17 = num16 * numGraftBaseVertices + num2;
				_graftWeights[num17] *= num15;
			}
		}
		if (hasGraft2)
		{
			Vector3[] array4 = graft2Mesh.baseVertices;
			DAZMeshGraftVertexPair[] vertexPairs4 = graft2Mesh.meshGraft.vertexPairs;
			int num18 = vertexPairs4.Length;
			_graft2Weights = new float[numGraft2BaseVertices * num18];
			_graft2IsFreeVert = new bool[numGraft2BaseVertices];
			for (int num19 = 0; num19 < numGraft2BaseVertices; num19++)
			{
				int value2;
				if (dictionary3.TryGetValue(num19, out value2))
				{
					_graft2IsFreeVert[num19] = false;
					DAZVertexMap dAZVertexMap2 = new DAZVertexMap();
					dAZVertexMap2.fromvert = value2;
					dAZVertexMap2.tovert = startGraft2VertIndex + num19;
					list.Add(dAZVertexMap2);
					dictionary4.Add(dAZVertexMap2.tovert, dAZVertexMap2.fromvert);
					_baseVertices[startGraft2VertIndex + num19] = array2[value2];
					continue;
				}
				_graft2IsFreeVert[num19] = true;
				_baseVertices[startGraft2VertIndex + num19] = array4[num19];
				float num20 = 0f;
				for (int num21 = 0; num21 < num18; num21++)
				{
					int vertexNum2 = vertexPairs4[num21].vertexNum;
					float num22 = array4[num19].x - array4[vertexNum2].x;
					float num23 = array4[num19].y - array4[vertexNum2].y;
					float num24 = array4[num19].z - array4[vertexNum2].z;
					float num25 = num22 * num22 + num23 * num23 + num24 * num24;
					float num26 = num25 * num25;
					num20 += num26;
					int num27 = num21 * numGraft2BaseVertices + num19;
					_graft2Weights[num27] = num26;
				}
				float num28 = 0f;
				for (int num29 = 0; num29 < num18; num29++)
				{
					int num30 = num29 * numGraft2BaseVertices + num19;
					float num31 = num20 / _graft2Weights[num30];
					_graft2Weights[num30] = num31;
					num28 += num31;
				}
				float num32 = 1f / num28;
				for (int num33 = 0; num33 < num18; num33++)
				{
					int num34 = num33 * numGraft2BaseVertices + num19;
					_graft2Weights[num34] *= num32;
				}
			}
		}
		int num35 = targetMesh.numMaterials;
		_numMaterials = num35 + graftMesh.numMaterials;
		if (hasGraft2)
		{
			_numMaterials += graft2Mesh.numMaterials;
		}
		_numMaterials++;
		materials = new Material[_numMaterials];
		materialsEnabled = new bool[_numMaterials];
		materialsShadowCastEnabled = new bool[_numMaterials];
		_materialNames = new string[_numMaterials];
		string[] array5 = targetMesh.materialNames;
		string[] array6 = graftMesh.materialNames;
		string[] array7 = null;
		if (hasGraft2)
		{
			array7 = graft2Mesh.materialNames;
		}
		for (int num36 = 0; num36 < targetMesh.numMaterials; num36++)
		{
			materials[num36] = targetMesh.materials[num36];
			materialsEnabled[num36] = true;
			materialsShadowCastEnabled[num36] = true;
			_materialNames[num36] = array5[num36];
		}
		int num37 = graftMesh.numMaterials;
		for (int num38 = 0; num38 < num37; num38++)
		{
			materials[num35 + num38] = graftMesh.materials[num38];
			materialsEnabled[num35 + num38] = true;
			materialsShadowCastEnabled[num35 + num38] = true;
			_materialNames[num35 + num38] = array6[num38];
		}
		if (hasGraft2)
		{
			for (int num39 = 0; num39 < graft2Mesh.numMaterials; num39++)
			{
				materials[num35 + num37 + num39] = graft2Mesh.materials[num39];
				materialsEnabled[num35 + num37 + num39] = true;
				materialsShadowCastEnabled[num35 + num37 + num39] = true;
				_materialNames[num35 + num37 + num39] = array7[num39];
			}
		}
		materialsEnabled[_numMaterials - 1] = true;
		materialsShadowCastEnabled[_numMaterials - 1] = true;
		_materialNames[_numMaterials - 1] = "Hidden";
		MeshPoly[] array8 = targetMesh.basePolyList;
		MeshPoly[] array9 = graftMesh.basePolyList;
		MeshPoly[] array10 = null;
		if (hasGraft2)
		{
			array10 = graft2Mesh.basePolyList;
		}
		int num40 = array8.Length;
		int num41 = array9.Length;
		int num42 = 0;
		if (hasGraft2)
		{
			num42 = array10.Length;
		}
		_numBasePolygons = num40 + num41;
		if (hasGraft2)
		{
			_numBasePolygons += num42;
		}
		_basePolyList = new MeshPoly[_numBasePolygons];
		int num43 = 0;
		for (int num44 = 0; num44 < num40; num44++)
		{
			bool value3;
			if (!dictionary.TryGetValue(num44, out value3))
			{
				MeshPoly meshPoly = array8[num44];
				_basePolyList[num43] = meshPoly;
				num43++;
			}
		}
		for (int num45 = 0; num45 < num41; num45++)
		{
			MeshPoly meshPoly2 = array9[num45];
			MeshPoly meshPoly3 = new MeshPoly();
			meshPoly3.materialNum = meshPoly2.materialNum + num35;
			meshPoly3.vertices = new int[meshPoly2.vertices.Length];
			for (int num46 = 0; num46 < meshPoly2.vertices.Length; num46++)
			{
				int value4;
				if (dictionary2.TryGetValue(meshPoly2.vertices[num46], out value4))
				{
					meshPoly3.vertices[num46] = value4;
				}
				else
				{
					meshPoly3.vertices[num46] = meshPoly2.vertices[num46] + startGraftVertIndex;
				}
			}
			_basePolyList[num43] = meshPoly3;
			num43++;
		}
		if (hasGraft2)
		{
			for (int num47 = 0; num47 < num42; num47++)
			{
				MeshPoly meshPoly4 = array10[num47];
				MeshPoly meshPoly5 = new MeshPoly();
				meshPoly5.materialNum = meshPoly4.materialNum + num35 + num37;
				meshPoly5.vertices = new int[meshPoly4.vertices.Length];
				for (int num48 = 0; num48 < meshPoly4.vertices.Length; num48++)
				{
					int value5;
					if (dictionary3.TryGetValue(meshPoly4.vertices[num48], out value5))
					{
						meshPoly5.vertices[num48] = value5;
					}
					else
					{
						meshPoly5.vertices[num48] = meshPoly4.vertices[num48] + startGraft2VertIndex;
					}
				}
				_basePolyList[num43] = meshPoly5;
				num43++;
			}
		}
		for (int num49 = 0; num49 < num40; num49++)
		{
			bool value6;
			if (dictionary.TryGetValue(num49, out value6))
			{
				MeshPoly meshPoly6 = array8[num49];
				MeshPoly meshPoly7 = new MeshPoly();
				meshPoly7.vertices = (int[])meshPoly6.vertices.Clone();
				meshPoly7.materialNum = _numMaterials - 1;
				_basePolyList[num43] = meshPoly7;
				num43++;
			}
		}
		_numUVVertices = numTargetUVVertices + numGraftUVVertices;
		if (hasGraft2)
		{
			_numUVVertices += graft2Mesh.numUVVertices;
		}
		_UV = new Vector2[_numUVVertices];
		_OrigUV = new Vector2[_numUVVertices];
		_UVVertices = new Vector3[_numUVVertices];
		_UVPolyList = new MeshPoly[_numBasePolygons];
		_morphedUVVertices = new Vector3[_numUVVertices];
		Vector2[] uV = targetMesh.UV;
		Vector2[] uV2 = graftMesh.UV;
		Vector2[] array11 = null;
		if (hasGraft2)
		{
			array11 = graft2Mesh.UV;
		}
		Vector3[] uVVertices = targetMesh.UVVertices;
		Vector3[] uVVertices2 = graftMesh.UVVertices;
		Vector3[] array12 = null;
		if (hasGraft2)
		{
			array12 = graft2Mesh.UVVertices;
		}
		Vector3[] array13 = targetMesh.morphedUVVertices;
		Vector3[] array14 = graftMesh.morphedUVVertices;
		Vector3[] array15 = null;
		if (hasGraft2)
		{
			array15 = graft2Mesh.morphedUVVertices;
		}
		DAZVertexMap[] array16 = targetMesh.baseVerticesToUVVertices;
		DAZVertexMap[] array17 = graftMesh.baseVerticesToUVVertices;
		DAZVertexMap[] array18 = null;
		if (hasGraft2)
		{
			array18 = graft2Mesh.baseVerticesToUVVertices;
		}
		int num50 = array16.Length;
		for (int num51 = 0; num51 < num50; num51++)
		{
			DAZVertexMap dAZVertexMap3 = array16[num51];
			DAZVertexMap dAZVertexMap4 = new DAZVertexMap();
			dAZVertexMap4.tovert = dAZVertexMap3.tovert + numGraftBaseVertices + numGraft2BaseVertices;
			int value7;
			if (dictionary4.TryGetValue(dAZVertexMap3.fromvert, out value7))
			{
				dAZVertexMap4.fromvert = value7;
			}
			else
			{
				dAZVertexMap4.fromvert = dAZVertexMap3.fromvert;
			}
			dictionary4.Add(dAZVertexMap4.tovert, dAZVertexMap4.fromvert);
			list.Add(dAZVertexMap4);
		}
		foreach (DAZVertexMap dAZVertexMap5 in array17)
		{
			DAZVertexMap dAZVertexMap6 = new DAZVertexMap();
			dAZVertexMap6.fromvert = dAZVertexMap5.fromvert + numTargetBaseVertices;
			dAZVertexMap6.tovert = dAZVertexMap5.tovert + numTargetUVVertices + numGraft2BaseVertices;
			int value8;
			if (dictionary4.TryGetValue(dAZVertexMap6.fromvert, out value8))
			{
				dAZVertexMap6.fromvert = value8;
			}
			dictionary4.Add(dAZVertexMap6.tovert, dAZVertexMap6.fromvert);
			list.Add(dAZVertexMap6);
		}
		if (hasGraft2)
		{
			foreach (DAZVertexMap dAZVertexMap7 in array18)
			{
				DAZVertexMap dAZVertexMap8 = new DAZVertexMap();
				dAZVertexMap8.fromvert = dAZVertexMap7.fromvert + numTargetBaseVertices + numGraftBaseVertices;
				dAZVertexMap8.tovert = dAZVertexMap7.tovert + numTargetUVVertices + numGraftUVVertices;
				int value9;
				if (dictionary4.TryGetValue(dAZVertexMap8.fromvert, out value9))
				{
					dAZVertexMap8.fromvert = value9;
				}
				dictionary4.Add(dAZVertexMap8.tovert, dAZVertexMap8.fromvert);
				list.Add(dAZVertexMap8);
			}
		}
		for (int num54 = 0; num54 < numTargetUVVertices; num54++)
		{
			int num55 = num54;
			if (num54 >= numTargetBaseVertices)
			{
				num55 += numGraftBaseVertices + numGraft2BaseVertices;
			}
			_OrigUV[num55] = uV[num54];
			_UV[num55] = uV[num54];
			_UVVertices[num55] = uVVertices[num54];
			_morphedUVVertices[num55] = array13[num54];
		}
		for (int num56 = 0; num56 < graftMesh.numUVVertices; num56++)
		{
			int num57 = num56 + numTargetBaseVertices;
			if (num56 >= numGraftBaseVertices)
			{
				num57 = num56 + numTargetUVVertices + numGraft2BaseVertices;
			}
			_OrigUV[num57] = uV2[num56];
			_UV[num57] = uV2[num56];
			_UVVertices[num57] = uVVertices2[num56];
			_morphedUVVertices[num57] = array14[num56];
		}
		if (hasGraft2)
		{
			for (int num58 = 0; num58 < graft2Mesh.numUVVertices; num58++)
			{
				int num59 = num58 + numTargetBaseVertices + numGraftBaseVertices;
				if (num58 >= numGraft2BaseVertices)
				{
					num59 = num58 + numTargetUVVertices + numGraftUVVertices;
				}
				_OrigUV[num59] = array11[num58];
				_UV[num59] = array11[num58];
				_UVVertices[num59] = array12[num58];
				_morphedUVVertices[num59] = array15[num58];
			}
		}
		MeshPoly[] uVPolyList = targetMesh.UVPolyList;
		MeshPoly[] uVPolyList2 = graftMesh.UVPolyList;
		MeshPoly[] array19 = null;
		if (hasGraft2)
		{
			array19 = graft2Mesh.UVPolyList;
		}
		num43 = 0;
		for (int num60 = 0; num60 < num40; num60++)
		{
			bool value10;
			if (dictionary.TryGetValue(num60, out value10))
			{
				continue;
			}
			MeshPoly meshPoly8 = uVPolyList[num60];
			MeshPoly meshPoly9 = new MeshPoly();
			meshPoly9.materialNum = meshPoly8.materialNum;
			meshPoly9.vertices = new int[meshPoly8.vertices.Length];
			for (int num61 = 0; num61 < meshPoly8.vertices.Length; num61++)
			{
				if (meshPoly8.vertices[num61] >= numTargetBaseVertices)
				{
					meshPoly9.vertices[num61] = meshPoly8.vertices[num61] + numGraftBaseVertices + numGraft2BaseVertices;
				}
				else
				{
					meshPoly9.vertices[num61] = meshPoly8.vertices[num61];
				}
			}
			_UVPolyList[num43] = meshPoly9;
			num43++;
		}
		for (int num62 = 0; num62 < num41; num62++)
		{
			MeshPoly meshPoly10 = uVPolyList2[num62];
			MeshPoly meshPoly11 = new MeshPoly();
			meshPoly11.materialNum = meshPoly10.materialNum + num35;
			meshPoly11.vertices = new int[meshPoly10.vertices.Length];
			for (int num63 = 0; num63 < meshPoly10.vertices.Length; num63++)
			{
				if (meshPoly10.vertices[num63] >= numGraftBaseVertices)
				{
					meshPoly11.vertices[num63] = meshPoly10.vertices[num63] + numTargetUVVertices + numGraft2BaseVertices;
				}
				else
				{
					meshPoly11.vertices[num63] = meshPoly10.vertices[num63] + numTargetBaseVertices;
				}
			}
			_UVPolyList[num43] = meshPoly11;
			num43++;
		}
		if (hasGraft2)
		{
			for (int num64 = 0; num64 < num42; num64++)
			{
				MeshPoly meshPoly12 = array19[num64];
				MeshPoly meshPoly13 = new MeshPoly();
				meshPoly13.materialNum = meshPoly12.materialNum + num35 + num37;
				meshPoly13.vertices = new int[meshPoly12.vertices.Length];
				for (int num65 = 0; num65 < meshPoly12.vertices.Length; num65++)
				{
					if (meshPoly12.vertices[num65] >= numGraft2BaseVertices)
					{
						meshPoly13.vertices[num65] = meshPoly12.vertices[num65] + numTargetUVVertices + numGraftUVVertices;
					}
					else
					{
						meshPoly13.vertices[num65] = meshPoly12.vertices[num65] + numTargetBaseVertices + numGraftBaseVertices;
					}
				}
				_UVPolyList[num43] = meshPoly13;
				num43++;
			}
		}
		for (int num66 = 0; num66 < num40; num66++)
		{
			bool value11;
			if (!dictionary.TryGetValue(num66, out value11))
			{
				continue;
			}
			MeshPoly meshPoly14 = uVPolyList[num66];
			MeshPoly meshPoly15 = new MeshPoly();
			meshPoly15.materialNum = _numMaterials - 1;
			meshPoly15.vertices = new int[meshPoly14.vertices.Length];
			for (int num67 = 0; num67 < meshPoly14.vertices.Length; num67++)
			{
				if (meshPoly14.vertices[num67] >= numTargetBaseVertices)
				{
					meshPoly15.vertices[num67] = meshPoly14.vertices[num67] + numGraftBaseVertices + numGraft2BaseVertices;
				}
				else
				{
					meshPoly15.vertices[num67] = meshPoly14.vertices[num67];
				}
			}
			_UVPolyList[num43] = meshPoly15;
			num43++;
		}
		_baseVerticesToUVVertices = list.ToArray();
		DeriveMeshes();
	}

	public new void RecalculateMorphedMeshTangentsAccurate()
	{
		base.RecalculateMorphedMeshTangentsAccurate();
		DAZMeshGraftVertexPair[] vertexPairs = graftMesh.meshGraft.vertexPairs;
		int num = vertexPairs.Length;
		for (int i = 0; i < num; i++)
		{
			int graftToVertexNum = vertexPairs[i].graftToVertexNum;
			int vertexNum = vertexPairs[i].vertexNum;
			_morphedUVTangents[graftToVertexNum] = _morphedUVTangents[startGraftVertIndex + vertexNum];
		}
	}

	public new void RecalculateMorphedMeshTangents(bool forceAll = false)
	{
		base.RecalculateMorphedMeshTangents(forceAll);
		DAZMeshGraftVertexPair[] vertexPairs = graftMesh.meshGraft.vertexPairs;
		int num = vertexPairs.Length;
		for (int i = 0; i < num; i++)
		{
			int graftToVertexNum = vertexPairs[i].graftToVertexNum;
			int vertexNum = vertexPairs[i].vertexNum;
			_morphedUVTangents[graftToVertexNum] = _morphedUVTangents[startGraftVertIndex + vertexNum];
		}
	}

	public void UpdateVertices(bool force = false)
	{
		UpdateVerticesPre();
		UpdateVerticesThreaded(force);
		UpdateVerticesPost(true);
	}

	public void UpdateVerticesPre(bool forceChange = false)
	{
		if (targetMesh != null)
		{
			_targetMeshVerticesChangedThisFrame = targetMesh.verticesChangedThisFrame || forceChange;
			_targetMeshVisibleNonPoseVerticesChangedThisFrame = targetMesh.visibleNonPoseVerticesChangedThisFrame;
			_targetMeshVisibleNonPoseVerticesChangedLastFrame = targetMesh.visibleNonPoseVerticesChangedLastFrame;
		}
		if (graftMesh != null)
		{
			_graftMeshVerticesChangedThisFrame = graftMesh.verticesChangedThisFrame || forceChange;
			_graftMeshVisibleNonPoseVerticesChangedThisFrame = graftMesh.visibleNonPoseVerticesChangedThisFrame;
			_graftMeshVisibleNonPoseVerticesChangedLastFrame = graftMesh.visibleNonPoseVerticesChangedLastFrame;
		}
		if (graft2Mesh != null)
		{
			_graft2MeshVerticesChangedThisFrame = graft2Mesh.verticesChangedThisFrame || forceChange;
			_graft2MeshVisibleNonPoseVerticesChangedThisFrame = graft2Mesh.visibleNonPoseVerticesChangedThisFrame;
			_graft2MeshVisibleNonPoseVerticesChangedLastFrame = graft2Mesh.visibleNonPoseVerticesChangedLastFrame;
		}
		_verticesChangedLastFrame = _verticesChangedThisFrame;
		_threadedVerticesChangedThisFrame = false;
		_verticesChangedThisFrame = false;
		_visibleNonPoseVerticesChangedLastFrame = _visibleNonPoseVerticesChangedThisFrame;
		_threadedVisibleNonPoseVerticesChangedThisFrame = false;
		_visibleNonPoseVerticesChangedThisFrame = false;
	}

	public void UpdateVerticesThreaded(bool force = false)
	{
		if (targetMesh == null || graftMesh == null)
		{
			return;
		}
		if (graftFactorsDirty)
		{
			force = true;
			graftFactorsDirty = false;
		}
		bool flag = false;
		Vector3[] array = targetMesh.morphedUVVertices;
		Vector3[] array2 = targetMesh.visibleMorphedUVVertices;
		Vector3[] array3 = targetMesh.morphedBaseVertices;
		int num = graftMesh.numUVVertices;
		if (_targetMeshVerticesChangedThisFrame || force)
		{
			Vector3[] array4 = targetMesh.morphedUVNormals;
			for (int i = 0; i < numTargetBaseVertices; i++)
			{
				_threadedMorphedUVVertices[i] = array[i];
				_threadedVisibleMorphedUVVertices[i] = array2[i];
				_threadedMorphedBaseVertices[i] = array3[i];
				if ((targetMesh.recalcNormalsOnMorph && targetMesh.normalsDirtyThisFrame) || force)
				{
					flag = true;
					_morphedUVNormals[i] = array4[i];
				}
			}
			if ((targetMesh.recalcTangentsOnMorph && targetMesh.tangentsDirtyThisFrame) || force)
			{
				Vector4[] array5 = targetMesh.morphedUVTangents;
				for (int j = 0; j < numTargetUVVertices; j++)
				{
					int num2 = j;
					if (j >= numTargetBaseVertices)
					{
						num2 += numGraftBaseVertices + numGraft2BaseVertices;
					}
					_morphedUVTangents[num2] = array5[j];
				}
			}
		}
		if ((graftMesh.normalsDirtyThisFrame && graftMesh.recalcNormalsOnMorph) || force)
		{
			flag = true;
			Vector3[] array6 = graftMesh.morphedUVNormals;
			for (int k = 0; k < numGraftBaseVertices; k++)
			{
				_morphedUVNormals[startGraftVertIndex + k] = array6[k];
			}
		}
		if ((graftMesh.tangentsDirtyThisFrame && graftMesh.recalcTangentsOnMorph) || force)
		{
			Vector4[] array7 = graftMesh.morphedUVTangents;
			for (int l = 0; l < graftMesh.numUVVertices; l++)
			{
				int num3 = ((l < numGraftBaseVertices) ? (l + startGraftVertIndex) : (l + numTargetUVVertices + numGraft2BaseVertices));
				_morphedUVTangents[num3] = array7[l];
			}
		}
		if (hasGraft2 && ((graft2Mesh.normalsDirtyThisFrame && graftMesh.recalcNormalsOnMorph) || force))
		{
			flag = true;
			Vector3[] array8 = graft2Mesh.morphedUVNormals;
			for (int m = 0; m < numGraft2BaseVertices; m++)
			{
				_morphedUVNormals[startGraft2VertIndex + m] = array8[m];
			}
		}
		if (hasGraft2 && ((graft2Mesh.tangentsDirtyThisFrame && graft2Mesh.recalcTangentsOnMorph) || force))
		{
			Vector4[] array9 = graft2Mesh.morphedUVTangents;
			for (int n = 0; n < graft2Mesh.numUVVertices; n++)
			{
				int num4 = ((n < numGraft2BaseVertices) ? (n + startGraft2VertIndex) : (n + numTargetUVVertices + num));
				_morphedUVTangents[num4] = array9[n];
			}
		}
		if (flag || force)
		{
			_updateDuplicateMorphedUVNormals();
		}
		bool flag2 = false;
		Vector3[] array10 = graftMesh.morphedUVVertices;
		Vector3[] array11 = graftMesh.visibleMorphedUVVertices;
		Vector3[] array12 = targetMesh.baseVertices;
		DAZMeshGraftVertexPair[] vertexPairs = graftMesh.meshGraft.vertexPairs;
		int num5 = vertexPairs.Length;
		float num6 = 0f;
		float num7 = 0f;
		float num8 = 0f;
		float num9 = 0f;
		float num10 = 0f;
		float num11 = 0f;
		if (_targetMeshVerticesChangedThisFrame || force)
		{
			for (int num12 = 0; num12 < num5; num12++)
			{
				int graftToVertexNum = vertexPairs[num12].graftToVertexNum;
				int vertexNum = vertexPairs[num12].vertexNum;
				int num13 = startGraftVertIndex + vertexNum;
				float num14 = array[graftToVertexNum].x - array12[graftToVertexNum].x;
				float num15 = array[graftToVertexNum].y - array12[graftToVertexNum].y;
				float num16 = array[graftToVertexNum].z - array12[graftToVertexNum].z;
				num6 += num14;
				num7 += num15;
				num8 += num16;
				if (_graftMovements[vertexNum].x != num14)
				{
					flag2 = true;
					_graftMovements[vertexNum].x = num14;
				}
				if (_graftMovements[vertexNum].y != num15)
				{
					flag2 = true;
					_graftMovements[vertexNum].y = num15;
				}
				if (_graftMovements[vertexNum].z != num16)
				{
					flag2 = true;
					_graftMovements[vertexNum].z = num16;
				}
				float num17 = array2[graftToVertexNum].x - array12[graftToVertexNum].x;
				float num18 = array2[graftToVertexNum].y - array12[graftToVertexNum].y;
				float num19 = array2[graftToVertexNum].z - array12[graftToVertexNum].z;
				num9 += num17;
				num10 += num18;
				num11 += num19;
				if (_graftMovements2[vertexNum].x != num17)
				{
					_graftMovements2[vertexNum].x = num17;
				}
				if (_graftMovements2[vertexNum].y != num18)
				{
					_graftMovements2[vertexNum].y = num18;
				}
				if (_graftMovements2[vertexNum].z != num19)
				{
					_graftMovements2[vertexNum].z = num19;
				}
				_threadedMorphedUVVertices[num13] = array[graftToVertexNum];
				_threadedMorphedBaseVertices[num13] = array3[graftToVertexNum];
				_threadedVisibleMorphedUVVertices[num13] = array2[graftToVertexNum];
			}
		}
		float num20 = num6 / (float)num5;
		float num21 = num7 / (float)num5;
		float num22 = num8 / (float)num5;
		float num23 = num9 / (float)num5;
		float num24 = num10 / (float)num5;
		float num25 = num11 / (float)num5;
		morphedNormalsDirty = false;
		_threadedVisibleNonPoseVerticesChangedThisFrame = _targetMeshVisibleNonPoseVerticesChangedThisFrame || _graftMeshVisibleNonPoseVerticesChangedThisFrame || _targetMeshVisibleNonPoseVerticesChangedLastFrame || _graftMeshVisibleNonPoseVerticesChangedLastFrame;
		if (_targetMeshVerticesChangedThisFrame || _graftMeshVerticesChangedThisFrame || force)
		{
			_threadedVerticesChangedThisFrame = true;
			MeshPoly[] array13 = targetMesh.basePolyList;
			switch (graftMethod)
			{
			case GraftMethod.Closest:
			{
				for (int num63 = 0; num63 < numGraftBaseVertices; num63++)
				{
					if (_graftIsFreeVert[num63])
					{
						int num64 = startGraftVertIndex + num63;
						FreeVertGraftWeight freeVertGraftWeight3 = _freeVertGraftWeights[num63];
						int graftVert2 = freeVertGraftWeight3.graftVert;
						float weight3 = freeVertGraftWeight3.weight;
						if (weight3 > 0f)
						{
							float num65 = 1f - weight3;
							float num66 = array[graftVert2].x - array12[graftVert2].x;
							float num67 = array[graftVert2].y - array12[graftVert2].y;
							float num68 = array[graftVert2].z - array12[graftVert2].z;
							_graftMovements[num63].x = num20 * num65 + num66 * weight3;
							_graftMovements[num63].y = num21 * num65 + num67 * weight3;
							_graftMovements[num63].z = num22 * num65 + num68 * weight3;
							float num69 = array2[graftVert2].x - array12[graftVert2].x;
							float num70 = array2[graftVert2].y - array12[graftVert2].y;
							float num71 = array2[graftVert2].z - array12[graftVert2].z;
							_graftMovements2[num63].x = num23 * num65 + num69 * weight3;
							_graftMovements2[num63].y = num24 * num65 + num70 * weight3;
							_graftMovements2[num63].z = num25 * num65 + num71 * weight3;
						}
						else
						{
							_graftMovements[num63].x = num20;
							_graftMovements[num63].y = num21;
							_graftMovements[num63].z = num22;
							_graftMovements2[num63].x = num23;
							_graftMovements2[num63].y = num24;
							_graftMovements2[num63].z = num25;
						}
						_threadedMorphedUVVertices[num64].x = array10[num63].x + _graftMovements[num63].x;
						_threadedMorphedUVVertices[num64].y = array10[num63].y + _graftMovements[num63].y;
						_threadedMorphedUVVertices[num64].z = array10[num63].z + _graftMovements[num63].z;
						_threadedMorphedBaseVertices[num64] = _morphedUVVertices[num64];
						_threadedVisibleMorphedUVVertices[num64].x = array10[num63].x + _graftMovements2[num63].x;
						_threadedVisibleMorphedUVVertices[num64].y = array10[num63].y + _graftMovements2[num63].y;
						_threadedVisibleMorphedUVVertices[num64].z = array10[num63].z + _graftMovements2[num63].z;
					}
				}
				break;
			}
			case GraftMethod.ClosestPoly:
			{
				for (int num36 = 0; num36 < numGraftBaseVertices; num36++)
				{
					if (!_graftIsFreeVert[num36])
					{
						continue;
					}
					int num37 = startGraftVertIndex + num36;
					FreeVertGraftWeight freeVertGraftWeight = _freeVertGraftWeights[num36];
					int graftPoly = freeVertGraftWeight.graftPoly;
					float weight = freeVertGraftWeight.weight;
					if (weight > 0f)
					{
						float num38 = 1f - weight;
						float num39 = 0f;
						float num40 = 0f;
						float num41 = 0f;
						float num42 = 0f;
						float num43 = 0f;
						float num44 = 0f;
						int[] vertices = array13[graftPoly].vertices;
						int num45 = vertices.Length;
						float num46 = 1f / (float)num45;
						for (int num47 = 0; num47 < num45; num47++)
						{
							int num48 = vertices[num47];
							num39 += (array[num48].x - array12[num48].x) * num46;
							num40 += (array[num48].y - array12[num48].y) * num46;
							num41 += (array[num48].z - array12[num48].z) * num46;
							num42 += (array2[num48].x - array12[num48].x) * num46;
							num43 += (array2[num48].y - array12[num48].y) * num46;
							num44 += (array2[num48].z - array12[num48].z) * num46;
						}
						_graftMovements[num36].x = num20 * num38 + num39 * weight;
						_graftMovements[num36].y = num21 * num38 + num40 * weight;
						_graftMovements[num36].z = num22 * num38 + num41 * weight;
						_graftMovements2[num36].x = num23 * num38 + num42 * weight;
						_graftMovements2[num36].y = num24 * num38 + num43 * weight;
						_graftMovements2[num36].z = num25 * num38 + num44 * weight;
					}
					else
					{
						_graftMovements[num36].x = num20;
						_graftMovements[num36].y = num21;
						_graftMovements[num36].z = num22;
						_graftMovements2[num36].x = num23;
						_graftMovements2[num36].y = num24;
						_graftMovements2[num36].z = num25;
					}
					_threadedMorphedUVVertices[num37].x = array10[num36].x + _graftMovements[num36].x;
					_threadedMorphedUVVertices[num37].y = array10[num36].y + _graftMovements[num36].y;
					_threadedMorphedUVVertices[num37].z = array10[num36].z + _graftMovements[num36].z;
					_threadedMorphedBaseVertices[num37] = _morphedUVVertices[num37];
					_threadedVisibleMorphedUVVertices[num37].x = array10[num36].x + _graftMovements2[num36].x;
					_threadedVisibleMorphedUVVertices[num37].y = array10[num36].y + _graftMovements2[num36].y;
					_threadedVisibleMorphedUVVertices[num37].z = array10[num36].z + _graftMovements2[num36].z;
				}
				break;
			}
			case GraftMethod.ClosestVertAndPoly:
			{
				for (int num49 = 0; num49 < numGraftBaseVertices; num49++)
				{
					if (!_graftIsFreeVert[num49])
					{
						continue;
					}
					int num50 = startGraftVertIndex + num49;
					FreeVertGraftWeight freeVertGraftWeight2 = _freeVertGraftWeights[num49];
					int graftPoly2 = freeVertGraftWeight2.graftPoly;
					int graftVert = freeVertGraftWeight2.graftVert;
					float weight2 = freeVertGraftWeight2.weight;
					if (weight2 > 0f)
					{
						float num51 = 1f - weight2;
						float num52 = 1f - freeVertGraftWeight2.graftVertToPolyRatio;
						float num53 = (array[graftVert].x - array12[graftVert].x) * num52;
						float num54 = (array[graftVert].y - array12[graftVert].y) * num52;
						float num55 = (array[graftVert].z - array12[graftVert].z) * num52;
						float num56 = (array2[graftVert].x - array12[graftVert].x) * num52;
						float num57 = (array2[graftVert].y - array12[graftVert].y) * num52;
						float num58 = (array2[graftVert].z - array12[graftVert].z) * num52;
						if (freeVertGraftWeight2.graftVertToPolyRatio > 0f && graftPoly2 != -1)
						{
							int[] vertices2 = array13[graftPoly2].vertices;
							int num59 = vertices2.Length;
							float graftVertToPolyRatio = freeVertGraftWeight2.graftVertToPolyRatio;
							float num60 = 1f / (float)num59 * graftVertToPolyRatio;
							for (int num61 = 0; num61 < num59; num61++)
							{
								int num62 = vertices2[num61];
								num53 += (array[num62].x - array12[num62].x) * num60;
								num54 += (array[num62].y - array12[num62].y) * num60;
								num55 += (array[num62].z - array12[num62].z) * num60;
								num56 += (array2[num62].x - array12[num62].x) * num60;
								num57 += (array2[num62].y - array12[num62].y) * num60;
								num58 += (array2[num62].z - array12[num62].z) * num60;
							}
						}
						_graftMovements[num49].x = num20 * num51 + num53 * weight2;
						_graftMovements[num49].y = num21 * num51 + num54 * weight2;
						_graftMovements[num49].z = num22 * num51 + num55 * weight2;
						_graftMovements2[num49].x = num23 * num51 + num56 * weight2;
						_graftMovements2[num49].y = num24 * num51 + num57 * weight2;
						_graftMovements2[num49].z = num25 * num51 + num58 * weight2;
					}
					else
					{
						_graftMovements[num49].x = num20;
						_graftMovements[num49].y = num21;
						_graftMovements[num49].z = num22;
						_graftMovements2[num49].x = num23;
						_graftMovements2[num49].y = num24;
						_graftMovements2[num49].z = num25;
					}
					_threadedMorphedUVVertices[num50].x = array10[num49].x + _graftMovements[num49].x;
					_threadedMorphedUVVertices[num50].y = array10[num49].y + _graftMovements[num49].y;
					_threadedMorphedUVVertices[num50].z = array10[num49].z + _graftMovements[num49].z;
					_threadedMorphedBaseVertices[num50] = _morphedUVVertices[num50];
					_threadedVisibleMorphedUVVertices[num50].x = array10[num49].x + _graftMovements2[num49].x;
					_threadedVisibleMorphedUVVertices[num50].y = array10[num49].y + _graftMovements2[num49].y;
					_threadedVisibleMorphedUVVertices[num50].z = array10[num49].z + _graftMovements2[num49].z;
				}
				break;
			}
			case GraftMethod.Boundary:
			{
				Vector3 vector = default(Vector3);
				Vector3 vector2 = default(Vector3);
				for (int num26 = 0; num26 < numGraftBaseVertices; num26++)
				{
					if (!_graftIsFreeVert[num26])
					{
						continue;
					}
					int num27 = startGraftVertIndex + num26;
					if (flag2 || force)
					{
						vector.x = 0f;
						vector.y = 0f;
						vector.z = 0f;
						vector2.x = 0f;
						vector2.y = 0f;
						vector2.z = 0f;
						float num28 = _graftXFactor;
						float num29 = _graftYFactor;
						float num30 = _graftZFactor;
						if (useGraftSymmetry)
						{
							switch (graftSymmetryAxis)
							{
							case GraftSymmetryAxis.X:
							{
								float num31 = Mathf.Abs(array10[num26].x);
								float num32 = Mathf.Clamp01(num31 / graftSymmetryDistance);
								num28 *= num32;
								break;
							}
							case GraftSymmetryAxis.Y:
							{
								float num31 = Mathf.Abs(array10[num26].y);
								float num32 = Mathf.Clamp01(num31 / graftSymmetryDistance);
								num29 *= num32;
								break;
							}
							case GraftSymmetryAxis.Z:
							{
								float num31 = Mathf.Abs(array10[num26].z);
								float num32 = Mathf.Clamp01(num31 / graftSymmetryDistance);
								num30 *= num32;
								break;
							}
							}
						}
						for (int num33 = 0; num33 < num5; num33++)
						{
							int vertexNum2 = vertexPairs[num33].vertexNum;
							int num34 = num33 * numGraftBaseVertices + num26;
							float num35 = _graftWeights[num34];
							vector.x += _graftMovements[vertexNum2].x * num35 * num28;
							vector.y += _graftMovements[vertexNum2].y * num35 * num29;
							vector.z += _graftMovements[vertexNum2].z * num35 * num30;
							vector2.x += _graftMovements2[vertexNum2].x * num35 * num28;
							vector2.y += _graftMovements2[vertexNum2].y * num35 * num29;
							vector2.z += _graftMovements2[vertexNum2].z * num35 * num30;
						}
						_graftMovements[num26] = vector;
						_graftMovements2[num26] = vector2;
					}
					_threadedMorphedUVVertices[num27].x = array10[num26].x + _graftMovements[num26].x;
					_threadedMorphedUVVertices[num27].y = array10[num26].y + _graftMovements[num26].y;
					_threadedMorphedUVVertices[num27].z = array10[num26].z + _graftMovements[num26].z;
					_threadedMorphedBaseVertices[num27] = _morphedUVVertices[num27];
					_threadedVisibleMorphedUVVertices[num27].x = array10[num26].x + _graftMovements2[num26].x;
					_threadedVisibleMorphedUVVertices[num27].y = array10[num26].y + _graftMovements2[num26].y;
					_threadedVisibleMorphedUVVertices[num27].z = array10[num26].z + _graftMovements2[num26].z;
				}
				break;
			}
			}
			if ((flag2 || force) && recalcNormalsOnMorph)
			{
				for (int num72 = 0; num72 < numGraftBaseVertices; num72++)
				{
					base.morphedBaseDirtyVertices[startGraftVertIndex + num72] = true;
				}
				for (int num73 = 0; num73 < num5; num73++)
				{
					int graftToVertexNum2 = vertexPairs[num73].graftToVertexNum;
					base.morphedBaseDirtyVertices[graftToVertexNum2] = true;
				}
				morphedNormalsDirty = true;
			}
			if ((flag2 || force) && recalcTangentsOnMorph)
			{
				for (int num74 = 0; num74 < numGraftBaseVertices; num74++)
				{
					base.morphedUVDirtyVertices[startGraftVertIndex + num74] = true;
				}
				for (int num75 = 0; num75 < num5; num75++)
				{
					int graftToVertexNum3 = vertexPairs[num75].graftToVertexNum;
					base.morphedUVDirtyVertices[graftToVertexNum3] = true;
				}
				morphedTangentsDirty = true;
			}
		}
		if (hasGraft2)
		{
			Vector3[] array14 = graft2Mesh.morphedUVVertices;
			Vector3[] array15 = graft2Mesh.visibleMorphedUVVertices;
			DAZMeshGraftVertexPair[] vertexPairs2 = graft2Mesh.meshGraft.vertexPairs;
			int num76 = vertexPairs2.Length;
			bool flag3 = false;
			if (_targetMeshVerticesChangedThisFrame || force)
			{
				for (int num77 = 0; num77 < num76; num77++)
				{
					int graftToVertexNum4 = vertexPairs2[num77].graftToVertexNum;
					int vertexNum3 = vertexPairs2[num77].vertexNum;
					int num78 = startGraft2VertIndex + vertexNum3;
					float num79 = array[graftToVertexNum4].x - array12[graftToVertexNum4].x;
					float num80 = array[graftToVertexNum4].y - array12[graftToVertexNum4].y;
					float num81 = array[graftToVertexNum4].z - array12[graftToVertexNum4].z;
					float num82 = array2[graftToVertexNum4].x - array12[graftToVertexNum4].x;
					float num83 = array2[graftToVertexNum4].y - array12[graftToVertexNum4].y;
					float num84 = array2[graftToVertexNum4].z - array12[graftToVertexNum4].z;
					if (_graft2Movements[vertexNum3].x != num79)
					{
						flag3 = true;
						_graft2Movements[vertexNum3].x = num79;
					}
					if (_graft2Movements[vertexNum3].y != num80)
					{
						flag3 = true;
						_graft2Movements[vertexNum3].y = num80;
					}
					if (_graft2Movements[vertexNum3].z != num81)
					{
						flag3 = true;
						_graft2Movements[vertexNum3].z = num81;
					}
					if (_graft2Movements2[vertexNum3].x != num82)
					{
						_graft2Movements2[vertexNum3].x = num82;
					}
					if (_graft2Movements2[vertexNum3].y != num83)
					{
						_graft2Movements2[vertexNum3].y = num83;
					}
					if (_graft2Movements2[vertexNum3].z != num84)
					{
						_graft2Movements2[vertexNum3].z = num84;
					}
					_threadedMorphedUVVertices[num78] = array[graftToVertexNum4];
					_threadedMorphedBaseVertices[num78] = array[graftToVertexNum4];
					_threadedVisibleMorphedUVVertices[num78] = array2[graftToVertexNum4];
				}
			}
			if (_targetMeshVerticesChangedThisFrame || _graft2MeshVerticesChangedThisFrame || force)
			{
				_threadedVerticesChangedThisFrame = true;
				_threadedVisibleNonPoseVerticesChangedThisFrame = _visibleNonPoseVerticesChangedThisFrame || _graft2MeshVisibleNonPoseVerticesChangedThisFrame;
				Vector3 vector3 = default(Vector3);
				Vector3 vector4 = default(Vector3);
				for (int num85 = 0; num85 < numGraft2BaseVertices; num85++)
				{
					if (!_graft2IsFreeVert[num85])
					{
						continue;
					}
					int num86 = startGraft2VertIndex + num85;
					if (flag3 || force)
					{
						vector3.x = 0f;
						vector3.y = 0f;
						vector3.z = 0f;
						vector4.x = 0f;
						vector4.y = 0f;
						vector4.z = 0f;
						for (int num87 = 0; num87 < num76; num87++)
						{
							int vertexNum4 = vertexPairs2[num87].vertexNum;
							int num88 = num87 * numGraft2BaseVertices + num85;
							float num89 = _graft2Weights[num88];
							vector3.x += _graft2Movements[vertexNum4].x * num89 * _graftXFactor;
							vector3.y += _graft2Movements[vertexNum4].y * num89 * _graftYFactor;
							vector3.z += _graft2Movements[vertexNum4].z * num89 * _graftZFactor;
							vector4.x += _graft2Movements2[vertexNum4].x * num89 * _graftXFactor;
							vector4.y += _graft2Movements2[vertexNum4].y * num89 * _graftYFactor;
							vector4.z += _graft2Movements2[vertexNum4].z * num89 * _graftZFactor;
						}
						_graft2Movements[num85] = vector3;
						_graft2Movements2[num85] = vector4;
					}
					_threadedMorphedUVVertices[num86].x = array14[num85].x + _graft2Movements[num85].x;
					_threadedMorphedUVVertices[num86].y = array14[num85].y + _graft2Movements[num85].y;
					_threadedMorphedUVVertices[num86].z = array14[num85].z + _graft2Movements[num85].z;
					_threadedMorphedBaseVertices[num86] = _morphedUVVertices[num86];
					_threadedVisibleMorphedUVVertices[num86].x = array15[num85].x + _graft2Movements2[num85].x;
					_threadedVisibleMorphedUVVertices[num86].y = array15[num85].y + _graft2Movements2[num85].y;
					_threadedVisibleMorphedUVVertices[num86].z = array15[num85].z + _graft2Movements2[num85].z;
				}
			}
			if ((flag3 || force) && recalcNormalsOnMorph)
			{
				for (int num90 = 0; num90 < numGraft2BaseVertices; num90++)
				{
					base.morphedBaseDirtyVertices[startGraft2VertIndex + num90] = true;
				}
				for (int num91 = 0; num91 < num76; num91++)
				{
					int graftToVertexNum5 = vertexPairs2[num91].graftToVertexNum;
					base.morphedBaseDirtyVertices[graftToVertexNum5] = true;
				}
				morphedNormalsDirty = true;
			}
			if ((flag3 || force) && recalcTangentsOnMorph)
			{
				for (int num92 = 0; num92 < numGraft2BaseVertices; num92++)
				{
					base.morphedUVDirtyVertices[startGraft2VertIndex + num92] = true;
				}
				for (int num93 = 0; num93 < num76; num93++)
				{
					int graftToVertexNum6 = vertexPairs2[num93].graftToVertexNum;
					base.morphedUVDirtyVertices[graftToVertexNum6] = true;
				}
				morphedTangentsDirty = true;
			}
		}
		_triggerNormalAndTangentRecalc();
	}

	public void UpdateVerticesPrepThreadedFast(Vector3[] mesh1MorphedVerts, Vector3[] mergedUVVerts, bool useAltMovementArray = false)
	{
		Vector3[] array = ((!useAltMovementArray) ? _graftMovements : _graftMovements2);
		Vector3[] array2 = targetMesh.baseVertices;
		for (int i = 0; i < array2.Length; i++)
		{
			mergedUVVerts[i] = mesh1MorphedVerts[i];
		}
		DAZMeshGraftVertexPair[] vertexPairs = graftMesh.meshGraft.vertexPairs;
		int num = vertexPairs.Length;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		for (int j = 0; j < num; j++)
		{
			int graftToVertexNum = vertexPairs[j].graftToVertexNum;
			int vertexNum = vertexPairs[j].vertexNum;
			float num5 = mesh1MorphedVerts[graftToVertexNum].x - array2[graftToVertexNum].x;
			float num6 = mesh1MorphedVerts[graftToVertexNum].y - array2[graftToVertexNum].y;
			float num7 = mesh1MorphedVerts[graftToVertexNum].z - array2[graftToVertexNum].z;
			num2 += num5;
			num3 += num6;
			num4 += num7;
			array[vertexNum].x = num5;
			array[vertexNum].y = num6;
			array[vertexNum].z = num7;
		}
		if (useAltMovementArray)
		{
			avgxdiff2 = num2 / (float)num;
			avgydiff2 = num3 / (float)num;
			avgzdiff2 = num4 / (float)num;
		}
		else
		{
			avgxdiff1 = num2 / (float)num;
			avgydiff1 = num3 / (float)num;
			avgzdiff1 = num4 / (float)num;
		}
	}

	public void UpdateVerticesThreadedFast(Vector3[] mesh1MorphedVerts, Vector3[] mesh2MorphedVerts, Vector3[] mesh3MorphedVerts, Vector3[] mergedUVVerts, int mini, int maxi, bool useAltMovementArray = false)
	{
		Vector3[] array;
		float num;
		float num2;
		float num3;
		if (useAltMovementArray)
		{
			array = _graftMovements2;
			num = avgxdiff2;
			num2 = avgydiff2;
			num3 = avgzdiff2;
		}
		else
		{
			array = _graftMovements;
			num = avgxdiff1;
			num2 = avgydiff1;
			num3 = avgzdiff1;
		}
		Vector3[] array2 = targetMesh.baseVertices;
		DAZMeshGraftVertexPair[] vertexPairs = graftMesh.meshGraft.vertexPairs;
		int num4 = vertexPairs.Length;
		MeshPoly[] array3 = targetMesh.basePolyList;
		switch (graftMethod)
		{
		case GraftMethod.Closest:
		{
			for (int num30 = mini; num30 < maxi; num30++)
			{
				if (_graftIsFreeVert[num30])
				{
					int num31 = startGraftVertIndex + num30;
					FreeVertGraftWeight freeVertGraftWeight3 = _freeVertGraftWeights[num30];
					int graftVert2 = freeVertGraftWeight3.graftVert;
					float weight3 = freeVertGraftWeight3.weight;
					if (weight3 > 0f)
					{
						float num32 = 1f - weight3;
						float num33 = mesh1MorphedVerts[graftVert2].x - array2[graftVert2].x;
						float num34 = mesh1MorphedVerts[graftVert2].y - array2[graftVert2].y;
						float num35 = mesh1MorphedVerts[graftVert2].z - array2[graftVert2].z;
						array[num30].x = num * num32 + num33 * weight3;
						array[num30].y = num2 * num32 + num34 * weight3;
						array[num30].z = num3 * num32 + num35 * weight3;
					}
					else
					{
						array[num30].x = num;
						array[num30].y = num2;
						array[num30].z = num3;
					}
					mergedUVVerts[num31].x = mesh2MorphedVerts[num30].x + array[num30].x;
					mergedUVVerts[num31].y = mesh2MorphedVerts[num30].y + array[num30].y;
					mergedUVVerts[num31].z = mesh2MorphedVerts[num30].z + array[num30].z;
				}
			}
			break;
		}
		case GraftMethod.ClosestPoly:
		{
			for (int k = mini; k < maxi; k++)
			{
				if (!_graftIsFreeVert[k])
				{
					continue;
				}
				int num13 = startGraftVertIndex + k;
				FreeVertGraftWeight freeVertGraftWeight = _freeVertGraftWeights[k];
				int graftPoly = freeVertGraftWeight.graftPoly;
				float weight = freeVertGraftWeight.weight;
				if (weight > 0f)
				{
					float num14 = 1f - weight;
					float num15 = 0f;
					float num16 = 0f;
					float num17 = 0f;
					int[] vertices = array3[graftPoly].vertices;
					int num18 = vertices.Length;
					float num19 = 1f / (float)num18;
					for (int l = 0; l < num18; l++)
					{
						int num20 = vertices[l];
						num15 += (mesh1MorphedVerts[num20].x - array2[num20].x) * num19;
						num16 += (mesh1MorphedVerts[num20].y - array2[num20].y) * num19;
						num17 += (mesh1MorphedVerts[num20].z - array2[num20].z) * num19;
					}
					array[k].x = num * num14 + num15 * weight;
					array[k].y = num2 * num14 + num16 * weight;
					array[k].z = num3 * num14 + num17 * weight;
				}
				else
				{
					array[k].x = num;
					array[k].y = num2;
					array[k].z = num3;
				}
				mergedUVVerts[num13].x = mesh2MorphedVerts[k].x + array[k].x;
				mergedUVVerts[num13].y = mesh2MorphedVerts[k].y + array[k].y;
				mergedUVVerts[num13].z = mesh2MorphedVerts[k].z + array[k].z;
			}
			break;
		}
		case GraftMethod.ClosestVertAndPoly:
		{
			for (int m = mini; m < maxi; m++)
			{
				if (!_graftIsFreeVert[m])
				{
					continue;
				}
				int num21 = startGraftVertIndex + m;
				FreeVertGraftWeight freeVertGraftWeight2 = _freeVertGraftWeights[m];
				int graftPoly2 = freeVertGraftWeight2.graftPoly;
				int graftVert = freeVertGraftWeight2.graftVert;
				float weight2 = freeVertGraftWeight2.weight;
				if (weight2 > 0f)
				{
					float num22 = 1f - weight2;
					float num23 = 1f - freeVertGraftWeight2.graftVertToPolyRatio;
					float num24 = (mesh1MorphedVerts[graftVert].x - array2[graftVert].x) * num23;
					float num25 = (mesh1MorphedVerts[graftVert].y - array2[graftVert].y) * num23;
					float num26 = (mesh1MorphedVerts[graftVert].z - array2[graftVert].z) * num23;
					if (freeVertGraftWeight2.graftVertToPolyRatio > 0f && graftPoly2 != -1)
					{
						int[] vertices2 = array3[graftPoly2].vertices;
						int num27 = vertices2.Length;
						float graftVertToPolyRatio = freeVertGraftWeight2.graftVertToPolyRatio;
						float num28 = 1f / (float)num27 * graftVertToPolyRatio;
						for (int n = 0; n < num27; n++)
						{
							int num29 = vertices2[n];
							num24 += (mesh1MorphedVerts[num29].x - array2[num29].x) * num28;
							num25 += (mesh1MorphedVerts[num29].y - array2[num29].y) * num28;
							num26 += (mesh1MorphedVerts[num29].z - array2[num29].z) * num28;
						}
					}
					array[m].x = num * num22 + num24 * weight2;
					array[m].y = num2 * num22 + num25 * weight2;
					array[m].z = num3 * num22 + num26 * weight2;
				}
				else
				{
					array[m].x = num;
					array[m].y = num2;
					array[m].z = num3;
				}
				mergedUVVerts[num21].x = mesh2MorphedVerts[m].x + array[m].x;
				mergedUVVerts[num21].y = mesh2MorphedVerts[m].y + array[m].y;
				mergedUVVerts[num21].z = mesh2MorphedVerts[m].z + array[m].z;
			}
			break;
		}
		case GraftMethod.Boundary:
		{
			Vector3 vector = default(Vector3);
			for (int i = mini; i < maxi; i++)
			{
				if (!_graftIsFreeVert[i])
				{
					continue;
				}
				int num5 = startGraftVertIndex + i;
				vector.x = 0f;
				vector.y = 0f;
				vector.z = 0f;
				float num6 = _graftXFactor;
				float num7 = _graftYFactor;
				float num8 = _graftZFactor;
				if (useGraftSymmetry)
				{
					switch (graftSymmetryAxis)
					{
					case GraftSymmetryAxis.X:
					{
						float num9 = Mathf.Abs(mesh2MorphedVerts[i].x);
						float num10 = Mathf.Clamp01(num9 / graftSymmetryDistance);
						num6 *= num10;
						break;
					}
					case GraftSymmetryAxis.Y:
					{
						float num9 = Mathf.Abs(mesh2MorphedVerts[i].y);
						float num10 = Mathf.Clamp01(num9 / graftSymmetryDistance);
						num7 *= num10;
						break;
					}
					case GraftSymmetryAxis.Z:
					{
						float num9 = Mathf.Abs(mesh2MorphedVerts[i].z);
						float num10 = Mathf.Clamp01(num9 / graftSymmetryDistance);
						num8 *= num10;
						break;
					}
					}
				}
				for (int j = 0; j < num4; j++)
				{
					int vertexNum = vertexPairs[j].vertexNum;
					int num11 = j * numGraftBaseVertices + i;
					float num12 = _graftWeights[num11];
					vector.x += array[vertexNum].x * num12 * num6;
					vector.y += array[vertexNum].y * num12 * num7;
					vector.z += array[vertexNum].z * num12 * num8;
				}
				array[i] = vector;
				mergedUVVerts[num5].x = mesh2MorphedVerts[i].x + array[i].x;
				mergedUVVerts[num5].y = mesh2MorphedVerts[i].y + array[i].y;
				mergedUVVerts[num5].z = mesh2MorphedVerts[i].z + array[i].z;
			}
			break;
		}
		}
	}

	public void UpdateVerticesFinishThreadedFast(Vector3[] mesh1MorphedVerts, Vector3[] mesh2MorphedVerts, Vector3[] mesh3MorphedVerts, Vector3[] mergedUVVerts, bool useAltMovementArray = false)
	{
		Vector3[] array = ((!hasGraft2) ? null : ((!useAltMovementArray) ? _graft2Movements : _graft2Movements2));
		Vector3[] array2 = targetMesh.baseVertices;
		if (hasGraft2)
		{
			DAZMeshGraftVertexPair[] vertexPairs = graft2Mesh.meshGraft.vertexPairs;
			int num = vertexPairs.Length;
			for (int i = 0; i < num; i++)
			{
				int graftToVertexNum = vertexPairs[i].graftToVertexNum;
				int vertexNum = vertexPairs[i].vertexNum;
				float x = mesh1MorphedVerts[graftToVertexNum].x - array2[graftToVertexNum].x;
				float y = mesh1MorphedVerts[graftToVertexNum].y - array2[graftToVertexNum].y;
				float z = mesh1MorphedVerts[graftToVertexNum].z - array2[graftToVertexNum].z;
				array[vertexNum].x = x;
				array[vertexNum].y = y;
				array[vertexNum].z = z;
			}
			Vector3 vector = default(Vector3);
			for (int j = 0; j < numGraft2BaseVertices; j++)
			{
				if (!_graft2IsFreeVert[j])
				{
					continue;
				}
				int num2 = startGraft2VertIndex + j;
				vector.x = 0f;
				vector.y = 0f;
				vector.z = 0f;
				float num3 = _graftXFactor;
				float num4 = _graftYFactor;
				float num5 = _graftZFactor;
				if (useGraftSymmetry)
				{
					switch (graftSymmetryAxis)
					{
					case GraftSymmetryAxis.X:
					{
						float num6 = Mathf.Abs(mesh3MorphedVerts[j].x);
						float num7 = Mathf.Clamp01(num6 / graftSymmetryDistance);
						num3 *= num7;
						break;
					}
					case GraftSymmetryAxis.Y:
					{
						float num6 = Mathf.Abs(mesh3MorphedVerts[j].y);
						float num7 = Mathf.Clamp01(num6 / graftSymmetryDistance);
						num4 *= num7;
						break;
					}
					case GraftSymmetryAxis.Z:
					{
						float num6 = Mathf.Abs(mesh3MorphedVerts[j].z);
						float num7 = Mathf.Clamp01(num6 / graftSymmetryDistance);
						num5 *= num7;
						break;
					}
					}
				}
				for (int k = 0; k < num; k++)
				{
					int vertexNum2 = vertexPairs[k].vertexNum;
					int num8 = k * numGraft2BaseVertices + j;
					float num9 = _graft2Weights[num8];
					vector.x += array[vertexNum2].x * num9 * num3;
					vector.y += array[vertexNum2].y * num9 * num4;
					vector.z += array[vertexNum2].z * num9 * num5;
				}
				array[j] = vector;
				mergedUVVerts[num2].x = mesh3MorphedVerts[j].x + array[j].x;
				mergedUVVerts[num2].y = mesh3MorphedVerts[j].y + array[j].y;
				mergedUVVerts[num2].z = mesh3MorphedVerts[j].z + array[j].z;
			}
		}
		if (base.baseVerticesToUVVertices != null)
		{
			DAZVertexMap[] array3 = base.baseVerticesToUVVertices;
			foreach (DAZVertexMap dAZVertexMap in array3)
			{
				mergedUVVerts[dAZVertexMap.tovert] = mergedUVVerts[dAZVertexMap.fromvert];
			}
		}
	}

	public void UpdateVerticesPost(bool updateBaseVertices = false)
	{
		_verticesChangedThisFrame = _threadedVerticesChangedThisFrame;
		_visibleNonPoseVerticesChangedThisFrame = _threadedVisibleNonPoseVerticesChangedThisFrame;
		if (_verticesChangedThisFrame || _visibleNonPoseVerticesChangedThisFrame)
		{
			if (updateBaseVertices)
			{
				for (int i = 0; i < _numBaseVertices; i++)
				{
					_morphedBaseVertices[i] = _threadedMorphedBaseVertices[i];
					_morphedUVVertices[i] = _threadedMorphedUVVertices[i];
					_visibleMorphedUVVertices[i] = _threadedVisibleMorphedUVVertices[i];
				}
				for (int j = _numBaseVertices; j < _numUVVertices; j++)
				{
					_morphedUVVertices[j] = _threadedMorphedUVVertices[j];
					_visibleMorphedUVVertices[j] = _threadedVisibleMorphedUVVertices[j];
				}
			}
			else
			{
				for (int k = 0; k < _numUVVertices; k++)
				{
					_morphedUVVertices[k] = _threadedMorphedUVVertices[k];
					_visibleMorphedUVVertices[k] = _threadedVisibleMorphedUVVertices[k];
				}
			}
		}
		if (_useSmoothing)
		{
			InitMeshSmooth();
			meshSmooth.LaplacianSmooth(_morphedUVVertices, _smoothedMorphedUVVertices);
			meshSmooth.HCCorrection(_morphedUVVertices, _smoothedMorphedUVVertices, 0.5f);
			_updateDuplicateSmoothedMorphedUVVertices();
			if (_drawMorphedUVMappedMesh || !Application.isPlaying)
			{
				_morphedUVMappedMesh.vertices = _smoothedMorphedUVVertices;
				_morphedUVMappedMesh.normals = _morphedUVNormals;
				_morphedUVMappedMesh.tangents = _morphedUVTangents;
			}
		}
		else
		{
			_updateDuplicateMorphedUVVertices();
			if (_drawMorphedUVMappedMesh || !Application.isPlaying)
			{
				_morphedUVMappedMesh.vertices = _morphedUVVertices;
				_morphedUVMappedMesh.normals = _morphedUVNormals;
				_morphedUVMappedMesh.tangents = _morphedUVTangents;
			}
		}
		if (base.drawMorphedBaseMesh)
		{
			_morphedBaseMesh.vertices = _morphedBaseVertices;
			_morphedBaseMesh.normals = _morphedBaseNormals;
		}
	}

	public new void Draw()
	{
		base.Draw();
		if (!drawGraftMorphedMesh || !(graftMesh != null) || !(_graftMorphedMesh != null))
		{
			return;
		}
		Vector3[] array = graftMesh.baseVertices;
		for (int i = 0; i < array.Length; i++)
		{
			_graftMorphedMeshVertices[i] = array[i];
		}
		if (graftMeshMorphNamesForGrafting != null && graftMesh.morphBank != null)
		{
			if (graftMeshMorphNamesForGrafting.Length == graftMeshMorphValuesForGrafting.Length)
			{
				for (int j = 0; j < graftMeshMorphNamesForGrafting.Length; j++)
				{
					float num = graftMeshMorphValuesForGrafting[j];
					DAZMorph builtInMorph = graftMesh.morphBank.GetBuiltInMorph(graftMeshMorphNamesForGrafting[j]);
					if (builtInMorph != null && builtInMorph.deltas.Length > 0)
					{
						DAZMorphVertex[] deltas = builtInMorph.deltas;
						foreach (DAZMorphVertex dAZMorphVertex in deltas)
						{
							Vector3 vector = dAZMorphVertex.delta * num;
							_graftMorphedMeshVertices[dAZMorphVertex.vertex] += vector;
						}
					}
					else
					{
						Debug.LogError("Could not find graft morph " + graftMeshMorphNamesForGrafting[j]);
					}
				}
				_graftMorphedMesh.vertices = _graftMorphedMeshVertices;
			}
			else
			{
				Debug.LogError("Graft mesh morph names and morph values are not same length");
			}
		}
		Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
		if (Application.isPlaying && drawFromBone != null)
		{
			localToWorldMatrix *= drawFromBone.changeFromOriginalMatrix;
		}
		if (simpleMaterial != null)
		{
			for (int l = 0; l < _graftMorphedMesh.subMeshCount; l++)
			{
				Graphics.DrawMesh(_graftMorphedMesh, localToWorldMatrix, simpleMaterial, 0, null, l, null, false, false);
			}
		}
		else
		{
			Debug.LogWarning("Draw Graft Morphed Mesh is enabled but simple material is not set");
		}
	}

	public void ManualUpdate()
	{
		Update();
	}

	private void Update()
	{
		if (!staticMesh)
		{
			UpdateVertices(!Application.isPlaying);
		}
		Draw();
	}

	public override void Init()
	{
		if (!_wasInit)
		{
			if (targetMesh != null)
			{
				targetMesh.Init();
			}
			if (graftMesh != null)
			{
				graftMesh.Init();
			}
			if (graft2Mesh != null)
			{
				graft2Mesh.Init();
			}
			base.Init();
		}
	}
}
