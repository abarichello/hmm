using System;
using Assets.ClientApiObjects.Components.API;
using HeavyMetalMachines.Arena.Infra;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Presenting;
using Hoplon.Localization.TranslationTable;
using UniRx;

namespace HeavyMetalMachines.Training.Presenter
{
	public class TrainingMatchResultPresenter : ITrainingMatchResultPresenter, IPresenter
	{
		public TrainingMatchResultPresenter(IViewLoader viewLoader, IViewProvider viewProvider)
		{
			this._viewLoader = viewLoader;
			this._viewProvider = viewProvider;
		}

		IObservable<Unit> IPresenter.Initialize()
		{
			return Observable.AsUnitObservable<Unit>(Observable.Do<Unit>(this._viewLoader.LoadView("UI_ADD_EndScreen_TrainingMode"), delegate(Unit _)
			{
				this.InitializeView();
			}));
		}

		IObservable<Unit> IPresenter.Show()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._traninigMatchResultView.Canvas.Enable();
				this._traninigMatchResultView.GoBackToMainMenuButton.IsInteractable = false;
				this._traninigMatchResultView.UiNavigationGroupHolder.AddGroup();
				return Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.NextFrame(0), (Unit _) => this.PlayEntryAnimation()), delegate(Unit _)
				{
					this.EnableButton();
				});
			});
		}

		IObservable<Unit> IPresenter.Hide()
		{
			throw new NotImplementedException();
		}

		IObservable<Unit> IPresenter.Dispose()
		{
			throw new NotImplementedException();
		}

		IObservable<Unit> IPresenter.ObserveHide()
		{
			throw new NotImplementedException();
		}

		private void InitializeView()
		{
			this._traninigMatchResultView = this._viewProvider.Provide<ITrainingMatchResultView>(null);
			this._traninigMatchResultView.Canvas.Disable();
			ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.First<Unit>(this._traninigMatchResultView.GoBackToMainMenuButton.OnClick()), (Unit _) => this._traninigMatchResultView.HideAnimation.Play()), delegate(Unit _)
			{
				this.GoToMainMenu();
			}));
			IGameModeItemTypeComponent currentArenaMode = this._arenaConfigProvider.GameArenaConfig.GetCurrentArenaMode();
			this._traninigMatchResultView.ArenaName.Text = this._translation.Get(currentArenaMode.GameModeNameDraft, TranslationContext.TrainingMode);
			this._traninigMatchResultView.BackgroundImage.SetImageName(currentArenaMode.BackgroundName);
			this._traninigMatchResultView.IconArena.SetImageName(currentArenaMode.IconName);
			this._traninigMatchResultView.IconGlow.SetImageName(currentArenaMode.IconName);
			bool flag = this._matchWinProvider.MatchWin();
			string text = (!flag) ? this._traninigMatchResultView.MatchLoseDraft : this._traninigMatchResultView.MatchWinDraft;
			this._traninigMatchResultView.MatchConcludeLabel.Text = this._translation.Get(text, TranslationContext.TrainingMode);
			Color color = (!flag) ? this._traninigMatchResultView.LoseColor : this._traninigMatchResultView.WinColor;
			this._traninigMatchResultView.MatchConcludeLabel.Color = color;
			string imageName = (!flag) ? this._traninigMatchResultView.AssertIconMatchLoseName : this._traninigMatchResultView.AssertIconMatchWonName;
			this._traninigMatchResultView.AssertIcon.SetImageName(imageName);
			this._traninigMatchResultView.AssertIconGlow.SetImageName(imageName);
		}

		private void EnableButton()
		{
			this._traninigMatchResultView.GoBackToMainMenuButton.IsInteractable = true;
		}

		private void GoToMainMenu()
		{
			this._mainMenuInitialization.NodeToGo = MainMenuNode.TrainingScreen;
			this._gameState.ClearBackToMain();
		}

		private IObservable<Unit> PlayEntryAnimation()
		{
			bool flag = this._matchWinProvider.MatchWin();
			if (flag)
			{
				return this._traninigMatchResultView.ShowAnimationVictory.Play();
			}
			return this._traninigMatchResultView.ShowAnimationDefeat.Play();
		}

		private const string TrainigModeEndScreenSceneName = "UI_ADD_EndScreen_TrainingMode";

		private readonly IViewProvider _viewProvider;

		private readonly IViewLoader _viewLoader;

		private ITrainingMatchResultView _traninigMatchResultView;

		[InjectOnClient]
		private IStateGame _gameState;

		[InjectOnClient]
		private IGameArenaConfigProvider _arenaConfigProvider;

		[InjectOnClient]
		private IMatchWinProvider _matchWinProvider;

		[InjectOnClient]
		private ILocalizeKey _translation;

		[InjectOnClient]
		private IMainMenuInitialization _mainMenuInitialization;
	}
}
