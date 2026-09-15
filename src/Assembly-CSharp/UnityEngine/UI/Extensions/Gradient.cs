using System.Collections.Generic;

namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("UI/Effects/Extensions/Gradient")]
	public class Gradient : BaseMeshEffect
	{
		[SerializeField]
		private GradientMode _gradientMode;

		[SerializeField]
		private GradientDir _gradientDir;

		[SerializeField]
		private bool _overwriteAllColor;

		[SerializeField]
		private Color _vertex1 = Color.white;

		[SerializeField]
		private Color _vertex2 = Color.black;

		private Graphic targetGraphic;

		public GradientMode GradientMode
		{
			get
			{
				return _gradientMode;
			}
			set
			{
				_gradientMode = value;
				base.graphic.SetVerticesDirty();
			}
		}

		public GradientDir GradientDir
		{
			get
			{
				return _gradientDir;
			}
			set
			{
				_gradientDir = value;
				base.graphic.SetVerticesDirty();
			}
		}

		public bool OverwriteAllColor
		{
			get
			{
				return _overwriteAllColor;
			}
			set
			{
				_overwriteAllColor = value;
				base.graphic.SetVerticesDirty();
			}
		}

		public Color Vertex1
		{
			get
			{
				return _vertex1;
			}
			set
			{
				_vertex1 = value;
				base.graphic.SetAllDirty();
			}
		}

		public Color Vertex2
		{
			get
			{
				return _vertex2;
			}
			set
			{
				_vertex2 = value;
				base.graphic.SetAllDirty();
			}
		}

		protected override void Awake()
		{
			targetGraphic = GetComponent<Graphic>();
		}

		public override void ModifyMesh(VertexHelper vh)
		{
			int currentVertCount = vh.currentVertCount;
			if (!IsActive() || currentVertCount == 0)
			{
				return;
			}
			List<UIVertex> list = new List<UIVertex>();
			vh.GetUIVertexStream(list);
			UIVertex vertex = default(UIVertex);
			if (_gradientMode == GradientMode.Global)
			{
				if (_gradientDir == GradientDir.DiagonalLeftToRight || _gradientDir == GradientDir.DiagonalRightToLeft)
				{
					_gradientDir = GradientDir.Vertical;
				}
				float num = ((_gradientDir != 0) ? list[list.Count - 1].position.x : list[list.Count - 1].position.y);
				float num2 = ((_gradientDir != 0) ? list[0].position.x : list[0].position.y);
				float num3 = num2 - num;
				for (int i = 0; i < currentVertCount; i++)
				{
					vh.PopulateUIVertex(ref vertex, i);
					if (_overwriteAllColor || !(vertex.color != targetGraphic.color))
					{
						vertex.color *= Color.Lerp(_vertex2, _vertex1, (((_gradientDir != 0) ? vertex.position.x : vertex.position.y) - num) / num3);
						vh.SetUIVertex(vertex, i);
					}
				}
				return;
			}
			for (int j = 0; j < currentVertCount; j++)
			{
				vh.PopulateUIVertex(ref vertex, j);
				if (_overwriteAllColor || CompareCarefully(vertex.color, targetGraphic.color))
				{
					switch (_gradientDir)
					{
					case GradientDir.Vertical:
						vertex.color *= ((j % 4 != 0 && (j - 1) % 4 != 0) ? _vertex2 : _vertex1);
						break;
					case GradientDir.Horizontal:
						vertex.color *= ((j % 4 != 0 && (j - 3) % 4 != 0) ? _vertex2 : _vertex1);
						break;
					case GradientDir.DiagonalLeftToRight:
						vertex.color *= ((j % 4 == 0) ? _vertex1 : (((j - 2) % 4 != 0) ? Color.Lerp(_vertex2, _vertex1, 0.5f) : _vertex2));
						break;
					case GradientDir.DiagonalRightToLeft:
						vertex.color *= (((j - 1) % 4 == 0) ? _vertex1 : (((j - 3) % 4 != 0) ? Color.Lerp(_vertex2, _vertex1, 0.5f) : _vertex2));
						break;
					}
					vh.SetUIVertex(vertex, j);
				}
			}
		}

		private bool CompareCarefully(Color col1, Color col2)
		{
			if (Mathf.Abs(col1.r - col2.r) < 0.003f && Mathf.Abs(col1.g - col2.g) < 0.003f && Mathf.Abs(col1.b - col2.b) < 0.003f && Mathf.Abs(col1.a - col2.a) < 0.003f)
			{
				return true;
			}
			return false;
		}
	}
}
