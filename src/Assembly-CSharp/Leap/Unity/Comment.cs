using UnityEngine;

namespace Leap.Unity
{
	public class Comment : MonoBehaviour
	{
		[TextArea]
		[SerializeField]
		protected string _comment;

		[SerializeField]
		[HideInInspector]
		protected bool _isEditing = true;

		public string text
		{
			get
			{
				return _comment;
			}
			set
			{
				_comment = value;
			}
		}
	}
}
