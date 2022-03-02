using System;
using HeavyMetalMachines.Matches;

namespace HeavyMetalMachines.Audio.Fmod
{
	public class FmodVoiceOverService : IVoiceOverService
	{
		public FmodVoiceOverService(MatchPlayers matchPlayers)
		{
			this._matchPlayers = matchPlayers;
		}

		public void PlayVoiceOver(VoiceOverEventGroup eventGroup, MatchClient matchClient)
		{
			VoiceOverController voiceOverController = this.GetVoiceOverController(matchClient);
			voiceOverController.Play(eventGroup);
		}

		private VoiceOverController GetVoiceOverController(MatchClient matchClient)
		{
			return this._matchPlayers.GetPlayerOrBot(matchClient).CharacterInstance.GetBitComponent<VoiceOverController>();
		}

		private readonly MatchPlayers _matchPlayers;
	}
}
