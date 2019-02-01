using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HMMUITweenGrayScale : UITweener
	{
		private void Cache()
		{
			this.mCached = true;
			this.mUI2DSprites = new List<UI2DSprite>();
			this.mUITextures = new List<UITexture>();
			foreach (UI2DSprite ui2DSprite in base.GetComponentsInChildren<UI2DSprite>())
			{
				if (ui2DSprite.shader.name == "Transparent Colored (selection grayscale)")
				{
					this.mUI2DSprites.Add(ui2DSprite);
				}
			}
			foreach (UITexture uitexture in base.GetComponentsInChildren<UITexture>())
			{
				if (uitexture.shader.name == "Transparent Colored (selection grayscale)")
				{
					this.mUITextures.Add(uitexture);
				}
			}
		}

		protected override void OnUpdate(float factor, bool isFinished)
		{
			if (!this.mCached)
			{
				this.Cache();
			}
			this.Value = Mathf.Lerp(this.from, this.to, factor);
			for (int i = 0; i < this.mUI2DSprites.Count; i++)
			{
				UI2DSprite ui2DSprite = this.mUI2DSprites[i];
				ui2DSprite.color = new Color(ui2DSprite.color.r, ui2DSprite.color.g, ui2DSprite.color.b, this.Value);
			}
			for (int j = 0; j < this.mUITextures.Count; j++)
			{
				UITexture uitexture = this.mUITextures[j];
				uitexture.color = new Color(uitexture.color.r, uitexture.color.g, uitexture.color.b, this.Value);
			}
		}

		public static HMMUITweenGrayScale Begin(GameObject go, float duration, float alpha)
		{
			HMMUITweenGrayScale hmmuitweenGrayScale = UITweener.Begin<HMMUITweenGrayScale>(go, duration);
			hmmuitweenGrayScale.from = hmmuitweenGrayScale.Value;
			hmmuitweenGrayScale.to = alpha;
			if (duration <= 0f)
			{
				hmmuitweenGrayScale.Sample(1f, true);
				hmmuitweenGrayScale.enabled = false;
			}
			return hmmuitweenGrayScale;
		}

		[ContextMenu("Set 'From' to current value")]
		public override void SetStartToCurrentValue()
		{
			this.from = this.Value;
		}

		[ContextMenu("Set 'To' to current value")]
		public override void SetEndToCurrentValue()
		{
			this.to = this.Value;
		}

		[ContextMenu("Assume value of 'From'")]
		private void SetCurrentValueToStart()
		{
			this.Value = this.from;
		}

		[ContextMenu("Assume value of 'To'")]
		private void SetCurrentValueToEnd()
		{
			this.Value = this.to;
		}

		public float from;

		public float to;

		private bool mCached;

		private List<UI2DSprite> mUI2DSprites;

		private List<UITexture> mUITextures;

		public float Value;
	}
}
