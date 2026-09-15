using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Leap.Unity.Query;
using Leap.Unity.Swizzle;
using UnityEngine;

namespace Leap.Unity
{
	public static class Utils
	{
		public struct ChildrenEnumerator : IEnumerator<Transform>, IEnumerator, IDisposable
		{
			private Transform _t;

			private int _idx;

			private int _count;

			object IEnumerator.Current => Current;

			public Transform Current => (!(_t == null)) ? _t.GetChild(_idx) : null;

			public ChildrenEnumerator(Transform t)
			{
				_t = t;
				_idx = -1;
				_count = t.childCount;
			}

			public ChildrenEnumerator GetEnumerator()
			{
				return this;
			}

			public bool MoveNext()
			{
				if (_idx < _count)
				{
					_idx++;
				}
				if (_idx == _count)
				{
					return false;
				}
				return true;
			}

			public void Reset()
			{
				_idx = -1;
				_count = _t.childCount;
			}

			public void Dispose()
			{
			}
		}

		public struct HorizontalLineRectEnumerator
		{
			private Rect rect;

			private int numLines;

			private int index;

			public float eachHeight => rect.height / (float)numLines;

			public Rect Current => new Rect(rect.x, rect.y + eachHeight * (float)index, rect.width, eachHeight);

			public HorizontalLineRectEnumerator(Rect rect, int numLines)
			{
				this.rect = rect;
				this.numLines = numLines;
				index = -1;
			}

			public bool MoveNext()
			{
				index++;
				return index < numLines;
			}

			public HorizontalLineRectEnumerator GetEnumerator()
			{
				return this;
			}

			public void Reset()
			{
				index = -1;
			}

			public Query<Rect> Query()
			{
				List<Rect> list = Pool<List<Rect>>.Spawn();
				try
				{
					HorizontalLineRectEnumerator enumerator = GetEnumerator();
					while (enumerator.MoveNext())
					{
						Rect current = enumerator.Current;
						list.Add(current);
					}
					return new Query<Rect>(list);
				}
				finally
				{
					list.Clear();
					Pool<List<Rect>>.Recycle(list);
				}
			}
		}

		private static TextureFormat[] _incompressibleFormats = new TextureFormat[7]
		{
			TextureFormat.R16,
			TextureFormat.EAC_R,
			TextureFormat.EAC_R_SIGNED,
			TextureFormat.EAC_RG,
			TextureFormat.EAC_RG_SIGNED,
			TextureFormat.ETC_RGB4_3DS,
			TextureFormat.ETC_RGBA8_3DS
		};

		public static void Swap<T>(ref T a, ref T b)
		{
			T val = a;
			a = b;
			b = val;
		}

		public static void Swap<T>(this IList<T> list, int a, int b)
		{
			T value = list[a];
			list[a] = list[b];
			list[b] = value;
		}

		public static void Swap<T>(this T[] array, int a, int b)
		{
			Swap(ref array[a], ref array[b]);
		}

		public static void Reverse<T>(this T[] array)
		{
			int num = array.Length / 2;
			int num2 = 0;
			int num3 = array.Length;
			while (num2 < num)
			{
				array.Swap(num2++, --num3);
			}
		}

		public static void Reverse<T>(this T[] array, int start, int length)
		{
			int num = start + length / 2;
			int num2 = start;
			int num3 = start + length;
			while (num2 < num)
			{
				array.Swap(num2++, --num3);
			}
		}

		public static void Shuffle<T>(this IList<T> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list.Swap(i, UnityEngine.Random.Range(i, list.Count));
			}
		}

		public static void DoubleCapacity<T>(ref T[] array)
		{
			T[] array2 = new T[array.Length * 2];
			Array.Copy(array, array2, array.Length);
			array = array2;
		}

		public static bool AreEqualUnordered<T>(IList<T> a, IList<T> b)
		{
			Dictionary<T, int> dictionary = Pool<Dictionary<T, int>>.Spawn();
			try
			{
				int num = 0;
				foreach (T item in a)
				{
					if (item == null)
					{
						num++;
						continue;
					}
					int value;
					if (!dictionary.TryGetValue(item, out value))
					{
						value = 0;
					}
					dictionary[item] = value + 1;
				}
				foreach (T item2 in b)
				{
					if (item2 == null)
					{
						num--;
						continue;
					}
					int value2;
					if (!dictionary.TryGetValue(item2, out value2))
					{
						return false;
					}
					dictionary[item2] = value2 - 1;
				}
				if (num != 0)
				{
					return false;
				}
				foreach (KeyValuePair<T, int> item3 in dictionary)
				{
					if (item3.Value != 0)
					{
						return false;
					}
				}
				return true;
			}
			finally
			{
				dictionary.Clear();
				Pool<Dictionary<T, int>>.Recycle(dictionary);
			}
		}

		public static bool ImplementsInterface(this Type type, Type ifaceType)
		{
			Type[] interfaces = type.GetInterfaces();
			for (int i = 0; i < interfaces.Length; i++)
			{
				if (interfaces[i] == ifaceType)
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsActiveRelativeToParent(this Transform obj, Transform parent)
		{
			if (!obj.gameObject.activeSelf)
			{
				return false;
			}
			if (obj.parent == null || obj.parent == parent)
			{
				return true;
			}
			return obj.parent.IsActiveRelativeToParent(parent);
		}

		public static List<int> GetSortedOrder<T>(this IList<T> list) where T : IComparable<T>
		{
			List<int> list2 = new List<int>();
			for (int i = 0; i < list.Count; i++)
			{
				list2.Add(i);
			}
			list2.Sort((int a, int b) => list[a].CompareTo(list[b]));
			return list2;
		}

		public static void ApplyOrdering<T>(this IList<T> list, List<int> ordering)
		{
			List<T> list2 = Pool<List<T>>.Spawn();
			try
			{
				list2.AddRange(list);
				for (int i = 0; i < list.Count; i++)
				{
					list[i] = list2[ordering[i]];
				}
			}
			finally
			{
				list2.Clear();
				Pool<List<T>>.Recycle(list2);
			}
		}

		public static string MakeRelativePath(string relativeTo, string path)
		{
			if (string.IsNullOrEmpty(relativeTo))
			{
				throw new ArgumentNullException("relativeTo");
			}
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException("path");
			}
			Uri uri = new Uri(relativeTo);
			Uri uri2 = new Uri(path);
			if (uri.Scheme != uri2.Scheme)
			{
				return path;
			}
			Uri uri3 = uri.MakeRelativeUri(uri2);
			string text = Uri.UnescapeDataString(uri3.ToString());
			if (uri2.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
			{
				text = text.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			}
			return text;
		}

		public static string TrimEnd(this string str, int characters)
		{
			return str.Substring(0, Mathf.Max(0, str.Length - characters));
		}

		public static string TrimStart(this string str, int characters)
		{
			return str.Substring(Mathf.Min(str.Length, characters));
		}

		public static string Capitalize(this string str)
		{
			char c = str[0];
			if (char.IsLetter(c))
			{
				return char.ToUpper(c) + str.Substring(1);
			}
			return str;
		}

		public static string GenerateNiceName(string value)
		{
			string text = string.Empty;
			string curr = string.Empty;
			Func<char, bool> func = delegate(char c)
			{
				if (curr.Length > 0 && char.IsUpper(curr[0]))
				{
					return false;
				}
				if (!char.IsLetter(c))
				{
					return false;
				}
				curr = c + curr;
				return true;
			};
			Func<char, bool> func2 = delegate(char c)
			{
				if (!char.IsLetter(c))
				{
					return false;
				}
				if (char.IsLower(c))
				{
					return false;
				}
				curr = c + curr;
				return true;
			};
			Func<char, bool> func3 = delegate(char c)
			{
				if (!char.IsDigit(c))
				{
					return false;
				}
				curr = c + curr;
				return true;
			};
			Func<char, bool> func4 = (char c) => (!char.IsDigit(c) && !char.IsLetter(c)) ? true : false;
			Func<char, bool> func5 = null;
			int num = value.Length;
			while (num != 0)
			{
				num--;
				char c2 = value[num];
				if (func5 != null && func5(c2))
				{
					continue;
				}
				if (curr != string.Empty)
				{
					text = " " + curr.Capitalize() + text;
					curr = string.Empty;
				}
				if (func2(c2))
				{
					func5 = func2;
					continue;
				}
				if (func(c2))
				{
					func5 = func;
					continue;
				}
				if (func3(c2))
				{
					func5 = func3;
					continue;
				}
				if (func4(c2))
				{
					func5 = func4;
					continue;
				}
				throw new Exception("Unexpected state, no function matched character " + c2);
			}
			if (curr != string.Empty)
			{
				text = curr.Capitalize() + text;
			}
			text = text.Trim();
			if (text.StartsWith("M ") || text.StartsWith("K "))
			{
				text = text.Substring(2);
			}
			return text.Trim();
		}

		public static string ToArrayString<T>(this IEnumerable<T> enumerable)
		{
			string text = "[" + typeof(T).Name + ": ";
			bool flag = false;
			foreach (T item in enumerable)
			{
				if (flag)
				{
					text += ", ";
				}
				text += item.ToString();
				flag = true;
			}
			return text + "]";
		}

		public static int Repeat(int x, int m)
		{
			int num = x % m;
			return (num >= 0) ? num : (num + m);
		}

		public static int Sign(int value)
		{
			if (value == 0)
			{
				return 0;
			}
			if (value > 0)
			{
				return 1;
			}
			return -1;
		}

		public static Vector2 Perpendicular(this Vector2 vector)
		{
			return new Vector2(vector.y, 0f - vector.x);
		}

		public static Vector3 Perpendicular(this Vector3 vector)
		{
			float num = vector.x * vector.x;
			float num2 = vector.y * vector.y;
			float num3 = vector.z * vector.z;
			float num4 = num3 + num;
			float num5 = num2 + num;
			float num6 = num3 + num2;
			if (num4 > num5)
			{
				if (num4 > num6)
				{
					return new Vector3(0f - vector.z, 0f, vector.x);
				}
				return new Vector3(0f, vector.z, 0f - vector.y);
			}
			if (num5 > num6)
			{
				return new Vector3(vector.y, 0f - vector.x, 0f);
			}
			return new Vector3(0f, vector.z, 0f - vector.y);
		}

		public static bool ContainsNaN(this Vector3 v)
		{
			return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
		}

		public static bool IsBetween(this float f, float f0, float f1)
		{
			if (f0 > f1)
			{
				Swap(ref f0, ref f1);
			}
			return f0 <= f && f <= f1;
		}

		public static bool IsBetween(this double d, double d0, double d1)
		{
			if (d0 > d1)
			{
				Swap(ref d0, ref d1);
			}
			return d0 <= d && d <= d1;
		}

		public static Vector3 TimedExtrapolate(Vector3 a, float aTime, Vector3 b, float bTime, float extrapolatedTime)
		{
			return Vector3.LerpUnclamped(a, b, extrapolatedTime.MapUnclamped(aTime, bTime, 0f, 1f));
		}

		public static Quaternion TimedExtrapolate(Quaternion a, float aTime, Quaternion b, float bTime, float extrapolatedTime)
		{
			return Quaternion.SlerpUnclamped(a, b, extrapolatedTime.MapUnclamped(aTime, bTime, 0f, 1f));
		}

		public static bool NextTuple(IList<int> tuple, int maxValue)
		{
			return NextTuple(tuple, (int i) => (i + 1) % maxValue);
		}

		public static bool NextTuple<T>(IList<T> tuple, Func<T, T> nextItem) where T : IComparable<T>
		{
			for (int num = tuple.Count - 1; num >= 0; num--)
			{
				T val = tuple[num];
				if ((tuple[num] = nextItem(val)).CompareTo(val) > 0)
				{
					return true;
				}
			}
			return false;
		}

		public static T[] ClearWithDefaults<T>(this T[] arr)
		{
			for (int i = 0; i < arr.Length; i++)
			{
				arr[i] = default(T);
			}
			return arr;
		}

		public static T[] ClearWith<T>(this T[] arr, T value)
		{
			for (int i = 0; i < arr.Length; i++)
			{
				arr[i] = value;
			}
			return arr;
		}

		public static void EnsureListExists<T>(ref List<T> list)
		{
			if (list == null)
			{
				list = new List<T>();
			}
		}

		public static void EnsureListCount<T>(this List<T> list, int count)
		{
			if (list.Count != count)
			{
				while (list.Count < count)
				{
					list.Add(default(T));
				}
				while (list.Count > count)
				{
					list.RemoveAt(list.Count - 1);
				}
			}
		}

		public static void EnsureListCount<T>(this List<T> list, int count, Func<T> createT, Action<T> deleteT = null)
		{
			while (list.Count < count)
			{
				list.Add(createT());
			}
			while (list.Count > count)
			{
				T obj = list[list.Count - 1];
				list.RemoveAt(list.Count - 1);
				deleteT?.Invoke(obj);
			}
		}

		public static void Add<T>(this List<T> list, T t0, T t1)
		{
			list.Add(t0);
			list.Add(t1);
		}

		public static void Add<T>(this List<T> list, T t0, T t1, T t2)
		{
			list.Add(t0);
			list.Add(t1);
			list.Add(t2);
		}

		public static void Add<T>(this List<T> list, T t0, T t1, T t2, T t3)
		{
			list.Add(t0);
			list.Add(t1);
			list.Add(t2);
			list.Add(t3);
		}

		public static T FindObjectInHierarchy<T>() where T : UnityEngine.Object
		{
			return (from o in Resources.FindObjectsOfTypeAll<T>().Query()
				where true
				select o).FirstOrDefault();
		}

		public static ChildrenEnumerator GetChildren(this Transform t)
		{
			return new ChildrenEnumerator(t);
		}

		public static void ResetLocalTransform(this Transform t)
		{
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = Vector3.one;
		}

		public static void ResetLocalPose(this Transform t)
		{
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
		}

		public static void FindOwnedChildComponents<ComponentType, OwnerType>(OwnerType rootObj, List<ComponentType> ownedComponents, bool includeInactiveObjects = false) where OwnerType : Component
		{
			ownedComponents.Clear();
			Stack<Transform> stack = Pool<Stack<Transform>>.Spawn();
			List<ComponentType> list = Pool<List<ComponentType>>.Spawn();
			try
			{
				stack.Push(rootObj.transform);
				while (stack.Count > 0)
				{
					Transform transform = stack.Pop();
					foreach (Transform child in transform.GetChildren())
					{
						if (child.GetComponent<OwnerType>() == null && (includeInactiveObjects || child.gameObject.activeInHierarchy))
						{
							stack.Push(child);
						}
					}
					list.Clear();
					transform.GetComponents(list);
					foreach (ComponentType item in list)
					{
						ownedComponents.Add(item);
					}
				}
			}
			finally
			{
				stack.Clear();
				Pool<Stack<Transform>>.Recycle(stack);
				list.Clear();
				Pool<List<ComponentType>>.Recycle(list);
			}
		}

		public static void LookAwayFrom(this Transform thisTransform, Transform transform)
		{
			thisTransform.rotation = Quaternion.LookRotation(thisTransform.position - transform.position, Vector3.up);
		}

		public static void LookAwayFrom(this Transform thisTransform, Transform transform, Vector3 upwards)
		{
			thisTransform.rotation = Quaternion.LookRotation(thisTransform.position - transform.position, upwards);
		}

		public static Vector3 ToVector3(this Vector4 v4)
		{
			return new Vector3(v4.x, v4.y, v4.z);
		}

		public static Vector3 InLocalSpace(this Vector3 v, Transform t)
		{
			return t.InverseTransformPoint(v);
		}

		public static Vector3 ToAngleAxisVector(this Quaternion q)
		{
			float angle;
			Vector3 axis;
			q.ToAngleAxis(out angle, out axis);
			return axis * angle;
		}

		public static Quaternion QuaternionFromAngleAxisVector(Vector3 angleAxisVector)
		{
			if (angleAxisVector == Vector3.zero)
			{
				return Quaternion.identity;
			}
			return Quaternion.AngleAxis(angleAxisVector.magnitude, angleAxisVector);
		}

		public static Quaternion ToNormalized(this Quaternion quaternion)
		{
			float x = quaternion.x;
			float y = quaternion.y;
			float z = quaternion.z;
			float w = quaternion.w;
			float num = Mathf.Sqrt(x * x + y * y + z * z + w * w);
			if (Mathf.Approximately(num, 0f))
			{
				return Quaternion.identity;
			}
			return new Quaternion(x / num, y / num, z / num, w / num);
		}

		public static Quaternion FaceTargetWithoutTwist(Vector3 fromPosition, Vector3 targetPosition, bool flip180 = false)
		{
			return FaceTargetWithoutTwist(fromPosition, targetPosition, Vector3.up, flip180);
		}

		public static Quaternion FaceTargetWithoutTwist(Vector3 fromPosition, Vector3 targetPosition, Vector3 upwardDirection, bool flip180 = false)
		{
			Vector3 vector = targetPosition - fromPosition;
			return Quaternion.LookRotation(((!flip180) ? 1 : (-1)) * vector, upwardDirection);
		}

		public static Quaternion Flipped(this Quaternion q)
		{
			return new Quaternion(0f - q.x, 0f - q.y, 0f - q.z, 0f - q.w);
		}

		public static void CompressQuatToBytes(Quaternion quat, byte[] buffer, ref int offset)
		{
			int num = 0;
			float num2 = Mathf.Abs(quat.w);
			float num3 = Mathf.Abs(quat.x);
			float num4 = Mathf.Abs(quat.y);
			float num5 = Mathf.Abs(quat.z);
			float num6 = num3;
			if (num4 > num6)
			{
				num = 1;
				num6 = num4;
			}
			if (num5 > num6)
			{
				num = 2;
				num6 = num5;
			}
			if (num2 > num6)
			{
				num = 3;
				num6 = num2;
			}
			float num7;
			float num8;
			float num9;
			if (quat[num] >= 0f)
			{
				num7 = quat[(num + 1) % 4];
				num8 = quat[(num + 2) % 4];
				num9 = quat[(num + 3) % 4];
			}
			else
			{
				num7 = 0f - quat[(num + 1) % 4];
				num8 = 0f - quat[(num + 2) % 4];
				num9 = 0f - quat[(num + 3) % 4];
			}
			uint num10 = (uint)(num << 30);
			float num11 = Mathf.Clamp01((num7 - -0.70710653f) / 1.4142131f);
			uint num12 = (uint)Mathf.Floor(num11 * 1023f + 0.5f);
			num10 |= (num12 & 0x3FF) << 20;
			num11 = Mathf.Clamp01((num8 - -0.70710653f) / 1.4142131f);
			num12 = (uint)Mathf.Floor(num11 * 1023f + 0.5f);
			num10 |= (num12 & 0x3FF) << 10;
			num11 = Mathf.Clamp01((num9 - -0.70710653f) / 1.4142131f);
			num12 = (uint)Mathf.Floor(num11 * 1023f + 0.5f);
			num10 |= num12 & 0x3FF;
			BitConverterNonAlloc.GetBytes(num10, buffer, ref offset);
		}

		public static Quaternion DecompressBytesToQuat(byte[] bytes, ref int offset)
		{
			uint num = BitConverterNonAlloc.ToUInt32(bytes, ref offset);
			int num2 = (int)(num >> 30);
			uint num3 = (num >> 20) & 0x3FF;
			float num4 = (float)num3 / 1023f;
			float num5 = num4 * 1.4142131f + -0.70710653f;
			num3 = (num >> 10) & 0x3FF;
			num4 = (float)num3 / 1023f;
			float num6 = num4 * 1.4142131f + -0.70710653f;
			num3 = num & 0x3FF;
			num4 = (float)num3 / 1023f;
			float num7 = num4 * 1.4142131f + -0.70710653f;
			Quaternion identity = Quaternion.identity;
			float value = Mathf.Sqrt(1f - num5 * num5 - num6 * num6 - num7 * num7);
			identity[num2] = value;
			identity[(num2 + 1) % 4] = num5;
			identity[(num2 + 2) % 4] = num6;
			identity[(num2 + 3) % 4] = num7;
			return identity;
		}

		public static Matrix4x4 CompMul(Matrix4x4 m, float f)
		{
			return new Matrix4x4(m.GetColumn(0) * f, m.GetColumn(1) * f, m.GetColumn(2) * f, m.GetColumn(3) * f);
		}

		public static void IgnoreCollisions(GameObject first, GameObject second, bool ignore = true)
		{
			if (first == null || second == null)
			{
				return;
			}
			List<Collider> list = Pool<List<Collider>>.Spawn();
			list.Clear();
			List<Collider> list2 = Pool<List<Collider>>.Spawn();
			list2.Clear();
			try
			{
				first.GetComponentsInChildren(list);
				second.GetComponentsInChildren(list2);
				for (int i = 0; i < list.Count; i++)
				{
					for (int j = 0; j < list2.Count; j++)
					{
						if (list[i] != list2[j] && list[i].enabled && list2[j].enabled)
						{
							Physics.IgnoreCollision(list[i], list2[j], ignore);
						}
					}
				}
			}
			finally
			{
				list.Clear();
				Pool<List<Collider>>.Recycle(list);
				list2.Clear();
				Pool<List<Collider>>.Recycle(list2);
			}
		}

		public static Vector3 GetDirection(this CapsuleCollider capsule)
		{
			switch (capsule.direction)
			{
			case 0:
				return Vector3.right;
			case 1:
				return Vector3.up;
			default:
				return Vector3.forward;
			}
		}

		public static float GetEffectiveRadius(this CapsuleCollider capsule)
		{
			return capsule.radius * capsule.GetEffectiveRadiusMultiplier();
		}

		public static float GetEffectiveRadiusMultiplier(this CapsuleCollider capsule)
		{
			float num = 0f;
			switch (capsule.direction)
			{
			case 0:
				return capsule.transform.lossyScale.yz().CompMax();
			case 1:
				return capsule.transform.lossyScale.xz().CompMax();
			default:
				return capsule.transform.lossyScale.xy().CompMax();
			}
		}

		public static void GetCapsulePoints(this CapsuleCollider capsule, out Vector3 a, out Vector3 b)
		{
			float effectiveRadiusMultiplier = capsule.GetEffectiveRadiusMultiplier();
			Vector3 direction = capsule.GetDirection();
			a = direction * (capsule.height / 2f);
			b = -a;
			a = capsule.transform.TransformPoint(a);
			b = capsule.transform.TransformPoint(b);
			a -= direction * effectiveRadiusMultiplier * capsule.radius;
			b += direction * effectiveRadiusMultiplier * capsule.radius;
		}

		public static void SetCapsulePoints(this CapsuleCollider capsule, Vector3 a, Vector3 b)
		{
			capsule.center = Vector3.zero;
			capsule.transform.position = (a + b) / 2f;
			Vector3 direction = capsule.GetDirection();
			Vector3 fromDirection = capsule.transform.TransformDirection(direction);
			Quaternion quaternion = Quaternion.FromToRotation(fromDirection, a - capsule.transform.position);
			capsule.transform.rotation = quaternion * capsule.transform.rotation;
			float magnitude = capsule.transform.InverseTransformPoint(a).magnitude;
			capsule.height = (magnitude + capsule.radius) * 2f;
		}

		public static void FindColliders<T>(GameObject obj, List<T> colliders, bool includeInactiveObjects = false) where T : Collider
		{
			colliders.Clear();
			Stack<Transform> stack = Pool<Stack<Transform>>.Spawn();
			List<T> list = Pool<List<T>>.Spawn();
			try
			{
				stack.Push(obj.transform);
				while (stack.Count > 0)
				{
					Transform transform = stack.Pop();
					foreach (Transform child in transform.GetChildren())
					{
						if (child.GetComponent<Rigidbody>() == null && (includeInactiveObjects || child.gameObject.activeSelf))
						{
							stack.Push(child);
						}
					}
					list.Clear();
					transform.GetComponents(list);
					foreach (T item in list)
					{
						colliders.Add(item);
					}
				}
			}
			finally
			{
				stack.Clear();
				Pool<Stack<Transform>>.Recycle(stack);
				list.Clear();
				Pool<List<T>>.Recycle(list);
			}
		}

		public static Color WithAlpha(this Color color, float alpha)
		{
			return new Color(color.r, color.g, color.b, alpha);
		}

		public static Color ParseHtmlColorString(string htmlString)
		{
			Color color;
			if (!ColorUtility.TryParseHtmlString(htmlString, out color))
			{
				throw new ArgumentException("The string [" + htmlString + "] is not a valid color code.  Valid color codes include:\n#RGB\n#RGBA\n#RRGGBB\n#RRGGBBAA\nFor more information, see the documentation for ColorUtility.TryParseHtmlString.");
			}
			return color;
		}

		public static Color LerpHSV(this Color color, Color towardsColor, float t)
		{
			float H;
			float S;
			float V;
			Color.RGBToHSV(color, out H, out S, out V);
			float H2;
			float S2;
			float V2;
			Color.RGBToHSV(towardsColor, out H2, out S2, out V2);
			if (H - H2 < -0.5f)
			{
				H += 1f;
			}
			if (H - H2 > 0.5f)
			{
				H2 += 1f;
			}
			float h = Mathf.Lerp(H, H2, t) % 1f;
			float s = Mathf.Lerp(S, S2, t);
			float v = Mathf.Lerp(V, V2, t);
			return Color.HSVToRGB(h, s, v);
		}

		public static float LerpHue(float h0, float h1, float t)
		{
			if (h0 < 0f)
			{
				h0 = 1f - (0f - h0) % 1f;
			}
			if (h1 < 0f)
			{
				h1 = 1f - (0f - h1) % 1f;
			}
			if (h0 > 1f)
			{
				h0 %= 1f;
			}
			if (h1 > 1f)
			{
				h1 %= 1f;
			}
			if (h0 - h1 < -0.5f)
			{
				h0 += 1f;
			}
			if (h0 - h1 > 0.5f)
			{
				h1 += 1f;
			}
			return Mathf.Lerp(h0, h1, t) % 1f;
		}

		public static void DrawCircle(Vector3 center, Vector3 normal, float radius, Color color, int quality = 32, float duration = 0f, bool depthTest = true)
		{
			Vector3 forward = Vector3.Slerp(normal, -normal, 0.5f);
			DrawArc(360f, center, forward, normal, radius, color, quality);
		}

		public static void DrawArc(float arc, Vector3 center, Vector3 forward, Vector3 normal, float radius, Color color, int quality = 32)
		{
			Gizmos.color = color;
			Vector3 normalized = Vector3.Cross(normal, forward).normalized;
			float num = arc / (float)quality;
			Vector3 from = center + forward * radius;
			Vector3 vector = default(Vector3);
			for (float num2 = 0f; Mathf.Abs(num2) <= Mathf.Abs(arc); num2 += num)
			{
				float num3 = Mathf.Cos(num2 * ((float)Math.PI / 180f));
				float num4 = Mathf.Sin(num2 * ((float)Math.PI / 180f));
				vector.x = center.x + radius * (num3 * forward.x + num4 * normalized.x);
				vector.y = center.y + radius * (num3 * forward.y + num4 * normalized.y);
				vector.z = center.z + radius * (num3 * forward.z + num4 * normalized.z);
				Gizmos.DrawLine(from, vector);
				from = vector;
			}
		}

		public static void DrawCone(Vector3 origin, Vector3 direction, float angle, float height, Color color, int quality = 4, float duration = 0f, bool depthTest = true)
		{
			float num = height / (float)quality;
			for (float num2 = num; num2 <= height; num2 += num)
			{
				DrawCircle(origin + direction * num2, direction, Mathf.Tan(angle * ((float)Math.PI / 180f)) * num2, color, quality * 8, duration, depthTest);
			}
		}

		public static bool IsCompressible(TextureFormat format)
		{
			if (format < (TextureFormat)0)
			{
				return false;
			}
			return Array.IndexOf(_incompressibleFormats, format) < 0;
		}

		public static float Area(this Rect rect)
		{
			return rect.width * rect.height;
		}

		public static Rect Extrude(this Rect r, float margin)
		{
			return new Rect(r.x - margin, r.y - margin, r.width + margin * 2f, r.height + margin * 2f);
		}

		public static Rect PadInner(this Rect r, float padding)
		{
			return r.PadInner(padding, padding, padding, padding);
		}

		public static Rect PadInner(this Rect r, float padTop, float padBottom, float padLeft, float padRight)
		{
			float x = r.x + padLeft;
			float y = r.y + padBottom;
			float num = r.width - padRight - padLeft;
			float num2 = r.height - padTop - padBottom;
			if (num < 0f)
			{
				x = r.x + padLeft / (padLeft + padRight) * r.width;
				num = 0f;
			}
			if (num2 < 0f)
			{
				y = r.y + padBottom / (padBottom + padTop) * r.height;
				num2 = 0f;
			}
			return new Rect(x, y, num, num2);
		}

		public static Rect PadTop(this Rect r, float padding)
		{
			return r.PadInner(padding, 0f, 0f, 0f);
		}

		public static Rect PadBottom(this Rect r, float padding)
		{
			return r.PadInner(0f, padding, 0f, 0f);
		}

		public static Rect PadLeft(this Rect r, float padding)
		{
			return r.PadInner(0f, 0f, padding, 0f);
		}

		public static Rect PadRight(this Rect r, float padding)
		{
			return r.PadInner(0f, 0f, 0f, padding);
		}

		public static Rect PadTop(this Rect r, float padding, out Rect marginRect)
		{
			marginRect = r.TakeTop(padding);
			return r.PadTop(padding);
		}

		public static Rect PadBottom(this Rect r, float padding, out Rect marginRect)
		{
			marginRect = r.TakeBottom(padding);
			return r.PadBottom(padding);
		}

		public static Rect PadLeft(this Rect r, float padding, out Rect marginRect)
		{
			marginRect = r.TakeLeft(padding);
			return r.PadLeft(padding);
		}

		public static Rect PadRight(this Rect r, float padding, out Rect marginRect)
		{
			marginRect = r.TakeRight(padding);
			return r.PadRight(padding);
		}

		public static Rect PadTopBottomPercent(this Rect r, float padPercent)
		{
			float num = r.height * padPercent;
			return r.PadInner(num, num, 0f, 0f);
		}

		public static Rect PadLeftRightPercent(this Rect r, float padPercent)
		{
			float num = r.width * padPercent;
			return r.PadInner(0f, 0f, num, num);
		}

		public static Rect PadTopPercent(this Rect r, float padPercent)
		{
			float padding = r.height * padPercent;
			return r.PadTop(padding);
		}

		public static Rect PadBottomPercent(this Rect r, float padPercent)
		{
			float padding = r.height * padPercent;
			return r.PadBottom(padding);
		}

		public static Rect PadLeftPercent(this Rect r, float padPercent)
		{
			return r.PadLeft(r.width * padPercent);
		}

		public static Rect PadRightPercent(this Rect r, float padPercent)
		{
			return r.PadRight(r.width * padPercent);
		}

		public static Rect TakeTop(this Rect r, float heightFromTop)
		{
			heightFromTop = Mathf.Clamp(heightFromTop, 0f, r.height);
			return new Rect(r.x, r.y + r.height - heightFromTop, r.width, heightFromTop);
		}

		public static Rect TakeBottom(this Rect r, float heightFromBottom)
		{
			heightFromBottom = Mathf.Clamp(heightFromBottom, 0f, r.height);
			return new Rect(r.x, r.y, r.width, heightFromBottom);
		}

		public static Rect TakeLeft(this Rect r, float widthFromLeft)
		{
			widthFromLeft = Mathf.Clamp(widthFromLeft, 0f, r.width);
			return new Rect(r.x, r.y, widthFromLeft, r.height);
		}

		public static Rect TakeRight(this Rect r, float widthFromRight)
		{
			widthFromRight = Mathf.Clamp(widthFromRight, 0f, r.width);
			return new Rect(r.x + r.width - widthFromRight, r.y, r.height, widthFromRight);
		}

		public static Rect TakeTop(this Rect r, float padding, out Rect theRest)
		{
			theRest = r.PadTop(padding);
			return r.TakeTop(padding);
		}

		public static Rect TakeBottom(this Rect r, float padding, out Rect theRest)
		{
			theRest = r.PadBottom(padding);
			return r.TakeBottom(padding);
		}

		public static Rect TakeLeft(this Rect r, float padding, out Rect theRest)
		{
			theRest = r.PadLeft(padding);
			return r.TakeLeft(padding);
		}

		public static Rect TakeRight(this Rect r, float padding, out Rect theRest)
		{
			theRest = r.PadRight(padding);
			return r.TakeRight(padding);
		}

		public static Rect TakeHorizontal(this Rect r, float lineHeight, out Rect theRest, bool fromTop = true)
		{
			theRest = new Rect(r.x, (!fromTop) ? r.y : (r.y + lineHeight), r.width, r.height - lineHeight);
			return new Rect(r.x, (!fromTop) ? (r.y + r.height - lineHeight) : r.y, r.width, lineHeight);
		}

		public static void SplitHorizontallyWithLeft(this Rect rect, out Rect left, out Rect right, float leftWidth)
		{
			left = rect;
			left.width = leftWidth;
			right = rect;
			right.x += left.width;
			right.width = rect.width - leftWidth;
		}

		public static HorizontalLineRectEnumerator TakeAllLines(this Rect r, int numLines)
		{
			return new HorizontalLineRectEnumerator(r, numLines);
		}

		public static Pose From(this Vector3 position, Pose fromPose)
		{
			return new Pose(position, fromPose.rotation).From(fromPose);
		}

		public static Pose GetPose(this Rigidbody rigidbody)
		{
			return new Pose(rigidbody.position, rigidbody.rotation);
		}

		public static Pose MirroredX(this Pose pose)
		{
			Vector3 position = pose.position;
			Quaternion rotation = pose.rotation;
			return new Pose(new Vector3(0f - position.x, position.y, position.z), new Quaternion(0f - rotation.x, rotation.y, rotation.z, 0f - rotation.w).Flipped());
		}

		public static Pose Negated(this Pose pose)
		{
			Vector3 position = pose.position;
			Quaternion rotation = pose.rotation;
			return new Pose(new Vector3(0f - position.x, 0f - position.y, 0f - position.z), new Quaternion(0f - rotation.z, 0f - rotation.y, 0f - rotation.z, rotation.w));
		}

		public static float Map(this float value, float valueMin, float valueMax, float resultMin, float resultMax)
		{
			if (valueMin == valueMax)
			{
				return resultMin;
			}
			return Mathf.Lerp(resultMin, resultMax, (value - valueMin) / (valueMax - valueMin));
		}

		public static float MapUnclamped(this float value, float valueMin, float valueMax, float resultMin, float resultMax)
		{
			if (valueMin == valueMax)
			{
				return resultMin;
			}
			return Mathf.LerpUnclamped(resultMin, resultMax, (value - valueMin) / (valueMax - valueMin));
		}

		public static Vector2 Map(this Vector2 value, float valueMin, float valueMax, float resultMin, float resultMax)
		{
			return new Vector2(value.x.Map(valueMin, valueMax, resultMin, resultMax), value.y.Map(valueMin, valueMax, resultMin, resultMax));
		}

		public static Vector2 MapUnclamped(this Vector2 value, float valueMin, float valueMax, float resultMin, float resultMax)
		{
			return new Vector2(value.x.MapUnclamped(valueMin, valueMax, resultMin, resultMax), value.y.MapUnclamped(valueMin, valueMax, resultMin, resultMax));
		}

		public static Vector3 Map(this Vector3 value, float valueMin, float valueMax, float resultMin, float resultMax)
		{
			return new Vector3(value.x.Map(valueMin, valueMax, resultMin, resultMax), value.y.Map(valueMin, valueMax, resultMin, resultMax), value.z.Map(valueMin, valueMax, resultMin, resultMax));
		}

		public static Vector3 MapUnclamped(this Vector3 value, float valueMin, float valueMax, float resultMin, float resultMax)
		{
			return new Vector3(value.x.MapUnclamped(valueMin, valueMax, resultMin, resultMax), value.y.MapUnclamped(valueMin, valueMax, resultMin, resultMax), value.z.MapUnclamped(valueMin, valueMax, resultMin, resultMax));
		}

		public static Vector4 Map(this Vector4 value, float valueMin, float valueMax, float resultMin, float resultMax)
		{
			return new Vector4(value.x.Map(valueMin, valueMax, resultMin, resultMax), value.y.Map(valueMin, valueMax, resultMin, resultMax), value.z.Map(valueMin, valueMax, resultMin, resultMax), value.w.Map(valueMin, valueMax, resultMin, resultMax));
		}

		public static Vector4 MapUnclamped(this Vector4 value, float valueMin, float valueMax, float resultMin, float resultMax)
		{
			return new Vector4(value.x.MapUnclamped(valueMin, valueMax, resultMin, resultMax), value.y.MapUnclamped(valueMin, valueMax, resultMin, resultMax), value.z.MapUnclamped(valueMin, valueMax, resultMin, resultMax), value.w.MapUnclamped(valueMin, valueMax, resultMin, resultMax));
		}

		public static Vector2 Map(float input, float valueMin, float valueMax, Vector2 resultMin, Vector2 resultMax)
		{
			return Vector2.Lerp(resultMin, resultMax, Mathf.InverseLerp(valueMin, valueMax, input));
		}

		public static Vector3 Map(float input, float valueMin, float valueMax, Vector3 resultMin, Vector3 resultMax)
		{
			return Vector3.Lerp(resultMin, resultMax, Mathf.InverseLerp(valueMin, valueMax, input));
		}

		public static Vector4 Map(float input, float valueMin, float valueMax, Vector4 resultMin, Vector4 resultMax)
		{
			return Vector4.Lerp(resultMin, resultMax, Mathf.InverseLerp(valueMin, valueMax, input));
		}

		public static Vector2 CompMul(this Vector2 A, Vector2 B)
		{
			return new Vector2(A.x * B.x, A.y * B.y);
		}

		public static Vector3 CompMul(this Vector3 A, Vector3 B)
		{
			return new Vector3(A.x * B.x, A.y * B.y, A.z * B.z);
		}

		public static Vector4 CompMul(this Vector4 A, Vector4 B)
		{
			return new Vector4(A.x * B.x, A.y * B.y, A.z * B.z, A.w * B.w);
		}

		public static Vector2 CompDiv(this Vector2 A, Vector2 B)
		{
			return new Vector2(A.x / B.x, A.y / B.y);
		}

		public static Vector3 CompDiv(this Vector3 A, Vector3 B)
		{
			return new Vector3(A.x / B.x, A.y / B.y, A.z / B.z);
		}

		public static Vector4 CompDiv(this Vector4 A, Vector4 B)
		{
			return new Vector4(A.x / B.x, A.y / B.y, A.z / B.z, A.w / B.w);
		}

		public static float CompSum(this Vector2 v)
		{
			return v.x + v.y;
		}

		public static float CompSum(this Vector3 v)
		{
			return v.x + v.y + v.z;
		}

		public static float CompSum(this Vector4 v)
		{
			return v.x + v.y + v.z + v.w;
		}

		public static float CompMax(this Vector2 v)
		{
			return Mathf.Max(v.x, v.y);
		}

		public static float CompMax(this Vector3 v)
		{
			return Mathf.Max(Mathf.Max(v.x, v.y), v.z);
		}

		public static float CompMax(this Vector4 v)
		{
			return Mathf.Max(Mathf.Max(Mathf.Max(v.x, v.y), v.z), v.w);
		}

		public static float CompMin(this Vector2 v)
		{
			return Mathf.Min(v.x, v.y);
		}

		public static float CompMin(this Vector3 v)
		{
			return Mathf.Min(Mathf.Min(v.x, v.y), v.z);
		}

		public static float CompMin(this Vector4 v)
		{
			return Mathf.Min(Mathf.Min(Mathf.Min(v.x, v.y), v.z), v.w);
		}

		public static float From(this float thisFloat, float otherFloat)
		{
			return thisFloat - otherFloat;
		}

		public static float To(this float thisFloat, float otherFloat)
		{
			return otherFloat - thisFloat;
		}

		public static float Then(this float thisFloat, float otherFloat)
		{
			return thisFloat + otherFloat;
		}

		public static Vector3 From(this Vector3 thisVector, Vector3 otherVector)
		{
			return thisVector - otherVector;
		}

		public static Vector3 To(this Vector3 thisVector, Vector3 otherVector)
		{
			return otherVector - thisVector;
		}

		public static Vector3 Then(this Vector3 thisVector, Vector3 otherVector)
		{
			return thisVector + otherVector;
		}

		public static Quaternion From(this Quaternion thisQuaternion, Quaternion otherQuaternion)
		{
			return Quaternion.Inverse(otherQuaternion) * thisQuaternion;
		}

		public static Quaternion To(this Quaternion thisQuaternion, Quaternion otherQuaternion)
		{
			return Quaternion.Inverse(thisQuaternion) * otherQuaternion;
		}

		public static Quaternion Then(this Quaternion thisQuaternion, Quaternion otherQuaternion)
		{
			return thisQuaternion * otherQuaternion;
		}

		public static Pose From(this Pose thisPose, Pose otherPose)
		{
			return otherPose.inverse * thisPose;
		}

		public static Pose To(this Pose thisPose, Pose otherPose)
		{
			return thisPose.inverse * otherPose;
		}

		public static Pose Then(this Pose thisPose, Pose otherPose)
		{
			return thisPose * otherPose;
		}

		public static Matrix4x4 From(this Matrix4x4 thisMatrix, Matrix4x4 otherMatrix)
		{
			return thisMatrix * otherMatrix.inverse;
		}

		public static Matrix4x4 To(this Matrix4x4 thisMatrix, Matrix4x4 otherMatrix)
		{
			return otherMatrix * thisMatrix.inverse;
		}

		public static Matrix4x4 Then(this Matrix4x4 thisMatrix, Matrix4x4 otherMatrix)
		{
			return otherMatrix * thisMatrix;
		}
	}
}
