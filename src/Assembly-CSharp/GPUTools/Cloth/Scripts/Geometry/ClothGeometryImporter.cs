using GPUTools.Cloth.Scripts.Geometry.Passes;

namespace GPUTools.Cloth.Scripts.Geometry
{
	public class ClothGeometryImporter
	{
		private readonly ClothSettings settings;

		protected CommonJobsPass commonJobsPass;

		protected PhysicsVerticesPass physicsVerticesPass;

		protected NeighborsPass neighborsPass;

		protected JointsPass jointsPass;

		protected StiffnessJointsPass stiffnessJointsPass;

		protected NearbyJointsPass nearbyJointsPass;

		protected bool cancelCacheThreaded;

		public ClothGeometryImporter(ClothSettings settings)
		{
			this.settings = settings;
			commonJobsPass = new CommonJobsPass(settings);
			physicsVerticesPass = new PhysicsVerticesPass(settings);
			neighborsPass = new NeighborsPass(settings);
			jointsPass = new JointsPass(settings);
			stiffnessJointsPass = new StiffnessJointsPass(settings);
			nearbyJointsPass = new NearbyJointsPass(settings);
		}

		public void Cache()
		{
			commonJobsPass.Cache();
			physicsVerticesPass.Cache();
			neighborsPass.Cache();
		}

		public void CancelCacheThreaded()
		{
			cancelCacheThreaded = true;
			jointsPass.CancelCache();
			stiffnessJointsPass.CancelCache();
			nearbyJointsPass.CancelCache();
		}

		public void PrepCacheThreaded()
		{
			cancelCacheThreaded = false;
			jointsPass.PrepCache();
			stiffnessJointsPass.PrepCache();
			nearbyJointsPass.PrepCache();
		}

		public void CacheThreaded()
		{
			if (!cancelCacheThreaded)
			{
				jointsPass.Cache();
			}
			if (!cancelCacheThreaded)
			{
				stiffnessJointsPass.Cache();
			}
			if (!cancelCacheThreaded)
			{
				nearbyJointsPass.Cache();
			}
			if (!cancelCacheThreaded && settings.GeometryData.Particles != null)
			{
				settings.GeometryData.IsProcessed = true;
			}
		}
	}
}
