using System.Collections.Generic;
using System.Threading;
using UnityEngine.Profiling;

namespace Leap.Unity
{
	public static class LeapProfiling
	{
		private static Dictionary<string, CustomSampler> _samplers = new Dictionary<string, CustomSampler>();

		private static Queue<string> _samplersToCreate = new Queue<string>();

		private static int _samplersToCreateCount = 0;

		public static void Update()
		{
			if (_samplersToCreateCount <= 0)
			{
				return;
			}
			lock (_samplersToCreate)
			{
				Dictionary<string, CustomSampler> dictionary = new Dictionary<string, CustomSampler>(_samplers);
				while (_samplersToCreate.Count > 0)
				{
					string text = _samplersToCreate.Dequeue();
					dictionary[text] = CustomSampler.Create(text);
				}
				_samplersToCreateCount = 0;
				_samplers = dictionary;
			}
		}

		public static void BeginProfilingForThread(BeginProfilingForThreadArgs eventData)
		{
			lock (_samplersToCreate)
			{
				string[] blockNames = eventData.blockNames;
				foreach (string item in blockNames)
				{
					_samplersToCreate.Enqueue(item);
				}
				Interlocked.Add(ref _samplersToCreateCount, eventData.blockNames.Length);
			}
		}

		public static void EndProfilingForThread(EndProfilingForThreadArgs eventData)
		{
		}

		public static void BeginProfilingBlock(BeginProfilingBlockArgs eventData)
		{
			CustomSampler value;
			if (!_samplers.TryGetValue(eventData.blockName, out value))
			{
			}
		}

		public static void EndProfilingBlock(EndProfilingBlockArgs eventData)
		{
			CustomSampler value;
			if (!_samplers.TryGetValue(eventData.blockName, out value))
			{
			}
		}
	}
}
