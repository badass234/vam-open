using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Battlehub.RTSaveLoad
{
	public class Job : MonoBehaviour, IJob
	{
		public class JobContainer
		{
			public object Lock = new object();

			public bool IsCompleted;

			private Action<Action> m_job;

			private Action m_completed;

			public JobContainer(Action<Action> job, Action completed)
			{
				m_job = job;
				m_completed = completed;
			}

			private void ThreadFunc(object arg)
			{
				m_job(delegate
				{
					lock (Lock)
					{
						IsCompleted = true;
					}
				});
			}

			public void Run()
			{
				ThreadPool.QueueUserWorkItem(ThreadFunc);
			}

			public void RaiseCompleted()
			{
				m_completed();
			}
		}

		private List<JobContainer> m_jobs = new List<JobContainer>();

		public void Submit(Action<Action> job, Action completed)
		{
			JobContainer jobContainer = new JobContainer(job, completed);
			m_jobs.Add(jobContainer);
			jobContainer.Run();
		}

		private void Update()
		{
			for (int num = m_jobs.Count - 1; num >= 0; num--)
			{
				JobContainer jobContainer = m_jobs[num];
				lock (jobContainer.Lock)
				{
					if (jobContainer.IsCompleted)
					{
						try
						{
							jobContainer.RaiseCompleted();
						}
						finally
						{
							m_jobs.RemoveAt(num);
						}
					}
				}
			}
		}
	}
}
