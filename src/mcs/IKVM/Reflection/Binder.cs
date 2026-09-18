using System;
using System.Globalization;

namespace IKVM.Reflection
{
	public abstract class Binder
	{
		public virtual MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] names, out object state)
		{
			throw new InvalidOperationException();
		}

		public virtual FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo culture)
		{
			throw new InvalidOperationException();
		}

		public virtual object ChangeType(object value, Type type, CultureInfo culture)
		{
			throw new InvalidOperationException();
		}

		public virtual void ReorderArgumentArray(ref object[] args, object state)
		{
			throw new InvalidOperationException();
		}

		public abstract MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers);

		public abstract PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers);
	}
}
