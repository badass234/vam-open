using GPUTools.Cloth.Scripts.Types;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using GPUTools.Physics.Scripts.Types.Shapes;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Runtime.Data
{
	public class RuntimeData
	{
		public GpuBuffer<GPParticle> Particles { get; set; }

		public GpuBuffer<Vector4> Planes { get; set; }

		public GpuBuffer<GPSphereWithDelta> ProcessedSpheres { get; set; }

		public GpuBuffer<GPLineSphereWithDelta> ProcessedLineSpheres { get; set; }

		public GpuBuffer<GPPointJoint> AllPointJoints { get; set; }

		public GpuBuffer<GPPointJoint> PointJoints { get; set; }

		public GroupedData<GPDistanceJoint> DistanceJoints { get; set; }

		public GpuBuffer<GPDistanceJoint> DistanceJointsBuffer { get; set; }

		public GroupedData<GPDistanceJoint> StiffnessJoints { get; set; }

		public GpuBuffer<GPDistanceJoint> StiffnessJointsBuffer { get; set; }

		public GroupedData<GPDistanceJoint> NearbyJoints { get; set; }

		public GpuBuffer<GPDistanceJoint> NearbyJointsBuffer { get; set; }

		public GpuBuffer<GPGrabSphere> GrabSpheres { get; set; }

		public GpuBuffer<ClothVertex> ClothVertices { get; set; }

		public GpuBuffer<Vector3> ClothOnlyVertices { get; set; }

		public GpuBuffer<int> MeshToPhysicsVerticesMap { get; set; }

		public GpuBuffer<int> MeshVertexToNeiborsMap { get; set; }

		public GpuBuffer<int> MeshVertexToNeiborsMapCounts { get; set; }

		public GpuBuffer<GPParticle> OutParticles { get; set; }

		public GpuBuffer<float> OutParticlesMap { get; set; }

		public Vector3 Wind { get; set; }
	}
}
