using System;
using System.Collections.Generic;
using LeapInternal;

namespace Leap
{
	public class Config
	{
		public enum ValueType
		{
			TYPE_UNKNOWN = 0,
			TYPE_BOOLEAN = 1,
			TYPE_INT32 = 2,
			TYPE_FLOAT = 6,
			TYPE_STRING = 8
		}

		private Connection _connection;

		private Dictionary<uint, object> _transactions = new Dictionary<uint, object>();

		public Config(int connectionKey)
		{
			_connection = Connection.GetConnection(connectionKey);
			Connection connection = _connection;
			connection.LeapConfigChange = (EventHandler<ConfigChangeEventArgs>)Delegate.Combine(connection.LeapConfigChange, new EventHandler<ConfigChangeEventArgs>(handleConfigChange));
			Connection connection2 = _connection;
			connection2.LeapConfigResponse = (EventHandler<SetConfigResponseEventArgs>)Delegate.Combine(connection2.LeapConfigResponse, new EventHandler<SetConfigResponseEventArgs>(handleConfigResponse));
		}

		private void handleConfigChange(object sender, ConfigChangeEventArgs eventArgs)
		{
			object value;
			if (_transactions.TryGetValue(eventArgs.RequestId, out value))
			{
				Action<bool> action = value as Action<bool>;
				action(eventArgs.Succeeded);
				_transactions.Remove(eventArgs.RequestId);
			}
		}

		private void handleConfigResponse(object sender, SetConfigResponseEventArgs eventArgs)
		{
			object value = new object();
			if (_transactions.TryGetValue(eventArgs.RequestId, out value))
			{
				switch (eventArgs.DataType)
				{
				case ValueType.TYPE_BOOLEAN:
				{
					Action<bool> action4 = value as Action<bool>;
					action4((int)eventArgs.Value != 0);
					break;
				}
				case ValueType.TYPE_FLOAT:
				{
					Action<float> action3 = value as Action<float>;
					action3((float)eventArgs.Value);
					break;
				}
				case ValueType.TYPE_INT32:
				{
					Action<int> action2 = value as Action<int>;
					action2((int)eventArgs.Value);
					break;
				}
				case ValueType.TYPE_STRING:
				{
					Action<string> action = value as Action<string>;
					action((string)eventArgs.Value);
					break;
				}
				}
				_transactions.Remove(eventArgs.RequestId);
			}
		}

		public bool Get<T>(string key, Action<T> onResult)
		{
			uint configValue = _connection.GetConfigValue(key);
			if (configValue != 0)
			{
				_transactions.Add(configValue, onResult);
				return true;
			}
			return false;
		}

		public bool Set<T>(string key, T value, Action<bool> onResult) where T : IConvertible
		{
			uint num = _connection.SetConfigValue(key, value);
			if (num != 0)
			{
				_transactions.Add(num, onResult);
				return true;
			}
			return false;
		}

		[Obsolete("Use the generic Set<T> method instead.")]
		public ValueType Type(string key)
		{
			return ValueType.TYPE_UNKNOWN;
		}

		[Obsolete("Use the generic Get<T> method instead.")]
		public bool GetBool(string key)
		{
			return false;
		}

		[Obsolete("Use the generic Set<T> method instead.")]
		public bool SetBool(string key, bool value)
		{
			return false;
		}

		[Obsolete("Use the generic Get<T> method instead.")]
		public bool GetInt32(string key)
		{
			return false;
		}

		[Obsolete("Use the generic Set<T> method instead.")]
		public bool SetInt32(string key, int value)
		{
			return false;
		}

		[Obsolete("Use the generic Get<T> method instead.")]
		public bool GetFloat(string key)
		{
			return false;
		}

		[Obsolete("Use the generic Set<T> method instead.")]
		public bool SetFloat(string key, float value)
		{
			return false;
		}

		[Obsolete("Use the generic Get<T> method instead.")]
		public bool GetString(string key)
		{
			return false;
		}

		[Obsolete("Use the generic Set<T> method instead.")]
		public bool SetString(string key, string value)
		{
			return false;
		}

		[Obsolete]
		public bool Save()
		{
			return false;
		}
	}
}
