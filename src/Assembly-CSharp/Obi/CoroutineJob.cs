using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Obi
{
	public class CoroutineJob
	{
		public class ProgressInfo
		{
			public string userReadableInfo;

			public float progress;

			public ProgressInfo(string userReadableInfo, float progress)
			{
				this.userReadableInfo = userReadableInfo;
				this.progress = progress;
			}
		}

		private object result;

		private bool isDone;

		private bool raisedException;

		private bool stop;

		private Exception e;

		public int asyncThreshold;

		public object Result
		{
			get
			{
				if (e != null)
				{
					throw e;
				}
				return result;
			}
		}

		public bool IsDone => isDone;

		public bool RaisedException => raisedException;

		private void Init()
		{
			isDone = false;
			raisedException = false;
			stop = false;
			result = null;
		}

		public static object RunSynchronously(IEnumerator coroutine)
		{
			List<object> list = new List<object>();
			if (coroutine == null)
			{
				return list;
			}
			try
			{
				while (coroutine.MoveNext())
				{
					list.Add(coroutine.Current);
				}
				return list;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public IEnumerator Start(IEnumerator coroutine)
		{
			Init();
			if (coroutine == null)
			{
				isDone = true;
				yield break;
			}
			Stopwatch sw = new Stopwatch();
			sw.Start();
			while (!stop)
			{
				try
				{
					if (!coroutine.MoveNext())
					{
						isDone = true;
						sw.Stop();
						break;
					}
				}
				catch (Exception ex)
				{
					Exception exception = (e = ex);
					raisedException = true;
					UnityEngine.Debug.LogException(exception);
					isDone = true;
					sw.Stop();
					break;
				}
				result = coroutine.Current;
				if (sw.ElapsedMilliseconds > asyncThreshold)
				{
					yield return result;
				}
			}
		}

		public void Stop()
		{
			stop = true;
		}
	}
}
