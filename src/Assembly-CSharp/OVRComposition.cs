using UnityEngine;

public abstract class OVRComposition
{
	protected bool usingLastAttachedNodePose;

	protected OVRPose lastAttachedNodePose = default(OVRPose);

	public abstract OVRManager.CompositionMethod CompositionMethod();

	public abstract void Update(Camera mainCamera);

	public abstract void Cleanup();

	public virtual void RecenterPose()
	{
	}

	internal OVRPose ComputeCameraWorldSpacePose(OVRPlugin.CameraExtrinsics extrinsics)
	{
		OVRPose oVRPose = default(OVRPose);
		OVRPose oVRPose2 = default(OVRPose);
		OVRPose oVRPose3 = extrinsics.RelativePose.ToOVRPose();
		oVRPose2 = oVRPose3;
		if (extrinsics.AttachedToNode != OVRPlugin.Node.None && OVRPlugin.GetNodePresent(extrinsics.AttachedToNode))
		{
			if (usingLastAttachedNodePose)
			{
				Debug.Log("The camera attached node get tracked");
				usingLastAttachedNodePose = false;
			}
			oVRPose2 = (lastAttachedNodePose = OVRPlugin.GetNodePose(extrinsics.AttachedToNode, OVRPlugin.Step.Render).ToOVRPose()) * oVRPose2;
		}
		else if (extrinsics.AttachedToNode != OVRPlugin.Node.None)
		{
			if (!usingLastAttachedNodePose)
			{
				Debug.LogWarning("The camera attached node could not be tracked, using the last pose");
				usingLastAttachedNodePose = true;
			}
			oVRPose2 = lastAttachedNodePose * oVRPose2;
		}
		return OVRExtensions.ToWorldSpacePose(oVRPose2);
	}
}
