using UnityEngine;

namespace Weelco.VRInput
{
	public abstract class IUIPointer
	{
		private float _distanceLimit;

		public abstract Transform target { get; }

		public virtual void Update()
		{
			Ray ray = new Ray(target.transform.position, target.transform.forward);
			RaycastHit hitInfo;
			bool flag = Physics.Raycast(ray, out hitInfo);
			float num = 100f;
			if (flag)
			{
				num = hitInfo.distance;
			}
			if (_distanceLimit > 0f)
			{
				num = Mathf.Min(num, _distanceLimit);
				flag = true;
			}
			UpdateRaycasting(flag, num);
			_distanceLimit = -1f;
		}

		public void LimitLaserDistance(float distance)
		{
			if (!(distance < 0f))
			{
				if (_distanceLimit < 0f)
				{
					_distanceLimit = distance;
				}
				else
				{
					_distanceLimit = Mathf.Min(_distanceLimit, distance);
				}
			}
		}

		public abstract void Initialize();

		public abstract void OnEnterControl(GameObject control);

		public abstract void OnExitControl(GameObject control);

		protected abstract void UpdateRaycasting(bool isHit, float distance);
	}
}
