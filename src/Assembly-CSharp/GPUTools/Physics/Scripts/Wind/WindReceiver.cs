using System.Collections.Generic;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Wind
{
	public class WindReceiver
	{
		private float angle;

		private readonly WindZone[] winds;

		private readonly Perlin2D perlin = new Perlin2D(566);

		private readonly List<NoiseOctave> octaves = new List<NoiseOctave>();

		public Vector3 Vector { get; set; }

		public WindReceiver()
		{
			winds = Object.FindObjectsOfType<WindZone>();
			octaves.Add(new NoiseOctave(1f, 1f));
			octaves.Add(new NoiseOctave(5f, 0.6f));
			octaves.Add(new NoiseOctave(10f, 0.4f));
			octaves.Add(new NoiseOctave(20f, 0.3f));
		}

		public Vector3 GetWind(Vector3 position)
		{
			Vector = Vector3.zero;
			WindZone[] array = winds;
			foreach (WindZone windZone in array)
			{
				if (windZone.mode == WindZoneMode.Directional)
				{
					UpdateDirectionalWind(windZone);
				}
				else
				{
					UpdateSphericalWind(windZone, position);
				}
			}
			return Vector;
		}

		private void UpdateDirectionalWind(WindZone wind)
		{
			Vector3 dirrection = wind.transform.rotation * Vector3.forward;
			Vector += GetAmplitude(wind, dirrection);
		}

		private void UpdateSphericalWind(WindZone wind, Vector3 center)
		{
			Vector3 vector = center - wind.transform.position;
			if (!(vector.magnitude > wind.radius))
			{
				Vector += GetAmplitude(wind, vector.normalized);
			}
		}

		private Vector3 GetAmplitude(WindZone wind, Vector3 dirrection)
		{
			angle += wind.windPulseFrequency;
			float noise = GetNoise(angle);
			float num = wind.windMain + noise * wind.windPulseMagnitude;
			return dirrection * num;
		}

		private float GetNoise(float angle)
		{
			return Mathf.Abs(perlin.Noise(new Vector2(0f, angle), octaves));
		}
	}
}
