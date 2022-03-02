using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;
using UniRx;

namespace HeavyMetalMachines.Battlepass.Rewards.View
{
	public interface IBattlepassRewardsView
	{
		IButton NextRewardButton { get; }

		IButton ClaimAllButton { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }

		IObservable<Unit> PlayInAnimation();

		IObservable<Unit> PlayOutAnimation();

		void LegacyOnClickNextRewardButton();

		void LegacyOnClickClaimAllButton();

		IObservable<Unit> LegacyObserveHide();
	}
}
