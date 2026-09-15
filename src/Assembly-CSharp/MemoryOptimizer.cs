using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AssetBundles;
using UnityEngine;
using UnityEngine.UI;

public class MemoryOptimizer : MonoBehaviour
{
	public delegate void OnMemoryOptimizeComplete();

	public delegate void MemoryOptimizerCallback();

	public delegate void MemoryOptimizerReporter(List<string> reportList);

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	protected struct MemUsage
	{
		public float WorkingSetSize;

		public float PeakWorkingSetSize;

		public float PageFileUsage;

		public float PeakPageFileUsage;
	}

	public static MemoryOptimizer singleton;

	protected bool optimizeTrigger;

	protected Coroutine optimizeMemoryUsageCoroutine;

	protected OnMemoryOptimizeComplete onMemoryOptimizeComplete;

	protected OnMemoryOptimizeComplete nextOnMemoryOptimizeComplete;

	protected MemoryOptimizerCallback memoryOptimizerCallbacks;

	protected List<string> memoryReports;

	protected MemoryOptimizerReporter memoryOptimizerReporters;

	public GameObject memoryOptimizerUI;

	public Text reportText;

	public Text systemMemorySizeText;

	public Text workingSetSizeText;

	public Text peakWorkingSetSizeText;

	public Text pageFileUsageText;

	public Text peakPageFileUsageText;

	public Text heapUsageText;

	public Text loadedBundlesText;

	public Button reportButton;

	public Button optimizeButton;

	public Button clearAtomPoolButton;

	public GameObject optimizingIndicator;

	protected MemUsage memUsage;

	protected float systemMemorySize;

	protected static float gByteMult = 9.313226E-10f;

	public float WorkingSetSizeInGB => memUsage.WorkingSetSize * gByteMult;

	public string WorkingSetSizeInGBText => (memUsage.WorkingSetSize * gByteMult).ToString("F2") + " GB";

	public float PeakWorkingSetSizeInGB => memUsage.PeakWorkingSetSize * gByteMult;

	public string PeakWorkingSetSizeInGBText => (memUsage.PeakWorkingSetSize * gByteMult).ToString("F2") + " GB";

	public float PageFileUsageInGB => memUsage.PageFileUsage * gByteMult;

	public string PageFileUsageInGBText => (memUsage.PageFileUsage * gByteMult).ToString("F2") + " GB";

	public float PeakPageFileUsageInGB => memUsage.PeakPageFileUsage * gByteMult;

	public string PeakPageFileUsageInGBText => (memUsage.PeakPageFileUsage * gByteMult).ToString("F2") + " GB";

	public float HeapSizeInGB => (float)GC.GetTotalMemory(false) * gByteMult;

	public string HeapSizeInGBText => ((float)GC.GetTotalMemory(false) * gByteMult).ToString("F2") + " GB";

	public float SystemMemorySizeInGB => systemMemorySize;

	public string SystemMemorySizeInGBText => systemMemorySize.ToString("F2") + " GB";

	public static float GByteMult => gByteMult;

	public IEnumerator OptimizeMemoryUsage()
	{
		yield return null;
		if (memoryOptimizerCallbacks != null)
		{
			memoryOptimizerCallbacks();
		}
		yield return null;
		yield return null;
		yield return Resources.UnloadUnusedAssets();
		GC.Collect();
	}

	protected IEnumerator OptimizeMemoryUsageCo()
	{
		yield return StartCoroutine(OptimizeMemoryUsage());
		yield return null;
		optimizeMemoryUsageCoroutine = null;
		if (onMemoryOptimizeComplete != null)
		{
			onMemoryOptimizeComplete();
		}
		onMemoryOptimizeComplete = nextOnMemoryOptimizeComplete;
		if (optimizingIndicator != null)
		{
			optimizingIndicator.SetActive(false);
		}
		if (reportText != null)
		{
			reportText.text = "Optimization Complete";
		}
	}

	public void TriggerOptimize(OnMemoryOptimizeComplete onComplete)
	{
		optimizeTrigger = true;
		if (onComplete != null)
		{
			if (optimizeMemoryUsageCoroutine == null)
			{
				onMemoryOptimizeComplete = (OnMemoryOptimizeComplete)Delegate.Combine(onMemoryOptimizeComplete, onComplete);
			}
			else
			{
				nextOnMemoryOptimizeComplete = (OnMemoryOptimizeComplete)Delegate.Combine(nextOnMemoryOptimizeComplete, onComplete);
			}
		}
	}

	public void TriggerOptimize()
	{
		TriggerOptimize(null);
	}

	protected void HandleOptimizeTrigger()
	{
		if (optimizeMemoryUsageCoroutine == null && optimizeTrigger)
		{
			optimizeTrigger = false;
			if (optimizingIndicator != null)
			{
				optimizingIndicator.SetActive(true);
			}
			if (reportText != null)
			{
				reportText.text = "Optimizing...";
			}
			StartCoroutine(OptimizeMemoryUsageCo());
		}
	}

	public void ClearAtomPool()
	{
		SuperController.singleton.ClearAtomPool();
	}

	public void RegisterMemoryOptimizerListener(MemoryOptimizerCallback memoryOptimizerCallback)
	{
		memoryOptimizerCallbacks = (MemoryOptimizerCallback)Delegate.Combine(memoryOptimizerCallbacks, memoryOptimizerCallback);
	}

	public void DeregisterMemoryOptimizerListener(MemoryOptimizerCallback memoryOptimizerCallback)
	{
		memoryOptimizerCallbacks = (MemoryOptimizerCallback)Delegate.Remove(memoryOptimizerCallbacks, memoryOptimizerCallback);
	}

	public void ReportMemoryUsage()
	{
		if (memoryReports == null)
		{
			memoryReports = new List<string>();
		}
		else
		{
			memoryReports.Clear();
		}
		if (memoryOptimizerReporters != null)
		{
			memoryOptimizerReporters(memoryReports);
		}
		if (reportText != null)
		{
			reportText.text = string.Empty;
		}
		foreach (string memoryReport in memoryReports)
		{
			if (reportText != null)
			{
				Text text = reportText;
				text.text = text.text + memoryReport + "\n";
			}
		}
	}

	public void RegisterMemoryOptimizerReporter(MemoryOptimizerReporter memoryOptimizerReporter)
	{
		memoryOptimizerReporters = (MemoryOptimizerReporter)Delegate.Combine(memoryOptimizerReporters, memoryOptimizerReporter);
	}

	public void DeregisterMemoryOptimizerReporter(MemoryOptimizerReporter memoryOptimizerReporter)
	{
		memoryOptimizerReporters = (MemoryOptimizerReporter)Delegate.Remove(memoryOptimizerReporters, memoryOptimizerReporter);
	}

	[DllImport("MemoryReporter")]
	protected static extern MemUsage GetMemoryUsage();

	protected void Awake()
	{
		singleton = this;
		systemMemorySize = (float)SystemInfo.systemMemorySize / 1024f;
		if (reportButton != null)
		{
			reportButton.onClick.AddListener(ReportMemoryUsage);
		}
		if (optimizeButton != null)
		{
			optimizeButton.onClick.AddListener(TriggerOptimize);
		}
		if (clearAtomPoolButton != null)
		{
			clearAtomPoolButton.onClick.AddListener(ClearAtomPool);
		}
	}

	protected void Update()
	{
		memUsage = GetMemoryUsage();
		HandleOptimizeTrigger();
		if (memoryOptimizerUI != null && memoryOptimizerUI.activeInHierarchy)
		{
			if (systemMemorySizeText != null)
			{
				systemMemorySizeText.text = SystemMemorySizeInGBText;
			}
			if (workingSetSizeText != null)
			{
				workingSetSizeText.text = WorkingSetSizeInGBText;
			}
			if (peakWorkingSetSizeText != null)
			{
				peakWorkingSetSizeText.text = PeakWorkingSetSizeInGBText;
			}
			if (pageFileUsageText != null)
			{
				pageFileUsageText.text = PageFileUsageInGBText;
			}
			if (peakPageFileUsageText != null)
			{
				peakPageFileUsageText.text = PeakPageFileUsageInGBText;
			}
			if (heapUsageText != null)
			{
				heapUsageText.text = HeapSizeInGBText;
			}
			if (loadedBundlesText != null)
			{
				loadedBundlesText.text = AssetBundleManager.GetNumberOfLoadedAssetBundles().ToString();
			}
		}
	}
}
