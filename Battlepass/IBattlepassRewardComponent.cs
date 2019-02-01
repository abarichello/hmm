using System;

namespace HeavyMetalMachines.Battlepass
{
	public interface IBattlepassRewardComponent
	{
		UnityUIBattlepassRewardView.DataReward RegisterRewardView(IBattlepassRewardView view);

		bool TryToOpenRewardsToClaim(Action onWindowCloseAction);

		void ShowRewardWindow(Action onWindowCloseAction);

		void HideRewardWindow();

		void OnRewardWindowDispose();

		void ClaimReward(int levelToClaim, bool premiumClaim);
	}
}
