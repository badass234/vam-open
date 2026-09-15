using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace DynamicCSharp
{
	public class ScriptProxy : IDisposable
	{
		private ScriptType scriptType;

		private IMemberProxy fields;

		private IMemberProxy properies;

		private object instance;

		public bool supportCoroutines = true;

		public ScriptType ScriptType
		{
			get
			{
				CheckDisposed();
				return scriptType;
			}
		}

		public IMemberProxy Fields
		{
			get
			{
				CheckDisposed();
				return fields;
			}
		}

		public IMemberProxy Properties
		{
			get
			{
				CheckDisposed();
				return properies;
			}
		}

		public object Instance
		{
			get
			{
				CheckDisposed();
				return instance;
			}
		}

		public UnityEngine.Object UnityInstance
		{
			get
			{
				CheckDisposed();
				return instance as UnityEngine.Object;
			}
		}

		public MonoBehaviour BehaviourInstance
		{
			get
			{
				CheckDisposed();
				return instance as MonoBehaviour;
			}
		}

		public ScriptableObject ScriptableInstance
		{
			get
			{
				CheckDisposed();
				return instance as ScriptableObject;
			}
		}

		public bool IsUnityObject
		{
			get
			{
				CheckDisposed();
				return scriptType.IsUnityObject;
			}
		}

		public bool IsMonoBehaviour
		{
			get
			{
				CheckDisposed();
				return scriptType.IsMonoBehaviour;
			}
		}

		public bool IsScriptableObject
		{
			get
			{
				CheckDisposed();
				return scriptType.IsScriptableObject;
			}
		}

		public bool IsDisposed => instance == null;

		internal ScriptProxy(ScriptType scriptType, object instance)
		{
			this.scriptType = scriptType;
			this.instance = instance;
			fields = new FieldProxy(this);
			properies = new PropertyProxy(this);
		}

		public object Call(string methodName)
		{
			CheckDisposed();
			MethodInfo methodInfo = scriptType.FindCachedMethod(methodName);
			if (methodInfo == null)
			{
				throw new TargetException($"Type '{ScriptType}' does not define a method called '{methodName}'");
			}
			object obj = methodInfo.Invoke(instance, null);
			if (obj is IEnumerator && supportCoroutines)
			{
				IEnumerator routine = obj as IEnumerator;
				if (IsMonoBehaviour)
				{
					MonoBehaviour instanceAs = GetInstanceAs<MonoBehaviour>(false);
					instanceAs.StartCoroutine(routine);
				}
			}
			return obj;
		}

		public object Call(string methodName, params object[] arguments)
		{
			CheckDisposed();
			MethodInfo methodInfo = scriptType.FindCachedMethod(methodName);
			if (methodInfo == null)
			{
				throw new TargetException($"Type '{ScriptType}' does not define a method called '{methodName}'");
			}
			object obj = methodInfo.Invoke(instance, arguments);
			if (obj is IEnumerator && supportCoroutines)
			{
				IEnumerator routine = obj as IEnumerator;
				if (IsMonoBehaviour)
				{
					MonoBehaviour instanceAs = GetInstanceAs<MonoBehaviour>(false);
					instanceAs.StartCoroutine(routine);
				}
			}
			return obj;
		}

		public object SafeCall(string method)
		{
			try
			{
				CheckDisposed();
				return Call(method);
			}
			catch
			{
				return null;
			}
		}

		public object SafeCall(string method, params object[] arguments)
		{
			try
			{
				CheckDisposed();
				return Call(method);
			}
			catch
			{
				return null;
			}
		}

		public Type GetInstanceType()
		{
			CheckDisposed();
			return instance.GetType();
		}

		public T GetInstanceAs<T>(bool throwOnError)
		{
			if (throwOnError)
			{
				return (T)instance;
			}
			try
			{
				return (T)instance;
			}
			catch
			{
				return default(T);
			}
		}

		public virtual void Dispose()
		{
			CheckDisposed();
			if (IsUnityObject)
			{
				UnityEngine.Object.Destroy(UnityInstance);
			}
			scriptType = null;
			instance = null;
		}

		public void MakePersistent()
		{
			if (IsUnityObject)
			{
				UnityEngine.Object.DontDestroyOnLoad(UnityInstance);
			}
		}

		private void CheckDisposed()
		{
			if (instance == null)
			{
				throw new ObjectDisposedException("The script has already been disposed. Unity types can be disposed automatically when the wrapped type is destroyed");
			}
		}
	}
}
