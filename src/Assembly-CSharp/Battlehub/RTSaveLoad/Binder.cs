using System;
using System.Runtime.Serialization;

namespace Battlehub.RTSaveLoad
{
	public class Binder : SerializationBinder
	{
		public override Type BindToType(string assemblyName, string typeName)
		{
			return Type.GetType($"{typeName}, {assemblyName}");
		}
	}
}
