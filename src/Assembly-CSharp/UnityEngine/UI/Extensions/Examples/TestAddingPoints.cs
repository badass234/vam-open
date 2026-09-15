using System.Collections.Generic;

namespace UnityEngine.UI.Extensions.Examples
{
	public class TestAddingPoints : MonoBehaviour
	{
		public UILineRenderer LineRenderer;

		public Text XValue;

		public Text YValue;

		public void AddNewPoint()
		{
			Vector2 vector = default(Vector2);
			vector.x = float.Parse(XValue.text);
			vector.y = float.Parse(YValue.text);
			Vector2 item = vector;
			List<Vector2> list = new List<Vector2>(LineRenderer.Points);
			list.Add(item);
			LineRenderer.Points = list.ToArray();
		}

		public void ClearPoints()
		{
			LineRenderer.Points = new Vector2[0];
		}
	}
}
