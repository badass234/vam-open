using System;
using System.Collections.Generic;
using Leap.Unity.Attributes;
using UnityEngine;

namespace Leap.Unity
{
	public class CapsuleHand : HandModelBase
	{
		private const int TOTAL_JOINT_COUNT = 20;

		private const float CYLINDER_MESH_RESOLUTION = 0.1f;

		private const int THUMB_BASE_INDEX = 0;

		private const int PINKY_BASE_INDEX = 16;

		private static int _leftColorIndex = 0;

		private static int _rightColorIndex = 0;

		private static Color[] _leftColorList = new Color[3]
		{
			new Color(0f, 0f, 1f),
			new Color(0.2f, 0f, 0.4f),
			new Color(0f, 0.2f, 0.2f)
		};

		private static Color[] _rightColorList = new Color[3]
		{
			new Color(1f, 0f, 0f),
			new Color(1f, 1f, 0f),
			new Color(1f, 0.5f, 0f)
		};

		[SerializeField]
		private Chirality handedness;

		[SerializeField]
		private bool _showArm = true;

		[SerializeField]
		private bool _castShadows = true;

		[SerializeField]
		private Material _material;

		[SerializeField]
		private Mesh _sphereMesh;

		[MinValue(3f)]
		[SerializeField]
		private int _cylinderResolution = 12;

		[MinValue(0f)]
		[SerializeField]
		private float _jointRadius = 0.008f;

		[MinValue(0f)]
		[SerializeField]
		private float _cylinderRadius = 0.006f;

		[MinValue(0f)]
		[SerializeField]
		private float _palmRadius = 0.015f;

		private Material _sphereMat;

		private Hand _hand;

		private Vector3[] _spherePositions;

		private Dictionary<int, Mesh> _meshMap = new Dictionary<int, Mesh>();

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

		public override bool SupportsEditorPersistence()
		{
			return true;
		}

		public override Hand GetLeapHand()
		{
			return _hand;
		}

		public override void SetLeapHand(Hand hand)
		{
			_hand = hand;
		}

		public override void InitHand()
		{
			if (_material != null)
			{
				_sphereMat = new Material(_material);
				_sphereMat.hideFlags = HideFlags.DontSaveInEditor;
			}
		}

		private void OnValidate()
		{
			_meshMap.Clear();
		}

		public override void BeginHand()
		{
			base.BeginHand();
			if (_hand.IsLeft)
			{
				_sphereMat.color = _leftColorList[_leftColorIndex];
				_leftColorIndex = (_leftColorIndex + 1) % _leftColorList.Length;
			}
			else
			{
				_sphereMat.color = _rightColorList[_rightColorIndex];
				_rightColorIndex = (_rightColorIndex + 1) % _rightColorList.Length;
			}
		}

		public override void UpdateHand()
		{
			if (_spherePositions == null || _spherePositions.Length != 20)
			{
				_spherePositions = new Vector3[20];
			}
			if (_sphereMat == null)
			{
				_sphereMat = new Material(_material);
				_sphereMat.hideFlags = HideFlags.DontSaveInEditor;
			}
			foreach (Finger finger in _hand.Fingers)
			{
				for (int i = 0; i < 4; i++)
				{
					int fingerJointIndex = getFingerJointIndex((int)finger.Type, i);
					Vector3 vector = finger.Bone((Bone.BoneType)i).NextJoint.ToVector3();
					_spherePositions[fingerJointIndex] = vector;
					drawSphere(vector);
				}
			}
			Vector3 position = _hand.PalmPosition.ToVector3();
			drawSphere(position, _palmRadius);
			Vector3 inDirection = _spherePositions[0] - _hand.PalmPosition.ToVector3();
			Vector3 vector2 = _hand.PalmPosition.ToVector3() + Vector3.Reflect(inDirection, _hand.Basis.xBasis.ToVector3());
			drawSphere(vector2);
			if (_showArm)
			{
				Arm arm = _hand.Arm;
				Vector3 vector3 = arm.Basis.xBasis.ToVector3() * arm.Width * 0.7f * 0.5f;
				Vector3 vector4 = arm.WristPosition.ToVector3();
				Vector3 vector5 = arm.ElbowPosition.ToVector3();
				float num = Vector3.Distance(vector4, vector5);
				vector4 -= arm.Direction.ToVector3() * num * 0.05f;
				Vector3 vector6 = vector4 + vector3;
				Vector3 vector7 = vector4 - vector3;
				Vector3 vector8 = vector5 + vector3;
				Vector3 vector9 = vector5 - vector3;
				drawSphere(vector6);
				drawSphere(vector7);
				drawSphere(vector9);
				drawSphere(vector8);
				drawCylinder(vector7, vector6);
				drawCylinder(vector9, vector8);
				drawCylinder(vector7, vector9);
				drawCylinder(vector6, vector8);
			}
			for (int j = 0; j < 5; j++)
			{
				for (int k = 0; k < 3; k++)
				{
					int fingerJointIndex2 = getFingerJointIndex(j, k);
					int fingerJointIndex3 = getFingerJointIndex(j, k + 1);
					Vector3 a = _spherePositions[fingerJointIndex2];
					Vector3 b = _spherePositions[fingerJointIndex3];
					drawCylinder(a, b);
				}
			}
			for (int l = 0; l < 4; l++)
			{
				int fingerJointIndex4 = getFingerJointIndex(l, 0);
				int fingerJointIndex5 = getFingerJointIndex(l + 1, 0);
				Vector3 a2 = _spherePositions[fingerJointIndex4];
				Vector3 b2 = _spherePositions[fingerJointIndex5];
				drawCylinder(a2, b2);
			}
			drawCylinder(vector2, 0);
			drawCylinder(vector2, 16);
		}

		private void drawSphere(Vector3 position)
		{
			drawSphere(position, _jointRadius);
		}

		private void drawSphere(Vector3 position, float radius)
		{
			Graphics.DrawMesh(_sphereMesh, Matrix4x4.TRS(position, Quaternion.identity, Vector3.one * radius * 2f * base.transform.lossyScale.x), _sphereMat, 0, null, 0, null, _castShadows);
		}

		private void drawCylinder(Vector3 a, Vector3 b)
		{
			float magnitude = (a - b).magnitude;
			Graphics.DrawMesh(getCylinderMesh(magnitude), Matrix4x4.TRS(a, Quaternion.LookRotation(b - a), new Vector3(base.transform.lossyScale.x, base.transform.lossyScale.x, 1f)), _material, base.gameObject.layer, null, 0, null, _castShadows);
		}

		private void drawCylinder(int a, int b)
		{
			drawCylinder(_spherePositions[a], _spherePositions[b]);
		}

		private void drawCylinder(Vector3 a, int b)
		{
			drawCylinder(a, _spherePositions[b]);
		}

		private int getFingerJointIndex(int fingerIndex, int jointIndex)
		{
			return fingerIndex * 4 + jointIndex;
		}

		private Mesh getCylinderMesh(float length)
		{
			int key = Mathf.RoundToInt(length * 100f / 0.1f);
			Mesh value;
			if (_meshMap.TryGetValue(key, out value))
			{
				return value;
			}
			value = new Mesh();
			value.name = "GeneratedCylinder";
			value.hideFlags = HideFlags.DontSave;
			List<Vector3> list = new List<Vector3>();
			List<Color> list2 = new List<Color>();
			List<int> list3 = new List<int>();
			Vector3 zero = Vector3.zero;
			Vector3 vector = Vector3.forward * length;
			for (int i = 0; i < _cylinderResolution; i++)
			{
				float f = (float)Math.PI * 2f * (float)i / (float)_cylinderResolution;
				float x = _cylinderRadius * Mathf.Cos(f);
				float y = _cylinderRadius * Mathf.Sin(f);
				Vector3 vector2 = new Vector3(x, y, 0f);
				list.Add(zero + vector2);
				list.Add(vector + vector2);
				list2.Add(Color.white);
				list2.Add(Color.white);
				int count = list.Count;
				int num = _cylinderResolution * 2;
				list3.Add(count % num);
				list3.Add((count + 2) % num);
				list3.Add((count + 1) % num);
				list3.Add((count + 2) % num);
				list3.Add((count + 3) % num);
				list3.Add((count + 1) % num);
			}
			value.SetVertices(list);
			value.SetIndices(list3.ToArray(), MeshTopology.Triangles, 0);
			value.RecalculateBounds();
			value.RecalculateNormals();
			value.UploadMeshData(true);
			_meshMap[key] = value;
			return value;
		}
	}
}
