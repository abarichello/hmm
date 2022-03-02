using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HeavyMetalMachines.DriverHelper;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Options.Presenting;
using HeavyMetalMachines.Pause;
using UniRx;

namespace HeavyMetalMachines.RadialMenu.View
{
	public class CanShowInGameOverlay : ICanShowInGameOverlay
	{
		public CanShowInGameOverlay(IHudTabPresenter hudTabPresenter, IHudChatPresenter hudChatPresenter, IDriverHelper driverHelperPresenter, IPausePresenter pausePresenter, IOptionsPresenter optionsPresenter, IScoreBoard gameModeStateProvider)
		{
			this._hudTabPresenter = hudTabPresenter;
			this._hudChatPresenter = hudChatPresenter;
			this._driverHelperPresenter = driverHelperPresenter;
			this._pausePresenter = pausePresenter;
			this._optionsPresenter = optionsPresenter;
			this._gameModeStateProvider = gameModeStateProvider;
		}

		public IObservable<bool> GetThenObserveCanShow()
		{
			IObservable<IList<bool>> observable = Observable.CombineLatest<bool>(new IObservable<bool>[]
			{
				this.GetHudChatVisibilityObservation(),
				this.GetHudTabVisibilityObservation(),
				this.GetDriverHelperVisibilityObservation(),
				this.GetHudPauseVisibilityObservation(),
				this.GetOptionsWindowVisibilityObservation(),
				this.GetGameModeChangedObservation()
			});
			if (CanShowInGameOverlay.<>f__mg$cache0 == null)
			{
				CanShowInGameOverlay.<>f__mg$cache0 = new Func<IList<bool>, bool>(CanShowInGameOverlay.AllValuesAreFalse);
			}
			return Observable.Select<IList<bool>, bool>(observable, CanShowInGameOverlay.<>f__mg$cache0);
		}

		private static bool AllValuesAreFalse(IList<bool> values)
		{
			return values.All((bool value) => !value);
		}

		private IObservable<bool> GetHudTabVisibilityObservation()
		{
			return Observable.StartWith<bool>(this._hudTabPresenter.VisibilityChanged(), () => this._hudTabPresenter.Visible);
		}

		private IObservable<bool> GetHudChatVisibilityObservation()
		{
			return Observable.StartWith<bool>(this._hudChatPresenter.VisibilityChanged(), () => this._hudChatPresenter.Visible);
		}

		private IObservable<bool> GetDriverHelperVisibilityObservation()
		{
			return Observable.StartWith<bool>(this._driverHelperPresenter.VisibilityChanged(), () => this._driverHelperPresenter.Visible);
		}

		private IObservable<bool> GetHudPauseVisibilityObservation()
		{
			return Observable.StartWith<bool>(this._pausePresenter.VisibilityChanged(), () => this._pausePresenter.Visible);
		}

		private IObservable<bool> GetOptionsWindowVisibilityObservation()
		{
			return Observable.StartWith<bool>(this._optionsPresenter.VisibilityChanged(), () => this._optionsPresenter.Visible);
		}

		private IObservable<bool> GetGameModeChangedObservation()
		{
			return Observable.StartWith<bool>(Observable.Select<BombScoreboardState, bool>(Observable.Select<ScoreBoardState, BombScoreboardState>(this._gameModeStateProvider.StateChangedObservation, (ScoreBoardState gameStateChange) => gameStateChange.CurrentState), new Func<BombScoreboardState, bool>(this.IsInvalidState)), () => this.IsInvalidState(this._gameModeStateProvider.CurrentState));
		}

		private bool IsInvalidState(BombScoreboardState gameState)
		{
			return gameState == BombScoreboardState.Warmup || gameState == BombScoreboardState.PreReplay || gameState == BombScoreboardState.Replay || gameState == BombScoreboardState.EndGame;
		}

		private readonly IHudTabPresenter _hudTabPresenter;

		private readonly IHudChatPresenter _hudChatPresenter;

		private readonly IDriverHelper _driverHelperPresenter;

		private readonly IPausePresenter _pausePresenter;

		private readonly IOptionsPresenter _optionsPresenter;

		private readonly IScoreBoard _gameModeStateProvider;

		[CompilerGenerated]
		private static Func<IList<bool>, bool> <>f__mg$cache0;
	}
}
