using System.Collections;
using UnityEngine;

namespace Leap.Unity
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class ConnectionMonitor : MonoBehaviour
	{
		[Tooltip("The scene LeapServiceProvider.")]
		public LeapServiceProvider provider;

		[Tooltip("How fast to make the connection notice sprite visible.")]
		[Range(0.1f, 10f)]
		public float fadeInTime = 1f;

		[Tooltip("How fast to fade out the connection notice sprite.")]
		[Range(0.1f, 10f)]
		public float fadeOutTime = 1f;

		[Tooltip("The easing curve for the fade in and out effect.")]
		public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		[Tooltip("How frequently to check the connection.")]
		public int monitorInterval = 2;

		[Tooltip("A tint applied to the connection notice sprite when on.")]
		public Color onColor = Color.white;

		[Tooltip("How far to place the sprite in front of the camera.")]
		public float distanceToCamera = 12f;

		private float fadedIn;

		private SpriteRenderer spriteRenderer;

		private bool connected;

		private void Start()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
			SetAlpha(0f);
			StartCoroutine(Monitor());
		}

		private void SetAlpha(float alpha)
		{
			spriteRenderer.color = Color.Lerp(Color.clear, onColor, alpha);
		}

		private void Update()
		{
			if (fadedIn > 0f)
			{
				Camera main = Camera.main;
				Vector3 position = main.transform.position + main.transform.forward * distanceToCamera;
				base.transform.position = position;
				base.transform.LookAt(main.transform);
			}
		}

		private IEnumerator Monitor()
		{
			yield return new WaitForSecondsRealtime(monitorInterval);
			while (true)
			{
				connected = provider.IsConnected();
				if (connected)
				{
					while ((double)fadedIn > 0.0)
					{
						fadedIn -= Time.deltaTime / fadeOutTime;
						fadedIn = Mathf.Clamp(fadedIn, 0f, 1f);
						SetAlpha(fadeCurve.Evaluate(fadedIn));
						yield return null;
					}
				}
				else
				{
					while ((double)fadedIn < 1.0)
					{
						fadedIn += Time.deltaTime / fadeOutTime;
						fadedIn = Mathf.Clamp(fadedIn, 0f, 1f);
						SetAlpha(fadeCurve.Evaluate(fadedIn));
						yield return null;
					}
				}
				yield return new WaitForSecondsRealtime(monitorInterval);
			}
		}
	}
}
