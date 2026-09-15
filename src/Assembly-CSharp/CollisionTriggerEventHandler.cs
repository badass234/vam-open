using System.Collections.Generic;
using UnityEngine;

public class CollisionTriggerEventHandler : MonoBehaviour
{
	public delegate void RelativeVelocityCallback(float f);

	public CollisionTrigger collisionTrigger;

	public Collider thisCollider;

	public Rigidbody thisRigidbody;

	public Dictionary<Collider, bool> collidingWithButFailedVelocityTestDictionary;

	public Dictionary<Collider, bool> collidingWithDictionary;

	public List<Collider> collidingWith;

	public string atomFilterUID;

	public bool invertAtomFilter;

	public bool useRelativeVelocityFilter;

	public bool invertRelativeVelocityFilter;

	public float relativeVelocityFilter = 1f;

	public float lastRelativeVelocity;

	public RelativeVelocityCallback relativeVelocityHandlers;

	public bool debug;

	protected List<Collider> removeList;

	protected bool PassVelocityFilter(Collision c)
	{
		if (useRelativeVelocityFilter)
		{
			lastRelativeVelocity = c.relativeVelocity.magnitude;
			if (relativeVelocityHandlers != null)
			{
				relativeVelocityHandlers(lastRelativeVelocity);
			}
			if (invertRelativeVelocityFilter)
			{
				return lastRelativeVelocity < relativeVelocityFilter;
			}
			return lastRelativeVelocity >= relativeVelocityFilter;
		}
		return true;
	}

	protected bool PassVelocityFilter(Collider c)
	{
		if (useRelativeVelocityFilter && thisRigidbody != null)
		{
			lastRelativeVelocity = (c.attachedRigidbody.velocity - thisRigidbody.velocity).magnitude;
			if (relativeVelocityHandlers != null)
			{
				relativeVelocityHandlers(lastRelativeVelocity);
			}
			if (invertRelativeVelocityFilter)
			{
				return lastRelativeVelocity < relativeVelocityFilter;
			}
			return lastRelativeVelocity >= relativeVelocityFilter;
		}
		return true;
	}

	protected bool PassAtomFilter(Collider c)
	{
		if (atomFilterUID != null && atomFilterUID != "None")
		{
			ForceReceiver componentInParent = c.GetComponentInParent<ForceReceiver>();
			if (componentInParent != null)
			{
				Atom containingAtom = componentInParent.containingAtom;
				if (invertAtomFilter)
				{
					if (containingAtom == null || containingAtom.uid != atomFilterUID)
					{
						return true;
					}
				}
				else if (containingAtom != null && containingAtom.uid == atomFilterUID)
				{
					return true;
				}
				return false;
			}
			return false;
		}
		return true;
	}

	protected bool IsCollidingWith(Collider c)
	{
		if (collidingWithDictionary != null && collidingWithDictionary.ContainsKey(c))
		{
			return true;
		}
		return false;
	}

	protected bool IsCollidingWithButFailedVelocityCheck(Collider c)
	{
		if (collidingWithButFailedVelocityTestDictionary != null && collidingWithButFailedVelocityTestDictionary.ContainsKey(c))
		{
			return true;
		}
		return false;
	}

	protected void AddCollidingWith(Collider c)
	{
		if (collidingWithDictionary != null && !collidingWithDictionary.ContainsKey(c))
		{
			collidingWithDictionary.Add(c, true);
		}
		if (collisionTrigger != null)
		{
			collisionTrigger.trigger.active = true;
			collisionTrigger.trigger.transitionInterpValue = 1f;
		}
	}

	protected void AddCollidingWithButFailedVelocityTest(Collider c)
	{
		if (collidingWithButFailedVelocityTestDictionary != null && !collidingWithButFailedVelocityTestDictionary.ContainsKey(c))
		{
			collidingWithButFailedVelocityTestDictionary.Add(c, true);
		}
	}

	protected void RemoveCollidingWith(Collider c)
	{
		if (collidingWithDictionary != null)
		{
			collidingWithDictionary.Remove(c);
			if (collidingWithDictionary.Count == 0 && collisionTrigger != null)
			{
				collisionTrigger.trigger.transitionInterpValue = 0f;
				collisionTrigger.trigger.active = false;
			}
		}
		if (collidingWithButFailedVelocityTestDictionary != null)
		{
			collidingWithButFailedVelocityTestDictionary.Remove(c);
		}
	}

	protected void RemoveAllCollidingWith()
	{
		if (collidingWithButFailedVelocityTestDictionary != null)
		{
			collidingWithButFailedVelocityTestDictionary.Clear();
		}
		if (collidingWithDictionary != null)
		{
			collidingWithDictionary.Clear();
		}
		if (collisionTrigger != null)
		{
			collisionTrigger.trigger.transitionInterpValue = 0f;
			collisionTrigger.trigger.active = false;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (PassAtomFilter(collision.collider) && (SuperController.singleton == null || !SuperController.singleton.isLoading))
		{
			if (PassVelocityFilter(collision))
			{
				AddCollidingWith(collision.collider);
			}
			else
			{
				AddCollidingWithButFailedVelocityTest(collision.collider);
			}
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (PassAtomFilter(collision.collider) && !IsCollidingWith(collision.collider) && !IsCollidingWithButFailedVelocityCheck(collision.collider) && (SuperController.singleton == null || !SuperController.singleton.isLoading))
		{
			if (PassVelocityFilter(collision))
			{
				AddCollidingWith(collision.collider);
			}
			else
			{
				AddCollidingWithButFailedVelocityTest(collision.collider);
			}
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (SuperController.singleton == null || !SuperController.singleton.isLoading)
		{
			RemoveCollidingWith(collision.collider);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (PassAtomFilter(other) && (SuperController.singleton == null || !SuperController.singleton.isLoading))
		{
			if (PassVelocityFilter(other))
			{
				AddCollidingWith(other);
			}
			else
			{
				AddCollidingWithButFailedVelocityTest(other);
			}
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (PassAtomFilter(other) && !IsCollidingWith(other) && !IsCollidingWithButFailedVelocityCheck(other) && (SuperController.singleton == null || !SuperController.singleton.isLoading))
		{
			if (PassVelocityFilter(other))
			{
				AddCollidingWith(other);
			}
			else
			{
				AddCollidingWithButFailedVelocityTest(other);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (SuperController.singleton == null || !SuperController.singleton.isLoading)
		{
			RemoveCollidingWith(other);
		}
	}

	public void Reset()
	{
		collisionTrigger.trigger.transitionInterpValue = 0f;
		collisionTrigger.trigger.active = false;
		thisCollider = GetComponent<Collider>();
		if (thisCollider != null)
		{
			thisRigidbody = thisCollider.attachedRigidbody;
		}
		if (collidingWithDictionary == null)
		{
			collidingWithDictionary = new Dictionary<Collider, bool>();
		}
		else
		{
			collidingWithDictionary.Clear();
		}
		if (collidingWithButFailedVelocityTestDictionary == null)
		{
			collidingWithButFailedVelocityTestDictionary = new Dictionary<Collider, bool>();
		}
		else
		{
			collidingWithButFailedVelocityTestDictionary.Clear();
		}
	}

	private void OnDisable()
	{
		RemoveAllCollidingWith();
	}

	private void FixedUpdate()
	{
		if (removeList == null)
		{
			removeList = new List<Collider>();
		}
		else
		{
			removeList.Clear();
		}
		if (collidingWithDictionary.Count > 0)
		{
			foreach (Collider key in collidingWithDictionary.Keys)
			{
				if (key == null)
				{
					removeList.Add(key);
				}
				else if (!key.gameObject.activeInHierarchy)
				{
					removeList.Add(key);
				}
			}
		}
		foreach (Collider remove in removeList)
		{
			RemoveCollidingWith(remove);
		}
		if (debug)
		{
			collidingWith = new List<Collider>(collidingWithDictionary.Keys);
		}
	}

	protected void OnDestroy()
	{
		RemoveAllCollidingWith();
	}
}
