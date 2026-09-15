using UnityEngine;

namespace Leap.Unity
{
	public class DebugHand : HandModelBase
	{
		private Hand hand_;

		[SerializeField]
		private bool visualizeBasis = true;

		protected Color[] colors = new Color[4]
		{
			Color.gray,
			Color.yellow,
			Color.cyan,
			Color.magenta
		};

		[SerializeField]
		private Chirality handedness;

		public bool VisualizeBasis
		{
			get
			{
				return visualizeBasis;
			}
			set
			{
				visualizeBasis = value;
			}
		}

		public override ModelType HandModelType => ModelType.Graphics;

		public override Chirality Handedness
		{
			get
			{
				return handedness;
			}
			set
			{
			}
		}

		public override Hand GetLeapHand()
		{
			return hand_;
		}

		public override void SetLeapHand(Hand hand)
		{
			hand_ = hand;
		}

		public override bool SupportsEditorPersistence()
		{
			return true;
		}

		public override void InitHand()
		{
			DrawDebugLines();
		}

		public override void UpdateHand()
		{
			DrawDebugLines();
		}

		protected void DrawDebugLines()
		{
			Hand leapHand = GetLeapHand();
			Debug.DrawLine(leapHand.Arm.ElbowPosition.ToVector3(), leapHand.Arm.WristPosition.ToVector3(), Color.red);
			Debug.DrawLine(leapHand.WristPosition.ToVector3(), leapHand.PalmPosition.ToVector3(), Color.white);
			Debug.DrawLine(leapHand.PalmPosition.ToVector3(), (leapHand.PalmPosition + leapHand.PalmNormal * leapHand.PalmWidth / 2f).ToVector3(), Color.black);
			if (VisualizeBasis)
			{
				DrawBasis(leapHand.PalmPosition, leapHand.Basis, leapHand.PalmWidth / 4f);
				DrawBasis(leapHand.Arm.ElbowPosition, leapHand.Arm.Basis, 0.01f);
			}
			for (int i = 0; i < 5; i++)
			{
				Finger finger = leapHand.Fingers[i];
				for (int j = 0; j < 4; j++)
				{
					Bone bone = finger.Bone((Bone.BoneType)j);
					Debug.DrawLine(bone.PrevJoint.ToVector3(), bone.PrevJoint.ToVector3() + bone.Direction.ToVector3() * bone.Length, colors[j]);
					if (VisualizeBasis)
					{
						DrawBasis(bone.PrevJoint, bone.Basis, 0.01f);
					}
				}
			}
		}

		public void DrawBasis(Vector position, LeapTransform basis, float scale)
		{
			Vector3 vector = position.ToVector3();
			Debug.DrawLine(vector, vector + basis.xBasis.ToVector3() * scale, Color.red);
			Debug.DrawLine(vector, vector + basis.yBasis.ToVector3() * scale, Color.green);
			Debug.DrawLine(vector, vector + basis.zBasis.ToVector3() * scale, Color.blue);
		}
	}
}
