using System;
using System.IO;
using MVR.FileManagement;
using MeshVR;
using UnityEngine;

public class DAZSkinWrapStore : ScriptableObject, IBinaryStorable
{
	[Serializable]
	public struct SkinWrapVert
	{
		public int closestTriangle;

		public int Vertex1;

		public int Vertex2;

		public int Vertex3;

		public float surfaceNormalProjection;

		public float surfaceTangent1Projection;

		public float surfaceTangent2Projection;

		public float surfaceNormalWrapNormalDot;

		public float surfaceTangent1WrapNormalDot;

		public float surfaceTangent2WrapNormalDot;
	}

	[HideInInspector]
	public SkinWrapVert[] wrapVertices;

	public bool LoadFromBinaryReader(BinaryReader binReader)
	{
		try
		{
			string text = binReader.ReadString();
			if (text != "DAZSkinWrapStore")
			{
				SuperController.LogError("Binary file corrupted. Tried to read DAZSkinWrapStore in wrong section");
				return false;
			}
			string text2 = binReader.ReadString();
			if (text2 != "1.0")
			{
				SuperController.LogError("DAZSkinWrapStore schema " + text2 + " is not compatible with this version of software");
				return false;
			}
			int num = binReader.ReadInt32();
			wrapVertices = new SkinWrapVert[num];
			for (int i = 0; i < num; i++)
			{
				wrapVertices[i] = default(SkinWrapVert);
				wrapVertices[i].closestTriangle = binReader.ReadInt32();
				wrapVertices[i].Vertex1 = binReader.ReadInt32();
				wrapVertices[i].Vertex2 = binReader.ReadInt32();
				wrapVertices[i].Vertex3 = binReader.ReadInt32();
				wrapVertices[i].surfaceNormalProjection = binReader.ReadSingle();
				wrapVertices[i].surfaceTangent1Projection = binReader.ReadSingle();
				wrapVertices[i].surfaceTangent2Projection = binReader.ReadSingle();
				wrapVertices[i].surfaceNormalWrapNormalDot = binReader.ReadSingle();
				wrapVertices[i].surfaceTangent1WrapNormalDot = binReader.ReadSingle();
				wrapVertices[i].surfaceTangent2WrapNormalDot = binReader.ReadSingle();
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Error while loading DAZSkinWrapStore from binary reader " + ex);
			return false;
		}
		return true;
	}

	public bool LoadFromBinaryFile(string path)
	{
		bool result = false;
		try
		{
			using (FileEntryStream fileEntryStream = FileManager.OpenStream(path, true))
			{
				using (BinaryReader binReader = new BinaryReader(fileEntryStream.Stream))
				{
					result = LoadFromBinaryReader(binReader);
				}
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Error while loading DAZSkinWrapStore from binary file " + path + " " + ex);
		}
		return result;
	}

	public bool StoreToBinaryWriter(BinaryWriter binWriter)
	{
		try
		{
			binWriter.Write("DAZSkinWrapStore");
			binWriter.Write("1.0");
			binWriter.Write(wrapVertices.Length);
			for (int i = 0; i < wrapVertices.Length; i++)
			{
				binWriter.Write(wrapVertices[i].closestTriangle);
				binWriter.Write(wrapVertices[i].Vertex1);
				binWriter.Write(wrapVertices[i].Vertex2);
				binWriter.Write(wrapVertices[i].Vertex3);
				binWriter.Write(wrapVertices[i].surfaceNormalProjection);
				binWriter.Write(wrapVertices[i].surfaceTangent1Projection);
				binWriter.Write(wrapVertices[i].surfaceTangent2Projection);
				binWriter.Write(wrapVertices[i].surfaceNormalWrapNormalDot);
				binWriter.Write(wrapVertices[i].surfaceTangent1WrapNormalDot);
				binWriter.Write(wrapVertices[i].surfaceTangent2WrapNormalDot);
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Error while storing DAZSkinWrapStore to binary reader " + ex);
			return false;
		}
		return true;
	}

	public bool StoreToBinaryFile(string path)
	{
		bool result = false;
		try
		{
			FileManager.AssertNotCalledFromPlugin();
			using (FileStream output = FileManager.OpenStreamForCreate(path))
			{
				using (BinaryWriter binWriter = new BinaryWriter(output))
				{
					result = StoreToBinaryWriter(binWriter);
				}
			}
		}
		catch (Exception ex)
		{
			SuperController.LogError("Error while storing DAZSkinWrapStore to binary file " + path + " " + ex);
		}
		return result;
	}
}
