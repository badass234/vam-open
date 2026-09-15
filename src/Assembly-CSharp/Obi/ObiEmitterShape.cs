using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	public abstract class ObiEmitterShape : MonoBehaviour
	{
		public enum SamplingMethod
		{
			SURFACE,
			LAYER,
			FILL
		}

		public struct DistributionPoint
		{
			public Vector3 position;

			public Vector3 velocity;

			public Color color;

			public DistributionPoint(Vector3 position, Vector3 velocity)
			{
				this.position = position;
				this.velocity = velocity;
				color = Color.white;
			}

			public DistributionPoint(Vector3 position, Vector3 velocity, Color color)
			{
				this.position = position;
				this.velocity = velocity;
				this.color = color;
			}
		}

		public SamplingMethod samplingMethod;

		[HideInInspector]
		public float particleSize;

		protected List<DistributionPoint> distribution = new List<DistributionPoint>();

		protected int lastDistributionPoint;

		public int DistributionPointsCount => distribution.Count;

		public void OnEnable()
		{
			ObiEmitter component = GetComponent<ObiEmitter>();
			if (component != null)
			{
				component.EmitterShape = this;
			}
		}

		public void OnDisable()
		{
			ObiEmitter component = GetComponent<ObiEmitter>();
			if (component != null)
			{
				component.EmitterShape = null;
			}
		}

		public abstract void GenerateDistribution();

		public abstract bool SupportsAllSamplingMethods();

		public DistributionPoint GetDistributionPoint()
		{
			if (lastDistributionPoint >= distribution.Count)
			{
				return default(DistributionPoint);
			}
			DistributionPoint result = distribution[lastDistributionPoint];
			lastDistributionPoint = (lastDistributionPoint + 1) % distribution.Count;
			return result;
		}
	}
}
