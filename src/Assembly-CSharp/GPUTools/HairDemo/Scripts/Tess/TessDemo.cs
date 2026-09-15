using GPUTools.Common.Scripts.Tools;
using UnityEngine;

namespace GPUTools.HairDemo.Scripts.Tess
{
	public class TessDemo : MonoBehaviour
	{
		[SerializeField]
		[Range(6f, 64f)]
		private int count = 64;

		private int oldCount;

		[SerializeField]
		private Vector3 a;

		[SerializeField]
		private Vector3 b;

		[SerializeField]
		private Vector3 c;

		private FixedList<Vector3> barycentric = new FixedList<Vector3>(64);

		private void Start()
		{
			Gen();
			oldCount = count;
		}

		private void Update()
		{
			if (count != oldCount)
			{
				Gen();
			}
			oldCount = count;
		}

		private void Gen()
		{
			barycentric.Reset();
			int steps = 1;
			float num = 0.2f;
			if (count >= 15)
			{
				num = 0.1f;
				steps = 2;
			}
			if (count >= 45)
			{
				num = 0.05f;
				steps = 3;
			}
			float num2 = 1f - num;
			float num3 = (1f - num2) * 0.5f;
			Split(new Vector3(num2, num3, num3), new Vector3(num3, num2, num3), new Vector3(num3, num3, num2), steps);
			while (barycentric.Count < count)
			{
				Vector3 randomK = GetRandomK();
				if (!barycentric.Contains(randomK))
				{
					barycentric.Add(GetRandomK());
				}
			}
			Debug.Log(barycentric.Count);
		}

		private void Split(Vector3 b1, Vector3 b2, Vector3 b3, int steps)
		{
			steps--;
			TryAdd(b1);
			TryAdd(b2);
			TryAdd(b3);
			Vector3 vector = (b1 + b2) * 0.5f;
			Vector3 vector2 = (b2 + b3) * 0.5f;
			Vector3 b4 = (b3 + b1) * 0.5f;
			if (steps >= 0)
			{
				Split(b1, vector, b4, steps);
				Split(b2, vector, vector2, steps);
				Split(b3, vector2, b4, steps);
				Split(vector, vector2, b4, steps);
			}
		}

		private void TryAdd(Vector3 v)
		{
			if (!barycentric.Contains(v))
			{
				barycentric.Add(v);
			}
		}

		private Vector3 GetRandomK()
		{
			float num = Random.Range(0f, 1f);
			float num2 = Random.Range(0f, 1f);
			if (num + num2 > 1f)
			{
				num = 1f - num;
				num2 = 1f - num2;
			}
			float z = 1f - (num + num2);
			return new Vector3(num, num2, z);
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawLine(a, b);
			Gizmos.DrawLine(b, c);
			Gizmos.DrawLine(c, a);
			Gizmos.color = new Color(1f, 0f, 0f, 1f);
			for (int i = 0; i < barycentric.Count; i++)
			{
				Vector3 vector = barycentric[i];
				Vector3 center = a * vector.x + b * vector.y + c * vector.z;
				Gizmos.DrawSphere(center, 0.01f);
			}
		}
	}
}
