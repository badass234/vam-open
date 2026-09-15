using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using Un4seen.Bass;
using UnityEngine;

namespace HelloMeow
{
	[AddComponentMenu("Audio/Bass Importer")]
	public class BassImporter : DecoderImporter
	{
		private int handle = -1;

		private float[] offsetBuffer = new float[4096];

		protected IntPtr byteArray;

		private void Awake()
		{
			BassNet.Registration("meshedvr@gmail.com", "2X22233718152222");
			Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
			BASSError bASSError = Bass.BASS_ErrorGetCode();
			if (bASSError != 0 && bASSError != BASSError.BASS_ERROR_ALREADY)
			{
				OnError(bASSError.ToString());
			}
		}

		private void OnApplicationQuit()
		{
			Bass.BASS_Free();
		}

		protected override AudioInfo GetInfo()
		{
			BASS_CHANNELINFO bASS_CHANNELINFO = Bass.BASS_ChannelGetInfo(handle);
			int lengthSamples = (int)Bass.BASS_ChannelGetLength(handle) / 4;
			return new AudioInfo(lengthSamples, bASS_CHANNELINFO.freq, bASS_CHANNELINFO.chans);
		}

		protected override float GetProgress()
		{
			float num = (float)Bass.BASS_ChannelGetPosition(handle, BASSMode.BASS_POS_BYTE) / 4f;
			return num / (float)base.info.lengthSamples;
		}

		protected override int GetSamples(float[] buffer, int offset, int count)
		{
			if (offset == 0)
			{
				return Bass.BASS_ChannelGetData(handle, buffer, count * 4) / 4;
			}
			if (offsetBuffer.Length != count)
			{
				offsetBuffer = new float[count];
			}
			int num = Bass.BASS_ChannelGetData(handle, offsetBuffer, Mathf.Min(count, buffer.Length - offset) * 4);
			if (num != -1 && num != 0)
			{
				num /= 4;
				Array.Copy(offsetBuffer, 0, buffer, offset, num);
			}
			return num;
		}

		protected override IEnumerator Initialize(string uri)
		{
			Cleanup();
			Thread loadThread = new Thread((ThreadStart)delegate
			{
				LoadChannel(uri);
			});
			loadThread.Start();
			while (loadThread.IsAlive)
			{
				yield return null;
			}
			if (handle == 0)
			{
				OnError("Could not open: " + uri);
			}
		}

		protected override void Cleanup()
		{
			if (handle != -1)
			{
				Bass.BASS_StreamFree(handle);
			}
		}

		private void LoadChannel(string uri)
		{
			if (uri.StartsWith("file://"))
			{
				handle = Bass.BASS_StreamCreateFile(uri.Substring(7), 0L, 0L, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE);
			}
			else
			{
				handle = Bass.BASS_StreamCreateURL(uri, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE, null, default(IntPtr));
			}
		}

		public IEnumerator SetData(IntPtr byteArray, long length)
		{
			handle = Bass.BASS_StreamCreateFile(byteArray, 0L, length, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE);
			if (handle == 0)
			{
				OnError("Could not decode mp3 bytes");
				yield break;
			}
			base.info = GetInfo();
			yield return StartCoroutine(LoadAudioClip(base.info.lengthSamples));
			Marshal.FreeHGlobal(byteArray);
			Cleanup();
		}

		public IEnumerator SetData(byte[] bytes)
		{
			IntPtr byteArray = Marshal.AllocHGlobal(bytes.Length);
			Marshal.Copy(bytes, 0, byteArray, bytes.Length);
			handle = Bass.BASS_StreamCreateFile(byteArray, 0L, bytes.Length, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE);
			if (handle == 0)
			{
				OnError("Could not decode mp3 bytes");
				yield break;
			}
			base.info = GetInfo();
			yield return StartCoroutine(LoadAudioClip(base.info.lengthSamples));
			Marshal.FreeHGlobal(byteArray);
			Cleanup();
		}
	}
}
