using UnityEngine;

namespace Leap.Unity
{
	public class RigidHand : SkeletalHand
	{
		public float filtering = 0.5f;

		public override ModelType HandModelType => ModelType.Physics;

		public override bool SupportsEditorPersistence()
		{
			return true;
		}

		public override void InitHand()
		{
			base.InitHand();
		}

		public override void UpdateHand()
		{
			for (int i = 0; i < fingers.Length; i++)
			{
				if (fingers[i] != null)
				{
					fingers[i].UpdateFinger();
				}
			}
			if (palm != null)
			{
				Rigidbody component = palm.GetComponent<Rigidbody>();
				if ((bool)component)
				{
					component.MovePosition(GetPalmCenter());
					component.MoveRotation(GetPalmRotation());
				}
				else
				{
					palm.position = GetPalmCenter();
					palm.rotation = GetPalmRotation();
				}
			}
			if (forearm != null)
			{
				CapsuleCollider component2 = forearm.GetComponent<CapsuleCollider>();
				if (component2 != null)
				{
					component2.direction = 2;
					forearm.localScale = new Vector3(1f / base.transform.lossyScale.x, 1f / base.transform.lossyScale.y, 1f / base.transform.lossyScale.z);
					component2.radius = GetArmWidth() / 2f;
					component2.height = GetArmLength() + GetArmWidth();
				}
				Rigidbody component3 = forearm.GetComponent<Rigidbody>();
				if ((bool)component3)
				{
					component3.MovePosition(GetArmCenter());
					component3.MoveRotation(GetArmRotation());
				}
				else
				{
					forearm.position = GetArmCenter();
					forearm.rotation = GetArmRotation();
				}
			}
		}
	}
}
