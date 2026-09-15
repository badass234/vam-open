using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	public static class ProjectItemTypes
	{
		public const int None = 0;

		public const int Folder = 1;

		public const int Scene = 2;

		public const int Obj = 1073741824;

		public const int Material = 1073741825;

		public const int Mesh = 1073741826;

		public const int Prefab = 1073741827;

		public const int Texture = 1073741828;

		public const int ProceduralMaterial = 1073741829;

		public static readonly Dictionary<int, string> Ext = new Dictionary<int, string>
		{
			{
				1,
				string.Empty
			},
			{ 2, "rtsc" },
			{ 1073741825, "rtmat" },
			{ 1073741829, "rtpmat" },
			{ 1073741826, "rtmesh" },
			{ 1073741827, "rtprefab" },
			{ 1073741828, "rtimg" },
			{ 1073741824, "rtobj" }
		};

		public static readonly Dictionary<Type, int> Type = new Dictionary<Type, int>
		{
			{
				typeof(GameObject),
				1073741827
			},
			{
				typeof(Mesh),
				1073741826
			},
			{
				typeof(Material),
				1073741825
			},
			{
				typeof(Texture),
				1073741828
			},
			{
				typeof(UnityEngine.Object),
				1073741824
			}
		};

		public static int GetProjectItemType(Type type)
		{
			while (type != null)
			{
				int value;
				if (Type.TryGetValue(type, out value))
				{
					return value;
				}
				type = type.BaseType();
			}
			return 0;
		}
	}
}
