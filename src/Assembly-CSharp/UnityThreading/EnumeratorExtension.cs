using System.Collections;

namespace UnityThreading
{
	public static class EnumeratorExtension
	{
		public static Task RunAsync(this IEnumerator that)
		{
			return that.RunAsync(UnityThreadHelper.TaskDistributor);
		}

		public static Task RunAsync(this IEnumerator that, TaskDistributor target)
		{
			return target.Dispatch(Task.Create(that));
		}
	}
}
