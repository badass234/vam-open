public class WAV
{
	public float[] LeftChannel { get; internal set; }

	public float[] RightChannel { get; internal set; }

	public int ChannelCount { get; internal set; }

	public int SampleCount { get; internal set; }

	public int Frequency { get; internal set; }

	public WAV(byte[] wav)
	{
		ChannelCount = wav[22];
		Frequency = bytesToInt(wav, 24);
		int num = 12;
		while (wav[num] != 100 || wav[num + 1] != 97 || wav[num + 2] != 116 || wav[num + 3] != 97)
		{
			num += 4;
			int num2 = wav[num] + wav[num + 1] * 256 + wav[num + 2] * 65536 + wav[num + 3] * 16777216;
			num += 4 + num2;
		}
		num += 8;
		SampleCount = (wav.Length - num) / 2;
		if (ChannelCount == 2)
		{
			SampleCount /= 2;
		}
		LeftChannel = new float[SampleCount];
		if (ChannelCount == 2)
		{
			RightChannel = new float[SampleCount];
		}
		else
		{
			RightChannel = null;
		}
		int num3 = 0;
		while (num < wav.Length)
		{
			LeftChannel[num3] = bytesToFloat(wav[num], wav[num + 1]);
			num += 2;
			if (ChannelCount == 2)
			{
				RightChannel[num3] = bytesToFloat(wav[num], wav[num + 1]);
				num += 2;
			}
			num3++;
		}
	}

	private static float bytesToFloat(byte firstByte, byte secondByte)
	{
		short num = (short)((secondByte << 8) | firstByte);
		return (float)num / 32768f;
	}

	private static int bytesToInt(byte[] bytes, int offset = 0)
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			num |= bytes[offset + i] << i * 8;
		}
		return num;
	}

	public override string ToString()
	{
		return $"[WAV: LeftChannel={LeftChannel}, RightChannel={RightChannel}, ChannelCount={ChannelCount}, SampleCount={SampleCount}, Frequency={Frequency}]";
	}
}
