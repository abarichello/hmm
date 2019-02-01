using System;
using ClientAPI.Objects;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public static class Texture2DUtils
	{
		public static Texture2D CreateTexture2D(SwordfishImage sfImage)
		{
			return Texture2DUtils.CreateTexture2D(sfImage.BitsPerPixel, sfImage.Width, sfImage.Height, sfImage.Buffer);
		}

		public static Texture2D CreateTexture2D(int bitsPerPixel, int width, int height, byte[] buffer)
		{
			int num = bitsPerPixel / 8;
			int num2 = width * num;
			int num3 = buffer.Length / num2;
			Color32[] array = new Color32[width * height];
			int num4 = 0;
			int num5 = num * width * height;
			int num6 = buffer.Length - num5;
			if (num6 < 0)
			{
				num6 = 0;
			}
			if (bitsPerPixel == 24)
			{
				for (int i = 0; i < num3; i++)
				{
					int num7 = num2 * i;
					for (int j = 0; j < num2; j += num)
					{
						int num8 = num7 + j + num6;
						array[num4++] = new Color32(buffer[num8 + 2], buffer[num8 + 1], buffer[num8], byte.MaxValue);
					}
				}
			}
			else if (bitsPerPixel != 8)
			{
				for (int k = num3 - 1; k >= 0; k--)
				{
					int num9 = num2 * k;
					for (int l = 0; l < num2; l += num)
					{
						int num10 = num9 + l;
						array[num4++] = new Color32(buffer[num10], buffer[++num10], buffer[++num10], buffer[num10 + 1]);
					}
				}
			}
			else
			{
				for (int m = 0; m < height; m++)
				{
					int num11 = num2 * m;
					for (int n = 0; n < num2; n += num)
					{
						int num12 = num11 + n + num6;
						array[num4++] = new Color32(buffer[num12], buffer[num12], buffer[num12], byte.MaxValue);
					}
				}
			}
			Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
			texture2D.SetPixels32(array);
			texture2D.filterMode = FilterMode.Bilinear;
			texture2D.Apply();
			return texture2D;
		}
	}
}
