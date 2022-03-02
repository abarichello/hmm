using System;
using HeavyMetalMachines.Players.Business;
using UniRx;

namespace HeavyMetalMachines.Matches
{
	public class RestoreCurrentMatch : IRestoreCurrentMatch
	{
		public RestoreCurrentMatch(ICurrentMatchStorage currentMatchStorage, IRunningMatchProvider runningMatchProvider, ILocalPlayerStorage playerStorage)
		{
			this._currentMatchStorage = currentMatchStorage;
			this._runningMatchProvider = runningMatchProvider;
			this._playerStorage = playerStorage;
		}

		public IObservable<bool> TryRestore()
		{
			return Observable.Select<GetRunningMatchResult, bool>(this._runningMatchProvider.GetRunningMatch(this._playerStorage.Player.UniversalId), new Func<GetRunningMatchResult, bool>(this.FillStorageAndReturnResult));
		}

		private bool FillStorageAndReturnResult(GetRunningMatchResult result)
		{
			if (result.FoundMatch)
			{
				this._currentMatchStorage.CurrentMatch = result.Match;
			}
			return result.FoundMatch;
		}

		private readonly ICurrentMatchStorage _currentMatchStorage;

		private readonly IRunningMatchProvider _runningMatchProvider;

		private readonly ILocalPlayerStorage _playerStorage;
	}
}
