using System.Collections.Generic;
using MVR;
using UnityEngine;

public class DAZCharacterRunMetricsUI : MonoBehaviour
{
	private enum Side
	{
		Left,
		Right
	}

	public DAZCharacterRun dazCharacterRun;

	public RectTransform leftSideMetricContainer;

	public RectTransform rightSideMetricContainer;

	public RectTransform metricUIPrefab;

	protected Metric FIXED_metric;

	protected Metric UPDATE_metric;

	protected Metric LATE_metric;

	protected Metric THREAD_metric;

	protected Metric MAIN_finishTime_metric;

	protected Metric MAIN_prepTime_metric;

	protected Metric MAIN_fixedThreadWaitTime_metric;

	protected Metric MAIN_updateThreadWaitTime_metric;

	protected Metric MAIN_autoColliderFinishTime_metric;

	protected Metric MAIN_skinPrepTime_metric;

	protected Metric MAIN_skinFinishTime_metric;

	protected Metric MAIN_skinDrawTime_metric;

	protected Metric MAIN_physicsMeshPrepTime_metric;

	protected Metric MAIN_physicsMeshFixedUpdateTime_metric;

	protected Metric MAIN_physicsMeshFinishTime_metric;

	protected Metric MAIN_morphPrepTime_metric;

	protected Metric MAIN_morphFinishTime_metric;

	protected Metric MAIN_bonePrepTime_metric;

	protected Metric MAIN_otherPrepTime_metric;

	private int frameCount;

	private int framesForAvg = 50;

	private Dictionary<string, Metric> metricNameToMetric;

	private void Update()
	{
		if (dazCharacterRun != null)
		{
			frameCount++;
			if (frameCount > framesForAvg)
			{
				frameCount = 0;
				FIXED_metric.CalculateAverage();
				UPDATE_metric.CalculateAverage();
				LATE_metric.CalculateAverage();
				THREAD_metric.CalculateAverage();
				MAIN_finishTime_metric.CalculateAverage();
				MAIN_prepTime_metric.CalculateAverage();
				MAIN_fixedThreadWaitTime_metric.CalculateAverage();
				MAIN_updateThreadWaitTime_metric.CalculateAverage();
				MAIN_autoColliderFinishTime_metric.CalculateAverage();
				MAIN_skinPrepTime_metric.CalculateAverage();
				MAIN_skinFinishTime_metric.CalculateAverage();
				MAIN_skinDrawTime_metric.CalculateAverage();
				MAIN_physicsMeshPrepTime_metric.CalculateAverage();
				MAIN_physicsMeshFixedUpdateTime_metric.CalculateAverage();
				MAIN_physicsMeshFinishTime_metric.CalculateAverage();
				MAIN_morphPrepTime_metric.CalculateAverage();
				MAIN_morphFinishTime_metric.CalculateAverage();
				MAIN_bonePrepTime_metric.CalculateAverage();
				MAIN_otherPrepTime_metric.CalculateAverage();
			}
			FIXED_metric.Accumulate(dazCharacterRun.FIXED_time);
			UPDATE_metric.Accumulate(dazCharacterRun.UPDATE_time);
			LATE_metric.Accumulate(dazCharacterRun.LATE_time);
			THREAD_metric.Accumulate(dazCharacterRun.THREAD_time);
			MAIN_finishTime_metric.Accumulate(dazCharacterRun.MAIN_finishTime);
			MAIN_prepTime_metric.Accumulate(dazCharacterRun.MAIN_prepTime);
			MAIN_fixedThreadWaitTime_metric.Accumulate(dazCharacterRun.MAIN_fixedThreadWaitTime);
			MAIN_updateThreadWaitTime_metric.Accumulate(dazCharacterRun.MAIN_updateThreadWaitTime);
			MAIN_autoColliderFinishTime_metric.Accumulate(dazCharacterRun.MAIN_autoColliderFinishTime);
			MAIN_skinPrepTime_metric.Accumulate(dazCharacterRun.MAIN_skinPrepTime);
			MAIN_skinFinishTime_metric.Accumulate(dazCharacterRun.MAIN_skinFinishTime);
			MAIN_skinDrawTime_metric.Accumulate(dazCharacterRun.MAIN_skinDrawTime);
			MAIN_physicsMeshPrepTime_metric.Accumulate(dazCharacterRun.MAIN_physicsMeshPrepTime);
			MAIN_physicsMeshFixedUpdateTime_metric.Accumulate(dazCharacterRun.MAIN_physicsMeshFixedUpdateTime);
			MAIN_physicsMeshFinishTime_metric.Accumulate(dazCharacterRun.MAIN_physicsMeshFinishTime);
			MAIN_morphPrepTime_metric.Accumulate(dazCharacterRun.MAIN_morphPrepTime);
			MAIN_morphFinishTime_metric.Accumulate(dazCharacterRun.MAIN_morphFinishTime);
			MAIN_bonePrepTime_metric.Accumulate(dazCharacterRun.MAIN_bonePrepTime);
			MAIN_otherPrepTime_metric.Accumulate(dazCharacterRun.MAIN_otherPrepTime);
		}
	}

	private Metric CreateMetric(string metricName, Side side = Side.Left)
	{
		if (!metricNameToMetric.ContainsKey(metricName))
		{
			Metric metric = new Metric(metricName);
			if (metricUIPrefab != null && leftSideMetricContainer != null && rightSideMetricContainer != null)
			{
				Transform transform = Object.Instantiate(metricUIPrefab);
				if (side == Side.Left)
				{
					transform.SetParent(leftSideMetricContainer, false);
				}
				else
				{
					transform.SetParent(rightSideMetricContainer, false);
				}
				MetricUI component = transform.GetComponent<MetricUI>();
				metric.UI = component;
			}
			metricNameToMetric.Add(metricName, metric);
			return metric;
		}
		return null;
	}

	private void UpdateMetricValue(string metricName, float value)
	{
		Metric value2;
		if (metricNameToMetric.TryGetValue(metricName, out value2))
		{
			value2.Value = value;
		}
	}

	private void UpdateMetricAverageValue(string metricName, float averageValue)
	{
		Metric value;
		if (metricNameToMetric.TryGetValue(metricName, out value))
		{
			value.AverageValue = averageValue;
		}
	}

	private void Start()
	{
		metricNameToMetric = new Dictionary<string, Metric>();
		FIXED_metric = CreateMetric("FIXED");
		MAIN_fixedThreadWaitTime_metric = CreateMetric("  Thread Wait Fixed");
		MAIN_physicsMeshFixedUpdateTime_metric = CreateMetric("  Soft Body Fixed");
		UPDATE_metric = CreateMetric("UPDATE");
		MAIN_updateThreadWaitTime_metric = CreateMetric("  Thread Wait Update");
		MAIN_finishTime_metric = CreateMetric("  Finish Run");
		MAIN_morphFinishTime_metric = CreateMetric("    Morph Finish");
		MAIN_autoColliderFinishTime_metric = CreateMetric("    Skin Collider Finish");
		MAIN_physicsMeshFinishTime_metric = CreateMetric("    Soft Body Finish");
		MAIN_skinFinishTime_metric = CreateMetric("    Skin Finish");
		MAIN_prepTime_metric = CreateMetric("  Prep Next Run");
		MAIN_skinPrepTime_metric = CreateMetric("    Skin Prep");
		MAIN_physicsMeshPrepTime_metric = CreateMetric("    Soft Body Prep");
		MAIN_morphPrepTime_metric = CreateMetric("    Morph Prep");
		MAIN_bonePrepTime_metric = CreateMetric("    Bone Prep");
		MAIN_otherPrepTime_metric = CreateMetric("    Other Prep");
		LATE_metric = CreateMetric("LATE");
		MAIN_skinDrawTime_metric = CreateMetric("  Skin Draw");
		THREAD_metric = CreateMetric("THREAD", Side.Right);
	}
}
