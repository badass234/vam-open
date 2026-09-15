using System.Collections.Generic;
using System.Linq;
using Leap.Unity.Attributes;
using UnityEngine;

namespace Leap.Unity
{
	public class RiggedHand : HandModel
	{
		[Tooltip("When True, hands will be put into a Leap editor pose near the LeapServiceProvider's transform.  When False, the hands will be returned to their Start Pose if it has been saved.")]
		[SerializeField]
		private bool setEditorLeapPose = true;

		[SerializeField]
		public bool DeformPositionsInFingers;

		[Tooltip("When True, hands will be put into a Leap editor pose near the LeapServiceProvider's transform.  When False, the hands will be returned to their Start Pose if it has been saved.")]
		[SerializeField]
		[HideInInspector]
		private bool deformPositionsState;

		[Tooltip("Hands are typically rigged in 3D packages with the palm transform near the wrist. Uncheck this is your model's palm transform is at the center of the palm similar to Leap's API drives")]
		public bool ModelPalmAtLeapWrist = true;

		[Tooltip("Set to True if each finger has an extra trasform between palm and base of the finger.")]
		public bool UseMetaCarpals;

		[Tooltip("Because bones only exist at their roots in model rigs, the length of the last fingertip bone is lost when placing bones at positions in the tracked hand. This option scales the last bone along its X axis (length axis) to match its bone length to the tracked bone length. This option only has an effect if Deform Positions In Fingers is enabled.")]
		[DisableIf("DeformPositionsInFingers", false, null)]
		[SerializeField]
		[OnEditorChange("scaleLastFingerBones")]
		private bool _scaleLastFingerBones = true;

		public Vector3 modelFingerPointing = new Vector3(0f, 0f, 0f);

		public Vector3 modelPalmFacing = new Vector3(0f, 0f, 0f);

		[Header("Values for Stored Start Pose")]
		[SerializeField]
		private List<Transform> jointList = new List<Transform>();

		[SerializeField]
		private List<Quaternion> localRotations = new List<Quaternion>();

		[SerializeField]
		private List<Vector3> localPositions = new List<Vector3>();

		public override ModelType HandModelType => ModelType.Graphics;

		public bool SetEditorLeapPose
		{
			get
			{
				return setEditorLeapPose;
			}
			set
			{
				if (!value)
				{
					RestoreJointsStartPose();
				}
				setEditorLeapPose = value;
			}
		}

		public bool scaleLastFingerBones
		{
			get
			{
				return _scaleLastFingerBones;
			}
			set
			{
				_scaleLastFingerBones = value;
				setScaleLastFingerBoneInFingers(_scaleLastFingerBones);
			}
		}

		public override bool SupportsEditorPersistence()
		{
			return SetEditorLeapPose;
		}

		public override void InitHand()
		{
			UpdateHand();
			setDeformPositionsInFingers(deformPositionsState);
			setScaleLastFingerBoneInFingers(scaleLastFingerBones);
		}

		public Quaternion Reorientation()
		{
			if (modelFingerPointing == Vector3.zero || modelPalmFacing == Vector3.zero)
			{
				return Quaternion.identity;
			}
			return Quaternion.Inverse(Quaternion.LookRotation(modelFingerPointing, -modelPalmFacing));
		}

		public override void UpdateHand()
		{
			if (palm != null)
			{
				if (ModelPalmAtLeapWrist)
				{
					palm.position = GetWristPosition();
				}
				else
				{
					palm.position = GetPalmPosition();
					if ((bool)wristJoint)
					{
						wristJoint.position = GetWristPosition();
					}
				}
				palm.rotation = GetRiggedPalmRotation() * Reorientation();
			}
			if (forearm != null)
			{
				forearm.rotation = GetArmRotation() * Reorientation();
			}
			for (int i = 0; i < fingers.Length; i++)
			{
				if (fingers[i] != null)
				{
					fingers[i].fingerType = (Finger.FingerType)i;
					fingers[i].UpdateFinger();
				}
			}
		}

		public Quaternion GetRiggedPalmRotation()
		{
			if (hand_ != null)
			{
				LeapTransform basis = hand_.Basis;
				return CalculateRotation(basis);
			}
			if ((bool)palm)
			{
				return palm.rotation;
			}
			return Quaternion.identity;
		}

		private Quaternion CalculateRotation(LeapTransform trs)
		{
			Vector3 upwards = trs.yBasis.ToVector3();
			Vector3 forward = trs.zBasis.ToVector3();
			return Quaternion.LookRotation(forward, upwards);
		}

		[ContextMenu("Setup Rigged Hand")]
		public void SetupRiggedHand()
		{
			Debug.Log("Using transform naming to setup RiggedHand on " + base.transform.name);
			modelFingerPointing = new Vector3(0f, 0f, 0f);
			modelPalmFacing = new Vector3(0f, 0f, 0f);
			assignRiggedFingersByName();
			SetupRiggedFingers();
			modelPalmFacing = calculateModelPalmFacing(palm, fingers[2].transform, fingers[1].transform);
			modelFingerPointing = calculateModelFingerPointing();
			setFingerPalmFacing();
		}

		public void AutoRigRiggedHand(Transform palm, Transform finger1, Transform finger2)
		{
			Debug.Log("Using Mecanim mapping to setup RiggedHand on " + base.transform.name);
			modelFingerPointing = new Vector3(0f, 0f, 0f);
			modelPalmFacing = new Vector3(0f, 0f, 0f);
			SetupRiggedFingers();
			modelPalmFacing = calculateModelPalmFacing(palm, finger1, finger2);
			modelFingerPointing = calculateModelFingerPointing();
			setFingerPalmFacing();
		}

		private void assignRiggedFingersByName()
		{
			List<string> list = new List<string>();
			list.Add("palm");
			List<string> source = list;
			list = new List<string>();
			list.Add("thumb");
			list.Add("tmb");
			List<string> source2 = list;
			list = new List<string>();
			list.Add("index");
			list.Add("idx");
			List<string> source3 = list;
			list = new List<string>();
			list.Add("middle");
			list.Add("mid");
			List<string> source4 = list;
			list = new List<string>();
			list.Add("ring");
			List<string> source5 = list;
			list = new List<string>();
			list.Add("pinky");
			list.Add("pin");
			List<string> source6 = list;
			Transform transform = null;
			Transform transform2 = null;
			Transform transform3 = null;
			Transform transform4 = null;
			Transform transform5 = null;
			Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>();
			if (source.Any((string w) => base.transform.name.ToLower().Contains(w)))
			{
				palm = base.transform;
			}
			else
			{
				Transform[] array = componentsInChildren;
				foreach (Transform t in array)
				{
					if (source.Any((string w) => t.name.ToLower().Contains(w)))
					{
						palm = t;
					}
				}
			}
			if (!palm)
			{
				palm = base.transform;
			}
			if (!palm)
			{
				return;
			}
			Transform[] array2 = componentsInChildren;
			foreach (Transform transform6 in array2)
			{
				RiggedFinger component = transform6.GetComponent<RiggedFinger>();
				string lowercaseName = transform6.name.ToLower();
				if (!component)
				{
					if (source2.Any((string w) => lowercaseName.Contains(w)) && transform6.parent == palm)
					{
						transform = transform6;
						RiggedFinger riggedFinger = transform.gameObject.AddComponent<RiggedFinger>();
						riggedFinger.fingerType = Finger.FingerType.TYPE_THUMB;
					}
					if (source3.Any((string w) => lowercaseName.Contains(w)) && transform6.parent == palm)
					{
						transform2 = transform6;
						RiggedFinger riggedFinger2 = transform2.gameObject.AddComponent<RiggedFinger>();
						riggedFinger2.fingerType = Finger.FingerType.TYPE_INDEX;
					}
					if (source4.Any((string w) => lowercaseName.Contains(w)) && transform6.parent == palm)
					{
						transform3 = transform6;
						RiggedFinger riggedFinger3 = transform3.gameObject.AddComponent<RiggedFinger>();
						riggedFinger3.fingerType = Finger.FingerType.TYPE_MIDDLE;
					}
					if (source5.Any((string w) => lowercaseName.Contains(w)) && transform6.parent == palm)
					{
						transform4 = transform6;
						RiggedFinger riggedFinger4 = transform4.gameObject.AddComponent<RiggedFinger>();
						riggedFinger4.fingerType = Finger.FingerType.TYPE_RING;
					}
					if (source6.Any((string w) => lowercaseName.Contains(w)) && transform6.parent == palm)
					{
						transform5 = transform6;
						RiggedFinger riggedFinger5 = transform5.gameObject.AddComponent<RiggedFinger>();
						riggedFinger5.fingerType = Finger.FingerType.TYPE_PINKY;
					}
				}
			}
		}

		private void SetupRiggedFingers()
		{
			RiggedFinger[] componentsInChildren = GetComponentsInChildren<RiggedFinger>();
			for (int i = 0; i < 5; i++)
			{
				int num = componentsInChildren[i].fingerType.indexOf();
				fingers[num] = componentsInChildren[i];
				componentsInChildren[i].SetupRiggedFinger(UseMetaCarpals);
			}
		}

		private void setFingerPalmFacing()
		{
			RiggedFinger[] componentsInChildren = GetComponentsInChildren<RiggedFinger>();
			for (int i = 0; i < 5; i++)
			{
				int num = componentsInChildren[i].fingerType.indexOf();
				fingers[num] = componentsInChildren[i];
				componentsInChildren[i].modelPalmFacing = modelPalmFacing;
			}
		}

		private Vector3 calculateModelPalmFacing(Transform palm, Transform finger1, Transform finger2)
		{
			Vector3 vector = palm.transform.InverseTransformPoint(palm.position);
			Vector3 vector2 = palm.transform.InverseTransformPoint(finger1.position);
			Vector3 vector3 = palm.transform.InverseTransformPoint(finger2.position);
			Vector3 vector4 = vector2 - vector;
			Vector3 vector5 = vector3 - vector;
			Vector3 vectorToZero = ((Handedness != 0) ? Vector3.Cross(vector5, vector4) : Vector3.Cross(vector4, vector5));
			return CalculateZeroedVector(vectorToZero);
		}

		private Vector3 calculateModelFingerPointing()
		{
			Vector3 vectorToZero = palm.transform.InverseTransformPoint(fingers[2].transform.GetChild(0).transform.position) - palm.transform.InverseTransformPoint(palm.position);
			Vector3 vector = CalculateZeroedVector(vectorToZero);
			return vector * -1f;
		}

		public static Vector3 CalculateZeroedVector(Vector3 vectorToZero)
		{
			Vector3 result = default(Vector3);
			float num = Mathf.Max(Mathf.Abs(vectorToZero.x), Mathf.Abs(vectorToZero.y), Mathf.Abs(vectorToZero.z));
			if (Mathf.Abs(vectorToZero.x) == num)
			{
				result = ((!(vectorToZero.x < 0f)) ? new Vector3(-1f, 0f, 0f) : new Vector3(1f, 0f, 0f));
			}
			if (Mathf.Abs(vectorToZero.y) == num)
			{
				result = ((!(vectorToZero.y < 0f)) ? new Vector3(0f, -1f, 0f) : new Vector3(0f, 1f, 0f));
			}
			if (Mathf.Abs(vectorToZero.z) == num)
			{
				result = ((!(vectorToZero.z < 0f)) ? new Vector3(0f, 0f, -1f) : new Vector3(0f, 0f, 1f));
			}
			return result;
		}

		[ContextMenu("StoreJointsStartPose")]
		public void StoreJointsStartPose()
		{
			Transform[] componentsInChildren = palm.parent.GetComponentsInChildren<Transform>();
			foreach (Transform transform in componentsInChildren)
			{
				jointList.Add(transform);
				localRotations.Add(transform.localRotation);
				localPositions.Add(transform.localPosition);
			}
		}

		[ContextMenu("RestoreJointsStartPose")]
		public void RestoreJointsStartPose()
		{
			for (int i = 0; i < jointList.Count; i++)
			{
				Transform transform = jointList[i];
				transform.localRotation = localRotations[i];
				transform.localPosition = localPositions[i];
			}
		}

		private void setDeformPositionsInFingers(bool onOff)
		{
			RiggedFinger[] componentsInChildren = GetComponentsInChildren<RiggedFinger>();
			RiggedFinger[] array = componentsInChildren;
			foreach (RiggedFinger riggedFinger in array)
			{
				riggedFinger.deformPosition = onOff;
			}
		}

		private void setScaleLastFingerBoneInFingers(bool shouldScale)
		{
			RiggedFinger[] componentsInChildren = GetComponentsInChildren<RiggedFinger>();
			RiggedFinger[] array = componentsInChildren;
			foreach (RiggedFinger riggedFinger in array)
			{
				riggedFinger.scaleLastFingerBone = shouldScale;
			}
		}

		public void OnValidate()
		{
			if (DeformPositionsInFingers != deformPositionsState)
			{
				RestoreJointsStartPose();
				setDeformPositionsInFingers(DeformPositionsInFingers);
				deformPositionsState = DeformPositionsInFingers;
			}
			if (!setEditorLeapPose)
			{
				RestoreJointsStartPose();
			}
		}
	}
}
