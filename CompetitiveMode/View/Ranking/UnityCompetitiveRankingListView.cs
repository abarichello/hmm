using System;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Players.Presenting;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Social;
using HeavyMetalMachines.ToggleableFeatures;
using Hoplon.Input.UiNavigation.AxisSelector;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace HeavyMetalMachines.CompetitiveMode.View.Ranking
{
	public class UnityCompetitiveRankingListView : MonoBehaviour, ICompetitiveRankingListView
	{
		public IScroller<CompetitiveRankingPlayerScrollerData> RankingScroller
		{
			get
			{
				return this._rankingScrollerDelegate;
			}
		}

		public IActivatable LoadingActivatable
		{
			get
			{
				return this._loadingIndicator;
			}
		}

		public IButton RefreshButton
		{
			get
			{
				return this._refreshButton;
			}
		}

		public IAnimation FadeInAnimation
		{
			get
			{
				return this._fadeInAnimation;
			}
		}

		public IAnimation FadeOutAnimation
		{
			get
			{
				return this._fadeOutAnimation;
			}
		}

		public ILabel LastUpdatedLabel
		{
			get
			{
				return this._lastUpdatedLabel;
			}
		}

		public IActivatable EmptyRankingGroup
		{
			get
			{
				return this._emptyRankingGroup;
			}
		}

		public IObservable<Unit> AnimateFadeOutScrollerItems()
		{
			return this._rankingScrollerDelegate.FadeOutAllItems();
		}

		public void DisableRankingScroll()
		{
			this._rankingScrollbar.interactable = false;
			this._rankingScrollbarRect.vertical = false;
		}

		public void EnableRankingScroll()
		{
			this._rankingScrollbar.interactable = true;
			this._rankingScrollbarRect.vertical = true;
		}

		private void Awake()
		{
			this._rankingScrollerDelegate = new CompetitiveRankingScrollerDelegate(this._rankingScroller, this._playerRankingViewPrefab, this._disableLabelForFeatureNotEnable, this._getFormattedPlayerTag, this._getPublisherUserName, this._getDisplayableNickName, this._badNameCensor);
			this._viewProvider.Bind<ICompetitiveRankingListView>(this, null);
		}

		private void OnDestroy()
		{
			this._rankingScrollerDelegate.Dispose();
			this._viewProvider.Unbind<ICompetitiveRankingListView>(null);
		}

		[SerializeField]
		private EnhancedScroller _rankingScroller;

		[SerializeField]
		private UnityCompetitiveRankingPlayerView _playerRankingViewPrefab;

		[SerializeField]
		private GameObjectActivatable _loadingIndicator;

		[SerializeField]
		private UnityButton _refreshButton;

		[SerializeField]
		private UnityAnimation _fadeInAnimation;

		[SerializeField]
		private UnityAnimation _fadeOutAnimation;

		[SerializeField]
		private UnityLabel _lastUpdatedLabel;

		[SerializeField]
		private Scrollbar _rankingScrollbar;

		[SerializeField]
		private ScrollRect _rankingScrollbarRect;

		[SerializeField]
		private GameObjectActivatable _emptyRankingGroup;

		[SerializeField]
		public UiNavigationAxisSelector _axisSelector;

		[InjectOnClient]
		[UsedImplicitly]
		private IViewProvider _viewProvider;

		[Inject]
		private IDisableLabelForFeatureNotEnable _disableLabelForFeatureNotEnable;

		[Inject]
		private IGetFormattedPlayerTag _getFormattedPlayerTag;

		[Inject]
		private IGetPublisherUserName _getPublisherUserName;

		[Inject]
		private IBadNameCensor _badNameCensor;

		[Inject]
		private IGetDisplayableNickName _getDisplayableNickName;

		private CompetitiveRankingScrollerDelegate _rankingScrollerDelegate;
	}
}
