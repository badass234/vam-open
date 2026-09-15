using System;

namespace DynamicCSharp
{
	public class Variable
	{
		protected string name = string.Empty;

		protected object data;

		public string Name => name;

		public object Value => data;

		internal Variable(string name, object data)
		{
			this.name = name;
			this.data = data;
		}

		internal void Update(object data)
		{
			this.data = data;
		}

		public override string ToString()
		{
			return (data != null) ? data.ToString() : "null";
		}
	}
	public class Variable<T> : Variable
	{
		public new T Value
		{
			get
			{
				try
				{
					return (T)data;
				}
				catch (InvalidCastException)
				{
					return default(T);
				}
			}
		}

		internal Variable(string name, T data)
			: base(name, data)
		{
		}

		public static implicit operator T(Variable<T> var)
		{
			return var.Value;
		}
	}
}
