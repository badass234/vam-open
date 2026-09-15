using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class RigidbodyAttributes : ScaleChangeReceiver
{
	private Rigidbody rb;

	[SerializeField]
	private bool _useOverrideTensor;

	private Vector3 _originalTensor;

	private Quaternion _originalTensorRotation;

	[SerializeField]
	private Vector3 _inertiaTensor = Vector3.one;

	[SerializeField]
	private bool _useInterpolation;

	[SerializeField]
	private bool _useOverrideIterations;

	private int _originalIterations;

	private int _originalVelocityIterations;

	[SerializeField]
	private int _solverIterations = 10;

	[SerializeField]
	private int _solverVelocityIterations = 1;

	protected bool _isEnabled;

	public bool useOverrideTensor
	{
		get
		{
			return _useOverrideTensor;
		}
		set
		{
			if (_useOverrideTensor != value)
			{
				_useOverrideTensor = value;
				SyncTensor();
			}
		}
	}

	public Vector3 originalTensor => _originalTensor;

	public Quaternion originalTensorRotation => _originalTensorRotation;

	public Vector3 currentTensor
	{
		get
		{
			if (rb != null)
			{
				return rb.inertiaTensor;
			}
			return Vector3.zero;
		}
	}

	public Vector3 inertiaTensor
	{
		get
		{
			return _inertiaTensor;
		}
		set
		{
			if (_inertiaTensor != value)
			{
				_inertiaTensor = value;
				SyncTensor();
			}
		}
	}

	public float maxDepenetrationVelocity
	{
		get
		{
			if (rb != null)
			{
				return rb.maxDepenetrationVelocity;
			}
			return -1f;
		}
	}

	public float maxAngularVelocity
	{
		get
		{
			if (rb != null)
			{
				return rb.maxAngularVelocity;
			}
			return -1f;
		}
	}

	public bool useInterpolation
	{
		get
		{
			return _useInterpolation;
		}
		set
		{
			if (_useInterpolation != value)
			{
				_useInterpolation = value;
				SyncInterpolation();
			}
		}
	}

	public bool useOverrideIterations
	{
		get
		{
			return _useOverrideIterations;
		}
		set
		{
			if (_useOverrideIterations != value)
			{
				_useOverrideIterations = value;
			}
		}
	}

	public int origianlIterations => _originalIterations;

	public int origianlVelocityIterations => _originalVelocityIterations;

	public int currentIterations
	{
		get
		{
			if (rb != null)
			{
				return rb.solverIterations;
			}
			return 0;
		}
	}

	public int solverIterations
	{
		get
		{
			return _solverIterations;
		}
		set
		{
			if (_solverIterations != value)
			{
				_solverIterations = value;
				SetOverrideIterations();
			}
		}
	}

	public int solverVelocityIterations
	{
		get
		{
			return _solverVelocityIterations;
		}
		set
		{
			if (_solverVelocityIterations != value)
			{
				_solverVelocityIterations = value;
				SetOverrideIterations();
			}
		}
	}

	public Rigidbody GetRigidbody()
	{
		return rb;
	}

	public void SyncTensor()
	{
		if (rb != null)
		{
			if (_useOverrideTensor)
			{
				rb.inertiaTensor = _inertiaTensor * _scale;
			}
			else
			{
				rb.ResetInertiaTensor();
			}
		}
	}

	public override void ScaleChanged(float scale)
	{
		base.ScaleChanged(scale);
		SyncTensor();
	}

	private void SetOriginalTensor()
	{
		if (rb != null)
		{
			_originalTensor = rb.inertiaTensor;
			_originalTensorRotation = rb.inertiaTensorRotation;
		}
	}

	private void SyncInterpolation()
	{
		if (rb != null)
		{
			if (Application.isPlaying && base.isActiveAndEnabled && _useInterpolation)
			{
				rb.interpolation = RigidbodyInterpolation.Interpolate;
			}
			else
			{
				rb.interpolation = RigidbodyInterpolation.None;
			}
		}
	}

	private IEnumerator ResumeInterpolation()
	{
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		SyncInterpolation();
	}

	public void TempDisableInterpolation()
	{
		if (rb != null)
		{
			rb.interpolation = RigidbodyInterpolation.None;
			if (_isEnabled)
			{
				StopAllCoroutines();
				StartCoroutine(ResumeInterpolation());
			}
		}
	}

	protected void SetOverrideIterations()
	{
		if (rb != null && Application.isPlaying && base.isActiveAndEnabled && _useOverrideIterations)
		{
			rb.solverIterations = _solverIterations;
			rb.solverVelocityIterations = _solverVelocityIterations;
		}
	}

	private void SetOriginalIterations()
	{
		if (rb != null)
		{
			_originalIterations = rb.solverIterations;
			_originalVelocityIterations = rb.solverVelocityIterations;
		}
	}

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		SetOriginalTensor();
		SetOriginalIterations();
		SyncTensor();
		SetOverrideIterations();
	}

	private void OnEnable()
	{
		rb = GetComponent<Rigidbody>();
		SyncInterpolation();
		SyncTensor();
		SetOverrideIterations();
		_isEnabled = true;
	}

	private void OnDisable()
	{
		SyncInterpolation();
		_isEnabled = false;
	}
}
