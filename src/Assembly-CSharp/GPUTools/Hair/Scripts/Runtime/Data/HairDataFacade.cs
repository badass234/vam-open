using System.Collections.Generic;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Hair.Scripts.Geometry.Constrains;
using GPUTools.Hair.Scripts.Runtime.Render;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using GPUTools.Physics.Scripts.Types.Shapes;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Runtime.Data
{
	public class HairDataFacade
	{
		private readonly HairSettings settings;

		public bool DebugDraw => settings.PhysicsSettings.DebugDraw;

		public bool DebugDrawNearbyJoints => settings.PhysicsSettings.DebugDrawNearbyJoints;

		public int Iterations => settings.PhysicsSettings.Iterations;

		public float Drag => settings.PhysicsSettings.Drag;

		public float InvDrag
		{
			get
			{
				float num = settings.PhysicsSettings.Drag * (1f / (float)Iterations);
				return 1f - num;
			}
		}

		public float WorldScale => settings.PhysicsSettings.WorldScale;

		public bool FastMovement => settings.PhysicsSettings.FastMovement;

		public Vector3 Gravity
		{
			get
			{
				if (StyleMode)
				{
					return UnityEngine.Physics.gravity * settings.PhysicsSettings.StyleModeGravityMultiplier;
				}
				return UnityEngine.Physics.gravity * settings.PhysicsSettings.GravityMultiplier;
			}
		}

		public Vector3 Wind => settings.RuntimeData.Wind;

		public bool IsPhysicsEnabled => settings.PhysicsSettings.IsEnabled;

		public bool IsCollisionEnabled => settings.PhysicsSettings.IsCollisionEnabled;

		public float Weight => settings.PhysicsSettings.Weight;

		public float Friction => settings.PhysicsSettings.Friction;

		public float CollisionPower => settings.PhysicsSettings.CollisionPower;

		public float CompressionJointPower => settings.PhysicsSettings.CompressionJointPower;

		public float NearbyJointPower => settings.PhysicsSettings.NearbyJointPower;

		public float NearbyJointPowerRolloff => settings.PhysicsSettings.NearbyJointPowerRolloff;

		public float SplineJointPower => settings.PhysicsSettings.SplineJointPower;

		public float ReverseSplineJointPower => settings.PhysicsSettings.ReverseSplineJointPower;

		public bool RunPhysicsOnUpdate => settings.PhysicsSettings.RunPhysicsOnUpdate;

		public bool UsePaintedRigidity => settings.PhysicsSettings.UsePaintedRigidity;

		public bool StyleMode => settings.PhysicsSettings.StyleMode;

		public GpuBuffer<Matrix4x4> MatricesBuffer => settings.StandsSettings.Provider.GetTransformsBuffer();

		public GpuBuffer<Vector3> NormalsBuffer => settings.StandsSettings.Provider.GetNormalsBuffer();

		public GpuBuffer<GPParticle> Particles => settings.RuntimeData.Particles;

		public GroupedData<GPDistanceJoint> DistanceJoints => settings.RuntimeData.DistanceJoints;

		public GpuBuffer<GPDistanceJoint> DistanceJointsBuffer => settings.RuntimeData.DistanceJointsBuffer;

		public GroupedData<GPDistanceJoint> CompressionJoints => settings.RuntimeData.CompressionJoints;

		public GpuBuffer<GPDistanceJoint> CompressionJointsBuffer => settings.RuntimeData.CompressionJointsBuffer;

		public GroupedData<GPDistanceJoint> NearbyDistanceJoints => settings.RuntimeData.NearbyDistanceJoints;

		public GpuBuffer<GPDistanceJoint> NearbyDistanceJointsBuffer => settings.RuntimeData.NearbyDistanceJointsBuffer;

		public GpuBuffer<GPPointJoint> PointJoints => settings.RuntimeData.PointJoints;

		public GpuBuffer<float> PointToPreviousPointDistances => settings.RuntimeData.PointToPreviousPointDistances;

		public List<HairJointArea> JointAreas => settings.PhysicsSettings.JointAreas;

		public Vector4 Size
		{
			get
			{
				int standsNum = settings.StandsSettings.Provider.GetStandsNum();
				int segmentsNum = settings.StandsSettings.Provider.GetSegmentsNum();
				return new Vector4(standsNum, segmentsNum);
			}
		}

		public GpuBuffer<GPSphereWithDelta> ProcessedSpheres => settings.RuntimeData.ProcessedSpheres;

		public GpuBuffer<GPLineSphereWithDelta> ProcessedLineSpheres => settings.RuntimeData.ProcessedLineSpheres;

		public GpuBuffer<Vector4> Planes => settings.RuntimeData.Planes;

		public GpuBuffer<GPLineSphere> CutLineSpheres => settings.RuntimeData.CutLineSpheres;

		public GpuBuffer<GPLineSphere> GrowLineSpheres => settings.RuntimeData.GrowLineSpheres;

		public GpuBuffer<GPLineSphere> HoldLineSpheres => settings.RuntimeData.HoldLineSpheres;

		public GpuBuffer<GPLineSphereWithMatrixDelta> GrabLineSpheres => settings.RuntimeData.GrabLineSpheres;

		public GpuBuffer<GPLineSphere> PushLineSpheres => settings.RuntimeData.PushLineSpheres;

		public GpuBuffer<GPLineSphere> PullLineSpheres => settings.RuntimeData.PullLineSpheres;

		public GpuBuffer<GPLineSphereWithDelta> BrushLineSpheres => settings.RuntimeData.BrushLineSpheres;

		public GpuBuffer<GPLineSphere> RigidityIncreaseLineSpheres => settings.RuntimeData.RigidityIncreaseLineSpheres;

		public GpuBuffer<GPLineSphere> RigidityDecreaseLineSpheres => settings.RuntimeData.RigidityDecreaseLineSpheres;

		public GpuBuffer<GPLineSphere> RigiditySetLineSpheres => settings.RuntimeData.RigiditySetLineSpheres;

		public GpuBuffer<TessRenderParticle> TessRenderParticles => settings.RuntimeData.TessRenderParticles;

		public GpuBuffer<GPParticle> OutParticles => settings.RuntimeData.OutParticles;

		public GpuBuffer<float> OutParticlesMap => settings.RuntimeData.OutParticlesMap;

		public GpuBuffer<RenderParticle> RenderParticles => settings.RuntimeData.RenderParticles;

		public GpuBuffer<Vector3> RandomsPerStrand => settings.RuntimeData.RandomsPerStrand;

		public Vector3 WavinessAxis => settings.RenderSettings.WavinessAxis;

		public Vector3 WorldWavinessAxis => settings.transform.TransformVector(WavinessAxis);

		public float WavinessFrequencyRandomness => settings.RenderSettings.WavinessFrequencyRandomness;

		public float WavinessScaleRandomness => settings.RenderSettings.WavinessScaleRandomness;

		public bool WavinessAllowReverse => settings.RenderSettings.WavinessAllowReverse;

		public bool WavinessAllowFlipAxis => settings.RenderSettings.WavinessAllowFlipAxis;

		public float WavinessNormalAdjust => settings.RenderSettings.WavinessNormalAdjust;

		public bool StyleModeShowCurls => settings.RenderSettings.StyleModeShowCurls;

		public Vector3 LightCenter => settings.StandsSettings.HeadCenterWorld;

		public float StandWidth => settings.LODSettings.GetWidth(LightCenter);

		public Vector3 TessFactor
		{
			get
			{
				int detail = settings.LODSettings.GetDetail(LightCenter);
				int dencity = settings.LODSettings.GetDencity(LightCenter);
				return new Vector4(detail, dencity, 0.99f / (float)detail, 0.99f / (float)dencity);
			}
		}

		public bool IsPhysicsEnabledLOD => settings.LODSettings.IsPhysicsEnabled(LightCenter);

		public bool CastShadows => settings.ShadowSettings.CastShadows;

		public bool ReseiveShadows => settings.ShadowSettings.ReseiveShadows;

		public float SpecularShift => settings.RenderSettings.SpecularShift;

		public float PrimarySpecular => settings.RenderSettings.PrimarySpecular;

		public float SecondarySpecular => settings.RenderSettings.SecondarySpecular;

		public Color SpecularColor => settings.RenderSettings.SpecularColor;

		public float Diffuse => settings.RenderSettings.Diffuse;

		public float FresnelPower => settings.RenderSettings.FresnelPower;

		public float FresnelAttenuation => settings.RenderSettings.FresnelAttenuation;

		public float NormalRandomize => settings.RenderSettings.NormalRandomize;

		public Vector3 Length => new Vector4(settings.RenderSettings.Length1, settings.RenderSettings.Length2, settings.RenderSettings.Length3);

		public Material material => settings.RenderSettings.material;

		public GpuBuffer<RenderParticle> ParticlesData => settings.RuntimeData.RenderParticles;

		public GpuBuffer<Vector3> Barycentrics => settings.RuntimeData.Barycentrics;

		public GpuBuffer<Vector3> BarycentricsFixed => settings.RuntimeData.BarycentricsFixed;

		public int[] Indices => settings.StandsSettings.Provider.GetIndices();

		public Bounds Bounds => settings.StandsSettings.Provider.GetBounds();

		public float Volume => settings.RenderSettings.Volume;

		public float RandomTexColorPower => settings.RenderSettings.RandomTexColorPower;

		public float RandomTexColorOffset => settings.RenderSettings.RandomTexColorOffset;

		public float IBLFactor => settings.RenderSettings.IBLFactor;

		public float MaxSpread => settings.RenderSettings.MaxSpread;

		public bool UpdateRigidityJointsBeforeRender => settings.PhysicsSettings.UpdateRigidityJointsBeforeRender;

		public HairDataFacade(HairSettings settings)
		{
			this.settings = settings;
		}
	}
}
