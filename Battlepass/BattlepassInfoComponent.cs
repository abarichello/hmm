using System;
using Pocketverse;
using UnityEngine.SceneManagement;

namespace HeavyMetalMachines.Battlepass
{
	public class BattlepassInfoComponent : GameHubScriptableObject, IBattlepassInfoComponent
	{
		public void RegisterInfoView(IBattlepassInfoView view)
		{
			this._battlepassInfoView = view;
			this._battlepassInfoView.SetVisibility(true);
		}

		public void ShowInfoWindow(Action onWindowCloseAction)
		{
			if (GameHubScriptableObject.Hub != null)
			{
				GameHubScriptableObject.Hub.GuiScripts.TopRightButtonsMenu.TryCloseAll();
			}
			if (this._battlepassInfoView == null)
			{
				SceneManager.LoadSceneAsync("UI_ADD_BattlepassInfo", 1);
				this._onInfoWindowCloseAction = onWindowCloseAction;
				return;
			}
			if (this._battlepassInfoView.IsVisible())
			{
				return;
			}
			this._onInfoWindowCloseAction = onWindowCloseAction;
			this._battlepassInfoView.SetVisibility(true);
		}

		public void HideInfoWindow()
		{
			if (!this._battlepassInfoView.IsVisible())
			{
				return;
			}
			this._battlepassInfoView.SetVisibility(false);
			this._battlepassInfoView = null;
			if (this._onInfoWindowCloseAction != null)
			{
				this._onInfoWindowCloseAction();
				this._onInfoWindowCloseAction = null;
			}
		}

		public void OnInfoWindowHideAnimationEnded()
		{
			SceneManager.UnloadSceneAsync("UI_ADD_BattlepassInfo");
		}

		private const string BATTLEPASS_INFO_SCENE_NAME = "UI_ADD_BattlepassInfo";

		private IBattlepassInfoView _battlepassInfoView;

		private Action _onInfoWindowCloseAction;
	}
}
