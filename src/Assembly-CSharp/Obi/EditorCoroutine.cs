using System.Collections;

namespace Obi
{
	public class EditorCoroutine
	{
		private readonly IEnumerator routine;

		private object result;

		private bool isDone;

		public object Result => result;

		public bool IsDone => isDone;

		private EditorCoroutine(IEnumerator _routine)
		{
			routine = _routine;
		}

		public static EditorCoroutine StartCoroutine(IEnumerator _routine)
		{
			EditorCoroutine editorCoroutine = new EditorCoroutine(_routine);
			editorCoroutine.Start();
			return editorCoroutine;
		}

		private void Start()
		{
			isDone = false;
			result = null;
			Update();
		}

		public void Stop()
		{
			isDone = true;
		}

		private void Update()
		{
			bool flag = routine.MoveNext();
			result = routine.Current;
			if (!flag)
			{
				Stop();
			}
		}

		public static void ShowCoroutineProgressBar(string title, EditorCoroutine coroutine)
		{
		}
	}
}
