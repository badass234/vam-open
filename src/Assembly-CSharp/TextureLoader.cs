using System;
using System.IO;
using UnityEngine;

public class TextureLoader : MonoBehaviour
{
	public static Texture2D LoadTGA(string fileName)
	{
		using (FileStream tGAStream = File.OpenRead(fileName))
		{
			return LoadTGA(tGAStream);
		}
	}

	public static Texture2D LoadDDSManual(string ddsPath)
	{
		try
		{
			byte[] array = File.ReadAllBytes(ddsPath);
			byte b = array[4];
			if (b != 124)
			{
				throw new Exception("Invalid DDS DXTn texture. Unable to read");
			}
			int height = array[13] * 256 + array[12];
			int width = array[17] * 256 + array[16];
			byte b2 = array[87];
			TextureFormat format = TextureFormat.DXT5;
			if (b2 == 49)
			{
				format = TextureFormat.DXT1;
			}
			if (b2 == 53)
			{
				format = TextureFormat.DXT5;
			}
			int num = 128;
			byte[] array2 = new byte[array.Length - num];
			Buffer.BlockCopy(array, num, array2, 0, array.Length - num);
			FileInfo fileInfo = new FileInfo(ddsPath);
			Texture2D texture2D = new Texture2D(width, height, format, false);
			texture2D.LoadRawTextureData(array2);
			texture2D.Apply();
			texture2D.name = fileInfo.Name;
			return texture2D;
		}
		catch (Exception ex)
		{
			Debug.LogError("Error: Could not load DDS " + ex);
			return new Texture2D(8, 8);
		}
	}

	public static void SetNormalMap(ref Texture2D tex)
	{
		Color[] pixels = tex.GetPixels();
		for (int i = 0; i < pixels.Length; i++)
		{
			Color color = pixels[i];
			color.r = 1f;
			color.a = pixels[i].r;
			pixels[i] = color;
		}
		tex.SetPixels(pixels);
		tex.Apply(true);
	}

	public static Texture2D LoadTexture(string fn, bool normalMap = false)
	{
		if (!File.Exists(fn))
		{
			return null;
		}
		switch (Path.GetExtension(fn).ToLower())
		{
		case ".png":
		case ".jpg":
		{
			Texture2D tex2 = new Texture2D(1, 1);
			tex2.LoadImage(File.ReadAllBytes(fn));
			if (normalMap)
			{
				SetNormalMap(ref tex2);
			}
			return tex2;
		}
		case ".dds":
		{
			Texture2D tex3 = LoadDDSManual(fn);
			if (normalMap)
			{
				SetNormalMap(ref tex3);
			}
			return tex3;
		}
		case ".tga":
		{
			Texture2D tex = LoadTGA(fn);
			if (normalMap)
			{
				SetNormalMap(ref tex);
			}
			return tex;
		}
		default:
			Debug.Log("texture not supported : " + fn);
			return null;
		}
	}

	public static Texture2D LoadTGA(Stream TGAStream, TextureFormat tf = TextureFormat.RGBA32, bool useMipMap = true, bool linear = false)
	{
		using (BinaryReader binaryReader = new BinaryReader(TGAStream))
		{
			binaryReader.BaseStream.Seek(12L, SeekOrigin.Begin);
			short num = binaryReader.ReadInt16();
			short num2 = binaryReader.ReadInt16();
			int num3 = binaryReader.ReadByte();
			binaryReader.BaseStream.Seek(1L, SeekOrigin.Current);
			Texture2D texture2D = new Texture2D(num, num2, tf, useMipMap, linear);
			Color32[] array = new Color32[num * num2];
			switch (num3)
			{
			case 32:
			{
				for (int j = 0; j < num * num2; j++)
				{
					byte b2 = binaryReader.ReadByte();
					byte g2 = binaryReader.ReadByte();
					byte r2 = binaryReader.ReadByte();
					byte a = binaryReader.ReadByte();
					array[j] = new Color32(r2, g2, b2, a);
				}
				break;
			}
			case 24:
			{
				for (int i = 0; i < num * num2; i++)
				{
					byte b = binaryReader.ReadByte();
					byte g = binaryReader.ReadByte();
					byte r = binaryReader.ReadByte();
					array[i] = new Color32(r, g, b, 1);
				}
				break;
			}
			default:
				throw new Exception("TGA texture had non 32/24 bit depth.");
			}
			texture2D.SetPixels32(array);
			texture2D.Apply();
			return texture2D;
		}
	}
}
