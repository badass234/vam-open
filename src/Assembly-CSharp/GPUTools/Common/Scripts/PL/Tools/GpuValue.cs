namespace GPUTools.Common.Scripts.PL.Tools
{
	public class GpuValue<T>
	{
		public T Value { get; set; }

		public GpuValue(T value = default(T))
		{
			Value = value;
		}
	}
}
