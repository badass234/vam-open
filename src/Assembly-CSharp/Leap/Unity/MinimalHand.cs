using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity
{
	public class MinimalHand : HandModelBase
	{
		[SerializeField]
		private Chirality _handedness;

		[SerializeField]
		private Mesh _palmMesh;

		[SerializeField]
		private float _palmScale = 0.02f;

		[SerializeField]
		private Material _palmMat;

		[SerializeField]
		private Mesh _jointMesh;

		[SerializeField]
		private float _jointScale = 0.01f;

		[SerializeField]
		private Material _jointMat;

		private Hand _hand;

		private Transform _palm;

		private Transform[] _joints;

		public override Chirality Handedness
		{
			get
			{
				return _handedness;
			}
			set
			{
				_handedness = value;
			}
		}

		public override ModelType HandModelType => ModelType.Graphics;

		public override bool SupportsEditorPersistence()
		{
			return true;
		}

		public override void SetLeapHand(Hand hand)
		{
			_hand = hand;
		}

		public override Hand GetLeapHand()
		{
			return _hand;
		}

		public override void InitHand()
		{
			_joints = new Transform[20];
			for (int i = 0; i < 20; i++)
			{
				_joints[i] = createRenderer("Joint", _jointMesh, _jointScale, _jointMat);
			}
			_palm = createRenderer("Palm", _palmMesh, _palmScale, _palmMat);
		}

		public override void UpdateHand()
		{
			List<Finger> fingers = _hand.Fingers;
			int num = 0;
			for (int i = 0; i < 5; i++)
			{
				Finger finger = fingers[i];
				for (int j = 0; j < 4; j++)
				{
					_joints[num++].position = finger.Bone((Bone.BoneType)j).NextJoint.ToVector3();
				}
			}
			_palm.position = _hand.PalmPosition.ToVector3();
		}

		private Transform createRenderer(string name, Mesh mesh, float scale, Material mat)
		{
			GameObject gameObject = new GameObject(name);
			gameObject.AddComponent<MeshFilter>().mesh = mesh;
			gameObject.AddComponent<MeshRenderer>().sharedMaterial = mat;
			gameObject.transform.parent = base.transform;
			gameObject.transform.localScale = Vector3.one * scale;
			return gameObject.transform;
		}
	}
}
