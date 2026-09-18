using System;
using System.Collections;
using System.Reflection;

namespace Mono.CSharp
{
	public class Comparer : IComparer
	{
		private delegate int ComparerFunc(object a, object b);

		private ComparerFunc cmp;

		private static Comparer MemberInfoComparer = new Comparer(CompareMemberInfo);

		private static Comparer MethodBaseComparer = new Comparer(CompareMethodBase);

		private static Comparer PropertyInfoComparer = new Comparer(ComparePropertyInfo);

		private static Comparer EventInfoComparer = new Comparer(CompareEventInfo);

		private Comparer(ComparerFunc f)
		{
			cmp = f;
		}

		public int Compare(object a, object b)
		{
			return cmp(a, b);
		}

		private static int CompareType(object a, object b)
		{
			Type obj = (Type)a;
			return string.Compare(strB: ((Type)b).Name, strA: obj.Name);
		}

		private static int CompareMemberInfo(object a, object b)
		{
			return string.Compare(((MemberInfo)a).Name, ((MemberInfo)b).Name);
		}

		public static MemberInfo[] Sort(MemberInfo[] inf)
		{
			Array.Sort(inf, MemberInfoComparer);
			return inf;
		}

		private static int CompareMethodBase(object a, object b)
		{
			MethodBase methodBase = (MethodBase)a;
			MethodBase methodBase2 = (MethodBase)b;
			if (methodBase.IsStatic == methodBase2.IsStatic)
			{
				int num = CompareMemberInfo(a, b);
				if (num != 0)
				{
					return num;
				}
				ParameterInfo[] parameters = methodBase.GetParameters();
				ParameterInfo[] parameters2 = methodBase2.GetParameters();
				int num2 = System.Math.Min(parameters.Length, parameters2.Length);
				for (int i = 0; i < num2; i++)
				{
					if ((num = CompareType(parameters[i].ParameterType, parameters2[i].ParameterType)) != 0)
					{
						return num;
					}
				}
				return parameters.Length.CompareTo(parameters2.Length);
			}
			if (methodBase.IsStatic)
			{
				return -1;
			}
			return 1;
		}

		public static MethodBase[] Sort(MethodBase[] inf)
		{
			Array.Sort(inf, MethodBaseComparer);
			return inf;
		}

		private static int ComparePropertyInfo(object a, object b)
		{
			PropertyInfo propertyInfo = (PropertyInfo)a;
			PropertyInfo propertyInfo2 = (PropertyInfo)b;
			bool isStatic = (propertyInfo.CanRead ? propertyInfo.GetGetMethod(true) : propertyInfo.GetSetMethod(true)).IsStatic;
			bool isStatic2 = (propertyInfo2.CanRead ? propertyInfo2.GetGetMethod(true) : propertyInfo2.GetSetMethod(true)).IsStatic;
			if (isStatic == isStatic2)
			{
				return CompareMemberInfo(a, b);
			}
			if (isStatic)
			{
				return -1;
			}
			return 1;
		}

		public static PropertyInfo[] Sort(PropertyInfo[] inf)
		{
			Array.Sort(inf, PropertyInfoComparer);
			return inf;
		}

		private static int CompareEventInfo(object a, object b)
		{
			EventInfo obj = (EventInfo)a;
			EventInfo eventInfo = (EventInfo)b;
			bool isStatic = obj.GetAddMethod(true).IsStatic;
			bool isStatic2 = eventInfo.GetAddMethod(true).IsStatic;
			if (isStatic == isStatic2)
			{
				return CompareMemberInfo(a, b);
			}
			if (isStatic)
			{
				return -1;
			}
			return 1;
		}

		public static EventInfo[] Sort(EventInfo[] inf)
		{
			Array.Sort(inf, EventInfoComparer);
			return inf;
		}
	}
}
