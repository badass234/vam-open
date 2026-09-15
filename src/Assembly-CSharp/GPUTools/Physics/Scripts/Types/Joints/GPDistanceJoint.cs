using GPUTools.Common.Scripts.PL.Tools;

namespace GPUTools.Physics.Scripts.Types.Joints
{
	public struct GPDistanceJoint : IGroupItem
	{
		public int Body1Id;

		public int Body2Id;

		public float Distance;

		public float Elasticity;

		public GPDistanceJoint(int body1Id, int body2Id, float distance, float elasticity)
		{
			Body1Id = body1Id;
			Body2Id = body2Id;
			Distance = distance;
			Elasticity = elasticity;
		}

		public static int Size()
		{
			return 16;
		}

		public bool HasConflict(IGroupItem item)
		{
			GPDistanceJoint gPDistanceJoint = (GPDistanceJoint)(object)item;
			return gPDistanceJoint.Body1Id == Body1Id || gPDistanceJoint.Body2Id == Body1Id || gPDistanceJoint.Body1Id == Body2Id || gPDistanceJoint.Body2Id == Body2Id;
		}
	}
}
