using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Weelco.VRInput
{
	public class VRInputSettings : MonoBehaviour
	{
		public enum InputControlMethod
		{
			MOUSE,
			GOOGLEVR,
			GAZE,
			VIVE,
			OCULUS_INPUT,
			OCULUS_TOUCH
		}

		public enum Hand
		{
			Both,
			Right,
			Left
		}

		public InputControlMethod ControlMethod;

		[SerializeField]
		private Hand usedHand = Hand.Right;

		[SerializeField]
		private Transform gazeCanvas;

		[SerializeField]
		private Image gazeProgressBar;

		[SerializeField]
		private float gazeClickTimer = 1f;

		[SerializeField]
		private float gazeClickTimerDelay = 1f;

		[SerializeField]
		private float laserThickness = 0.01f;

		[SerializeField]
		private float laserHitScale = 1f;

		[SerializeField]
		private Color laserColor = Color.red;

		[SerializeField]
		private bool useHapticPulse;

		[SerializeField]
		private bool useCustomLaserPointer;

		[SerializeField]
		private bool hitAlwaysOn = true;

		private List<IUIPointer> _pointersList;

		private static VRInputSettings _instance;

		public static VRInputSettings instance => _instance;

		public float LaserThickness
		{
			get
			{
				return laserThickness;
			}
			set
			{
				laserThickness = value;
			}
		}

		public float LaserHitScale
		{
			get
			{
				return laserHitScale;
			}
			set
			{
				laserHitScale = value;
			}
		}

		public Color LaserColor
		{
			get
			{
				return laserColor;
			}
			set
			{
				laserColor = value;
			}
		}

		public bool UseHapticPulse
		{
			get
			{
				return useHapticPulse;
			}
			set
			{
				useHapticPulse = value;
			}
		}

		public bool UseCustomLaserPointer
		{
			get
			{
				return useCustomLaserPointer;
			}
			set
			{
				useCustomLaserPointer = value;
			}
		}

		public bool HitAlwaysOn
		{
			get
			{
				return hitAlwaysOn;
			}
			set
			{
				hitAlwaysOn = value;
			}
		}

		public Hand UsedHand
		{
			get
			{
				return usedHand;
			}
			set
			{
				usedHand = value;
			}
		}

		public float GazeClickTimer
		{
			get
			{
				return gazeClickTimer;
			}
			set
			{
				gazeClickTimer = value;
			}
		}

		public float GazeClickTimerDelay
		{
			get
			{
				return gazeClickTimerDelay;
			}
			set
			{
				gazeClickTimerDelay = value;
			}
		}

		public Transform GazeCanvas
		{
			get
			{
				return gazeCanvas;
			}
			set
			{
				gazeCanvas = value;
			}
		}

		public Image GazeProgressBar
		{
			get
			{
				return gazeProgressBar;
			}
			set
			{
				gazeProgressBar = value;
			}
		}

		private void Awake()
		{
			if (_instance != null)
			{
				Debug.LogWarning("Trying to instantiate multiple VRInputSystems.");
				Object.DestroyImmediate(base.gameObject);
			}
			_instance = this;
		}

		private void Start()
		{
			_pointersList = new List<IUIPointer>();
			createEventSystem(ControlMethod);
			switch (ControlMethod)
			{
			case InputControlMethod.OCULUS_TOUCH:
				break;
			case InputControlMethod.OCULUS_INPUT:
				break;
			case InputControlMethod.GAZE:
				createGazePointer();
				break;
			case InputControlMethod.VIVE:
				break;
			}
		}

		private void Update()
		{
			foreach (IUIPointer pointers in _pointersList)
			{
				pointers.Update();
			}
		}

		private void OnDestroy()
		{
			if (!(VRInputModule.instance != null))
			{
				return;
			}
			foreach (IUIPointer pointers in _pointersList)
			{
				VRInputModule.instance.RemoveController(pointers);
			}
			_pointersList.Clear();
		}

		private void createEventSystem(InputControlMethod method)
		{
			GameObject gameObject = null;
			if (method.Equals(InputControlMethod.MOUSE) || method.Equals(InputControlMethod.GOOGLEVR))
			{
				return;
			}
			if (Object.FindObjectOfType<EventSystem>() != null)
			{
				gameObject = Object.FindObjectOfType<EventSystem>().gameObject;
			}
			if (gameObject == null)
			{
				gameObject = new GameObject("EventSystem");
			}
			MonoBehaviour[] components = gameObject.GetComponents<MonoBehaviour>();
			MonoBehaviour[] array = components;
			foreach (MonoBehaviour monoBehaviour in array)
			{
				if (!(monoBehaviour is EventSystem))
				{
					monoBehaviour.enabled = false;
				}
			}
			if (gameObject.GetComponent<VRInputModule>() == null && VRInputModule.instance == null)
			{
				VRInputModule vRInputModule;
				switch (method)
				{
				case InputControlMethod.GOOGLEVR:
				case InputControlMethod.GAZE:
					vRInputModule = gameObject.AddComponent<VRGazeInputModule>();
					break;
				case InputControlMethod.VIVE:
				case InputControlMethod.OCULUS_INPUT:
				case InputControlMethod.OCULUS_TOUCH:
					vRInputModule = gameObject.AddComponent<VRHitInputModule>();
					break;
				default:
					vRInputModule = gameObject.AddComponent<VRInputModule>();
					break;
				}
				vRInputModule.enabled = true;
			}
		}

		private void createGazePointer()
		{
			UIGazePointer uIGazePointer = new UIGazePointer();
			uIGazePointer.GazeCanvas = gazeCanvas;
			uIGazePointer.GazeProgressBar = gazeProgressBar;
			uIGazePointer.GazeClickTimer = gazeClickTimer;
			uIGazePointer.GazeClickTimerDelay = gazeClickTimerDelay;
			initPointer(uIGazePointer);
		}

		private void initPointer(IUIPointer pointer)
		{
			VRInputModule.instance.AddController(pointer);
			pointer.Initialize();
			_pointersList.Add(pointer);
		}
	}
}
