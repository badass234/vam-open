using GPUTools.Cloth.Scripts.Types;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using GPUTools.Physics.Scripts.Types.Shapes;
using GPUTools.Skinner.Scripts.Providers;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Runtime.Data
{
	public class ClothDataFacade
	{
		private readonly ClothSettings settings;

		public MeshProvider MeshProvider => settings.MeshProvider;

		public bool DebugDraw => settings.PhysicsDebugDraw;

		public int Iterations => settings.Iterations;

		public int InnerIterations => settings.InnerIterations;

		public float Drag => settings.Drag;

		public float InvDrag
		{
			get
			{
				float num = settings.Drag * (1f / (float)Iterations);
				return 1f - num;
			}
		}

		public bool IntegrateEnabled => settings.IntegrateEnabled;

		public Vector3 Gravity => UnityEngine.Physics.gravity * settings.GravityMultiplier;

		public float GravityMultiplier => settings.GravityMultiplier;

		public Vector3 Wind => settings.Runtime.Wind;

		public float Stretchability => settings.Stretchability;

		public float Stiffness => settings.Stiffness;

		public float CompressionResistance => settings.CompressionResistance;

		public float WorldScale => settings.WorldScale;

		public float DistanceScale => settings.DistanceScale;

		public bool BreakEnabled => settings.BreakEnabled;

		public float BreakThreshold => settings.BreakThreshold;

		public float JointStrength => settings.JointStrength;

		public float Weight => settings.Weight;

		public float Friction => settings.Friction;

		public float StaticMultiplier => settings.StaticMultiplier;

		public bool CollisionEnabled => settings.CollisionEnabled;

		public float CollisionPower => settings.CollisionPower;

		public GpuBuffer<Matrix4x4> MatricesBuffer => settings.MeshProvider.ToWorldMatricesBuffer;

		public GpuBuffer<Vector3> PreCalculatedVerticesBuffer => settings.MeshProvider.PreCalculatedVerticesBuffer;

		public GpuBuffer<GPParticle> Particles => settings.Runtime.Particles;

		public GpuBuffer<GPSphereWithDelta> ProcessedSpheres => settings.Runtime.ProcessedSpheres;

		public GpuBuffer<GPLineSphereWithDelta> ProcessedLineSpheres => settings.Runtime.ProcessedLineSpheres;

		public GpuBuffer<Vector4> Planes => settings.Runtime.Planes;

		public GpuBuffer<GPGrabSphere> GrabSpheres => settings.Runtime.GrabSpheres;

		public GroupedData<GPDistanceJoint> DistanceJoints => settings.Runtime.DistanceJoints;

		public GpuBuffer<GPDistanceJoint> DistanceJointsBuffer => settings.Runtime.DistanceJointsBuffer;

		public GroupedData<GPDistanceJoint> StiffnessJoints => settings.Runtime.StiffnessJoints;

		public GpuBuffer<GPDistanceJoint> StiffnessJointsBuffer => settings.Runtime.StiffnessJointsBuffer;

		public GroupedData<GPDistanceJoint> NearbyJoints => settings.Runtime.NearbyJoints;

		public GpuBuffer<GPDistanceJoint> NearbyJointsBuffer => settings.Runtime.NearbyJointsBuffer;

		public GpuBuffer<GPPointJoint> PointJoints => settings.Runtime.PointJoints;

		public GpuBuffer<GPPointJoint> AllPointJoints => settings.Runtime.AllPointJoints;

		public GpuBuffer<ClothVertex> ClothVertices => settings.Runtime.ClothVertices;

		public GpuBuffer<Vector3> ClothOnlyVertices => settings.Runtime.ClothOnlyVertices;

		public GpuBuffer<int> MeshVertexToNeiborsMap => settings.Runtime.MeshVertexToNeiborsMap;

		public GpuBuffer<int> MeshVertexToNeiborsMapCounts => settings.Runtime.MeshVertexToNeiborsMapCounts;

		public GpuBuffer<int> MeshToPhysicsVerticesMap => settings.Runtime.MeshToPhysicsVerticesMap;

		public GpuBuffer<GPParticle> OutParticles => settings.Runtime.OutParticles;

		public GpuBuffer<float> OutParticlesMap => settings.Runtime.OutParticlesMap;

		public Material Material => settings.Material;

		public Material[] Materials => settings.Materials;

		public Bounds Bounds => settings.Bounds;

		public bool CustomBounds => settings.CustomBounds;

		public ClothDataFacade(ClothSettings settings)
		{
			this.settings = settings;
		}
	}
}
