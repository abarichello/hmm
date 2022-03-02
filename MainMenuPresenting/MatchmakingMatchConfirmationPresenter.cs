using System;
using HeavyMetalMachines.Matchmaking.Queue;
using HeavyMetalMachines.Presenting;
using UniRx;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public class MatchmakingMatchConfirmationPresenter : IMatchmakingMatchConfirmation
	{
		public MatchmakingMatchConfirmationPresenter(IViewProvider viewProvider)
		{
			this._viewProvider = viewProvider;
		}

		public IObservable<bool> ConfirmMatch()
		{
			IMatchmakingMatchAcceptView matchmakingMatchAcceptView = this._viewProvider.Provide<IMatchmakingMatchAcceptView>(null);
			IObservable<bool>[] array = new IObservable<bool>[2];
			array[0] = Observable.Select<Unit, bool>(matchmakingMatchAcceptView.AcceptButton.OnClick(), (Unit _) => true);
			array[1] = Observable.Select<Unit, bool>(matchmakingMatchAcceptView.DeclineButton.OnClick(), (Unit _) => false);
			return Observable.First<bool>(Observable.Merge<bool>(array));
		}

		private readonly IViewProvider _viewProvider;
	}
}
