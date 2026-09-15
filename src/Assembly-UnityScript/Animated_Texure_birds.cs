using System;
using UnityEngine;

[Serializable]
public class Animated_Texure_birds : MonoBehaviour
{
	public int uvAnimationTileX;

	public int uvAnimationTileY;

	public float framesPerSecond;

	public Animated_Texure_birds()
	{
		uvAnimationTileX = 6;
		uvAnimationTileY = 1;
		framesPerSecond = 1f;
	}

	public virtual void Update()
	{
		int num = (int)(Time.time * framesPerSecond);
		num %= uvAnimationTileX * uvAnimationTileY;
		Vector2 value = new Vector2(1f / (float)uvAnimationTileX, 1f / (float)uvAnimationTileY);
		int num2 = num % uvAnimationTileX;
		int num3 = num / uvAnimationTileX;
		Vector2 value2 = new Vector2((float)num2 * value.x, 1f - value.y - (float)num3 * value.y);
		GetComponent<Renderer>().material.SetTextureOffset("_MainTex", value2);
		GetComponent<Renderer>().material.SetTextureScale("_MainTex", value);
	}

	public virtual void Main()
	{
	}
}
