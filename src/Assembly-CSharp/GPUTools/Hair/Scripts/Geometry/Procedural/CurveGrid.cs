using System;
using GPUTools.Common.Scripts.Utils;
using UnityEngine;

namespace GPUTools.Hair.Scripts.Geometry.Procedural
{
	[Serializable]
	public class CurveGrid
	{
		[SerializeField]
		public Vector3[] ControlPoints;

		[SerializeField]
		public int ControlSizeX;

		[SerializeField]
		public int ControlSizeY;

		[SerializeField]
		public int ViewSizeX;

		[SerializeField]
		public int ViewSizeY;

		public void GenerateControl()
		{
			ControlPoints = new Vector3[ControlSizeX * ControlSizeY];
			for (int i = 0; i < ControlSizeX; i++)
			{
				for (int j = 0; j < ControlSizeY; j++)
				{
					float x = (float)i / (float)ControlSizeX;
					float z = (float)j / (float)ControlSizeY;
					SetControl(i, j, new Vector3(x, 0f, z));
				}
			}
		}

		public void GenerateView()
		{
			for (int i = 0; i < ViewSizeX; i++)
			{
				for (int j = 0; j < ViewSizeY; j++)
				{
					float tX = (float)i / (float)ViewSizeX;
					float tY = (float)j / (float)ViewSizeY;
					GetSplinePoint(tX, tY);
				}
			}
		}

		public Vector3 GetSplinePoint(float tX, float tY)
		{
			int num = (int)(tX * (float)ControlSizeX);
			int x = Mathf.Max(0, num - 1);
			int x2 = Mathf.Min(num, ControlSizeX - 1);
			int x3 = Mathf.Min(num + 1, ControlSizeX - 1);
			int num2 = (int)(tY * (float)ControlSizeY);
			int y = Mathf.Max(0, num2 - 1);
			int y2 = Mathf.Min(num2, ControlSizeY - 1);
			int y3 = Mathf.Min(num2 + 1, ControlSizeY - 1);
			Vector3 control = GetControl(x, y);
			Vector3 control2 = GetControl(x2, y);
			Vector3 control3 = GetControl(x3, y);
			Vector3 control4 = GetControl(x, y2);
			Vector3 control5 = GetControl(x2, y2);
			Vector3 control6 = GetControl(x3, y2);
			Vector3 control7 = GetControl(x, y3);
			Vector3 control8 = GetControl(x2, y3);
			Vector3 control9 = GetControl(x3, y3);
			Vector3 p = (control + control2) * 0.5f;
			Vector3 p2 = (control2 + control3) * 0.5f;
			Vector3 p3 = (control4 + control5) * 0.5f;
			Vector3 p4 = (control5 + control6) * 0.5f;
			Vector3 p5 = (control7 + control8) * 0.5f;
			Vector3 p6 = (control8 + control9) * 0.5f;
			float num3 = 1f / (float)ControlSizeX;
			float t = tX % num3 * (float)ControlSizeX;
			Vector3 bezierPoint = CurveUtils.GetBezierPoint(p, control2, p2, t);
			Vector3 bezierPoint2 = CurveUtils.GetBezierPoint(p3, control5, p4, t);
			Vector3 bezierPoint3 = CurveUtils.GetBezierPoint(p5, control8, p6, t);
			Vector3 p7 = (bezierPoint + bezierPoint2) * 0.5f;
			Vector3 p8 = (bezierPoint3 + bezierPoint2) * 0.5f;
			float num4 = 1f / (float)ControlSizeY;
			float t2 = tY % num4 * (float)ControlSizeY;
			return CurveUtils.GetBezierPoint(p7, bezierPoint2, p8, t2);
		}

		public void SetControl(int x, int y, Vector3 value)
		{
			ControlPoints[x * ControlSizeY + y] = value;
		}

		public Vector3 GetControl(int x, int y)
		{
			return ControlPoints[x * ControlSizeY + y];
		}

		public void SetView(int x, int y, Vector3 value)
		{
			ControlPoints[x * ViewSizeY + y] = value;
		}

		public Vector3 GetView(int x, int y)
		{
			return ControlPoints[x * ViewSizeX + y];
		}
	}
}
