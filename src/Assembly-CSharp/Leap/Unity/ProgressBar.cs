using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Leap.Unity
{
	public class ProgressBar
	{
		private List<int> chunks = new List<int>();

		private List<int> progress = new List<int>();

		private List<string> titleStrings = new List<string>();

		private List<string> infoStrings = new List<string>();

		private Stopwatch stopwatch = new Stopwatch();

		private bool _forceUpdate;

		private IProgressView _view;

		public ProgressBar(IProgressView view)
		{
			_view = view;
		}

		public void Begin(int sections, string title, string info, Action action)
		{
			if (!stopwatch.IsRunning)
			{
				stopwatch.Reset();
				stopwatch.Start();
			}
			chunks.Add(sections);
			progress.Add(0);
			titleStrings.Add(title);
			infoStrings.Add(info);
			try
			{
				_forceUpdate = true;
				action();
			}
			finally
			{
				int num = chunks.Count - 1;
				chunks.RemoveAt(num);
				progress.RemoveAt(num);
				titleStrings.RemoveAt(num);
				infoStrings.RemoveAt(num);
				num--;
				if (num >= 0)
				{
					progress[num]++;
				}
				if (chunks.Count == 0)
				{
					_view.Clear();
					stopwatch.Stop();
				}
			}
		}

		public void Step(string infoString = "")
		{
			progress[progress.Count - 1]++;
			if (stopwatch.ElapsedMilliseconds > 17 || _forceUpdate)
			{
				displayBar(infoString);
				stopwatch.Reset();
				stopwatch.Start();
			}
		}

		private void displayBar(string info = "")
		{
			_forceUpdate = false;
			float num = 0f;
			float num2 = 1f;
			string text = string.Empty;
			string text2 = string.Empty;
			for (int i = 0; i < chunks.Count; i++)
			{
				float num3 = chunks[i];
				float num4 = progress[i];
				num += num2 * (num4 / num3);
				num2 /= num3;
				text += titleStrings[i];
				text2 += infoStrings[i];
			}
			text2 += info;
			_view.DisplayProgress(text, text2, num);
		}
	}
}
