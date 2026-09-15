using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[ExecuteInEditMode]
	public abstract class ObiCurve : MonoBehaviour
	{
		protected int arcLenghtSamples = 5;

		protected int minPoints = 4;

		protected int unusedPoints = 2;

		protected int pointStride = 1;

		[HideInInspector]
		public List<Vector3> controlPoints;

		[HideInInspector]
		[SerializeField]
		protected List<float> arcLengthTable;

		[HideInInspector]
		[SerializeField]
		protected float totalSplineLenght;

		[HideInInspector]
		[SerializeField]
		protected bool closed;

		public bool Closed
		{
			get
			{
				return closed;
			}
			set
			{
				SetClosed(value);
			}
		}

		public float Length => totalSplineLenght;

		public virtual void Awake()
		{
			if (controlPoints == null)
			{
				controlPoints = new List<Vector3>
				{
					Vector3.left,
					Vector3.zero,
					Vector3.right,
					Vector3.right * 2f
				};
			}
			if (arcLengthTable == null)
			{
				arcLengthTable = new List<float>();
				RecalculateSplineLenght(1E-05f, 7);
			}
		}

		protected abstract void SetClosed(bool closed);

		public abstract void DisplaceControlPoint(int index, Vector3 delta);

		public abstract int GetNumSpans();

		public float RecalculateSplineLenght(float acc, int maxevals)
		{
			totalSplineLenght = 0f;
			arcLengthTable.Clear();
			arcLengthTable.Add(0f);
			float num = 1f / (float)(arcLenghtSamples + 1);
			if (controlPoints.Count >= minPoints)
			{
				for (int i = 1; i < controlPoints.Count - unusedPoints; i += pointStride)
				{
					Vector3 vector = base.transform.TransformPoint(controlPoints[i - 1]);
					Vector3 vector2 = base.transform.TransformPoint(controlPoints[i]);
					Vector3 vector3 = base.transform.TransformPoint(controlPoints[i + 1]);
					Vector3 vector4 = base.transform.TransformPoint(controlPoints[i + 2]);
					for (int j = 0; j <= Mathf.Max(1, arcLenghtSamples); j++)
					{
						float num2 = (float)j * num;
						float num3 = (float)(j + 1) * num;
						float num4 = GaussLobattoIntegrationStep(vector, vector2, vector3, vector4, num2, num3, EvaluateFirstDerivative3D(vector, vector2, vector3, vector4, num2).magnitude, EvaluateFirstDerivative3D(vector, vector2, vector3, vector4, num3).magnitude, 0, maxevals, acc);
						totalSplineLenght += num4;
						arcLengthTable.Add(totalSplineLenght);
					}
				}
			}
			else
			{
				Debug.LogWarning("Catmull-Rom spline needs at least 4 control points to be defined.");
			}
			return totalSplineLenght;
		}

		private float GaussLobattoIntegrationStep(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float a, float b, float fa, float fb, int nevals, int maxevals, float acc)
		{
			if (nevals >= maxevals)
			{
				return 0f;
			}
			float num = Mathf.Sqrt(2f / 3f);
			float num2 = 1f / Mathf.Sqrt(5f);
			float num3 = (b - a) / 2f;
			float num4 = (a + b) / 2f;
			float num5 = num4 - num * num3;
			float num6 = num4 - num2 * num3;
			float num7 = num4 + num2 * num3;
			float num8 = num4 + num * num3;
			nevals += 5;
			float magnitude = EvaluateFirstDerivative3D(p1, p2, p3, p4, num5).magnitude;
			float magnitude2 = EvaluateFirstDerivative3D(p1, p2, p3, p4, num6).magnitude;
			float magnitude3 = EvaluateFirstDerivative3D(p1, p2, p3, p4, num4).magnitude;
			float magnitude4 = EvaluateFirstDerivative3D(p1, p2, p3, p4, num7).magnitude;
			float magnitude5 = EvaluateFirstDerivative3D(p1, p2, p3, p4, num8).magnitude;
			float num9 = num3 / 6f * (fa + fb + 5f * (magnitude2 + magnitude4));
			float num10 = num3 / 1470f * (77f * (fa + fb) + 432f * (magnitude + magnitude5) + 625f * (magnitude2 + magnitude4) + 672f * magnitude3);
			if (num9 - num10 < acc || num5 <= a || b <= num8)
			{
				if (!(num4 > a) || !(b > num4))
				{
					Debug.LogError("Spline integration reached an interval with no more machine numbers");
				}
				return num10;
			}
			return GaussLobattoIntegrationStep(p1, p2, p3, p4, a, num5, fa, magnitude, nevals, maxevals, acc) + GaussLobattoIntegrationStep(p1, p2, p3, p4, num5, num6, magnitude, magnitude2, nevals, maxevals, acc) + GaussLobattoIntegrationStep(p1, p2, p3, p4, num6, num4, magnitude2, magnitude3, nevals, maxevals, acc) + GaussLobattoIntegrationStep(p1, p2, p3, p4, num4, num7, magnitude3, magnitude4, nevals, maxevals, acc) + GaussLobattoIntegrationStep(p1, p2, p3, p4, num7, num8, magnitude4, magnitude5, nevals, maxevals, acc) + GaussLobattoIntegrationStep(p1, p2, p3, p4, num8, b, magnitude5, fb, nevals, maxevals, acc);
		}

		public float GetMuAtLenght(float length)
		{
			if (length <= 0f)
			{
				return 0f;
			}
			if (length >= totalSplineLenght)
			{
				return 1f;
			}
			int i;
			for (i = 1; i < arcLengthTable.Count && !(length < arcLengthTable[i]); i++)
			{
			}
			float num = (float)(i - 1) / (float)(arcLengthTable.Count - 1);
			float num2 = (float)i / (float)(arcLengthTable.Count - 1);
			float num3 = (length - arcLengthTable[i - 1]) / (arcLengthTable[i] - arcLengthTable[i - 1]);
			return num + (num2 - num) * num3;
		}

		public abstract int GetSpanControlPointForMu(float mu, out float spanMu);

		public Vector3 GetPositionAt(float mu)
		{
			if (controlPoints.Count >= minPoints)
			{
				if (!float.IsNaN(mu))
				{
					float spanMu;
					int spanControlPointForMu = GetSpanControlPointForMu(mu, out spanMu);
					return Evaluate3D(controlPoints[spanControlPointForMu], controlPoints[spanControlPointForMu + 1], controlPoints[spanControlPointForMu + 2], controlPoints[spanControlPointForMu + 3], spanMu);
				}
				return controlPoints[0];
			}
			if (controlPoints.Count >= 2)
			{
				if (!float.IsNaN(mu))
				{
					return Vector3.Lerp(controlPoints[0], controlPoints[controlPoints.Count - 1], mu);
				}
				return controlPoints[0];
			}
			if (controlPoints.Count == 1)
			{
				return controlPoints[0];
			}
			throw new InvalidOperationException("Cannot get position in Catmull-Rom spline because it has zero control points.");
		}

		public Vector3 GetFirstDerivativeAt(float mu)
		{
			if (controlPoints.Count >= minPoints)
			{
				if (!float.IsNaN(mu))
				{
					float spanMu;
					int spanControlPointForMu = GetSpanControlPointForMu(mu, out spanMu);
					return EvaluateFirstDerivative3D(controlPoints[spanControlPointForMu], controlPoints[spanControlPointForMu + 1], controlPoints[spanControlPointForMu + 2], controlPoints[spanControlPointForMu + 3], spanMu);
				}
				return controlPoints[controlPoints.Count - 1] - controlPoints[0];
			}
			if (controlPoints.Count >= 2)
			{
				return controlPoints[controlPoints.Count - 1] - controlPoints[0];
			}
			throw new InvalidOperationException("Cannot get tangent in Catmull-Rom spline because it has zero or one control points.");
		}

		public Vector3 GetSecondDerivativeAt(float mu)
		{
			if (controlPoints.Count >= minPoints)
			{
				if (!float.IsNaN(mu))
				{
					float spanMu;
					int spanControlPointForMu = GetSpanControlPointForMu(mu, out spanMu);
					return EvaluateSecondDerivative3D(controlPoints[spanControlPointForMu], controlPoints[spanControlPointForMu + 1], controlPoints[spanControlPointForMu + 2], controlPoints[spanControlPointForMu + 3], spanMu);
				}
				return Vector3.zero;
			}
			return Vector3.zero;
		}

		private Vector3 Evaluate3D(Vector3 y0, Vector3 y1, Vector3 y2, Vector3 y3, float mu)
		{
			return new Vector3(Evaluate1D(y0.x, y1.x, y2.x, y3.x, mu), Evaluate1D(y0.y, y1.y, y2.y, y3.y, mu), Evaluate1D(y0.z, y1.z, y2.z, y3.z, mu));
		}

		protected abstract float Evaluate1D(float y0, float y1, float y2, float y3, float mu);

		private Vector3 EvaluateFirstDerivative3D(Vector3 y0, Vector3 y1, Vector3 y2, Vector3 y3, float mu)
		{
			return new Vector3(EvaluateFirstDerivative1D(y0.x, y1.x, y2.x, y3.x, mu), EvaluateFirstDerivative1D(y0.y, y1.y, y2.y, y3.y, mu), EvaluateFirstDerivative1D(y0.z, y1.z, y2.z, y3.z, mu));
		}

		protected abstract float EvaluateFirstDerivative1D(float y0, float y1, float y2, float y3, float mu);

		private Vector3 EvaluateSecondDerivative3D(Vector3 y0, Vector3 y1, Vector3 y2, Vector3 y3, float mu)
		{
			return new Vector3(EvaluateSecondDerivative1D(y0.x, y1.x, y2.x, y3.x, mu), EvaluateSecondDerivative1D(y0.y, y1.y, y2.y, y3.y, mu), EvaluateSecondDerivative1D(y0.z, y1.z, y2.z, y3.z, mu));
		}

		protected abstract float EvaluateSecondDerivative1D(float y0, float y1, float y2, float y3, float mu);
	}
}
