using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mono.CSharp
{
	public class TimeReporter
	{
		public enum TimerType
		{
			ParseTotal = 0,
			AssemblyBuilderSetup = 1,
			CreateTypeTotal = 2,
			ReferencesLoading = 3,
			ReferencesImporting = 4,
			PredefinedTypesInit = 5,
			ModuleDefinitionTotal = 6,
			EmitTotal = 7,
			CloseTypes = 8,
			Resouces = 9,
			OutputSave = 10,
			DebugSave = 11
		}

		private readonly Stopwatch[] timers;

		private Stopwatch total;

		public TimeReporter(bool enabled)
		{
			if (enabled)
			{
				timers = new Stopwatch[System.Enum.GetValues(typeof(TimerType)).Length];
			}
		}

		public void Start(TimerType type)
		{
			if (timers != null)
			{
				Stopwatch stopwatch = new Stopwatch();
				timers[(int)type] = stopwatch;
				stopwatch.Start();
			}
		}

		public void StartTotal()
		{
			total = new Stopwatch();
			total.Start();
		}

		public void Stop(TimerType type)
		{
			if (timers != null)
			{
				timers[(int)type].Stop();
			}
		}

		public void StopTotal()
		{
			total.Stop();
		}

		public void ShowStats()
		{
			if (timers != null)
			{
				Dictionary<TimerType, string> dictionary = new Dictionary<TimerType, string>
				{
					{
						TimerType.ParseTotal,
						"Parsing source files"
					},
					{
						TimerType.AssemblyBuilderSetup,
						"Assembly builder setup"
					},
					{
						TimerType.CreateTypeTotal,
						"Compiled types created"
					},
					{
						TimerType.ReferencesLoading,
						"Referenced assemblies loading"
					},
					{
						TimerType.ReferencesImporting,
						"Referenced assemblies importing"
					},
					{
						TimerType.PredefinedTypesInit,
						"Predefined types initialization"
					},
					{
						TimerType.ModuleDefinitionTotal,
						"Module definition"
					},
					{
						TimerType.EmitTotal,
						"Resolving and emitting members blocks"
					},
					{
						TimerType.CloseTypes,
						"Module types closed"
					},
					{
						TimerType.Resouces,
						"Embedding resources"
					},
					{
						TimerType.OutputSave,
						"Writing output file"
					},
					{
						TimerType.DebugSave,
						"Writing debug symbols file"
					}
				};
				int num = 0;
				double num2 = (double)total.ElapsedMilliseconds / 100.0;
				long num3 = total.ElapsedMilliseconds;
				Stopwatch[] array = timers;
				foreach (Stopwatch stopwatch in array)
				{
					string arg = dictionary[(TimerType)(num++)];
					long num4 = stopwatch?.ElapsedMilliseconds ?? 0;
					Console.WriteLine("{0,4:0.0}% {1,5}ms {2}", (double)num4 / num2, num4, arg);
					num3 -= num4;
				}
				Console.WriteLine("{0,4:0.0}% {1,5}ms Other tasks", (double)num3 / num2, num3);
				Console.WriteLine();
				Console.WriteLine("Total elapsed time: {0}", total.Elapsed);
			}
		}
	}
}
