using System;
using System.Threading;
using UnityEngine;

namespace DynamicCSharp.Compiler
{
	public class AsyncCompileOperation : CustomYieldInstruction
	{
		private ScriptCompiler handler;

		private Func<bool> asyncCallback;

		private volatile bool threadExit;

		private bool hasStarted;

		protected byte[] assemblyData;

		protected string[] errors;

		protected string[] warnings;

		protected bool isSuccessful;

		protected bool isDone;

		public byte[] AssemblyData => assemblyData;

		public string[] CompilerErrors => errors;

		public string[] CompilerWarnings => warnings;

		public bool IsSuccessful => isSuccessful;

		public bool IsDone => isDone;

		public override bool keepWaiting
		{
			get
			{
				if (!hasStarted)
				{
					isSuccessful = true;
					ThreadPool.QueueUserWorkItem(delegate
					{
						try
						{
							if (asyncCallback != null && !asyncCallback())
							{
								isSuccessful = false;
							}
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
							isSuccessful = false;
						}
						threadExit = true;
					});
					hasStarted = true;
				}
				if (!threadExit)
				{
					return true;
				}
				try
				{
					DoSyncFinalize();
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
					isSuccessful = false;
				}
				if (handler.HasErrors)
				{
					isSuccessful = false;
				}
				isDone = true;
				return false;
			}
		}

		internal AsyncCompileOperation(ScriptCompiler handler, Func<bool> asyncCallback)
		{
			this.handler = handler;
			this.asyncCallback = asyncCallback;
		}

		protected virtual void DoSyncFinalize()
		{
			assemblyData = handler.AssemblyData;
			errors = handler.Errors;
			warnings = handler.Warnings;
		}

		public new void Reset()
		{
			threadExit = false;
			isSuccessful = false;
			isDone = false;
			hasStarted = false;
		}
	}
}
