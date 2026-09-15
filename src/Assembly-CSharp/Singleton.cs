using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	private static object _lock = new object();

	private static bool applicationIsQuitting = false;

	public static T Instance
	{
		get
		{
			if (applicationIsQuitting)
			{
				return (T)null;
			}
			lock (_lock)
			{
				if (_instance == null)
				{
					_instance = (T)Object.FindObjectOfType(typeof(T));
					if (Object.FindObjectsOfType(typeof(T)).Length > 1)
					{
						return _instance;
					}
					if (_instance == null)
					{
						GameObject gameObject = new GameObject();
						_instance = gameObject.AddComponent<T>();
						gameObject.name = "(singleton) " + typeof(T).ToString();
						Object.DontDestroyOnLoad(gameObject);
					}
				}
				return _instance;
			}
		}
	}

	public void OnDestroy()
	{
		applicationIsQuitting = true;
	}
}
