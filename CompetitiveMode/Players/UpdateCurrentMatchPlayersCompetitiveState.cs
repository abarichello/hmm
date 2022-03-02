using System;
using System.Collections.Generic;
using System.Linq;
using HeavyMetalMachines.Matches;
using Standard_Assets.Scripts.HMM.GameStates.Matches.Exceptions;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Players
{
	public class UpdateCurrentMatchPlayersCompetitiveState : IUpdateCurrentMatchPlayersCompetitiveState
	{
		public UpdateCurrentMatchPlayersCompetitiveState(IGetCurrentMatch getCurrentMatch, IUpdatePlayerState updatePlayerState)
		{
			this._getCurrentMatch = getCurrentMatch;
			this._updatePlayerState = updatePlayerState;
		}

		public IObservable<Unit> Update()
		{
			Match? match = new Match?(GetCurrentMatchExtensions.Get(this._getCurrentMatch));
			if (match == null)
			{
				throw new NoCurrentMatchException();
			}
			IEnumerable<long> source = from p in match.Value.Clients
			where !p.IsBot
			select p.PlayerId;
			return this._updatePlayerState.Update(source.ToArray<long>());
		}

		private readonly IGetCurrentMatch _getCurrentMatch;

		private readonly IUpdatePlayerState _updatePlayerState;
	}
}
