using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace Battlehub.RTSaveLoad.PersistentObjects
{
	[Serializable]
	[ProtoContract(AsReferenceDefault = true, ImplicitFields = ImplicitFields.AllFields)]
	public class PersistentConfigurableJoint : PersistentJoint
	{
		public Vector3 secondaryAxis;

		public uint xMotion;

		public uint yMotion;

		public uint zMotion;

		public uint angularXMotion;

		public uint angularYMotion;

		public uint angularZMotion;

		public SoftJointLimitSpring linearLimitSpring;

		public SoftJointLimitSpring angularXLimitSpring;

		public SoftJointLimitSpring angularYZLimitSpring;

		public SoftJointLimit linearLimit;

		public SoftJointLimit lowAngularXLimit;

		public SoftJointLimit highAngularXLimit;

		public SoftJointLimit angularYLimit;

		public SoftJointLimit angularZLimit;

		public Vector3 targetPosition;

		public Vector3 targetVelocity;

		public JointDrive xDrive;

		public JointDrive yDrive;

		public JointDrive zDrive;

		public Quaternion targetRotation;

		public Vector3 targetAngularVelocity;

		public uint rotationDriveMode;

		public JointDrive angularXDrive;

		public JointDrive angularYZDrive;

		public JointDrive slerpDrive;

		public uint projectionMode;

		public float projectionDistance;

		public float projectionAngle;

		public bool configuredInWorldSpace;

		public bool swapBodies;

		public override object WriteTo(object obj, Dictionary<long, UnityEngine.Object> objects)
		{
			obj = base.WriteTo(obj, objects);
			if (obj == null)
			{
				return null;
			}
			ConfigurableJoint configurableJoint = (ConfigurableJoint)obj;
			configurableJoint.secondaryAxis = secondaryAxis;
			configurableJoint.xMotion = (ConfigurableJointMotion)xMotion;
			configurableJoint.yMotion = (ConfigurableJointMotion)yMotion;
			configurableJoint.zMotion = (ConfigurableJointMotion)zMotion;
			configurableJoint.angularXMotion = (ConfigurableJointMotion)angularXMotion;
			configurableJoint.angularYMotion = (ConfigurableJointMotion)angularYMotion;
			configurableJoint.angularZMotion = (ConfigurableJointMotion)angularZMotion;
			configurableJoint.linearLimitSpring = linearLimitSpring;
			configurableJoint.angularXLimitSpring = angularXLimitSpring;
			configurableJoint.angularYZLimitSpring = angularYZLimitSpring;
			configurableJoint.linearLimit = linearLimit;
			configurableJoint.lowAngularXLimit = lowAngularXLimit;
			configurableJoint.highAngularXLimit = highAngularXLimit;
			configurableJoint.angularYLimit = angularYLimit;
			configurableJoint.angularZLimit = angularZLimit;
			configurableJoint.targetPosition = targetPosition;
			configurableJoint.targetVelocity = targetVelocity;
			configurableJoint.xDrive = xDrive;
			configurableJoint.yDrive = yDrive;
			configurableJoint.zDrive = zDrive;
			configurableJoint.targetRotation = targetRotation;
			configurableJoint.targetAngularVelocity = targetAngularVelocity;
			configurableJoint.rotationDriveMode = (RotationDriveMode)rotationDriveMode;
			configurableJoint.angularXDrive = angularXDrive;
			configurableJoint.angularYZDrive = angularYZDrive;
			configurableJoint.slerpDrive = slerpDrive;
			configurableJoint.projectionMode = (JointProjectionMode)projectionMode;
			configurableJoint.projectionDistance = projectionDistance;
			configurableJoint.projectionAngle = projectionAngle;
			configurableJoint.configuredInWorldSpace = configuredInWorldSpace;
			configurableJoint.swapBodies = swapBodies;
			return configurableJoint;
		}

		public override void ReadFrom(object obj)
		{
			base.ReadFrom(obj);
			if (obj != null)
			{
				ConfigurableJoint configurableJoint = (ConfigurableJoint)obj;
				secondaryAxis = configurableJoint.secondaryAxis;
				xMotion = (uint)configurableJoint.xMotion;
				yMotion = (uint)configurableJoint.yMotion;
				zMotion = (uint)configurableJoint.zMotion;
				angularXMotion = (uint)configurableJoint.angularXMotion;
				angularYMotion = (uint)configurableJoint.angularYMotion;
				angularZMotion = (uint)configurableJoint.angularZMotion;
				linearLimitSpring = configurableJoint.linearLimitSpring;
				angularXLimitSpring = configurableJoint.angularXLimitSpring;
				angularYZLimitSpring = configurableJoint.angularYZLimitSpring;
				linearLimit = configurableJoint.linearLimit;
				lowAngularXLimit = configurableJoint.lowAngularXLimit;
				highAngularXLimit = configurableJoint.highAngularXLimit;
				angularYLimit = configurableJoint.angularYLimit;
				angularZLimit = configurableJoint.angularZLimit;
				targetPosition = configurableJoint.targetPosition;
				targetVelocity = configurableJoint.targetVelocity;
				xDrive = configurableJoint.xDrive;
				yDrive = configurableJoint.yDrive;
				zDrive = configurableJoint.zDrive;
				targetRotation = configurableJoint.targetRotation;
				targetAngularVelocity = configurableJoint.targetAngularVelocity;
				rotationDriveMode = (uint)configurableJoint.rotationDriveMode;
				angularXDrive = configurableJoint.angularXDrive;
				angularYZDrive = configurableJoint.angularYZDrive;
				slerpDrive = configurableJoint.slerpDrive;
				projectionMode = (uint)configurableJoint.projectionMode;
				projectionDistance = configurableJoint.projectionDistance;
				projectionAngle = configurableJoint.projectionAngle;
				configuredInWorldSpace = configurableJoint.configuredInWorldSpace;
				swapBodies = configurableJoint.swapBodies;
			}
		}

		public override void FindDependencies<T>(Dictionary<long, T> dependencies, Dictionary<long, T> objects, bool allowNulls)
		{
			base.FindDependencies(dependencies, objects, allowNulls);
		}
	}
}
