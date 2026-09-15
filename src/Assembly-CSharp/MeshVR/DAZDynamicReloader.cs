using System.Collections;
using UnityEngine;

namespace MeshVR
{
	public class DAZDynamicReloader : JSONStorable
	{
		protected JSONStorableAction reloadAction;

		public DAZImport dazImportForReload;

		protected DAZDynamic dd;

		private IEnumerator LoadDelay()
		{
			yield return null;
			dd.Load();
			if (dazImportForReload != null && dazImportForReload.materialUIConnectorMaster != null)
			{
				dazImportForReload.materialUIConnectorMaster.Rebuild();
			}
			JSONStorableDynamic jsd = GetComponentInParent<JSONStorableDynamic>();
			if (jsd != null)
			{
				jsd.enabled = true;
			}
		}

		protected void Reload()
		{
			if (dd != null)
			{
				JSONStorableDynamic componentInParent = GetComponentInParent<JSONStorableDynamic>();
				if (componentInParent != null)
				{
					componentInParent.enabled = false;
				}
				dd.Clear();
				StartCoroutine(LoadDelay());
			}
		}

		protected override void InitUI(Transform t, bool isAlt)
		{
			base.InitUI(t, isAlt);
			if (t != null)
			{
				DAZDynamicReloaderUI componentInChildren = t.GetComponentInChildren<DAZDynamicReloaderUI>(true);
				if (dd != null && componentInChildren != null)
				{
					reloadAction.RegisterButton(componentInChildren.reloadButton, isAlt);
				}
			}
		}

		protected virtual void Init()
		{
			if (dazImportForReload == null)
			{
				dazImportForReload = GetComponent<DAZImport>();
			}
			dd = GetComponent<DAZDynamic>();
			if (dd != null)
			{
				reloadAction = new JSONStorableAction("Reload", Reload);
				RegisterAction(reloadAction);
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
	}
}
