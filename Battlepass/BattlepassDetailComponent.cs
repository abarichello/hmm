using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.ScriptableObjects;
using Pocketverse;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeavyMetalMachines.Battlepass
{
	public class BattlepassDetailComponent : GameHubScriptableObject, IBattlepassDetailComponent
	{
		public void RegisterDetailView(IBattlepassDetailView view)
		{
			this._battlepassDetailView = view;
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
				SceneManager.LoadSceneAsync("UI_ADD_BattlepassDetail", 1);
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

		public BattlepassConfig BattlepassConfig
		{
			get
			{
				return GameHubScriptableObject.Hub.SharedConfigs.Battlepass;
			}
		}

		private void MarkDetailWindowOpened()
		{
			BattlepassDetailComponent._detailWindowHasOpened = true;
		}

		public bool CanOpenDetailWindow()
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
