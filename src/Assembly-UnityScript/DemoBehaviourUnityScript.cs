using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DemoBehaviourUnityScript : MonoBehaviour
{
	public List<string> wishlist;

	public List<Vector2> points;

	public DemoBehaviourUnityScript()
	{
		wishlist = new List<string>();
		points = new List<Vector2>();
	}

	public virtual void Main()
	{
	}
}
