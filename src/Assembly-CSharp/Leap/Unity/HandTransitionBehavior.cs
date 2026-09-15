using UnityEngine;

namespace Leap.Unity
{
	public abstract class HandTransitionBehavior : MonoBehaviour
	{
		protected HandModelBase handModelBase;

		protected abstract void HandReset();

		protected abstract void HandFinish();

		protected virtual void Awake()
		{
			handModelBase = GetComponent<HandModelBase>();
			if (handModelBase == null)
			{
				Debug.LogWarning("HandTransitionBehavior components require a HandModelBase component attached to the same GameObject. (Awake)");
				return;
			}
			handModelBase.OnBegin -= HandReset;
			handModelBase.OnBegin += HandReset;
			handModelBase.OnFinish -= HandFinish;
			handModelBase.OnFinish += HandFinish;
		}

		protected virtual void OnDestroy()
		{
			if (handModelBase == null)
			{
				HandModelBase component = GetComponent<HandModelBase>();
				if (component == null)
				{
					Debug.LogWarning("HandTransitionBehavior components require a HandModelBase component attached to the same GameObject. (OnDestroy)");
					return;
				}
			}
			handModelBase.OnBegin -= HandReset;
			handModelBase.OnFinish -= HandFinish;
		}
	}
}
