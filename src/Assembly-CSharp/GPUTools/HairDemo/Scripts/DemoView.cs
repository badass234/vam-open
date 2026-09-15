using UnityEngine;
using UnityEngine.UI;

namespace GPUTools.HairDemo.Scripts
{
	public class DemoView : MonoBehaviour
	{
		[SerializeField]
		private Button play;

		[SerializeField]
		private Button stop;

		[SerializeField]
		private Button next;

		[SerializeField]
		private Button prev;

		[SerializeField]
		private Button rotate;

		[SerializeField]
		private GameObject[] styles;

		[SerializeField]
		private ConstantRotation rotation;

		private int currentStyleIndex;

		private GameObject CurrentStyle => styles[currentStyleIndex];

		private int CurrentStyleIndex
		{
			get
			{
				return currentStyleIndex;
			}
			set
			{
				currentStyleIndex = value;
				if (currentStyleIndex < 0)
				{
					currentStyleIndex = styles.Length - 1;
				}
				if (currentStyleIndex > styles.Length - 1)
				{
					currentStyleIndex = 0;
				}
				ApplyStyle();
			}
		}

		private void Start()
		{
			SetStartStyle();
			play.onClick.AddListener(OnClickPlay);
			stop.onClick.AddListener(OnClickStop);
			next.onClick.AddListener(OnClickNext);
			prev.onClick.AddListener(OnClickPrev);
			rotate.onClick.AddListener(OnClickRotate);
		}

		private void OnClickRotate()
		{
			rotation.Speed += 200f;
			if (rotation.Speed >= 800f)
			{
				rotation.Speed = 0f;
			}
		}

		private void OnClickPrev()
		{
			CurrentStyleIndex--;
		}

		private void OnClickNext()
		{
			CurrentStyleIndex++;
		}

		private void OnClickStop()
		{
			CurrentStyle.GetComponent<Animator>().enabled = false;
		}

		private void OnClickPlay()
		{
			CurrentStyle.GetComponent<Animator>().enabled = true;
		}

		private void ApplyStyle()
		{
			for (int i = 0; i < styles.Length; i++)
			{
				GameObject gameObject = styles[i];
				gameObject.SetActive(i == currentStyleIndex);
			}
		}

		private void SetStartStyle()
		{
			for (int i = 0; i < styles.Length; i++)
			{
				GameObject gameObject = styles[i];
				if (gameObject.activeSelf)
				{
					CurrentStyleIndex = i;
				}
			}
		}
	}
}
