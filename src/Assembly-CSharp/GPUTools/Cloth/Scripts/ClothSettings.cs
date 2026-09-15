using System.Collections.Generic;
using GPUTools.Cloth.Scripts.Geometry;
using GPUTools.Cloth.Scripts.Geometry.Data;
using GPUTools.Cloth.Scripts.Geometry.DebugDraw;
using GPUTools.Cloth.Scripts.Geometry.Tools;
using GPUTools.Cloth.Scripts.Runtime;
using GPUTools.Cloth.Scripts.Runtime.Data;
using GPUTools.Painter.Scripts;
using GPUTools.Skinner.Scripts.Providers;
using UnityEngine;

namespace GPUTools.Cloth.Scripts
{
	public class ClothSettings : GPUCollidersConsumer
	{
		[SerializeField]
		public bool GeometryDebugDraw;

		[SerializeField]
		public bool GeometryDebugDrawDistanceJoints = true;

		[SerializeField]
		public bool GeometryDebugDrawStiffnessJoints = true;

		[SerializeField]
		public ClothEditorType EditorType;

		[SerializeField]
		public ClothEditorType EditorStrengthType;

		[SerializeField]
		public Texture2D EditorTexture;

		[SerializeField]
		public Texture2D EditorStrengthTexture;

		[SerializeField]
		public PainterSettings EditorPainter;

		[SerializeField]
		public PainterSettings EditorStrengthPainter;

		[SerializeField]
		public ColorChannel SimulateVsKinematicChannel;

		[SerializeField]
		public ColorChannel StrengthChannel;

		[SerializeField]
		public bool PhysicsDebugDraw;

		[SerializeField]
		public bool IntegrateEnabled = true;

		[SerializeField]
		public Vector3 Gravity = new Vector3(0f, -1f, 0f);

		[SerializeField]
		public float Drag = 0.06f;

		[SerializeField]
		public float Stretchability;

		[SerializeField]
		public float Stiffness = 0.5f;

		[SerializeField]
		public float DistanceScale = 1f;

		[SerializeField]
		public float WorldScale = 1f;

		[SerializeField]
		public float CompressionResistance = 0.5f;

		[SerializeField]
		public float Weight = 1f;

		[SerializeField]
		public float Friction = 0.5f;

		[SerializeField]
		public float StaticMultiplier = 2f;

		[SerializeField]
		public bool CreateNearbyJoints;

		[SerializeField]
		public float NearbyJointsMaxDistance = 0.001f;

		[SerializeField]
		public bool CollisionEnabled = true;

		[SerializeField]
		public float CollisionPower = 0.5f;

		[SerializeField]
		public float GravityMultiplier = 1f;

		[SerializeField]
		public float ParticleRadius = 0.01f;

		[SerializeField]
		public bool BreakEnabled;

		[SerializeField]
		public float BreakThreshold = 0.005f;

		[SerializeField]
		public float JointStrength = 1f;

		[SerializeField]
		public int Iterations = 3;

		[SerializeField]
		public int InnerIterations = 2;

		[SerializeField]
		public float WindMultiplier;

		[SerializeField]
		public List<GameObject> ColliderProviders = new List<GameObject>();

		[SerializeField]
		public List<GameObject> AccessoriesProviders = new List<GameObject>();

		[SerializeField]
		public MeshProvider MeshProvider = new MeshProvider();

		[SerializeField]
		public ClothSphereBrash Brush = new ClothSphereBrash();

		public bool CustomBounds;

		public Bounds Bounds = default(Bounds);

		[SerializeField]
		public ClothGeometryData GeometryData;

		private ClothGeometryImporter geometryImporter;

		private bool _wasInit;

		public Material Material => GetComponent<Renderer>().material;

		public Material SharedMaterial => GetComponent<Renderer>().sharedMaterial;

		public Material[] Materials => GetComponent<Renderer>().materials;

		public Material[] SharedMaterials => GetComponent<Renderer>().sharedMaterials;

		public RuntimeData Runtime { get; private set; }

		public BuildRuntimeCloth builder { get; private set; }

		public void ProcessGeometry()
		{
			ProcessGeometryMainThread();
			ProcessGeometryThreaded();
		}

		public void ProcessGeometryMainThread()
		{
			GeometryData = new ClothGeometryData();
			geometryImporter = new ClothGeometryImporter(this);
			geometryImporter.Cache();
		}

		public void CancelProcessGeometryThreaded()
		{
			if (geometryImporter != null)
			{
				geometryImporter.CancelCacheThreaded();
			}
		}

		public void ProcessGeometryThreaded()
		{
			if (geometryImporter != null)
			{
				geometryImporter.CacheThreaded();
			}
			GeometryData.LogStatistics();
		}

		private void Init()
		{
			if (!_wasInit && MeshProvider.Validate(false) && GeometryData != null && GeometryData.IsProcessed)
			{
				MeshProvider.Dispatch();
				if (Runtime == null)
				{
					Runtime = new RuntimeData();
				}
				if (builder == null)
				{
					builder = new BuildRuntimeCloth(this);
				}
				builder.Build();
				_wasInit = true;
			}
		}

		public void Reset()
		{
			if (builder != null)
			{
				if (builder.particles != null)
				{
					builder.particles.UpdateSettings();
				}
				if (builder.distanceJoints != null)
				{
					builder.distanceJoints.UpdateSettings();
				}
				if (builder.pointJoints != null)
				{
					builder.pointJoints.UpdateSettings();
				}
			}
		}

		private void Start()
		{
			Init();
		}

		private void FixedUpdate()
		{
			if (_wasInit)
			{
				builder.FixedDispatch();
			}
		}

		private void LateUpdate()
		{
			Init();
			if (_wasInit)
			{
				builder.DispatchCopyToOld();
				MeshProvider.Dispatch();
				builder.Dispatch();
				if (MeshProvider.Type == ScalpMeshType.PreCalc && MeshProvider.PreCalcProvider != null)
				{
					MeshProvider.PreCalcProvider.PostProcessDispatch(Runtime.ClothOnlyVertices.ComputeBuffer);
				}
			}
		}

		private void OnDestroy()
		{
			MeshProvider.Dispose();
			if (builder != null)
			{
				builder.Dispose();
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			MeshProvider.Stop();
		}

		public void UpdateSettings()
		{
			if (Application.isPlaying && builder != null)
			{
				builder.UpdateSettings();
			}
		}

		public void OnDrawGizmos()
		{
			if (GeometryDebugDraw)
			{
				ClothDebugDraw.Draw(this);
			}
			ClothDebugDraw.DrawAlways(this);
		}
	}
}
