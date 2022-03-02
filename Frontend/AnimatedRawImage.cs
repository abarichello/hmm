using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class AnimatedRawImage : HmmUiRawImage, ITextureMappingUpdater
	{
		protected override void Awake()
		{
			if (this._animator == null)
			{
				this._animator = new SpritesheetAnimator(this);
			}
			base.Awake();
		}

		public void UpdateUv(Rect uvRectangle)
		{
			base.uvRect = uvRectangle;
		}

		public void ChangeVisibility(bool visibility)
		{
			base.enabled = visibility;
		}

		public bool IsPlaying()
		{
			return this._animator != null && this._animator.IsPlaying;
		}

		public void Update()
		{
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

		public override void OnAssetLoaded(string textureName, Texture2D loadedTexture)
		{
			base.OnAssetLoaded(textureName, loadedTexture);
			this.InitializeAnimator();
		}

		public void InitializeAnimator()
		{
			if (this._animator == null)
			{
				this._animator = new SpritesheetAnimator(this);
			}
			this._animator.Initialize(this.columns, this.lines, this.duration);
		}

		[NonSerialized]
		private SpritesheetAnimator _animator;

		[SerializeField]
		private int columns = 3;

		[SerializeField]
		private int lines = 3;

		[SerializeField]
		private float duration = 0.6f;
	}
}
