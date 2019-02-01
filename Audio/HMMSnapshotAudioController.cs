using System;
using FMod;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Audio
{
	public class HMMSnapshotAudioController : GameHubObject
	{
		public void RegisterEvents()
		{
			GameHubObject.Hub.State.ListenToStateChanged += this.StateOnListenToStateChanged;
			GameHubObject.Hub.Server.OnMatchStateChanged += this.ServerOnMatchStateChanged;
		}

		private void StateOnListenToStateChanged(GameState changedstate)
		{
			if (changedstate.StateKind != GameState.GameStateKind.Game && this._matchEndSnapshotAudio != null && !this._matchEndSnapshotAudio.IsInvalidated())
			{
				this._matchEndSnapshotAudio.Stop();
			}
		}

		private void ServerOnMatchStateChanged(MatchData.MatchState newMatchstate)
		{
			if (newMatchstate == MatchData.MatchState.MatchOverBluWins || newMatchstate == MatchData.MatchState.MatchOverRedWins)
			{
				this._matchEndSnapshotAudio = FMODAudioManager.PlayAt(GameHubObject.Hub.AudioSettings.MatchEndSnapshot, GameHubObject.Hub.transform);
			}
		}

		public void UnegisterEvents()
		{
			GameHubObject.Hub.State.ListenToStateChanged -= this.StateOnListenToStateChanged;
			GameHubObject.Hub.Server.OnMatchStateChanged -= this.ServerOnMatchStateChanged;
		}

		private FMODAudioManager.FMODAudio _matchEndSnapshotAudio;

		private FMODAudioManager.FMODAudio _deathSnapshotAudio;
	}
}
