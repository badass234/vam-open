using System.Reflection;

namespace DynamicCSharp
{
	public sealed class FieldProxy : IMemberProxy
	{
		private ScriptProxy owner;

		public object this[string name]
		{
			get
			{
				FieldInfo fieldInfo = owner.ScriptType.FindCachedField(name);
				if (fieldInfo == null)
				{
					throw new TargetException($"Type '{owner.ScriptType}' does not define a field called '{name}'");
				}
				return fieldInfo.GetValue(owner.Instance);
			}
			set
			{
				FieldInfo fieldInfo = owner.ScriptType.FindCachedField(name);
				if (fieldInfo == null)
				{
					throw new TargetException($"Type '{owner.ScriptType}' does not define a field called '{name}'");
				}
				fieldInfo.SetValue(owner.Instance, value);
			}
		}

		internal FieldProxy(ScriptProxy owner)
		{
			this.owner = owner;
		}
	}
}
