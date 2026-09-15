namespace Leap.Unity
{
	public interface IProgressView
	{
		void Clear();

		void DisplayProgress(string title, string info, float progress);
	}
}
