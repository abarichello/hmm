using System;
using Commons.Swordfish.Battlepass;
using HeavyMetalMachines.Infra.ScriptableObjects;
using Pocketverse;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeavyMetalMachines.Battlepass
{
	public class BattlepassDetailComponent : GameHubScriptableObject, IBattlepassDetailComponent
	{
		public UnityUiBattlepassDetailView.BattlepassDetailViewData RegisterDetailView(IBattlepassDetailView view)
		{
			this._battlepassDetailView = view;
			BattlepassConfig battlepass = GameHubScriptableObject.Hub.SharedConfigs.Battlepass;
			DateTime startDate = battlepass.GetStartDate();
			DateTime endDate = battlepass.GetEndDate();
			return new UnityUiBattlepassDetailView.BattlepassDetailViewData
			{
				StartDay = startDate.Day,
				StartMonth = "BATTLEPASS_WELCOME_START_DATE",
				EndDay = endDate.Day,
				EndMonth = "BATTLEPASS_WELCOME_END_DATE"
			};
		}

		public bool TryToShowDetailWindow(Action<bool> onWindowCloseAction)
		{
			if (!this.CanOpenDetailWindow())
			{
				return false;
			}
			if (this._battlepassDetailView == null)
			{
				this._onDetailWindowCloseAction = onWindowCloseAction;
				SceneManager.LoadSceneAsync("UI_ADD_BattlepassDetail", LoadSceneMode.Additive);
				this.MarkDetailWindowOpened();
				return true;
			}
			if (!this._battlepassDetailView.IsVisible())
			{
				this._onDetailWindowCloseAction = onWindowCloseAction;
				this._battlepassDetailView.SetVisibility(true);
				return true;
			}
			return false;
		}

		public void HideDetailWindow(bool showMetalpassWindow)
		{
			if (this._battlepassDetailView != null && this._battlepassDetailView.IsVisible())
			{
				if (this._onDetailWindowCloseAction != null)
				{
					this._onDetailWindowCloseAction(showMetalpassWindow);
					this._onDetailWindowCloseAction = null;
				}
				SceneManager.UnloadSceneAsync("UI_ADD_BattlepassDetail");
				this._battlepassDetailView = null;
			}
		}

		private void MarkDetailWindowOpened()
		{
			BattlepassDetailComponent._detailWindowHasOpened = true;
		}

		private bool CanOpenDetailWindow()
		{
			if (BattlepassDetailComponent._detailWindowHasOpened)
			{
				return false;
			}
			BattlepassProgress progress = this._battlepassProgressScriptableObject.Progress;
			return progress.Season > 0 && progress.CurrentXp == 0 && !progress.HasPremium();
		}

		[SerializeField]
		private BattlepassProgressScriptableObject _battlepassProgressScriptableObject;

		private IBattlepassDetailView _battlepassDetailView;

		private Action<bool> _onDetailWindowCloseAction;

		private static bool _detailWindowHasOpened;
	}
}
