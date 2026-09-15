using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Battlehub.RTSaveLoad
{
	public class TypeModelCreator
	{
		public RuntimeTypeModel Create()
		{
			RuntimeTypeModel runtimeTypeModel = TypeModel.Create();
			RegisterTypes(runtimeTypeModel);
			return runtimeTypeModel;
		}

		protected void RegisterTypes(RuntimeTypeModel model)
		{
			Type[] array = (from type in Reflection.GetAllFromCurrentAssembly()
				where type.IsDefined(typeof(ProtoContractAttribute), false)
				select type).ToArray();
			Type[] array2 = array;
			foreach (Type type2 in array2)
			{
				if (!type2.IsGenericType())
				{
					model.Add(type2, true);
					model.Add(typeof(PrimitiveContract<>).MakeGenericType(type2.MakeArrayType()), true);
					model.Add(typeof(List<>).MakeGenericType(type2), true);
				}
			}
			Type[] array3 = new Type[13]
			{
				typeof(bool),
				typeof(char),
				typeof(byte),
				typeof(short),
				typeof(int),
				typeof(long),
				typeof(ushort),
				typeof(uint),
				typeof(ulong),
				typeof(string),
				typeof(float),
				typeof(double),
				typeof(decimal)
			};
			Type[] array4 = array3;
			foreach (Type type3 in array4)
			{
				if (!type3.IsGenericType())
				{
					model.Add(typeof(List<>).MakeGenericType(type3), true);
				}
			}
			Dictionary<Type, ISerializationSurrogate> surrogates = SerializationSurrogates.GetSurrogates();
			foreach (KeyValuePair<Type, ISerializationSurrogate> item in surrogates)
			{
				model.Add(item.Value.GetType(), true);
				model.Add(item.Key, false).SetSurrogate(item.Value.GetType());
				model.Add(typeof(PrimitiveContract<>).MakeGenericType(item.Key.MakeArrayType()), true);
				model.Add(typeof(List<>).MakeGenericType(item.Key), true);
			}
		}
	}
}
