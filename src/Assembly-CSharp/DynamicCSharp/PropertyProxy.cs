using System.Reflection;

namespace DynamicCSharp
{
	public sealed class PropertyProxy : IMemberProxy
	{
		private ScriptProxy owner;

		public object this[string name]
		{
			get
			{
				PropertyInfo propertyInfo = owner.ScriptType.FindCachedProperty(name);
				if (propertyInfo == null)
				{
					throw new TargetException($"Type '{owner.ScriptType}' does not define a property called '{name}'");
				}
				if (!propertyInfo.CanRead)
				{
					throw new TargetException($"The property '{name}' was found but it does not define a get accessor");
				}
				return propertyInfo.GetValue(owner.Instance, null);
			}
			set
			{
				PropertyInfo propertyInfo = owner.ScriptType.FindCachedProperty(name);
				if (propertyInfo == null)
				{
					throw new TargetException($"Type '{owner.ScriptType}' does not define a property called '{name}'");
				}
				if (!propertyInfo.CanWrite)
				{
					throw new TargetException($"The property '{name}' was found but it does not define a set accessor");
				}
				propertyInfo.SetValue(owner.Instance, value, null);
			}
		}

		internal PropertyProxy(ScriptProxy owner)
		{
			this.owner = owner;
		}
	}
}
