using System;
using Hoplon.UserInterface;
using UnityEngine;
using UnityEngine.Sprites;

namespace HeavyMetalMachines.Frontend
{
	public class UILifeBar : HoplonGraphic
	{
		protected override Texture mainTexture
		{
			get
			{
				return (!this.allyHpSlice) ? base.mainTexture : this.allyHpSlice.texture;
			}
		}

		protected override void InnerUpdateGeometry(UIMeshBuilder builder)
		{
			if (this.HPAmount > this.maxHp)
			{
				this.HPAmount = this.maxHp;
			}
			if (this.tempHPAmount < 0f)
			{
				this.tempHPAmount = 0f;
			}
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
			float num10 = pixelAdjustedRect.width / num2;
			float num11 = (float)Mathf.FloorToInt(num10);
			float num12 = this.spacing;
			while (num11 - num12 <= 0f)
			{
				num12 -= 1f;
				if (num12 < 0f)
				{
					UILifeBar.CustomGenerateSlicedSprite(builder, pixelAdjustedRect, this.tempHpSlice, this.ShieldColor);
					return;
				}
			}
			float width = (num6 - (float)num7) * (num11 - num12);
			float width2 = (num4 - (float)num5) * (num11 - num12);
			int num13 = Mathf.FloorToInt(pixelAdjustedRect.width - num2 * num11);
			Sprite sprite = null;
			Color color = this.SelfBackgroundColor;
			UILifeBar.Kind kind = this.kind;
			if (kind != UILifeBar.Kind.Self)
			{
				if (kind != UILifeBar.Kind.Ally)
				{
					if (kind == UILifeBar.Kind.Enemy)
					{
						sprite = this.enemyHpSlice;
						color = this.EnemyBackgroundColor;
					}
				}
				else
				{
					sprite = this.allyHpSlice;
					color = this.AllyBackgroundColor;
				}
			}
			else
			{
				sprite = this.selfHpSlice;
				color = this.SelfBackgroundColor;
			}
			Rect pixelAdjustedRect2 = pixelAdjustedRect;
			pixelAdjustedRect2.width = num11 - num12;
			for (int i = 0; i < num3; i++)
			{
				if (i < num13)
				{
					pixelAdjustedRect2.width = num11 - num12 + 1f;
				}
				if (i >= num5)
				{
					if (i - num5 < num7)
					{
						UILifeBar.CustomGenerateSlicedSprite(builder, pixelAdjustedRect2, this.tempHpSlice, this.ShieldColor);
					}
					else if (i < num9)
					{
						UILifeBar.CustomGenerateSlicedSprite(builder, pixelAdjustedRect2, this.tempHpSlice, this.BleedColor);
					}
					else
					{
						UILifeBar.CustomGenerateSlicedSprite(builder, pixelAdjustedRect2, this.sliceBG, color);
					}
				}
				else
				{
					UILifeBar.CustomGenerateSlicedSprite(builder, pixelAdjustedRect2, sprite, Color.white);
				}
				pixelAdjustedRect2.x += num11;
				if (i < num13)
				{
					pixelAdjustedRect2.x += 1f;
					pixelAdjustedRect2.width -= 1f;
				}
			}
			if (this.HPAmount < this.maxHp)
			{
				pixelAdjustedRect2.x = (float)num3 * num11 + (float)num13;
				pixelAdjustedRect2.width = (num2 - (float)num3) * (num11 - num12);
				UILifeBar.CustomGenerateSlicedSprite(builder, pixelAdjustedRect2, this.sliceBG, color);
			}
			if (this.BleedHp > this.HPAmount + this.tempHPAmount)
			{
				pixelAdjustedRect2.x = (float)num9 * num11 + (float)Mathf.Min(num13, num9);
				pixelAdjustedRect2.width = (num8 - (float)num9) * (num11 - num12);
				UILifeBar.CustomGenerateSlicedSprite(builder, pixelAdjustedRect2, this.tempHpSlice, this.BleedColor);
			}
			if (this.tempHPAmount > 0f)
			{
				int num14 = num5 + num7;
				pixelAdjustedRect2.x = (float)num14 * num11 + (float)Mathf.Min(num13, num14);
				pixelAdjustedRect2.width = width;
				UILifeBar.CustomGenerateSlicedSprite(builder, pixelAdjustedRect2, this.tempHpSlice, this.ShieldColor);
			}
			pixelAdjustedRect2.x = (float)num5 * num11 + (float)Mathf.Min(num13, num5);
			pixelAdjustedRect2.width = width2;
			UILifeBar.CustomGenerateSlicedSprite(builder, pixelAdjustedRect2, sprite, Color.white);
		}

		private static Vector4 GetAdjustedBorders(Vector4 border, Rect rect)
		{
			for (int i = 0; i <= 1; i++)
			{
				float num = border[i] + border[i + 2];
				if (rect.size[i] < num && Math.Abs(num) > Mathf.Epsilon)
				{
					float num2 = rect.size[i] / num;
					ref Vector4 ptr = ref border;
					int num3;
					border[num3 = i] = ptr[num3] * num2;
					ptr = ref border;
					int num4;
					border[num4 = i + 2] = ptr[num4] * num2;
				}
			}
			return border;
		}

		private static void CustomGenerateSlicedSprite(UIMeshBuilder builder, Rect pixelAdjustedRect, Sprite sprite, Color32 sliceColor)
		{
			if (sprite == null)
			{
				return;
			}
			Vector4 outerUV = DataUtility.GetOuterUV(sprite);
			Vector4 innerUV = DataUtility.GetInnerUV(sprite);
			Vector4 padding = DataUtility.GetPadding(sprite);
			Vector4 border = sprite.border;
			Vector4 adjustedBorders = UILifeBar.GetAdjustedBorders(border, pixelAdjustedRect);
			UILifeBar.s_VertScratch[0].Set(padding.x, padding.y);
			UILifeBar.s_VertScratch[3].Set(pixelAdjustedRect.width - padding.z, pixelAdjustedRect.height - padding.w);
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
			for (int j = 0; j < 3; j++)
			{
				int num3 = j + 1;
				for (int k = 0; k < 3; k++)
				{
					int num4 = k + 1;
					int vertexCount = builder.VertexCount;
					builder.AddVertex(new Vector3(UILifeBar.s_VertScratch[j].x, UILifeBar.s_VertScratch[k].y, 0f), sliceColor, new Vector2(UILifeBar.s_UVScratch[j].x, UILifeBar.s_UVScratch[k].y));
					builder.AddVertex(new Vector3(UILifeBar.s_VertScratch[j].x, UILifeBar.s_VertScratch[num4].y, 0f), sliceColor, new Vector2(UILifeBar.s_UVScratch[j].x, UILifeBar.s_UVScratch[num4].y));
					builder.AddVertex(new Vector3(UILifeBar.s_VertScratch[num3].x, UILifeBar.s_VertScratch[num4].y, 0f), sliceColor, new Vector2(UILifeBar.s_UVScratch[num3].x, UILifeBar.s_UVScratch[num4].y));
					builder.AddVertex(new Vector3(UILifeBar.s_VertScratch[num3].x, UILifeBar.s_VertScratch[k].y, 0f), sliceColor, new Vector2(UILifeBar.s_UVScratch[num3].x, UILifeBar.s_UVScratch[k].y));
					builder.AddTriangle(vertexCount, vertexCount + 1, vertexCount + 2);
					builder.AddTriangle(vertexCount + 2, vertexCount + 3, vertexCount);
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

		public Color SelfBackgroundColor;

		public Color AllyBackgroundColor;

		public Color EnemyBackgroundColor;

		public Color ShieldColor;

		public Color BleedColor;

		private static readonly Vector2[] s_UVScratch = new Vector2[4];

		private static readonly Vector2[] s_VertScratch = new Vector2[4];

		public enum Kind
		{
			Enemy,
			Ally,
			Self
		}
	}
}
