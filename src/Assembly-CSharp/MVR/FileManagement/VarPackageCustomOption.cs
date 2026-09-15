using System;
using SimpleJSON;

namespace MVR.FileManagement
{
	[Serializable]
	public class VarPackageCustomOption
	{
		public string name;

		public string displayName;

		public bool defaultValue;

		protected JSONStorableBool boolJSON;

		public bool Value
		{
			get
			{
				if (boolJSON != null)
				{
					return boolJSON.val;
				}
				return false;
			}
			set
			{
				if (boolJSON != null)
				{
					boolJSON.val = value;
				}
			}
		}

		public bool ValueNoCallback
		{
			get
			{
				if (boolJSON != null)
				{
					return boolJSON.val;
				}
				return false;
			}
			set
			{
				if (boolJSON != null)
				{
					boolJSON.valNoCallback = value;
				}
			}
		}

		public void Init(JSONStorableBool.SetJSONBoolCallback callback)
		{
			boolJSON = new JSONStorableBool(name, defaultValue, callback);
		}

		public void SetToggle(UIDynamicToggle toggle)
		{
			boolJSON.toggle = toggle.toggle;
			toggle.label = displayName;
		}

		public void StoreJSON(JSONClass jc)
		{
			boolJSON.StoreJSON(jc, true, true, true);
		}

		public void RestoreFromJSON(JSONClass jc)
		{
			boolJSON.RestoreFromJSON(jc);
		}
	}
}
