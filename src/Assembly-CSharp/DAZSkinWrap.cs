using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Skinner.Scripts.Providers;
using MVR.FileManagement;
using MeshVR;
using UnityEngine;

[ExecuteInEditMode]
public class DAZSkinWrap : PreCalcMeshProvider, IBinaryStorable, RenderSuspend
{
	protected struct Triangle
	{
		public int vert1;

		public int vert2;

		public int vert3;
	}

	public static bool staticDraw;

	protected List<UnityEngine.Object> allocatedObjects;

	private GpuBuffer<Matrix4x4> _ToWorldMatricesBuffer;

	private GpuBuffer<Vector3> _PreCalculatedVerticesBuffer;

	private GpuBuffer<Vector3> _NormalsBuffer;

	protected bool _updateDrawDisabled;

	protected bool _renderSuspend;

	protected DAZSkinWrapStore startingWrapStore;

	public DAZSkinWrapStore wrapStore;

	public string wrapName = "Normal";

	public bool draw;

	public bool debug;

	public int debugStartVert;

	public int debugStopVert;

	public float debugTan1;

	public float debugTan2;

	public float debugSize = 0.005f;

	public DAZMesh dazMesh;

	public Transform skinTransform;

	public DAZSkinV2 skin;

	public bool forceRawSkinVerts = true;

	[NonSerialized]
	public float defaultAdditionalThicknessMultiplier;

	public float additionalThicknessMultiplier = 0.001f;

	[NonSerialized]
	public float defaultSurfaceOffset;

	public float surfaceOffset = 0.0003f;

	public int smoothOuterLoops = 1;

	public int laplacianSmoothPasses = 1;

	public float laplacianSmoothBeta = 0.5f;

	public int springSmoothPasses;

	public float springSmoothFactor = 0.2f;

	public bool useSpring2;

	public float spring2SmoothFactor = 1f;

	public bool moveToSurface = true;

	public float moveToSurfaceOffset = 0.0003f;

	public AutoCollider autoColliderToEnable;

	public AutoCollider autoColliderToDisable;

	public bool forceCPU;

	protected Vector3[] _verts1;

	protected Vector3[] _verts2;

	protected Vector3[] _drawVerts;

	protected int _wrapProgress;

	public Transform morphCopyFromTransform;

	public DAZSkinWrap morphCopyFrom;

	[SerializeField]
	protected List<string> _usedMorphNames;

	[SerializeField]
	protected List<float> _usedMorphValues;

	public int vertexIndexLimit = -1;

	public bool wrapCheckNormals;

	public bool wrapToSkinnedVertices;

	public bool wrapToMorphedVertices;

	public bool wrapToDisabledMaterials;

	protected float threadedProgress;

	protected float threadedCount;

	protected bool killThread;

	public string assetSavePath = "Assets/VaMAssets/Generated/";

	protected Thread wrapThread;

	protected bool appIsPlayingThreaded;

	protected bool isWrapping;

	protected string wrapStatus;

	protected MeshSmooth meshSmooth;

	protected MeshSmoothGPU meshSmoothGPU;

	protected Mesh mesh;

	protected Mesh startMesh;

	protected bool meshWasInit;

	protected bool startMeshWasInit;

	protected const int vertGroupSize = 256;

	protected int numVertThreadGroups;

	public ComputeShader GPUSkinWrapper;

	public ComputeShader GPUMeshCompute;

	protected ComputeBuffer _wrapVerticesBuffer;

	protected ComputeBuffer _verticesBuffer1;

	protected ComputeBuffer _verticesBuffer2;

	protected ComputeBuffer _originalVerticesBuffer;

	protected ComputeBuffer _matricesBuffer;

	protected ComputeBuffer _drawVerticesBuffer;

	protected ComputeBuffer _delayedVertsBuffer;

	protected ComputeBuffer _delayedNormalsBuffer;

	protected ComputeBuffer _delayedTangentsBuffer;

	protected MapVerticesGPU mapVerticesGPU;

	protected int _skinWrapKernel;

	protected int _skinWrapCalcChangeMatricesKernel;

	protected int _copyKernel;

	protected int _copyTangentsKernel;

	protected int _nullVertexIndex;

	protected int[] numSubsetVertThreadGroups;

	public bool GPUuseSimpleMaterial;

	public Material GPUsimpleMaterial;

	public Material[] GPUmaterials;

	public Texture2D[] simTextures;

	public bool GPUAutoSwapShader = true;

	public bool onlyUpdateEnabledMaterials = true;

	public bool[] materialsEnabled;

	[SerializeField]
	protected int _numMaterials;

	[SerializeField]
	protected string[] _materialNames;

	public bool recalculateNormals = true;

	public bool recalculateTangents = true;

	protected RecalculateNormalsGPU originalRecalcNormalsGPU;

	protected RecalculateNormalsGPU recalcNormalsGPU;

	protected RecalculateTangentsGPU originalRecalcTangentsGPU;

	protected RecalculateTangentsGPU recalcTangentsGPU;

	protected ComputeBuffer _originalNormalsBuffer;

	protected ComputeBuffer _normalsBuffer;

	protected ComputeBuffer _originalTangentsBuffer;

	protected ComputeBuffer _tangentsBuffer;

	protected ComputeBuffer _originalSurfaceNormalsBuffer;

	protected ComputeBuffer _surfaceNormalsBuffer;

	protected bool currentMoveToSurface;

	protected float currentMoveToSurfaceOffset;

	protected List<int> usedSkinVerts;

	protected Dictionary<int, ComputeBuffer> materialVertsBuffers;

	protected Dictionary<int, ComputeBuffer> materialNormalsBuffers;

	protected Dictionary<int, ComputeBuffer> materialTangentsBuffers;

	protected bool _materialsWereInit;

	public override Mesh Mesh
	{
		get
		{
			InitMesh();
			return mesh;
		}
	}

	public override Mesh BaseMesh
	{
		get
		{
			InitMesh();
			if (dazMesh != null)
			{
				return dazMesh.baseMesh;
			}
			return null;
		}
	}

	public override Mesh MeshForImport
	{
		get
		{
			InitStartMesh();
			return startMesh;
		}
	}

	public override GpuBuffer<Matrix4x4> ToWorldMatricesBuffer
	{
		get
		{
			InitMesh();
			SkinWrapGPUInit();
			return _ToWorldMatricesBuffer;
		}
	}

	public override GpuBuffer<Vector3> PreCalculatedVerticesBuffer
	{
		get
		{
			InitMesh();
			SkinWrapGPUInit();
			return _PreCalculatedVerticesBuffer;
		}
		protected set
		{
			_PreCalculatedVerticesBuffer = value;
		}
	}

	public override GpuBuffer<Vector3> NormalsBuffer
	{
		get
		{
			InitMesh();
			SkinWrapGPUInit();
			return _NormalsBuffer;
		}
		protected set
		{
			_NormalsBuffer = value;
		}
	}

	public override Color[] VertexSimColors
	{
		get
		{
			InitMesh();
			Color[] array = new Color[dazMesh.numUVVertices];
			if (simTextures != null)
			{
				Vector2[] uV = dazMesh.UV;
				MeshPoly[] uVPolyList = dazMesh.UVPolyList;
				foreach (MeshPoly meshPoly in uVPolyList)
				{
					if (simTextures.Length > meshPoly.materialNum && simTextures[meshPoly.materialNum] != null)
					{
						for (int j = 0; j < meshPoly.vertices.Length; j++)
						{
							int num = meshPoly.vertices[j];
							Vector2 vector = uV[num];
							array[num] = simTextures[meshPoly.materialNum].GetPixelBilinear(vector.x, vector.y);
						}
					}
				}
			}
			return array;
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

	public int wrapProgress => _wrapProgress;

	public List<string> usedMorphNames => _usedMorphNames;

	public List<float> usedMorphValue => _usedMorphValues;

	public bool IsWrapping => isWrapping;

	public string WrapStatus => wrapStatus;

	public int numMaterials => _numMaterials;

	public string[] materialNames => _materialNames;

	protected void RegisterAllocatedObject(UnityEngine.Object o)
	{
		if (Application.isPlaying)
		{
			if (allocatedObjects == null)
			{
				allocatedObjects = new List<UnityEngine.Object>();
			}
			allocatedObjects.Add(o);
		}
	}

	protected void DestroyAllocatedObjects()
	{
		if (!Application.isPlaying || allocatedObjects == null)
		{
			return;
		}
		foreach (UnityEngine.Object allocatedObject in allocatedObjects)
		{
			UnityEngine.Object.Destroy(allocatedObject);
		}
	}

	protected void RunNormalTangentRecalc()
	{
		if (recalculateNormals && recalcNormalsGPU != null)
		{
			recalcNormalsGPU.RecalculateNormals(_drawVerticesBuffer);
		}
		if (recalculateTangents && recalcTangentsGPU != null)
		{
			recalcTangentsGPU.RecalculateTangents(_drawVerticesBuffer, _normalsBuffer);
		}
	}

	public override void Dispatch()
	{
		UpdateVertsGPU(provideToWorldMatrices);
		if (_drawVerticesBuffer != null && _delayedVertsBuffer != null)
		{
			GPUSkinWrapper.SetBuffer(_copyKernel, "inVerts", _drawVerticesBuffer);
			GPUSkinWrapper.SetBuffer(_copyKernel, "outVerts", _delayedVertsBuffer);
			GPUSkinWrapper.Dispatch(_copyKernel, numVertThreadGroups, 1, 1);
			if (provideToWorldMatrices)
			{
				GPUSkinWrapper.SetBuffer(_skinWrapCalcChangeMatricesKernel, "originalPositions", _originalVerticesBuffer);
				GPUSkinWrapper.SetBuffer(_skinWrapCalcChangeMatricesKernel, "originalNormals", _originalNormalsBuffer);
				GPUSkinWrapper.SetBuffer(_skinWrapCalcChangeMatricesKernel, "originalTangents", _originalTangentsBuffer);
				GPUSkinWrapper.SetBuffer(_skinWrapCalcChangeMatricesKernel, "currentPositions", _drawVerticesBuffer);
				GPUSkinWrapper.SetBuffer(_skinWrapCalcChangeMatricesKernel, "currentNormals", _normalsBuffer);
				GPUSkinWrapper.SetBuffer(_skinWrapCalcChangeMatricesKernel, "currentTangents", _tangentsBuffer);
				GPUSkinWrapper.SetBuffer(_skinWrapCalcChangeMatricesKernel, "vertexChangeMatrices", _matricesBuffer);
				GPUSkinWrapper.Dispatch(_skinWrapCalcChangeMatricesKernel, numVertThreadGroups, 1, 1);
			}
		}
	}

	public override void Dispose()
	{
		_updateDrawDisabled = false;
	}

	public override void Stop()
	{
		_updateDrawDisabled = false;
	}

	public override void PostProcessDispatch(ComputeBuffer finalVerts)
	{
		if (!drawInPostProcess || _wrapVerticesBuffer == null)
		{
			return;
		}
		if (!provideToWorldMatrices)
		{
			if (smoothOuterLoops > 0 && (laplacianSmoothPasses > 0 || springSmoothPasses > 0))
			{
				DoSmoothingGPU(finalVerts);
			}
			else
			{
				_drawVerticesBuffer = finalVerts;
			}
			mapVerticesGPU.Map(_drawVerticesBuffer);
			RunNormalTangentRecalc();
		}
		_updateDrawDisabled = true;
		DrawMeshGPU(true);
	}

	public bool LoadFromBinaryReader(BinaryReader binReader)
	{
		try
		{
			string text = binReader.ReadString();
			if (text != "DAZSkinWrap")
			{
				SuperController.LogError("Binary file corrupted. Tried to read DAZSkinWrap in wrong section");
				return false;
			}
			string text2 = binReader.ReadString();
			if (text2 != "1.0")
			{
				SuperController.LogError("DAZSkinWrap schema " + text2 + " is not compatible with this version of software");
				return false;
			}
			wrapName = binReader.ReadString();
			if (wrapStore == null)
			{
				wrapStore = ScriptableObject.CreateInstance<DAZSkinWrapStore>();
			}
			if (!wrapStore.LoadFromBinaryReader(binReader))
			{
				return false;
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Error while loading DAZSkinWrap from binary reader " + ex);
			return false;
		}
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
			SuperController.LogError("Error while loading DAZSkinWrap from binary file " + path + " " + ex);
		}
		return result;
	}

	public bool StoreToBinaryWriter(BinaryWriter binWriter)
	{
		try
		{
			binWriter.Write("DAZSkinWrap");
			binWriter.Write("1.0");
			binWriter.Write(wrapName);
			if (!wrapStore.StoreToBinaryWriter(binWriter))
			{
				return false;
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Error while storing DAZSkinWrap to binary writer " + ex);
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
			SuperController.LogError("Error while storing DAZSkinWrap to binary file " + path + " " + ex);
		}
		return result;
	}

	public void SetUsedMorphsFromCurrentMorphs()
	{
		_usedMorphNames = new List<string>();
		_usedMorphValues = new List<float>();
		if (!(dazMesh != null) || !(dazMesh.morphBank != null))
		{
			return;
		}
		dazMesh.ConnectMorphBank();
		dazMesh.morphBank.Init();
		foreach (DAZMorph morph in dazMesh.morphBank.morphs)
		{
			if (morph.morphValue != 0f)
			{
				_usedMorphNames.Add(morph.morphName);
				_usedMorphValues.Add(morph.morphValue);
			}
		}
	}

	public void SetMeshMorphsFromUsedValue()
	{
		if (!(dazMesh != null) || !(dazMesh.morphBank != null))
		{
			return;
		}
		dazMesh.ConnectMorphBank();
		dazMesh.morphBank.Init();
		foreach (DAZMorph morph2 in dazMesh.morphBank.morphs)
		{
			morph2.morphValue = 0f;
		}
		int num = 0;
		foreach (string usedMorphName in _usedMorphNames)
		{
			DAZMorph morph = dazMesh.morphBank.GetMorph(usedMorphName);
			if (morph != null)
			{
				morph.morphValueAdjustLimits = _usedMorphValues[num];
			}
			num++;
		}
		dazMesh.ApplyMorphs(true);
	}

	public void ClearUsedMorphs()
	{
		_usedMorphNames = new List<string>();
		_usedMorphValues = new List<float>();
	}

	public void CopyMorphs()
	{
		if (!(morphCopyFrom != null) || !(dazMesh != null) || !(dazMesh.morphBank != null))
		{
			return;
		}
		List<string> list = morphCopyFrom.usedMorphNames;
		List<float> list2 = morphCopyFrom.usedMorphValue;
		_usedMorphNames = new List<string>();
		_usedMorphValues = new List<float>();
		int num = 0;
		foreach (string item in list)
		{
			DAZMorph morph = dazMesh.morphBank.GetMorph(item);
			if (morph != null)
			{
				_usedMorphNames.Add(item);
				_usedMorphValues.Add(list2[num]);
			}
			else
			{
				Debug.LogError("Could not find morph " + item);
			}
			num++;
		}
	}

	protected void WrapThreaded()
	{
		try
		{
			if (!(dazMesh != null) || !(skin != null) || !(skin.dazMesh != null))
			{
				return;
			}
			wrapStore.wrapVertices = new DAZSkinWrapStore.SkinWrapVert[dazMesh.numUVVertices];
			Vector3[] morphedBaseVertices = dazMesh.morphedBaseVertices;
			Vector3[] morphedUVNormals = dazMesh.morphedUVNormals;
			Vector3[] baseSurfaceNormals = skin.dazMesh.baseSurfaceNormals;
			int[] baseTriangles = skin.dazMesh.baseTriangles;
			Vector3[] array = ((wrapToSkinnedVertices && appIsPlayingThreaded) ? ((skin.skinMethod != DAZSkinV2.SkinMethod.CPUAndGPU) ? skin.drawVerts : skin.rawSkinnedVerts) : ((!wrapToMorphedVertices) ? skin.dazMesh.baseVertices : ((!appIsPlayingThreaded) ? skin.dazMesh.morphedBaseVertices : skin.dazMesh.morphedUVVertices)));
			for (int i = 0; i < dazMesh.numUVVertices; i++)
			{
				wrapStore.wrapVertices[i] = default(DAZSkinWrapStore.SkinWrapVert);
			}
			Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
			int[][] baseMaterialVertices = skin.dazMesh.baseMaterialVertices;
			bool[] array2 = skin.dazMesh.materialsEnabled;
			for (int j = 0; j < array2.Length; j++)
			{
				if (!array2[j])
				{
					continue;
				}
				for (int k = 0; k < baseMaterialVertices[j].Length; k++)
				{
					if (!dictionary.ContainsKey(baseMaterialVertices[j][k]))
					{
						dictionary.Add(baseMaterialVertices[j][k], true);
					}
				}
			}
			for (int l = 0; l < dazMesh.numBaseVertices; l++)
			{
				if (killThread)
				{
					wrapStatus = null;
					return;
				}
				float num = (float)l * 1f / (float)dazMesh.numBaseVertices;
				threadedCount = l;
				threadedProgress = num;
				int num2 = -1;
				float num3 = 10000f;
				int num4 = -1;
				for (int m = 0; m < baseTriangles.Length; m += 3)
				{
					num4++;
					int num5 = baseTriangles[m];
					int num6 = baseTriangles[m + 1];
					int num7 = baseTriangles[m + 2];
					if (vertexIndexLimit != -1 && (num5 >= vertexIndexLimit || num6 >= vertexIndexLimit || num7 >= vertexIndexLimit))
					{
						continue;
					}
					if (wrapCheckNormals)
					{
						float num8 = Vector3.Dot(baseSurfaceNormals[num4], morphedUVNormals[l]);
						if (num8 < 0f)
						{
							continue;
						}
					}
					if (wrapToDisabledMaterials || (dictionary.ContainsKey(num5) && dictionary.ContainsKey(num6) && dictionary.ContainsKey(num7)))
					{
						Vector3 vector = (array[num5] + array[num6] + array[num7]) * 0.33333f;
						float magnitude = (vector - morphedBaseVertices[l]).magnitude;
						if (magnitude < num3)
						{
							num2 = num4;
							num3 = magnitude;
						}
					}
				}
				if (num2 == -1)
				{
					Debug.LogError("Could not find closest triangle during skin wrap");
				}
				DAZSkinWrapStore.SkinWrapVert skinWrapVert = wrapStore.wrapVertices[l];
				skinWrapVert.closestTriangle = num2;
				int num9 = num2 * 3;
				int num10 = baseTriangles[num9];
				int num11 = baseTriangles[num9 + 1];
				int num12 = baseTriangles[num9 + 2];
				float magnitude2 = (array[num10] - morphedBaseVertices[l]).magnitude;
				float magnitude3 = (array[num11] - morphedBaseVertices[l]).magnitude;
				float magnitude4 = (array[num12] - morphedBaseVertices[l]).magnitude;
				if (magnitude2 < magnitude3)
				{
					if (magnitude2 < magnitude4)
					{
						skinWrapVert.Vertex1 = num10;
						if (magnitude3 < magnitude4)
						{
							skinWrapVert.Vertex2 = num11;
							skinWrapVert.Vertex3 = num12;
						}
						else
						{
							skinWrapVert.Vertex2 = num12;
							skinWrapVert.Vertex3 = num11;
						}
					}
					else
					{
						skinWrapVert.Vertex1 = num12;
						skinWrapVert.Vertex2 = num10;
						skinWrapVert.Vertex3 = num11;
					}
				}
				else if (magnitude3 < magnitude4)
				{
					skinWrapVert.Vertex1 = num11;
					if (magnitude2 < magnitude4)
					{
						skinWrapVert.Vertex2 = num10;
						skinWrapVert.Vertex3 = num12;
					}
					else
					{
						skinWrapVert.Vertex2 = num12;
						skinWrapVert.Vertex3 = num10;
					}
				}
				else
				{
					skinWrapVert.Vertex1 = num12;
					skinWrapVert.Vertex2 = num11;
					skinWrapVert.Vertex3 = num10;
				}
				Vector3 vector2 = (array[skinWrapVert.Vertex1] + array[skinWrapVert.Vertex2] + array[skinWrapVert.Vertex3]) * 0.33333f;
				Vector3 vector3 = vector2 - array[skinWrapVert.Vertex1];
				Vector3 rhs = baseSurfaceNormals[num2];
				Vector3 rhs2 = Vector3.Cross(vector3, rhs);
				Vector3 lhs = morphedBaseVertices[l] - array[skinWrapVert.Vertex1];
				skinWrapVert.surfaceNormalProjection = Vector3.Dot(lhs, rhs) / rhs.sqrMagnitude;
				skinWrapVert.surfaceTangent1Projection = Vector3.Dot(lhs, vector3) / vector3.sqrMagnitude;
				skinWrapVert.surfaceTangent2Projection = Vector3.Dot(lhs, rhs2) / rhs2.sqrMagnitude;
				skinWrapVert.surfaceNormalWrapNormalDot = Vector3.Dot(morphedUVNormals[l], rhs);
				skinWrapVert.surfaceTangent1WrapNormalDot = Vector3.Dot(morphedUVNormals[l], vector3) / vector3.sqrMagnitude;
				skinWrapVert.surfaceTangent2WrapNormalDot = Vector3.Dot(morphedUVNormals[l], rhs2) / rhs2.sqrMagnitude;
				wrapStore.wrapVertices[l] = skinWrapVert;
			}
			DAZVertexMap[] baseVerticesToUVVertices = dazMesh.baseVerticesToUVVertices;
			foreach (DAZVertexMap dAZVertexMap in baseVerticesToUVVertices)
			{
				wrapStore.wrapVertices[dAZVertexMap.tovert].closestTriangle = wrapStore.wrapVertices[dAZVertexMap.fromvert].closestTriangle;
				wrapStore.wrapVertices[dAZVertexMap.tovert].Vertex1 = wrapStore.wrapVertices[dAZVertexMap.fromvert].Vertex1;
				wrapStore.wrapVertices[dAZVertexMap.tovert].Vertex2 = wrapStore.wrapVertices[dAZVertexMap.fromvert].Vertex2;
				wrapStore.wrapVertices[dAZVertexMap.tovert].Vertex3 = wrapStore.wrapVertices[dAZVertexMap.fromvert].Vertex3;
				wrapStore.wrapVertices[dAZVertexMap.tovert].surfaceNormalProjection = wrapStore.wrapVertices[dAZVertexMap.fromvert].surfaceNormalProjection;
				wrapStore.wrapVertices[dAZVertexMap.tovert].surfaceTangent1Projection = wrapStore.wrapVertices[dAZVertexMap.fromvert].surfaceTangent1Projection;
				wrapStore.wrapVertices[dAZVertexMap.tovert].surfaceTangent2Projection = wrapStore.wrapVertices[dAZVertexMap.fromvert].surfaceTangent2Projection;
				wrapStore.wrapVertices[dAZVertexMap.tovert].surfaceNormalWrapNormalDot = wrapStore.wrapVertices[dAZVertexMap.fromvert].surfaceNormalWrapNormalDot;
				wrapStore.wrapVertices[dAZVertexMap.tovert].surfaceTangent1WrapNormalDot = wrapStore.wrapVertices[dAZVertexMap.fromvert].surfaceTangent1WrapNormalDot;
				wrapStore.wrapVertices[dAZVertexMap.tovert].surfaceTangent2WrapNormalDot = wrapStore.wrapVertices[dAZVertexMap.fromvert].surfaceTangent2WrapNormalDot;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception during wrap: " + ex);
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

	private IEnumerator WatchThread()
	{
		while (wrapThread.IsAlive)
		{
			wrapStatus = "Wrapped " + threadedCount + " of " + dazMesh.numBaseVertices + " vertices";
			yield return null;
		}
		isWrapping = false;
	}

	public void Wrap()
	{
		if (isWrapping)
		{
			return;
		}
		isWrapping = true;
		wrapStatus = "Wrapping";
		bool flag = false;
		if (wrapStore != null)
		{
			flag = true;
		}
		wrapStore = ScriptableObject.CreateInstance<DAZSkinWrapStore>();
		RegisterAllocatedObject(wrapStore);
		bool flag2 = !Application.isPlaying || wrapToSkinnedVertices;
		if (flag)
		{
			SetMeshMorphsFromUsedValue();
		}
		else
		{
			SetUsedMorphsFromCurrentMorphs();
		}
		if (dazMesh != null && skin != null && skin.dazMesh != null)
		{
			dazMesh.Init();
			skin.dazMesh.Init();
			if (dazMesh.morphBank != null)
			{
				dazMesh.ConnectMorphBank();
				dazMesh.morphBank.Init();
			}
		}
		threadedCount = 0f;
		threadedProgress = 0f;
		killThread = false;
		appIsPlayingThreaded = Application.isPlaying;
		wrapThread = new Thread(WrapThreaded);
		wrapThread.Start();
		if (!flag2)
		{
			StartCoroutine(WatchThread());
			return;
		}
		while (wrapThread.IsAlive)
		{
			Thread.Sleep(100);
		}
		isWrapping = false;
	}

	protected void InitSmoothing()
	{
		if (meshSmooth == null && dazMesh != null)
		{
			meshSmooth = new MeshSmooth(dazMesh.baseVertices, dazMesh.basePolyList);
		}
		if (meshSmoothGPU == null && GPUMeshCompute != null && Application.isPlaying && dazMesh != null)
		{
			meshSmoothGPU = new MeshSmoothGPU(GPUMeshCompute, dazMesh.baseVertices, dazMesh.basePolyList);
		}
	}

	public Mesh GetStartMesh()
	{
		return startMesh;
	}

	public void InitMesh(bool force = false)
	{
		if (dazMesh != null && (force || !meshWasInit))
		{
			dazMesh.Init();
			defaultAdditionalThicknessMultiplier = additionalThicknessMultiplier;
			defaultSurfaceOffset = surfaceOffset;
			meshSmooth = null;
			meshSmoothGPU = null;
			meshWasInit = true;
			_verts1 = (Vector3[])dazMesh.morphedUVVertices.Clone();
			_verts2 = (Vector3[])dazMesh.morphedUVVertices.Clone();
			mesh = UnityEngine.Object.Instantiate(dazMesh.morphedUVMappedMesh);
			RegisterAllocatedObject(mesh);
			Bounds bounds = new Bounds(size: new Vector3(10000f, 10000f, 10000f), center: base.transform.position);
			mesh.bounds = bounds;
			startMesh = UnityEngine.Object.Instantiate(dazMesh.morphedUVMappedMesh);
			RegisterAllocatedObject(startMesh);
			startMesh.bounds = bounds;
		}
	}

	public void InitStartMesh()
	{
		if (dazMesh != null && !startMeshWasInit && wrapStore != null && wrapStore.wrapVertices != null)
		{
			startMeshWasInit = true;
			startMesh = UnityEngine.Object.Instantiate(dazMesh.morphedUVMappedMesh);
			RegisterAllocatedObject(startMesh);
			Bounds bounds = new Bounds(size: new Vector3(10000f, 10000f, 10000f), center: base.transform.position);
			startMesh.bounds = bounds;
			UpdateVerts(true);
		}
	}

	public void CopyMaterials()
	{
		if (dazMesh != null)
		{
			_numMaterials = dazMesh.materials.Length;
			GPUsimpleMaterial = dazMesh.simpleMaterial;
			GPUmaterials = new Material[_numMaterials];
			simTextures = new Texture2D[_numMaterials];
			materialsEnabled = new bool[_numMaterials];
			_materialNames = new string[_numMaterials];
			for (int i = 0; i < _numMaterials; i++)
			{
				GPUmaterials[i] = dazMesh.materials[i];
				materialsEnabled[i] = dazMesh.materialsEnabled[i];
				_materialNames[i] = dazMesh.materialNames[i];
			}
		}
	}

	protected void InitRecalcNormalsTangents()
	{
		if (recalculateNormals && originalRecalcNormalsGPU == null && dazMesh != null)
		{
			originalRecalcNormalsGPU = new RecalculateNormalsGPU(GPUMeshCompute, dazMesh.baseTriangles, dazMesh.numUVVertices, dazMesh.baseVerticesToUVVertices);
			_originalNormalsBuffer = originalRecalcNormalsGPU.normalsBuffer;
			_originalSurfaceNormalsBuffer = originalRecalcNormalsGPU.surfaceNormalsBuffer;
			originalRecalcNormalsGPU.RecalculateNormals(_drawVerticesBuffer);
		}
		if (recalculateNormals && recalcNormalsGPU == null && dazMesh != null)
		{
			recalcNormalsGPU = new RecalculateNormalsGPU(GPUMeshCompute, dazMesh.baseTriangles, dazMesh.numUVVertices, dazMesh.baseVerticesToUVVertices);
			_normalsBuffer = recalcNormalsGPU.normalsBuffer;
			_surfaceNormalsBuffer = recalcNormalsGPU.surfaceNormalsBuffer;
		}
		if (recalculateTangents && originalRecalcTangentsGPU == null && dazMesh != null)
		{
			originalRecalcTangentsGPU = new RecalculateTangentsGPU(GPUMeshCompute, dazMesh.UVTriangles, dazMesh.UV, dazMesh.numUVVertices);
			_originalTangentsBuffer = originalRecalcTangentsGPU.tangentsBuffer;
			originalRecalcTangentsGPU.RecalculateTangents(_drawVerticesBuffer, _originalNormalsBuffer);
		}
		if (recalculateTangents && recalcTangentsGPU == null && dazMesh != null)
		{
			recalcTangentsGPU = new RecalculateTangentsGPU(GPUMeshCompute, dazMesh.UVTriangles, dazMesh.UV, dazMesh.numUVVertices);
			_tangentsBuffer = recalcTangentsGPU.tangentsBuffer;
		}
	}

	protected void GPURecheckSurfaceNormalOffsets()
	{
		if (currentMoveToSurface == moveToSurface && currentMoveToSurfaceOffset == moveToSurfaceOffset)
		{
			return;
		}
		if (moveToSurface)
		{
			for (int i = 0; i < dazMesh.numBaseVertices; i++)
			{
				if (startingWrapStore.wrapVertices[i].surfaceNormalProjection < moveToSurfaceOffset)
				{
					wrapStore.wrapVertices[i].surfaceNormalProjection = moveToSurfaceOffset;
				}
				else
				{
					wrapStore.wrapVertices[i].surfaceNormalProjection = startingWrapStore.wrapVertices[i].surfaceNormalProjection;
				}
			}
		}
		else
		{
			for (int j = 0; j < dazMesh.numBaseVertices; j++)
			{
				wrapStore.wrapVertices[j].surfaceNormalProjection = startingWrapStore.wrapVertices[j].surfaceNormalProjection;
			}
		}
		currentMoveToSurface = moveToSurface;
		currentMoveToSurfaceOffset = moveToSurfaceOffset;
		if (_wrapVerticesBuffer != null)
		{
			_wrapVerticesBuffer.SetData(wrapStore.wrapVertices);
		}
	}

	protected void SkinWrapGPUInit()
	{
		if (!Application.isPlaying || _wrapVerticesBuffer != null)
		{
			return;
		}
		_skinWrapKernel = GPUSkinWrapper.FindKernel("SkinWrap");
		_skinWrapCalcChangeMatricesKernel = GPUSkinWrapper.FindKernel("SkinWrapCalcChangeMatrices");
		_copyKernel = GPUSkinWrapper.FindKernel("SkinWrapCopyVerts");
		_copyTangentsKernel = GPUMeshCompute.FindKernel("CopyTangents");
		int numUVVertices = dazMesh.numUVVertices;
		numVertThreadGroups = numUVVertices / 256;
		if (numUVVertices % 256 != 0)
		{
			numVertThreadGroups++;
		}
		if (startingWrapStore == null)
		{
			startingWrapStore = wrapStore;
			wrapStore = UnityEngine.Object.Instantiate(wrapStore);
			RegisterAllocatedObject(wrapStore);
		}
		GPURecheckSurfaceNormalOffsets();
		int num = numVertThreadGroups * 256;
		_wrapVerticesBuffer = new ComputeBuffer(num, 40);
		_wrapVerticesBuffer.SetData(wrapStore.wrapVertices);
		_verticesBuffer1 = new ComputeBuffer(num, 12);
		_verticesBuffer2 = new ComputeBuffer(num, 12);
		_delayedVertsBuffer = new ComputeBuffer(num, 12);
		_drawVerticesBuffer = _delayedVertsBuffer;
		_drawVerticesBuffer.SetData(dazMesh.morphedUVVertices);
		_delayedNormalsBuffer = new ComputeBuffer(num, 12);
		_delayedTangentsBuffer = new ComputeBuffer(num, 16);
		_originalVerticesBuffer = new ComputeBuffer(num, 12);
		_originalVerticesBuffer.SetData(dazMesh.morphedUVVertices);
		mapVerticesGPU = new MapVerticesGPU(GPUMeshCompute, dazMesh.baseVerticesToUVVertices);
		InitRecalcNormalsTangents();
		PreCalculatedVerticesBuffer = new GpuBuffer<Vector3>(_delayedVertsBuffer);
		PreCalculatedVerticesBuffer.Data = new Vector3[num];
		_matricesBuffer = new ComputeBuffer(num, 64);
		_ToWorldMatricesBuffer = new GpuBuffer<Matrix4x4>(_matricesBuffer);
		NormalsBuffer = new GpuBuffer<Vector3>(_normalsBuffer);
		InitMaterials();
		if (!GPUAutoSwapShader)
		{
			return;
		}
		for (int i = 0; i < GPUmaterials.Length; i++)
		{
			Shader shader = GPUmaterials[i].shader;
			Shader shader2 = MeshVR.VamShaderProvider.FindComputeBuff(shader.name);
			int renderQueue = GPUmaterials[i].renderQueue;
			if (shader2 != null)
			{
				GPUmaterials[i].shader = shader2;
				GPUmaterials[i].renderQueue = renderQueue;
			}
		}
	}

	protected void DetermineUsedSkinVerts()
	{
		if (usedSkinVerts != null || !(dazMesh != null))
		{
			return;
		}
		Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
		for (int i = 0; i < dazMesh.numBaseVertices; i++)
		{
			DAZSkinWrapStore.SkinWrapVert skinWrapVert = wrapStore.wrapVertices[i];
			if (!dictionary.ContainsKey(skinWrapVert.Vertex1))
			{
				dictionary.Add(skinWrapVert.Vertex1, true);
			}
			if (!dictionary.ContainsKey(skinWrapVert.Vertex2))
			{
				dictionary.Add(skinWrapVert.Vertex2, true);
			}
			if (!dictionary.ContainsKey(skinWrapVert.Vertex3))
			{
				dictionary.Add(skinWrapVert.Vertex3, true);
			}
		}
		usedSkinVerts = new List<int>(dictionary.Keys);
	}

	protected void UpdatePostSkinVerts()
	{
		DetermineUsedSkinVerts();
		foreach (int usedSkinVert in usedSkinVerts)
		{
			if (!skin.postSkinVerts[usedSkinVert])
			{
				skin.postSkinVerts[usedSkinVert] = true;
				skin.postSkinVertsChanged = true;
			}
			if (!skin.postSkinNormalVerts[usedSkinVert])
			{
				skin.postSkinNormalVerts[usedSkinVert] = true;
				skin.postSkinVertsChanged = true;
			}
		}
	}

	public void UpdateVerts(bool updateStartMesh = false)
	{
		InitMesh();
		Vector3[] array;
		Vector3[] array2;
		if (Application.isPlaying)
		{
			if (updateStartMesh)
			{
				array = skin.dazMesh.baseVertices;
				array2 = skin.dazMesh.baseSurfaceNormals;
			}
			else if (skin.skinMethod == DAZSkinV2.SkinMethod.CPUAndGPU)
			{
				UpdatePostSkinVerts();
				array = skin.rawSkinnedVerts;
				array2 = skin.drawSurfaceNormals;
			}
			else if (skin.skin)
			{
				array = skin.drawVerts;
				array2 = skin.drawSurfaceNormals;
			}
			else
			{
				array = skin.dazMesh.morphedUVVertices;
				array2 = skin.dazMesh.baseSurfaceNormals;
			}
		}
		else
		{
			array = skin.dazMesh.morphedUVVertices;
			array2 = skin.dazMesh.baseSurfaceNormals;
		}
		int[] baseTriangles = skin.dazMesh.baseTriangles;
		bool flag = true;
		for (int i = 0; i < _numMaterials; i++)
		{
			if (!dazMesh.materialsEnabled[i])
			{
				flag = false;
				break;
			}
		}
		float x = base.transform.lossyScale.x;
		float num = 1f / x;
		if (flag || !onlyUpdateEnabledMaterials || dazMesh.baseMaterialVertices == null)
		{
			for (int j = 0; j < dazMesh.numBaseVertices; j++)
			{
				DAZSkinWrapStore.SkinWrapVert skinWrapVert = wrapStore.wrapVertices[j];
				int closestTriangle = skinWrapVert.closestTriangle;
				Vector3 vector = array[skinWrapVert.Vertex1];
				Vector3 vector2 = (array[skinWrapVert.Vertex1] + array[skinWrapVert.Vertex2] + array[skinWrapVert.Vertex3]) * 0.33333f;
				Vector3 vector3 = (vector2 - vector) * num;
				Vector3 vector4 = array2[closestTriangle];
				Vector3 vector5 = Vector3.Cross(vector3, vector4);
				float surfaceNormalProjection = skinWrapVert.surfaceNormalProjection;
				if (moveToSurface && surfaceNormalProjection < moveToSurfaceOffset)
				{
					surfaceNormalProjection = moveToSurfaceOffset;
				}
				Vector3 vector6 = vector + vector3 * x * (skinWrapVert.surfaceTangent1Projection + skinWrapVert.surfaceTangent1WrapNormalDot * additionalThicknessMultiplier) + vector5 * x * (skinWrapVert.surfaceTangent2Projection + skinWrapVert.surfaceTangent2WrapNormalDot * additionalThicknessMultiplier) + vector4 * x * (surfaceNormalProjection + surfaceOffset + skinWrapVert.surfaceNormalWrapNormalDot * additionalThicknessMultiplier);
				_verts1[j] = vector6;
				if (debug && j >= debugStartVert && j <= debugStopVert)
				{
					debugTan1 = skinWrapVert.surfaceTangent1Projection;
					debugTan2 = skinWrapVert.surfaceTangent2Projection;
					MyDebug.DrawWireCube(vector2, debugSize * 0.5f, Color.cyan);
					MyDebug.DrawWireCube(vector6, debugSize * 0.6f, Color.gray);
					MyDebug.DrawWireCube(vector, debugSize, Color.blue);
					MyDebug.DrawWireCube(_verts1[j], debugSize, Color.green);
					Debug.DrawLine(vector, _verts1[j], Color.yellow);
					Debug.DrawLine(vector, vector2, Color.yellow);
					Debug.DrawLine(vector, vector + vector3, Color.red);
					Debug.DrawLine(vector, vector + vector5, Color.red);
				}
			}
		}
		else
		{
			int[][] baseMaterialVertices = dazMesh.baseMaterialVertices;
			for (int k = 0; k < _numMaterials; k++)
			{
				if (!dazMesh.materialsEnabled[k])
				{
					continue;
				}
				for (int l = 0; l < baseMaterialVertices[k].Length; l++)
				{
					int num2 = baseMaterialVertices[k][l];
					DAZSkinWrapStore.SkinWrapVert skinWrapVert2 = wrapStore.wrapVertices[num2];
					int closestTriangle2 = skinWrapVert2.closestTriangle;
					Vector3 vector7 = array[skinWrapVert2.Vertex1];
					Vector3 vector8 = (array[skinWrapVert2.Vertex1] + array[skinWrapVert2.Vertex2] + array[skinWrapVert2.Vertex3]) * 0.33333f;
					Vector3 vector9 = (vector8 - vector7) * num;
					Vector3 vector10 = array2[closestTriangle2];
					Vector3 vector11 = Vector3.Cross(vector9, vector10);
					float surfaceNormalProjection2 = skinWrapVert2.surfaceNormalProjection;
					if (moveToSurface && surfaceNormalProjection2 < moveToSurfaceOffset)
					{
						surfaceNormalProjection2 = moveToSurfaceOffset;
					}
					Vector3 vector12 = vector7 + vector9 * x * (skinWrapVert2.surfaceTangent1Projection + skinWrapVert2.surfaceTangent1WrapNormalDot * additionalThicknessMultiplier) + vector11 * x * (skinWrapVert2.surfaceTangent2Projection + skinWrapVert2.surfaceTangent2WrapNormalDot * additionalThicknessMultiplier) + vector10 * x * (surfaceNormalProjection2 + surfaceOffset + skinWrapVert2.surfaceNormalWrapNormalDot * additionalThicknessMultiplier);
					_verts1[num2] = vector12;
				}
			}
		}
		if (smoothOuterLoops > 0 && (laplacianSmoothPasses > 0 || springSmoothPasses > 0))
		{
			InitSmoothing();
			if (meshSmooth != null)
			{
				int num3 = 0;
				for (int m = 0; m < smoothOuterLoops; m++)
				{
					for (int n = 0; n < laplacianSmoothPasses; n++)
					{
						if (num3 % 2 == 0)
						{
							meshSmooth.LaplacianSmooth(_verts1, _verts2);
							meshSmooth.HCCorrection(_verts1, _verts2, laplacianSmoothBeta);
							_drawVerts = _verts2;
						}
						else
						{
							meshSmooth.LaplacianSmooth(_verts2, _verts1);
							meshSmooth.HCCorrection(_verts2, _verts1, laplacianSmoothBeta);
							_drawVerts = _verts1;
						}
						num3++;
					}
					for (int num4 = 0; num4 < springSmoothPasses; num4++)
					{
						if (num3 % 2 == 0)
						{
							meshSmooth.SpringSmooth(_verts1, _verts2, springSmoothFactor, x);
							_drawVerts = _verts2;
						}
						else
						{
							meshSmooth.SpringSmooth(_verts2, _verts1, springSmoothFactor, x);
							_drawVerts = _verts1;
						}
						num3++;
					}
				}
			}
			else
			{
				_drawVerts = _verts1;
			}
		}
		else
		{
			_drawVerts = _verts1;
		}
		DAZVertexMap[] baseVerticesToUVVertices = dazMesh.baseVerticesToUVVertices;
		foreach (DAZVertexMap dAZVertexMap in baseVerticesToUVVertices)
		{
			_drawVerts[dAZVertexMap.tovert] = _drawVerts[dAZVertexMap.fromvert];
		}
		if (updateStartMesh)
		{
			startMesh.vertices = _drawVerts;
		}
		else
		{
			mesh.vertices = _drawVerts;
		}
	}

	protected void DoSmoothingGPU(ComputeBuffer startBuffer)
	{
		InitSmoothing();
		float x = base.transform.lossyScale.x;
		int num = 0;
		for (int i = 0; i < smoothOuterLoops; i++)
		{
			for (int j = 0; j < laplacianSmoothPasses; j++)
			{
				if (num % 2 == 0)
				{
					meshSmoothGPU.LaplacianSmoothGPU(startBuffer, _verticesBuffer2);
					meshSmoothGPU.HCCorrectionGPU(startBuffer, _verticesBuffer2, laplacianSmoothBeta);
					_drawVerticesBuffer = _verticesBuffer2;
				}
				else
				{
					meshSmoothGPU.LaplacianSmoothGPU(_verticesBuffer2, startBuffer);
					meshSmoothGPU.HCCorrectionGPU(_verticesBuffer2, startBuffer, laplacianSmoothBeta);
					_drawVerticesBuffer = startBuffer;
				}
				num++;
			}
			for (int k = 0; k < springSmoothPasses; k++)
			{
				if (num % 2 == 0)
				{
					if (useSpring2)
					{
						meshSmoothGPU.SpringSmooth2GPU(startBuffer, _verticesBuffer2, spring2SmoothFactor, x);
					}
					else
					{
						meshSmoothGPU.SpringSmoothGPU(startBuffer, _verticesBuffer2, springSmoothFactor, x);
					}
					_drawVerticesBuffer = _verticesBuffer2;
				}
				else
				{
					if (useSpring2)
					{
						meshSmoothGPU.SpringSmooth2GPU(_verticesBuffer2, startBuffer, spring2SmoothFactor, x);
					}
					else
					{
						meshSmoothGPU.SpringSmoothGPU(_verticesBuffer2, startBuffer, springSmoothFactor, x);
					}
					_drawVerticesBuffer = startBuffer;
				}
				num++;
			}
		}
	}

	/// <summary>Same recovery as DAZSkinV2.EnsureGPUComputeShaders, for the wrap/graft skin.</summary>
	private void EnsureGPUComputeShaders()
	{
		if (GPUSkinWrapper == null)
		{
			GPUSkinWrapper = VamComputeShaderProvider.SkinShader;
		}
		if (GPUMeshCompute == null)
		{
			GPUMeshCompute = VamComputeShaderProvider.MeshShader;
		}
	}

	protected void UpdateVertsGPU(bool fullUpdate = true)
	{
		EnsureGPUComputeShaders();
		if (!(skin != null) || !skin.isActiveAndEnabled || !(GPUSkinWrapper != null))
		{
			return;
		}
		InitMesh();
		SkinWrapGPUInit();
		GPURecheckSurfaceNormalOffsets();
		if (_wrapVerticesBuffer == null)
		{
			return;
		}
		float x = base.transform.lossyScale.x;
		float val = 1f / x;
		GPUSkinWrapper.SetFloat("skinWrapScale", x);
		GPUSkinWrapper.SetFloat("skinWrapOneOverScale", val);
		if (skin.useSmoothing)
		{
			if (forceRawSkinVerts)
			{
				GPUSkinWrapper.SetBuffer(_skinWrapKernel, "skinToVertices", skin.rawVertsBuffer);
				GPUSkinWrapper.SetBuffer(_skinWrapKernel, "skinToSurfaceNormals", skin.surfaceNormalsRawBuffer);
			}
			else
			{
				GPUSkinWrapper.SetBuffer(_skinWrapKernel, "skinToVertices", skin.smoothedVertsBuffer);
				GPUSkinWrapper.SetBuffer(_skinWrapKernel, "skinToSurfaceNormals", skin.surfaceNormalsBuffer);
			}
		}
		else
		{
			GPUSkinWrapper.SetBuffer(_skinWrapKernel, "skinToVertices", skin.rawVertsBuffer);
			GPUSkinWrapper.SetBuffer(_skinWrapKernel, "skinToSurfaceNormals", skin.surfaceNormalsBuffer);
		}
		GPUSkinWrapper.SetBuffer(_skinWrapKernel, "skinWrapVerts", _wrapVerticesBuffer);
		GPUSkinWrapper.SetBuffer(_skinWrapKernel, "outVerts", _verticesBuffer1);
		GPUSkinWrapper.SetFloat("skinWrapNormalOffset", surfaceOffset);
		GPUSkinWrapper.SetFloat("skinWrapThicknessMultiplier", additionalThicknessMultiplier);
		GPUSkinWrapper.Dispatch(_skinWrapKernel, numVertThreadGroups, 1, 1);
		if (fullUpdate && smoothOuterLoops > 0 && (laplacianSmoothPasses > 0 || springSmoothPasses > 0))
		{
			DoSmoothingGPU(_verticesBuffer1);
		}
		else
		{
			_drawVerticesBuffer = _verticesBuffer1;
		}
		mapVerticesGPU.Map(_drawVerticesBuffer);
		if (fullUpdate)
		{
			if (recalculateNormals && recalcNormalsGPU != null)
			{
				recalcNormalsGPU.RecalculateNormals(_drawVerticesBuffer);
			}
			if (recalculateTangents && recalcTangentsGPU != null)
			{
				recalcTangentsGPU.RecalculateTangents(_drawVerticesBuffer, _normalsBuffer);
			}
		}
	}

	public void DrawMesh()
	{
		if (!(mesh != null) || _renderSuspend)
		{
			return;
		}
		Matrix4x4 matrix = (Application.isPlaying ? Matrix4x4.identity : ((!(skin != null) || !(skin.root != null)) ? base.transform.localToWorldMatrix : skin.root.transform.localToWorldMatrix));
		for (int i = 0; i < mesh.subMeshCount; i++)
		{
			if (dazMesh.useSimpleMaterial && (bool)dazMesh.simpleMaterial)
			{
				Graphics.DrawMesh(mesh, matrix, dazMesh.simpleMaterial, 0, null, i, null, dazMesh.castShadows, dazMesh.receiveShadows);
			}
			else if (dazMesh.materialsEnabled[i] && dazMesh.materials[i] != null)
			{
				Graphics.DrawMesh(mesh, matrix, dazMesh.materials[i], 0, null, i, null, dazMesh.castShadows, dazMesh.receiveShadows);
			}
		}
	}

	protected void DrawMeshNative()
	{
		MeshFilter component = GetComponent<MeshFilter>();
		MeshRenderer component2 = GetComponent<MeshRenderer>();
		if (!(component != null) || !(component2 != null))
		{
			return;
		}
		if (component.sharedMesh != mesh)
		{
			component.sharedMesh = mesh;
		}
		for (int i = 0; i < mesh.subMeshCount; i++)
		{
			if (dazMesh.materialsEnabled[i])
			{
				if (dazMesh.useSimpleMaterial)
				{
					component2.sharedMaterials[i] = dazMesh.simpleMaterial;
				}
				else
				{
					component2.sharedMaterials[i] = dazMesh.materials[i];
				}
			}
			else
			{
				component2.materials[i] = null;
			}
		}
	}

	public void FlushBuffers()
	{
		materialVertsBuffers = new Dictionary<int, ComputeBuffer>();
		materialNormalsBuffers = new Dictionary<int, ComputeBuffer>();
		materialTangentsBuffers = new Dictionary<int, ComputeBuffer>();
	}

	private void OnApplicationFocus(bool focus)
	{
		FlushBuffers();
	}

	protected void DrawMeshGPU(bool noDelay = false)
	{
		if (_renderSuspend)
		{
			return;
		}
		if (materialVertsBuffers == null)
		{
			materialVertsBuffers = new Dictionary<int, ComputeBuffer>();
		}
		if (materialNormalsBuffers == null)
		{
			materialNormalsBuffers = new Dictionary<int, ComputeBuffer>();
		}
		if (materialTangentsBuffers == null)
		{
			materialTangentsBuffers = new Dictionary<int, ComputeBuffer>();
		}
		if (staticDraw)
		{
			Matrix4x4 identity = Matrix4x4.identity;
			dazMesh.DrawMorphedUVMappedMesh(identity);
		}
		else
		{
			if (!(mesh != null))
			{
				return;
			}
			Matrix4x4 identity2 = Matrix4x4.identity;
			if (dazMesh.drawFromBone != null)
			{
				Bounds bounds = mesh.bounds;
				bounds.center = dazMesh.drawFromBone.transform.position;
				Vector3 size = bounds.size;
				size.x = 0.1f;
				size.y = 0.1f;
				size.z = 0.1f;
				bounds.size = size;
				mesh.bounds = bounds;
			}
			for (int i = 0; i < mesh.subMeshCount; i++)
			{
				if (GPUuseSimpleMaterial && (bool)GPUsimpleMaterial)
				{
					if (skin.delayDisplayOneFrame && !noDelay)
					{
						GPUsimpleMaterial.SetBuffer("verts", _delayedVertsBuffer);
						GPUsimpleMaterial.SetBuffer("normals", _delayedNormalsBuffer);
						GPUsimpleMaterial.SetBuffer("tangents", _delayedTangentsBuffer);
					}
					else
					{
						GPUsimpleMaterial.SetBuffer("verts", _drawVerticesBuffer);
						GPUsimpleMaterial.SetBuffer("normals", _normalsBuffer);
						GPUsimpleMaterial.SetBuffer("tangents", _tangentsBuffer);
					}
					Graphics.DrawMesh(mesh, identity2, GPUsimpleMaterial, 0, null, i, null, dazMesh.castShadows, dazMesh.receiveShadows);
				}
				else
				{
					if (materialsEnabled == null || !materialsEnabled[i] || !(GPUmaterials[i] != null))
					{
						continue;
					}
					if (skin != null && skin.delayDisplayOneFrame && !noDelay)
					{
						ComputeBuffer value;
						materialVertsBuffers.TryGetValue(i, out value);
						if (value != _delayedVertsBuffer)
						{
							GPUmaterials[i].SetBuffer("verts", _delayedVertsBuffer);
							materialVertsBuffers.Remove(i);
							materialVertsBuffers.Add(i, _delayedVertsBuffer);
						}
						materialNormalsBuffers.TryGetValue(i, out value);
						if (value != _delayedNormalsBuffer)
						{
							GPUmaterials[i].SetBuffer("normals", _delayedNormalsBuffer);
							materialNormalsBuffers.Remove(i);
							materialNormalsBuffers.Add(i, _delayedNormalsBuffer);
						}
						materialTangentsBuffers.TryGetValue(i, out value);
						if (value != _delayedTangentsBuffer)
						{
							GPUmaterials[i].SetBuffer("tangents", _delayedTangentsBuffer);
							materialTangentsBuffers.Remove(i);
							materialTangentsBuffers.Add(i, _delayedTangentsBuffer);
						}
					}
					else
					{
						ComputeBuffer value2;
						materialVertsBuffers.TryGetValue(i, out value2);
						if (value2 != _drawVerticesBuffer)
						{
							GPUmaterials[i].SetBuffer("verts", _drawVerticesBuffer);
							materialVertsBuffers.Remove(i);
							materialVertsBuffers.Add(i, _drawVerticesBuffer);
						}
						materialNormalsBuffers.TryGetValue(i, out value2);
						if (value2 != _normalsBuffer)
						{
							GPUmaterials[i].SetBuffer("normals", _normalsBuffer);
							materialNormalsBuffers.Remove(i);
							materialNormalsBuffers.Add(i, _normalsBuffer);
						}
						materialTangentsBuffers.TryGetValue(i, out value2);
						if (value2 != _tangentsBuffer)
						{
							GPUmaterials[i].SetBuffer("tangents", _tangentsBuffer);
							materialTangentsBuffers.Remove(i);
							materialTangentsBuffers.Add(i, _tangentsBuffer);
						}
					}
					Graphics.DrawMesh(mesh, identity2, GPUmaterials[i], 0, null, i, null, dazMesh.castShadows, dazMesh.receiveShadows);
				}
			}
		}
	}

	protected void GPUCleanup()
	{
		if (meshSmoothGPU != null)
		{
			meshSmoothGPU.Release();
			meshSmoothGPU = null;
		}
		if (mapVerticesGPU != null)
		{
			mapVerticesGPU.Release();
			mapVerticesGPU = null;
		}
		if (originalRecalcNormalsGPU != null)
		{
			originalRecalcNormalsGPU.Release();
			originalRecalcNormalsGPU = null;
		}
		if (recalcNormalsGPU != null)
		{
			recalcNormalsGPU.Release();
			recalcNormalsGPU = null;
		}
		if (originalRecalcTangentsGPU != null)
		{
			originalRecalcTangentsGPU.Release();
			originalRecalcTangentsGPU = null;
		}
		if (recalcTangentsGPU != null)
		{
			recalcTangentsGPU.Release();
			recalcTangentsGPU = null;
		}
		if (_wrapVerticesBuffer != null)
		{
			_wrapVerticesBuffer.Release();
			_wrapVerticesBuffer = null;
		}
		if (_verticesBuffer1 != null)
		{
			_verticesBuffer1.Release();
			_verticesBuffer1 = null;
		}
		if (_verticesBuffer2 != null)
		{
			_verticesBuffer2.Release();
			_verticesBuffer2 = null;
		}
		if (_delayedVertsBuffer != null)
		{
			_delayedVertsBuffer.Release();
			_delayedVertsBuffer = null;
		}
		if (_delayedNormalsBuffer != null)
		{
			_delayedNormalsBuffer.Release();
			_delayedNormalsBuffer = null;
		}
		if (_delayedTangentsBuffer != null)
		{
			_delayedTangentsBuffer.Release();
			_delayedTangentsBuffer = null;
		}
		if (_originalVerticesBuffer != null)
		{
			_originalVerticesBuffer.Release();
			_originalVerticesBuffer = null;
		}
		if (_matricesBuffer != null)
		{
			_matricesBuffer.Release();
			_matricesBuffer = null;
		}
	}

	private void OnEnable()
	{
		FlushBuffers();
	}

	protected void OnDestroy()
	{
		DestroyAllocatedObjects();
		GPUCleanup();
		killThread = true;
	}

	public void InitMaterials()
	{
		if (Application.isPlaying && !_materialsWereInit && GPUmaterials != null)
		{
			_materialsWereInit = true;
			for (int i = 0; i < GPUmaterials.Length; i++)
			{
				Material material = new Material(GPUmaterials[i]);
				RegisterAllocatedObject(material);
				GPUmaterials[i] = material;
			}
		}
	}

	private void LateUpdate()
	{
		if ((!draw && !debug) || !(skin != null) || _updateDrawDisabled || !(dazMesh != null) || isWrapping || !(wrapStore != null) || wrapStore.wrapVertices == null)
		{
			return;
		}
		if (autoColliderToEnable != null)
		{
			autoColliderToEnable.on = true;
		}
		if (autoColliderToDisable != null)
		{
			autoColliderToDisable.on = false;
		}
		if (skin.skinMethod == DAZSkinV2.SkinMethod.CPU || forceCPU || !Application.isPlaying)
		{
			UpdateVerts();
			if (draw)
			{
				DrawMesh();
			}
			return;
		}
		if (skin.delayDisplayOneFrame && GPUSkinWrapper != null && _delayedVertsBuffer != null)
		{
			GPUSkinWrapper.SetBuffer(_copyKernel, "inVerts", _drawVerticesBuffer);
			GPUSkinWrapper.SetBuffer(_copyKernel, "outVerts", _delayedVertsBuffer);
			GPUSkinWrapper.Dispatch(_copyKernel, numVertThreadGroups, 1, 1);
			if (_normalsBuffer != null)
			{
				GPUSkinWrapper.SetBuffer(_copyKernel, "inVerts", _normalsBuffer);
				GPUSkinWrapper.SetBuffer(_copyKernel, "outVerts", _delayedNormalsBuffer);
				GPUSkinWrapper.Dispatch(_copyKernel, numVertThreadGroups, 1, 1);
			}
			if (_tangentsBuffer != null)
			{
				GPUMeshCompute.SetBuffer(_copyTangentsKernel, "inTangents", _tangentsBuffer);
				GPUMeshCompute.SetBuffer(_copyTangentsKernel, "outTangents", _delayedTangentsBuffer);
				GPUMeshCompute.Dispatch(_copyTangentsKernel, numVertThreadGroups, 1, 1);
			}
		}
		UpdateVertsGPU();
		if (draw)
		{
			DrawMeshGPU();
		}
	}
}
