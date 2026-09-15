using System.Collections.Generic;
using GPUTools.Cloth.Scripts.Geometry.Data;
using GPUTools.Cloth.Scripts.Types;
using GPUTools.Common.Scripts.Tools.Commands;
using UnityEngine;

namespace GPUTools.Cloth.Scripts.Geometry.Passes
{
	public class JointsPass : ICacheCommand
	{
		private readonly ClothGeometryData data;

		private List<HashSet<int>> jointGroupsSet;

		private List<Int2ListContainer> jointGroups;

		protected bool cancelCache;

		public JointsPass(ClothSettings settings)
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
				List<Vector2> list = CreateJointsList(data.AllTringles, data.MeshToPhysicsVerticesMap);
				if (!cancelCache)
				{
					data.JointGroups = CreateJointsGroups(list);
				}
			}
		}

		private List<Vector2> CreateJointsList(int[] indices, int[] meshToPhysicsVerticesMap)
		{
			HashSet<Vector2> hashSet = new HashSet<Vector2>();
			for (int i = 0; i < indices.Length; i += 3)
			{
				if (cancelCache)
				{
					return null;
				}
				int num = meshToPhysicsVerticesMap[indices[i]];
				int num2 = meshToPhysicsVerticesMap[indices[i + 1]];
				int num3 = meshToPhysicsVerticesMap[indices[i + 2]];
				AddToHashSet(hashSet, num, num2);
				AddToHashSet(hashSet, num2, num3);
				AddToHashSet(hashSet, num3, num);
			}
			return hashSet.ToList();
		}

		private void AddToHashSet(HashSet<Vector2> set, int i1, int i2)
		{
			if (i1 != -1 && i2 != -1)
			{
				set.Add((i1 <= i2) ? new Vector2(i2, i1) : new Vector2(i1, i2));
			}
		}

		private List<Int2ListContainer> CreateJointsGroups(List<Vector2> list)
		{
			jointGroupsSet = new List<HashSet<int>>();
			jointGroups = new List<Int2ListContainer>();
			foreach (Vector2 item in list)
			{
				if (cancelCache)
				{
					return null;
				}
				AddJoint(new Int2((int)item.x, (int)item.y));
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
