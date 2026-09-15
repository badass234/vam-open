using System.Collections.Generic;
using UnityEngine;

public class IgnoreChildColliders : MonoBehaviour
{
	public Transform[] additionalIgnores;

	protected List<Collider> allPossibleCollidersList;

	protected List<Collider> myCollidersList;

	protected List<Transform> rigidbodyChildren;

	protected List<Collider> allPossibleChildCollidersList;

	protected List<Collider> childCollidersList;

	protected List<Collider> allPossibleAdditionalCollidersList;

	protected List<Collider> additionalCollidersList;

	private void GetCollidersRecursive(Transform rootTransform, Transform t, List<Collider> colliders)
	{
		if (t != rootTransform && (bool)t.GetComponent<Rigidbody>())
		{
			return;
		}
		Collider[] components = t.GetComponents<Collider>();
		foreach (Collider collider in components)
		{
			if (collider != null)
			{
				colliders.Add(collider);
			}
		}
		foreach (Transform item in t)
		{
			GetCollidersRecursive(rootTransform, item, colliders);
		}
	}

	private void GetRigidbodyChildrenRecursive(Transform rootTransform, Transform t, List<Transform> children)
	{
		if (t != rootTransform && (bool)t.GetComponent<Rigidbody>())
		{
			children.Add(t);
			return;
		}
		foreach (Transform item in t)
		{
			GetRigidbodyChildrenRecursive(rootTransform, item, children);
		}
	}

	public void SyncColliders()
	{
		if (myCollidersList == null)
		{
			myCollidersList = new List<Collider>();
		}
		else
		{
			myCollidersList.Clear();
		}
		if (allPossibleCollidersList == null)
		{
			allPossibleCollidersList = new List<Collider>();
			GetCollidersRecursive(base.transform, base.transform, allPossibleCollidersList);
		}
		foreach (Collider allPossibleColliders in allPossibleCollidersList)
		{
			if (allPossibleColliders != null && allPossibleColliders.gameObject.activeInHierarchy && allPossibleColliders.enabled)
			{
				myCollidersList.Add(allPossibleColliders);
			}
		}
		if (rigidbodyChildren == null)
		{
			rigidbodyChildren = new List<Transform>();
			GetRigidbodyChildrenRecursive(base.transform, base.transform, rigidbodyChildren);
		}
		if (childCollidersList == null)
		{
			childCollidersList = new List<Collider>();
		}
		else
		{
			childCollidersList.Clear();
		}
		if (allPossibleChildCollidersList == null)
		{
			allPossibleChildCollidersList = new List<Collider>();
			foreach (Transform rigidbodyChild in rigidbodyChildren)
			{
				GetCollidersRecursive(rigidbodyChild, rigidbodyChild, allPossibleChildCollidersList);
			}
		}
		foreach (Collider allPossibleChildColliders in allPossibleChildCollidersList)
		{
			if (allPossibleChildColliders != null && allPossibleChildColliders.gameObject.activeInHierarchy && allPossibleChildColliders.enabled)
			{
				childCollidersList.Add(allPossibleChildColliders);
			}
		}
		foreach (Collider myColliders in myCollidersList)
		{
			foreach (Collider childColliders in childCollidersList)
			{
				Physics.IgnoreCollision(myColliders, childColliders);
			}
		}
		if (additionalIgnores == null)
		{
			return;
		}
		if (additionalCollidersList == null)
		{
			additionalCollidersList = new List<Collider>();
		}
		else
		{
			additionalCollidersList.Clear();
		}
		if (allPossibleAdditionalCollidersList == null)
		{
			allPossibleAdditionalCollidersList = new List<Collider>();
			Transform[] array = additionalIgnores;
			foreach (Transform transform in array)
			{
				GetCollidersRecursive(transform, transform, allPossibleAdditionalCollidersList);
			}
		}
		foreach (Collider allPossibleAdditionalColliders in allPossibleAdditionalCollidersList)
		{
			if (allPossibleAdditionalColliders != null && allPossibleAdditionalColliders.gameObject.activeInHierarchy && allPossibleAdditionalColliders.enabled)
			{
				additionalCollidersList.Add(allPossibleAdditionalColliders);
			}
		}
		foreach (Collider myColliders2 in myCollidersList)
		{
			foreach (Collider additionalColliders in additionalCollidersList)
			{
				Physics.IgnoreCollision(myColliders2, additionalColliders);
			}
		}
	}

	private void OnEnable()
	{
		SyncColliders();
	}
}
