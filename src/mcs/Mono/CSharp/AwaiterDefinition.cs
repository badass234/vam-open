namespace Mono.CSharp
{
	public class AwaiterDefinition
	{
		public PropertySpec IsCompleted { get; set; }

		public MethodSpec GetResult { get; set; }

		public bool INotifyCompletion { get; set; }

		public bool IsValidPattern
		{
			get
			{
				if (IsCompleted != null && GetResult != null)
				{
					return IsCompleted.HasGet;
				}
				return false;
			}
		}
	}
}
