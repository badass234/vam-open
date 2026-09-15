using System;

namespace GPUTools.Hair.Scripts.Settings.Abstract
{
	[Serializable]
	public class HairSettingsBase
	{
		public bool IsVisible;

		public virtual bool Validate()
		{
			return true;
		}

		public virtual void DrawGizmos()
		{
		}
	}
}
