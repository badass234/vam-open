public class ScaleChangeReceiverJSONStorable : JSONStorable
{
	protected float _scale = 1f;

	public float scale => _scale;

	public virtual void ScaleChanged(float scale)
	{
		_scale = scale;
	}
}
