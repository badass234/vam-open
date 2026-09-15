using System.Collections;
using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser
{
	public class ForcedCooperation : MonoBehaviour
	{
		public Transform whoWillComply;

		public float howLongWillTheyComply;

		public void Comply()
		{
			StartCoroutine(_Comply());
		}

		protected IEnumerator _Comply()
		{
			float t0 = Time.time;
			do
			{
				Vector3 pos = base.transform.InverseTransformPoint(whoWillComply.position);
				if (pos.z > 0f)
				{
					pos.z = 0f;
					whoWillComply.position = base.transform.TransformPoint(pos);
				}
				yield return null;
			}
			while (Time.time - t0 < howLongWillTheyComply);
		}
	}
}
