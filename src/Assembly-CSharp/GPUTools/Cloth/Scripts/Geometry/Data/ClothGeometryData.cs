using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GPUTools.Cloth.Scripts.Types;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Geometry.Data
{
	[Serializable]
	public class ClothGeometryData
	{
		[NonSerialized]
		public string status = string.Empty;

		public int[] AllTringles;

		public Vector3[] Particles;

		public int[] MeshToPhysicsVerticesMap;

		public int[] PhysicsToMeshVerticesMap;

		public List<Int2ListContainer> JointGroups = new List<Int2ListContainer>();

		public List<Int2ListContainer> StiffnessJointGroups = new List<Int2ListContainer>();

		public List<Int2ListContainer> NearbyJointGroups = new List<Int2ListContainer>();

		public int[] ParticleToNeibor;

		public int[] ParticleToNeiborCounts;

		public float[] ParticlesBlend;

		public float[] ParticlesStrength;

		public bool IsProcessed;

		public void ResetParticlesBlend()
		{
			for (int i = 0; i < ParticlesBlend.Length; i++)
			{
				ParticlesBlend[i] = 0f;
			}
		}

		public bool LoadFromBinaryReader(BinaryReader reader)
		{
			try
			{
				string text = reader.ReadString();
				if (text != "ClothGeometryData")
				{
					Debug.LogError("Binary file corrupted. Tried to read ClothGeometryData in wrong section");
					return false;
				}
				string text2 = reader.ReadString();
				if (text2 != "1.0")
				{
					Debug.LogError("ClothGeometryData schema " + text2 + " is not compatible with this version of software");
					return false;
				}
				int num = reader.ReadInt32();
				AllTringles = new int[num];
				for (int i = 0; i < num; i++)
				{
					AllTringles[i] = reader.ReadInt32();
				}
				int num2 = reader.ReadInt32();
				Particles = new Vector3[num2];
				Vector3 vector = default(Vector3);
				for (int j = 0; j < num2; j++)
				{
					vector.x = reader.ReadSingle();
					vector.y = reader.ReadSingle();
					vector.z = reader.ReadSingle();
					Particles[j] = vector;
				}
				int num3 = reader.ReadInt32();
				MeshToPhysicsVerticesMap = new int[num3];
				for (int k = 0; k < num3; k++)
				{
					MeshToPhysicsVerticesMap[k] = reader.ReadInt32();
				}
				int num4 = reader.ReadInt32();
				PhysicsToMeshVerticesMap = new int[num4];
				for (int l = 0; l < num4; l++)
				{
					PhysicsToMeshVerticesMap[l] = reader.ReadInt32();
				}
				int num5 = reader.ReadInt32();
				JointGroups = new List<Int2ListContainer>();
				for (int m = 0; m < num5; m++)
				{
					Int2ListContainer int2ListContainer = new Int2ListContainer();
					JointGroups.Add(int2ListContainer);
					int num6 = reader.ReadInt32();
					int2ListContainer.List = new List<Int2>();
					for (int n = 0; n < num6; n++)
					{
						Int2 item = default(Int2);
						item.X = reader.ReadInt32();
						item.Y = reader.ReadInt32();
						int2ListContainer.List.Add(item);
					}
				}
				int num7 = reader.ReadInt32();
				StiffnessJointGroups = new List<Int2ListContainer>();
				for (int num8 = 0; num8 < num7; num8++)
				{
					Int2ListContainer int2ListContainer2 = new Int2ListContainer();
					StiffnessJointGroups.Add(int2ListContainer2);
					int num9 = reader.ReadInt32();
					int2ListContainer2.List = new List<Int2>();
					for (int num10 = 0; num10 < num9; num10++)
					{
						Int2 item2 = default(Int2);
						item2.X = reader.ReadInt32();
						item2.Y = reader.ReadInt32();
						int2ListContainer2.List.Add(item2);
					}
				}
				int num11 = reader.ReadInt32();
				NearbyJointGroups = new List<Int2ListContainer>();
				for (int num12 = 0; num12 < num11; num12++)
				{
					Int2ListContainer int2ListContainer3 = new Int2ListContainer();
					NearbyJointGroups.Add(int2ListContainer3);
					int num13 = reader.ReadInt32();
					int2ListContainer3.List = new List<Int2>();
					for (int num14 = 0; num14 < num13; num14++)
					{
						Int2 item3 = default(Int2);
						item3.X = reader.ReadInt32();
						item3.Y = reader.ReadInt32();
						int2ListContainer3.List.Add(item3);
					}
				}
				int num15 = reader.ReadInt32();
				ParticleToNeibor = new int[num15];
				for (int num16 = 0; num16 < num15; num16++)
				{
					ParticleToNeibor[num16] = reader.ReadInt32();
				}
				int num17 = reader.ReadInt32();
				ParticleToNeiborCounts = new int[num17];
				for (int num18 = 0; num18 < num17; num18++)
				{
					ParticleToNeiborCounts[num18] = reader.ReadInt32();
				}
				int num19 = reader.ReadInt32();
				ParticlesBlend = new float[num19];
				for (int num20 = 0; num20 < num19; num20++)
				{
					ParticlesBlend[num20] = reader.ReadSingle();
				}
				int num21 = reader.ReadInt32();
				ParticlesStrength = new float[num21];
				for (int num22 = 0; num22 < num21; num22++)
				{
					ParticlesStrength[num22] = reader.ReadSingle();
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Error while loading ClothGeometryData from binary reader " + ex);
				return false;
			}
			return true;
		}

		public bool StoreToBinaryWriter(BinaryWriter writer)
		{
			try
			{
				writer.Write("ClothGeometryData");
				writer.Write("1.0");
				writer.Write(AllTringles.Length);
				for (int i = 0; i < AllTringles.Length; i++)
				{
					writer.Write(AllTringles[i]);
				}
				writer.Write(Particles.Length);
				for (int j = 0; j < Particles.Length; j++)
				{
					writer.Write(Particles[j].x);
					writer.Write(Particles[j].y);
					writer.Write(Particles[j].z);
				}
				writer.Write(MeshToPhysicsVerticesMap.Length);
				for (int k = 0; k < MeshToPhysicsVerticesMap.Length; k++)
				{
					writer.Write(MeshToPhysicsVerticesMap[k]);
				}
				writer.Write(PhysicsToMeshVerticesMap.Length);
				for (int l = 0; l < PhysicsToMeshVerticesMap.Length; l++)
				{
					writer.Write(PhysicsToMeshVerticesMap[l]);
				}
				writer.Write(JointGroups.Count);
				for (int m = 0; m < JointGroups.Count; m++)
				{
					Int2ListContainer int2ListContainer = JointGroups[m];
					writer.Write(int2ListContainer.List.Count);
					for (int n = 0; n < int2ListContainer.List.Count; n++)
					{
						writer.Write(int2ListContainer.List[n].X);
						writer.Write(int2ListContainer.List[n].Y);
					}
				}
				writer.Write(StiffnessJointGroups.Count);
				for (int num = 0; num < StiffnessJointGroups.Count; num++)
				{
					Int2ListContainer int2ListContainer2 = StiffnessJointGroups[num];
					writer.Write(int2ListContainer2.List.Count);
					for (int num2 = 0; num2 < int2ListContainer2.List.Count; num2++)
					{
						writer.Write(int2ListContainer2.List[num2].X);
						writer.Write(int2ListContainer2.List[num2].Y);
					}
				}
				writer.Write(NearbyJointGroups.Count);
				for (int num3 = 0; num3 < NearbyJointGroups.Count; num3++)
				{
					Int2ListContainer int2ListContainer3 = NearbyJointGroups[num3];
					writer.Write(int2ListContainer3.List.Count);
					for (int num4 = 0; num4 < int2ListContainer3.List.Count; num4++)
					{
						writer.Write(int2ListContainer3.List[num4].X);
						writer.Write(int2ListContainer3.List[num4].Y);
					}
				}
				writer.Write(ParticleToNeibor.Length);
				for (int num5 = 0; num5 < ParticleToNeibor.Length; num5++)
				{
					writer.Write(ParticleToNeibor[num5]);
				}
				writer.Write(ParticleToNeiborCounts.Length);
				for (int num6 = 0; num6 < ParticleToNeiborCounts.Length; num6++)
				{
					writer.Write(ParticleToNeiborCounts[num6]);
				}
				writer.Write(ParticlesBlend.Length);
				for (int num7 = 0; num7 < ParticlesBlend.Length; num7++)
				{
					writer.Write(ParticlesBlend[num7]);
				}
				writer.Write(ParticlesStrength.Length);
				for (int num8 = 0; num8 < ParticlesStrength.Length; num8++)
				{
					writer.Write(ParticlesStrength[num8]);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Error while storeing ClothGeometryData to binary writer " + ex);
				return false;
			}
			return true;
		}

		public void LogStatistics()
		{
			Debug.Log("Vertices Num: " + MeshToPhysicsVerticesMap.Length);
			Debug.Log("Physics Vertices Num: " + Particles.Length);
			Debug.Log("Mesh Vertex To Neibor Num: " + ParticleToNeibor.Length);
			int num = JointGroups.Sum((Int2ListContainer container) => container.List.Count);
			Debug.Log("Joints Num: " + num);
			int num2 = StiffnessJointGroups.Sum((Int2ListContainer container) => container.List.Count);
			Debug.Log("Stiffness Joints Num: " + num2);
			int num3 = NearbyJointGroups.Sum((Int2ListContainer container) => container.List.Count);
			Debug.Log("Nearby Joints Num: " + num3);
		}
	}
}
