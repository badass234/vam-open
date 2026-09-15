using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace VikingCrewTools.UI
{
	public class SpeechBubbleBehaviour : MonoBehaviour
	{
		private float _timeToLive = 1f;

		private Transform _objectToFollow;

		private Vector3 _offset;

		[FormerlySerializedAs("text")]
		[SerializeField]
		private Text _text;

		[FormerlySerializedAs("image")]
		[SerializeField]
		private Image _image;

		private int _iteration;

		private Camera _cam;

		public int Iteration => _iteration;

		public Camera Cam
		{
			get
			{
				return _cam;
			}
			set
			{
				_cam = value;
			}
		}

		protected void Update()
		{
			_timeToLive -= Time.unscaledDeltaTime;
			if (0f < _timeToLive && _timeToLive < 1f)
			{
				_image.color = new Color(_image.color.r, _image.color.g, _image.color.b, _timeToLive);
				_text.color = new Color(_text.color.r, _text.color.g, _text.color.b, _timeToLive);
			}
			if (_timeToLive <= 0f)
			{
				Clear();
			}
		}

		protected void LateUpdate()
		{
			if (_objectToFollow != null)
			{
				base.transform.position = _objectToFollow.position + _offset;
			}
			base.transform.rotation = _cam.transform.rotation;
		}

		public void Clear()
		{
			base.gameObject.SetActive(false);
			_iteration++;
		}

		public void UpdateText(string text, float newTimeToLive)
		{
			_text.text = text;
			_timeToLive = newTimeToLive;
		}

		public void Setup(Vector3 position, string text, float timeToLive, Color color, Camera cam)
		{
			Setup(text, timeToLive, color, cam);
			base.transform.position = position;
			base.transform.rotation = _cam.transform.rotation;
			_objectToFollow = null;
			_offset = Vector3.zero;
			if (timeToLive > 0f)
			{
				base.gameObject.SetActive(true);
			}
		}

		public void Setup(Transform objectToFollow, Vector3 offset, string text, float timeToLive, Color color, Camera cam)
		{
			Setup(text, timeToLive, color, cam);
			_objectToFollow = objectToFollow;
			base.transform.position = objectToFollow.position + offset;
			base.transform.rotation = _cam.transform.rotation;
			_offset = offset;
			if (timeToLive > 0f)
			{
				base.gameObject.SetActive(true);
			}
		}

		private void Setup(string text, float timeToLive, Color color, Camera cam)
		{
			if ((bool)cam)
			{
				_cam = cam;
			}
			else
			{
				_cam = Camera.main;
			}
			_timeToLive = timeToLive;
			_text.text = text;
			_image.color = color;
			_text.color = new Color(_text.color.r, _text.color.g, _text.color.b, 1f);
		}
	}
}
