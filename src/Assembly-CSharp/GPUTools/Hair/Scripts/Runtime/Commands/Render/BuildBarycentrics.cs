using System.Collections.Generic;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Common.Scripts.Tools.Commands;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Runtime.Commands.Render
{
	public class BuildBarycentrics : IBuildCommand
	{
		public static int MaxCount = 64;

		private readonly HairSettings settings;

		private List<Vector3> barycentric = new List<Vector3>();

		private List<Vector3> barycentricFixed = new List<Vector3>();

		public BuildBarycentrics(HairSettings settings)
		{
			this.settings = settings;
		}

		public void Build()
		{
			Gen();
			if (settings.RuntimeData.Barycentrics != null)
			{
				settings.RuntimeData.Barycentrics.Dispose();
			}
			if (barycentric.Count > 0)
			{
				settings.RuntimeData.Barycentrics = new GpuBuffer<Vector3>(barycentric.ToArray(), 12);
			}
			else
			{
				settings.RuntimeData.Barycentrics = null;
			}
			if (settings.RuntimeData.BarycentricsFixed != null)
			{
				settings.RuntimeData.BarycentricsFixed.Dispose();
			}
			if (barycentricFixed.Count > 0)
			{
				settings.RuntimeData.BarycentricsFixed = new GpuBuffer<Vector3>(barycentricFixed.ToArray(), 12);
			}
			else
			{
				settings.RuntimeData.BarycentricsFixed = null;
			}
		}

		public void Dispatch()
		{
		}

		public void FixedDispatch()
		{
		}

		public void UpdateSettings()
		{
			Gen();
			if (settings.RuntimeData.Barycentrics != null)
			{
				settings.RuntimeData.Barycentrics.PushData();
			}
			if (settings.RuntimeData.BarycentricsFixed != null)
			{
				settings.RuntimeData.BarycentricsFixed.PushData();
			}
		}

		public void Dispose()
		{
			if (settings.RuntimeData.Barycentrics != null)
			{
				settings.RuntimeData.Barycentrics.Dispose();
			}
			if (settings.RuntimeData.BarycentricsFixed != null)
			{
				settings.RuntimeData.BarycentricsFixed.Dispose();
			}
		}

		private void Gen()
		{
			Random.InitState(6);
			barycentric = new List<Vector3>();
			for (int i = 0; i < settings.StandsSettings.Provider.GetStandsNum(); i++)
			{
				for (int j = 0; j < MaxCount; j++)
				{
					barycentric.Add(GetRandomK());
				}
			}
			barycentricFixed = new List<Vector3>();
			for (int k = 0; k < settings.StandsSettings.Provider.GetStandsNum(); k++)
			{
				for (int l = 0; l < MaxCount; l += 3)
				{
					barycentricFixed.Add(new Vector3(0.99f, 0.005f, 0.005f));
					barycentricFixed.Add(new Vector3(0.005f, 0.99f, 0.005f));
					barycentricFixed.Add(new Vector3(0.005f, 0.005f, 0.99f));
				}
			}
			barycentricFixed = barycentricFixed.GetRange(0, MaxCount * settings.StandsSettings.Provider.GetStandsNum());
		}

		private void Split(Vector3 b1, Vector3 b2, Vector3 b3, int steps)
		{
			steps--;
			TryAdd(b1);
			TryAdd(b2);
			TryAdd(b3);
			Vector3 vector = (b1 + b2) * 0.5f;
			Vector3 vector2 = (b2 + b3) * 0.5f;
			Vector3 b4 = (b3 + b1) * 0.5f;
			if (steps >= 0)
			{
				Split(b1, vector, b4, steps);
				Split(b2, vector, vector2, steps);
				Split(b3, vector2, b4, steps);
				Split(vector, vector2, b4, steps);
			}
		}

		private void TryAdd(Vector3 v)
		{
			if (!barycentric.Contains(v))
			{
				barycentric.Add(v);
			}
		}

		private Vector3 GetRandomK()
		{
			float num = Random.Range(0f, 1f);
			float num2 = Random.Range(0f, 1f);
			if (num + num2 > 1f)
			{
				num = 1f - num;
				num2 = 1f - num2;
			}
			float z = 1f - (num + num2);
			return new Vector3(num, num2, z);
		}
	}
}
