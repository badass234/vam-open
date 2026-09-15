using UnityEngine;

namespace GPUTools.Skinner.Scripts.Utils
{
	public class MeshSkinUtils
	{
		public static Matrix4x4 CreateToWorldMatrix(SkinnedMeshRenderer skin)
		{
			Mesh sharedMesh = skin.sharedMesh;
			BoneWeight boneWeight = sharedMesh.boneWeights[0];
			Matrix4x4 matrix4x = skin.bones[boneWeight.boneIndex0].localToWorldMatrix * sharedMesh.bindposes[boneWeight.boneIndex0];
			Matrix4x4 matrix4x2 = skin.bones[boneWeight.boneIndex1].localToWorldMatrix * sharedMesh.bindposes[boneWeight.boneIndex1];
			Matrix4x4 matrix4x3 = skin.bones[boneWeight.boneIndex2].localToWorldMatrix * sharedMesh.bindposes[boneWeight.boneIndex2];
			Matrix4x4 matrix4x4 = skin.bones[boneWeight.boneIndex3].localToWorldMatrix * sharedMesh.bindposes[boneWeight.boneIndex3];
			Matrix4x4 result = default(Matrix4x4);
			for (int i = 0; i < 16; i++)
			{
				result[i] = matrix4x[i] * boneWeight.weight0 + matrix4x2[i] * boneWeight.weight1 + matrix4x3[i] * boneWeight.weight2 + matrix4x4[i] * boneWeight.weight3;
			}
			return result;
		}

		public static Matrix4x4[] CreateToWorldMatrices(SkinnedMeshRenderer skin)
		{
			Matrix4x4[] array = new Matrix4x4[skin.sharedMesh.vertexCount];
			CreateToWorldMatrices(skin, array);
			return array;
		}

		public static void CreateToWorldMatrices(SkinnedMeshRenderer skin, Matrix4x4[] matrices)
		{
			Mesh sharedMesh = skin.sharedMesh;
			Matrix4x4[] array = new Matrix4x4[skin.bones.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = skin.bones[i].localToWorldMatrix * sharedMesh.bindposes[i];
			}
			for (int j = 0; j < sharedMesh.vertexCount; j++)
			{
				BoneWeight boneWeight = sharedMesh.boneWeights[j];
				Matrix4x4 matrix4x = array[boneWeight.boneIndex0];
				Matrix4x4 matrix4x2 = array[boneWeight.boneIndex1];
				Matrix4x4 matrix4x3 = array[boneWeight.boneIndex2];
				Matrix4x4 matrix4x4 = array[boneWeight.boneIndex3];
				Matrix4x4 matrix4x5 = default(Matrix4x4);
				for (int k = 0; k < 16; k++)
				{
					matrix4x5[k] = matrix4x[k] * boneWeight.weight0 + matrix4x2[k] * boneWeight.weight1 + matrix4x3[k] * boneWeight.weight2 + matrix4x4[k] * boneWeight.weight3;
				}
				matrices[j] = matrix4x5;
			}
		}

		public static Matrix4x4[] CreateToObjectMatrices(SkinnedMeshRenderer skin)
		{
			Matrix4x4[] array = CreateToWorldMatrices(skin);
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].inverse;
			}
			return array;
		}
	}
}
