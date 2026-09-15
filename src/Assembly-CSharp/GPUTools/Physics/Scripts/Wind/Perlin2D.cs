using System;
using System.Collections.Generic;
using UnityEngine;

namespace GPUTools.Physics.Scripts.Wind
{
	public class Perlin2D
	{
		private readonly byte[] permutationTable;

		public Perlin2D(int seed = 0)
		{
			System.Random random = new System.Random(seed);
			permutationTable = new byte[1024];
			random.NextBytes(permutationTable);
		}

		private static float QunticCurve(float t)
		{
			return t * t * t * (t * (t * 6f - 15f) + 10f);
		}

		private Vector2 GetPseudoRandomGradientVector(int x, int y)
		{
			int num = (int)(((x * 1836311903) ^ (y * 2971215073u + 4807526976L)) & 0x3FF);
			switch (permutationTable[num] & 3)
			{
			case 0:
				return new Vector2(1f, 0f);
			case 1:
				return new Vector2(-1f, 0f);
			case 2:
				return new Vector2(0f, 1f);
			default:
				return new Vector2(0f, -1f);
			}
		}

		public float Noise(Vector2 fp)
		{
			int num = (int)Math.Floor(fp.x);
			int num2 = (int)Math.Floor(fp.y);
			float num3 = fp.x - (float)num;
			float num4 = fp.y - (float)num2;
			Vector2 pseudoRandomGradientVector = GetPseudoRandomGradientVector(num, num2);
			Vector2 pseudoRandomGradientVector2 = GetPseudoRandomGradientVector(num + 1, num2);
			Vector2 pseudoRandomGradientVector3 = GetPseudoRandomGradientVector(num, num2 + 1);
			Vector2 pseudoRandomGradientVector4 = GetPseudoRandomGradientVector(num + 1, num2 + 1);
			Vector2 vector = new Vector2(num3, num4);
			Vector2 vector2 = new Vector2(num3 - 1f, num4);
			Vector2 vector3 = new Vector2(num3, num4 - 1f);
			Vector2 vector4 = new Vector2(num3 - 1f, num4 - 1f);
			float a = Vector3.Dot(vector, pseudoRandomGradientVector);
			float b = Vector3.Dot(vector2, pseudoRandomGradientVector2);
			float a2 = Vector3.Dot(vector3, pseudoRandomGradientVector3);
			float b2 = Vector3.Dot(vector4, pseudoRandomGradientVector4);
			num3 = QunticCurve(num3);
			num4 = QunticCurve(num4);
			float a3 = Mathf.Lerp(a, b, num3);
			float b3 = Mathf.Lerp(a2, b2, num3);
			return Mathf.Lerp(a3, b3, num4);
		}

		public float Noise(Vector2 fp, int octaves, float persistence = 0.5f)
		{
			float num = 1f;
			float num2 = 0f;
			float num3 = 0f;
			while (octaves-- > 0)
			{
				num2 += num;
				num3 += Noise(fp) * num;
				num *= persistence;
				fp.x *= 2f;
				fp.y *= 2f;
			}
			return num3 / num2;
		}

		public float Noise(Vector2 fp, List<NoiseOctave> octaves)
		{
			float num = 0f;
			float num2 = 0f;
			for (int i = 0; i < octaves.Count; i++)
			{
				NoiseOctave noiseOctave = octaves[i];
				num2 += noiseOctave.Amplitude;
				num += Noise(fp * noiseOctave.Scale) * noiseOctave.Amplitude;
			}
			return num / num2;
		}
	}
}
