using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace HelloMeow
{
	[AddComponentMenu("Audio/MobileImporter")]
	public class MobileImporter : AudioImporter
	{
		protected override IEnumerator Load(string uri)
		{
			using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(uri.ToString(), AudioType.MPEG))
			{
				AsyncOperation operation = request.Send();
				while (!operation.isDone)
				{
					yield return null;
					OnProgress(request.downloadProgress);
				}
				if (request.isNetworkError)
				{
					OnError(request.error);
					yield break;
				}
				AudioClip audioClip = DownloadHandlerAudioClip.GetContent(request);
				if (!(audioClip == null))
				{
					OnLoaded(audioClip);
				}
			}
		}

		protected override IEnumerator LoadStreaming(string uri, int initialLength)
		{
			Debug.LogWarning("MobileImporter does not support streaming.");
			yield return StartCoroutine(Load(uri));
		}
	}
}
