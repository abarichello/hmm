using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[Obsolete]
	[ExecuteInEditMode]
	[RequireComponent(typeof(UI2DSprite))]
	public class HMM2DSpriteAnimation : MonoBehaviour
	{
		protected void OnEnable()
		{
			this.Update();
		}

		protected void Update()
		{
			if (this.frames == null || this.frames.Length == 0)
			{
				base.enabled = false;
				return;
			}
			this.animationTime = Mathf.Clamp01(this.animationTime);
			this.mIndex = (int)(this.animationTime * (float)(this.frames.Length - 1));
			this.UpdateSprite();
		}

		private void UpdateSprite()
		{
			if (this.mUnitySprite == null && this.mNguiSprite == null)
			{
				this.mUnitySprite = base.GetComponent<SpriteRenderer>();
				this.mNguiSprite = base.GetComponent<UI2DSprite>();
				if (this.mUnitySprite == null && this.mNguiSprite == null)
				{
					base.enabled = false;
					return;
				}
			}
			if (this.mUnitySprite != null)
			{
				this.mUnitySprite.sprite = this.frames[this.mIndex];
			}
			else if (this.mNguiSprite != null)
			{
				this.mNguiSprite.nextSprite = this.frames[this.mIndex];
			}
		}

		public float animationTime;

		private UI2DSprite mNguiSprite;

		public Sprite[] frames;

		private SpriteRenderer mUnitySprite;

		private int mIndex;
	}
}
