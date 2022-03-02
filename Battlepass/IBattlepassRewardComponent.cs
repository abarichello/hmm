using System;
using System.Collections.Generic;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Store.Business;

namespace HeavyMetalMachines.Battlepass
{
	public interface IBattlepassRewardComponent
	{
		void SetStoreBusinessFactory(IStoreBusinessFactory storeBusinessFactory);

		UnityUIBattlepassRewardView.DataReward RegisterRewardView(IBattlepassRewardView view);

		bool HasRewardToClaim();

		void HideRewardWindow();

		void OnRewardWindowDispose();

		void ClaimReward(int levelToClaim, bool premiumClaim);

		void ClaimAllRewards(List<ClaimRewardInfo> rewardsInfo, Action onRewardsClaimed);
	}
}
