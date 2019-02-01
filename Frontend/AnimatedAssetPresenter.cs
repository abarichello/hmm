using System;
using SharedUtils.Loading;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class AnimatedAssetPresenter : IDisposable
	{
		public AnimatedAssetPresenter(IAnimation fadeInAnimation, IAnimation fadeOutAnimation, IAssetLoader assetLoader, IAssetView modelView)
		{
			this._fadeInAnimation = fadeInAnimation;
			this._fadeOutAnimation = fadeOutAnimation;
			this._assetLoader = assetLoader;
			this._modelView = modelView;
			this._assetLoader.AssetLoaded += this.OnAssetLoaded;
		}

		public AnimatedAssetPresenter.State CurrentState { get; private set; }

		public void Dispose()
		{
			this._assetLoader.AssetLoaded -= this.OnAssetLoaded;
		}

		public void Update()
		{
			AnimatedAssetPresenter.State currentState = this.CurrentState;
			if (currentState == AnimatedAssetPresenter.State.FadingOut)
			{
				this.UpdateWhenFadingOut();
				return;
			}
			if (currentState != AnimatedAssetPresenter.State.FadingIn)
			{
				return;
			}
			this.UpdateWhenFadingIn();
		}

		public void ShowAsset(string assetName)
		{
			this._assetNameToShow = assetName;
			if (this.CurrentState == AnimatedAssetPresenter.State.LoadingAsset)
			{
				this._assetLoader.LoadAsset(assetName);
				return;
			}
			this.CurrentState = AnimatedAssetPresenter.State.FadingOut;
			this._fadeOutAnimation.Play();
		}

		private void UpdateWhenFadingOut()
		{
			if (this._fadeOutAnimation.IsPlaying)
			{
				return;
			}
			this.CurrentState = AnimatedAssetPresenter.State.LoadingAsset;
			this._assetLoader.LoadAsset(this._assetNameToShow);
		}

		private void UpdateWhenFadingIn()
		{
			if (this._fadeInAnimation.IsPlaying)
			{
				return;
			}
			this.CurrentState = AnimatedAssetPresenter.State.Idle;
		}

		private void OnAssetLoaded(string assetName, GameObject asset)
		{
			if (!string.Equals(assetName, this._assetNameToShow))
			{
				return;
			}
			this._modelView.ShowAsset(asset, assetName);
			this._fadeInAnimation.Play();
			this.CurrentState = AnimatedAssetPresenter.State.FadingIn;
		}

		private readonly IAnimation _fadeInAnimation;

		private readonly IAnimation _fadeOutAnimation;

		private readonly IAssetLoader _assetLoader;

		private readonly IAssetView _modelView;

		private string _assetNameToShow;

		public enum State
		{
			Idle,
			FadingOut,
			LoadingAsset,
			FadingIn
		}
	}
}
