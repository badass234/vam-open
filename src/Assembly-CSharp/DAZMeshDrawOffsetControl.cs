public class DAZMeshDrawOffsetControl : JSONStorable
{
	public DAZMesh mesh;

	protected JSONStorableFloat xPositionOffsetJSON;

	protected float _xPositionOffset;

	protected JSONStorableFloat yPositionOffsetJSON;

	protected float _yPositionOffset;

	protected JSONStorableFloat zPositionOffsetJSON;

	protected float _zPositionOffset;

	protected JSONStorableFloat xRotationOffsetJSON;

	protected float _xRotationOffset;

	protected JSONStorableFloat yRotationOffsetJSON;

	protected float _yRotationOffset;

	protected JSONStorableFloat zRotationOffsetJSON;

	protected float _zRotationOffset;

	protected JSONStorableFloat overallScaleJSON;

	protected float _overallScale = 1f;

	protected JSONStorableFloat xScaleJSON;

	protected float _xScale = 1f;

	protected JSONStorableFloat yScaleJSON;

	protected float _yScale = 1f;

	protected JSONStorableFloat zScaleJSON;

	protected float _zScale = 1f;

	public float xPositionOffset
	{
		get
		{
			return _xPositionOffset;
		}
		set
		{
			if (xPositionOffsetJSON != null)
			{
				xPositionOffsetJSON.val = value;
			}
			else if (_xPositionOffset != value)
			{
				SyncXPositionOffset(value);
			}
		}
	}

	public float yPositionOffset
	{
		get
		{
			return _yPositionOffset;
		}
		set
		{
			if (yPositionOffsetJSON != null)
			{
				yPositionOffsetJSON.val = value;
			}
			else if (_yPositionOffset != value)
			{
				SyncYPositionOffset(value);
			}
		}
	}

	public float zPositionOffset
	{
		get
		{
			return _zPositionOffset;
		}
		set
		{
			if (zPositionOffsetJSON != null)
			{
				zPositionOffsetJSON.val = value;
			}
			else if (_zPositionOffset != value)
			{
				SyncZPositionOffset(value);
			}
		}
	}

	public float xRotationOffset
	{
		get
		{
			return _xRotationOffset;
		}
		set
		{
			if (xRotationOffsetJSON != null)
			{
				xRotationOffsetJSON.val = value;
			}
			else if (_xRotationOffset != value)
			{
				SyncXRotationOffset(value);
			}
		}
	}

	public float yRotationOffset
	{
		get
		{
			return _yRotationOffset;
		}
		set
		{
			if (yRotationOffsetJSON != null)
			{
				yRotationOffsetJSON.val = value;
			}
			else if (_yRotationOffset != value)
			{
				SyncYRotationOffset(value);
			}
		}
	}

	public float zRotationOffset
	{
		get
		{
			return _zRotationOffset;
		}
		set
		{
			if (zRotationOffsetJSON != null)
			{
				zRotationOffsetJSON.val = value;
			}
			else if (_zRotationOffset != value)
			{
				SyncZRotationOffset(value);
			}
		}
	}

	public float overallScale
	{
		get
		{
			return _overallScale;
		}
		set
		{
			if (overallScaleJSON != null)
			{
				overallScaleJSON.val = value;
			}
			else if (_overallScale != value)
			{
				SyncOverallScale(value);
			}
		}
	}

	public float xScale
	{
		get
		{
			return _xScale;
		}
		set
		{
			if (xScaleJSON != null)
			{
				xScaleJSON.val = value;
			}
			else if (_xScale != value)
			{
				SyncXScale(value);
			}
		}
	}

	public float yScale
	{
		get
		{
			return _yScale;
		}
		set
		{
			if (yScaleJSON != null)
			{
				yScaleJSON.val = value;
			}
			else if (_yScale != value)
			{
				SyncYScale(value);
			}
		}
	}

	public float zScale
	{
		get
		{
			return _zScale;
		}
		set
		{
			if (zScaleJSON != null)
			{
				zScaleJSON.val = value;
			}
			else if (_zScale != value)
			{
				SyncZScale(value);
			}
		}
	}

	protected void SyncMesh()
	{
		if (mesh != null)
		{
			mesh.useDrawOffset = true;
			mesh.drawOffset.x = _xPositionOffset;
			mesh.drawOffset.y = _yPositionOffset;
			mesh.drawOffset.z = _zPositionOffset;
			mesh.drawOffsetRotation.x = _xRotationOffset;
			mesh.drawOffsetRotation.y = _yRotationOffset;
			mesh.drawOffsetRotation.z = _zRotationOffset;
			mesh.drawOffsetOverallScale = _overallScale;
			mesh.drawOffsetScale.x = _xScale;
			mesh.drawOffsetScale.y = _yScale;
			mesh.drawOffsetScale.z = _zScale;
		}
	}

	protected void SyncXPositionOffset(float f)
	{
		_xPositionOffset = f;
		SyncMesh();
	}

	protected void SyncYPositionOffset(float f)
	{
		_yPositionOffset = f;
		SyncMesh();
	}

	protected void SyncZPositionOffset(float f)
	{
		_zPositionOffset = f;
		SyncMesh();
	}

	protected void SyncXRotationOffset(float f)
	{
		_xRotationOffset = f;
		SyncMesh();
	}

	protected void SyncYRotationOffset(float f)
	{
		_yRotationOffset = f;
		SyncMesh();
	}

	protected void SyncZRotationOffset(float f)
	{
		_zRotationOffset = f;
		SyncMesh();
	}

	protected void SyncOverallScale(float f)
	{
		_overallScale = f;
		SyncMesh();
	}

	protected void SyncXScale(float f)
	{
		_xScale = f;
		SyncMesh();
	}

	protected void SyncYScale(float f)
	{
		_yScale = f;
		SyncMesh();
	}

	protected void SyncZScale(float f)
	{
		_zScale = f;
		SyncMesh();
	}

	protected void Init()
	{
		if (mesh != null)
		{
			_xPositionOffset = mesh.drawOffset.x;
			_yPositionOffset = mesh.drawOffset.y;
			_zPositionOffset = mesh.drawOffset.z;
			_xRotationOffset = mesh.drawOffsetRotation.x;
			_yRotationOffset = mesh.drawOffsetRotation.y;
			_zRotationOffset = mesh.drawOffsetRotation.z;
			_overallScale = mesh.drawOffsetOverallScale;
			_xScale = mesh.drawOffsetScale.x;
			_yScale = mesh.drawOffsetScale.y;
			_zScale = mesh.drawOffsetScale.z;
		}
		xPositionOffsetJSON = new JSONStorableFloat("positionOffset:x", _xPositionOffset, SyncXPositionOffset, -0.1f, 0.1f, false);
		RegisterFloat(xPositionOffsetJSON);
		yPositionOffsetJSON = new JSONStorableFloat("positionOffset:y", _yPositionOffset, SyncYPositionOffset, -0.1f, 0.1f, false);
		RegisterFloat(yPositionOffsetJSON);
		zPositionOffsetJSON = new JSONStorableFloat("positionOffset:z", _zPositionOffset, SyncZPositionOffset, -0.1f, 0.1f, false);
		RegisterFloat(zPositionOffsetJSON);
		xRotationOffsetJSON = new JSONStorableFloat("rotationOffset:x", _xRotationOffset, SyncXRotationOffset, -180f, 180f);
		RegisterFloat(xRotationOffsetJSON);
		yRotationOffsetJSON = new JSONStorableFloat("rotationOffset:y", _yRotationOffset, SyncYRotationOffset, -180f, 180f);
		RegisterFloat(yRotationOffsetJSON);
		zRotationOffsetJSON = new JSONStorableFloat("rotationOffset:z", _zRotationOffset, SyncZRotationOffset, -180f, 180f);
		RegisterFloat(zRotationOffsetJSON);
		overallScaleJSON = new JSONStorableFloat("scale:overall", _overallScale, SyncOverallScale, 0.1f, 2f, false);
		RegisterFloat(overallScaleJSON);
		xScaleJSON = new JSONStorableFloat("scale:x", _xScale, SyncXScale, 0.1f, 2f, false);
		RegisterFloat(xScaleJSON);
		yScaleJSON = new JSONStorableFloat("scale:y", _yScale, SyncYScale, 0.1f, 2f, false);
		RegisterFloat(yScaleJSON);
		zScaleJSON = new JSONStorableFloat("scale:z", _zScale, SyncZScale, 0.1f, 2f, false);
		RegisterFloat(zScaleJSON);
	}

	public override void InitUI()
	{
		if (UITransform != null)
		{
			DAZMeshDrawOffsetControlUI componentInChildren = UITransform.GetComponentInChildren<DAZMeshDrawOffsetControlUI>();
			if (componentInChildren != null)
			{
				xPositionOffsetJSON.slider = componentInChildren.xPositionOffsetSlider;
				yPositionOffsetJSON.slider = componentInChildren.yPositionOffsetSlider;
				zPositionOffsetJSON.slider = componentInChildren.zPositionOffsetSlider;
				xRotationOffsetJSON.slider = componentInChildren.xRotationOffsetSlider;
				yRotationOffsetJSON.slider = componentInChildren.yRotationOffsetSlider;
				zRotationOffsetJSON.slider = componentInChildren.zRotationOffsetSlider;
				overallScaleJSON.slider = componentInChildren.overallScaleSlider;
				xScaleJSON.slider = componentInChildren.xScaleSlider;
				yScaleJSON.slider = componentInChildren.yScaleSlider;
				zScaleJSON.slider = componentInChildren.zScaleSlider;
			}
		}
	}

	protected override void Awake()
	{
		if (!awakecalled)
		{
			base.Awake();
			Init();
			InitUI();
		}
	}
}
