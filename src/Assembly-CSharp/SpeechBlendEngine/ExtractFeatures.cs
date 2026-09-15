using System;
using UnityEngine;

namespace SpeechBlendEngine
{
	public class ExtractFeatures
	{
		public class FeatureOutput
		{
			public int[] reg;

			public float[] w;

			public int size;

			public FeatureOutput(int sz)
			{
				reg = new int[sz];
				w = new float[sz];
				size = sz;
			}
		}

		private struct EuclideanDistance
		{
			public float distance;

			public int number;
		}

		public static int no_visemes = 16;

		private static int[] M_FB = new int[3] { 14, 20, 30 };

		private static int[] C = new int[3] { 10, 12, 16 };

		private static int L = 22;

		private static int[] lf = new int[3] { 400, 300, 200 };

		private static int[] hf = new int[3] { 4200, 4500, 5000 };

		private static int[] no_phonemes = new int[3] { 16, 32, 64 };

		private static int N_freq_bins = 4096;

		private static float fs = 48000f;

		public static int NI = 4;

		private static float ISF = 0.2f;

		private float[] spectrumData;

		private float[] getData;

		private float[] soundData;

		private LomontFFT lomontFFT;

		private double[] lomontFFTData;

		private float[] FB_energy_log;

		private float[] CCnumArray;

		private float[] CCcc;

		private float[] DeltaCCNumArray;

		private EuclideanDistance[] euclDistanceArray;

		private int[] shortest;

		public FeatureOutput featureOutput { get; protected set; }

		public ExtractFeatures()
		{
			featureOutput = new FeatureOutput(NI);
		}

		public static float CalculateFres()
		{
			return (float)((double)fs / (double)N_freq_bins / 2.0);
		}

		private static float[] MeanNormalization(float[] spectrum)
		{
			float num = 0f;
			for (int i = 0; i < spectrum.Length; i++)
			{
				num += spectrum[i];
			}
			for (int j = 0; j < spectrum.Length; j++)
			{
				spectrum[j] /= num;
			}
			return spectrum;
		}

		public static float[] CreateCC_lifter(SpeechUtil.Accuracy accuracy)
		{
			int num = C[(int)accuracy] + 1;
			float[] array = new float[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = (float)(1.0 + 0.5 * (double)L * (double)Mathf.Sin((float)Math.PI * (float)i / (float)L));
			}
			return array;
		}

		public static float[,] BuildExtractor(float frequency_resolution, int LF, int HF, SpeechUtil.Accuracy accuracy)
		{
			float[] array = new float[N_freq_bins];
			for (int i = 0; i < N_freq_bins; i++)
			{
				array[i] = (float)i * frequency_resolution;
			}
			float num = fs / 2f;
			float[] array2 = new float[N_freq_bins];
			for (int j = 0; j < N_freq_bins; j++)
			{
				array2[j] = (float)j * num / (float)N_freq_bins;
			}
			int[] array3 = new int[M_FB[(int)accuracy] + 2];
			float num2 = (float)(((double)Hertz2Mel(HF) - (double)Hertz2Mel(LF)) / ((double)M_FB[(int)accuracy] + 1.0));
			for (int k = 0; k < M_FB[(int)accuracy] + 2; k++)
			{
				float f = Mel2Hertz(Hertz2Mel(LF) + (float)k * num2);
				array3[k] = Mathf.RoundToInt(f);
			}
			float[,] array4 = new float[M_FB[(int)accuracy], N_freq_bins];
			for (int l = 0; l < M_FB[(int)accuracy]; l++)
			{
				for (int m = 0; m < N_freq_bins; m++)
				{
					array4[l, m] = 0f;
				}
			}
			for (int n = 0; n < M_FB[(int)accuracy]; n++)
			{
				for (int num3 = 0; num3 < N_freq_bins; num3++)
				{
					if (((double)array2[num3] > (double)array3[n]) & ((double)array2[num3] < (double)array3[n + 1]))
					{
						array4[n, num3] = (float)(((double)array2[num3] - (double)array3[n]) / ((double)array3[n + 1] - (double)array3[n]));
					}
					else if (((double)array2[num3] > (double)array3[n + 1]) & ((double)array2[num3] < (double)array3[n + 2]))
					{
						array4[n, num3] = (float)(((double)array3[n + 2] - (double)array2[num3]) / ((double)array3[n + 2] - (double)array3[n + 1]));
					}
				}
			}
			return array4;
		}

		public static float Mel2Hertz(float mel)
		{
			return (float)(700.0 * (double)Mathf.Exp(mel / 1127f) - 700.0);
		}

		public static float Hertz2Mel(float hertz)
		{
			return 1127f * Mathf.Log((float)(1.0 + (double)hertz / 700.0));
		}

		public static float[,] GenerateTransformer(SpeechUtil.Accuracy accuracy)
		{
			int num = getC(accuracy) + 1;
			float[,] array = new float[num, M_FB[(int)accuracy]];
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < M_FB[(int)accuracy]; j++)
				{
					float num2 = i;
					float num3 = (float)(3.1415927410125732 * ((double)((float)j + 1f) - 0.5)) / (float)M_FB[(int)accuracy];
					array[i, j] = Mathf.Sqrt(2f / (float)M_FB[(int)accuracy]) * Mathf.Cos(num2 * num3);
				}
			}
			return array;
		}

		public static int getC(SpeechUtil.Accuracy accuracy)
		{
			return C[(int)accuracy];
		}

		public static int getlf(SpeechUtil.Accuracy accuracy)
		{
			return lf[(int)accuracy];
		}

		public static int gethf(SpeechUtil.Accuracy accuracy)
		{
			return hf[(int)accuracy];
		}

		public static float EqualizeDistance(float volume, AudioSource voiceAudioSource, AudioListener activeListener)
		{
			if ((double)voiceAudioSource.spatialBlend == 0.0)
			{
				return volume;
			}
			if (activeListener == null)
			{
				activeListener = Camera.allCameras[0].gameObject.GetComponent<AudioListener>();
			}
			float num = Vector3.Distance(activeListener.transform.position, voiceAudioSource.transform.position);
			float num2 = 0f;
			switch (voiceAudioSource.rolloffMode)
			{
			case AudioRolloffMode.Logarithmic:
			{
				if (num <= voiceAudioSource.minDistance || num == 0f)
				{
					num2 = 1f;
					break;
				}
				float num3 = voiceAudioSource.minDistance / num;
				num2 = num3;
				break;
			}
			case AudioRolloffMode.Linear:
				num2 = ((!(num <= voiceAudioSource.minDistance) && num != 0f) ? ((!(num > voiceAudioSource.maxDistance)) ? Mathf.Lerp(1f, 0f, (num - voiceAudioSource.minDistance) / (voiceAudioSource.maxDistance - voiceAudioSource.minDistance)) : 0f) : 1f);
				break;
			case AudioRolloffMode.Custom:
				num2 = ((!(num <= voiceAudioSource.minDistance) && num != 0f) ? ((!(num > voiceAudioSource.maxDistance)) ? voiceAudioSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff).Evaluate(num / voiceAudioSource.maxDistance) : 0f) : 1f);
				break;
			}
			if (num2 == 1f)
			{
				return volume;
			}
			if (num2 >= 0.01f)
			{
				float b = volume / num2;
				return Mathf.Lerp(volume, b, voiceAudioSource.spatialBlend);
			}
			return Mathf.Lerp(volume, 0f, voiceAudioSource.spatialBlend);
		}

		private void InitSpectrumData()
		{
			if (spectrumData == null || spectrumData.Length != N_freq_bins)
			{
				spectrumData = new float[N_freq_bins];
				return;
			}
			for (int i = 0; i < spectrumData.Length; i++)
			{
				spectrumData[i] = 0f;
			}
		}

		public float[] GetUnitySpectrumData(AudioSource source, bool useHamming = true)
		{
			InitSpectrumData();
			if (useHamming)
			{
				source.GetSpectrumData(spectrumData, 0, FFTWindow.Hamming);
			}
			else
			{
				source.GetSpectrumData(spectrumData, 0, FFTWindow.Rectangular);
			}
			return spectrumData;
		}

		private void InitSoundData(int numChannels)
		{
			int num = N_freq_bins * 2;
			if (getData == null || getData.Length != numChannels * num)
			{
				getData = new float[numChannels * num];
			}
			if (soundData == null || soundData.Length != num)
			{
				soundData = new float[num];
			}
		}

		public float[] GetUnitySoundData(AudioSource source, float timeOffset)
		{
			int channels = source.clip.channels;
			InitSoundData(channels);
			float num = source.time + timeOffset;
			int num2 = Mathf.RoundToInt(num * (float)source.clip.frequency);
			bool flag = false;
			if (source.loop)
			{
				if (num >= source.clip.length)
				{
					while (num >= source.clip.length)
					{
						num -= source.clip.length;
					}
					num2 = Mathf.RoundToInt(num * (float)source.clip.frequency);
				}
			}
			else if (num2 + N_freq_bins * 2 > source.clip.samples)
			{
				flag = true;
				for (int i = 0; i < soundData.Length; i++)
				{
					soundData[i] = 0f;
				}
			}
			if (!flag)
			{
				source.clip.GetData(getData, num2);
				for (int j = 0; j < soundData.Length; j++)
				{
					soundData[j] = getData[j * channels];
				}
			}
			return soundData;
		}

		public float[] ConvertSoundDataToSpectrumData(bool useHamming = true)
		{
			int num = N_freq_bins * 2;
			InitSpectrumData();
			if (lomontFFT == null)
			{
				lomontFFT = new LomontFFT();
			}
			if (lomontFFTData == null || lomontFFTData.Length != num)
			{
				lomontFFTData = new double[num];
			}
			int num2 = soundData.Length;
			if (useHamming)
			{
				for (int i = 0; i < num2; i++)
				{
					float num3 = 0.54f - 0.46f * Mathf.Cos((float)Math.PI * 2f * (float)i / (float)num2);
					lomontFFTData[i] = soundData[i] * num3;
				}
			}
			else
			{
				for (int j = 0; j < num2; j++)
				{
					lomontFFTData[j] = soundData[j];
				}
			}
			lomontFFT.TableFFT(lomontFFTData, true);
			for (int k = 0; k < spectrumData.Length; k++)
			{
				int num4 = k * 2;
				float num5 = (float)lomontFFTData[num4];
				float num6 = (float)lomontFFTData[num4 + 1];
				spectrumData[k] = Mathf.Sqrt(num5 * num5 + num6 * num6);
			}
			return spectrumData;
		}

		public float GetVolume(float[] data)
		{
			float num = 0f;
			for (int i = 0; i < data.Length; i++)
			{
				num += Mathf.Abs(data[i]);
			}
			return num / (float)data.Length;
		}

		public float[] ExtractSample(float[] spectrum, float[,] filterBanks, float[,] DCT_mat, float[] cep_lifter, ref float[,] cc_mem, int f_low, int f_high, SpeechUtil.Accuracy accuracy)
		{
			int num = M_FB[(int)accuracy];
			int num2 = C[(int)accuracy];
			float num3 = -1f;
			for (int i = 0; i < spectrum.Length; i++)
			{
				if ((double)spectrum[i] > (double)num3)
				{
					num3 = spectrum[i];
				}
			}
			for (int j = 0; j < spectrum.Length; j++)
			{
				spectrum[j] /= num3;
			}
			if (FB_energy_log == null || FB_energy_log.Length != num)
			{
				FB_energy_log = new float[num];
			}
			for (int k = 0; k < num; k++)
			{
				FB_energy_log[k] = 0f;
				for (int l = f_low; l < f_high; l++)
				{
					FB_energy_log[k] += filterBanks[k, l] * Mathf.Abs(spectrum[l]);
				}
				FB_energy_log[k] = Mathf.Log(FB_energy_log[k]);
			}
			return CepstralCoefficients(FB_energy_log, DCT_mat, ref cc_mem, cep_lifter, num2 + 1, num);
		}

		private float[] CepstralCoefficients(float[] FB_energy_log, float[,] DCT_mat, ref float[,] cc_mem, float[] cep_lifter, int N, int M)
		{
			if (CCnumArray == null || CCnumArray.Length != N)
			{
				CCnumArray = new float[N];
			}
			if (CCcc == null || CCcc.Length != N)
			{
				CCcc = new float[N];
			}
			for (int i = 0; i < N; i++)
			{
				CCnumArray[i] = 0f;
				for (int j = 0; j < M; j++)
				{
					CCnumArray[i] += DCT_mat[i, j] * FB_energy_log[j];
				}
			}
			for (int k = 0; k < N; k++)
			{
				CCcc[k] = CCnumArray[k] * cep_lifter[k];
			}
			return DeltaCC(CCcc, ref cc_mem, N);
		}

		private float[] DeltaCC(float[] cc, ref float[,] cc_mem, int N)
		{
			int num = N * 2;
			if (DeltaCCNumArray == null || DeltaCCNumArray.Length != num)
			{
				DeltaCCNumArray = new float[num];
			}
			for (int i = 0; i < N; i++)
			{
				DeltaCCNumArray[i] = cc[i];
			}
			for (int j = N; j < num; j++)
			{
				DeltaCCNumArray[j] = (float)(((double)cc[j - N] - (double)cc_mem[j - N, 1] + 2.0 * ((double)cc[j - N] - (double)cc_mem[j - N, 0])) / 8.0);
			}
			for (int k = 0; k < N; k++)
			{
				cc_mem[k, 0] = cc_mem[k, 1];
				cc_mem[k, 1] = cc[k];
			}
			return DeltaCCNumArray;
		}

		public FeatureOutput Evaluate(float[] mfccs, VoiceProfile.VoiceType voiceType, SpeechUtil.Accuracy accuracy)
		{
			int num = (C[(int)accuracy] + 1) * 2;
			int num2 = no_phonemes[(int)accuracy];
			if (euclDistanceArray == null || euclDistanceArray.Length != num2)
			{
				euclDistanceArray = new EuclideanDistance[num2];
			}
			for (int i = 0; i < num2; i++)
			{
				float num3 = 0f;
				for (int j = 0; j < num; j++)
				{
					num3 += Mathf.Pow(mfccs[j] - VoiceProfile.VQ_center(voiceType, j, i, accuracy), 2f);
				}
				euclDistanceArray[i] = new EuclideanDistance
				{
					distance = Mathf.Sqrt(num3),
					number = i
				};
			}
			GetShortest(euclDistanceArray, NI);
			InterpolateFeatures(euclDistanceArray);
			for (int k = 0; k < NI; k++)
			{
				featureOutput.reg[k] = euclDistanceArray[k].number;
			}
			return featureOutput;
		}

		private int[] GetShortest(EuclideanDistance[] euclDistanceArray, int N_list)
		{
			if (shortest == null || shortest.Length != N_list)
			{
				shortest = new int[N_list];
			}
			Array.Sort(euclDistanceArray, (EuclideanDistance p1, EuclideanDistance p2) => p1.distance.CompareTo(p2.distance));
			int num = euclDistanceArray.Length;
			for (int i = 0; i < N_list; i++)
			{
				shortest[i] = euclDistanceArray[i].number;
			}
			return shortest;
		}

		private void InterpolateFeatures(EuclideanDistance[] ECD)
		{
			featureOutput.w[0] = 1f;
			float num = 1f;
			if ((double)ECD[0].distance > 5.0)
			{
				for (int i = 1; i < NI; i++)
				{
					featureOutput.w[i] = (float)(1.0 / ((double)ECD[i].distance - (double)ECD[0].distance + 2.0)) * ISF;
					num += featureOutput.w[i];
				}
				for (int j = 1; j < NI; j++)
				{
					featureOutput.w[j] /= num;
				}
			}
		}
	}
}
