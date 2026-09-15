using System.Collections.Generic;
using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Hair.Scripts.Runtime.Data;
using GPUTools.Hair.Scripts.Runtime.Kernels;
using GPUTools.Hair.Scripts.Runtime.Render;
using GPUTools.Physics.Scripts.DebugDraw;
using GPUTools.Physics.Scripts.Kernels;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using GPUTools.Physics.Scripts.Types.Shapes;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Runtime.Physics
{
	public class HairPhysicsWorld : PrimitiveBase
	{
		private readonly HairDataFacade data;

		private ResetToPointJointsKernel resetKernel;

		private IntegrateVelocityKernel integrateVelocityKernel;

		private IntegrateVelocityInnerKernel integrateVelocityInnerKernel;

		private IntegrateIterKernel integrateIterKernel;

		private IntegrateIterWithParticleHoldKernel integrateIterWithParticleHoldKernel;

		private DistanceJointsKernel distanceJointsKernel;

		private CompressionJointsKernel compressionJointsKernel;

		private NearbyDistanceJointsKernel nearbyDistanceJointsKernel;

		private ParticleCollisionResetKernel particleCollisionResetKernel;

		private ParticleLineSphereCollisionKernel lineSphereCollisionKernel;

		private ParticleSphereCollisionKernel sphereCollisionKernel;

		private ParticlePlaneCollisionKernel planeCollisionKernel;

		private ParticleLineSphereHoldResetKernel lineSphereHoldResetKernel;

		private ParticleLineSphereHoldKernel lineSphereHoldKernel;

		private ParticleLineSphereGrabKernel lineSphereGrabKernel;

		private ParticleLineSpherePushKernel lineSpherePushKernel;

		private ParticleLineSpherePullKernel lineSpherePullKernel;

		private ParticleLineSphereBrushKernel lineSphereBrushKernel;

		private ParticleLineSphereGrowKernel lineSphereGrowKernel;

		private ParticleLineSphereCutKernel lineSphereCutKernel;

		private ParticleLineSphereRigidityIncreaseKernel lineSphereRigidityIncreaseKernel;

		private ParticleLineSphereRigidityDecreaseKernel lineSphereRigidityDecreaseKernel;

		private ParticleLineSphereRigiditySetKernel lineSphereRigiditySetKernel;

		private DistanceJointsAdjustKernel distanceJointsAdjustKernel;

		private SplineJointsKernel splineJointsKernel;

		private SplineJointsReverseKernel splineJointsReverseKernel;

		private SplineJointsWithParticleHoldKernel splineJointsWithParticleHoldKernel;

		private SplineJointsReverseWithParticleHoldKernel splineJointsReverseWithParticleHoldKernel;

		private PointJointsKernel pointJointsKernel;

		private PointJointsFixedRigidityKernel pointJointsFixedRigidityKernel;

		private PointJointsFinalKernel pointJointsFinalKernel;

		private MovePointJointsToParticlesKernel movePointJointsToParticlesKernel;

		private CopySpecificParticlesKernel copySpecificParticlesKernel;

		private TesselateKernel tesselateKernel;

		private TesselateWithNormalsKernel tesselateWithNormalsKernel;

		private TesselateWithNormalsRenderRigidityKernel tesselateWithNormalsRenderRigidityKernel;

		private int frame;

		private int outerIterations;

		private int iterations;

		private List<KernelBase> staticQueue = new List<KernelBase>();

		private bool isPhysics;

		[GpuData("weight")]
		public GpuValue<float> Weight { get; set; }

		[GpuData("step")]
		public GpuValue<float> Step { get; set; }

		[GpuData("dt")]
		public GpuValue<float> DT { get; set; }

		[GpuData("dtrecip")]
		public GpuValue<float> DTRecip { get; set; }

		[GpuData("t")]
		public GpuValue<float> T { get; set; }

		[GpuData("accelDT2")]
		public GpuValue<Vector3> AccelDT2 { get; set; }

		[GpuData("invDrag")]
		public GpuValue<float> InvDrag { get; set; }

		[GpuData("distanceScale")]
		public GpuValue<float> DistanceScale { get; set; }

		[GpuData("compressionDistanceScale")]
		public GpuValue<float> CompressionDistanceScale { get; set; }

		[GpuData("nearbyDistanceScale")]
		public GpuValue<float> NearbyDistanceScale { get; set; }

		[GpuData("friction")]
		public GpuValue<float> Friction { get; set; }

		[GpuData("staticFriction")]
		public GpuValue<float> StaticFriction { get; set; }

		[GpuData("collisionPower")]
		public GpuValue<float> CollisionPower { get; set; }

		[GpuData("compressionJointPower")]
		public GpuValue<float> CompressionJointPower { get; set; }

		[GpuData("nearbyJointPower")]
		public GpuValue<float> NearbyJointPower { get; set; }

		[GpuData("nearbyJointPowerRolloff")]
		public GpuValue<float> NearbyJointPowerRolloff { get; set; }

		[GpuData("splineJointPower")]
		public GpuValue<float> SplineJointPower { get; set; }

		[GpuData("reverseSplineJointPower")]
		public GpuValue<float> ReverseSplineJointPower { get; set; }

		[GpuData("distanceJointPower")]
		public GpuValue<float> DistanceJointPower { get; set; }

		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("normals")]
		public GpuBuffer<Vector3> Normals { get; set; }

		[GpuData("transforms")]
		public GpuBuffer<Matrix4x4> Transforms { get; set; }

		[GpuData("oldTransforms")]
		public GpuBuffer<Matrix4x4> OldTransforms { get; set; }

		[GpuData("pointJoints")]
		public GpuBuffer<GPPointJoint> PointJoints { get; set; }

		[GpuData("pointToPreviousPointDistances")]
		public GpuBuffer<float> PointToPreviousPointDistances { get; set; }

		[GpuData("isFixed")]
		public GpuValue<int> IsFixed { get; set; }

		[GpuData("fixedRigidity")]
		public GpuValue<float> FixedRigidity { get; set; }

		[GpuData("processedSpheres")]
		public GpuBuffer<GPSphereWithDelta> ProcessedSpheres { get; set; }

		[GpuData("processedLineSpheres")]
		public GpuBuffer<GPLineSphereWithDelta> ProcessedLineSpheres { get; set; }

		[GpuData("planes")]
		public GpuBuffer<Vector4> Planes { get; set; }

		[GpuData("cutLineSpheres")]
		public GpuBuffer<GPLineSphere> CutLineSpheres { get; set; }

		[GpuData("growLineSpheres")]
		public GpuBuffer<GPLineSphere> GrowLineSpheres { get; set; }

		[GpuData("holdLineSpheres")]
		public GpuBuffer<GPLineSphere> HoldLineSpheres { get; set; }

		[GpuData("grabLineSpheres")]
		public GpuBuffer<GPLineSphereWithMatrixDelta> GrabLineSpheres { get; set; }

		[GpuData("pushLineSpheres")]
		public GpuBuffer<GPLineSphere> PushLineSpheres { get; set; }

		[GpuData("pullLineSpheres")]
		public GpuBuffer<GPLineSphere> PullLineSpheres { get; set; }

		[GpuData("brushLineSpheres")]
		public GpuBuffer<GPLineSphereWithDelta> BrushLineSpheres { get; set; }

		[GpuData("rigidityIncreaseLineSpheres")]
		public GpuBuffer<GPLineSphere> RigidityIncreaseLineSpheres { get; set; }

		[GpuData("rigidityDecreaseLineSpheres")]
		public GpuBuffer<GPLineSphere> RigidityDecreaseLineSpheres { get; set; }

		[GpuData("rigiditySetLineSpheres")]
		public GpuBuffer<GPLineSphere> RigiditySetLineSpheres { get; set; }

		[GpuData("outParticles")]
		public GpuBuffer<GPParticle> OutParticles { get; set; }

		[GpuData("outParticlesMap")]
		public GpuBuffer<float> OutParticlesMap { get; set; }

		[GpuData("renderParticles")]
		public GpuBuffer<RenderParticle> RenderParticles { get; set; }

		[GpuData("tessRenderParticles")]
		public GpuBuffer<TessRenderParticle> TessRenderParticles { get; set; }

		[GpuData("tessRenderParticlesCount")]
		public GpuValue<int> TessRenderParticlesCount { get; set; }

		[GpuData("randomsPerStrand")]
		public GpuBuffer<Vector3> RandomsPerStrand { get; set; }

		[GpuData("segments")]
		public GpuValue<int> Segments { get; set; }

		[GpuData("tessSegments")]
		public GpuValue<int> TessSegments { get; set; }

		[GpuData("wavinessAxis")]
		public GpuValue<Vector3> WavinessAxis { get; set; }

		[GpuData("wavinessFrequencyRandomness")]
		public GpuValue<float> WavinessFrequencyRandomness { get; set; }

		[GpuData("wavinessScaleRandomness")]
		public GpuValue<float> WavinessScaleRandomness { get; set; }

		[GpuData("wavinessAllowReverse")]
		public GpuValue<bool> WavinessAllowReverse { get; set; }

		[GpuData("wavinessAllowFlipAxis")]
		public GpuValue<bool> WavinessAllowFlipAxis { get; set; }

		[GpuData("wavinessNormalAdjust")]
		public GpuValue<float> WavinessNormalAdjust { get; set; }

		[GpuData("lightCenter")]
		public GpuValue<Vector3> LightCenter { get; set; }

		[GpuData("normalRandomize")]
		public GpuValue<float> NormalRandomize { get; set; }

		public HairPhysicsWorld(HairDataFacade data)
		{
			this.data = data;
			T = new GpuValue<float>(0f);
			DT = new GpuValue<float>(0f);
			DTRecip = new GpuValue<float>(0f);
			Weight = new GpuValue<float>(0f);
			Step = new GpuValue<float>(0f);
			AccelDT2 = new GpuValue<Vector3>();
			InvDrag = new GpuValue<float>(0f);
			DistanceScale = new GpuValue<float>(0f);
			CompressionDistanceScale = new GpuValue<float>(0f);
			NearbyDistanceScale = new GpuValue<float>(0f);
			Friction = new GpuValue<float>(0f);
			StaticFriction = new GpuValue<float>(0f);
			CollisionPower = new GpuValue<float>(0f);
			CompressionJointPower = new GpuValue<float>(0f);
			NearbyJointPower = new GpuValue<float>(0f);
			NearbyJointPowerRolloff = new GpuValue<float>(0f);
			SplineJointPower = new GpuValue<float>(0f);
			ReverseSplineJointPower = new GpuValue<float>(0f);
			DistanceJointPower = new GpuValue<float>(0f);
			Segments = new GpuValue<int>(0);
			TessSegments = new GpuValue<int>(0);
			TessRenderParticlesCount = new GpuValue<int>(0);
			WavinessAxis = new GpuValue<Vector3>();
			WavinessFrequencyRandomness = new GpuValue<float>(0f);
			WavinessScaleRandomness = new GpuValue<float>(0f);
			WavinessAllowReverse = new GpuValue<bool>(false);
			WavinessAllowFlipAxis = new GpuValue<bool>(false);
			WavinessNormalAdjust = new GpuValue<float>(0f);
			IsFixed = new GpuValue<int>(0);
			FixedRigidity = new GpuValue<float>(0f);
			LightCenter = new GpuValue<Vector3>();
			NormalRandomize = new GpuValue<float>(0f);
			InitData();
			InitBuffers();
			InitPasses();
			Bind();
		}

		private void InitData()
		{
			if (Time.fixedDeltaTime > 0.02f)
			{
				outerIterations = 2;
				iterations = data.Iterations;
			}
			else
			{
				outerIterations = 1;
				iterations = data.Iterations;
			}
			Weight.Value = data.Weight;
			if (data.StyleMode)
			{
				InvDrag.Value = 0f;
			}
			else
			{
				InvDrag.Value = data.InvDrag;
			}
			DistanceScale.Value = 1f;
			CompressionDistanceScale.Value = 1f;
			NearbyDistanceScale.Value = data.WorldScale;
			Friction.Value = data.Friction;
			StaticFriction.Value = data.Friction * 2f;
			CollisionPower.Value = data.CollisionPower;
			CompressionJointPower.Value = data.CompressionJointPower / (float)iterations;
			NearbyJointPower.Value = data.NearbyJointPower * 0.5f / (float)iterations;
			NearbyJointPowerRolloff.Value = data.NearbyJointPowerRolloff;
			ReverseSplineJointPower.Value = data.ReverseSplineJointPower;
			FixedRigidity.Value = 0.1f;
			if (data.StyleMode)
			{
				SplineJointPower.Value = 1f;
			}
			else
			{
				SplineJointPower.Value = Mathf.Min(data.SplineJointPower * 2f / (float)iterations, 1f);
			}
			DistanceJointPower.Value = 0.5f;
			Segments.Value = (int)data.Size.y;
			TessSegments.Value = (int)data.TessFactor.y;
			if (data.Particles != null)
			{
				TessRenderParticlesCount.Value = (int)data.TessFactor.y * data.Particles.Count;
			}
			else
			{
				TessRenderParticlesCount.Value = 0;
			}
			if (data.StyleMode && !data.StyleModeShowCurls)
			{
				WavinessAxis.Value = Vector3.zero;
				WavinessNormalAdjust.Value = 0f;
			}
			else
			{
				WavinessAxis.Value = data.WavinessAxis;
				WavinessNormalAdjust.Value = data.WavinessNormalAdjust * data.WorldScale;
			}
			WavinessFrequencyRandomness.Value = data.WavinessFrequencyRandomness;
			WavinessScaleRandomness.Value = data.WavinessScaleRandomness;
			WavinessAllowReverse.Value = data.WavinessAllowReverse;
			WavinessAllowFlipAxis.Value = data.WavinessAllowFlipAxis;
			IsFixed.Value = ((!isPhysics) ? 1 : 0);
			LightCenter.Value = data.LightCenter;
			NormalRandomize.Value = data.NormalRandomize;
		}

		private void InitBuffers()
		{
			Particles = data.Particles;
			Normals = data.NormalsBuffer;
			Transforms = data.MatricesBuffer;
			if (Transforms != null)
			{
				OldTransforms = new GpuBuffer<Matrix4x4>(Transforms.Count, 64);
			}
			PointJoints = data.PointJoints;
			PointToPreviousPointDistances = data.PointToPreviousPointDistances;
			ProcessedSpheres = data.ProcessedSpheres;
			ProcessedLineSpheres = data.ProcessedLineSpheres;
			CutLineSpheres = data.CutLineSpheres;
			GrowLineSpheres = data.GrowLineSpheres;
			HoldLineSpheres = data.HoldLineSpheres;
			GrabLineSpheres = data.GrabLineSpheres;
			PushLineSpheres = data.PushLineSpheres;
			PullLineSpheres = data.PullLineSpheres;
			BrushLineSpheres = data.BrushLineSpheres;
			RigidityIncreaseLineSpheres = data.RigidityIncreaseLineSpheres;
			RigidityDecreaseLineSpheres = data.RigidityDecreaseLineSpheres;
			RigiditySetLineSpheres = data.RigiditySetLineSpheres;
			RenderParticles = data.RenderParticles;
			TessRenderParticles = data.TessRenderParticles;
			RandomsPerStrand = data.RandomsPerStrand;
			OutParticles = data.OutParticles;
			OutParticlesMap = data.OutParticlesMap;
		}

		private void UpdateBuffers()
		{
			if (Transforms != data.MatricesBuffer)
			{
				Transforms = data.MatricesBuffer;
				if (resetKernel != null)
				{
					resetKernel.Transforms = Transforms;
					resetKernel.ClearCacheAttributes();
				}
				if (pointJointsKernel != null)
				{
					pointJointsKernel.Transforms = Transforms;
					pointJointsKernel.ClearCacheAttributes();
				}
				if (pointJointsFixedRigidityKernel != null)
				{
					pointJointsFixedRigidityKernel.Transforms = Transforms;
					pointJointsFixedRigidityKernel.ClearCacheAttributes();
				}
				if (pointJointsFinalKernel != null)
				{
					pointJointsFinalKernel.Transforms = Transforms;
					pointJointsFinalKernel.ClearCacheAttributes();
				}
				if (movePointJointsToParticlesKernel != null)
				{
					movePointJointsToParticlesKernel.Transforms = Transforms;
					movePointJointsToParticlesKernel.ClearCacheAttributes();
				}
				if (tesselateKernel != null)
				{
					tesselateKernel.Transforms = Transforms;
					tesselateKernel.ClearCacheAttributes();
				}
				else if (tesselateWithNormalsKernel != null)
				{
					tesselateWithNormalsKernel.Transforms = Transforms;
					tesselateWithNormalsKernel.ClearCacheAttributes();
					if (tesselateWithNormalsRenderRigidityKernel != null)
					{
						tesselateWithNormalsRenderRigidityKernel.Transforms = Transforms;
						tesselateWithNormalsRenderRigidityKernel.ClearCacheAttributes();
					}
				}
			}
			if (Normals != data.NormalsBuffer)
			{
				Normals = data.NormalsBuffer;
				if (tesselateWithNormalsKernel != null)
				{
					tesselateWithNormalsKernel.Normals = Normals;
					tesselateWithNormalsKernel.ClearCacheAttributes();
					if (tesselateWithNormalsRenderRigidityKernel != null)
					{
						tesselateWithNormalsRenderRigidityKernel.Normals = Normals;
						tesselateWithNormalsRenderRigidityKernel.ClearCacheAttributes();
					}
				}
			}
			if (ProcessedLineSpheres != data.ProcessedLineSpheres)
			{
				ProcessedLineSpheres = data.ProcessedLineSpheres;
				lineSphereCollisionKernel.ProcessedLineSpheres = ProcessedLineSpheres;
				lineSphereCollisionKernel.ClearCacheAttributes();
			}
			if (ProcessedSpheres != data.ProcessedSpheres)
			{
				ProcessedSpheres = data.ProcessedSpheres;
				sphereCollisionKernel.ProcessedSpheres = ProcessedSpheres;
				sphereCollisionKernel.ClearCacheAttributes();
			}
			if (Planes != data.Planes)
			{
				Planes = data.Planes;
				planeCollisionKernel.Planes = Planes;
				planeCollisionKernel.ClearCacheAttributes();
			}
			if (GrowLineSpheres != data.GrowLineSpheres)
			{
				GrowLineSpheres = data.GrowLineSpheres;
				lineSphereGrowKernel.GrowLineSpheres = GrowLineSpheres;
				lineSphereGrowKernel.ClearCacheAttributes();
			}
			if (CutLineSpheres != data.CutLineSpheres)
			{
				CutLineSpheres = data.CutLineSpheres;
				lineSphereCutKernel.CutLineSpheres = CutLineSpheres;
				lineSphereCutKernel.ClearCacheAttributes();
			}
			if (PushLineSpheres != data.PushLineSpheres)
			{
				PushLineSpheres = data.PushLineSpheres;
				lineSpherePushKernel.PushLineSpheres = PushLineSpheres;
				lineSpherePushKernel.ClearCacheAttributes();
			}
			if (PullLineSpheres != data.PullLineSpheres)
			{
				PullLineSpheres = data.PullLineSpheres;
				lineSpherePullKernel.PullLineSpheres = PullLineSpheres;
				lineSpherePullKernel.ClearCacheAttributes();
			}
			if (BrushLineSpheres != data.BrushLineSpheres)
			{
				BrushLineSpheres = data.BrushLineSpheres;
				lineSphereBrushKernel.BrushLineSpheres = BrushLineSpheres;
				lineSphereBrushKernel.ClearCacheAttributes();
			}
			if (HoldLineSpheres != data.HoldLineSpheres)
			{
				HoldLineSpheres = data.HoldLineSpheres;
				lineSphereHoldKernel.HoldLineSpheres = HoldLineSpheres;
				lineSphereHoldKernel.ClearCacheAttributes();
			}
			if (GrabLineSpheres != data.GrabLineSpheres)
			{
				GrabLineSpheres = data.GrabLineSpheres;
				lineSphereGrabKernel.GrabLineSpheres = GrabLineSpheres;
				lineSphereGrabKernel.ClearCacheAttributes();
			}
			if (RigidityIncreaseLineSpheres != data.RigidityIncreaseLineSpheres)
			{
				RigidityIncreaseLineSpheres = data.RigidityIncreaseLineSpheres;
				lineSphereRigidityIncreaseKernel.RigidityIncreaseLineSpheres = RigidityIncreaseLineSpheres;
				lineSphereRigidityIncreaseKernel.ClearCacheAttributes();
			}
			if (RigidityDecreaseLineSpheres != data.RigidityDecreaseLineSpheres)
			{
				RigidityDecreaseLineSpheres = data.RigidityDecreaseLineSpheres;
				lineSphereRigidityDecreaseKernel.RigidityDecreaseLineSpheres = RigidityDecreaseLineSpheres;
				lineSphereRigidityDecreaseKernel.ClearCacheAttributes();
			}
			if (RigiditySetLineSpheres != data.RigiditySetLineSpheres)
			{
				RigiditySetLineSpheres = data.RigiditySetLineSpheres;
				lineSphereRigiditySetKernel.RigiditySetLineSpheres = RigiditySetLineSpheres;
				lineSphereRigiditySetKernel.ClearCacheAttributes();
			}
		}

		private void AddStaticPass(KernelBase kernel)
		{
			AddPass(kernel);
			staticQueue.Add(kernel);
		}

		private void InitPasses()
		{
			AddPass(integrateVelocityKernel = new IntegrateVelocityKernel());
			AddPass(integrateVelocityInnerKernel = new IntegrateVelocityInnerKernel());
			AddPass(integrateIterKernel = new IntegrateIterKernel());
			AddPass(integrateIterWithParticleHoldKernel = new IntegrateIterWithParticleHoldKernel());
			AddPass(distanceJointsKernel = new DistanceJointsKernel(data.DistanceJoints, data.DistanceJointsBuffer));
			AddPass(compressionJointsKernel = new CompressionJointsKernel(data.CompressionJoints, data.CompressionJointsBuffer));
			AddPass(nearbyDistanceJointsKernel = new NearbyDistanceJointsKernel(data.NearbyDistanceJoints, data.NearbyDistanceJointsBuffer));
			AddPass(splineJointsKernel = new SplineJointsKernel());
			AddPass(splineJointsReverseKernel = new SplineJointsReverseKernel());
			AddPass(splineJointsWithParticleHoldKernel = new SplineJointsWithParticleHoldKernel());
			AddPass(splineJointsReverseWithParticleHoldKernel = new SplineJointsReverseWithParticleHoldKernel());
			AddPass(particleCollisionResetKernel = new ParticleCollisionResetKernel());
			AddPass(planeCollisionKernel = new ParticlePlaneCollisionKernel());
			AddPass(sphereCollisionKernel = new ParticleSphereCollisionKernel());
			AddPass(lineSphereCollisionKernel = new ParticleLineSphereCollisionKernel());
			AddPass(lineSphereHoldResetKernel = new ParticleLineSphereHoldResetKernel());
			AddPass(lineSphereHoldKernel = new ParticleLineSphereHoldKernel());
			AddPass(lineSphereGrabKernel = new ParticleLineSphereGrabKernel());
			AddPass(lineSpherePushKernel = new ParticleLineSpherePushKernel());
			AddPass(lineSpherePullKernel = new ParticleLineSpherePullKernel());
			AddPass(lineSphereBrushKernel = new ParticleLineSphereBrushKernel());
			AddPass(lineSphereGrowKernel = new ParticleLineSphereGrowKernel());
			AddPass(lineSphereCutKernel = new ParticleLineSphereCutKernel());
			AddPass(lineSphereRigidityIncreaseKernel = new ParticleLineSphereRigidityIncreaseKernel());
			AddPass(lineSphereRigidityDecreaseKernel = new ParticleLineSphereRigidityDecreaseKernel());
			AddPass(lineSphereRigiditySetKernel = new ParticleLineSphereRigiditySetKernel());
			AddPass(distanceJointsAdjustKernel = new DistanceJointsAdjustKernel(data.DistanceJoints, data.DistanceJointsBuffer));
			AddPass(pointJointsKernel = new PointJointsKernel());
			AddPass(pointJointsFixedRigidityKernel = new PointJointsFixedRigidityKernel());
			AddPass(movePointJointsToParticlesKernel = new MovePointJointsToParticlesKernel());
			AddStaticPass(pointJointsFinalKernel = new PointJointsFinalKernel());
			if (data.OutParticles != null)
			{
				AddStaticPass(copySpecificParticlesKernel = new CopySpecificParticlesKernel());
			}
			if (data.NormalsBuffer != null)
			{
				AddStaticPass(tesselateWithNormalsKernel = new TesselateWithNormalsKernel());
				AddStaticPass(tesselateWithNormalsRenderRigidityKernel = new TesselateWithNormalsRenderRigidityKernel());
			}
			else
			{
				AddStaticPass(tesselateKernel = new TesselateKernel());
			}
			AddStaticPass(resetKernel = new ResetToPointJointsKernel());
		}

		public void RebindData()
		{
			Particles = data.Particles;
			Transforms = data.MatricesBuffer;
			Normals = data.NormalsBuffer;
			if (OldTransforms != null)
			{
				OldTransforms.Dispose();
			}
			if (Transforms != null)
			{
				OldTransforms = new GpuBuffer<Matrix4x4>(Transforms.Count, 64);
			}
			else
			{
				OldTransforms = null;
			}
			PointJoints = data.PointJoints;
			PointToPreviousPointDistances = data.PointToPreviousPointDistances;
			RenderParticles = data.RenderParticles;
			TessRenderParticles = data.TessRenderParticles;
			RandomsPerStrand = data.RandomsPerStrand;
			BindAttributes();
			integrateIterKernel.ClearCacheAttributes();
			integrateIterWithParticleHoldKernel.ClearCacheAttributes();
			integrateVelocityKernel.ClearCacheAttributes();
			integrateVelocityInnerKernel.ClearCacheAttributes();
			distanceJointsKernel.DistanceJoints = data.DistanceJoints;
			distanceJointsKernel.DistanceJointsBuffer = data.DistanceJointsBuffer;
			distanceJointsKernel.ClearCacheAttributes();
			compressionJointsKernel.CompressionJoints = data.CompressionJoints;
			compressionJointsKernel.CompressionJointsBuffer = data.CompressionJointsBuffer;
			compressionJointsKernel.ClearCacheAttributes();
			nearbyDistanceJointsKernel.NearbyDistanceJoints = data.NearbyDistanceJoints;
			nearbyDistanceJointsKernel.NearbyDistanceJointsBuffer = data.NearbyDistanceJointsBuffer;
			nearbyDistanceJointsKernel.ClearCacheAttributes();
			splineJointsKernel.ClearCacheAttributes();
			splineJointsReverseKernel.ClearCacheAttributes();
			splineJointsWithParticleHoldKernel.ClearCacheAttributes();
			splineJointsReverseWithParticleHoldKernel.ClearCacheAttributes();
			particleCollisionResetKernel.ClearCacheAttributes();
			planeCollisionKernel.ClearCacheAttributes();
			sphereCollisionKernel.ClearCacheAttributes();
			lineSphereCollisionKernel.ClearCacheAttributes();
			lineSphereHoldResetKernel.ClearCacheAttributes();
			lineSphereHoldKernel.ClearCacheAttributes();
			lineSphereGrabKernel.ClearCacheAttributes();
			lineSpherePushKernel.ClearCacheAttributes();
			lineSpherePullKernel.ClearCacheAttributes();
			lineSphereBrushKernel.ClearCacheAttributes();
			lineSphereGrowKernel.ClearCacheAttributes();
			lineSphereCutKernel.ClearCacheAttributes();
			lineSphereRigidityIncreaseKernel.ClearCacheAttributes();
			lineSphereRigidityDecreaseKernel.ClearCacheAttributes();
			lineSphereRigiditySetKernel.ClearCacheAttributes();
			distanceJointsAdjustKernel.DistanceJoints = data.DistanceJoints;
			distanceJointsAdjustKernel.DistanceJointsBuffer = data.DistanceJointsBuffer;
			distanceJointsAdjustKernel.ClearCacheAttributes();
			pointJointsKernel.ClearCacheAttributes();
			pointJointsFixedRigidityKernel.ClearCacheAttributes();
			pointJointsFinalKernel.ClearCacheAttributes();
			movePointJointsToParticlesKernel.ClearCacheAttributes();
			if (copySpecificParticlesKernel != null)
			{
				copySpecificParticlesKernel.ClearCacheAttributes();
			}
			if (tesselateKernel != null)
			{
				tesselateKernel.ClearCacheAttributes();
			}
			else if (tesselateWithNormalsKernel != null)
			{
				tesselateWithNormalsKernel.ClearCacheAttributes();
				if (tesselateWithNormalsRenderRigidityKernel != null)
				{
					tesselateWithNormalsRenderRigidityKernel.ClearCacheAttributes();
				}
			}
			resetKernel.ClearCacheAttributes();
		}

		public void Reset()
		{
			frame = 0;
		}

		public void PartialReset()
		{
			if (frame > 10)
			{
				frame = 10;
			}
		}

		public void FixedDispatch()
		{
			if (isPhysics && !data.RunPhysicsOnUpdate)
			{
				DispatchPhysicsImpl();
			}
		}

		public override void Dispatch()
		{
			UpdateIsPhysics();
			if (!isPhysics)
			{
				DispatchStaticImpl();
			}
			else if (data.RunPhysicsOnUpdate)
			{
				DispatchPhysicsImpl();
			}
			DispatchImpl();
		}

		private void DispatchPhysicsImpl()
		{
			InitData();
			UpdateBuffers();
			if (frame < 10)
			{
				resetKernel.Dispatch();
				particleCollisionResetKernel.Dispatch();
				return;
			}
			for (int i = 1; i <= outerIterations; i++)
			{
				float num = Time.fixedDeltaTime * Mathf.Max(data.Weight, 0.01f) / (float)outerIterations;
				Step.Value = 1f / (float)(iterations * outerIterations);
				float num2 = num / (float)iterations;
				DT.Value = num2;
				DTRecip.Value = 1f / num2;
				AccelDT2.Value = num2 * num2 * 0.5f * (data.Gravity + data.Wind);
				if (data.StyleMode)
				{
					lineSphereHoldResetKernel.Dispatch();
					if (lineSphereHoldKernel.HoldLineSpheres != null)
					{
						lineSphereHoldKernel.Dispatch();
					}
					if (lineSphereBrushKernel.BrushLineSpheres != null)
					{
						lineSphereBrushKernel.Dispatch();
					}
					splineJointsReverseWithParticleHoldKernel.Dispatch();
					splineJointsWithParticleHoldKernel.Dispatch();
					if (lineSphereBrushKernel.BrushLineSpheres != null)
					{
						lineSphereBrushKernel.Dispatch();
					}
					if (lineSpherePullKernel.PullLineSpheres != null)
					{
						lineSpherePullKernel.Dispatch();
					}
					if (lineSpherePushKernel.PushLineSpheres != null)
					{
						lineSpherePushKernel.Dispatch();
					}
					if (lineSphereGrabKernel.GrabLineSpheres != null)
					{
						lineSphereGrabKernel.Dispatch();
					}
					splineJointsReverseWithParticleHoldKernel.Dispatch();
					splineJointsWithParticleHoldKernel.Dispatch();
					if (lineSphereGrowKernel.GrowLineSpheres != null)
					{
						lineSphereGrowKernel.Dispatch();
						splineJointsReverseKernel.Dispatch();
					}
					for (int j = 0; j < 4; j++)
					{
						if (lineSphereCutKernel.CutLineSpheres != null)
						{
							lineSphereCutKernel.Dispatch();
							splineJointsKernel.Dispatch();
						}
					}
					movePointJointsToParticlesKernel.Dispatch();
					for (int k = 1; k <= iterations; k++)
					{
						T.Value = (float)k / (float)iterations;
						if (frame > 20)
						{
							integrateIterWithParticleHoldKernel.Dispatch();
						}
						pointJointsFixedRigidityKernel.Dispatch();
						splineJointsWithParticleHoldKernel.Dispatch();
						splineJointsReverseWithParticleHoldKernel.Dispatch();
						integrateVelocityInnerKernel.Dispatch();
					}
					Step.Value = 1f / (float)outerIterations;
					DT.Value = num;
					DTRecip.Value = 1f / num;
					particleCollisionResetKernel.Dispatch();
					if (data.IsCollisionEnabled)
					{
						if (planeCollisionKernel.Planes != null)
						{
							planeCollisionKernel.Dispatch();
						}
						if (sphereCollisionKernel.ProcessedSpheres != null)
						{
							sphereCollisionKernel.Dispatch();
						}
						if (lineSphereCollisionKernel.ProcessedLineSpheres != null)
						{
							lineSphereCollisionKernel.Dispatch();
						}
					}
					pointJointsFixedRigidityKernel.Dispatch();
					splineJointsWithParticleHoldKernel.Dispatch();
					splineJointsReverseWithParticleHoldKernel.Dispatch();
					integrateVelocityKernel.Dispatch();
					if (data.UsePaintedRigidity)
					{
						if (lineSphereRigidityIncreaseKernel.RigidityIncreaseLineSpheres != null)
						{
							lineSphereRigidityIncreaseKernel.Dispatch();
						}
						if (lineSphereRigidityDecreaseKernel.RigidityDecreaseLineSpheres != null)
						{
							lineSphereRigidityDecreaseKernel.Dispatch();
						}
						if (lineSphereRigiditySetKernel.RigiditySetLineSpheres != null)
						{
							lineSphereRigiditySetKernel.Dispatch();
						}
					}
					continue;
				}
				for (int l = 1; l <= iterations; l++)
				{
					T.Value = (float)l / (float)iterations;
					if (frame > 20)
					{
						integrateIterKernel.Dispatch();
					}
					pointJointsKernel.Dispatch();
					distanceJointsKernel.Dispatch();
					if (CompressionJointPower.Value != 0f)
					{
						compressionJointsKernel.Dispatch();
					}
					if (NearbyJointPower.Value != 0f)
					{
						nearbyDistanceJointsKernel.Dispatch();
					}
					distanceJointsKernel.Dispatch();
					splineJointsKernel.Dispatch();
					distanceJointsKernel.Dispatch();
					integrateVelocityInnerKernel.Dispatch();
				}
				Step.Value = 1f / (float)outerIterations;
				DT.Value = num;
				DTRecip.Value = 1f / num;
				particleCollisionResetKernel.Dispatch();
				if (data.IsCollisionEnabled)
				{
					if (planeCollisionKernel.Planes != null)
					{
						planeCollisionKernel.Dispatch();
					}
					if (sphereCollisionKernel.ProcessedSpheres != null)
					{
						sphereCollisionKernel.Dispatch();
					}
					if (lineSphereCollisionKernel.ProcessedLineSpheres != null)
					{
						lineSphereCollisionKernel.Dispatch();
					}
				}
				pointJointsKernel.Dispatch();
				integrateVelocityKernel.Dispatch();
			}
		}

		private void DispatchStaticImpl()
		{
			T.Value = 1f;
			for (int i = 0; i < staticQueue.Count; i++)
			{
				staticQueue[i].Dispatch();
			}
		}

		private void DispatchImpl()
		{
			InitData();
			UpdateBuffers();
			Step.Value = 1f;
			if (frame < 1)
			{
				resetKernel.Dispatch();
			}
			if (!data.StyleMode && data.UpdateRigidityJointsBeforeRender)
			{
				pointJointsFinalKernel.Dispatch();
			}
			if (copySpecificParticlesKernel != null)
			{
				copySpecificParticlesKernel.Dispatch();
			}
			if (tesselateKernel != null)
			{
				tesselateKernel.Dispatch();
			}
			else if (tesselateWithNormalsKernel != null)
			{
				if (data.StyleMode && tesselateWithNormalsRenderRigidityKernel != null)
				{
					tesselateWithNormalsRenderRigidityKernel.Dispatch();
				}
				else
				{
					tesselateWithNormalsKernel.Dispatch();
				}
			}
			frame++;
		}

		public override void Dispose()
		{
			base.Dispose();
			if (OldTransforms != null)
			{
				OldTransforms.Dispose();
			}
		}

		public void DebugDraw()
		{
			if (data.DebugDraw && data.IsPhysicsEnabled && data.IsPhysicsEnabledLOD && nearbyDistanceJointsKernel != null)
			{
				GPDebugDraw.Draw(distanceJointsKernel.DistanceJointsBuffer, nearbyDistanceJointsKernel.NearbyDistanceJointsBuffer, Particles, false, !data.DebugDrawNearbyJoints, data.DebugDrawNearbyJoints);
			}
		}

		public void UpdateIsPhysics()
		{
			bool flag = (data.IsPhysicsEnabled && data.IsPhysicsEnabledLOD) || data.StyleMode;
			if (!isPhysics && flag)
			{
				resetKernel.Dispatch();
			}
			isPhysics = flag;
		}
	}
}
