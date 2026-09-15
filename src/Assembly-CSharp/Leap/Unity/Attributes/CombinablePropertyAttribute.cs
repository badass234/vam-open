using System.Reflection;
using UnityEngine;

namespace Leap.Unity.Attributes
{
	public abstract class CombinablePropertyAttribute : PropertyAttribute
	{
		private bool _isInitialized;

		private FieldInfo _fieldInfo;

		private Object[] _targets;

		public FieldInfo fieldInfo
		{
			get
			{
				if (!_isInitialized)
				{
					Debug.LogError("CombinablePropertyAttribute needed fieldInfo but was not initialized. Did you call Init()?");
				}
				return _fieldInfo;
			}
			protected set
			{
				_fieldInfo = value;
			}
		}

		public Object[] targets
		{
			get
			{
				if (!_isInitialized)
				{
					Debug.LogError("CombinablePropertyAttribute needed fieldInfo but was not initialized. Did you call Init()?");
				}
				return _targets;
			}
			protected set
			{
				_targets = value;
			}
		}
	}
}
