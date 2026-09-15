using UnityEngine;

public class SlowmoToggler : MonoBehaviour
{
	public void Slowmo(bool slowmo)
	{
		Time.timeScale = ((!slowmo) ? 1f : 0.25f);
	}
}
