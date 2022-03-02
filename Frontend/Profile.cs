using System;
using HeavyMetalMachines.Social.Profile.Presenting;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class Profile : GameState
	{
		protected override void OnMyLevelLoaded()
		{
			ICreateProfilePresenter createProfilePresenter = this._diContainer.Resolve<ICreateProfilePresenter>();
			ObservableExtensions.Subscribe<Unit>(Observable.DoOnCompleted<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(createProfilePresenter.Initialize(), delegate(Unit _)
			{
				GameHubBehaviour.Hub.GuiScripts.Loading.HideLoading();
			}), (Unit _) => createProfilePresenter.Show()), createProfilePresenter.FinishObservation), new Action(this.GoToMainMenu)));
		}

		private void GoToMainMenu()
		{
			base.GoToState(this._mainMenu, true);
		}

		[Inject]
		private DiContainer _diContainer;

		[SerializeField]
		private MainMenu _mainMenu;
	}
}
