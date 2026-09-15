using UnityEngine;

namespace Obi
{
	[ExecuteInEditMode]
	public class ObiCatmullRomCurve : ObiCurve
	{
		[HideInInspector]
		public Vector3 lastOpenCP0;

		[HideInInspector]
		public Vector3 lastOpenCP1;

		[HideInInspector]
		public Vector3 lastOpenCPN;

		public override void Awake()
		{
			minPoints = 4;
			unusedPoints = 2;
			base.Awake();
		}

		protected override void SetClosed(bool closed)
		{
			if (base.closed != closed)
			{
				if (!base.closed && closed)
				{
					lastOpenCP0 = controlPoints[0];
					lastOpenCP1 = controlPoints[1];
					lastOpenCPN = controlPoints[controlPoints.Count - 1];
					controlPoints[0] = controlPoints[controlPoints.Count - 3];
					controlPoints[1] = controlPoints[controlPoints.Count - 2];
					controlPoints[controlPoints.Count - 1] = controlPoints[2];
				}
				else
				{
					controlPoints[0] = lastOpenCP0;
					controlPoints[1] = lastOpenCP1;
					controlPoints[controlPoints.Count - 1] = lastOpenCPN;
				}
				base.closed = closed;
			}
		}

		public override void DisplaceControlPoint(int index, Vector3 delta)
		{
			if (index < 0 || index >= controlPoints.Count)
			{
				return;
			}
			if (closed)
			{
				if (index == 0 || index == controlPoints.Count - 3)
				{
					controlPoints[0] += delta;
					controlPoints[controlPoints.Count - 3] += delta;
				}
				else if (index == 1 || index == controlPoints.Count - 2)
				{
					controlPoints[1] += delta;
					controlPoints[controlPoints.Count - 2] += delta;
				}
				else if (index == 2 || index == controlPoints.Count - 1)
				{
					controlPoints[2] += delta;
					controlPoints[controlPoints.Count - 1] += delta;
				}
				else
				{
					controlPoints[index] += delta;
				}
			}
			else
			{
				controlPoints[index] += delta;
			}
		}

		public override int GetNumSpans()
		{
			return controlPoints.Count - unusedPoints - 1;
		}

		public override int GetSpanControlPointForMu(float mu, out float spanMu)
		{
			int numSpans = GetNumSpans();
			spanMu = mu * (float)numSpans;
			int num = ((!(mu >= 1f)) ? ((int)spanMu) : (numSpans - 1));
			spanMu -= num;
			return num;
		}

		protected override float Evaluate1D(float y0, float y1, float y2, float y3, float mu)
		{
			float num = mu * mu;
			float num2 = -0.5f * y0 + 1.5f * y1 - 1.5f * y2 + 0.5f * y3;
			float num3 = y0 - 2.5f * y1 + 2f * y2 - 0.5f * y3;
			float num4 = -0.5f * y0 + 0.5f * y2;
			return num2 * mu * num + num3 * num + num4 * mu + y1;
		}

		protected override float EvaluateFirstDerivative1D(float y0, float y1, float y2, float y3, float mu)
		{
			float num = mu * mu;
			float num2 = -0.5f * y0 + 1.5f * y1 - 1.5f * y2 + 0.5f * y3;
			float num3 = y0 - 2.5f * y1 + 2f * y2 - 0.5f * y3;
			float num4 = -0.5f * y0 + 0.5f * y2;
			return 3f * num2 * num + 2f * num3 * mu + num4;
		}

		protected override float EvaluateSecondDerivative1D(float y0, float y1, float y2, float y3, float mu)
		{
			float num = -0.5f * y0 + 1.5f * y1 - 1.5f * y2 + 0.5f * y3;
			float num2 = y0 - 2.5f * y1 + 2f * y2 - 0.5f * y3;
			return 6f * num * mu + 2f * num2;
		}
	}
}
