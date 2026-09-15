using System;
using UnityEngine;

namespace Leap.Unity.Encoding
{
	[Serializable]
	public class VectorHand
	{
		public const int NUM_JOINT_POSITIONS = 25;

		public bool isLeft;

		public Vector3 palmPos;

		public Quaternion palmRot;

		[SerializeField]
		private Vector3[] _backingJointPositions;

		public Vector3[] jointPositions
		{
			get
			{
				if (_backingJointPositions == null)
				{
					_backingJointPositions = new Vector3[25];
				}
				return _backingJointPositions;
			}
		}

		public int numBytesRequired => 86;

		public VectorHand()
		{
		}

		public VectorHand(Hand hand)
			: this()
		{
			Encode(hand);
		}

		public void Encode(Hand fromHand)
		{
			isLeft = fromHand.IsLeft;
			palmPos = fromHand.PalmPosition.ToVector3();
			palmRot = fromHand.Rotation.ToQuaternion();
			int num = 0;
			for (int i = 0; i < 5; i++)
			{
				Vector3 vector = ToLocal(fromHand.Fingers[i].bones[0].PrevJoint.ToVector3(), palmPos, palmRot);
				jointPositions[num++] = vector;
				for (int j = 0; j < 4; j++)
				{
					Vector3 vector2 = ToLocal(fromHand.Fingers[i].bones[j].NextJoint.ToVector3(), palmPos, palmRot);
					jointPositions[num++] = vector2;
				}
			}
		}

		public void Decode(Hand intoHand)
		{
			int num = 0;
			Vector3 zero = Vector3.zero;
			Vector3 vector = Vector3.zero;
			Quaternion quaternion = Quaternion.identity;
			for (int i = 0; i < 5; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					num = i * 4 + j;
					zero = jointPositions[i * 5 + j];
					vector = jointPositions[i * 5 + j + 1];
					quaternion = ((!((vector - zero).normalized == Vector3.zero)) ? Quaternion.LookRotation((vector - zero).normalized, Vector3.Cross((vector - zero).normalized, (i != 0) ? Vector3.right : ((!isLeft) ? Vector3.up : (-Vector3.up)))) : Quaternion.identity);
					vector = ToWorld(vector, palmPos, palmRot);
					zero = ToWorld(zero, palmPos, palmRot);
					quaternion = palmRot * quaternion;
					intoHand.GetBone(num).Fill(zero.ToVector(), vector.ToVector(), ((vector + zero) / 2f).ToVector(), (palmRot * Vector3.forward).ToVector(), (zero - vector).magnitude, 0.01f, (Bone.BoneType)j, quaternion.ToLeapQuaternion());
				}
				intoHand.Fingers[i].Fill(-1L, (!isLeft) ? 1 : 0, i, Time.time, vector.ToVector(), (quaternion * Vector3.forward).ToVector(), 1f, 1f, true, (Finger.FingerType)i);
			}
			intoHand.Arm.Fill(ToWorld(new Vector3(0f, 0f, -0.3f), palmPos, palmRot).ToVector(), ToWorld(new Vector3(0f, 0f, -0.055f), palmPos, palmRot).ToVector(), ToWorld(new Vector3(0f, 0f, -0.125f), palmPos, palmRot).ToVector(), Vector.Zero, 0.3f, 0.05f, palmRot.ToLeapQuaternion());
			intoHand.Fill(-1L, (!isLeft) ? 1 : 0, 1f, 0.5f, 100f, 0.5f, 50f, 0.085f, isLeft, 1f, null, palmPos.ToVector(), palmPos.ToVector(), Vector3.zero.ToVector(), (palmRot * Vector3.down).ToVector(), palmRot.ToLeapQuaternion(), (palmRot * Vector3.forward).ToVector(), ToWorld(new Vector3(0f, 0f, -0.055f), palmPos, palmRot).ToVector());
		}

		public void ReadBytes(byte[] bytes, ref int offset)
		{
			if (bytes.Length - offset < numBytesRequired)
			{
				throw new IndexOutOfRangeException(string.Concat("Not enough room to read bytes for VectorHand encoding starting at offset ", offset, " for array of size ", bytes, "; need at least ", numBytesRequired, " bytes from the offset position."));
			}
			isLeft = bytes[offset++] == 0;
			for (int i = 0; i < 3; i++)
			{
				palmPos[i] = Convert.ToSingle(BitConverterNonAlloc.ToInt16(bytes, ref offset)) / 4096f;
			}
			palmRot = Utils.DecompressBytesToQuat(bytes, ref offset);
			for (int j = 0; j < 25; j++)
			{
				for (int k = 0; k < 3; k++)
				{
					jointPositions[j][k] = VectorHandExtensions.ByteToFloat(bytes[offset++]);
				}
			}
		}

		public void FillBytes(byte[] bytesToFill, ref int offset)
		{
			if (_backingJointPositions == null)
			{
				throw new InvalidOperationException("Joint positions array is null. You must fill a VectorHand with data before you can use it to fill byte representations.");
			}
			if (bytesToFill.Length - offset < numBytesRequired)
			{
				throw new IndexOutOfRangeException("Not enough room to fill bytes for VectorHand encoding starting at offset " + offset + " for array of size " + bytesToFill.Length + "; need at least " + numBytesRequired + " bytes from the offset position.");
			}
			bytesToFill[offset++] = ((!isLeft) ? ((byte)1) : ((byte)0));
			for (int i = 0; i < 3; i++)
			{
				BitConverterNonAlloc.GetBytes(Convert.ToInt16(palmPos[i] * 4096f), bytesToFill, ref offset);
			}
			Utils.CompressQuatToBytes(palmRot, bytesToFill, ref offset);
			for (int j = 0; j < 25; j++)
			{
				for (int k = 0; k < 3; k++)
				{
					bytesToFill[offset++] = VectorHandExtensions.FloatToByte(jointPositions[j][k]);
				}
			}
		}

		public void FillBytes(byte[] bytesToFill)
		{
			int offset = 0;
			FillBytes(bytesToFill, ref offset);
		}

		public void ReadBytes(byte[] bytes, ref int offset, Hand intoHand)
		{
			ReadBytes(bytes, ref offset);
			Decode(intoHand);
		}

		public void FillBytes(byte[] bytes, ref int offset, Hand fromHand)
		{
			Encode(fromHand);
			FillBytes(bytes, ref offset);
		}

		public static Vector3 ToWorld(Vector3 localPoint, Vector3 localOrigin, Quaternion localRot)
		{
			return localRot * localPoint + localOrigin;
		}

		public static Vector3 ToLocal(Vector3 worldPoint, Vector3 localOrigin, Quaternion localRot)
		{
			return Quaternion.Inverse(localRot) * (worldPoint - localOrigin);
		}
	}
}
