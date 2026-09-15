using GPUTools.Common.Scripts.Tools.Commands;
using GPUTools.Hair.Scripts.Runtime.Commands.Physics;
using GPUTools.Hair.Scripts.Runtime.Commands.Render;
using GPUTools.Hair.Scripts.Runtime.Data;
using GPUTools.Hair.Scripts.Runtime.Physics;
using GPUTools.Hair.Scripts.Runtime.Render;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Runtime.Commands
{
	public class BuildRuntimeHair : BuildChainCommand
	{
		private readonly HairSettings settings;

		private GameObject obj;

		public GPHairPhysics physics { get; private set; }

		public HairRender render { get; private set; }

		public BuildParticles particles { get; private set; }

		public BuildPlanes planes { get; private set; }

		public BuildSpheres spheres { get; private set; }

		public BuildLineSpheres lineSpheres { get; private set; }

		public BuildEditLineSpheres editLineSpheres { get; private set; }

		public BuildPointJoints pointJoints { get; private set; }

		public BuildParticlesData particlesData { get; private set; }

		public BuildDistanceJoints distanceJoints { get; private set; }

		public BuildCompressionJoints compressionJoints { get; private set; }

		public BuildNearbyDistanceJoints nearbyDistanceJoints { get; private set; }

		public BuildTesselation tesselation { get; private set; }

		public BuildBarycentrics barycentrics { get; private set; }

		public bool IsVisible => render.IsVisible;

		public BuildRuntimeHair(HairSettings settings)
		{
			this.settings = settings;
			particles = new BuildParticles(settings);
			Add(particles);
			planes = new BuildPlanes(settings);
			Add(planes);
			spheres = new BuildSpheres(settings);
			Add(spheres);
			lineSpheres = new BuildLineSpheres(settings);
			Add(lineSpheres);
			editLineSpheres = new BuildEditLineSpheres(settings);
			Add(editLineSpheres);
			distanceJoints = new BuildDistanceJoints(settings);
			Add(distanceJoints);
			compressionJoints = new BuildCompressionJoints(settings);
			Add(compressionJoints);
			nearbyDistanceJoints = new BuildNearbyDistanceJoints(settings);
			Add(nearbyDistanceJoints);
			pointJoints = new BuildPointJoints(settings);
			Add(pointJoints);
			Add(new BuildAccessories(settings));
			barycentrics = new BuildBarycentrics(settings);
			Add(barycentrics);
			particlesData = new BuildParticlesData(settings);
			Add(particlesData);
			tesselation = new BuildTesselation(settings);
			Add(tesselation);
		}

		public void RebuildHair()
		{
			particles.Build();
			distanceJoints.Build();
			compressionJoints.Build();
			nearbyDistanceJoints.Build();
			pointJoints.Build();
			barycentrics.Build();
			particlesData.Build();
			tesselation.Build();
			render.InitializeMesh();
			physics.RebindData();
			physics.ResetPhysics();
		}

		protected override void OnBuild()
		{
			obj = new GameObject("Render");
			obj.layer = settings.gameObject.layer;
			obj.transform.SetParent(settings.transform, false);
			HairDataFacade data = new HairDataFacade(settings);
			physics = obj.AddComponent<GPHairPhysics>();
			physics.Initialize(data);
			render = obj.AddComponent<HairRender>();
			render.Initialize(data);
		}

		protected override void OnDispatch()
		{
			physics.Dispatch();
			render.Dispatch();
		}

		protected override void OnFixedDispatch()
		{
			physics.FixedDispatch();
		}

		protected override void OnDispose()
		{
			Object.Destroy(obj);
		}
	}
}
