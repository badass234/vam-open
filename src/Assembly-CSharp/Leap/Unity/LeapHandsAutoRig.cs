using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Leap.Unity
{
	[AddComponentMenu("Leap/Auto Rig Hands")]
	public class LeapHandsAutoRig : MonoBehaviour
	{
		private HandModelManager HandPoolToPopulate;

		public Animator AnimatorForMapping;

		public string ModelGroupName;

		[Tooltip("Set to True if each finger has an extra trasform between palm and base of the finger.")]
		public bool UseMetaCarpals;

		[Header("RiggedHand Components")]
		public RiggedHand RiggedHand_L;

		public RiggedHand RiggedHand_R;

		[Header("HandTransitionBehavior Components")]
		public HandTransitionBehavior HandTransitionBehavior_L;

		public HandTransitionBehavior HandTransitionBehavior_R;

		[Tooltip("Test")]
		[Header("RiggedFinger Components")]
		public RiggedFinger RiggedFinger_L_Thumb;

		public RiggedFinger RiggedFinger_L_Index;

		public RiggedFinger RiggedFinger_L_Mid;

		public RiggedFinger RiggedFinger_L_Ring;

		public RiggedFinger RiggedFinger_L_Pinky;

		public RiggedFinger RiggedFinger_R_Thumb;

		public RiggedFinger RiggedFinger_R_Index;

		public RiggedFinger RiggedFinger_R_Mid;

		public RiggedFinger RiggedFinger_R_Ring;

		public RiggedFinger RiggedFinger_R_Pinky;

		[Header("Palm & Finger Direction Vectors.")]
		public Vector3 modelFingerPointing_L = new Vector3(0f, 0f, 0f);

		public Vector3 modelPalmFacing_L = new Vector3(0f, 0f, 0f);

		public Vector3 modelFingerPointing_R = new Vector3(0f, 0f, 0f);

		public Vector3 modelPalmFacing_R = new Vector3(0f, 0f, 0f);

		[Tooltip("Toggling this value will reverse the ModelPalmFacing vectors to both RiggedHand's and all RiggedFingers.  Change if hands appear backward when tracking.")]
		[SerializeField]
		public bool FlipPalms;

		[SerializeField]
		[HideInInspector]
		private bool flippedPalmsState;

		[ContextMenu("AutoRig")]
		public void AutoRig()
		{
			HandPoolToPopulate = Object.FindObjectOfType<HandModelManager>();
			AnimatorForMapping = base.gameObject.GetComponent<Animator>();
			if (AnimatorForMapping != null)
			{
				if (AnimatorForMapping.isHuman)
				{
					AutoRigMecanim();
					RiggedHand_L.StoreJointsStartPose();
					RiggedHand_R.StoreJointsStartPose();
					return;
				}
				Debug.LogWarning("The Mecanim Avatar for this asset does not contain a valid IsHuman definition.  Attempting to auto map by name.");
			}
			AutoRigByName();
		}

		[ContextMenu("StoreStartPose")]
		public void StoreStartPose()
		{
			if ((bool)RiggedHand_L && (bool)RiggedHand_R)
			{
				RiggedHand_L.StoreJointsStartPose();
				RiggedHand_R.StoreJointsStartPose();
			}
			else
			{
				Debug.LogWarning("Please AutoRig before attempting to Store Start Pose");
			}
		}

		[ContextMenu("AutoRigByName")]
		private void AutoRigByName()
		{
			List<string> list = new List<string>();
			list.Add("left");
			List<string> source = list;
			list = new List<string>();
			list.Add("right");
			List<string> source2 = list;
			HandPoolToPopulate = Object.FindObjectOfType<HandModelManager>();
			Reset();
			Transform transform = null;
			foreach (Transform t in base.transform)
			{
				if (source.Any((string w) => t.name.ToLower().Contains(w)))
				{
					transform = t;
				}
			}
			if (transform != null)
			{
				RiggedHand_L = transform.gameObject.AddComponent<RiggedHand>();
				HandTransitionBehavior_L = transform.gameObject.AddComponent<HandEnableDisable>();
				RiggedHand_L.Handedness = Chirality.Left;
				RiggedHand_L.SetEditorLeapPose = false;
				RiggedHand_L.UseMetaCarpals = UseMetaCarpals;
				RiggedHand_L.SetupRiggedHand();
				RiggedFinger_L_Thumb = (RiggedFinger)RiggedHand_L.fingers[0];
				RiggedFinger_L_Index = (RiggedFinger)RiggedHand_L.fingers[1];
				RiggedFinger_L_Mid = (RiggedFinger)RiggedHand_L.fingers[2];
				RiggedFinger_L_Ring = (RiggedFinger)RiggedHand_L.fingers[3];
				RiggedFinger_L_Pinky = (RiggedFinger)RiggedHand_L.fingers[4];
				modelFingerPointing_L = RiggedHand_L.modelFingerPointing;
				modelPalmFacing_L = RiggedHand_L.modelPalmFacing;
				RiggedHand_L.StoreJointsStartPose();
			}
			Transform transform2 = null;
			foreach (Transform t2 in base.transform)
			{
				if (source2.Any((string w) => t2.name.ToLower().Contains(w)))
				{
					transform2 = t2;
				}
			}
			if (transform2 != null)
			{
				RiggedHand_R = transform2.gameObject.AddComponent<RiggedHand>();
				HandTransitionBehavior_R = transform2.gameObject.AddComponent<HandEnableDisable>();
				RiggedHand_R.Handedness = Chirality.Right;
				RiggedHand_R.SetEditorLeapPose = false;
				RiggedHand_R.UseMetaCarpals = UseMetaCarpals;
				RiggedHand_R.SetupRiggedHand();
				RiggedFinger_R_Thumb = (RiggedFinger)RiggedHand_R.fingers[0];
				RiggedFinger_R_Index = (RiggedFinger)RiggedHand_R.fingers[1];
				RiggedFinger_R_Mid = (RiggedFinger)RiggedHand_R.fingers[2];
				RiggedFinger_R_Ring = (RiggedFinger)RiggedHand_R.fingers[3];
				RiggedFinger_R_Pinky = (RiggedFinger)RiggedHand_R.fingers[4];
				modelFingerPointing_R = RiggedHand_R.modelFingerPointing;
				modelPalmFacing_R = RiggedHand_R.modelPalmFacing;
				RiggedHand_R.StoreJointsStartPose();
			}
			if (ModelGroupName == string.Empty || ModelGroupName != null)
			{
				ModelGroupName = base.transform.name;
			}
			HandPoolToPopulate.AddNewGroup(ModelGroupName, RiggedHand_L, RiggedHand_R);
		}

		[ContextMenu("AutoRigMecanim")]
		private void AutoRigMecanim()
		{
			AnimatorForMapping = base.gameObject.GetComponent<Animator>();
			HandPoolToPopulate = Object.FindObjectOfType<HandModelManager>();
			Reset();
			Transform boneTransform = AnimatorForMapping.GetBoneTransform(HumanBodyBones.LeftHand);
			if ((bool)boneTransform.GetComponent<RiggedHand>())
			{
				RiggedHand_L = boneTransform.GetComponent<RiggedHand>();
			}
			else
			{
				RiggedHand_L = boneTransform.gameObject.AddComponent<RiggedHand>();
			}
			HandTransitionBehavior_L = boneTransform.gameObject.AddComponent<HandDrop>();
			RiggedHand_L.Handedness = Chirality.Left;
			RiggedHand_L.SetEditorLeapPose = false;
			Transform boneTransform2 = AnimatorForMapping.GetBoneTransform(HumanBodyBones.RightHand);
			if ((bool)boneTransform2.GetComponent<RiggedHand>())
			{
				RiggedHand_R = boneTransform2.GetComponent<RiggedHand>();
			}
			else
			{
				RiggedHand_R = boneTransform2.gameObject.AddComponent<RiggedHand>();
			}
			HandTransitionBehavior_R = boneTransform2.gameObject.AddComponent<HandDrop>();
			RiggedHand_R.Handedness = Chirality.Right;
			RiggedHand_R.SetEditorLeapPose = false;
			RiggedHand_L.palm = AnimatorForMapping.GetBoneTransform(HumanBodyBones.LeftHand);
			RiggedHand_R.palm = AnimatorForMapping.GetBoneTransform(HumanBodyBones.RightHand);
			RiggedHand_R.UseMetaCarpals = UseMetaCarpals;
			RiggedHand_L.UseMetaCarpals = UseMetaCarpals;
			findAndAssignRiggedFingers(UseMetaCarpals);
			RiggedHand_L.AutoRigRiggedHand(RiggedHand_L.palm, RiggedFinger_L_Pinky.transform, RiggedFinger_L_Index.transform);
			RiggedHand_R.AutoRigRiggedHand(RiggedHand_R.palm, RiggedFinger_R_Pinky.transform, RiggedFinger_R_Index.transform);
			if (ModelGroupName == string.Empty || ModelGroupName != null)
			{
				ModelGroupName = base.transform.name;
			}
			HandPoolToPopulate.AddNewGroup(ModelGroupName, RiggedHand_L, RiggedHand_R);
			modelFingerPointing_L = RiggedHand_L.modelFingerPointing;
			modelPalmFacing_L = RiggedHand_L.modelPalmFacing;
			modelFingerPointing_R = RiggedHand_R.modelFingerPointing;
			modelPalmFacing_R = RiggedHand_R.modelPalmFacing;
		}

		private void findAndAssignRiggedFingers(bool useMetaCarpals)
		{
			if (!useMetaCarpals)
			{
				RiggedFinger_L_Thumb = AnimatorForMapping.GetBoneTransform(HumanBodyBones.LeftThumbProximal).gameObject.AddComponent<RiggedFinger>();
				RiggedFinger_L_Index = AnimatorForMapping.GetBoneTransform(HumanBodyBones.LeftIndexProximal).gameObject.AddComponent<RiggedFinger>();
				RiggedFinger_L_Mid = AnimatorForMapping.GetBoneTransform(HumanBodyBones.LeftMiddleProximal).gameObject.AddComponent<RiggedFinger>();
				RiggedFinger_L_Ring = AnimatorForMapping.GetBoneTransform(HumanBodyBones.LeftRingProximal).gameObject.AddComponent<RiggedFinger>();
				RiggedFinger_L_Pinky = AnimatorForMapping.GetBoneTransform(HumanBodyBones.LeftLittleProximal).gameObject.AddComponent<RiggedFinger>();
				RiggedFinger_R_Thumb = AnimatorForMapping.GetBoneTransform(HumanBodyBones.RightThumbProximal).gameObject.AddComponent<RiggedFinger>();
				RiggedFinger_R_Index = AnimatorForMapping.GetBoneTransform(HumanBodyBones.RightIndexProximal).gameObject.AddComponent<RiggedFinger>();
				RiggedFinger_R_Mid = AnimatorForMapping.GetBoneTransform(HumanBodyBones.RightMiddleProximal).gameObject.AddComponent<RiggedFinger>();
				RiggedFinger_R_Ring = AnimatorForMapping.GetBoneTransform(HumanBodyBones.RightRingProximal).gameObject.AddComponent<RiggedFinger>();
				RiggedFinger_R_Pinky = AnimatorForMapping.GetBoneTransform(HumanBodyBones.RightLittleProximal).gameObject.AddComponent<RiggedFinger>();
			}
			else
			{
				RiggedFinger_L_Thumb = AnimatorForMapping.GetBoneTransform(HumanBodyBones.LeftThumbProximal).gameObject.AddComponent<RiggedFinger>();
				RiggedFinger_L_Index = AnimatorForMapping.GetBoneTransform(HumanBodyBones.LeftIndexProximal).gameObject.transform.parent.gameObject.AddComponent<RiggedFinger>();
				RiggedFinger_L_Mid = AnimatorForMapping.GetBoneTransform(HumanBodyBones.LeftMiddleProximal).gameObject.transform.parent.gameObject.AddComponent<RiggedFinger>();
				RiggedFinger_L_Ring = AnimatorForMapping.GetBoneTransform(HumanBodyBones.LeftRingProximal).gameObject.transform.parent.gameObject.AddComponent<RiggedFinger>();
				RiggedFinger_L_Pinky = AnimatorForMapping.GetBoneTransform(HumanBodyBones.LeftLittleProximal).gameObject.transform.parent.gameObject.AddComponent<RiggedFinger>();
				RiggedFinger_R_Thumb = AnimatorForMapping.GetBoneTransform(HumanBodyBones.RightThumbProximal).gameObject.AddComponent<RiggedFinger>();
				RiggedFinger_R_Index = AnimatorForMapping.GetBoneTransform(HumanBodyBones.RightIndexProximal).gameObject.transform.parent.gameObject.AddComponent<RiggedFinger>();
				RiggedFinger_R_Mid = AnimatorForMapping.GetBoneTransform(HumanBodyBones.RightMiddleProximal).gameObject.transform.parent.gameObject.AddComponent<RiggedFinger>();
				RiggedFinger_R_Ring = AnimatorForMapping.GetBoneTransform(HumanBodyBones.RightRingProximal).gameObject.transform.parent.gameObject.AddComponent<RiggedFinger>();
				RiggedFinger_R_Pinky = AnimatorForMapping.GetBoneTransform(HumanBodyBones.RightLittleProximal).gameObject.transform.parent.gameObject.AddComponent<RiggedFinger>();
			}
			RiggedFinger_L_Thumb.fingerType = Finger.FingerType.TYPE_THUMB;
			RiggedFinger_L_Index.fingerType = Finger.FingerType.TYPE_INDEX;
			RiggedFinger_L_Mid.fingerType = Finger.FingerType.TYPE_MIDDLE;
			RiggedFinger_L_Ring.fingerType = Finger.FingerType.TYPE_RING;
			RiggedFinger_L_Pinky.fingerType = Finger.FingerType.TYPE_PINKY;
			RiggedFinger_R_Thumb.fingerType = Finger.FingerType.TYPE_THUMB;
			RiggedFinger_R_Index.fingerType = Finger.FingerType.TYPE_INDEX;
			RiggedFinger_R_Mid.fingerType = Finger.FingerType.TYPE_MIDDLE;
			RiggedFinger_R_Ring.fingerType = Finger.FingerType.TYPE_RING;
			RiggedFinger_R_Pinky.fingerType = Finger.FingerType.TYPE_PINKY;
		}

		private void Reset()
		{
			RiggedFinger[] componentsInChildren = GetComponentsInChildren<RiggedFinger>();
			RiggedFinger[] array = componentsInChildren;
			foreach (RiggedFinger obj in array)
			{
				Object.DestroyImmediate(obj);
			}
			Object.DestroyImmediate(RiggedHand_L);
			Object.DestroyImmediate(RiggedHand_R);
			Object.DestroyImmediate(HandTransitionBehavior_L);
			Object.DestroyImmediate(HandTransitionBehavior_R);
			if (HandPoolToPopulate != null)
			{
				HandPoolToPopulate.RemoveGroup(ModelGroupName);
			}
		}

		public void PushVectorValues()
		{
			if ((bool)RiggedHand_L)
			{
				RiggedHand_L.modelFingerPointing = modelFingerPointing_L;
				RiggedHand_L.modelPalmFacing = modelPalmFacing_L;
			}
			if ((bool)RiggedHand_R)
			{
				RiggedHand_R.modelFingerPointing = modelFingerPointing_R;
				RiggedHand_R.modelPalmFacing = modelPalmFacing_R;
			}
			if ((bool)RiggedFinger_L_Thumb)
			{
				RiggedFinger_L_Thumb.modelFingerPointing = modelFingerPointing_L;
				RiggedFinger_L_Thumb.modelPalmFacing = modelPalmFacing_L;
			}
			if ((bool)RiggedFinger_L_Index)
			{
				RiggedFinger_L_Index.modelFingerPointing = modelFingerPointing_L;
				RiggedFinger_L_Index.modelPalmFacing = modelPalmFacing_L;
			}
			if ((bool)RiggedFinger_L_Mid)
			{
				RiggedFinger_L_Mid.modelFingerPointing = modelFingerPointing_L;
				RiggedFinger_L_Mid.modelPalmFacing = modelPalmFacing_L;
			}
			if ((bool)RiggedFinger_L_Ring)
			{
				RiggedFinger_L_Ring.modelFingerPointing = modelFingerPointing_L;
				RiggedFinger_L_Ring.modelPalmFacing = modelPalmFacing_L;
			}
			if ((bool)RiggedFinger_L_Pinky)
			{
				RiggedFinger_L_Pinky.modelFingerPointing = modelFingerPointing_L;
				RiggedFinger_L_Pinky.modelPalmFacing = modelPalmFacing_L;
			}
			if ((bool)RiggedFinger_R_Thumb)
			{
				RiggedFinger_R_Thumb.modelFingerPointing = modelFingerPointing_R;
				RiggedFinger_R_Thumb.modelPalmFacing = modelPalmFacing_R;
			}
			if ((bool)RiggedFinger_R_Index)
			{
				RiggedFinger_R_Index.modelFingerPointing = modelFingerPointing_R;
				RiggedFinger_R_Index.modelPalmFacing = modelPalmFacing_R;
			}
			if ((bool)RiggedFinger_R_Mid)
			{
				RiggedFinger_R_Mid.modelFingerPointing = modelFingerPointing_R;
				RiggedFinger_R_Mid.modelPalmFacing = modelPalmFacing_R;
			}
			if ((bool)RiggedFinger_R_Ring)
			{
				RiggedFinger_R_Ring.modelFingerPointing = modelFingerPointing_R;
				RiggedFinger_R_Ring.modelPalmFacing = modelPalmFacing_R;
			}
			if ((bool)RiggedFinger_R_Pinky)
			{
				RiggedFinger_R_Pinky.modelFingerPointing = modelFingerPointing_R;
				RiggedFinger_R_Pinky.modelPalmFacing = modelPalmFacing_R;
			}
		}

		private void OnValidate()
		{
			if (FlipPalms != flippedPalmsState)
			{
				modelPalmFacing_L *= -1f;
				modelPalmFacing_R *= -1f;
				flippedPalmsState = FlipPalms;
				PushVectorValues();
			}
		}

		private void OnDestroy()
		{
			if (HandPoolToPopulate != null)
			{
				HandPoolToPopulate.RemoveGroup(ModelGroupName);
			}
		}
	}
}
