using System;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	[ExecuteInEditMode]
	public class TestTexture2d : MonoBehaviour
	{
		private void Update()
		{
			if (this.TryAgain)
			{
				this.TryAgain = false;
				byte[] array = Convert.FromBase64String((!this.TestWorking) ? this.Base64Str : this.Base64Working);
				Debug.Log(string.Format("Will create texture with bitsPerPixel:{0} lenght:{1}", this.BistPerPixel, array.Length));
				int num = this.BistPerPixel * this.Width * this.Height / 8;
				if (num != array.Length)
				{
					Debug.LogWarning(string.Format("Buffer size different from expected! Expected:{0} Acctual:{1}", num, array.Length));
				}
				if (this.OverrideFirstBytes > 0)
				{
					for (int i = 0; i < this.OverrideFirstBytes; i++)
					{
						array[i] = byte.MaxValue;
					}
				}
				if (this.AutoFillRemaingData)
				{
					byte[] array2 = new byte[num];
					for (int j = 0; j < array2.Length; j++)
					{
						if (j < array.Length)
						{
							array2[j] = array[j];
						}
						else
						{
							array2[j] = byte.MaxValue;
						}
					}
					array = array2;
				}
				if (this.OffsetBytes)
				{
					int num2 = array.Length - num;
					Debug.Log("Diff:" + num2);
					byte[] array3 = new byte[num];
					int num3 = this.OffsetInt;
					int k = 0;
					while (k < num)
					{
						array3[k] = array[num3];
						k++;
						num3++;
					}
					array = array3;
				}
				if (this.SwitchChannel)
				{
					for (int l = 0; l < array.Length; l += 3)
					{
						byte b = array[l];
						array[l] = array[l + 2];
						array[l + 2] = b;
					}
				}
				Texture2D mainTexture = Texture2DUtils.CreateTexture2D(this.BistPerPixel, this.Width, this.Height, array);
				this.Texture.mainTexture = mainTexture;
				Debug.Log("Updated create texture");
			}
		}

		public UITexture Texture;

		public int BistPerPixel = 32;

		public int Width = 184;

		public int Height = 184;

		public string Base64Str;

		public string Base64Working;

		[Header("This is a 32 bits image")]
		public bool TestWorking;

		public bool TryAgain;

		public int OverrideFirstBytes;

		public bool OffsetBytes;

		public int OffsetInt;

		public bool AutoFillRemaingData;

		public bool SwitchChannel;
	}
}
