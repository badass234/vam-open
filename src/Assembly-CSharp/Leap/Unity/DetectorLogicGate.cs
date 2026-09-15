using System.Collections.Generic;
using UnityEngine;

namespace Leap.Unity
{
	public class DetectorLogicGate : Detector
	{
		[SerializeField]
		[Tooltip("The list of observed detectors.")]
		private List<Detector> Detectors;

		[Tooltip("Add all detectors on this object automatically.")]
		public bool AddAllSiblingDetectorsOnAwake = true;

		[Tooltip("The type of logic used to combine detector state.")]
		public LogicType GateType;

		[Tooltip("Whether to negate the gate output.")]
		public bool Negate;

		public void AddDetector(Detector detector)
		{
			if (!Detectors.Contains(detector))
			{
				Detectors.Add(detector);
				activateDetector(detector);
			}
		}

		public void RemoveDetector(Detector detector)
		{
			detector.OnActivate.RemoveListener(CheckDetectors);
			detector.OnDeactivate.RemoveListener(CheckDetectors);
			Detectors.Remove(detector);
		}

		public void AddAllSiblingDetectors()
		{
			Detector[] components = GetComponents<Detector>();
			for (int i = 0; i < components.Length; i++)
			{
				if (components[i] != this && components[i].enabled)
				{
					AddDetector(components[i]);
				}
			}
		}

		private void Awake()
		{
			for (int i = 0; i < Detectors.Count; i++)
			{
				activateDetector(Detectors[i]);
			}
			if (AddAllSiblingDetectorsOnAwake)
			{
				AddAllSiblingDetectors();
			}
		}

		private void activateDetector(Detector detector)
		{
			detector.OnActivate.RemoveListener(CheckDetectors);
			detector.OnDeactivate.RemoveListener(CheckDetectors);
			detector.OnActivate.AddListener(CheckDetectors);
			detector.OnDeactivate.AddListener(CheckDetectors);
		}

		private void OnEnable()
		{
			CheckDetectors();
		}

		private void OnDisable()
		{
			Deactivate();
		}

		protected void CheckDetectors()
		{
			if (Detectors.Count >= 1)
			{
				bool flag = Detectors[0].IsActive;
				for (int i = 1; i < Detectors.Count; i++)
				{
					flag = ((GateType != 0) ? (flag || Detectors[i].IsActive) : (flag && Detectors[i].IsActive));
				}
				if (Negate)
				{
					flag = !flag;
				}
				if (flag)
				{
					Activate();
				}
				else
				{
					Deactivate();
				}
			}
		}
	}
}
