using GPUTools.Cloth.Scripts.Runtime.Data;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Runtime.Physics
{
	public class ClothPhysics : MonoBehaviour
	{
		private ClothPhysicsWorld world;

		public void Initialize(ClothDataFacade data)
		{
			world = new ClothPhysicsWorld(data);
		}

		public void ResetPhysics()
		{
			world.Reset();
		}

		public void PartialResetPhysics()
		{
			world.PartialReset();
		}

		public void FixedDispatch()
		{
			world.FixedDispatch();
		}

		public void DispatchCopyToOld()
		{
			world.DispatchCopyToOld();
		}

		public void Dispatch()
		{
			world.Dispatch();
		}

		private void OnDestroy()
		{
			world.Dispose();
		}

		private void OnDrawGizmos()
		{
			world.DebugDraw();
		}
	}
}
