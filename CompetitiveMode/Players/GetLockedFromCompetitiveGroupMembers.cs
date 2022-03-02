using System;
using System.Collections.Generic;
using System.Linq;
using HeavyMetalMachines.Groups.Business.Exceptions;
using HeavyMetalMachines.Social.Groups.Business;
using HeavyMetalMachines.Social.Groups.Models;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Players
{
	public class GetLockedFromCompetitiveGroupMembers : IGetLockedFromCompetitiveGroupMembers
	{
		public GetLockedFromCompetitiveGroupMembers(IGroupStorage groupStorage, IGetOrFetchPlayerState getOrFetchPlayerState)
		{
			this._groupStorage = groupStorage;
			this._getOrFetchPlayerState = getOrFetchPlayerState;
		}

		public IObservable<long[]> Get()
		{
			this.AssertPlayerIsInGroup();
			long[] playerId = (from member in this._groupStorage.Group.Members
			select member.PlayerId).ToArray<long>();
			return Observable.Select<PlayerCompetitiveState[], long[]>(this._getOrFetchPlayerState.GetFromPlayersIds(playerId), new Func<PlayerCompetitiveState[], long[]>(this.FilterLockedPlayers));
		}

		private long[] FilterLockedPlayers(PlayerCompetitiveState[] states)
		{
			List<long> list = new List<long>(states.Length);
			foreach (PlayerCompetitiveState playerCompetitiveState in states)
			{
				if (playerCompetitiveState.Status == null)
				{
					list.Add(playerCompetitiveState.PlayerId);
				}
			}
			return list.ToArray();
		}

		private void AssertPlayerIsInGroup()
		{
			Group group = this._groupStorage.Group;
			if (group == null || group.Members.Count == 0)
			{
				throw new PlayerNotInGroupException();
			}
		}

		private readonly IGroupStorage _groupStorage;

		private readonly IGetOrFetchPlayerState _getOrFetchPlayerState;
	}
}
