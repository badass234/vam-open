using System;
using System.Collections.Generic;
using GPUTools.Hair.Scripts.Geometry.Constrains;
using GPUTools.Hair.Scripts.Settings.Abstract;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Settings
{
	[Serializable]
	public class HairPhysicsSettings : HairSettingsBase
	{
		public bool DebugDraw;

		public bool DebugDrawNearbyJoints;

		public bool IsEnabled = true;

		public int Iterations = 1;

		public bool FastMovement = true;

		public bool RunPhysicsOnUpdate;

		public Vector3 Gravity = new Vector3(0f, -1f, 0f);

		public float GravityMultiplier = 1f;

		public float Drag;

		public bool UseSeparateRootRadius;

		public float StandRootRadius = 0.01f;

		public float StandRadius = 0.01f;

		public float WorldScale = 1f;

		public bool StyleMode;

		public float StyleModeGravityMultiplier;

		public float StyleModeStrandRootRadius = 0.004f;

		public float StyleModeStrandRadius = 0.002f;

		public bool UsePaintedRigidity;

		public float PaintedRigidityMultiplier = 1f;

		public float RootRigidity = 0.1f;

		public float MainRigidity = 0.02f;

		public float TipRigidity;

		public float RigidityRolloffPower = 8f;

		public float JointRigidity = 0.1f;

		public AnimationCurve ElasticityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		public float SplineJointPower = 0.2f;

		public float ReverseSplineJointPower = 0.2f;

		public float CompressionJointPower = 0.2f;

		public float Weight = 1.5f;

		public float Friction = 0.8f;

		public float CollisionPower = 1f;

		public float NearbyJointPower = 1f;

		public float NearbyJointPowerRolloff;

		public bool UpdateRigidityJointsBeforeRender;

		public float WindMultiplier = 0.0001f;

		public bool IsCollisionEnabled = true;

		public List<GameObject> ColliderProviders = new List<GameObject>();

		public List<GameObject> AccessoriesProviders = new List<GameObject>();

		public List<HairJointArea> JointAreas = new List<HairJointArea>();

		private List<SphereCollider> colliders;

		public List<SphereCollider> GetColliders()
		{
			List<SphereCollider> list = new List<SphereCollider>();
			foreach (GameObject colliderProvider in ColliderProviders)
			{
				list.AddRange(colliderProvider.GetComponents<SphereCollider>().ToList());
			}
			return list;
		}

		public override bool Validate()
		{
			return true;
		}
	}
}
