using System.Collections.Generic;
using GPUTools.Cloth.Scripts.Geometry.Data;
using GPUTools.Cloth.Scripts.Types;
using GPUTools.Common.Scripts.Tools.Commands;

namespace GPUTools.Cloth.Scripts.Geometry.Passes
{
	public class StiffnessJointsPass : ICacheCommand
	{
		private readonly ClothGeometryData data;

		private List<HashSet<int>> jointGroupsSet;

		private List<Int2ListContainer> jointGroups;

		protected bool cancelCache;

		public StiffnessJointsPass(ClothSettings settings)
		{
			data = settings.GeometryData;
		}

		public void CancelCache()
		{
			cancelCache = true;
		}

		public void PrepCache()
		{
			cancelCache = false;
		}

		public void Cache()
		{
			if (data != null)
			{
				List<Int2> list = CreateJointsList(data.AllTringles, data.MeshToPhysicsVerticesMap);
				if (!cancelCache)
				{
					data.StiffnessJointGroups = CreateJointsGroups(list);
				}
			}
		}

		private void AddNeighbors(Dictionary<int, HashSet<int>> vertToNeighborVerts, int index, int neighbor1, int neighbor2)
		{
			HashSet<int> value;
			if (!vertToNeighborVerts.TryGetValue(index, out value))
			{
				value = new HashSet<int>();
				vertToNeighborVerts.Add(index, value);
			}
			value.Add(neighbor1);
			value.Add(neighbor2);
		}

		private List<Int2> CreateJointsList(int[] indices, int[] meshToPhysicsVerticesMap)
		{
			HashSet<Int2> hashSet = new HashSet<Int2>();
			Dictionary<int, HashSet<int>> dictionary = new Dictionary<int, HashSet<int>>();
			for (int i = 0; i < indices.Length; i += 3)
			{
				if (cancelCache)
				{
					return null;
				}
				int num = meshToPhysicsVerticesMap[indices[i]];
				int num2 = meshToPhysicsVerticesMap[indices[i + 1]];
				int num3 = meshToPhysicsVerticesMap[indices[i + 2]];
				AddNeighbors(dictionary, num, num2, num3);
				AddNeighbors(dictionary, num2, num, num3);
				AddNeighbors(dictionary, num3, num, num2);
			}
			for (int j = 0; j < indices.Length; j++)
			{
				if (cancelCache)
				{
					return null;
				}
				if (j % 100 == 0)
				{
					data.status = "Stiffness Joints: " + j * 100 / indices.Length + "%";
				}
				int num4 = meshToPhysicsVerticesMap[indices[j]];
				HashSet<int> value;
				if (!dictionary.TryGetValue(num4, out value))
				{
					continue;
				}
				foreach (int item in value)
				{
					AddToHashSet(hashSet, num4, item);
					HashSet<int> value2;
					if (!dictionary.TryGetValue(item, out value2))
					{
						continue;
					}
					foreach (int item2 in value2)
					{
						if (num4 != item2 && !value.Contains(item2))
						{
							AddToHashSet(hashSet, num4, item2);
						}
					}
				}
			}
			return hashSet.ToList();
		}

		private void AddToHashSet(HashSet<Int2> set, int i1, int i2)
		{
			if (i1 != -1 && i2 != -1)
			{
				set.Add((i1 <= i2) ? new Int2(i2, i1) : new Int2(i1, i2));
			}
		}

		private List<Int2ListContainer> CreateJointsGroups(List<Int2> list)
		{
			jointGroupsSet = new List<HashSet<int>>();
			jointGroups = new List<Int2ListContainer>();
			foreach (Int2 item in list)
			{
				if (cancelCache)
				{
					return null;
				}
				AddJoint(item);
			}
			return jointGroups;
		}

		private void AddJoint(Int2 item)
		{
			for (int i = 0; i < jointGroupsSet.Count; i++)
			{
				HashSet<int> hashSet = jointGroupsSet[i];
				List<Int2> list = jointGroups[i].List;
				if (!hashSet.Contains(item.X) && !hashSet.Contains(item.Y))
				{
					hashSet.Add(item.X);
					hashSet.Add(item.Y);
					list.Add(item);
					return;
				}
			}
			CreateNewGroup(item);
		}

		private void CreateNewGroup(Int2 item)
		{
			HashSet<int> hashSet = new HashSet<int>();
			hashSet.Add(item.X);
			hashSet.Add(item.Y);
			jointGroupsSet.Add(hashSet);
			List<Int2> list = new List<Int2>();
			list.Add(item);
			List<Int2> list2 = list;
			jointGroups.Add(new Int2ListContainer
			{
				List = list2
			});
		}
	}
}
