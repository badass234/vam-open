using UnityEngine;

public class UISideAlign : MonoBehaviour
{
	public enum Side
	{
		Left,
		Right
	}

	public static Side globalSide = Side.Right;

	public static bool useNeutralRotation;

	public bool obeyGlobalSide = true;

	public bool invertGlobalSide;

	protected RectTransform rt;

	[SerializeField]
	protected Side _currentSide;

	protected bool _usingNeutralRotation;

	public float neutralAngleY;

	public float neutralAngleZ;

	public float leftSideAnchorMinMax;

	public float leftSidePivotX = 1f;

	public float leftSideOffsetX = -2f;

	public float leftSideAngleY = -30f;

	public float leftSideAngleZ;

	public float rightSideAnchorMinMax = 1f;

	public float rightSidePivotX;

	public float rightSideOffsetX = 2f;

	public float rightSideAngleY = 30f;

	public float rightSideAngleZ;

	public Side currentSide
	{
		get
		{
			return _currentSide;
		}
		protected set
		{
			if (_currentSide != value)
			{
				_currentSide = value;
				Sync();
			}
		}
	}

	public bool usingNeutralRotation
	{
		get
		{
			return _usingNeutralRotation;
		}
		set
		{
			if (_usingNeutralRotation != value)
			{
				_usingNeutralRotation = value;
				Sync();
			}
		}
	}

	public void Sync()
	{
		if (rt == null)
		{
			rt = GetComponent<RectTransform>();
		}
		if (!(rt != null))
		{
			return;
		}
		Vector2 anchorMin = rt.anchorMin;
		Vector2 anchorMax = rt.anchorMax;
		Vector2 pivot = rt.pivot;
		Vector2 anchoredPosition = rt.anchoredPosition;
		Vector3 localEulerAngles = rt.localEulerAngles;
		if (_currentSide == Side.Left)
		{
			pivot.x = leftSidePivotX;
			rt.pivot = pivot;
			anchorMin.x = leftSideAnchorMinMax;
			rt.anchorMin = anchorMin;
			anchorMax.x = leftSideAnchorMinMax;
			rt.anchorMax = anchorMax;
			anchoredPosition.x = leftSideOffsetX;
			rt.anchoredPosition = anchoredPosition;
			if (useNeutralRotation)
			{
				localEulerAngles.y = neutralAngleY;
				localEulerAngles.z = neutralAngleZ;
			}
			else
			{
				localEulerAngles.y = leftSideAngleY;
				localEulerAngles.z = leftSideAngleZ;
			}
			rt.localEulerAngles = localEulerAngles;
		}
		else
		{
			pivot.x = rightSidePivotX;
			rt.pivot = pivot;
			anchorMin.x = rightSideAnchorMinMax;
			rt.anchorMin = anchorMin;
			anchorMax.x = rightSideAnchorMinMax;
			rt.anchorMax = anchorMax;
			anchoredPosition.x = rightSideOffsetX;
			rt.anchoredPosition = anchoredPosition;
			if (useNeutralRotation)
			{
				localEulerAngles.y = neutralAngleY;
				localEulerAngles.z = neutralAngleZ;
			}
			else
			{
				localEulerAngles.y = rightSideAngleY;
				localEulerAngles.z = rightSideAngleZ;
			}
			rt.localEulerAngles = localEulerAngles;
		}
	}

	protected void SyncToGlobal()
	{
		if (!obeyGlobalSide)
		{
			return;
		}
		if (invertGlobalSide)
		{
			if (globalSide == Side.Left)
			{
				currentSide = Side.Right;
			}
			else
			{
				currentSide = Side.Left;
			}
		}
		else if (globalSide == Side.Left)
		{
			currentSide = Side.Left;
		}
		else
		{
			currentSide = Side.Right;
		}
		usingNeutralRotation = useNeutralRotation;
	}

	protected void Awake()
	{
		SyncToGlobal();
		Sync();
	}

	protected void OnEnable()
	{
		SyncToGlobal();
	}

	protected void Update()
	{
		SyncToGlobal();
	}
}
