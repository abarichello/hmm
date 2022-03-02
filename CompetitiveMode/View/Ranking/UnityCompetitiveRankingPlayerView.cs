using System;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Social.ContextMenu;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.CompetitiveMode.View.Ranking
{
	public class UnityCompetitiveRankingPlayerView : EnhancedScrollerCellView, IItemView
	{
		public ILabel PositionLabel
		{
			get
			{
				return this._positionLabel;
			}
		}

		public ILabel PlayerNameLabel
		{
			get
			{
				return this._playerNameLabel;
			}
		}

		public ILabel PlayerTagLabel
		{
			get
			{
				return this._playerTagLabel;
			}
		}

		public ILabel ScoreLabel
		{
			get
			{
				return this._scoreLabel;
			}
		}

		public IIdentifiable Model { get; set; }

		public float Height
		{
			get
			{
				return this._height;
			}
		}

		public IDynamicImage SubdivisionDynamicImage
		{
			get
			{
				return this._subdivisionDynamicImage;
			}
		}

		public void AnimateComingFromAbove()
		{
			this._animation.clip = this._comingFromAboveAnimation;
			this._animation.Play();
		}

		public void AnimateComingFromBelow()
		{
			this._animation.clip = this._comingFromBelowAnimation;
			this._animation.Play();
		}

		public IObservable<Unit> AnimateFadingOut()
		{
			return this._fadingOutAnimation.Play();
		}

		private void SetColorConfiguration(UnityCompetitiveRankingPlayerViewColorConfiguration colorConfiguration)
		{
			this._positionBackgroundImage.Color = colorConfiguration.BackgroundColor.ToHmmColor();
			this._rankingBackgroundImage.Color = colorConfiguration.BackgroundColor.ToHmmColor();
			this._positionLabel.Color = colorConfiguration.PositionTextColor.ToHmmColor();
			this._playerNameLabel.Color = colorConfiguration.NameTextColor.ToHmmColor();
			this._scoreLabel.Color = colorConfiguration.ScoreTextColor.ToHmmColor();
		}

		public void SetAsLocalPlayer()
		{
			this.SetColorConfiguration(this._myPlayerColorConfiguration);
		}

		public void SetAsOtherPlayer()
		{
			this.SetColorConfiguration(this._otherPlayerColorConfiguration);
		}

		public EnchancedScrollerSocialContextMenuButtonView SocialContextMenuButtonView
		{
			get
			{
				return this._socialContextMenuButtonView;
			}
		}

		[SerializeField]
		private UnityLabel _positionLabel;

		[SerializeField]
		private UnityLabel _playerNameLabel;

		[SerializeField]
		private UnityLabel _playerTagLabel;

		[SerializeField]
		private UnityLabel _scoreLabel;

		[SerializeField]
		private float _height;

		[SerializeField]
		private Animation _animation;

		[SerializeField]
		private AnimationClip _comingFromAboveAnimation;

		[SerializeField]
		private AnimationClip _comingFromBelowAnimation;

		[SerializeField]
		private UnityAnimation _fadingOutAnimation;

		[SerializeField]
		private UnityDynamicImage _subdivisionDynamicImage;

		[SerializeField]
		private UnityImage _positionBackgroundImage;

		[SerializeField]
		private UnityImage _rankingBackgroundImage;

		[SerializeField]
		private EnchancedScrollerSocialContextMenuButtonView _socialContextMenuButtonView;

		[SerializeField]
		private UnityCompetitiveRankingPlayerViewColorConfiguration _myPlayerColorConfiguration;

		[SerializeField]
		private UnityCompetitiveRankingPlayerViewColorConfiguration _otherPlayerColorConfiguration;
	}
}
