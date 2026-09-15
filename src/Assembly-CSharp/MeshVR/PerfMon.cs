using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MeshVR
{
	public class PerfMon : MonoBehaviour
	{
		public static float physicsTime;

		public static float scriptsTime;

		public static float preRenderTime;

		public static float renderTime;

		public static float waitTime;

		public static float totalTime;

		public Toggle onToggle;

		[SerializeField]
		protected bool _on;

		public Transform perfMonUI;

		public Transform perfMonUIAlt;

		public Text totalTimeText;

		public Text totalTimeTextAlt;

		public Text scriptsTimeText;

		public Text scriptsTimeTextAlt;

		public Text renderTimeText;

		public Text renderTimeTextAlt;

		public Text physicsTimeText;

		public Text physicsTimeTextAlt;

		public Text waitTimeText;

		public Text waitTimeTextAlt;

		public Text avgTotalTimeText;

		public Text avgTotalTimeTextAlt;

		public Text avgScriptsTimeText;

		public Text avgScriptsTimeTextAlt;

		public Text avgRenderTimeText;

		public Text avgRenderTimeTextAlt;

		public Text avgPhysicsTimeText;

		public Text avgPhysicsTimeTextAlt;

		public Text avgWaitTimeText;

		public Text avgWaitTimeTextAlt;

		public int framesBetweenUpdate = 10;

		public float _frameStartTime;

		public float _frameStopTime;

		public float _preRenderStopTime;

		protected float _internalPhysicsStopTime;

		public float _physicsTime;

		public float _internalPhysicsTime;

		public float _totalTime;

		public float _preRenderTime;

		public float _scriptsTime;

		public float _renderTime;

		protected float _totPhysicsTime;

		protected float _totIntenalPhysicsTime;

		protected float _totTotalTime;

		protected float _totRenderTime;

		protected float _totScriptsTime;

		protected float _totWaitTime;

		protected int _totFrames;

		public float _avgPhysicsTime;

		public float _avgInternalPhysicsTime;

		public float _avgTotalTime;

		public float _avgRenderTime;

		public float _avgScriptsTime;

		public float _avgWaitTime;

		public int avgCalcStartFrame = -1;

		public int avgCalcNumFrames = 900;

		protected int cnt;

		public string fps;

		public Text fpsText;

		public Text fpsTextAlt;

		public Text avgFpsText;

		public Text avgFpsTextAlt;

		public Text physicalMemoryUsageText;

		public Text physicalMemoryUsageTextAlt;

		public Text pagedMemoryUsageText;

		public Text pagedMemoryUsageTextAlt;

		protected float _fpsAccumTime;

		protected int _fpsFrames;

		protected float _lastFrameTime;

		protected float _totFpsTime;

		public float _avgFps;

		public bool on
		{
			get
			{
				return _on;
			}
			set
			{
				if (_on != value)
				{
					_on = value;
					if (perfMonUI != null)
					{
						perfMonUI.gameObject.SetActive(_on);
					}
					if (perfMonUIAlt != null)
					{
						perfMonUIAlt.gameObject.SetActive(_on);
					}
					if (onToggle != null)
					{
						onToggle.isOn = _on;
					}
				}
			}
		}

		public static void ReportWaitTime(float t)
		{
			waitTime += t;
		}

		public void RestartAverageCalc()
		{
			avgCalcStartFrame = _totFrames;
			_totPhysicsTime = 0f;
			_totIntenalPhysicsTime = 0f;
			_totScriptsTime = 0f;
			_totTotalTime = 0f;
			_totRenderTime = 0f;
			_totFpsTime = 0f;
			_totWaitTime = 0f;
		}

		protected void DoUpdate()
		{
			_frameStopTime = GlobalStopwatch.GetElapsedMilliseconds();
			float num = _frameStopTime - _lastFrameTime;
			_fpsAccumTime += 1000f / num;
			_lastFrameTime = _frameStopTime;
			_fpsFrames++;
			if (PerfMonCamera.wasSet)
			{
				_renderTime = _frameStopTime - PerfMonCamera.renderStartTime;
			}
			else
			{
				_renderTime = 0f;
			}
			_totalTime = _preRenderTime + _renderTime;
			physicsTime = _physicsTime;
			scriptsTime = _scriptsTime;
			preRenderTime = _preRenderTime;
			renderTime = _renderTime;
			totalTime = _totalTime;
			if (SuperController.singleton != null && !SuperController.singleton.isLoading && !SuperController.singleton.IsSimulationResetting() && !GlobalSceneOptions.IsLoading)
			{
				_totFrames++;
				int num2 = avgCalcStartFrame + avgCalcNumFrames;
				if (_totFrames > avgCalcStartFrame && _totFrames <= num2)
				{
					_totFpsTime += 1000f / num;
					_totPhysicsTime += _physicsTime;
					_totIntenalPhysicsTime += _internalPhysicsTime;
					_totScriptsTime += _scriptsTime;
					_totTotalTime += _totalTime;
					_totRenderTime += _renderTime;
					_totWaitTime += waitTime;
					float num3 = 1f / (float)(_totFrames - avgCalcStartFrame);
					_avgPhysicsTime = _totPhysicsTime * num3;
					_avgInternalPhysicsTime = _totIntenalPhysicsTime * num3;
					_avgScriptsTime = _totScriptsTime * num3;
					_avgRenderTime = _totRenderTime * num3;
					_avgWaitTime = _totWaitTime * num3;
					_avgTotalTime = _totTotalTime * num3;
					_avgFps = _totFpsTime * num3;
					if (_totFrames == num2)
					{
						Debug.Log("Benchmark complete. Avg. tot time: " + _avgTotalTime.ToString("F2") + " Avg. physics time: " + _avgPhysicsTime.ToString("F2") + " Avg. internal physics time: " + _avgInternalPhysicsTime.ToString("F2") + " Avg. scripts time: " + _avgScriptsTime.ToString("F2") + " Avg. render time: " + _avgRenderTime.ToString("F2") + " Avg. wait time: " + _avgWaitTime.ToString("F2") + " Avg. FPS: " + _avgFps.ToString("F2"));
					}
				}
			}
			if (cnt == 0)
			{
				fps = (_fpsAccumTime / (float)_fpsFrames).ToString("F2");
				if ((bool)fpsText)
				{
					fpsText.text = fps;
				}
				if ((bool)fpsTextAlt)
				{
					fpsTextAlt.text = fps;
				}
				_fpsAccumTime = 0f;
				_fpsFrames = 0;
				if (perfMonUI != null && perfMonUI.gameObject.activeInHierarchy)
				{
					if (totalTimeText != null)
					{
						totalTimeText.text = _totalTime.ToString("F2");
					}
					if (renderTimeText != null)
					{
						renderTimeText.text = _renderTime.ToString("F2");
					}
					if (scriptsTimeText != null)
					{
						scriptsTimeText.text = _scriptsTime.ToString("F2");
					}
					if (physicsTimeText != null)
					{
						physicsTimeText.text = _physicsTime.ToString("F2");
					}
					if (waitTimeText != null)
					{
						waitTimeText.text = waitTime.ToString("F2");
					}
					if (avgTotalTimeText != null)
					{
						avgTotalTimeText.text = _avgTotalTime.ToString("F2");
					}
					if (avgRenderTimeText != null)
					{
						avgRenderTimeText.text = _avgRenderTime.ToString("F2");
					}
					if (avgScriptsTimeText != null)
					{
						avgScriptsTimeText.text = _avgScriptsTime.ToString("F2");
					}
					if (avgPhysicsTimeText != null)
					{
						avgPhysicsTimeText.text = _avgPhysicsTime.ToString("F2");
					}
					if (avgWaitTimeText != null)
					{
						avgWaitTimeText.text = _avgWaitTime.ToString("F2");
					}
					if (avgFpsText != null)
					{
						avgFpsText.text = _avgFps.ToString("F2");
					}
					if (MemoryOptimizer.singleton != null)
					{
						if (physicalMemoryUsageText != null)
						{
							physicalMemoryUsageText.text = MemoryOptimizer.singleton.WorkingSetSizeInGBText;
						}
						if (pagedMemoryUsageText != null)
						{
							pagedMemoryUsageText.text = MemoryOptimizer.singleton.PageFileUsageInGBText;
						}
					}
				}
				if (perfMonUIAlt != null && perfMonUIAlt.gameObject.activeInHierarchy)
				{
					if (totalTimeTextAlt != null)
					{
						totalTimeTextAlt.text = _totalTime.ToString("F2");
					}
					if (renderTimeTextAlt != null)
					{
						renderTimeTextAlt.text = _renderTime.ToString("F2");
					}
					if (scriptsTimeTextAlt != null)
					{
						scriptsTimeTextAlt.text = _scriptsTime.ToString("F2");
					}
					if (physicsTimeTextAlt != null)
					{
						physicsTimeTextAlt.text = _physicsTime.ToString("F2");
					}
					if (waitTimeTextAlt != null)
					{
						waitTimeTextAlt.text = waitTime.ToString("F2");
					}
					if (avgTotalTimeTextAlt != null)
					{
						avgTotalTimeTextAlt.text = _avgTotalTime.ToString("F2");
					}
					if (avgRenderTimeTextAlt != null)
					{
						avgRenderTimeTextAlt.text = _avgRenderTime.ToString("F2");
					}
					if (avgScriptsTimeTextAlt != null)
					{
						avgScriptsTimeTextAlt.text = _avgScriptsTime.ToString("F2");
					}
					if (avgPhysicsTimeTextAlt != null)
					{
						avgPhysicsTimeTextAlt.text = _avgPhysicsTime.ToString("F2");
					}
					if (avgWaitTimeTextAlt != null)
					{
						avgWaitTimeTextAlt.text = _avgWaitTime.ToString("F2");
					}
					if (avgFpsTextAlt != null)
					{
						avgFpsTextAlt.text = _avgFps.ToString("F2");
					}
					if (MemoryOptimizer.singleton != null)
					{
						if (physicalMemoryUsageTextAlt != null)
						{
							physicalMemoryUsageTextAlt.text = MemoryOptimizer.singleton.WorkingSetSizeInGBText;
						}
						if (pagedMemoryUsageTextAlt != null)
						{
							pagedMemoryUsageTextAlt.text = MemoryOptimizer.singleton.PageFileUsageInGBText;
						}
					}
				}
			}
			waitTime = 0f;
		}

		private void FixedUpdate()
		{
			_internalPhysicsStopTime = GlobalStopwatch.GetElapsedMilliseconds();
			_internalPhysicsTime = _internalPhysicsStopTime - PerfMonPre.physicsStartTime;
		}

		private void LateUpdate()
		{
			_frameStartTime = PerfMonPre.frameStartTime;
			_preRenderStopTime = GlobalStopwatch.GetElapsedMilliseconds();
			_preRenderTime = _preRenderStopTime - _frameStartTime;
			_physicsTime = PerfMonPre.physicsTime;
			_scriptsTime = _preRenderTime - _physicsTime;
			cnt++;
			if (cnt == framesBetweenUpdate)
			{
				cnt = 0;
			}
		}

		public IEnumerator Start()
		{
			if (GlobalSceneOptions.singleton != null && GlobalSceneOptions.singleton.enablePerfMonOnStart)
			{
				_on = true;
			}
			if (perfMonUI != null)
			{
				perfMonUI.gameObject.SetActive(_on);
			}
			if (perfMonUIAlt != null)
			{
				perfMonUIAlt.gameObject.SetActive(_on);
			}
			if (onToggle != null)
			{
				onToggle.isOn = _on;
				onToggle.onValueChanged.AddListener(delegate
				{
					on = onToggle.isOn;
				});
			}
			while (true)
			{
				yield return new WaitForEndOfFrame();
				DoUpdate();
			}
		}
	}
}
