using System;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class AnimatedNguiTexture : HMMUI2DDynamicTexture, ITextureMappingUpdater
	{
		protected override void Awake()
		{
			if (this._animator == null)
			{
				this._animator = new SpritesheetAnimator(this);
			}
			base.Awake();
		}

		public void UpdateUv(Rect uvRect)
		{
			this.mRect = uvRect;
			this.MarkAsChanged();
		}

		public void ChangeVisibility(bool visibility)
		{
			base.enabled = visibility;
		}

		public bool IsPlaying()
		{
			return this._animator != null && this._animator.IsPlaying;
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();
			this._animator.Update();
		}

		public void Stop()
		{
			this._animator.Stop();
		}

		public void StartAnimation()
		{
			this._animator.StartAnimation();
		}

		public void TryToLoadAsset(string textureName)
		{
			base.TextureName = textureName;
		}

		public override void OnAssetLoaded(string textureName, Texture2D loadedTexture)
		{
			base.OnAssetLoaded(textureName, loadedTexture);
			if (this._animator == null)
			{
				this._animator = new SpritesheetAnimator(this);
			}
			this._animator.Initialize(this.columns, this.lines, this.duration);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(AnimatedNguiTexture));

		private SpritesheetAnimator _animator;

		[SerializeField]
		private int columns = 3;

		[SerializeField]
		private int lines = 3;

		[SerializeField]
		private float duration = 0.6f;
	}
}
