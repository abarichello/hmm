using System;

namespace HeavyMetalMachines.CompetitiveMode.View.Matchmaking
{
	public interface ICompetitiveQueueJoinPresenter : IDisposable
	{
		string ViewProviderContext { get; set; }

		void Initialize();

		void Enable();

		void Disable();
	}
}
