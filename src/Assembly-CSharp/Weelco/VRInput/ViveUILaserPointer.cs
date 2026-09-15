using System;
using UnityEngine;

namespace Weelco.VRInput
{
	public class ViveUILaserPointer : IUILaserPointer
	{
		public override Transform target
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public override bool ButtonDown()
		{
			return false;
		}

		public override bool ButtonUp()
		{
			return false;
		}

		public override void OnEnterControl(GameObject control)
		{
			throw new NotImplementedException();
		}

		public override void OnExitControl(GameObject control)
		{
			throw new NotImplementedException();
		}
	}
}
