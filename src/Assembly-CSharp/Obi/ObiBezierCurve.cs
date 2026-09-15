using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[ExecuteInEditMode]
	public class ObiBezierCurve : ObiCurve
	{
		public enum BezierCPMode
		{
			Free,
			Aligned,
			Mirrored
		}

		[HideInInspector]
		public List<BezierCPMode> controlPointModes;

		[HideInInspector]
		public BezierCPMode lastOpenCPMode;

		[HideInInspector]
		public Vector3 lastOpenCP;

		public override void Awake()
		{
			minPoints = 4;
			unusedPoints = 2;
			pointStride = 3;
			base.Awake();
			if (controlPointModes == null)
			{
				controlPointModes = new List<BezierCPMode>
				{
					BezierCPMode.Free,
					BezierCPMode.Free
				};
			}
		}

		protected override void SetClosed(bool closed)
		{
			if (base.closed != closed)
			{
				if (!base.closed && closed)
				{
					lastOpenCP = controlPoints[0];
					lastOpenCPMode = controlPointModes[0];
					controlPoints[0] = controlPoints[controlPoints.Count - 1];
					controlPointModes[0] = controlPointModes[controlPointModes.Count - 1];
				}
				else
				{
					controlPoints[0] = lastOpenCP;
					controlPointModes[0] = lastOpenCPMode;
				}
				base.closed = closed;
				EnforceMode(0);
			}
		}

		public override int GetNumSpans()
		{
			return (controlPoints.Count + (controlPoints.Count - 4) / 3) / 4;
		}

		public bool IsHandle(int index)
		{
			return index % 3 != 0;
		}

		public int GetHandleControlPointIndex(int index)
		{
			if (index < 0 || index >= controlPoints.Count)
			{
				return -1;
			}
			if (index % 3 == 1)
			{
				return index - 1;
			}
			if (index % 3 == 2)
			{
				return index + 1;
			}
			return index;
		}

		public List<int> GetHandleIndicesForControlPoint(int index)
		{
			List<int> list = new List<int>();
			if (index < 0 || index >= controlPoints.Count)
			{
				return list;
			}
			if (!IsHandle(index))
			{
				if (closed)
				{
					if (index == 0)
					{
						list.Add(1);
						list.Add(controlPoints.Count - 2);
					}
					else if (index == controlPoints.Count - 1)
					{
						list.Add(1);
						list.Add(index - 1);
					}
					else
					{
						list.Add(index + 1);
						list.Add(index - 1);
					}
				}
				else
				{
					if (index > 0)
					{
						list.Add(index - 1);
					}
					if (index + 1 < controlPoints.Count)
					{
						list.Add(index + 1);
					}
				}
			}
			return list;
		}

		public override void DisplaceControlPoint(int index, Vector3 delta)
		{
			if (index < 0 || index >= controlPoints.Count)
			{
				return;
			}
			if (!IsHandle(index))
			{
				if (closed)
				{
					if (index == 0)
					{
						controlPoints[1] += delta;
						controlPoints[controlPoints.Count - 2] += delta;
						controlPoints[controlPoints.Count - 1] += delta;
					}
					else if (index == controlPoints.Count - 1)
					{
						controlPoints[0] += delta;
						controlPoints[1] += delta;
						controlPoints[index - 1] += delta;
					}
					else
					{
						controlPoints[index - 1] += delta;
						controlPoints[index + 1] += delta;
					}
				}
				else
				{
					if (index > 0)
					{
						controlPoints[index - 1] += delta;
					}
					if (index + 1 < controlPoints.Count)
					{
						controlPoints[index + 1] += delta;
					}
				}
			}
			controlPoints[index] += delta;
			EnforceMode(index);
		}

		public override int GetSpanControlPointForMu(float mu, out float spanMu)
		{
			int numSpans = GetNumSpans();
			spanMu = mu * (float)numSpans;
			int num = ((!(mu >= 1f)) ? ((int)spanMu) : (numSpans - 1));
			spanMu -= num;
			return num * 3;
		}

		public BezierCPMode GetControlPointMode(int index)
		{
			int index2 = (index + 1) / 3;
			return controlPointModes[index2];
		}

		public void SetControlPointMode(int index, BezierCPMode mode)
		{
			int num = (index + 1) / 3;
			controlPointModes[num] = mode;
			if (closed)
			{
				if (num == 0)
				{
					controlPointModes[controlPointModes.Count - 1] = mode;
				}
				else if (num == controlPointModes.Count - 1)
				{
					controlPointModes[0] = mode;
				}
			}
			EnforceMode(index);
		}

		public void EnforceMode(int index)
		{
			int num = (index + 1) / 3;
			BezierCPMode bezierCPMode = controlPointModes[num];
			if (bezierCPMode == BezierCPMode.Free || (!closed && (num == 0 || num == controlPointModes.Count - 1)))
			{
				return;
			}
			int num2 = num * 3;
			int num3;
			int num4;
			if (index <= num2)
			{
				num3 = num2 - 1;
				if (num3 < 0)
				{
					num3 = controlPoints.Count - 2;
				}
				num4 = num2 + 1;
				if (num4 >= controlPoints.Count)
				{
					num4 = 1;
				}
			}
			else
			{
				num3 = num2 + 1;
				if (num3 >= controlPoints.Count)
				{
					num3 = 1;
				}
				num4 = num2 - 1;
				if (num4 < 0)
				{
					num4 = controlPoints.Count - 2;
				}
			}
			Vector3 vector = controlPoints[num2];
			Vector3 vector2 = vector - controlPoints[num3];
			if (bezierCPMode == BezierCPMode.Aligned)
			{
				vector2 = vector2.normalized * Vector3.Distance(vector, controlPoints[num4]);
			}
			controlPoints[num4] = vector + vector2;
		}

		public void AddSpan()
		{
			int index = controlPoints.Count - 1;
			Vector3 vector = controlPoints[index];
			controlPoints.Add(vector + Vector3.right * 0.5f);
			controlPoints.Add(vector + Vector3.right);
			controlPoints.Add(vector + Vector3.right * 1.5f);
			controlPointModes.Add(BezierCPMode.Free);
			EnforceMode(index);
			if (closed)
			{
				controlPoints[controlPoints.Count - 1] = controlPoints[0];
				controlPointModes[controlPointModes.Count - 1] = controlPointModes[0];
				EnforceMode(0);
			}
		}

		public void RemoveCurvePoint(int curvePoint)
		{
			if (controlPoints.Count <= 4)
			{
				return;
			}
			int num = Mathf.Max(0, curvePoint * 3 - 1);
			int count = 3;
			if (num == controlPoints.Count - 2)
			{
				num--;
			}
			controlPoints.RemoveRange(num, count);
			controlPointModes.RemoveAt(curvePoint);
			if (closed)
			{
				if (num == controlPoints.Count)
				{
					controlPoints[0] = controlPoints[controlPoints.Count - 1];
					controlPointModes[0] = controlPointModes[controlPointModes.Count - 1];
				}
				else if (num == 0)
				{
					controlPoints[controlPoints.Count - 1] = controlPoints[0];
					controlPointModes[controlPointModes.Count - 1] = controlPointModes[0];
				}
			}
			EnforceMode(num);
		}

		protected override float Evaluate1D(float y0, float y1, float y2, float y3, float mu)
		{
			float num = 1f - mu;
			return num * num * num * y0 + 3f * num * num * mu * y1 + 3f * num * mu * mu * y2 + mu * mu * mu * y3;
		}

		protected override float EvaluateFirstDerivative1D(float y0, float y1, float y2, float y3, float mu)
		{
			float num = 1f - mu;
			return 3f * num * num * (y1 - y0) + 6f * num * mu * (y2 - y1) + 3f * mu * mu * (y3 - y2);
		}

		protected override float EvaluateSecondDerivative1D(float y0, float y1, float y2, float y3, float mu)
		{
			float num = 1f - mu;
			return 3f * num * num * (y1 - y0) + 6f * num * mu * (y2 - y1) + 3f * mu * mu * (y3 - y2);
		}
	}
}
