using System;
using System.Collections.Generic;
using ClientAPI.Objects;
using UniRx;

namespace HeavyMetalMachines.Teams
{
	public interface IMenuTeamsService
	{
		IObservable<Team> GetGroupTeam(List<string> universalIds);
	}
}
