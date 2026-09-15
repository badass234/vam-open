using UnityEngine;

namespace Assets.OVR.Scripts
{
	public class FixRecord : Record
	{
		public FixMethodDelegate fixMethod;

		public Object targetObject;

		public string[] buttonNames;

		public bool complete;

		public FixRecord(string cat, string msg, FixMethodDelegate fix, Object target, string[] buttons)
			: base(cat, msg)
		{
			buttonNames = buttons;
			fixMethod = fix;
			targetObject = target;
			complete = false;
		}
	}
}
