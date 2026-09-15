using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[ExecuteInEditMode]
	[AddComponentMenu("Physics/Obi/Obi Rope")]
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(ObiDistanceConstraints))]
	[RequireComponent(typeof(ObiBendingConstraints))]
	[RequireComponent(typeof(ObiTetherConstraints))]
	[RequireComponent(typeof(ObiPinConstraints))]
	[DisallowMultipleComponent]
	public class ObiRope : ObiActor
	{
		public enum RenderingMode
		{
			ProceduralRope,
			Chain,
			Line
		}

		public class CurveFrame
		{
			public Vector3 position = Vector3.zero;

			public Vector3 tangent = Vector3.forward;

			public Vector3 normal = Vector3.up;

			public Vector3 binormal = Vector3.left;

			public CurveFrame(float twist)
			{
				Quaternion quaternion = Quaternion.AngleAxis(twist, tangent);
				normal = quaternion * normal;
				binormal = quaternion * binormal;
			}

			public void Reset()
			{
				position = Vector3.zero;
				tangent = Vector3.forward;
				normal = Vector3.up;
				binormal = Vector3.left;
			}

			public void Transport(Vector3 newPosition, Vector3 newTangent, float twist)
			{
				Quaternion quaternion = Quaternion.FromToRotation(tangent, newTangent);
				Quaternion quaternion2 = Quaternion.AngleAxis(twist, newTangent);
				Quaternion quaternion3 = quaternion2 * quaternion;
				normal = quaternion3 * normal;
				binormal = quaternion3 * binormal;
				tangent = newTangent;
				position = newPosition;
			}
		}

		public const float DEFAULT_PARTICLE_MASS = 0.1f;

		public const float MAX_YOUNG_MODULUS = 200f;

		public const float MIN_YOUNG_MODULUS = 0.0001f;

		[Tooltip("Amount of additional particles in this rope's pool that can be used to extend its lenght, or to tear it.")]
		public int pooledParticles = 10;

		[Tooltip("Path used to generate the rope.")]
		public ObiCurve ropePath;

		[HideInInspector]
		[SerializeField]
		private ObiRopeSection section;

		[HideInInspector]
		[SerializeField]
		private float sectionTwist;

		[HideInInspector]
		[SerializeField]
		private float sectionThicknessScale = 0.8f;

		[HideInInspector]
		[SerializeField]
		private bool thicknessFromParticles = true;

		[HideInInspector]
		[SerializeField]
		private Vector2 uvScale = Vector3.one;

		[HideInInspector]
		[SerializeField]
		private float uvAnchor;

		[HideInInspector]
		[SerializeField]
		private bool normalizeV = true;

		[Tooltip("Modulates the amount of particles per lenght unit. 1 means as many particles as needed for the given length/thickness will be used, whichcan be a lot in very thin and long ropes. Setting values between 0 and 1 allows you to override the amount of particles used.")]
		[Range(0f, 1f)]
		public float resolution = 0.5f;

		[HideInInspector]
		[SerializeField]
		private uint smoothing = 1u;

		public bool tearable;

		[Tooltip("Maximum strain betweeen particles before the spring constraint holding them together would break.")]
		[Delayed]
		public float tearResistanceMultiplier = 1000f;

		[HideInInspector]
		public float[] tearResistance;

		[HideInInspector]
		[SerializeField]
		private RenderingMode renderMode;

		public List<GameObject> chainLinks = new List<GameObject>();

		[HideInInspector]
		[SerializeField]
		private Vector3 linkScale = Vector3.one;

		[HideInInspector]
		[SerializeField]
		private bool randomizeLinks;

		[HideInInspector]
		public Mesh ropeMesh;

		[HideInInspector]
		[SerializeField]
		private List<GameObject> linkInstances;

		public GameObject startPrefab;

		public GameObject endPrefab;

		public GameObject tearPrefab;

		[Tooltip("Thickness of the rope, it is equivalent to particle radius.")]
		public float thickness = 0.05f;

		private GameObject[] tearPrefabPool;

		[HideInInspector]
		[SerializeField]
		private bool closed;

		[HideInInspector]
		[SerializeField]
		private float interParticleDistance;

		[HideInInspector]
		[SerializeField]
		private float restLength;

		[HideInInspector]
		[SerializeField]
		private int usedParticles;

		[HideInInspector]
		[SerializeField]
		private int totalParticles;

		private MeshFilter meshFilter;

		private GameObject startPrefabInstance;

		private GameObject endPrefabInstance;

		private float curveLength;

		private float curveSections;

		private List<Vector4[]> curves = new List<Vector4[]>();

		private List<Vector3> vertices = new List<Vector3>();

		private List<Vector3> normals = new List<Vector3>();

		private List<Vector4> tangents = new List<Vector4>();

		private List<Vector2> uvs = new List<Vector2>();

		private List<int> tris = new List<int>();

		public ObiDistanceConstraints DistanceConstraints => constraints[Oni.ConstraintType.Distance] as ObiDistanceConstraints;

		public ObiBendingConstraints BendingConstraints => constraints[Oni.ConstraintType.Bending] as ObiBendingConstraints;

		public ObiTetherConstraints TetherConstraints => constraints[Oni.ConstraintType.Tether] as ObiTetherConstraints;

		public ObiPinConstraints PinConstraints => constraints[Oni.ConstraintType.Pin] as ObiPinConstraints;

		public RenderingMode RenderMode
		{
			get
			{
				return renderMode;
			}
			set
			{
				if (value != renderMode)
				{
					renderMode = value;
					ClearChainLinkInstances();
					UnityEngine.Object.DestroyImmediate(ropeMesh);
					GenerateVisualRepresentation();
				}
			}
		}

		public ObiRopeSection Section
		{
			get
			{
				return section;
			}
			set
			{
				if (value != section)
				{
					section = value;
					GenerateProceduralRopeMesh();
				}
			}
		}

		public float SectionThicknessScale
		{
			get
			{
				return sectionThicknessScale;
			}
			set
			{
				if (value != sectionThicknessScale)
				{
					sectionThicknessScale = Mathf.Max(0f, value);
					UpdateProceduralRopeMesh();
				}
			}
		}

		public bool ThicknessFromParticles
		{
			get
			{
				return thicknessFromParticles;
			}
			set
			{
				if (value != thicknessFromParticles)
				{
					thicknessFromParticles = value;
					UpdateVisualRepresentation();
				}
			}
		}

		public float SectionTwist
		{
			get
			{
				return sectionTwist;
			}
			set
			{
				if (value != sectionTwist)
				{
					sectionTwist = value;
					UpdateVisualRepresentation();
				}
			}
		}

		public uint Smoothing
		{
			get
			{
				return smoothing;
			}
			set
			{
				if (value != smoothing)
				{
					smoothing = value;
					UpdateProceduralRopeMesh();
				}
			}
		}

		public Vector3 LinkScale
		{
			get
			{
				return linkScale;
			}
			set
			{
				if (value != linkScale)
				{
					linkScale = value;
					UpdateProceduralChainLinks();
				}
			}
		}

		public Vector2 UVScale
		{
			get
			{
				return uvScale;
			}
			set
			{
				if (value != uvScale)
				{
					uvScale = value;
					UpdateProceduralRopeMesh();
				}
			}
		}

		public float UVAnchor
		{
			get
			{
				return uvAnchor;
			}
			set
			{
				if (value != uvAnchor)
				{
					uvAnchor = value;
					UpdateProceduralRopeMesh();
				}
			}
		}

		public bool NormalizeV
		{
			get
			{
				return normalizeV;
			}
			set
			{
				if (value != normalizeV)
				{
					normalizeV = value;
					UpdateProceduralRopeMesh();
				}
			}
		}

		public bool RandomizeLinks
		{
			get
			{
				return randomizeLinks;
			}
			set
			{
				if (value != randomizeLinks)
				{
					randomizeLinks = value;
					GenerateProceduralChainLinks();
				}
			}
		}

		public float InterparticleDistance => interParticleDistance * DistanceConstraints.stretchingScale;

		public int TotalParticles => totalParticles;

		public int UsedParticles
		{
			get
			{
				return usedParticles;
			}
			set
			{
				usedParticles = value;
				pooledParticles = totalParticles - usedParticles;
			}
		}

		public float RestLength
		{
			get
			{
				return restLength;
			}
			set
			{
				restLength = value;
			}
		}

		public bool Closed => closed;

		public int PooledParticles => pooledParticles;

		public override void Awake()
		{
			base.Awake();
			linkInstances = new List<GameObject>();
			meshFilter = GetComponent<MeshFilter>();
		}

		public void OnValidate()
		{
			thickness = Mathf.Max(0.0001f, thickness);
			uvAnchor = Mathf.Clamp01(uvAnchor);
			tearResistanceMultiplier = Mathf.Max(0.1f, tearResistanceMultiplier);
			resolution = Mathf.Max(0.0001f, resolution);
		}

		public override void OnEnable()
		{
			base.OnEnable();
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(RopePreCull));
			GenerateVisualRepresentation();
		}

		public override void OnDisable()
		{
			base.OnDisable();
			Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(RopePreCull));
		}

		public void RopePreCull(Camera cam)
		{
			if (renderMode == RenderingMode.Line)
			{
				UpdateLineMesh(cam);
			}
		}

		public override void OnSolverStepEnd()
		{
			base.OnSolverStepEnd();
			if (base.isActiveAndEnabled)
			{
				ApplyTearing();
				if (PinConstraints.GetBatches().Count > 0)
				{
					((ObiPinConstraintBatch)PinConstraints.GetBatches()[0]).BreakConstraints();
				}
			}
		}

		public override void OnSolverFrameEnd()
		{
			base.OnSolverFrameEnd();
			UpdateVisualRepresentation();
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			UnityEngine.Object.DestroyImmediate(ropeMesh);
			ClearChainLinkInstances();
			ClearPrefabInstances();
		}

		public override bool AddToSolver(object info)
		{
			if (base.Initialized && base.AddToSolver(info))
			{
				solver.RequireRenderablePositions();
				return true;
			}
			return false;
		}

		public override bool RemoveFromSolver(object info)
		{
			if (solver != null)
			{
				solver.RelinquishRenderablePositions();
			}
			return base.RemoveFromSolver(info);
		}

		public IEnumerator GeneratePhysicRepresentationForMesh()
		{
			initialized = false;
			initializing = true;
			interParticleDistance = -1f;
			RemoveFromSolver(null);
			if (ropePath == null)
			{
				Debug.LogError("Cannot initialize rope. There's no ropePath present. Please provide a spline to define the shape of the rope");
				yield break;
			}
			ropePath.RecalculateSplineLenght(1E-05f, 7);
			closed = ropePath.Closed;
			restLength = ropePath.Length;
			usedParticles = Mathf.CeilToInt(restLength / thickness * resolution) + ((!closed) ? 1 : 0);
			totalParticles = usedParticles + pooledParticles;
			active = new bool[totalParticles];
			positions = new Vector3[totalParticles];
			velocities = new Vector3[totalParticles];
			invMasses = new float[totalParticles];
			solidRadii = new float[totalParticles];
			phases = new int[totalParticles];
			restPositions = new Vector4[totalParticles];
			tearResistance = new float[totalParticles];
			int numSegments = usedParticles - ((!closed) ? 1 : 0);
			if (numSegments > 0)
			{
				interParticleDistance = restLength / (float)numSegments;
			}
			else
			{
				interParticleDistance = 0f;
			}
			float radius = interParticleDistance * resolution;
			for (int i = 0; i < usedParticles; i++)
			{
				active[i] = true;
				invMasses[i] = 10f;
				float mu = ropePath.GetMuAtLenght(interParticleDistance * (float)i);
				positions[i] = base.transform.InverseTransformPoint(ropePath.transform.TransformPoint(ropePath.GetPositionAt(mu)));
				solidRadii[i] = radius;
				phases[i] = Oni.MakePhase(1, selfCollisions ? Oni.ParticlePhase.SelfCollide : ((Oni.ParticlePhase)0));
				tearResistance[i] = 1f;
				if (i % 100 == 0)
				{
					yield return new CoroutineJob.ProgressInfo("ObiRope: generating particles...", (float)i / (float)usedParticles);
				}
			}
			for (int j = usedParticles; j < totalParticles; j++)
			{
				active[j] = false;
				invMasses[j] = 10f;
				solidRadii[j] = radius;
				phases[j] = Oni.MakePhase(1, selfCollisions ? Oni.ParticlePhase.SelfCollide : ((Oni.ParticlePhase)0));
				tearResistance[j] = 1f;
				if (j % 100 == 0)
				{
					yield return new CoroutineJob.ProgressInfo("ObiRope: generating particles...", (float)j / (float)usedParticles);
				}
			}
			DistanceConstraints.Clear();
			ObiDistanceConstraintBatch distanceBatch = new ObiDistanceConstraintBatch(false, false, 0.0001f, 200f);
			DistanceConstraints.AddBatch(distanceBatch);
			for (int k = 0; k < numSegments; k++)
			{
				distanceBatch.AddConstraint(k, (k + 1) % ((!ropePath.Closed) ? (usedParticles + 1) : usedParticles), interParticleDistance, 1f, 1f);
				if (k % 500 == 0)
				{
					yield return new CoroutineJob.ProgressInfo("ObiRope: generating structural constraints...", (float)k / (float)numSegments);
				}
			}
			BendingConstraints.Clear();
			ObiBendConstraintBatch bendingBatch = new ObiBendConstraintBatch(false, false, 0.0001f, 200f);
			BendingConstraints.AddBatch(bendingBatch);
			for (int l = 0; l < usedParticles - ((!closed) ? 2 : 0); l++)
			{
				bendingBatch.AddConstraint(l, (l + 2) % usedParticles, (l + 1) % usedParticles, 0f, 0f, 1f);
				if (l % 500 == 0)
				{
					yield return new CoroutineJob.ProgressInfo("ObiRope: adding bend constraints...", (float)l / (float)usedParticles);
				}
			}
			TetherConstraints.Clear();
			PinConstraints.Clear();
			ObiPinConstraintBatch pinBatch = new ObiPinConstraintBatch(false, false, 0f, 200f);
			PinConstraints.AddBatch(pinBatch);
			initializing = false;
			initialized = true;
			RegenerateRestPositions();
			GenerateVisualRepresentation();
		}

		public void RegenerateRestPositions()
		{
			ObiDistanceConstraintBatch obiDistanceConstraintBatch = DistanceConstraints.GetBatches()[0] as ObiDistanceConstraintBatch;
			int num = -1;
			int num2 = -1;
			float num3 = 0f;
			for (int i = 0; i < obiDistanceConstraintBatch.ConstraintCount; i++)
			{
				if (i == 0)
				{
					num2 = (num = obiDistanceConstraintBatch.springIndices[i * 2]);
					restPositions[num] = Vector4.zero;
				}
				num3 += Mathf.Min(interParticleDistance, solidRadii[num], solidRadii[num2]);
				num = obiDistanceConstraintBatch.springIndices[i * 2 + 1];
				restPositions[num] = Vector3.right * num3;
				restPositions[num][3] = 0f;
			}
			PushDataToSolver(ParticleData.REST_POSITIONS);
		}

		public void RecalculateLenght()
		{
			ObiDistanceConstraintBatch obiDistanceConstraintBatch = DistanceConstraints.GetBatches()[0] as ObiDistanceConstraintBatch;
			restLength = 0f;
			for (int i = 0; i < obiDistanceConstraintBatch.ConstraintCount; i++)
			{
				restLength += obiDistanceConstraintBatch.restLengths[i];
			}
		}

		public float CalculateLength()
		{
			ObiDistanceConstraintBatch obiDistanceConstraintBatch = DistanceConstraints.GetBatches()[0] as ObiDistanceConstraintBatch;
			float num = 0f;
			for (int i = 0; i < obiDistanceConstraintBatch.ConstraintCount; i++)
			{
				Vector3 particlePosition = GetParticlePosition(obiDistanceConstraintBatch.springIndices[i * 2]);
				Vector3 particlePosition2 = GetParticlePosition(obiDistanceConstraintBatch.springIndices[i * 2 + 1]);
				num += Vector3.Distance(particlePosition, particlePosition2);
			}
			return num;
		}

		public void GenerateVisualRepresentation()
		{
			GeneratePrefabInstances();
			if (renderMode != RenderingMode.Chain)
			{
				GenerateProceduralRopeMesh();
			}
			else
			{
				GenerateProceduralChainLinks();
			}
		}

		public void UpdateVisualRepresentation()
		{
			if (renderMode != RenderingMode.Chain)
			{
				UpdateProceduralRopeMesh();
			}
			else
			{
				UpdateProceduralChainLinks();
			}
		}

		private void GenerateProceduralRopeMesh()
		{
			if (initialized)
			{
				UnityEngine.Object.DestroyImmediate(ropeMesh);
				ropeMesh = new Mesh();
				ropeMesh.MarkDynamic();
				meshFilter.mesh = ropeMesh;
				UpdateProceduralRopeMesh();
			}
		}

		private void GeneratePrefabInstances()
		{
			ClearPrefabInstances();
			if (tearPrefab != null)
			{
				tearPrefabPool = new GameObject[pooledParticles * 2];
				for (int i = 0; i < tearPrefabPool.Length; i++)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(tearPrefab);
					gameObject.hideFlags = HideFlags.HideAndDontSave;
					gameObject.SetActive(false);
					tearPrefabPool[i] = gameObject;
				}
			}
			if (startPrefabInstance == null && startPrefab != null)
			{
				startPrefabInstance = UnityEngine.Object.Instantiate(startPrefab);
				startPrefabInstance.hideFlags = HideFlags.HideAndDontSave;
			}
			if (endPrefabInstance == null && endPrefab != null)
			{
				endPrefabInstance = UnityEngine.Object.Instantiate(endPrefab);
				endPrefabInstance.hideFlags = HideFlags.HideAndDontSave;
			}
		}

		private void ClearPrefabInstances()
		{
			UnityEngine.Object.DestroyImmediate(startPrefabInstance);
			UnityEngine.Object.DestroyImmediate(endPrefabInstance);
			if (tearPrefabPool == null)
			{
				return;
			}
			for (int i = 0; i < tearPrefabPool.Length; i++)
			{
				if (tearPrefabPool[i] != null)
				{
					UnityEngine.Object.DestroyImmediate(tearPrefabPool[i]);
					tearPrefabPool[i] = null;
				}
			}
		}

		public void GenerateProceduralChainLinks()
		{
			ClearChainLinkInstances();
			if (!initialized)
			{
				return;
			}
			if (chainLinks.Count > 0)
			{
				for (int i = 0; i < totalParticles; i++)
				{
					int index = ((!randomizeLinks) ? (i % chainLinks.Count) : UnityEngine.Random.Range(0, chainLinks.Count));
					GameObject gameObject = null;
					if (chainLinks[index] != null)
					{
						gameObject = UnityEngine.Object.Instantiate(chainLinks[index]);
						gameObject.hideFlags = HideFlags.HideAndDontSave;
						gameObject.SetActive(false);
					}
					linkInstances.Add(gameObject);
				}
			}
			UpdateProceduralChainLinks();
		}

		private void ClearChainLinkInstances()
		{
			for (int i = 0; i < linkInstances.Count; i++)
			{
				if (linkInstances[i] != null)
				{
					UnityEngine.Object.DestroyImmediate(linkInstances[i]);
				}
			}
			linkInstances.Clear();
		}

		private Vector4[] ChaikinSmoothing(Vector4[] input, uint k)
		{
			if (k == 0 || input.Length < 3)
			{
				return input;
			}
			int num = (int)Mathf.Pow(2f, k);
			int num2 = input.Length - 1;
			float num3 = Mathf.Pow(2f, 0L - (long)(k + 1));
			float num4 = Mathf.Pow(2f, 0L - (long)k);
			float num5 = Mathf.Pow(2f, -2 * k);
			float num6 = Mathf.Pow(2f, -2 * k - 1);
			Vector4[] array = new Vector4[(num2 - 1) * num + 2];
			float[] array2 = new float[num];
			float[] array3 = new float[num];
			float[] array4 = new float[num];
			for (int i = 1; i <= num; i++)
			{
				array2[i - 1] = 0.5f - num3 - (float)(i - 1) * (num4 - (float)i * num6);
				array3[i - 1] = 0.5f + num3 + (float)(i - 1) * (num4 - (float)i * num5);
				array4[i - 1] = (float)((i - 1) * i) * num6;
			}
			array[0] = (0.5f + num3) * input[0] + (0.5f - num3) * input[1];
			array[num * num2 - num + 1] = (0.5f - num3) * input[num2 - 1] + (0.5f + num3) * input[num2];
			for (int j = 1; j < num2; j++)
			{
				for (int l = 1; l <= num; l++)
				{
					array[(j - 1) * num + l] = array2[l - 1] * input[j - 1] + array3[l - 1] * input[j] + array4[l - 1] * input[j + 1];
				}
			}
			return array;
		}

		private float CalculateCurveLength(Vector4[] curve)
		{
			float num = 0f;
			for (int i = 1; i < curve.Length; i++)
			{
				num += Vector3.Distance(curve[i], curve[i - 1]);
			}
			return num;
		}

		public int GetConstraintIndexAtNormalizedCoordinate(float coord)
		{
			ObiDistanceConstraintBatch obiDistanceConstraintBatch = DistanceConstraints.GetBatches()[0] as ObiDistanceConstraintBatch;
			float f = coord * (float)obiDistanceConstraintBatch.ConstraintCount;
			return Mathf.Clamp(Mathf.FloorToInt(f), 0, obiDistanceConstraintBatch.ConstraintCount - 1);
		}

		private List<int> CountContinuousSections()
		{
			List<int> list = new List<int>(usedParticles);
			ObiDistanceConstraintBatch obiDistanceConstraintBatch = DistanceConstraints.GetBatches()[0] as ObiDistanceConstraintBatch;
			int num = 0;
			int num2 = -1;
			for (int i = 0; i < obiDistanceConstraintBatch.ConstraintCount; i++)
			{
				int num3 = obiDistanceConstraintBatch.springIndices[i * 2];
				int num4 = obiDistanceConstraintBatch.springIndices[i * 2 + 1];
				if (num3 != num2 && num > 0)
				{
					list.Add(num);
					num = 0;
				}
				num2 = num4;
				num++;
			}
			if (num > 0)
			{
				list.Add(num);
			}
			return list;
		}

		private void SmoothCurvesFromParticles()
		{
			curveSections = 0f;
			curveLength = 0f;
			curves.Clear();
			List<int> list = CountContinuousSections();
			ObiDistanceConstraintBatch obiDistanceConstraintBatch = DistanceConstraints.GetBatches()[0] as ObiDistanceConstraintBatch;
			Matrix4x4 worldToLocalMatrix = base.transform.worldToLocalMatrix;
			int num = 0;
			foreach (int item in list)
			{
				Vector4[] array = new Vector4[item + 1];
				for (int i = 0; i < item; i++)
				{
					int num2 = obiDistanceConstraintBatch.springIndices[(num + i) * 2];
					array[i] = worldToLocalMatrix.MultiplyPoint3x4(GetParticlePosition(num2));
					array[i].w = solidRadii[num2];
					if (i == item - 1)
					{
						num2 = obiDistanceConstraintBatch.springIndices[(num + i) * 2 + 1];
						array[i + 1] = worldToLocalMatrix.MultiplyPoint3x4(GetParticlePosition(num2));
						array[i + 1].w = solidRadii[num2];
					}
				}
				num += item;
				Vector4[] array2 = ChaikinSmoothing(array, smoothing);
				array2[0] = array[0];
				array2[array2.Length - 1] = array[array.Length - 1];
				curves.Add(array2);
				curveSections += array2.Length - 1;
				curveLength += CalculateCurveLength(array2);
			}
		}

		private void PlaceObjectAtCurveFrame(CurveFrame frame, GameObject obj, Space space, bool reverseLookDirection)
		{
			if (space == Space.Self)
			{
				Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
				obj.transform.position = localToWorldMatrix.MultiplyPoint3x4(frame.position);
				if (frame.tangent != Vector3.zero)
				{
					obj.transform.rotation = Quaternion.LookRotation(localToWorldMatrix.MultiplyVector((!reverseLookDirection) ? (-frame.tangent) : frame.tangent), localToWorldMatrix.MultiplyVector(frame.normal));
				}
			}
			else
			{
				obj.transform.position = frame.position;
				if (frame.tangent != Vector3.zero)
				{
					obj.transform.rotation = Quaternion.LookRotation((!reverseLookDirection) ? (-frame.tangent) : frame.tangent, frame.normal);
				}
			}
		}

		public void UpdateProceduralRopeMesh()
		{
			if (base.enabled && !(ropeMesh == null) && !(section == null))
			{
				SmoothCurvesFromParticles();
				if (renderMode == RenderingMode.ProceduralRope)
				{
					UpdateRopeMesh();
				}
			}
		}

		private void ClearMeshData()
		{
			ropeMesh.Clear();
			vertices.Clear();
			normals.Clear();
			tangents.Clear();
			uvs.Clear();
			tris.Clear();
		}

		private void CommitMeshData()
		{
			ropeMesh.SetVertices(vertices);
			ropeMesh.SetNormals(normals);
			ropeMesh.SetTangents(tangents);
			ropeMesh.SetUVs(0, uvs);
			ropeMesh.SetTriangles(tris, 0, true);
		}

		private void UpdateRopeMesh()
		{
			ClearMeshData();
			float num = curveLength / restLength;
			int segments = section.Segments;
			int num2 = segments + 1;
			float num3 = (0f - uvScale.y) * restLength * uvAnchor;
			int num4 = 0;
			int num5 = 0;
			CurveFrame curveFrame = new CurveFrame((0f - sectionTwist) * curveSections * uvAnchor);
			Vector3 vector = Vector3.forward;
			Vector4 zero = Vector4.zero;
			Vector2 zero2 = Vector2.zero;
			for (int i = 0; i < curves.Count; i++)
			{
				Vector4[] array = curves[i];
				curveFrame.Reset();
				for (int j = 0; j < array.Length; j++)
				{
					int num6 = Mathf.Min(j + 1, array.Length - 1);
					int num7 = Mathf.Max(j - 1, 0);
					Vector3 vector2 = ((!closed || i != curves.Count - 1 || j != array.Length - 1) ? ((Vector3)(array[num6] - array[j])) : vector);
					Vector3 vector3 = array[j] - array[num7];
					Vector3 vector4 = vector2 + vector3;
					curveFrame.Transport(array[j], vector4, sectionTwist);
					if (tearPrefabPool != null)
					{
						if (num5 < tearPrefabPool.Length && i > 0 && j == 0)
						{
							if (!tearPrefabPool[num5].activeSelf)
							{
								tearPrefabPool[num5].SetActive(true);
							}
							PlaceObjectAtCurveFrame(curveFrame, tearPrefabPool[num5], Space.Self, false);
							num5++;
						}
						if (num5 < tearPrefabPool.Length && i < curves.Count - 1 && j == array.Length - 1)
						{
							if (!tearPrefabPool[num5].activeSelf)
							{
								tearPrefabPool[num5].SetActive(true);
							}
							PlaceObjectAtCurveFrame(curveFrame, tearPrefabPool[num5], Space.Self, true);
							num5++;
						}
					}
					if (i == 0 && j == 0)
					{
						vector = vector4;
						if (startPrefabInstance != null && !closed)
						{
							PlaceObjectAtCurveFrame(curveFrame, startPrefabInstance, Space.Self, false);
						}
					}
					else if (i == curves.Count - 1 && j == array.Length - 1 && endPrefabInstance != null && !closed)
					{
						PlaceObjectAtCurveFrame(curveFrame, endPrefabInstance, Space.Self, true);
					}
					num3 += uvScale.y * (Vector3.Distance(array[j], array[num7]) / ((!normalizeV) ? num : curveLength));
					float num8 = ((!thicknessFromParticles) ? thickness : array[j].w) * sectionThicknessScale;
					for (int k = 0; k <= segments; k++)
					{
						vertices.Add(curveFrame.position + (section.vertices[k].x * curveFrame.normal + section.vertices[k].y * curveFrame.binormal) * num8);
						normals.Add(vertices[vertices.Count - 1] - curveFrame.position);
						zero = -Vector3.Cross(normals[normals.Count - 1], curveFrame.tangent);
						zero.w = 1f;
						tangents.Add(zero);
						zero2.Set((float)k / (float)segments * uvScale.x, num3);
						uvs.Add(zero2);
						if (k < segments && j < array.Length - 1)
						{
							tris.Add(num4 * num2 + k);
							tris.Add(num4 * num2 + (k + 1));
							tris.Add((num4 + 1) * num2 + k);
							tris.Add(num4 * num2 + (k + 1));
							tris.Add((num4 + 1) * num2 + (k + 1));
							tris.Add((num4 + 1) * num2 + k);
						}
					}
					num4++;
				}
			}
			CommitMeshData();
		}

		private void UpdateLineMesh(Camera camera)
		{
			ClearMeshData();
			float num = curveLength / restLength;
			float num2 = (0f - uvScale.y) * restLength * uvAnchor;
			int num3 = 0;
			int num4 = 0;
			Vector3 vector = base.transform.InverseTransformPoint(camera.transform.position);
			CurveFrame curveFrame = new CurveFrame((0f - sectionTwist) * curveSections * uvAnchor);
			Vector3 vector2 = Vector3.forward;
			Vector4 zero = Vector4.zero;
			Vector2 zero2 = Vector2.zero;
			for (int i = 0; i < curves.Count; i++)
			{
				Vector4[] array = curves[i];
				curveFrame.Reset();
				for (int j = 0; j < array.Length; j++)
				{
					int num5 = Mathf.Min(j + 1, array.Length - 1);
					int num6 = Mathf.Max(j - 1, 0);
					Vector3 vector3 = ((!closed || i != curves.Count - 1 || j != array.Length - 1) ? ((Vector3)(array[num5] - array[j])) : vector2);
					Vector3 vector4 = array[j] - array[num6];
					Vector3 vector5 = vector3 + vector4;
					curveFrame.Transport(array[j], vector5, sectionTwist);
					if (tearPrefabPool != null)
					{
						if (num4 < tearPrefabPool.Length && i > 0 && j == 0)
						{
							if (!tearPrefabPool[num4].activeSelf)
							{
								tearPrefabPool[num4].SetActive(true);
							}
							PlaceObjectAtCurveFrame(curveFrame, tearPrefabPool[num4], Space.Self, false);
							num4++;
						}
						if (num4 < tearPrefabPool.Length && i < curves.Count - 1 && j == array.Length - 1)
						{
							if (!tearPrefabPool[num4].activeSelf)
							{
								tearPrefabPool[num4].SetActive(true);
							}
							PlaceObjectAtCurveFrame(curveFrame, tearPrefabPool[num4], Space.Self, true);
							num4++;
						}
					}
					if (i == 0 && j == 0)
					{
						vector2 = vector5;
						if (startPrefabInstance != null && !closed)
						{
							PlaceObjectAtCurveFrame(curveFrame, startPrefabInstance, Space.Self, false);
						}
					}
					else if (i == curves.Count - 1 && j == array.Length - 1 && endPrefabInstance != null && !closed)
					{
						PlaceObjectAtCurveFrame(curveFrame, endPrefabInstance, Space.Self, true);
					}
					num2 += uvScale.y * (Vector3.Distance(array[j], array[num6]) / ((!normalizeV) ? num : curveLength));
					float num7 = ((!thicknessFromParticles) ? thickness : array[j].w) * sectionThicknessScale;
					Vector3 vector6 = curveFrame.position - vector;
					vector6.Normalize();
					Vector3 vector7 = Vector3.Cross(curveFrame.tangent, vector6);
					vector7.Normalize();
					vertices.Add(curveFrame.position + vector7 * num7);
					vertices.Add(curveFrame.position - vector7 * num7);
					normals.Add(-vector6);
					normals.Add(-vector6);
					zero = -vector7;
					zero.w = 1f;
					tangents.Add(zero);
					tangents.Add(zero);
					zero2.Set(0f, num2);
					uvs.Add(zero2);
					zero2.Set(1f, num2);
					uvs.Add(zero2);
					if (j < array.Length - 1)
					{
						tris.Add(num3 * 2);
						tris.Add(num3 * 2 + 1);
						tris.Add((num3 + 1) * 2);
						tris.Add(num3 * 2 + 1);
						tris.Add((num3 + 1) * 2 + 1);
						tris.Add((num3 + 1) * 2);
					}
					num3++;
				}
			}
			CommitMeshData();
		}

		public void UpdateProceduralChainLinks()
		{
			if (linkInstances.Count == 0)
			{
				return;
			}
			ObiDistanceConstraintBatch obiDistanceConstraintBatch = DistanceConstraints.GetBatches()[0] as ObiDistanceConstraintBatch;
			CurveFrame curveFrame = new CurveFrame((0f - sectionTwist) * (float)obiDistanceConstraintBatch.ConstraintCount * uvAnchor);
			int num = -1;
			int num2 = 0;
			for (int i = 0; i < obiDistanceConstraintBatch.ConstraintCount; i++)
			{
				int num3 = obiDistanceConstraintBatch.springIndices[i * 2];
				int num4 = obiDistanceConstraintBatch.springIndices[i * 2 + 1];
				Vector3 particlePosition = GetParticlePosition(num3);
				Vector3 particlePosition2 = GetParticlePosition(num4);
				Vector3 vector = particlePosition2 - particlePosition;
				Vector3 normalized = vector.normalized;
				if (i > 0 && num3 != num)
				{
					if (tearPrefabPool != null && num2 < tearPrefabPool.Length)
					{
						if (!tearPrefabPool[num2].activeSelf)
						{
							tearPrefabPool[num2].SetActive(true);
						}
						PlaceObjectAtCurveFrame(curveFrame, tearPrefabPool[num2], Space.World, true);
						num2++;
					}
					curveFrame.Reset();
				}
				curveFrame.Transport(particlePosition2, normalized, sectionTwist);
				if (i > 0 && num3 != num && tearPrefabPool != null && num2 < tearPrefabPool.Length)
				{
					if (!tearPrefabPool[num2].activeSelf)
					{
						tearPrefabPool[num2].SetActive(true);
					}
					curveFrame.position = particlePosition;
					PlaceObjectAtCurveFrame(curveFrame, tearPrefabPool[num2], Space.World, false);
					num2++;
				}
				if (!closed)
				{
					if (i == 0 && startPrefabInstance != null)
					{
						PlaceObjectAtCurveFrame(curveFrame, startPrefabInstance, Space.World, false);
					}
					else if (i == obiDistanceConstraintBatch.ConstraintCount - 1 && endPrefabInstance != null)
					{
						curveFrame.position = particlePosition2;
						PlaceObjectAtCurveFrame(curveFrame, endPrefabInstance, Space.World, true);
					}
				}
				if (linkInstances[i] != null)
				{
					linkInstances[i].SetActive(true);
					Transform transform = linkInstances[i].transform;
					transform.position = particlePosition + vector * 0.5f;
					transform.localScale = ((!thicknessFromParticles) ? linkScale : (solidRadii[num3] / thickness * linkScale));
					transform.rotation = Quaternion.LookRotation(normalized, curveFrame.normal);
				}
				num = num4;
			}
			for (int j = obiDistanceConstraintBatch.ConstraintCount; j < linkInstances.Count; j++)
			{
				if (linkInstances[j] != null)
				{
					linkInstances[j].SetActive(false);
				}
			}
		}

		public override void ResetActor()
		{
			PushDataToSolver(ParticleData.POSITIONS | ParticleData.VELOCITIES);
			if (particleIndices != null)
			{
				for (int i = 0; i < particleIndices.Length; i++)
				{
					solver.renderablePositions[particleIndices[i]] = positions[i];
				}
			}
			UpdateVisualRepresentation();
		}

		private void ApplyTearing()
		{
			if (!tearable)
			{
				return;
			}
			ObiDistanceConstraintBatch obiDistanceConstraintBatch = DistanceConstraints.GetBatches()[0] as ObiDistanceConstraintBatch;
			float[] array = new float[obiDistanceConstraintBatch.ConstraintCount];
			Oni.GetBatchConstraintForces(obiDistanceConstraintBatch.OniBatch, array, obiDistanceConstraintBatch.ConstraintCount, 0);
			List<int> list = new List<int>();
			for (int i = 0; i < array.Length; i++)
			{
				float num = tearResistance[obiDistanceConstraintBatch.springIndices[i * 2]];
				float num2 = tearResistance[obiDistanceConstraintBatch.springIndices[i * 2 + 1]];
				float num3 = (num + num2) * 0.5f * tearResistanceMultiplier;
				if ((0f - array[i]) * 1000f > num3)
				{
					list.Add(i);
				}
			}
			if (list.Count > 0)
			{
				DistanceConstraints.RemoveFromSolver(null);
				BendingConstraints.RemoveFromSolver(null);
				for (int j = 0; j < list.Count; j++)
				{
					Tear(list[j]);
				}
				BendingConstraints.AddToSolver(this);
				DistanceConstraints.AddToSolver(this);
				BendingConstraints.SetActiveConstraints();
				solver.UpdateActiveParticles();
			}
		}

		public bool DoesBendConstraintSpanDistanceConstraint(ObiDistanceConstraintBatch dbatch, ObiBendConstraintBatch bbatch, int d, int b)
		{
			return (bbatch.bendingIndices[b * 3 + 2] == dbatch.springIndices[d * 2] && bbatch.bendingIndices[b * 3 + 1] == dbatch.springIndices[d * 2 + 1]) || (bbatch.bendingIndices[b * 3 + 1] == dbatch.springIndices[d * 2] && bbatch.bendingIndices[b * 3 + 2] == dbatch.springIndices[d * 2 + 1]) || (bbatch.bendingIndices[b * 3 + 2] == dbatch.springIndices[d * 2] && bbatch.bendingIndices[b * 3] == dbatch.springIndices[d * 2 + 1]) || (bbatch.bendingIndices[b * 3] == dbatch.springIndices[d * 2] && bbatch.bendingIndices[b * 3 + 2] == dbatch.springIndices[d * 2 + 1]);
		}

		public void Tear(int constraintIndex)
		{
			if (usedParticles >= totalParticles)
			{
				return;
			}
			ObiDistanceConstraintBatch obiDistanceConstraintBatch = (ObiDistanceConstraintBatch)DistanceConstraints.GetBatches()[0];
			ObiBendConstraintBatch obiBendConstraintBatch = (ObiBendConstraintBatch)BendingConstraints.GetBatches()[0];
			int num = obiDistanceConstraintBatch.springIndices[constraintIndex * 2];
			int num2 = obiDistanceConstraintBatch.springIndices[constraintIndex * 2 + 1];
			bool flag = (constraintIndex < obiDistanceConstraintBatch.ConstraintCount - 1 && obiDistanceConstraintBatch.springIndices[(constraintIndex + 1) * 2] == num) || (constraintIndex > 0 && obiDistanceConstraintBatch.springIndices[(constraintIndex - 1) * 2 + 1] == num);
			bool flag2 = (constraintIndex < obiDistanceConstraintBatch.ConstraintCount - 1 && obiDistanceConstraintBatch.springIndices[(constraintIndex + 1) * 2] == num2) || (constraintIndex > 0 && obiDistanceConstraintBatch.springIndices[(constraintIndex - 1) * 2 + 1] == num2);
			if ((invMasses[num] > invMasses[num2] || invMasses[num] == 0f) && flag2)
			{
				int num3 = num;
				num = num2;
				num2 = num3;
			}
			if (invMasses[num] == 0f || !flag)
			{
				return;
			}
			invMasses[num] *= 2f;
			positions[usedParticles] = positions[num];
			velocities[usedParticles] = velocities[num];
			active[usedParticles] = active[num];
			invMasses[usedParticles] = invMasses[num];
			solidRadii[usedParticles] = solidRadii[num];
			phases[usedParticles] = phases[num];
			tearResistance[usedParticles] = tearResistance[num];
			restPositions[usedParticles] = positions[num];
			restPositions[usedParticles][3] = 0f;
			Vector4[] array = new Vector4[1] { Vector4.zero };
			Oni.GetParticleVelocities(solver.OniSolver, array, 1, particleIndices[num]);
			Oni.SetParticleVelocities(solver.OniSolver, array, 1, particleIndices[usedParticles]);
			Vector4[] array2 = new Vector4[1] { Vector4.zero };
			Oni.GetParticlePositions(solver.OniSolver, array2, 1, particleIndices[num]);
			Oni.SetParticlePositions(solver.OniSolver, array2, 1, particleIndices[usedParticles]);
			Oni.SetParticleInverseMasses(solver.OniSolver, new float[1] { invMasses[num] }, 1, particleIndices[usedParticles]);
			Oni.SetParticleSolidRadii(solver.OniSolver, new float[1] { solidRadii[num] }, 1, particleIndices[usedParticles]);
			Oni.SetParticlePhases(solver.OniSolver, new int[1] { phases[num] }, 1, particleIndices[usedParticles]);
			for (int i = 0; i < obiBendConstraintBatch.ConstraintCount; i++)
			{
				if (obiBendConstraintBatch.bendingIndices[i * 3 + 2] == num)
				{
					obiBendConstraintBatch.DeactivateConstraint(i);
				}
				else if (!DoesBendConstraintSpanDistanceConstraint(obiDistanceConstraintBatch, obiBendConstraintBatch, constraintIndex, i))
				{
					if (obiBendConstraintBatch.bendingIndices[i * 3] == num)
					{
						obiBendConstraintBatch.bendingIndices[i * 3] = usedParticles;
					}
					else if (obiBendConstraintBatch.bendingIndices[i * 3 + 1] == num)
					{
						obiBendConstraintBatch.bendingIndices[i * 3 + 1] = usedParticles;
					}
				}
			}
			if (constraintIndex < obiDistanceConstraintBatch.ConstraintCount - 1)
			{
				if (obiDistanceConstraintBatch.springIndices[(constraintIndex + 1) * 2] == num)
				{
					obiDistanceConstraintBatch.springIndices[(constraintIndex + 1) * 2] = usedParticles;
				}
				if (obiDistanceConstraintBatch.springIndices[(constraintIndex + 1) * 2 + 1] == num)
				{
					obiDistanceConstraintBatch.springIndices[(constraintIndex + 1) * 2 + 1] = usedParticles;
				}
			}
			if (constraintIndex > 0)
			{
				if (obiDistanceConstraintBatch.springIndices[(constraintIndex - 1) * 2] == num)
				{
					obiDistanceConstraintBatch.springIndices[(constraintIndex - 1) * 2] = usedParticles;
				}
				if (obiDistanceConstraintBatch.springIndices[(constraintIndex - 1) * 2 + 1] == num)
				{
					obiDistanceConstraintBatch.springIndices[(constraintIndex - 1) * 2 + 1] = usedParticles;
				}
			}
			usedParticles++;
			pooledParticles--;
		}

		public override bool GenerateTethers(TetherType type)
		{
			if (!base.Initialized)
			{
				return false;
			}
			TetherConstraints.Clear();
			if (type == TetherType.Hierarchical)
			{
				GenerateHierarchicalTethers(5);
			}
			else
			{
				GenerateFixedTethers(2);
			}
			return true;
		}

		private void GenerateFixedTethers(int maxTethers)
		{
			ObiTetherConstraintBatch obiTetherConstraintBatch = new ObiTetherConstraintBatch(true, false, 0.0001f, 200f);
			TetherConstraints.AddBatch(obiTetherConstraintBatch);
			List<HashSet<int>> list = new List<HashSet<int>>();
			for (int i = 0; i < usedParticles; i++)
			{
				if (invMasses[i] > 0f || !active[i])
				{
					continue;
				}
				int num = -1;
				List<int> list2 = new List<int>();
				int num2 = Mathf.Max(i - 1, 0);
				int num3 = Mathf.Min(i + 1, usedParticles - 1);
				for (int j = 0; j < list.Count; j++)
				{
					if ((active[num2] && list[j].Contains(num2)) || (active[num3] && list[j].Contains(num3)))
					{
						if (num < 0)
						{
							num = j;
							list[j].Add(i);
						}
						else if (num != j && !list2.Contains(j))
						{
							list2.Add(j);
						}
					}
				}
				foreach (int item in list2)
				{
					list[num].UnionWith(list[item]);
				}
				list2.Sort();
				list2.Reverse();
				foreach (int item2 in list2)
				{
					list.RemoveAt(item2);
				}
				if (num < 0)
				{
					list.Add(new HashSet<int> { i });
				}
			}
			for (int k = 0; k < usedParticles; k++)
			{
				if (invMasses[k] == 0f)
				{
					continue;
				}
				List<KeyValuePair<float, int>> list3 = new List<KeyValuePair<float, int>>(list.Count);
				foreach (HashSet<int> item3 in list)
				{
					int num4 = -1;
					float num5 = float.PositiveInfinity;
					foreach (int item4 in item3)
					{
						int num6 = Mathf.Min(k, item4);
						int num7 = Mathf.Max(k, item4);
						float num8 = 0f;
						for (int l = num6; l < num7; l++)
						{
							num8 += Vector3.Distance(positions[l], positions[l + 1]);
						}
						if (num8 < num5)
						{
							num5 = num8;
							num4 = item4;
						}
					}
					if (num4 >= 0)
					{
						list3.Add(new KeyValuePair<float, int>(num5, num4));
					}
				}
				list3.Sort((KeyValuePair<float, int> x, KeyValuePair<float, int> y) => x.Key.CompareTo(y.Key));
				for (int m = 0; m < Mathf.Min(maxTethers, list3.Count); m++)
				{
					obiTetherConstraintBatch.AddConstraint(k, list3[m].Value, list3[m].Key, 1f, 1f);
				}
			}
			obiTetherConstraintBatch.Cook();
		}

		private void GenerateHierarchicalTethers(int maxLevels)
		{
			ObiTetherConstraintBatch obiTetherConstraintBatch = new ObiTetherConstraintBatch(true, false, 0.0001f, 200f);
			TetherConstraints.AddBatch(obiTetherConstraintBatch);
			for (int i = 1; i <= maxLevels; i++)
			{
				int num = i * 2;
				for (int j = 0; j < usedParticles - num; j++)
				{
					int num2 = j + num;
					obiTetherConstraintBatch.AddConstraint(j, num2 % usedParticles, interParticleDistance * (float)num, 1f, 1f);
				}
			}
			obiTetherConstraintBatch.Cook();
		}
	}
}
