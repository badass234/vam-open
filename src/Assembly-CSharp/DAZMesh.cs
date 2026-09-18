using System;
using System.Collections.Generic;
using System.IO;
using MVR;
using MVR.FileManagement;
using MeshVR;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class DAZMesh : ObjectAllocator, IBinaryStorable, RenderSuspend
{
	public enum BoneSide
	{
		Both,
		Right,
		Left,
		None
	}

	protected const float geoScale = 0.01f;

	public bool drawBaseMesh;

	[SerializeField]
	protected bool _drawMorphedBaseMesh;

	public bool drawUVMappedMesh;

	[SerializeField]
	protected bool _drawMorphedUVMappedMesh;

	public bool drawInEditorWhenNotPlaying;

	protected bool _renderSuspend;

	public bool recalcNormalsOnMorph;

	public bool recalcTangentsOnMorph;

	public bool useUnityRecalcNormals;

	[SerializeField]
	protected bool _useSmoothing;

	public bool useSimpleMaterial;

	public Material simpleMaterial;

	public Material discardMaterial;

	public Material[] materials;

	public bool use2PassMaterials;

	public Material[] materialsPass1;

	public Material[] materialsPass2;

	public bool[] materialsEnabled;

	public bool[] materialsPass1Enabled;

	public bool[] materialsPass2Enabled;

	public bool[] materialsShadowCastEnabled;

	public DAZMesh copyMaterialsFrom;

	public bool castShadows = true;

	public bool receiveShadows = true;

	public string nodeId;

	public string sceneNodeId;

	public string geometryId;

	public string overrideGeometryId;

	public string sceneGeometryId;

	[SerializeField]
	protected int _numBaseVertices;

	[SerializeField]
	protected int _numBasePolygons;

	[SerializeField]
	protected int _numUVVertices;

	[SerializeField]
	protected int _numMaterials;

	[SerializeField]
	protected string[] _materialNames;

	public DAZMorphBank morphBank;

	public float vertexNormalOffset;

	public Vector3 vertexOffset = Vector3.zero;

	protected bool _verticesChangedLastFrame;

	protected bool _visibleNonPoseVerticesChangedLastFrame;

	protected bool _verticesChangedThisFrame;

	protected bool _visibleNonPoseVerticesChangedThisFrame;

	private bool _normalsDirtyThisFrame;

	private bool _tangentsDirtyThisFrame;

	protected Mesh _baseMesh;

	[SerializeField]
	protected DAZMeshData _meshData;

	[SerializeField]
	protected Vector3[] _baseVertices;

	protected Vector3[] _baseNormals;

	protected Vector3[] _baseSurfaceNormals;

	[SerializeField]
	protected MeshPoly[] _basePolyList;

	protected int[] _baseTriangles;

	protected int[][] _baseMaterialVertices;

	protected Mesh _morphedBaseMesh;

	protected Vector3[] _morphedBaseVertices;

	protected Vector3[] _morphedBaseNormals;

	protected Vector3[] _morphedBaseSurfaceNormals;

	public bool debugGrafting;

	public DAZMeshGraft meshGraft;

	public DAZMesh graftTo;

	public bool debug;

	public int debugVertex;

	[SerializeField]
	protected DAZVertexMap[] _baseVerticesToUVVertices;

	protected Mesh _uvMappedMesh;

	[SerializeField]
	protected Vector3[] _UVVertices;

	protected Vector3[] _UVNormals;

	protected Vector4[] _UVTangents;

	[SerializeField]
	protected MeshPoly[] _UVPolyList;

	protected int[] _UVTriangles;

	protected Vector2[] _UV;

	[SerializeField]
	protected Vector2[] _OrigUV;

	[SerializeField]
	protected bool _usePatches;

	public int numUVPatches;

	public DAZUVPatch[] UVPatches;

	protected MeshSmooth meshSmooth;

	protected Mesh _morphedUVMappedMesh;

	protected Mesh _visibleMorphedUVMappedMesh;

	[SerializeField]
	protected bool _drawVisibleMorphedUVMappedMesh;

	protected Vector3[] _visibleMorphedUVVertices;

	protected Vector3[] _morphedUVVertices;

	protected Vector3[] _smoothedMorphedUVVertices;

	protected bool[] _morphedBaseDirtyVertices;

	protected bool[] _morphedUVDirtyVertices;

	public bool morphedNormalsDirty;

	public bool morphedTangentsDirty;

	protected Vector3[] _morphedUVNormals;

	protected Vector4[] _morphedUVTangents;

	[SerializeField]
	protected bool _createMeshFilterAndRenderer;

	[SerializeField]
	protected bool _createMeshCollider;

	[SerializeField]
	protected bool _useConvexCollider;

	protected bool useUnityMaterialOrdering;

	public string shaderNameForDynamicLoad = "Custom/Subsurface/TransparentGlossNMNoCullSeparateAlpha";

	public Vector3 reducePoint;

	public Vector3 reduceUp;

	public string assetSavePath = "Assets/VaMAssets/Generated/";

	protected bool _meshRendererMaterialsWasInit;

	public bool useDrawOffset;

	public Vector3 drawOffset;

	public Vector3 drawOffsetOrigin;

	public Vector3 drawOffsetRotation;

	public float drawOffsetOverallScale = 1f;

	public Vector3 drawOffsetScale = Vector3.one;

	protected Matrix4x4 lastMatrix1 = Matrix4x4.identity;

	protected Matrix4x4 lastMatrix2 = Matrix4x4.identity;

	protected Matrix4x4 identityMatrix = Matrix4x4.identity;

	protected Matrix4x4 drawOffsetMatrix = Matrix4x4.identity;

	protected Matrix4x4 drawOffsetOriginMatrix1 = Matrix4x4.identity;

	protected Matrix4x4 drawOffsetOriginMatrix2 = Matrix4x4.identity;

	protected Quaternion identityQuaternion = Quaternion.identity;

	protected Vector3 oneVector = Vector3.one;

	public DAZBone drawFromBone;

	public BoneSide boneSide;

	public int delayDisplayFrameCount;

	protected bool _materialsWereInit;

	protected bool _wasInit;

	public bool drawMorphedBaseMesh
	{
		get
		{
			return _drawMorphedBaseMesh;
		}
		set
		{
			if (_drawMorphedBaseMesh != value)
			{
				_drawMorphedBaseMesh = value;
				if (value)
				{
					ApplyMorphs(true);
				}
			}
		}
	}

	public bool drawMorphedUVMappedMesh
	{
		get
		{
			return _drawMorphedUVMappedMesh;
		}
		set
		{
			if (_drawMorphedUVMappedMesh != value)
			{
				_drawMorphedUVMappedMesh = value;
				if (value)
				{
					ApplyMorphs(true);
				}
			}
		}
	}

	public bool renderSuspend
	{
		get
		{
			return _renderSuspend;
		}
		set
		{
			_renderSuspend = value;
		}
	}

	public bool useSmoothing
	{
		get
		{
			return _useSmoothing;
		}
		set
		{
			if (_useSmoothing != value)
			{
				_useSmoothing = value;
				ApplyMorphs(true);
			}
		}
	}

	public int numBaseVertices => _numBaseVertices;

	public int numBasePolygons => _numBasePolygons;

	public int numUVVertices => _numUVVertices;

	public int numMaterials => _numMaterials;

	public string[] materialNames => _materialNames;

	public bool verticesChangedLastFrame => _verticesChangedLastFrame;

	public bool visibleNonPoseVerticesChangedLastFrame => _visibleNonPoseVerticesChangedLastFrame;

	public bool verticesChangedThisFrame => _verticesChangedThisFrame;

	public bool visibleNonPoseVerticesChangedThisFrame => _visibleNonPoseVerticesChangedThisFrame;

	public bool normalsDirtyThisFrame => _normalsDirtyThisFrame;

	public bool tangentsDirtyThisFrame => _tangentsDirtyThisFrame;

	public Mesh baseMesh
	{
		get
		{
			if (_baseMesh == null)
			{
				Init();
			}
			return _baseMesh;
		}
	}

	public DAZMeshData meshData
	{
		get
		{
			return _meshData;
		}
		set
		{
			if (_meshData != value)
			{
				if (value == null)
				{
					_baseVertices = (Vector3[])_meshData.baseVertices.Clone();
					_basePolyList = (MeshPoly[])_meshData.basePolyList.Clone();
					_baseVerticesToUVVertices = (DAZVertexMap[])_meshData.baseVerticesToUVVertices.Clone();
					_UVVertices = (Vector3[])_meshData.UVVertices.Clone();
					_UVPolyList = (MeshPoly[])_meshData.UVPolyList.Clone();
					_OrigUV = (Vector2[])_meshData.OrigUV.Clone();
					_meshData = null;
				}
				else
				{
					_meshData = value;
					_baseVertices = null;
					_basePolyList = null;
					_baseVerticesToUVVertices = null;
					_UVVertices = null;
					_UVPolyList = null;
					_OrigUV = null;
				}
			}
		}
	}

	public Vector3[] baseVertices
	{
		get
		{
			if (_meshData != null)
			{
				return _meshData.baseVertices;
			}
			return _baseVertices;
		}
	}

	public Vector3[] baseNormals => _baseNormals;

	public Vector3[] baseSurfaceNormals => _baseSurfaceNormals;

	public MeshPoly[] basePolyList
	{
		get
		{
			if (_meshData != null)
			{
				return _meshData.basePolyList;
			}
			return _basePolyList;
		}
	}

	public int[] baseTriangles => _baseTriangles;

	public int[][] baseMaterialVertices => _baseMaterialVertices;

	public Vector3[] morphedBaseVertices => _morphedBaseVertices;

	public DAZVertexMap[] baseVerticesToUVVertices
	{
		get
		{
			if (_meshData != null)
			{
				return _meshData.baseVerticesToUVVertices;
			}
			return _baseVerticesToUVVertices;
		}
	}

	public Dictionary<int, List<int>> baseVertToUVVertFullMap
	{
		get
		{
			Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
			for (int i = 0; i < _numBaseVertices; i++)
			{
				List<int> list = new List<int>();
				list.Add(i);
				dictionary.Add(i, list);
			}
			DAZVertexMap[] array = baseVerticesToUVVertices;
			for (int j = 0; j < array.Length; j++)
			{
				int fromvert = array[j].fromvert;
				int tovert = array[j].tovert;
				List<int> value;
				if (dictionary.TryGetValue(fromvert, out value))
				{
					value.Add(tovert);
				}
			}
			return dictionary;
		}
	}

	public Dictionary<int, int> uvVertToBaseVert
	{
		get
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			DAZVertexMap[] array = baseVerticesToUVVertices;
			for (int i = 0; i < array.Length; i++)
			{
				int fromvert = array[i].fromvert;
				int tovert = array[i].tovert;
				if (!dictionary.ContainsKey(tovert))
				{
					dictionary.Add(tovert, fromvert);
				}
			}
			return dictionary;
		}
	}

	public Mesh uvMappedMesh => _uvMappedMesh;

	public Vector3[] UVVertices
	{
		get
		{
			if (_meshData != null)
			{
				return _meshData.UVVertices;
			}
			return _UVVertices;
		}
	}

	public Vector3[] UVNormals => _UVNormals;

	public Vector4[] UVTangents => _UVTangents;

	public MeshPoly[] UVPolyList
	{
		get
		{
			if (_meshData != null)
			{
				return _meshData.UVPolyList;
			}
			return _UVPolyList;
		}
	}

	public int[] UVTriangles => _UVTriangles;

	public Vector2[] UV => _UV;

	public Vector2[] OrigUV
	{
		get
		{
			if (_meshData != null)
			{
				return _meshData.OrigUV;
			}
			return _OrigUV;
		}
	}

	public bool usePatches
	{
		get
		{
			return _usePatches;
		}
		set
		{
			if (_usePatches != value)
			{
				_usePatches = value;
				ApplyUVPatches();
				RecalculateMorphedMeshTangents(true);
			}
		}
	}

	public Mesh morphedUVMappedMesh => _morphedUVMappedMesh;

	public bool drawVisibleMorphedUVMappedMesh
	{
		get
		{
			return _drawVisibleMorphedUVMappedMesh;
		}
		set
		{
			if (_drawVisibleMorphedUVMappedMesh != value)
			{
				_drawVisibleMorphedUVMappedMesh = value;
				if (value)
				{
					ApplyMorphs(true);
				}
			}
		}
	}

	public Vector3[] visibleMorphedUVVertices => _visibleMorphedUVVertices;

	public Vector3[] rawMorphedUVVertices => _morphedUVVertices;

	public Vector3[] morphedUVVertices
	{
		get
		{
			if (useSmoothing)
			{
				return _smoothedMorphedUVVertices;
			}
			return _morphedUVVertices;
		}
	}

	public bool[] morphedBaseDirtyVertices
	{
		get
		{
			return _morphedBaseDirtyVertices;
		}
		set
		{
			_morphedBaseDirtyVertices = value;
		}
	}

	public bool[] morphedUVDirtyVertices
	{
		get
		{
			return _morphedUVDirtyVertices;
		}
		set
		{
			_morphedUVDirtyVertices = value;
		}
	}

	public Vector3[] morphedUVNormals => _morphedUVNormals;

	public Vector4[] morphedUVTangents => _morphedUVTangents;

	public bool createMeshFilterAndRenderer
	{
		get
		{
			return _createMeshFilterAndRenderer;
		}
		set
		{
			if (_createMeshFilterAndRenderer != value)
			{
				_createMeshFilterAndRenderer = value;
				InitMeshFilterAndRenderer();
			}
		}
	}

	public bool createMeshCollider
	{
		get
		{
			return _createMeshCollider;
		}
		set
		{
			if (_createMeshCollider != value)
			{
				_createMeshCollider = value;
				InitCollider();
			}
		}
	}

	public bool useConvexCollider
	{
		get
		{
			return _useConvexCollider;
		}
		set
		{
			if (_useConvexCollider != value)
			{
				_useConvexCollider = value;
				InitCollider();
			}
		}
	}

	public bool wasInit => _wasInit;

	public void CopyMaterials()
	{
		if (copyMaterialsFrom != null)
		{
			materials = new Material[copyMaterialsFrom.materials.Length];
			materialsPass1 = new Material[copyMaterialsFrom.materialsPass1.Length];
			materialsPass2 = new Material[copyMaterialsFrom.materialsPass2.Length];
			materialsEnabled = new bool[copyMaterialsFrom.materials.Length];
			materialsPass1Enabled = new bool[copyMaterialsFrom.materialsPass1.Length];
			materialsPass2Enabled = new bool[copyMaterialsFrom.materialsPass2.Length];
			materialsShadowCastEnabled = new bool[copyMaterialsFrom.materials.Length];
			for (int i = 0; i < copyMaterialsFrom.materials.Length; i++)
			{
				materials[i] = copyMaterialsFrom.materials[i];
				materialsEnabled[i] = copyMaterialsFrom.materialsEnabled[i];
				materialsPass2Enabled[i] = copyMaterialsFrom.materialsPass2Enabled[i];
				materialsShadowCastEnabled[i] = copyMaterialsFrom.materialsShadowCastEnabled[i];
			}
			for (int j = 0; j < copyMaterialsFrom.materialsPass1.Length; j++)
			{
				materialsPass1[j] = copyMaterialsFrom.materialsPass1[j];
			}
			for (int k = 0; k < copyMaterialsFrom.materialsPass2.Length; k++)
			{
				materialsPass2[k] = copyMaterialsFrom.materialsPass2[k];
			}
		}
	}

	public void ApplyUVPatches()
	{
		_UV = new Vector2[_numUVVertices];
		Vector2[] origUV = OrigUV;
		for (int i = 0; i < _numUVVertices; i++)
		{
			_UV[i] = origUV[i];
		}
		if (_usePatches)
		{
			for (int j = 0; j < numUVPatches; j++)
			{
				int vertexNum = UVPatches[j].vertexNum;
				if (vertexNum >= 0 && vertexNum < _numUVVertices)
				{
					_UV[vertexNum] = UVPatches[j].uv;
				}
			}
		}
		_uvMappedMesh.uv = _UV;
		_morphedUVMappedMesh.uv = _UV;
	}

	protected void InitMeshSmooth()
	{
		if (meshSmooth == null && baseVertices != null && basePolyList != null && baseVerticesToUVVertices != null)
		{
			meshSmooth = new MeshSmooth(baseVertices, basePolyList);
		}
	}

	private void InitCollider()
	{
		MeshCollider meshCollider = base.gameObject.GetComponent<MeshCollider>();
		if (_createMeshCollider)
		{
			if (meshCollider == null)
			{
				meshCollider = base.gameObject.AddComponent<MeshCollider>();
			}
			meshCollider.sharedMesh = morphedUVMappedMesh;
			meshCollider.convex = useConvexCollider;
			// InflateConvexMesh is left out: 2018.1 still honoured it and had not deprecated it, but
			// since 2018.3 the engine ignores the bit and naming it is obsolete-as-error. The other
			// three keep their 2018.1 values.
			meshCollider.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.EnableMeshCleaning | MeshColliderCookingOptions.WeldColocatedVertices;
			meshCollider.skinWidth = 0.0001f;
		}
	}

	protected void PolyListToTriangleIndexes(MeshPoly[] polylist, List<List<int>> indexes, List<HashSet<int>> materialVertices = null)
	{
		Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
		int num = -1;
		foreach (MeshPoly meshPoly in polylist)
		{
			int materialNum = meshPoly.materialNum;
			if (useUnityMaterialOrdering)
			{
				bool value;
				if (!dictionary.TryGetValue(materialNum, out value))
				{
					num++;
					dictionary.Add(materialNum, true);
				}
			}
			else
			{
				num = materialNum;
			}
			int item = meshPoly.vertices[0];
			int item2 = meshPoly.vertices[1];
			int item3 = meshPoly.vertices[2];
			indexes[num].Add(item3);
			indexes[num].Add(item2);
			indexes[num].Add(item);
			if (materialVertices != null)
			{
				materialVertices[num].Add(item);
				materialVertices[num].Add(item2);
				materialVertices[num].Add(item3);
			}
			if (meshPoly.vertices.Length == 4)
			{
				int item4 = meshPoly.vertices[3];
				indexes[num].Add(item);
				indexes[num].Add(item4);
				indexes[num].Add(item3);
				materialVertices?[num].Add(item4);
			}
		}
	}

	public bool LoadFromBinaryReader(BinaryReader binReader)
	{
		try
		{
			string text = binReader.ReadString();
			if (text != "DAZMesh")
			{
				SuperController.LogError("Binary file corrupted. Tried to read DAZMesh in wrong section");
				return false;
			}
			string text2 = binReader.ReadString();
			if (text2 != "1.0")
			{
				SuperController.LogError("DAZMesh schema " + text2 + " is not compatible with this version of software");
				return false;
			}
			nodeId = binReader.ReadString();
			sceneNodeId = binReader.ReadString();
			geometryId = binReader.ReadString();
			sceneGeometryId = binReader.ReadString();
			Shader shader = Shader.Find(shaderNameForDynamicLoad);
			if (shader == null)
			{
				SuperController.LogError("Could not find shader " + shaderNameForDynamicLoad + ". Can't load DAZMesh from binary file");
				return false;
			}
			meshSmooth = null;
			_meshData = null;
			_numBaseVertices = binReader.ReadInt32();
			_baseVertices = new Vector3[_numBaseVertices];
			Vector3 vector = default(Vector3);
			for (int i = 0; i < _numBaseVertices; i++)
			{
				vector.x = binReader.ReadSingle();
				vector.y = binReader.ReadSingle();
				vector.z = binReader.ReadSingle();
				_baseVertices[i] = vector;
			}
			_numMaterials = binReader.ReadInt32();
			_materialNames = new string[_numMaterials];
			materials = new Material[_numMaterials];
			materialsEnabled = new bool[_numMaterials];
			materialsShadowCastEnabled = new bool[_numMaterials];
			for (int j = 0; j < _numMaterials; j++)
			{
				_materialNames[j] = binReader.ReadString();
				materialsEnabled[j] = true;
				materialsShadowCastEnabled[j] = true;
				if (shader != null)
				{
					materials[j] = new Material(shader);
					RegisterAllocatedObject(materials[j]);
					materials[j].name = _materialNames[j];
				}
			}
			_numBasePolygons = binReader.ReadInt32();
			_basePolyList = new MeshPoly[_numBasePolygons];
			_UVPolyList = new MeshPoly[_numBasePolygons];
			for (int k = 0; k < _numBasePolygons; k++)
			{
				MeshPoly meshPoly = new MeshPoly();
				meshPoly.materialNum = binReader.ReadInt32();
				int num = binReader.ReadInt32();
				meshPoly.vertices = new int[num];
				for (int l = 0; l < num; l++)
				{
					meshPoly.vertices[l] = binReader.ReadInt32();
				}
				_basePolyList[k] = meshPoly;
			}
			for (int m = 0; m < _numBasePolygons; m++)
			{
				MeshPoly meshPoly2 = new MeshPoly();
				meshPoly2.materialNum = binReader.ReadInt32();
				int num2 = binReader.ReadInt32();
				meshPoly2.vertices = new int[num2];
				for (int n = 0; n < num2; n++)
				{
					meshPoly2.vertices[n] = binReader.ReadInt32();
				}
				_UVPolyList[m] = meshPoly2;
			}
			_numUVVertices = binReader.ReadInt32();
			_UVVertices = new Vector3[_numUVVertices];
			for (int num3 = 0; num3 < _baseVertices.Length; num3++)
			{
				_UVVertices[num3] = _baseVertices[num3];
			}
			_OrigUV = new Vector2[_numUVVertices];
			Vector2 vector2 = default(Vector2);
			for (int num4 = 0; num4 < _numUVVertices; num4++)
			{
				vector2.x = binReader.ReadSingle();
				vector2.y = binReader.ReadSingle();
				_OrigUV[num4] = vector2;
			}
			int num5 = binReader.ReadInt32();
			_baseVerticesToUVVertices = new DAZVertexMap[num5];
			for (int num6 = 0; num6 < num5; num6++)
			{
				DAZVertexMap dAZVertexMap = new DAZVertexMap();
				dAZVertexMap.fromvert = binReader.ReadInt32();
				dAZVertexMap.tovert = binReader.ReadInt32();
				dAZVertexMap.polyindex = binReader.ReadInt32();
				_baseVerticesToUVVertices[num6] = dAZVertexMap;
			}
			DAZVertexMap[] array = _baseVerticesToUVVertices;
			foreach (DAZVertexMap dAZVertexMap2 in array)
			{
				_UVVertices[dAZVertexMap2.tovert] = _UVVertices[dAZVertexMap2.fromvert];
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Error while loading DAZMesh from binary reader " + ex);
			return false;
		}
		DeriveMeshes();
		return true;
	}

	public bool LoadFromBinaryFile(string path)
	{
		bool result = false;
		try
		{
			using (FileEntryStream fileEntryStream = FileManager.OpenStream(path, true))
			{
				using (BinaryReader binReader = new BinaryReader(fileEntryStream.Stream))
				{
					result = LoadFromBinaryReader(binReader);
				}
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Error while loading DAZMesh from binary file " + path + " " + ex);
		}
		return result;
	}

	public bool StoreToBinaryWriter(BinaryWriter binWriter)
	{
		try
		{
			binWriter.Write("DAZMesh");
			binWriter.Write("1.0");
			binWriter.Write(nodeId);
			binWriter.Write(sceneNodeId);
			binWriter.Write(geometryId);
			binWriter.Write(sceneGeometryId);
			binWriter.Write(_numBaseVertices);
			Vector3[] array = baseVertices;
			for (int i = 0; i < array.Length; i++)
			{
				Vector3 vector = array[i];
				binWriter.Write(vector.x);
				binWriter.Write(vector.y);
				binWriter.Write(vector.z);
			}
			binWriter.Write(_numMaterials);
			string[] array2 = _materialNames;
			foreach (string value in array2)
			{
				binWriter.Write(value);
			}
			binWriter.Write(_numBasePolygons);
			MeshPoly[] array3 = basePolyList;
			foreach (MeshPoly meshPoly in array3)
			{
				binWriter.Write(meshPoly.materialNum);
				binWriter.Write(meshPoly.vertices.Length);
				int[] vertices = meshPoly.vertices;
				foreach (int value2 in vertices)
				{
					binWriter.Write(value2);
				}
			}
			MeshPoly[] uVPolyList = UVPolyList;
			foreach (MeshPoly meshPoly2 in uVPolyList)
			{
				binWriter.Write(meshPoly2.materialNum);
				binWriter.Write(meshPoly2.vertices.Length);
				int[] vertices2 = meshPoly2.vertices;
				foreach (int value3 in vertices2)
				{
					binWriter.Write(value3);
				}
			}
			binWriter.Write(_numUVVertices);
			Vector2[] origUV = _OrigUV;
			for (int num = 0; num < origUV.Length; num++)
			{
				Vector2 vector2 = origUV[num];
				binWriter.Write(vector2.x);
				binWriter.Write(vector2.y);
			}
			binWriter.Write(_baseVerticesToUVVertices.Length);
			DAZVertexMap[] array4 = _baseVerticesToUVVertices;
			foreach (DAZVertexMap dAZVertexMap in array4)
			{
				binWriter.Write(dAZVertexMap.fromvert);
				binWriter.Write(dAZVertexMap.tovert);
				binWriter.Write(dAZVertexMap.polyindex);
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Error while storing DAZMesh to binary reader " + ex);
			return false;
		}
		return true;
	}

	public bool StoreToBinaryFile(string path)
	{
		bool result = false;
		try
		{
			FileManager.AssertNotCalledFromPlugin();
			using (FileStream output = FileManager.OpenStreamForCreate(path))
			{
				using (BinaryWriter binWriter = new BinaryWriter(output))
				{
					result = StoreToBinaryWriter(binWriter);
				}
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Error while storing DAZMesh to binary file " + path + " " + ex);
		}
		return result;
	}

	public virtual void DeriveMeshes()
	{
		bool flag = false;
		if (baseVertices.Length > 65000 || UVVertices.Length > 65000)
		{
			flag = true;
		}
		_baseMesh = new Mesh();
		RegisterAllocatedObject(_baseMesh);
		_morphedBaseMesh = new Mesh();
		RegisterAllocatedObject(_morphedBaseMesh);
		_uvMappedMesh = new Mesh();
		RegisterAllocatedObject(_uvMappedMesh);
		if (_morphedUVMappedMesh == null || !Application.isPlaying)
		{
			_morphedUVMappedMesh = new Mesh();
			RegisterAllocatedObject(_morphedUVMappedMesh);
		}
		_visibleMorphedUVMappedMesh = new Mesh();
		RegisterAllocatedObject(_visibleMorphedUVMappedMesh);
		if (flag)
		{
			_baseMesh.indexFormat = IndexFormat.UInt32;
			_morphedBaseMesh.indexFormat = IndexFormat.UInt32;
			_uvMappedMesh.indexFormat = IndexFormat.UInt32;
			_morphedUVMappedMesh.indexFormat = IndexFormat.UInt32;
			_visibleMorphedUVMappedMesh.indexFormat = IndexFormat.UInt32;
		}
		_morphedBaseVertices = (Vector3[])baseVertices.Clone();
		_morphedUVVertices = (Vector3[])UVVertices.Clone();
		_visibleMorphedUVVertices = (Vector3[])UVVertices.Clone();
		_smoothedMorphedUVVertices = (Vector3[])_morphedUVVertices.Clone();
		_baseMesh.vertices = baseVertices;
		_morphedBaseMesh.vertices = baseVertices;
		_uvMappedMesh.vertices = UVVertices;
		_morphedUVMappedMesh.vertices = _morphedUVVertices;
		_visibleMorphedUVMappedMesh.vertices = _visibleMorphedUVVertices;
		List<List<int>> list = new List<List<int>>();
		List<HashSet<int>> list2 = new List<HashSet<int>>();
		List<List<int>> list3 = new List<List<int>>();
		list.Capacity = _numMaterials;
		list2.Capacity = _numMaterials;
		list3.Capacity = _numMaterials;
		for (int i = 0; i < _numMaterials; i++)
		{
			list.Add(new List<int>());
			list2.Add(new HashSet<int>());
			list3.Add(new List<int>());
		}
		_baseMesh.subMeshCount = _numMaterials;
		_morphedBaseMesh.subMeshCount = _numMaterials;
		_uvMappedMesh.subMeshCount = _numMaterials;
		_morphedUVMappedMesh.subMeshCount = _numMaterials;
		_visibleMorphedUVMappedMesh.subMeshCount = _numMaterials;
		PolyListToTriangleIndexes(basePolyList, list, list2);
		PolyListToTriangleIndexes(UVPolyList, list3);
		_baseMaterialVertices = new int[_numMaterials][];
		for (int j = 0; j < _numMaterials; j++)
		{
			_baseMaterialVertices[j] = new int[list2[j].Count];
			list2[j].CopyTo(_baseMaterialVertices[j]);
			int[] indices = list[j].ToArray();
			_baseMesh.SetIndices(indices, MeshTopology.Triangles, j);
			_morphedBaseMesh.SetIndices(indices, MeshTopology.Triangles, j);
			int[] indices2 = list3[j].ToArray();
			_uvMappedMesh.SetIndices(indices2, MeshTopology.Triangles, j);
			_morphedUVMappedMesh.SetIndices(indices2, MeshTopology.Triangles, j);
			_visibleMorphedUVMappedMesh.SetIndices(indices2, MeshTopology.Triangles, j);
		}
		_baseTriangles = _baseMesh.triangles;
		_UVTriangles = _uvMappedMesh.triangles;
		_baseMesh.RecalculateBounds();
		_morphedBaseMesh.bounds = _baseMesh.bounds;
		_uvMappedMesh.bounds = _baseMesh.bounds;
		_morphedUVMappedMesh.bounds = _baseMesh.bounds;
		_visibleMorphedUVMappedMesh.bounds = _baseMesh.bounds;
		ApplyUVPatches();
		_baseNormals = new Vector3[_numBaseVertices];
		_baseSurfaceNormals = new Vector3[_baseTriangles.Length / 3];
		RecalculateNormals.recalculateNormals(_baseTriangles, baseVertices, _baseNormals, _baseSurfaceNormals);
		_morphedBaseSurfaceNormals = (Vector3[])_baseSurfaceNormals.Clone();
		_morphedBaseNormals = (Vector3[])_baseNormals.Clone();
		_morphedUVNormals = new Vector3[_numUVVertices];
		for (int k = 0; k < _morphedBaseNormals.Length; k++)
		{
			_morphedUVNormals[k] = _morphedBaseNormals[k];
		}
		_updateDuplicateMorphedUVNormals();
		_UVNormals = (Vector3[])_morphedUVNormals.Clone();
		_baseMesh.normals = _baseNormals;
		_morphedBaseMesh.normals = _morphedBaseNormals;
		_uvMappedMesh.normals = _UVNormals;
		_morphedUVMappedMesh.normals = _morphedUVNormals;
		_visibleMorphedUVMappedMesh.normals = _morphedUVNormals;
		ResetMorphedVerticesToOffset();
		_UVTangents = new Vector4[_numUVVertices];
		RecalculateTangents.recalculateTangentsFast(_UVTriangles, UVVertices, _UVNormals, UV, _UVTangents);
		_morphedUVTangents = (Vector4[])_UVTangents.Clone();
		_uvMappedMesh.tangents = _UVTangents;
		_morphedUVMappedMesh.tangents = _morphedUVTangents;
		_visibleMorphedUVMappedMesh.tangents = _morphedUVTangents;
		_morphedBaseDirtyVertices = new bool[_morphedUVNormals.Length];
		_morphedUVDirtyVertices = new bool[_morphedUVNormals.Length];
	}

	public void RecalculateMorphedMeshNormals(bool forceAll = false)
	{
		if (useUnityRecalcNormals)
		{
			_morphedBaseMesh.vertices = _morphedBaseVertices;
			_morphedBaseMesh.RecalculateNormals();
			_morphedBaseNormals = _morphedBaseMesh.normals;
		}
		else if (_baseTriangles != null)
		{
			if (forceAll)
			{
				RecalculateNormals.recalculateNormals(_baseTriangles, morphedUVVertices, _morphedBaseNormals, _morphedBaseSurfaceNormals);
			}
			else
			{
				RecalculateNormals.recalculateNormals(_baseTriangles, morphedUVVertices, _morphedBaseNormals, _morphedBaseSurfaceNormals, morphedBaseDirtyVertices);
			}
			if (_drawMorphedBaseMesh)
			{
				_morphedBaseMesh.normals = _morphedBaseNormals;
			}
		}
		for (int i = 0; i < _morphedBaseNormals.Length; i++)
		{
			_morphedUVNormals[i] = _morphedBaseNormals[i];
		}
		_updateDuplicateMorphedUVNormals();
		if (_drawMorphedUVMappedMesh || !Application.isPlaying)
		{
			_morphedUVMappedMesh.normals = _morphedUVNormals;
		}
		if (_drawMorphedUVMappedMesh || !Application.isPlaying)
		{
			_visibleMorphedUVMappedMesh.normals = _morphedUVNormals;
		}
	}

	public void RecalculateMorphedMeshTangentsAccurate()
	{
		Vector3[] tan = null;
		Vector3[] tan2 = null;
		RecalculateTangents.recalculateTangentsAccurate(_UVTriangles, morphedUVVertices, _morphedUVNormals, UV, ref tan, ref tan2, _morphedUVTangents);
		if (_drawMorphedUVMappedMesh || !Application.isPlaying)
		{
			_morphedUVMappedMesh.tangents = _morphedUVTangents;
		}
		if (_drawVisibleMorphedUVMappedMesh || !Application.isPlaying)
		{
			_visibleMorphedUVMappedMesh.tangents = _morphedUVTangents;
		}
	}

	public void RecalculateMorphedMeshTangents(bool forceAll = false)
	{
		if (forceAll)
		{
			RecalculateTangents.recalculateTangentsFast(_UVTriangles, morphedUVVertices, _morphedUVNormals, UV, _morphedUVTangents);
		}
		else
		{
			RecalculateTangents.recalculateTangentsFast(_UVTriangles, morphedUVVertices, _morphedUVNormals, UV, _morphedUVTangents, morphedUVDirtyVertices);
		}
		if (_drawMorphedUVMappedMesh || !Application.isPlaying)
		{
			_morphedUVMappedMesh.tangents = _morphedUVTangents;
		}
		if (_drawVisibleMorphedUVMappedMesh || !Application.isPlaying)
		{
			_visibleMorphedUVMappedMesh.tangents = _morphedUVTangents;
		}
	}

	protected void _triggerNormalAndTangentRecalc()
	{
		if (recalcNormalsOnMorph && morphedNormalsDirty)
		{
			RecalculateMorphedMeshNormals();
			morphedNormalsDirty = false;
			_normalsDirtyThisFrame = true;
		}
		if (recalcTangentsOnMorph && morphedTangentsDirty)
		{
			DAZVertexMap[] array = baseVerticesToUVVertices;
			foreach (DAZVertexMap dAZVertexMap in array)
			{
				morphedUVDirtyVertices[dAZVertexMap.tovert] = morphedUVDirtyVertices[dAZVertexMap.fromvert];
			}
			RecalculateMorphedMeshTangents();
			morphedTangentsDirty = false;
			_tangentsDirtyThisFrame = true;
		}
	}

	public void Import(JSONNode jsonGeometry, DAZUVMap uvmap, Dictionary<string, Material> materialMap, bool inverseTransform = false)
	{
		meshSmooth = null;
		_meshData = null;
		JSONNode jSONNode = jsonGeometry["vertices"];
		_numBaseVertices = jSONNode["count"].AsInt;
		_baseVertices = new Vector3[_numBaseVertices];
		int num = 0;
		foreach (JSONNode item in jSONNode["values"].AsArray)
		{
			float asFloat = item[0].AsFloat;
			float asFloat2 = item[1].AsFloat;
			float asFloat3 = item[2].AsFloat;
			_baseVertices[num].x = (0f - asFloat) * 0.01f;
			_baseVertices[num].y = asFloat2 * 0.01f;
			_baseVertices[num].z = asFloat3 * 0.01f;
			if (inverseTransform)
			{
				_baseVertices[num] = base.transform.InverseTransformPoint(_baseVertices[num]);
			}
			num++;
		}
		_numMaterials = jsonGeometry["polygon_material_groups"]["count"].AsInt;
		if (materials == null || materials.Length != _numMaterials)
		{
			materials = new Material[_numMaterials];
			_materialNames = new string[_numMaterials];
		}
		if (materialsPass1 == null || materialsPass1.Length != _numMaterials)
		{
			materialsPass1 = new Material[_numMaterials];
		}
		if (materialsPass2 == null || materialsPass2.Length != _numMaterials)
		{
			materialsPass2 = new Material[_numMaterials];
		}
		if (materialsEnabled == null || materialsEnabled.Length != _numMaterials)
		{
			materialsEnabled = new bool[_numMaterials];
		}
		if (materialsPass1Enabled == null || materialsPass1Enabled.Length != _numMaterials)
		{
			materialsPass1Enabled = new bool[_numMaterials];
		}
		if (materialsPass2Enabled == null || materialsPass2Enabled.Length != _numMaterials)
		{
			materialsPass2Enabled = new bool[_numMaterials];
		}
		if (materialsShadowCastEnabled == null || materialsShadowCastEnabled.Length != _numMaterials)
		{
			materialsShadowCastEnabled = new bool[_numMaterials];
		}
		num = 0;
		foreach (JSONNode item2 in jsonGeometry["polygon_material_groups"]["values"].AsArray)
		{
			_materialNames[num] = item2;
			materialsEnabled[num] = true;
			materialsShadowCastEnabled[num] = true;
			Material value;
			if (materialMap.TryGetValue(item2, out value))
			{
				materials[num] = value;
			}
			num++;
		}
		JSONNode jSONNode4 = jsonGeometry["polylist"];
		_numBasePolygons = jSONNode4["count"].AsInt;
		_basePolyList = new MeshPoly[_numBasePolygons];
		_UVPolyList = new MeshPoly[_numBasePolygons];
		num = 0;
		foreach (JSONNode item3 in jSONNode4["values"].AsArray)
		{
			int asInt = item3[1].AsInt;
			int asInt2 = item3[2].AsInt;
			int asInt3 = item3[3].AsInt;
			int asInt4 = item3[4].AsInt;
			MeshPoly meshPoly = new MeshPoly();
			if (item3.Count == 6)
			{
				int asInt5 = item3[5].AsInt;
				meshPoly.vertices = new int[4];
				meshPoly.vertices[3] = asInt5;
			}
			else
			{
				meshPoly.vertices = new int[3];
			}
			meshPoly.materialNum = asInt;
			meshPoly.vertices[0] = asInt2;
			meshPoly.vertices[1] = asInt3;
			meshPoly.vertices[2] = asInt4;
			_basePolyList[num] = meshPoly;
			MeshPoly meshPoly2 = new MeshPoly();
			meshPoly2.materialNum = meshPoly.materialNum;
			meshPoly2.vertices = new int[meshPoly.vertices.Length];
			for (int i = 0; i < meshPoly.vertices.Length; i++)
			{
				meshPoly2.vertices[i] = meshPoly.vertices[i];
			}
			_UVPolyList[num] = meshPoly2;
			num++;
		}
		num = 0;
		if (jsonGeometry["graft"] != null)
		{
			DAZMeshGraft dAZMeshGraft = new DAZMeshGraft();
			JSONNode jSONNode6 = jsonGeometry["graft"]["vertex_pairs"];
			if (jSONNode6 != null)
			{
				int asInt6 = jSONNode6["count"].AsInt;
				dAZMeshGraft.vertexPairs = new DAZMeshGraftVertexPair[asInt6];
				num = 0;
				foreach (JSONNode item4 in jSONNode6["values"].AsArray)
				{
					DAZMeshGraftVertexPair dAZMeshGraftVertexPair = new DAZMeshGraftVertexPair();
					dAZMeshGraftVertexPair.vertexNum = item4[0].AsInt;
					dAZMeshGraftVertexPair.graftToVertexNum = item4[1].AsInt;
					dAZMeshGraft.vertexPairs[num] = dAZMeshGraftVertexPair;
					num++;
				}
				JSONNode jSONNode8 = jsonGeometry["graft"]["hidden_polys"];
				int asInt7 = jSONNode8["count"].AsInt;
				dAZMeshGraft.hiddenPolys = new int[asInt7];
				num = 0;
				foreach (JSONNode item5 in jSONNode8["values"].AsArray)
				{
					dAZMeshGraft.hiddenPolys[num] = item5.AsInt;
					num++;
				}
				meshGraft = dAZMeshGraft;
			}
		}
		if (uvmap.uvs == null)
		{
			_OrigUV = new Vector2[_numBaseVertices];
			_UV = new Vector2[_numBaseVertices];
			_numUVVertices = _numBaseVertices;
			_UVVertices = new Vector3[_numUVVertices];
			_morphedUVVertices = new Vector3[_numUVVertices];
			_baseVerticesToUVVertices = new DAZVertexMap[0];
			for (int j = 0; j < _baseVertices.Length; j++)
			{
				_UVVertices[j] = _baseVertices[j];
				_morphedUVVertices[j] = _baseVertices[j];
			}
		}
		else
		{
			_OrigUV = uvmap.uvs;
			_UV = (Vector2[])uvmap.uvs.Clone();
			_numUVVertices = uvmap.uvs.Length;
			_UVVertices = new Vector3[_numUVVertices];
			_morphedUVVertices = new Vector3[_numUVVertices];
			_baseVerticesToUVVertices = new DAZVertexMap[_numUVVertices - _numBaseVertices];
			for (int k = 0; k < _baseVertices.Length; k++)
			{
				_UVVertices[k] = _baseVertices[k];
				_morphedUVVertices[k] = _baseVertices[k];
			}
			Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
			int num2 = 0;
			DAZVertexMap[] vertexMap = uvmap.vertexMap;
			foreach (DAZVertexMap dAZVertexMap in vertexMap)
			{
				_UVVertices[dAZVertexMap.tovert] = _baseVertices[dAZVertexMap.fromvert];
				_morphedUVVertices[dAZVertexMap.tovert] = _baseVertices[dAZVertexMap.fromvert];
				bool value2;
				if (!dictionary.TryGetValue(dAZVertexMap.tovert, out value2))
				{
					_baseVerticesToUVVertices[num2] = dAZVertexMap;
					dictionary.Add(dAZVertexMap.tovert, true);
					num2++;
				}
				MeshPoly meshPoly3 = _UVPolyList[dAZVertexMap.polyindex];
				for (int m = 0; m < meshPoly3.vertices.Length; m++)
				{
					if (meshPoly3.vertices[m] == dAZVertexMap.fromvert)
					{
						meshPoly3.vertices[m] = dAZVertexMap.tovert;
					}
				}
			}
		}
		DeriveMeshes();
		InitMeshFilterAndRenderer();
	}

	public void ReduceMeshToSingleMaterial(int materialNum)
	{
		if (materialNum < 0 || materialNum >= _numMaterials)
		{
			return;
		}
		DeriveMeshes();
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		int num = _baseMaterialVertices[materialNum].Length;
		Vector3[] array = new Vector3[num];
		Vector3[] array2 = baseVertices;
		for (int i = 0; i < num; i++)
		{
			int num2 = _baseMaterialVertices[materialNum][i];
			dictionary.Add(num2, i);
			dictionary2.Add(i, num2);
			array[i] = array2[num2];
		}
		int num3 = 1;
		Material[] array3 = new Material[num3];
		array3[0] = materials[materialNum];
		string[] array4 = new string[num3];
		array4[0] = _materialNames[materialNum];
		Material[] array5 = new Material[num3];
		if (materialsPass1 != null && materialNum < materialsPass1.Length)
		{
			array5[0] = materialsPass1[materialNum];
		}
		Material[] array6 = new Material[num3];
		if (materialsPass2 != null && materialNum < materialsPass2.Length)
		{
			array6[0] = materialsPass2[materialNum];
		}
		bool[] array7 = new bool[num3];
		array7[0] = materialsEnabled[materialNum];
		bool[] array8 = new bool[num3];
		if (materialsPass1Enabled != null && materialNum < materialsPass1Enabled.Length)
		{
			array8[0] = materialsPass1Enabled[materialNum];
		}
		bool[] array9 = new bool[num3];
		if (materialsPass2Enabled != null && materialNum < materialsPass2Enabled.Length)
		{
			array9[0] = materialsPass2Enabled[materialNum];
		}
		bool[] array10 = new bool[num3];
		array10[0] = materialsShadowCastEnabled[materialNum];
		List<MeshPoly> list = new List<MeshPoly>();
		List<MeshPoly> list2 = new List<MeshPoly>();
		List<DAZVertexMap> list3 = new List<DAZVertexMap>();
		int num4 = num;
		int num5 = 0;
		MeshPoly[] array11 = basePolyList;
		MeshPoly[] uVPolyList = UVPolyList;
		for (int j = 0; j < _numBasePolygons; j++)
		{
			MeshPoly meshPoly = array11[j];
			MeshPoly meshPoly2 = uVPolyList[j];
			if (meshPoly.materialNum != materialNum)
			{
				continue;
			}
			MeshPoly meshPoly3 = new MeshPoly();
			MeshPoly meshPoly4 = new MeshPoly();
			meshPoly3.vertices = new int[meshPoly.vertices.Length];
			meshPoly4.vertices = new int[meshPoly.vertices.Length];
			for (int k = 0; k < meshPoly.vertices.Length; k++)
			{
				int value;
				if (dictionary.TryGetValue(meshPoly.vertices[k], out value))
				{
					meshPoly3.vertices[k] = value;
					int value2;
					if (dictionary.TryGetValue(meshPoly2.vertices[k], out value2))
					{
						meshPoly4.vertices[k] = value2;
						continue;
					}
					meshPoly4.vertices[k] = num4;
					dictionary.Add(meshPoly2.vertices[k], num4);
					dictionary2.Add(num4, meshPoly2.vertices[k]);
					DAZVertexMap dAZVertexMap = new DAZVertexMap();
					dAZVertexMap.polyindex = num5;
					dAZVertexMap.fromvert = value;
					dAZVertexMap.tovert = num4;
					list3.Add(dAZVertexMap);
					num4++;
				}
				else
				{
					Debug.LogError("Could not find vert index " + meshPoly.vertices[k] + " in old vert to new vert map, but it should be there");
				}
			}
			meshPoly3.materialNum = 0;
			list.Add(meshPoly3);
			meshPoly4.materialNum = 0;
			list2.Add(meshPoly4);
			num5++;
		}
		int count = list.Count;
		MeshPoly[] array12 = list.ToArray();
		MeshPoly[] uVPolyList2 = list2.ToArray();
		int num6 = num4;
		Vector3[] array13 = new Vector3[num6];
		Vector3[] array14 = new Vector3[num6];
		for (int l = 0; l < num; l++)
		{
			array13[l] = array[l];
			array14[l] = array[l];
		}
		foreach (DAZVertexMap item in list3)
		{
			array13[item.tovert] = array[item.fromvert];
			array14[item.tovert] = array[item.fromvert];
		}
		Vector2[] array15 = new Vector2[num6];
		Vector2[] origUV = OrigUV;
		for (int m = 0; m < num6; m++)
		{
			int value3;
			if (dictionary2.TryGetValue(m, out value3))
			{
				array15[m] = origUV[value3];
			}
			else
			{
				Debug.LogError("Could not find new vert index " + m + " in newVertToOldVert map, but it should be there");
			}
		}
		_numBaseVertices = num;
		_baseVertices = array;
		_numMaterials = num3;
		_materialNames = array4;
		materials = array3;
		materialsEnabled = array7;
		materialsPass1 = array5;
		materialsPass2 = array6;
		materialsPass1Enabled = array8;
		materialsPass2Enabled = array9;
		materialsShadowCastEnabled = array10;
		_numBasePolygons = count;
		_basePolyList = array12;
		_UVPolyList = uVPolyList2;
		_numUVVertices = num6;
		_UVVertices = array13;
		_morphedUVVertices = array14;
		_baseVerticesToUVVertices = list3.ToArray();
		_OrigUV = array15;
		DeriveMeshes();
		InitMeshFilterAndRenderer();
	}

	public void ReduceMeshToMaterials(List<int> materialNums)
	{
		if (materialNums.Count <= 0 || materialNums[0] < 0 || materialNums[0] >= _numMaterials)
		{
			return;
		}
		DeriveMeshes();
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		int num = 0;
		for (int i = 0; i < _numMaterials; i++)
		{
			if (!materialNums.Contains(i))
			{
				continue;
			}
			for (int j = 0; j < _baseMaterialVertices[i].Length; j++)
			{
				int num2 = _baseMaterialVertices[i][j];
				if (!dictionary.ContainsKey(num2))
				{
					dictionary.Add(num2, num);
					dictionary2.Add(num, num2);
					num++;
				}
			}
		}
		Vector3[] array = new Vector3[num];
		Vector3[] array2 = baseVertices;
		foreach (int key in dictionary.Keys)
		{
			int value;
			if (dictionary.TryGetValue(key, out value))
			{
				array[value] = array2[key];
			}
		}
		int count = materialNums.Count;
		Material[] array3 = new Material[count];
		Material[] array4 = new Material[count];
		Material[] array5 = new Material[count];
		bool[] array6 = new bool[count];
		bool[] array7 = new bool[count];
		bool[] array8 = new bool[count];
		bool[] array9 = new bool[count];
		string[] array10 = new string[count];
		int num3 = 0;
		Dictionary<int, int> dictionary3 = new Dictionary<int, int>();
		for (int k = 0; k < _numMaterials; k++)
		{
			if (materialNums.Contains(k))
			{
				dictionary3.Add(k, num3);
				array3[num3] = materials[k];
				array10[num3] = _materialNames[k];
				if (materialsPass1 != null && k < materialsPass1.Length)
				{
					array4[num3] = materialsPass1[k];
				}
				if (materialsPass2 != null && k < materialsPass2.Length)
				{
					array5[num3] = materialsPass2[k];
				}
				array6[num3] = materialsEnabled[k];
				if (materialsPass1Enabled != null && k < materialsPass1Enabled.Length)
				{
					array7[num3] = materialsPass1Enabled[k];
				}
				if (materialsPass2Enabled != null && k < materialsPass2Enabled.Length)
				{
					array8[num3] = materialsPass2Enabled[k];
				}
				array9[num3] = materialsShadowCastEnabled[k];
				num3++;
			}
		}
		List<MeshPoly> list = new List<MeshPoly>();
		List<MeshPoly> list2 = new List<MeshPoly>();
		List<DAZVertexMap> list3 = new List<DAZVertexMap>();
		int num4 = num;
		int num5 = 0;
		MeshPoly[] array11 = basePolyList;
		MeshPoly[] uVPolyList = UVPolyList;
		for (int l = 0; l < _numBasePolygons; l++)
		{
			MeshPoly meshPoly = array11[l];
			MeshPoly meshPoly2 = uVPolyList[l];
			if (!materialNums.Contains(meshPoly.materialNum))
			{
				continue;
			}
			MeshPoly meshPoly3 = new MeshPoly();
			MeshPoly meshPoly4 = new MeshPoly();
			meshPoly3.vertices = new int[meshPoly.vertices.Length];
			meshPoly4.vertices = new int[meshPoly.vertices.Length];
			for (int m = 0; m < meshPoly.vertices.Length; m++)
			{
				int value2;
				if (dictionary.TryGetValue(meshPoly.vertices[m], out value2))
				{
					meshPoly3.vertices[m] = value2;
					int value3;
					if (dictionary.TryGetValue(meshPoly2.vertices[m], out value3))
					{
						meshPoly4.vertices[m] = value3;
						continue;
					}
					meshPoly4.vertices[m] = num4;
					dictionary.Add(meshPoly2.vertices[m], num4);
					dictionary2.Add(num4, meshPoly2.vertices[m]);
					DAZVertexMap dAZVertexMap = new DAZVertexMap();
					dAZVertexMap.polyindex = num5;
					dAZVertexMap.fromvert = value2;
					dAZVertexMap.tovert = num4;
					list3.Add(dAZVertexMap);
					num4++;
				}
				else
				{
					Debug.LogError("Could not find vert index " + meshPoly.vertices[m] + " in old vert to new vert map, but it should be there");
				}
			}
			int value4;
			if (dictionary3.TryGetValue(meshPoly.materialNum, out value4))
			{
				meshPoly3.materialNum = value4;
				list.Add(meshPoly3);
				meshPoly4.materialNum = value4;
				list2.Add(meshPoly4);
				num5++;
			}
		}
		int count2 = list.Count;
		MeshPoly[] array12 = list.ToArray();
		MeshPoly[] uVPolyList2 = list2.ToArray();
		int num6 = num4;
		Vector3[] array13 = new Vector3[num6];
		Vector3[] array14 = new Vector3[num6];
		for (int n = 0; n < num; n++)
		{
			array13[n] = array[n];
			array14[n] = array[n];
		}
		foreach (DAZVertexMap item in list3)
		{
			array13[item.tovert] = array[item.fromvert];
			array14[item.tovert] = array[item.fromvert];
		}
		Vector2[] array15 = new Vector2[num6];
		Vector2[] origUV = OrigUV;
		for (int num7 = 0; num7 < num6; num7++)
		{
			int value5;
			if (dictionary2.TryGetValue(num7, out value5))
			{
				array15[num7] = origUV[value5];
			}
			else
			{
				Debug.LogError("Could not find new vert index " + num7 + " in newVertToOldVert map, but it should be there");
			}
		}
		_numBaseVertices = num;
		_baseVertices = array;
		_numMaterials = count;
		_materialNames = array10;
		materials = array3;
		materialsEnabled = array6;
		materialsPass1 = array4;
		materialsPass2 = array5;
		materialsPass1Enabled = array7;
		materialsPass2Enabled = array8;
		materialsShadowCastEnabled = array9;
		_numBasePolygons = count2;
		_basePolyList = array12;
		_UVPolyList = uVPolyList2;
		_numUVVertices = num6;
		_UVVertices = array13;
		_morphedUVVertices = array14;
		_baseVerticesToUVVertices = list3.ToArray();
		_OrigUV = array15;
		DeriveMeshes();
		InitMeshFilterAndRenderer();
	}

	public void ReduceMeshBelowPlane()
	{
		DeriveMeshes();
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		int num = 0;
		List<Vector3> list = new List<Vector3>();
		List<Vector3> list2 = new List<Vector3>();
		Vector3[] array = baseVertices;
		for (int i = 0; i < _numBaseVertices; i++)
		{
			Vector3 vector = array[i];
			float num2 = Vector3.Dot(reduceUp, vector - reducePoint);
			if (num2 >= 0f)
			{
				dictionary.Add(i, num);
				dictionary2.Add(num, i);
				list.Add(vector);
				list2.Add(vector);
				num++;
			}
		}
		int num3 = num;
		Vector3[] uVVertices = UVVertices;
		for (int j = _numBaseVertices; j < _numUVVertices; j++)
		{
			Vector3 vector2 = uVVertices[j];
			float num4 = Vector3.Dot(reduceUp, vector2 - reducePoint);
			if (num4 >= 0f)
			{
				dictionary.Add(j, num);
				dictionary2.Add(num, j);
				list2.Add(vector2);
				num++;
			}
		}
		Vector3[] array2 = list.ToArray();
		Vector3[] array3 = list2.ToArray();
		Vector3[] array4 = (Vector3[])array3.Clone();
		List<MeshPoly> list3 = new List<MeshPoly>();
		List<MeshPoly> list4 = new List<MeshPoly>();
		int num5 = 0;
		List<DAZVertexMap> list5 = new List<DAZVertexMap>();
		MeshPoly[] array5 = basePolyList;
		MeshPoly[] uVPolyList = UVPolyList;
		for (int k = 0; k < _numBasePolygons; k++)
		{
			MeshPoly meshPoly = array5[k];
			MeshPoly meshPoly2 = uVPolyList[k];
			bool flag = true;
			MeshPoly meshPoly3 = new MeshPoly();
			MeshPoly meshPoly4 = new MeshPoly();
			meshPoly3.materialNum = meshPoly.materialNum;
			meshPoly4.materialNum = meshPoly2.materialNum;
			meshPoly3.vertices = new int[meshPoly.vertices.Length];
			meshPoly4.vertices = new int[meshPoly.vertices.Length];
			for (int l = 0; l < meshPoly.vertices.Length; l++)
			{
				int value;
				if (dictionary.TryGetValue(meshPoly.vertices[l], out value))
				{
					meshPoly3.vertices[l] = value;
					int value2;
					if (dictionary.TryGetValue(meshPoly2.vertices[l], out value2))
					{
						meshPoly4.vertices[l] = value2;
					}
					continue;
				}
				flag = false;
				break;
			}
			if (!flag)
			{
				continue;
			}
			list3.Add(meshPoly3);
			list4.Add(meshPoly4);
			for (int m = 0; m < meshPoly3.vertices.Length; m++)
			{
				int num6 = meshPoly3.vertices[m];
				int num7 = meshPoly4.vertices[m];
				if (num6 != num7)
				{
					DAZVertexMap dAZVertexMap = new DAZVertexMap();
					dAZVertexMap.fromvert = num6;
					dAZVertexMap.tovert = num7;
					dAZVertexMap.polyindex = num5;
					list5.Add(dAZVertexMap);
				}
			}
			num5++;
		}
		int count = list3.Count;
		MeshPoly[] array6 = list3.ToArray();
		MeshPoly[] uVPolyList2 = list4.ToArray();
		Vector2[] array7 = new Vector2[num];
		Vector2[] origUV = OrigUV;
		for (int n = 0; n < num; n++)
		{
			int value3;
			if (dictionary2.TryGetValue(n, out value3))
			{
				array7[n] = origUV[value3];
			}
			else
			{
				Debug.LogError("Could not find new vert index " + n + " in newVertToOldVert map, but it should be there");
			}
		}
		_numBaseVertices = num3;
		_baseVertices = array2;
		_numBasePolygons = count;
		_basePolyList = array6;
		_UVPolyList = uVPolyList2;
		_numUVVertices = num;
		_UVVertices = array3;
		_morphedUVVertices = array4;
		_baseVerticesToUVVertices = list5.ToArray();
		_OrigUV = array7;
		_meshData = null;
		DeriveMeshes();
		InitMeshFilterAndRenderer();
	}

	protected void _updateDuplicateMorphedUVVertices()
	{
		if (baseVerticesToUVVertices != null)
		{
			DAZVertexMap[] array = baseVerticesToUVVertices;
			foreach (DAZVertexMap dAZVertexMap in array)
			{
				_morphedUVVertices[dAZVertexMap.tovert] = _morphedUVVertices[dAZVertexMap.fromvert];
				_visibleMorphedUVVertices[dAZVertexMap.tovert] = _visibleMorphedUVVertices[dAZVertexMap.fromvert];
			}
		}
	}

	protected void _updateDuplicateSmoothedMorphedUVVertices()
	{
		if (baseVerticesToUVVertices != null)
		{
			DAZVertexMap[] array = baseVerticesToUVVertices;
			foreach (DAZVertexMap dAZVertexMap in array)
			{
				_smoothedMorphedUVVertices[dAZVertexMap.tovert] = _smoothedMorphedUVVertices[dAZVertexMap.fromvert];
				_visibleMorphedUVVertices[dAZVertexMap.tovert] = _visibleMorphedUVVertices[dAZVertexMap.fromvert];
			}
		}
	}

	protected void _updateDuplicateMorphedUVNormals()
	{
		if (baseVerticesToUVVertices != null)
		{
			DAZVertexMap[] array = baseVerticesToUVVertices;
			foreach (DAZVertexMap dAZVertexMap in array)
			{
				_morphedUVNormals[dAZVertexMap.tovert] = _morphedUVNormals[dAZVertexMap.fromvert];
			}
		}
	}

	public void StartMorph()
	{
		_verticesChangedLastFrame = _verticesChangedThisFrame;
		_visibleNonPoseVerticesChangedLastFrame = _visibleNonPoseVerticesChangedThisFrame;
		_verticesChangedThisFrame = false;
		_visibleNonPoseVerticesChangedThisFrame = false;
	}

	public void ApplyMorphVertices(bool visibleNonPoseVerticesChanged)
	{
		_verticesChangedThisFrame = true;
		_visibleNonPoseVerticesChangedThisFrame = visibleNonPoseVerticesChanged;
		if (useSmoothing)
		{
			InitMeshSmooth();
			meshSmooth.LaplacianSmooth(_morphedUVVertices, _smoothedMorphedUVVertices);
			meshSmooth.HCCorrection(_morphedUVVertices, _smoothedMorphedUVVertices, 0.5f);
			_updateDuplicateSmoothedMorphedUVVertices();
			if (_drawMorphedUVMappedMesh || !Application.isPlaying)
			{
				_morphedUVMappedMesh.vertices = _smoothedMorphedUVVertices;
			}
			_triggerNormalAndTangentRecalc();
		}
		else
		{
			_updateDuplicateMorphedUVVertices();
			if (_drawMorphedUVMappedMesh || !Application.isPlaying)
			{
				_morphedUVMappedMesh.vertices = _morphedUVVertices;
			}
			_triggerNormalAndTangentRecalc();
		}
		if (_drawMorphedBaseMesh)
		{
			_morphedBaseMesh.vertices = _morphedBaseVertices;
		}
		if (_drawVisibleMorphedUVMappedMesh || !Application.isPlaying)
		{
			_visibleMorphedUVMappedMesh.vertices = _visibleMorphedUVVertices;
		}
	}

	public void ReInitMorphs()
	{
		if (morphBank != null)
		{
			Init();
			morphBank.ReInit();
		}
	}

	public void ResetMorphs()
	{
		if (morphBank != null)
		{
			morphBank.ResetMorphs();
		}
	}

	public void ApplyMorphs(bool force = false)
	{
		if (morphBank != null)
		{
			morphBank.ApplyMorphs(force);
		}
	}

	public void ResetMorphedVerticesToOffset()
	{
		Vector3[] array = baseVertices;
		Vector3[] uVVertices = UVVertices;
		if (vertexNormalOffset != 0f)
		{
			for (int i = 0; i < _morphedBaseVertices.Length; i++)
			{
				_morphedBaseVertices[i] = array[i] + _morphedBaseNormals[i] * vertexNormalOffset + vertexOffset;
			}
			for (int j = 0; j < _morphedUVVertices.Length; j++)
			{
				_morphedUVVertices[j] = uVVertices[j] + _morphedUVNormals[j] * vertexNormalOffset + vertexOffset;
				_visibleMorphedUVVertices[j] = _morphedUVVertices[j];
			}
		}
		else
		{
			for (int k = 0; k < _morphedBaseVertices.Length; k++)
			{
				_morphedBaseVertices[k] = array[k] + vertexOffset;
			}
			for (int l = 0; l < _morphedUVVertices.Length; l++)
			{
				_morphedUVVertices[l] = uVVertices[l] + vertexOffset;
				_visibleMorphedUVVertices[l] = _morphedUVVertices[l];
			}
		}
		if (_morphedBaseMesh != null)
		{
			_morphedBaseMesh.vertices = _morphedBaseVertices;
		}
		if (_morphedUVMappedMesh != null)
		{
			_morphedUVMappedMesh.vertices = _morphedUVVertices;
		}
		if (_visibleMorphedUVMappedMesh != null)
		{
			_visibleMorphedUVMappedMesh.vertices = _visibleMorphedUVVertices;
		}
	}

	public void ResetMorphedVertices()
	{
		if (!_wasInit)
		{
			return;
		}
		_verticesChangedThisFrame = true;
		_visibleNonPoseVerticesChangedThisFrame = true;
		Vector3[] uVVertices = UVVertices;
		if (vertexNormalOffset == 0f)
		{
			for (int i = 0; i < _numUVVertices; i++)
			{
				_morphedUVVertices[i] = uVVertices[i] + vertexOffset;
				_visibleMorphedUVVertices[i] = _morphedUVVertices[i];
			}
		}
		else
		{
			for (int j = 0; j < _numUVVertices; j++)
			{
				_morphedUVVertices[j] = uVVertices[j] + _UVNormals[j] * vertexNormalOffset + vertexOffset;
				_visibleMorphedUVVertices[j] = _morphedUVVertices[j];
			}
		}
		if (_morphedUVMappedMesh != null && _morphedUVVertices != null)
		{
			_morphedUVMappedMesh.vertices = _morphedUVVertices;
		}
		if (_visibleMorphedUVMappedMesh != null && _visibleMorphedUVVertices != null)
		{
			_visibleMorphedUVMappedMesh.vertices = _visibleMorphedUVVertices;
		}
	}

	public void CreateFolderIfNeeded(string filename)
	{
		string directoryName = Path.GetDirectoryName(filename);
		if (!Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
	}

	public void SaveMeshAsset(bool overwrite = false)
	{
	}

	public void SyncMeshRendererMaterials()
	{
		if (!_createMeshFilterAndRenderer || (_meshRendererMaterialsWasInit && Application.isPlaying))
		{
			return;
		}
		MeshRenderer component = GetComponent<MeshRenderer>();
		if (!(component != null) || materials == null)
		{
			return;
		}
		_meshRendererMaterialsWasInit = true;
		Material[] array = component.sharedMaterials;
		if (array.Length != materials.Length)
		{
			array = new Material[materials.Length];
		}
		for (int i = 0; i < materials.Length; i++)
		{
			if (materialsEnabled[i])
			{
				if (useSimpleMaterial)
				{
					array[i] = simpleMaterial;
				}
				else
				{
					array[i] = materials[i];
				}
			}
			else
			{
				array[i] = discardMaterial;
			}
		}
		component.sharedMaterials = array;
	}

	protected void InitMeshFilterAndRenderer()
	{
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
		if (meshFilter == null && _createMeshFilterAndRenderer)
		{
			meshFilter = base.gameObject.AddComponent<MeshFilter>();
		}
		if (meshRenderer == null && _createMeshFilterAndRenderer)
		{
			meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
		}
		if (meshFilter != null && meshRenderer != null && meshRenderer.enabled)
		{
			if (meshFilter.sharedMesh != morphedUVMappedMesh)
			{
				meshFilter.sharedMesh = morphedUVMappedMesh;
			}
			SyncMeshRendererMaterials();
		}
	}

	public void DrawMorphedUVMappedMesh(Matrix4x4 m)
	{
		MeshFilter component = GetComponent<MeshFilter>();
		MeshRenderer component2 = GetComponent<MeshRenderer>();
		if (component != null && component2 != null && component2.enabled)
		{
			if (component.sharedMesh != morphedUVMappedMesh)
			{
				component.sharedMesh = morphedUVMappedMesh;
			}
			return;
		}
		if (use2PassMaterials)
		{
			for (int i = 0; i < morphedUVMappedMesh.subMeshCount; i++)
			{
				Material material = null;
				if (materialsPass1 != null)
				{
					material = materialsPass1[i];
				}
				if (material == null || useSimpleMaterial)
				{
					material = simpleMaterial;
				}
				if (material != null && materialsPass1Enabled[i])
				{
					Graphics.DrawMesh(morphedUVMappedMesh, m, material, 0, null, i, null, castShadows && materialsShadowCastEnabled[i], receiveShadows);
				}
			}
			for (int j = 0; j < morphedUVMappedMesh.subMeshCount; j++)
			{
				Material material2 = null;
				if (materialsPass2 != null)
				{
					material2 = materialsPass2[j];
				}
				if (material2 == null || useSimpleMaterial)
				{
					material2 = simpleMaterial;
				}
				if (material2 != null && materialsPass2Enabled[j])
				{
					Graphics.DrawMesh(morphedUVMappedMesh, m, material2, 0, null, j, null, castShadows && materialsShadowCastEnabled[j], receiveShadows);
				}
			}
			return;
		}
		for (int k = 0; k < morphedUVMappedMesh.subMeshCount; k++)
		{
			Material material3 = null;
			if (materials != null)
			{
				material3 = materials[k];
			}
			if (material3 == null || useSimpleMaterial)
			{
				material3 = simpleMaterial;
			}
			if (material3 != null && materialsEnabled[k])
			{
				Graphics.DrawMesh(morphedUVMappedMesh, m, material3, 0, null, k, null, castShadows && materialsShadowCastEnabled[k], receiveShadows);
			}
		}
	}

	public void Draw()
	{
		if (_renderSuspend || (!drawBaseMesh && !drawMorphedBaseMesh && !drawUVMappedMesh && !_drawMorphedUVMappedMesh && !_drawVisibleMorphedUVMappedMesh && !debugGrafting && (!drawInEditorWhenNotPlaying || Application.isPlaying)))
		{
			return;
		}
		Matrix4x4 matrix4x;
		if (Application.isPlaying && drawFromBone != null)
		{
			if (useDrawOffset)
			{
				Quaternion q = Quaternion.Euler(drawOffsetRotation);
				drawOffsetMatrix.SetTRS(drawOffset, q, drawOffsetScale * drawOffsetOverallScale);
				drawOffsetOriginMatrix1.SetTRS(drawOffsetOrigin, identityQuaternion, oneVector);
				drawOffsetOriginMatrix2.SetTRS(-drawOffsetOrigin, identityQuaternion, oneVector);
				matrix4x = drawFromBone.transform.localToWorldMatrix * drawFromBone.nonMorphedWorldToLocalMatrix * drawOffsetOriginMatrix1 * drawOffsetMatrix * drawOffsetOriginMatrix2;
			}
			else
			{
				matrix4x = drawFromBone.transform.localToWorldMatrix * drawFromBone.nonMorphedWorldToLocalMatrix;
			}
		}
		else if (useDrawOffset)
		{
			Quaternion q2 = Quaternion.Euler(drawOffsetRotation);
			Vector3 position = base.transform.TransformPoint(drawOffsetOrigin);
			MyDebug.DrawWireCube(position, 0.01f, Color.blue);
			drawOffsetMatrix.SetTRS(drawOffset, q2, drawOffsetScale * drawOffsetOverallScale);
			drawOffsetOriginMatrix1.SetTRS(drawOffsetOrigin, identityQuaternion, oneVector);
			drawOffsetOriginMatrix2.SetTRS(-drawOffsetOrigin, identityQuaternion, oneVector);
			matrix4x = base.transform.localToWorldMatrix * drawOffsetOriginMatrix1 * drawOffsetMatrix * drawOffsetOriginMatrix2;
		}
		else
		{
			matrix4x = base.transform.localToWorldMatrix;
		}
		if (delayDisplayFrameCount == 2)
		{
			Matrix4x4 matrix4x2 = lastMatrix2;
			lastMatrix2 = lastMatrix1;
			lastMatrix1 = matrix4x;
			matrix4x = matrix4x2;
		}
		else if (delayDisplayFrameCount == 1)
		{
			Matrix4x4 matrix4x3 = lastMatrix1;
			lastMatrix1 = matrix4x;
			matrix4x = matrix4x3;
		}
		if (drawBaseMesh && simpleMaterial != null)
		{
			for (int i = 0; i < baseMesh.subMeshCount; i++)
			{
				Graphics.DrawMesh(baseMesh, matrix4x, simpleMaterial, 0, null, i, null, castShadows, receiveShadows);
			}
		}
		if (drawMorphedBaseMesh && simpleMaterial != null)
		{
			for (int j = 0; j < _morphedBaseMesh.subMeshCount; j++)
			{
				Graphics.DrawMesh(_morphedBaseMesh, matrix4x, simpleMaterial, 0, null, j, null, castShadows, receiveShadows);
			}
		}
		if (debugGrafting && meshGraft != null && (bool)graftTo)
		{
			Vector3[] normals = baseMesh.normals;
			DAZMeshGraftVertexPair[] vertexPairs = meshGraft.vertexPairs;
			foreach (DAZMeshGraftVertexPair dAZMeshGraftVertexPair in vertexPairs)
			{
				Vector3 point = graftTo.morphedUVVertices[dAZMeshGraftVertexPair.graftToVertexNum];
				Vector3 vector = matrix4x.MultiplyPoint(point);
				Vector3 end = vector + normals[dAZMeshGraftVertexPair.vertexNum] * 0.01f;
				Debug.DrawLine(vector, end, Color.red);
			}
		}
		if (drawUVMappedMesh)
		{
			for (int l = 0; l < uvMappedMesh.subMeshCount; l++)
			{
				Material material = null;
				if (materials != null)
				{
					material = materials[l];
				}
				if (material == null || useSimpleMaterial)
				{
					material = simpleMaterial;
				}
				if (material != null && materialsEnabled[l])
				{
					Graphics.DrawMesh(uvMappedMesh, matrix4x, material, 0, null, l, null, castShadows && materialsShadowCastEnabled[l], receiveShadows);
				}
			}
		}
		if (_drawMorphedUVMappedMesh || (drawInEditorWhenNotPlaying && !Application.isPlaying))
		{
			DrawMorphedUVMappedMesh(matrix4x);
		}
		if (!_drawVisibleMorphedUVMappedMesh)
		{
			return;
		}
		for (int m = 0; m < _visibleMorphedUVMappedMesh.subMeshCount; m++)
		{
			Material material2 = null;
			if (materials != null)
			{
				material2 = materials[m];
			}
			if (material2 == null || useSimpleMaterial)
			{
				material2 = simpleMaterial;
			}
			if (material2 != null && materialsEnabled[m])
			{
				Graphics.DrawMesh(_visibleMorphedUVMappedMesh, matrix4x, material2, 0, null, m, null, castShadows && materialsShadowCastEnabled[m], receiveShadows);
			}
		}
	}

	public virtual void InitMaterials()
	{
		if (!Application.isPlaying || _materialsWereInit)
		{
			return;
		}
		_materialsWereInit = true;
		if (materials == null)
		{
			return;
		}
		for (int i = 0; i < materials.Length; i++)
		{
			if (materials[i] != null)
			{
				Material material = new Material(materials[i]);
				RegisterAllocatedObject(material);
				materials[i] = material;
			}
		}
		if (materialsShadowCastEnabled == null || materialsShadowCastEnabled.Length != materials.Length)
		{
			materialsShadowCastEnabled = new bool[materials.Length];
			for (int j = 0; j < materials.Length; j++)
			{
				materialsShadowCastEnabled[j] = true;
			}
		}
	}

	public virtual void Init()
	{
		if (!_wasInit && baseVertices != null)
		{
			_wasInit = true;
			DeriveMeshes();
			InitMeshFilterAndRenderer();
			InitCollider();
		}
	}

	public void SetMorphedUVMeshVertices(Vector3[] verts)
	{
		_morphedUVMappedMesh.vertices = verts;
	}

	public void ConnectMorphBank()
	{
		if (morphBank != null)
		{
			morphBank.connectedMesh = this;
		}
	}

	private void LateUpdate()
	{
		if (debug && debugVertex < _numUVVertices)
		{
			MyDebug.DrawWireCube(_morphedUVVertices[debugVertex], 0.01f, Color.blue);
		}
		Draw();
	}

	private void OnDisable()
	{
		if (!Application.isPlaying)
		{
			_wasInit = false;
		}
	}

	private void OnEnable()
	{
		Init();
		ConnectMorphBank();
	}

	private void Awake()
	{
		InitMaterials();
	}

	private void Start()
	{
		Init();
	}
}
