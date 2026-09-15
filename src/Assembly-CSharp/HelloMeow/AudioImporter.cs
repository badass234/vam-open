using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace HelloMeow
{
	public abstract class AudioImporter : MonoBehaviour
	{
		private ImportOperation operation;

		public string uri { get; private set; }

		public AudioClip audioClip { get; private set; }

		public float progress { get; private set; }

		public bool isLoaded { get; private set; }

		public bool isError { get; private set; }

		public string error { get; private set; }

		public event Action<AudioClip> Loaded;

		public event Action<float> Progress;

		public event Action<string> Error;

		protected AudioImporter()
		{
			operation = new ImportOperation(this);
		}

		public ImportOperation Import(string uri)
		{
			Cleanup();
			this.uri = GetUri(uri);
			StartCoroutine(Load(this.uri));
			return operation;
		}

		public ImportOperation ImportStreaming(string uri, int initialLength = 0)
		{
			Cleanup();
			this.uri = GetUri(uri);
			initialLength = Mathf.Max(1, initialLength);
			StartCoroutine(LoadStreaming(this.uri, initialLength));
			return operation;
		}

		protected virtual IEnumerator Load(string uri)
		{
			yield return null;
		}

		protected virtual IEnumerator LoadStreaming(string uri, int initialLength)
		{
			yield return null;
		}

		protected virtual string GetName()
		{
			return Path.GetFileNameWithoutExtension(uri);
		}

		private string GetUri(string uri)
		{
			if (uri.StartsWith("file://") || uri.StartsWith("http://") || uri.StartsWith("https://"))
			{
				return uri;
			}
			return "file://" + uri;
		}

		public void Prep()
		{
			Cleanup();
		}

		private void Cleanup()
		{
			StopAllCoroutines();
			uri = null;
			audioClip = null;
			isLoaded = false;
			isError = false;
			error = null;
			progress = 0f;
		}

		protected void OnLoaded(AudioClip audioClip)
		{
			audioClip.name = GetName();
			this.audioClip = audioClip;
			isLoaded = true;
			if (this.Loaded != null)
			{
				this.Loaded(audioClip);
			}
		}

		protected void OnProgress(float progress)
		{
			if (this.progress != progress)
			{
				this.progress = progress;
				if (this.Progress != null)
				{
					this.Progress(progress);
				}
			}
		}

		protected void OnError(string error)
		{
			isError = true;
			this.error = error;
			Debug.LogError(error);
			if (this.Error != null)
			{
				this.Error(error);
			}
		}
	}
}
