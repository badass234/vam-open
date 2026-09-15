using GPUTools.Common.Scripts.PL.Abstract;
using UnityEngine;

namespace GPUTools.Common.Scripts.PL.Tools
{
	public class GpuBuffer<T> : IBufferWrapper where T : struct
	{
		public ComputeBuffer ComputeBuffer { get; private set; }

		public T[] Data { get; set; }

		public int Count => ComputeBuffer.count;

		public GpuBuffer(ComputeBuffer computeBuffer)
		{
			ComputeBuffer = computeBuffer;
		}

		public GpuBuffer(int count, int stride)
		{
			Data = new T[count];
			ComputeBuffer = new ComputeBuffer(count, stride);
			ComputeBuffer.SetData(Data);
		}

		public GpuBuffer(T[] data, int stride)
		{
			Data = data;
			ComputeBuffer = new ComputeBuffer(data.Length, stride);
			ComputeBuffer.SetData(data);
		}

		public void PushData()
		{
			ComputeBuffer.SetData(Data);
		}

		public void PullData()
		{
			ComputeBuffer.GetData(Data);
		}

		public void Dispose()
		{
			ComputeBuffer.Dispose();
		}
	}
}
