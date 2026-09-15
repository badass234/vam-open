using System;
using UnityEngine;

[Serializable]
public class ChainRelease : MonoBehaviour
{
	public string Keystroke;

	public ChainRelease()
	{
		Keystroke = string.Empty;
	}

	public virtual void Start()
	{
	}

	public virtual void Update()
	{
		if (Input.GetKeyUp(Keystroke))
		{
			if ((bool)gameObject.GetComponent<HingeJoint>())
			{
				UnityEngine.Object.Destroy(gameObject.GetComponent<HingeJoint>());
			}
			if ((bool)(CharacterJoint)gameObject.GetComponent(typeof(CharacterJoint)))
			{
				UnityEngine.Object.Destroy((CharacterJoint)gameObject.GetComponent(typeof(CharacterJoint)));
			}
		}
	}

	public virtual void Main()
	{
	}
}
