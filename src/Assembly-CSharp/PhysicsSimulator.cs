using System.Collections.Generic;
using UnityEngine;

public class PhysicsSimulator : ScaleChangeReceiver
{
	protected bool _resetSimulation;

	protected bool _freezeSimulation;

	protected bool _collisionEnabled;

	protected bool _useInterpolation;

	protected int _solverIterations = 15;

	protected List<AsyncFlag> waitResumeSimulationFlags;

	public virtual bool resetSimulation
	{
		get
		{
			return _resetSimulation;
		}
		set
		{
			if (_resetSimulation != value)
			{
				_resetSimulation = value;
				SyncResetSimulation();
			}
		}
	}

	public virtual bool freezeSimulation
	{
		get
		{
			return _freezeSimulation;
		}
		set
		{
			if (_freezeSimulation != value)
			{
				_freezeSimulation = value;
				SyncFreezeSimulation();
			}
		}
	}

	public virtual bool collisionEnabled
	{
		get
		{
			return _collisionEnabled;
		}
		set
		{
			if (_collisionEnabled != value)
			{
				_collisionEnabled = value;
				SyncCollisionEnabled();
			}
		}
	}

	public virtual bool useInterpolation
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
				SyncUseInterpolation();
			}
		}
	}

	public virtual int solverIterations
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
				SyncSolverIterations();
			}
		}
	}

	protected virtual void SyncResetSimulation()
	{
		SyncCollisionEnabled();
	}

	protected virtual void SyncFreezeSimulation()
	{
		SyncCollisionEnabled();
	}

	protected virtual void SyncCollisionEnabled()
	{
	}

	protected virtual void SyncUseInterpolation()
	{
	}

	protected virtual void SyncSolverIterations()
	{
	}

	protected virtual void CheckResumeSimulation()
	{
		if (waitResumeSimulationFlags == null)
		{
			waitResumeSimulationFlags = new List<AsyncFlag>();
		}
		if (waitResumeSimulationFlags.Count <= 0)
		{
			return;
		}
		bool flag = false;
		List<AsyncFlag> list = new List<AsyncFlag>();
		foreach (AsyncFlag waitResumeSimulationFlag in waitResumeSimulationFlags)
		{
			if (waitResumeSimulationFlag.Raised)
			{
				list.Add(waitResumeSimulationFlag);
				flag = true;
			}
		}
		foreach (AsyncFlag item in list)
		{
			waitResumeSimulationFlags.Remove(item);
		}
		if (waitResumeSimulationFlags.Count > 0)
		{
			resetSimulation = true;
		}
		else if (flag)
		{
			resetSimulation = false;
		}
	}

	public virtual bool IsSimulationResetting()
	{
		return _resetSimulation;
	}

	public virtual void ResetSimulation(AsyncFlag waitFor)
	{
		if (waitResumeSimulationFlags == null)
		{
			waitResumeSimulationFlags = new List<AsyncFlag>();
		}
		waitResumeSimulationFlags.Add(waitFor);
		resetSimulation = true;
	}

	protected virtual void Update()
	{
		if (Application.isPlaying)
		{
			CheckResumeSimulation();
		}
	}
}
