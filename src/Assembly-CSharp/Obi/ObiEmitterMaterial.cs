using System;
using UnityEngine;

namespace Obi
{
	public abstract class ObiEmitterMaterial : ScriptableObject
	{
		public class MaterialChangeEventArgs : EventArgs
		{
			public MaterialChanges changes;

			public MaterialChangeEventArgs(MaterialChanges changes)
			{
				this.changes = changes;
			}
		}

		[Flags]
		public enum MaterialChanges
		{
			PER_MATERIAL_DATA = 0,
			PER_PARTICLE_DATA = 1
		}

		public float resolution = 1f;

		public float restDensity = 1000f;

		private EventHandler<MaterialChangeEventArgs> onChangesMade;

		public event EventHandler<MaterialChangeEventArgs> OnChangesMade
		{
			add
			{
				onChangesMade = (EventHandler<MaterialChangeEventArgs>)Delegate.Remove(onChangesMade, value);
				onChangesMade = (EventHandler<MaterialChangeEventArgs>)Delegate.Combine(onChangesMade, value);
			}
			remove
			{
				onChangesMade = (EventHandler<MaterialChangeEventArgs>)Delegate.Remove(onChangesMade, value);
			}
		}

		public void CommitChanges(MaterialChanges changes)
		{
			if (onChangesMade != null)
			{
				onChangesMade(this, new MaterialChangeEventArgs(changes));
			}
		}

		public float GetParticleSize(Oni.SolverParameters.Mode mode)
		{
			return 1f / (10f * Mathf.Pow(resolution, 1f / ((mode != 0) ? 2f : 3f)));
		}

		public float GetParticleMass(Oni.SolverParameters.Mode mode)
		{
			return restDensity * Mathf.Pow(GetParticleSize(mode), (mode != 0) ? 2 : 3);
		}

		public abstract Oni.FluidMaterial GetEquivalentOniMaterial(Oni.SolverParameters.Mode mode);
	}
}
