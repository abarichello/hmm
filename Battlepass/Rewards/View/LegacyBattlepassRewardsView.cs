using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Battlepass.Rewards.View
{
	public class LegacyBattlepassRewardsView : MonoBehaviour, IBattlepassRewardsView
	{
		public IButton NextRewardButton
		{
			get
			{
				return this._nextRewardButton;
			}
		}

		public IButton ClaimAllButton
		{
			get
			{
				return this._claimAllButtonButton;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public IObservable<Unit> PlayInAnimation()
		{
			this._legacyBattlepassRewardView.SetVisibility(true);
			return Observable.ReturnUnit();
		}

		public IObservable<Unit> PlayOutAnimation()
		{
			this._legacyBattlepassRewardView.SetVisibility(false);
			return Observable.ReturnUnit();
		}

		public void LegacyOnClickNextRewardButton()
		{
			this._legacyBattlepassRewardView.OnButtonNextReward();
		}

		public void LegacyOnClickClaimAllButton()
		{
			this._legacyBattlepassRewardView.OnButtonClaimAllRewards();
		}

		public IObservable<Unit> LegacyObserveHide()
		{
			return this._legacyBattlepassRewardView.ObserveHide();
		}

		private void Awake()
		{
			this._viewProvider.Bind<IBattlepassRewardsView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IBattlepassRewardsView>(null);
		}

		[Inject]
		[UsedImplicitly]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityButton _nextRewardButton;

		[SerializeField]
		private UnityButton _claimAllButtonButton;

		[SerializeField]
		private UnityUIBattlepassRewardView _legacyBattlepassRewardView;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;
	}
}
