using System;
using System.IO;
using NAudio.Wave;
using UnityEngine;

public static class NAudioPlayer
{
	public static WAV WAVFromMp3Data(byte[] data)
	{
		MemoryStream inputStream = new MemoryStream(data);
		Mp3FileReader sourceStream = new Mp3FileReader(inputStream);
		WaveStream waveStream = WaveFormatConversionStream.CreatePcmStream(sourceStream);
		if (waveStream.TotalTime.TotalSeconds < 1200.0)
		{
			return new WAV(AudioMemStream(waveStream).ToArray());
		}
		Debug.LogError("MP3 is too long (> 20mins) to convert to WAV as it will require too much memory and crash Unity.");
		return null;
	}

	public static AudioClip AudioClipFromWAV(WAV wav)
	{
		int channelCount = wav.ChannelCount;
		AudioClip audioClip;
		if (channelCount == 1)
		{
			audioClip = AudioClip.Create("testSound", wav.SampleCount, 1, wav.Frequency, false);
			audioClip.SetData(wav.LeftChannel, 0);
		}
		else
		{
			audioClip = AudioClip.Create("testSound", wav.SampleCount, 2, wav.Frequency, false);
			float[] array = new float[wav.LeftChannel.Length + wav.RightChannel.Length];
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			while (num < array.Length)
			{
				array[num] = wav.LeftChannel[num2];
				num2++;
				num++;
				array[num] = wav.RightChannel[num3];
				num3++;
				num++;
			}
			audioClip.SetData(array, 0);
		}
		return audioClip;
	}

	public static AudioClip AudioClipFromMp3Data(byte[] data)
	{
		WAV wAV = WAVFromMp3Data(data);
		if (wAV != null)
		{
			return AudioClipFromWAV(wAV);
		}
		return null;
	}

	private static MemoryStream AudioMemStream(WaveStream waveStream)
	{
		MemoryStream memoryStream = new MemoryStream();
		using (WaveFileWriter waveFileWriter = new WaveFileWriter(memoryStream, waveStream.WaveFormat))
		{
			byte[] array = new byte[waveStream.Length];
			waveStream.Position = 0L;
			waveStream.Read(array, 0, Convert.ToInt32(waveStream.Length));
			waveFileWriter.Write(array, 0, array.Length);
			waveFileWriter.Flush();
			return memoryStream;
		}
	}
}
