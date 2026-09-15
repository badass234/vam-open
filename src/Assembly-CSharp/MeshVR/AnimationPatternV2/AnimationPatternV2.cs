using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace MeshVR.AnimationPatternV2
{
	public class AnimationPatternV2 : JSONStorable, AnimationTimelineTriggerHandler, TriggerHandler
	{
		protected List<Track> tracks;

		public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
		{
			return base.GetJSON(includePhysical, includeAppearance, forceStore);
		}

		public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
		{
			base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
		}

		public float GetCurrentTimeCounter()
		{
			return 0f;
		}

		public float GetTotalTime()
		{
			return 0f;
		}

		public void RemoveTrigger(Trigger t)
		{
		}

		public void DuplicateTrigger(Trigger t)
		{
		}

		public RectTransform CreateTriggerActionsUI()
		{
			return null;
		}

		public RectTransform CreateTriggerActionMiniUI()
		{
			return null;
		}

		public RectTransform CreateTriggerActionDiscreteUI()
		{
			return null;
		}

		public RectTransform CreateTriggerActionTransitionUI()
		{
			return null;
		}

		public void RemoveTriggerActionUI(RectTransform rt)
		{
		}

		protected void OnAtomUIDRename(string fromid, string toid)
		{
			foreach (Track track in tracks)
			{
				if (track.freeControllerAtomUID == fromid)
				{
					track.freeControllerAtomUID = toid;
				}
				if (track.receiverAtomSelectionPopup != null)
				{
					track.receiverAtomSelectionPopup.currentValueNoCallback = toid;
				}
			}
		}

		protected void RunAnimation()
		{
		}

		protected void Init()
		{
			if (SuperController.singleton != null)
			{
				SuperController singleton = SuperController.singleton;
				singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Combine(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomUIDRename));
			}
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

		private void OnDestroy()
		{
			if (SuperController.singleton != null)
			{
				SuperController singleton = SuperController.singleton;
				singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Remove(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomUIDRename));
			}
		}

		private void Update()
		{
			if (SuperController.singleton == null || !SuperController.singleton.freezeAnimation)
			{
				RunAnimation();
			}
		}
	}
}
