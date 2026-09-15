using GPUTools.Cloth.Scripts.Geometry.Passes;
using GPUTools.Cloth.Scripts.Runtime.Commands;
using GPUTools.Cloth.Scripts.Runtime.Data;
using GPUTools.Cloth.Scripts.Runtime.Physics;
using GPUTools.Cloth.Scripts.Runtime.Render;
using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Skinner.Scripts.Providers;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Runtime
{
	public class BuildRuntimeCloth : BuildChainCommand
	{
		private readonly ClothSettings settings;

		private GameObject obj;

		private ClothRender render;

		public ClothPhysics physics { get; private set; }

		public BuildParticles particles { get; private set; }

		public BuildSpheres spheres { get; private set; }

		public BuildLineSpheres lineSpheres { get; private set; }

		public BuildPointJoints pointJoints { get; private set; }

		public BuildDistanceJoints distanceJoints { get; private set; }

		public BuildStiffnessJoints stiffnessJoints { get; private set; }

		public BuildNearbyJoints nearbyJoints { get; private set; }

		public BuildPhysicsBlend physicsBlend { get; private set; }

		public BuildGrabSpheres grabSpheres { get; private set; }

		public BuildRuntimeCloth(ClothSettings settings)
		{
			this.settings = settings;
			physicsBlend = new BuildPhysicsBlend(settings);
			Add(physicsBlend);
			Add(new BuildPhysicsStrength(settings));
			particles = new BuildParticles(settings);
			Add(particles);
			Add(new BuildPlanes(settings));
			spheres = new BuildSpheres(settings);
			Add(spheres);
			grabSpheres = new BuildGrabSpheres(settings);
			Add(grabSpheres);
			lineSpheres = new BuildLineSpheres(settings);
			Add(lineSpheres);
			distanceJoints = new BuildDistanceJoints(settings);
			Add(distanceJoints);
			stiffnessJoints = new BuildStiffnessJoints(settings);
			Add(stiffnessJoints);
			nearbyJoints = new BuildNearbyJoints(settings);
			Add(nearbyJoints);
			pointJoints = new BuildPointJoints(settings);
			Add(pointJoints);
			Add(new BuildVertexData(settings));
			Add(new BuildClothAccessories(settings));
		}

		protected override void OnBuild()
		{
			obj = settings.gameObject;
			obj.layer = settings.gameObject.layer;
			obj.transform.SetParent(settings.transform.parent, false);
			ClothDataFacade data = new ClothDataFacade(settings);
			physics = obj.AddComponent<ClothPhysics>();
			physics.Initialize(data);
			if (settings.MeshProvider.Type != ScalpMeshType.PreCalc)
			{
				render = obj.AddComponent<ClothRender>();
				render.Initialize(data);
			}
		}

		protected override void OnUpdateSettings()
		{
			physics.ResetPhysics();
		}

		public void DispatchCopyToOld()
		{
			physics.DispatchCopyToOld();
		}

		protected override void OnDispatch()
		{
			physics.Dispatch();
		}

		protected override void OnFixedDispatch()
		{
			physics.FixedDispatch();
		}
	}
}
