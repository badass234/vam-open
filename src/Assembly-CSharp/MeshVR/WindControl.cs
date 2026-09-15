using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeshVR
{
	public class WindControl : JSONStorable
	{
		public static Vector3 globalWind = Vector3.zero;

		public bool isGlobal;

		protected JSONStorableBool isGlobalJSON;

		protected Atom receivingAtom;

		protected JSONStorableStringChooser atomJSON;

		protected string _missingReceiverStoreId = string.Empty;

		protected JSONStorable receiver;

		protected JSONStorableStringChooser receiverJSON;

		protected string _receiverTargetName;

		protected JSONStorableVector3 receiverTarget;

		protected JSONStorableStringChooser receiverTargetJSON;

		protected JSONStorableFloat currentMagnitudeJSON;

		protected JSONStorableBool autoJSON;

		protected JSONStorableFloat periodJSON;

		protected JSONStorableFloat quicknessJSON;

		protected JSONStorableFloat targetMagnitudeJSON;

		protected JSONStorableFloat lowerMagnitudeJSON;

		protected JSONStorableFloat upperMagnitudeJSON;

		protected float timer;

		protected void SyncIsGlobal(bool b)
		{
			isGlobal = b;
		}

		protected void SyncAtomChocies()
		{
			List<string> list = new List<string>();
			list.Add("None");
			foreach (string visibleAtomUID in SuperController.singleton.GetVisibleAtomUIDs())
			{
				list.Add(visibleAtomUID);
			}
			atomJSON.choices = list;
		}

		protected void OnAtomRename(string oldid, string newid)
		{
			SyncAtomChocies();
			if (atomJSON != null && receivingAtom != null)
			{
				atomJSON.valNoCallback = receivingAtom.uid;
			}
		}

		protected void SyncAtom(string atomUID)
		{
			List<string> list = new List<string>();
			list.Add("None");
			if (atomUID != null)
			{
				receivingAtom = SuperController.singleton.GetAtomByUid(atomUID);
				if (receivingAtom != null)
				{
					foreach (string storableID in receivingAtom.GetStorableIDs())
					{
						list.Add(storableID);
					}
				}
			}
			else
			{
				receivingAtom = null;
			}
			receiverJSON.choices = list;
			receiverJSON.val = "None";
		}

		protected void CheckMissingReceiver()
		{
			if (_missingReceiverStoreId != string.Empty && receivingAtom != null)
			{
				JSONStorable storableByID = receivingAtom.GetStorableByID(_missingReceiverStoreId);
				if (storableByID != null)
				{
					string receiverTargetName = _receiverTargetName;
					SyncReceiver(_missingReceiverStoreId);
					_missingReceiverStoreId = string.Empty;
					insideRestore = true;
					receiverTargetJSON.val = receiverTargetName;
					insideRestore = false;
				}
			}
		}

		protected void SyncReceiver(string receiverID)
		{
			List<string> list = new List<string>();
			list.Add("None");
			if (receivingAtom != null && receiverID != null)
			{
				receiver = receivingAtom.GetStorableByID(receiverID);
				if (receiver != null)
				{
					foreach (string vector3ParamName in receiver.GetVector3ParamNames())
					{
						list.Add(vector3ParamName);
					}
				}
				else if (receiverID != "None")
				{
					_missingReceiverStoreId = receiverID;
				}
			}
			else
			{
				receiver = null;
			}
			receiverTargetJSON.choices = list;
			receiverTargetJSON.val = "None";
		}

		protected void SyncReceiverTarget(string receiverTargetName)
		{
			_receiverTargetName = receiverTargetName;
			receiverTarget = null;
			if (receiver != null && receiverTargetName != null)
			{
				receiverTarget = receiver.GetVector3JSONParam(receiverTargetName);
			}
		}

		protected void SyncMagnitude(Vector3 v)
		{
		}

		protected override void InitUI(Transform t, bool isAlt)
		{
			base.InitUI(t, isAlt);
			if (t != null)
			{
				WindControlUI componentInChildren = t.GetComponentInChildren<WindControlUI>(true);
				if (componentInChildren != null)
				{
					isGlobalJSON.RegisterToggle(componentInChildren.isGlobalToggle, isAlt);
					atomJSON.RegisterPopup(componentInChildren.atomPopup, isAlt);
					receiverJSON.RegisterPopup(componentInChildren.receiverPopup, isAlt);
					receiverTargetJSON.RegisterPopup(componentInChildren.receiverTargetPopup, isAlt);
					currentMagnitudeJSON.RegisterSlider(componentInChildren.currentMagnitudeSlider);
					autoJSON.RegisterToggle(componentInChildren.autoToggle, isAlt);
					periodJSON.RegisterSlider(componentInChildren.periodSlider, isAlt);
					quicknessJSON.RegisterSlider(componentInChildren.quicknessSlider, isAlt);
					lowerMagnitudeJSON.RegisterSlider(componentInChildren.lowerMagnitudeSlider, isAlt);
					upperMagnitudeJSON.RegisterSlider(componentInChildren.upperMagnitudeSlider, isAlt);
					targetMagnitudeJSON.RegisterSlider(componentInChildren.targetMagnitudeSlider, isAlt);
				}
			}
		}

		protected void Init()
		{
			if ((bool)SuperController.singleton)
			{
				SuperController singleton = SuperController.singleton;
				singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Combine(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRename));
			}
			isGlobalJSON = new JSONStorableBool("global", false, SyncIsGlobal);
			RegisterBool(isGlobalJSON);
			atomJSON = new JSONStorableStringChooser("atom", SuperController.singleton.GetAtomUIDs(), null, "Atom", SyncAtom);
			atomJSON.representsAtomUid = true;
			RegisterStringChooser(atomJSON);
			atomJSON.popupOpenCallback = SyncAtomChocies;
			receiverJSON = new JSONStorableStringChooser("receiver", null, null, "Receiver", SyncReceiver);
			RegisterStringChooser(receiverJSON);
			receiverTargetJSON = new JSONStorableStringChooser("receiverTarget", null, null, "Target", SyncReceiverTarget);
			RegisterStringChooser(receiverTargetJSON);
			currentMagnitudeJSON = new JSONStorableFloat("currentMagnitude", 0f, -50f, 50f, false);
			RegisterFloat(currentMagnitudeJSON);
			autoJSON = new JSONStorableBool("auto", false);
			RegisterBool(autoJSON);
			periodJSON = new JSONStorableFloat("period", 0.5f, 0f, 10f, false);
			RegisterFloat(periodJSON);
			quicknessJSON = new JSONStorableFloat("quickness", 10f, 0f, 100f);
			RegisterFloat(quicknessJSON);
			lowerMagnitudeJSON = new JSONStorableFloat("lowerMagnitude", 0f, -50f, 50f, false);
			RegisterFloat(lowerMagnitudeJSON);
			upperMagnitudeJSON = new JSONStorableFloat("upperMagnitude", 0f, -50f, 50f, false);
			RegisterFloat(upperMagnitudeJSON);
			targetMagnitudeJSON = new JSONStorableFloat("targetMagnitude", 0f, -50f, 50f, false, false);
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

		private void Update()
		{
			if (currentMagnitudeJSON == null)
			{
				return;
			}
			if (autoJSON != null && autoJSON.val)
			{
				timer -= Time.deltaTime;
				if (timer < 0f)
				{
					timer = periodJSON.val;
					targetMagnitudeJSON.val = UnityEngine.Random.Range(lowerMagnitudeJSON.val, upperMagnitudeJSON.val);
				}
				currentMagnitudeJSON.val = Mathf.Lerp(currentMagnitudeJSON.val, targetMagnitudeJSON.val, Time.deltaTime * quicknessJSON.val);
			}
			Vector3 vector = currentMagnitudeJSON.val * base.transform.forward;
			if (isGlobal)
			{
				globalWind = vector;
				return;
			}
			CheckMissingReceiver();
			if (receiverTarget != null)
			{
				receiverTarget.val = currentMagnitudeJSON.val * base.transform.forward;
			}
		}

		protected void OnDestroy()
		{
			if ((bool)SuperController.singleton)
			{
				SuperController singleton = SuperController.singleton;
				singleton.onAtomUIDRenameHandlers = (SuperController.OnAtomUIDRename)Delegate.Remove(singleton.onAtomUIDRenameHandlers, new SuperController.OnAtomUIDRename(OnAtomRename));
			}
		}
	}
}
