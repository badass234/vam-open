using UnityEngine;

namespace MeshVR
{
	public class DAZDynamicDeleter : JSONStorable
	{
		protected JSONStorableAction deleteConfirmAction;

		protected JSONStorableAction cancelAction;

		protected JSONStorableAction deleteAction;

		protected DAZDynamic dd;

		protected void DeleteConfirm()
		{
			if (cancelAction != null && cancelAction.button != null)
			{
				cancelAction.button.gameObject.SetActive(false);
			}
			if (deleteConfirmAction != null && deleteConfirmAction.button != null)
			{
				deleteConfirmAction.button.gameObject.SetActive(false);
			}
			if (!(dd != null))
			{
				return;
			}
			dd.Delete();
			DAZClothingItemControl component = GetComponent<DAZClothingItemControl>();
			if (component != null)
			{
				component.Delete();
				component.RefreshClothingItems();
			}
			else
			{
				DAZHairGroupControl component2 = GetComponent<DAZHairGroupControl>();
				if (component2 != null)
				{
					component2.Delete();
					component2.RefreshHairItems();
				}
			}
			if (SuperController.singleton != null)
			{
				SuperController.singleton.SetActiveUI("SelectedOptions");
			}
		}

		protected void Cancel()
		{
			if (cancelAction != null && cancelAction.button != null)
			{
				cancelAction.button.gameObject.SetActive(false);
			}
			if (deleteConfirmAction != null && deleteConfirmAction.button != null)
			{
				deleteConfirmAction.button.gameObject.SetActive(false);
			}
		}

		protected void Delete()
		{
			if (cancelAction != null && cancelAction.button != null)
			{
				cancelAction.button.gameObject.SetActive(true);
			}
			if (deleteConfirmAction != null && deleteConfirmAction.button != null)
			{
				deleteConfirmAction.button.gameObject.SetActive(true);
			}
		}

		protected override void InitUI(Transform t, bool isAlt)
		{
			base.InitUI(t, isAlt);
			if (!(t != null))
			{
				return;
			}
			DAZDynamicDeleterUI componentInChildren = t.GetComponentInChildren<DAZDynamicDeleterUI>();
			if (!(dd != null) || !(componentInChildren != null))
			{
				return;
			}
			if (dd.IsInPackage())
			{
				if (componentInChildren.deleteButton != null)
				{
					componentInChildren.deleteButton.interactable = false;
				}
				return;
			}
			if (componentInChildren.deleteButton != null)
			{
				componentInChildren.deleteButton.interactable = true;
			}
			deleteAction.RegisterButton(componentInChildren.deleteButton, isAlt);
			cancelAction.RegisterButton(componentInChildren.cancelButton, isAlt);
			deleteConfirmAction.RegisterButton(componentInChildren.confirmButton, isAlt);
		}

		protected virtual void Init()
		{
			dd = GetComponent<DAZDynamic>();
			if (dd != null && !dd.IsInPackage())
			{
				deleteAction = new JSONStorableAction("Delete", Delete);
				RegisterAction(deleteAction);
				cancelAction = new JSONStorableAction("Cancel", Cancel);
				RegisterAction(cancelAction);
				deleteConfirmAction = new JSONStorableAction("DeleteConfirm", DeleteConfirm);
				RegisterAction(deleteConfirmAction);
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
