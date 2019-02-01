using System;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	public class UILifeBar : Graphic
	{
		public override Texture mainTexture
		{
			get
			{
				return this.allyHpSlice.texture;
			}
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			if (this.HPAmount > this.maxHp)
			{
				this.HPAmount = this.maxHp;
			}
			if (this.tempHPAmount < 0f)
			{
				this.tempHPAmount = 0f;
			}
			vh.Clear();
			Rect pixelAdjustedRect = base.GetPixelAdjustedRect();
			float num = Mathf.Max(Mathf.Max(this.maxHp, this.HPAmount + this.tempHPAmount), this.BleedHp);
			float num2 = num / this.lifeSlice;
			int num3 = Mathf.FloorToInt(num2);
			float num4 = this.HPAmount / this.lifeSlice;
			int num5 = Mathf.FloorToInt(num4);
			float num6 = this.tempHPAmount / this.lifeSlice + (num4 - (float)num5);
			int num7 = Mathf.FloorToInt(num6);
			float num8 = this.BleedHp / this.lifeSlice;
			int num9 = Mathf.FloorToInt(num8);
			float f = pixelAdjustedRect.width / num2;
			float num10 = (float)Mathf.FloorToInt(f);
			float num11 = this.spacing;
			while (num10 - num11 <= 0f)
			{
				num11 -= 1f;
				if (num11 < 0f)
				{
					this.CustomGenerateSlicedSprite(vh, pixelAdjustedRect, this.tempHpSlice, this.ShieldColor);
					return;
				}
			}
			float width = (num6 - (float)num7) * (num10 - num11);
			float width2 = (num4 - (float)num5) * (num10 - num11);
			float num12 = 0f;
			int num13 = Mathf.FloorToInt(pixelAdjustedRect.width - num2 * num10);
			Sprite sprite = null;
			Color sliceColor = this.SelfBackgroundColor;
			UILifeBar.Kind kind = this.kind;
			if (kind != UILifeBar.Kind.Self)
			{
				if (kind != UILifeBar.Kind.Ally)
				{
					if (kind == UILifeBar.Kind.Enemy)
					{
						sprite = this.enemyHpSlice;
						sliceColor = this.EnemyBackgroundColor;
					}
				}
				else
				{
					sprite = this.allyHpSlice;
					sliceColor = this.AllyBackgroundColor;
				}
			}
			else
			{
				sprite = this.selfHpSlice;
				sliceColor = this.SelfBackgroundColor;
			}
			Rect pixelAdjustedRect2 = pixelAdjustedRect;
			pixelAdjustedRect2.width = num10 - num11;
			pixelAdjustedRect2.x += num12;
			for (int i = 0; i < num3; i++)
			{
				if (i < num13)
				{
					pixelAdjustedRect2.width = num10 - num11 + 1f;
				}
				if (i >= num5)
				{
					if (i - num5 < num7)
					{
						this.CustomGenerateSlicedSprite(vh, pixelAdjustedRect2, this.tempHpSlice, this.ShieldColor);
					}
					else if (i < num9)
					{
						this.CustomGenerateSlicedSprite(vh, pixelAdjustedRect2, this.tempHpSlice, this.BleedColor);
					}
					else
					{
						this.CustomGenerateSlicedSprite(vh, pixelAdjustedRect2, this.sliceBG, sliceColor);
					}
				}
				else
				{
					this.CustomGenerateSlicedSprite(vh, pixelAdjustedRect2, sprite, Color.white);
				}
				pixelAdjustedRect2.x += num10;
				if (i < num13)
				{
					pixelAdjustedRect2.x += 1f;
					pixelAdjustedRect2.width -= 1f;
				}
			}
			if (this.HPAmount < this.maxHp)
			{
				pixelAdjustedRect2.x = (float)num3 * num10 + num12 + (float)num13;
				pixelAdjustedRect2.width = (num2 - (float)num3) * (num10 - num11);
				this.CustomGenerateSlicedSprite(vh, pixelAdjustedRect2, this.sliceBG, sliceColor);
			}
			if (this.BleedHp > this.HPAmount + this.tempHPAmount)
			{
				pixelAdjustedRect2.x = (float)num9 * num10 + num12 + (float)Mathf.Min(num13, num9);
				pixelAdjustedRect2.width = (num8 - (float)num9) * (num10 - num11);
				this.CustomGenerateSlicedSprite(vh, pixelAdjustedRect2, this.tempHpSlice, this.BleedColor);
			}
			if (this.tempHPAmount > 0f)
			{
				int num14 = num5 + num7;
				pixelAdjustedRect2.x = (float)num14 * num10 + num12 + (float)Mathf.Min(num13, num14);
				pixelAdjustedRect2.width = width;
				this.CustomGenerateSlicedSprite(vh, pixelAdjustedRect2, this.tempHpSlice, this.ShieldColor);
			}
			pixelAdjustedRect2.x = (float)num5 * num10 + num12 + (float)Mathf.Min(num13, num5);
			pixelAdjustedRect2.width = width2;
			this.CustomGenerateSlicedSprite(vh, pixelAdjustedRect2, sprite, Color.white);
		}

		private Vector4 GetAdjustedBorders(Vector4 border, Rect rect)
		{
			for (int i = 0; i <= 1; i++)
			{
				float num = border[i] + border[i + 2];
				if ((double)rect.size[i] < (double)num && (double)num != 0.0)
				{
					float num2 = rect.size[i] / num;
					ref Vector4 ptr = ref border;
					int index;
					border[index = i] = ptr[index] * num2;
					ptr = ref border;
					int index2;
					border[index2 = i + 2] = ptr[index2] * num2;
				}
			}
			return border;
		}

		private void CustomGenerateSlicedSprite(VertexHelper vh, Rect pixelAdjustedRect, Sprite sprite, Color sliceColor)
		{
			if (sprite == null)
			{
				return;
			}
			Vector4 outerUV = DataUtility.GetOuterUV(sprite);
			Vector4 innerUV = DataUtility.GetInnerUV(sprite);
			Vector4 padding = DataUtility.GetPadding(sprite);
			Vector4 border = sprite.border;
			Vector4 adjustedBorders = this.GetAdjustedBorders(border / 1f, pixelAdjustedRect);
			Vector4 vector = padding / 1f;
			UILifeBar.s_VertScratch[0].Set(vector.x, vector.y);
			UILifeBar.s_VertScratch[3].Set(pixelAdjustedRect.width - vector.z, pixelAdjustedRect.height - vector.w);
			UILifeBar.s_VertScratch[1].x = adjustedBorders.x;
			UILifeBar.s_VertScratch[1].y = adjustedBorders.y;
			UILifeBar.s_VertScratch[2].x = pixelAdjustedRect.width - adjustedBorders.z;
			UILifeBar.s_VertScratch[2].y = pixelAdjustedRect.height - adjustedBorders.w;
			for (int i = 0; i < 4; i++)
			{
				Vector2[] array = UILifeBar.s_VertScratch;
				int num = i;
				array[num].x = array[num].x + pixelAdjustedRect.x;
				Vector2[] array2 = UILifeBar.s_VertScratch;
				int num2 = i;
				array2[num2].y = array2[num2].y + pixelAdjustedRect.y;
			}
			UILifeBar.s_UVScratch[0].Set(outerUV.x, outerUV.y);
			UILifeBar.s_UVScratch[1].Set(innerUV.x, innerUV.y);
			UILifeBar.s_UVScratch[2].Set(innerUV.z, innerUV.w);
			UILifeBar.s_UVScratch[3].Set(outerUV.z, outerUV.w);
			UIVertex[] array3 = new UIVertex[4];
			for (int j = 0; j < 3; j++)
			{
				int num3 = j + 1;
				for (int k = 0; k < 3; k++)
				{
					int num4 = k + 1;
					array3[0].position.Set(UILifeBar.s_VertScratch[j].x, UILifeBar.s_VertScratch[k].y, 0f);
					array3[0].uv0.Set(UILifeBar.s_UVScratch[j].x, UILifeBar.s_UVScratch[k].y);
					array3[0].color = sliceColor;
					array3[1].position.Set(UILifeBar.s_VertScratch[j].x, UILifeBar.s_VertScratch[num4].y, 0f);
					array3[1].uv0.Set(UILifeBar.s_UVScratch[j].x, UILifeBar.s_UVScratch[num4].y);
					array3[1].color = sliceColor;
					array3[2].position.Set(UILifeBar.s_VertScratch[num3].x, UILifeBar.s_VertScratch[num4].y, 0f);
					array3[2].uv0.Set(UILifeBar.s_UVScratch[num3].x, UILifeBar.s_UVScratch[num4].y);
					array3[2].color = sliceColor;
					array3[3].position.Set(UILifeBar.s_VertScratch[num3].x, UILifeBar.s_VertScratch[k].y, 0f);
					array3[3].uv0.Set(UILifeBar.s_UVScratch[num3].x, UILifeBar.s_UVScratch[k].y);
					array3[3].color = sliceColor;
					vh.AddUIVertexQuad(array3);
				}
			}
		}

		public float maxHp = 100f;

		public float lifeSlice = 10f;

		public float spacing = 2f;

		public float HPAmount = 100f;

		public float tempHPAmount;

		public float BleedHp;

		public UILifeBar.Kind kind;

		public Sprite allyHpSlice;

		public Sprite selfHpSlice;

		public Sprite enemyHpSlice;

		public Sprite tempHpSlice;

		public Sprite sliceBG;

		private static readonly Vector2[] s_UVScratch = new Vector2[4];

		private static readonly Vector2[] s_VertScratch = new Vector2[4];

		public Color SelfBackgroundColor;

		public Color AllyBackgroundColor;

		public Color EnemyBackgroundColor;

		public Color ShieldColor;

		public Color BleedColor;

		public enum Kind
		{
			Enemy,
			Ally,
			Self
		}
	}
}
