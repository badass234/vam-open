using UnityEngine;

public class ScaleChangeReceiver : MonoBehaviour
{
	protected float _scale = 1f;

	public float scale => _scale;

	public virtual void ScaleChanged(float scale)
	{
		_scale = scale;
	}
}
