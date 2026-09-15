using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Types.Dynamic;
using GPUTools.Physics.Scripts.Types.Joints;
using UnityEngine;

namespace GPUTools.Physics.Scripts.DebugDraw
{
	public class GPDebugDraw
	{
		public static void Draw(GpuBuffer<GPDistanceJoint> joints, GpuBuffer<GPDistanceJoint> joints2, GpuBuffer<GPParticle> particles, bool drawParticles, bool drawJoints, bool drawJoints2)
		{
			particles.PullData();
			Gizmos.color = Color.green;
			if (drawParticles)
			{
				GPParticle[] data = particles.Data;
				for (int i = 0; i < data.Length; i++)
				{
					GPParticle gPParticle = data[i];
					Gizmos.DrawWireSphere(gPParticle.Position, gPParticle.Radius);
				}
			}
			if (drawJoints && joints != null)
			{
				joints.PullData();
				GPDistanceJoint[] data2 = joints.Data;
				for (int j = 0; j < data2.Length; j++)
				{
					GPDistanceJoint gPDistanceJoint = data2[j];
					GPParticle gPParticle2 = particles.Data[gPDistanceJoint.Body1Id];
					GPParticle gPParticle3 = particles.Data[gPDistanceJoint.Body2Id];
					Gizmos.DrawLine(gPParticle2.Position, gPParticle3.Position);
				}
			}
			Gizmos.color = Color.yellow;
			if (drawJoints2 && joints2 != null)
			{
				joints2.PullData();
				GPDistanceJoint[] data3 = joints2.Data;
				for (int k = 0; k < data3.Length; k++)
				{
					GPDistanceJoint gPDistanceJoint2 = data3[k];
					GPParticle gPParticle4 = particles.Data[gPDistanceJoint2.Body1Id];
					GPParticle gPParticle5 = particles.Data[gPDistanceJoint2.Body2Id];
					Gizmos.DrawLine(gPParticle4.Position, gPParticle5.Position);
				}
			}
		}
	}
}
