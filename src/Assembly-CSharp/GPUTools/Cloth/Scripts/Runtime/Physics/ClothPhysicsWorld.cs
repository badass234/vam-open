using GPUTools.Cloth.Scripts.Runtime.Data;
using GPUTools.Cloth.Scripts.Runtime.Kernels;
using GPUTools.Cloth.Scripts.Types;
using GPUTools.Common.Scripts.PL.Abstract;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Common.Scripts.Tools.Kernels;
using GPUTools.Physics.Scripts.DebugDraw;
using GPUTools.Physics.Scripts.Kernels;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using GPUTools.Physics.Scripts.Types.Shapes;
using GPUTools.Skinner.Scripts.Providers;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Runtime.Physics
{
	public class ClothPhysicsWorld : PrimitiveBase
	{
		private readonly ClothDataFacade data;

		private GPUMatrixCopyPaster matrixCopyPaster;

		private GPUVector3CopyPaster vector3CopyPaster;

		private ResetToPointJointsKernel resetKernel;

		private ResetToPointJointsPreCalculatedKernel resetToPointJointsPreCalculatedKernel;

		private IntegrateVelocityKernel integrateVelocityKernel;

		private IntegrateVelocityInnerKernel integrateVelocityInnerKernel;

		private IntegrateIterKernel integrateIterKernel;

		private StiffnessJointsKernel stiffnessJointsKernel;

		private StiffnessJointsKernel nearbyJointsKernel;

		private ParticleCollisionResetKernel particleCollisionResetKernel;

		private ParticleNeiborsCollisionKernel particleNeiborsCollisionKernel;

		private ParticlePlaneCollisionKernel particlePlaneCollisionKernel;

		private ParticleSphereCollisionKernel sphereCollisionKernel;

		private ParticleLineSphereCollisionKernel lineSphereCollisionKernel;

		private PointJointsPreCalculatedKernel pointJointsPreCalculatedKernel;

		private PointJointsKernel pointJointsKernel;

		private GrabJointsKernel grabJointsKernel;

		private PointJointsPreCalculatedFinalKernel pointJointsPreCalculatedFinalKernel;

		private PointJointsFinalKernel pointJointsFinalKernel;

		private CopySpecificParticlesKernel copySpecificParticlesKernel;

		private CreateVertexDataKernel createVertexDataKernel;

		private CreateVertexOnlyDataKernel createVertexOnlyDataKernel;

		private int frame;

		private int outerIterations;

		private int iterations;

		private bool usesPreCalcVerts;

		protected int fixedDispatchCount;

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

		[GpuData("stiffness")]
		public GpuValue<float> Stiffness { get; set; }

		[GpuData("distanceScale")]
		public GpuValue<float> DistanceScale { get; set; }

		[GpuData("compressionResistance")]
		public GpuValue<float> CompressionResistance { get; set; }

		[GpuData("breakThreshold")]
		public GpuValue<float> BreakThreshold { get; set; }

		[GpuData("jointStrength")]
		public GpuValue<float> JointStrength { get; set; }

		[GpuData("jointPrediction")]
		public GpuValue<float> JointPrediction { get; set; }

		[GpuData("friction")]
		public GpuValue<float> Friction { get; set; }

		[GpuData("staticFriction")]
		public GpuValue<float> StaticFriction { get; set; }

		[GpuData("collisionPower")]
		public GpuValue<float> CollisionPower { get; set; }

		[GpuData("particles")]
		public GpuBuffer<GPParticle> Particles { get; set; }

		[GpuData("transforms")]
		public GpuBuffer<Matrix4x4> Transforms { get; set; }

		[GpuData("oldTransforms")]
		public GpuBuffer<Matrix4x4> OldTransforms { get; set; }

		[GpuData("positions")]
		public GpuBuffer<Vector3> PreCalculatedPositions { get; set; }

		[GpuData("oldPositions")]
		public GpuBuffer<Vector3> OldPreCalculatedPositions { get; set; }

		[GpuData("pointJoints")]
		public GpuBuffer<GPPointJoint> PointJoints { get; set; }

		[GpuData("allPointJoints")]
		public GpuBuffer<GPPointJoint> AllPointJoints { get; set; }

		[GpuData("grabSpheres")]
		public GpuBuffer<GPGrabSphere> GrabSpheres { get; set; }

		[GpuData("processedSpheres")]
		public GpuBuffer<GPSphereWithDelta> ProcessedSpheres { get; set; }

		[GpuData("processedLineSpheres")]
		public GpuBuffer<GPLineSphereWithDelta> ProcessedLineSpheres { get; set; }

		[GpuData("planes")]
		public GpuBuffer<Vector4> Planes { get; set; }

		[GpuData("outParticles")]
		public GpuBuffer<GPParticle> OutParticles { get; set; }

		[GpuData("outParticlesMap")]
		public GpuBuffer<float> OutParticlesMap { get; set; }

		[GpuData("clothVertices")]
		public GpuBuffer<ClothVertex> ClothVertices { get; set; }

		[GpuData("clothOnlyVertices")]
		public GpuBuffer<Vector3> ClothOnlyVertices { get; set; }

		[GpuData("meshToPhysicsVerticesMap")]
		public GpuBuffer<int> MeshToPhysicsVerticesMap { get; set; }

		[GpuData("meshVertexToNeiborsMap")]
		public GpuBuffer<int> MeshVertexToNeiborsMap { get; set; }

		[GpuData("meshVertexToNeiborsMapCounts")]
		public GpuBuffer<int> MeshVertexToNeiborsMapCounts { get; set; }

		public ClothPhysicsWorld(ClothDataFacade data)
		{
			this.data = data;
			T = new GpuValue<float>(0f);
			DT = new GpuValue<float>(0f);
			DTRecip = new GpuValue<float>(0f);
			Weight = new GpuValue<float>(0f);
			Step = new GpuValue<float>(0f);
			AccelDT2 = new GpuValue<Vector3>();
			InvDrag = new GpuValue<float>(0f);
			Stiffness = new GpuValue<float>(0f);
			CompressionResistance = new GpuValue<float>(0f);
			DistanceScale = new GpuValue<float>(0f);
			BreakThreshold = new GpuValue<float>(0f);
			JointStrength = new GpuValue<float>(0f);
			JointPrediction = new GpuValue<float>(0f);
			Friction = new GpuValue<float>(0f);
			StaticFriction = new GpuValue<float>(0f);
			CollisionPower = new GpuValue<float>(0f);
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
			InvDrag.Value = data.InvDrag;
			Stiffness.Value = data.Stiffness;
			CompressionResistance.Value = data.CompressionResistance;
			DistanceScale.Value = data.DistanceScale * data.WorldScale;
			Friction.Value = data.Friction;
			StaticFriction.Value = data.Friction * data.StaticMultiplier;
			CollisionPower.Value = data.CollisionPower;
			if (fixedDispatchCount == 1)
			{
				JointPrediction.Value = 1f;
			}
			else if (fixedDispatchCount == 2)
			{
				JointPrediction.Value = 1.05f;
			}
			else
			{
				JointPrediction.Value = 1.1f;
			}
			JointStrength.Value = data.JointStrength;
			if (data.BreakEnabled && frame > 15)
			{
				BreakThreshold.Value = data.BreakThreshold;
			}
			else
			{
				BreakThreshold.Value = 1000000f;
			}
		}

		private void InitBuffers()
		{
			usesPreCalcVerts = data.MeshProvider.Type == ScalpMeshType.PreCalc;
			Particles = data.Particles;
			if (usesPreCalcVerts)
			{
				PreCalculatedPositions = data.PreCalculatedVerticesBuffer;
				OldPreCalculatedPositions = new GpuBuffer<Vector3>(PreCalculatedPositions.Data, 12);
				OldPreCalculatedPositions.PushData();
			}
			else
			{
				Transforms = data.MatricesBuffer;
				OldTransforms = new GpuBuffer<Matrix4x4>(Transforms.Data, 64);
				OldTransforms.PushData();
			}
			AllPointJoints = data.AllPointJoints;
			PointJoints = data.PointJoints;
			ProcessedSpheres = data.ProcessedSpheres;
			ProcessedLineSpheres = data.ProcessedLineSpheres;
			Planes = data.Planes;
			GrabSpheres = data.GrabSpheres;
			ClothVertices = data.ClothVertices;
			ClothOnlyVertices = data.ClothOnlyVertices;
			MeshToPhysicsVerticesMap = data.MeshToPhysicsVerticesMap;
			MeshVertexToNeiborsMap = data.MeshVertexToNeiborsMap;
			MeshVertexToNeiborsMapCounts = data.MeshVertexToNeiborsMapCounts;
			OutParticles = data.OutParticles;
			OutParticlesMap = data.OutParticlesMap;
		}

		private void UpdateBuffers()
		{
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
				particlePlaneCollisionKernel.Planes = Planes;
				particlePlaneCollisionKernel.ClearCacheAttributes();
			}
			if (GrabSpheres != data.GrabSpheres)
			{
				GrabSpheres = data.GrabSpheres;
				grabJointsKernel.GrabSpheres = GrabSpheres;
				grabJointsKernel.ClearCacheAttributes();
			}
			if (usesPreCalcVerts && PreCalculatedPositions != data.PreCalculatedVerticesBuffer)
			{
				PreCalculatedPositions = data.PreCalculatedVerticesBuffer;
				vector3CopyPaster.Vector3From = PreCalculatedPositions;
				vector3CopyPaster.ClearCacheAttributes();
				pointJointsPreCalculatedKernel.Positions = PreCalculatedPositions;
				pointJointsPreCalculatedKernel.ClearCacheAttributes();
				pointJointsPreCalculatedFinalKernel.Positions = PreCalculatedPositions;
				pointJointsPreCalculatedFinalKernel.ClearCacheAttributes();
				resetToPointJointsPreCalculatedKernel.Positions = PreCalculatedPositions;
				resetToPointJointsPreCalculatedKernel.ClearCacheAttributes();
				resetToPointJointsPreCalculatedKernel.Dispatch();
			}
		}

		private void InitPasses()
		{
			if (usesPreCalcVerts)
			{
				AddPass(resetToPointJointsPreCalculatedKernel = new ResetToPointJointsPreCalculatedKernel());
			}
			else
			{
				AddPass(resetKernel = new ResetToPointJointsKernel());
			}
			AddPass(integrateVelocityKernel = new IntegrateVelocityKernel());
			AddPass(integrateVelocityInnerKernel = new IntegrateVelocityInnerKernel());
			AddPass(integrateIterKernel = new IntegrateIterKernel());
			AddPass(stiffnessJointsKernel = new StiffnessJointsKernel(data.StiffnessJoints, data.StiffnessJointsBuffer));
			AddPass(nearbyJointsKernel = new StiffnessJointsKernel(data.NearbyJoints, data.NearbyJointsBuffer));
			AddPass(particleCollisionResetKernel = new ParticleCollisionResetKernel());
			AddPass(particlePlaneCollisionKernel = new ParticlePlaneCollisionKernel());
			AddPass(sphereCollisionKernel = new ParticleSphereCollisionKernel());
			AddPass(lineSphereCollisionKernel = new ParticleLineSphereCollisionKernel());
			if (data.PointJoints != null)
			{
				if (usesPreCalcVerts)
				{
					AddPass(pointJointsPreCalculatedKernel = new PointJointsPreCalculatedKernel());
					AddPass(pointJointsPreCalculatedFinalKernel = new PointJointsPreCalculatedFinalKernel());
				}
				else
				{
					AddPass(pointJointsKernel = new PointJointsKernel());
					AddPass(pointJointsFinalKernel = new PointJointsFinalKernel());
				}
			}
			AddPass(grabJointsKernel = new GrabJointsKernel());
			if (data.OutParticles != null)
			{
				AddPass(copySpecificParticlesKernel = new CopySpecificParticlesKernel());
			}
			if (usesPreCalcVerts)
			{
				AddPass(createVertexOnlyDataKernel = new CreateVertexOnlyDataKernel());
				AddPass(vector3CopyPaster = new GPUVector3CopyPaster(PreCalculatedPositions, OldPreCalculatedPositions));
			}
			else
			{
				AddPass(createVertexDataKernel = new CreateVertexDataKernel());
				AddPass(matrixCopyPaster = new GPUMatrixCopyPaster(Transforms, OldTransforms));
			}
		}

		public void FixedDispatch()
		{
			fixedDispatchCount++;
			DispatchPhysicsImpl();
		}

		public void DispatchCopyToOld()
		{
			if (usesPreCalcVerts)
			{
				vector3CopyPaster.Dispatch();
			}
			else
			{
				matrixCopyPaster.Dispatch();
			}
		}

		public override void Dispatch()
		{
			fixedDispatchCount = 0;
			DispatchImpl();
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

		private void DispatchPhysicsImpl()
		{
			InitData();
			UpdateBuffers();
			if (frame < 10)
			{
				if (usesPreCalcVerts)
				{
					vector3CopyPaster.Dispatch();
					resetToPointJointsPreCalculatedKernel.Dispatch();
				}
				else
				{
					matrixCopyPaster.Dispatch();
					resetKernel.Dispatch();
				}
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
				for (int j = 1; j <= iterations; j++)
				{
					T.Value = (float)j / (float)iterations;
					if (data.IntegrateEnabled && frame > 20)
					{
						integrateIterKernel.Dispatch();
					}
					if (pointJointsPreCalculatedKernel != null)
					{
						pointJointsPreCalculatedKernel.Dispatch();
					}
					else if (pointJointsKernel != null)
					{
						pointJointsKernel.Dispatch();
					}
					if (data.Stiffness > 0f)
					{
						stiffnessJointsKernel.Dispatch();
						nearbyJointsKernel.Dispatch();
					}
					integrateVelocityInnerKernel.Dispatch();
				}
				Step.Value = 1f / (float)outerIterations;
				DT.Value = num;
				DTRecip.Value = 1f / num;
				if (data.CollisionEnabled)
				{
					particleCollisionResetKernel.Dispatch();
					if (particleNeiborsCollisionKernel != null)
					{
						particleNeiborsCollisionKernel.Dispatch();
					}
					if (particlePlaneCollisionKernel.Planes != null)
					{
						particlePlaneCollisionKernel.Dispatch();
					}
					if (sphereCollisionKernel.ProcessedSpheres != null)
					{
						sphereCollisionKernel.Dispatch();
					}
					if (lineSphereCollisionKernel.ProcessedLineSpheres != null)
					{
						lineSphereCollisionKernel.Dispatch();
					}
					if (grabJointsKernel.GrabSpheres != null)
					{
						grabJointsKernel.Dispatch();
					}
				}
				integrateVelocityKernel.Dispatch();
			}
		}

		private void DispatchImpl()
		{
			InitData();
			UpdateBuffers();
			Step.Value = 1f;
			if (frame < 1)
			{
				if (usesPreCalcVerts)
				{
					vector3CopyPaster.Dispatch();
					resetToPointJointsPreCalculatedKernel.Dispatch();
				}
				else
				{
					matrixCopyPaster.Dispatch();
					resetKernel.Dispatch();
				}
			}
			if (pointJointsPreCalculatedFinalKernel != null)
			{
				pointJointsPreCalculatedFinalKernel.Dispatch();
			}
			else if (pointJointsFinalKernel != null)
			{
				pointJointsFinalKernel.Dispatch();
			}
			if (copySpecificParticlesKernel != null)
			{
				copySpecificParticlesKernel.Dispatch();
			}
			if (createVertexOnlyDataKernel != null)
			{
				createVertexOnlyDataKernel.Dispatch();
			}
			if (createVertexDataKernel != null)
			{
				createVertexDataKernel.Dispatch();
			}
			frame++;
		}

		public override void Dispose()
		{
			base.Dispose();
			if (usesPreCalcVerts)
			{
				OldPreCalculatedPositions.Dispose();
			}
			else
			{
				OldTransforms.Dispose();
			}
		}

		public void DebugDraw()
		{
			if (data.DebugDraw)
			{
				GPDebugDraw.Draw(stiffnessJointsKernel.StiffnessJointsBuffer, nearbyJointsKernel.StiffnessJointsBuffer, Particles, false, true, true);
			}
		}
	}
}
