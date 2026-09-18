namespace UnityEngine.UI.Extensions
{
	[AddComponentMenu("Layout/Extensions/NonDrawingGraphic")]
	// 2020.1 removed the RequireComponent(CanvasRenderer) attribute from UnityEngine.UI.Graphic,
	// so every subclass has to declare it to keep dragging the component in adding a CanvasRenderer.
	[RequireComponent(typeof(CanvasRenderer))]
	public class NonDrawingGraphic : MaskableGraphic
	{
		public override void SetMaterialDirty()
		{
		}

		public override void SetVerticesDirty()
		{
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();
		}
	}
}
