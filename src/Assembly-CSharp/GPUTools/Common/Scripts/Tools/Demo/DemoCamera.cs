using System;
using UnityEngine;

namespace GPUTools.Common.Scripts.Tools.Demo
{
	public class DemoCamera : MonoBehaviour
	{
		[SerializeField]
		private Vector3 lookAt = new Vector3(0f, 0.05f, 0f);

		private float radius;

		private float angle = (float)Math.PI / 2f;

		private float elevation;

		private void Awake()
		{
			radius = base.transform.position.z;
			elevation = base.transform.position.y;
		}

		private void OnEnable()
		{
		}

		private void Update()
		{
			float x = Mathf.Cos(angle) * radius;
			float y = elevation;
			float z = Mathf.Sin(angle) * radius;
			base.transform.position = new Vector3(x, y, z);
			base.transform.LookAt(lookAt);
			HandleWheel();
			HandleMove();
		}

		private void HandleWheel()
		{
			radius += Input.GetAxis("Mouse ScrollWheel");
		}

		private void HandleMove()
		{
			if (Input.GetMouseButton(0))
			{
				angle -= Input.GetAxis("Mouse X") * Time.deltaTime;
				elevation -= Input.GetAxis("Mouse Y") * Time.deltaTime;
			}
		}
	}
}
