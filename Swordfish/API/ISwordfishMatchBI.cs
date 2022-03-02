using System;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines.Swordfish.API
{
	public interface ISwordfishMatchBI
	{
		void Update();

		void GameOver(MatchController.GameOverMessage msg);

		void OnMatchLoaded();

		void ClientOnMatchEnded();

		void ServerOnMatchStarted();

		void ClientAnnouncerConfigured(int isChange);
	}
}
