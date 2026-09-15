using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MeshVR
{
	public class PresetManagerControlOverview : JSONStorable
	{
		public PresetManagerControl[] presetManagerControls;

		protected Dictionary<PresetManagerControl, Toggle> presetManagerControlLockParamsToToggle;

		public void SyncPresetManagerControlLockParams(PresetManagerControl pmc)
		{
			Toggle value;
			if (presetManagerControlLockParamsToToggle.TryGetValue(pmc, out value))
			{
				value.isOn = pmc.lockParams;
			}
		}

		protected override void InitUI(Transform t, bool isAlt)
		{
			base.InitUI(t, isAlt);
			if (!(t != null))
			{
				return;
			}
			PresetManagerControlOverviewUI componentInChildren = t.GetComponentInChildren<PresetManagerControlOverviewUI>(true);
			if (!(componentInChildren != null) || componentInChildren.toggles == null)
			{
				return;
			}
			UIDynamicToggle[] toggles = componentInChildren.toggles;
			int num = presetManagerControls.Length;
			for (int i = 0; i < toggles.Length; i++)
			{
				if (!(toggles[i] != null))
				{
					continue;
				}
				if (i < num)
				{
					PresetManagerControl presetManagerControl = presetManagerControls[i];
					toggles[i].gameObject.SetActive(true);
					toggles[i].label = presetManagerControl.storeId;
					Toggle toggle = toggles[i].toggle;
					presetManagerControlLockParamsToToggle.Add(presetManagerControl, toggle);
					toggle.isOn = presetManagerControl.lockParams;
					toggle.onValueChanged.AddListener(delegate(bool b)
					{
						presetManagerControl.lockParams = b;
					});
				}
				else
				{
					toggles[i].gameObject.SetActive(false);
				}
			}
		}

		protected virtual void Init()
		{
			presetManagerControlLockParamsToToggle = new Dictionary<PresetManagerControl, Toggle>();
		}

		protected override void Awake()
		{
			if (!awakecalled)
			{
				base.Awake();
				Init();
				InitUI();
				InitUIAlt();
			}
		}
	}
}
