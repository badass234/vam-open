using UnityEngine;

namespace Battlehub.Utils
{
	public class PsAutoDestroy : MonoBehaviour
	{
		private ParticleSystem ps;

		public void Start()
		{
			ps = GetComponent<ParticleSystem>();
		}

		public void Update()
		{
			if ((bool)ps && !ps.IsAlive())
			{
				Object.Destroy(base.gameObject, 0f);
			}
		}
	}
}
