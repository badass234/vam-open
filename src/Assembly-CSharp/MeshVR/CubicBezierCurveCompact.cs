using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace MeshVR
{
	public class CubicBezierCurveCompact
	{
		public Transform transform;

		protected bool _draw;

		protected Color _drawColor;

		protected List<CubicBezierPointCompact> _points;

		protected bool _loop = true;

		protected int _curveSmooth = 10;

		public Material material;

		protected Material materialLocal;

		protected int[] indices;

		protected Vector3[] vertices;

		protected Mesh mesh;

		protected Vector3[] K;

		protected Vector3[] r;

		protected float[] a;

		protected float[] b;

		protected float[] c;

		public bool draw
		{
			get
			{
				return _draw;
			}
			set
			{
				if (_draw != value)
				{
					_draw = value;
				}
			}
		}

		public List<CubicBezierPointCompact> points
		{
			get
			{
				return _points;
			}
			set
			{
				_points = value;
				AutoComputeControlPoints();
				RegenerateMesh();
			}
		}

		public bool loop
		{
			get
			{
				return _loop;
			}
			set
			{
				if (_loop != value)
				{
					_loop = value;
					SyncLoop();
				}
			}
		}

		public int curveSmooth
		{
			get
			{
				return _curveSmooth;
			}
			set
			{
				if (_curveSmooth != value)
				{
					_curveSmooth = value;
					RegenerateMesh();
				}
			}
		}

		public JSONClass GetJSON()
		{
			return new JSONClass();
		}

		public void RestoreFromJSON(JSONClass jc)
		{
		}

		public void SetDrawColor(Color c)
		{
			if (_drawColor.r != c.r || _drawColor.g != c.g || _drawColor.b != c.b)
			{
				_drawColor.r = c.r;
				_drawColor.g = c.g;
				_drawColor.b = c.b;
				materialLocal.color = _drawColor;
			}
		}

		public CubicBezierPointCompact CreatePoint()
		{
			CubicBezierPointCompact cubicBezierPointCompact = new CubicBezierPointCompact();
			cubicBezierPointCompact.parent = this;
			return cubicBezierPointCompact;
		}

		public void AddPointAt(int index, CubicBezierPointCompact point, bool updateControlPoints = true)
		{
			if (index < points.Count)
			{
				points.Insert(index, point);
			}
			if (updateControlPoints)
			{
				AutoComputeControlPoints();
			}
		}

		public void AddPoint(CubicBezierPointCompact point, bool updateControlPoints = true)
		{
			points.Add(point);
			if (updateControlPoints)
			{
				AutoComputeControlPoints();
			}
		}

		public void RemovePointAt(int index, bool updateControlPoints = true)
		{
			points.RemoveAt(index);
			if (updateControlPoints)
			{
				AutoComputeControlPoints();
			}
		}

		protected void SyncLoop()
		{
			RegenerateMesh();
		}

		protected void AutoComputeControlPoints()
		{
			if (_points == null || _points.Count == 0)
			{
				return;
			}
			int count = _points.Count;
			switch (count)
			{
			case 1:
				_points[0].controlPointIn = _points[0].position;
				_points[0].controlPointOut = _points[0].position;
				return;
			case 2:
				if (!_loop)
				{
					_points[0].controlPointIn = _points[0].position;
					_points[0].controlPointOut = _points[0].position;
					_points[1].controlPointIn = _points[1].position;
					_points[1].controlPointOut = _points[1].position;
					return;
				}
				break;
			}
			int num = ((!_loop) ? (count - 1) : (count + 1));
			if (K == null || K.Length < num + 1)
			{
				K = new Vector3[num + 1];
			}
			if (_loop)
			{
				K[0] = _points[count - 1].position;
				for (int i = 1; i < num; i++)
				{
					K[i] = _points[i - 1].position;
				}
				K[num] = _points[0].position;
			}
			else
			{
				for (int j = 0; j < count; j++)
				{
					K[j] = _points[j].position;
				}
			}
			if (a == null || a.Length < num)
			{
				a = new float[num];
			}
			if (b == null || b.Length < num)
			{
				b = new float[num];
			}
			if (c == null || c.Length < num)
			{
				c = new float[num];
			}
			if (r == null || r.Length < num)
			{
				r = new Vector3[num];
			}
			a[0] = 0f;
			b[0] = 2f;
			c[0] = 1f;
			r[0] = K[0] + 2f * K[1];
			for (int k = 1; k < num - 1; k++)
			{
				a[k] = 1f;
				b[k] = 4f;
				c[k] = 1f;
				r[k] = 4f * K[k] + 2f * K[k + 1];
			}
			a[num - 1] = 2f;
			b[num - 1] = 7f;
			c[num - 1] = 0f;
			r[num - 1] = 8f * K[num - 1] + K[num];
			for (int l = 1; l < num; l++)
			{
				float num2 = a[l] / b[l - 1];
				b[l] -= num2 * c[l - 1];
				r[l] = r[l] - num2 * r[l - 1];
			}
			if (_loop)
			{
				Vector3 vector = r[num - 1] / b[num - 1];
				_points[num - 2].controlPointOut = (r[num - 1] - c[num - 1] * vector) / b[num - 1];
				for (int num3 = num - 3; num3 >= 0; num3--)
				{
					_points[num3].controlPointOut = (r[num3 + 1] - c[num3 + 1] * _points[num3 + 1].controlPointOut) / b[num3 + 1];
				}
			}
			else
			{
				_points[num].controlPointOut = _points[num].position;
				_points[num - 1].controlPointOut = r[num - 1] / b[num - 1];
				for (int num4 = num - 2; num4 >= 0; num4--)
				{
					_points[num4].controlPointOut = (r[num4] - c[num4] * _points[num4 + 1].controlPointOut) / b[num4];
				}
			}
			if (_loop)
			{
				for (int m = 0; m < num - 1; m++)
				{
					_points[m].controlPointIn = 2f * K[m + 1] - _points[m].controlPointOut;
				}
				return;
			}
			_points[0].controlPointIn = _points[0].position;
			for (int n = 1; n < num; n++)
			{
				_points[n].controlPointIn = 2f * K[n] - _points[n].controlPointOut;
			}
			_points[num].controlPointIn = 0.5f * (K[num] + _points[num - 1].controlPointOut);
		}

		public void RegenerateMesh()
		{
			if (_points != null && _points.Count > 1)
			{
				int num = _points.Count - 1;
				if (_loop)
				{
					num++;
				}
				mesh = new Mesh();
				indices = new int[num * _curveSmooth];
				vertices = new Vector3[num * _curveSmooth];
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					for (int j = 0; j < _curveSmooth; j++)
					{
						indices[num2] = num2;
						num2++;
					}
				}
				mesh.vertices = vertices;
				mesh.SetIndices(indices, MeshTopology.LineStrip, 0);
			}
			else
			{
				mesh = new Mesh();
			}
		}

		protected void UpdateMesh()
		{
			if (!(mesh != null) || _points.Count <= 1)
			{
				return;
			}
			int num = _points.Count - 1;
			if (_loop)
			{
				num++;
			}
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				Vector3 position = _points[i].position;
				Vector3 controlPointOut = _points[i].controlPointOut;
				Vector3 controlPointIn;
				Vector3 position2;
				if (_loop && i == _points.Count - 1)
				{
					controlPointIn = _points[0].controlPointIn;
					position2 = _points[0].position;
				}
				else
				{
					controlPointIn = _points[i + 1].controlPointIn;
					position2 = _points[i + 1].position;
				}
				for (int j = 0; j < _curveSmooth; j++)
				{
					float num3 = (float)j * 1f / (float)(_curveSmooth - 1);
					float num4 = 1f - num3;
					float num5 = num4 * num4;
					float num6 = num5 * num4;
					float num7 = num3 * num3;
					float num8 = num7 * num3;
					Vector3 position3 = position * num6 + 3f * controlPointOut * num5 * num3 + 3f * controlPointIn * num4 * num7 + position2 * num8;
					vertices[num2] = transform.TransformPoint(position3);
					num2++;
				}
			}
			mesh.vertices = vertices;
			mesh.RecalculateBounds();
		}

		protected void DrawMesh(GameObject gameObject)
		{
			if (mesh != null && material != null && _draw)
			{
				UpdateMesh();
				Matrix4x4 identity = Matrix4x4.identity;
				Graphics.DrawMesh(mesh, identity, materialLocal, gameObject.layer, null, 0, null, false, false);
			}
		}

		public Vector3 GetPositionFromPoint(int fromPoint, float t)
		{
			Vector3 position = _points[fromPoint].position;
			Vector3 controlPointOut = _points[fromPoint].controlPointOut;
			Vector3 position2;
			if (_points.Count == 1)
			{
				position2 = position;
			}
			else
			{
				Vector3 controlPointIn;
				Vector3 position3;
				if (fromPoint == _points.Count - 1)
				{
					if (!_loop)
					{
						return position;
					}
					controlPointIn = _points[0].controlPointIn;
					position3 = _points[0].position;
				}
				else
				{
					controlPointIn = _points[fromPoint + 1].controlPointIn;
					position3 = _points[fromPoint + 1].position;
				}
				float num = 1f - t;
				float num2 = num * num;
				float num3 = num2 * num;
				float num4 = t * t;
				float num5 = num4 * t;
				position2 = position * num3 + 3f * controlPointOut * num2 * t + 3f * controlPointIn * num * num4 + position3 * num5;
			}
			return transform.TransformPoint(position2);
		}

		public Quaternion GetRotationFromPoint(int fromPoint, float t)
		{
			Quaternion rotation = _points[fromPoint].rotation;
			Quaternion quaternion;
			if (_points.Count == 1)
			{
				quaternion = rotation;
			}
			else
			{
				Quaternion rotation2;
				if (fromPoint == _points.Count - 1)
				{
					if (!_loop)
					{
						return rotation;
					}
					rotation2 = _points[0].rotation;
				}
				else
				{
					rotation2 = _points[fromPoint + 1].rotation;
				}
				quaternion = Quaternion.Lerp(rotation, rotation2, t);
			}
			return transform.rotation * quaternion;
		}

		protected virtual void Init()
		{
			if (material != null)
			{
				materialLocal = Object.Instantiate(material);
				_drawColor = materialLocal.color;
			}
		}

		protected void Update(GameObject gameObject)
		{
			if (mesh == null)
			{
				RegenerateMesh();
			}
			DrawMesh(gameObject);
		}
	}
}
