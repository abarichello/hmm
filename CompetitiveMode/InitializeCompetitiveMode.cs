using System;
using HeavyMetalMachines.CompetitiveMode.Seasons;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode
{
	public class InitializeCompetitiveMode : IInitializeCompetitiveMode
	{
		public InitializeCompetitiveMode(IUpdateSeasons updateSeasons)
		{
			this._updateSeasons = updateSeasons;
		}

		public IObservable<Unit> Initialize()
		{
			return this.UpdateSeasons();
		}

		private IObservable<Unit> UpdateSeasons()
		{
			return this._updateSeasons.Update();
		}

		private readonly IUpdateSeasons _updateSeasons;
	}
}
