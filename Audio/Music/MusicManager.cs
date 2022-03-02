using System;
using System.Collections.Generic;
using FMod;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.Context;
using Pocketverse;

namespace HeavyMetalMachines.Audio.Music
{
	public class MusicManager : GameHubBehaviour
	{
		private void Awake()
		{
			MusicManager._instance = this;
		}

		private void OnEnable()
		{
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.ListenToPhaseChange;
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.StateOnListenToStateChanged;
		}

		private void OnDisable()
		{
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.ListenToPhaseChange;
			GameHubBehaviour.Hub.State.ListenToStateChanged -= this.StateOnListenToStateChanged;
		}

		private void StateOnListenToStateChanged(GameState changedState)
		{
			GameState.GameStateKind stateKind = changedState.StateKind;
			if (stateKind != GameState.GameStateKind.Game)
			{
				Game game = GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.Game) as Game;
				if (game != null)
				{
					game.FinishedLoading -= this.GameFinishedLoading;
				}
				return;
			}
			((Game)GameHubBehaviour.Hub.State.Current).FinishedLoading += this.GameFinishedLoading;
		}

		private void GameFinishedLoading()
		{
			if (GameHubBehaviour.Hub.User.IsReconnecting)
			{
				return;
			}
			this.StopCurrentMusic();
		}

		private void ListenToPhaseChange(BombScoreboardState state)
		{
			if (state == BombScoreboardState.BombDelivery)
			{
				MusicManager.PlayMusic(MusicManager.State.InGame);
				return;
			}
			if (state == BombScoreboardState.Replay)
			{
				if (!GameHubBehaviour.Hub.BombManager.ScoreBoard.MatchOver)
				{
					MusicManager.PlayMusic(MusicManager.State.InGame);
				}
				return;
			}
			if (state != BombScoreboardState.PreReplay)
			{
				return;
			}
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.MatchOver)
			{
				MusicManager.StopMusic();
			}
		}

		public static void PlayMusic(MusicManager.State state)
		{
			if (MusicManager._instance == null)
			{
				MusicManager.Log.Error("No music manager found!");
				return;
			}
			MusicManager._instance.Play(state, true, true);
		}

		public static void PlayAmbienceOnly(MusicManager.State state)
		{
			if (MusicManager._instance == null)
			{
				MusicManager.Log.Error("No music manager found!");
				return;
			}
			MusicManager._instance.Play(state, false, true);
		}

		public static void StopMusic()
		{
			if (MusicManager._instance == null)
			{
				MusicManager.Log.Warn("No music manager found!");
			}
			else
			{
				MusicManager._instance.StopCurrentMusic();
			}
		}

		private void StopCurrentMusic()
		{
			Guid musicId = this.GetMusicId(this._currentState);
			FMODAudioManager.FMODAudio fmodaudio;
			if (!this._musicDictionary.TryGetValue(musicId, out fmodaudio))
			{
				if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.FMODDebug))
				{
					MusicManager.Log.DebugFormat("StopCurrentMusic: Music not found. id {0} state {1}", new object[]
					{
						musicId,
						this._currentState
					});
				}
				return;
			}
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.FMODDebug))
			{
				MusicManager.Log.DebugFormat("stopping music id {0}", new object[]
				{
					musicId
				});
			}
			if (fmodaudio != null)
			{
				fmodaudio.Stop();
			}
			this.currentMusic = null;
		}

		private void StopCurrentAmbience()
		{
			Guid musicId = this.GetMusicId(this._currentState);
			FMODAudioManager.FMODAudio fmodaudio;
			if (this._ambienceDictionary.TryGetValue(this.GetAmbienceId(this._currentState), out fmodaudio))
			{
				if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.FMODDebug))
				{
					MusicManager.Log.DebugFormat("stopping ambience id {0}", new object[]
					{
						musicId
					});
				}
				if (fmodaudio != null)
				{
					fmodaudio.Stop();
				}
				this.currentAmbience = null;
			}
			else if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.FMODDebug))
			{
				MusicManager.Log.DebugFormat("StopCurrentAmbience: Music not found. id {0} state {1}", new object[]
				{
					musicId,
					this._currentState
				});
			}
		}

		public static void PlayCharacterMusic(int characterMusicId)
		{
			if (MusicManager._instance == null)
			{
				MusicManager.Log.Error("No music manager found!");
			}
			MusicManager._instance.Play(MusicManager.State.CharacterPick, true, true);
			FMODAudioManager.FMODAudio fmodaudio;
			if (!MusicManager._instance._musicDictionary.TryGetValue(MusicManager._instance.GetMusicId(MusicManager.State.CharacterPick), out fmodaudio))
			{
				return;
			}
			fmodaudio.ChangeParameter(MusicManager._characterParameter, (float)characterMusicId);
		}

		public void Play(MusicManager.State state, bool playMusic = true, bool playAmbience = true)
		{
			this._currentState = state;
			MusicAndAmbience musicAndAmbience = this.GetMusicAndAmbience(state);
			if (musicAndAmbience == null)
			{
				MusicManager.Log.WarnFormat("Trying to play null music on state {0}. Please check AudioSettings.asset", new object[]
				{
					state
				});
				return;
			}
			if (playMusic && this.currentMusic != musicAndAmbience.MusicAsset)
			{
				MusicManager.Log.DebugFormat("Will play {0} music", new object[]
				{
					state
				});
				this.ChangePlayingAudio(this._musicDictionary, this.currentMusic, musicAndAmbience.MusicAsset);
				this.currentMusic = musicAndAmbience.MusicAsset;
			}
			if (playAmbience && this.currentAmbience != musicAndAmbience.AmbienceAsset)
			{
				MusicManager.Log.DebugFormat("Will play {0} ambience", new object[]
				{
					state
				});
				this.ChangePlayingAudio(this._ambienceDictionary, this.currentAmbience, musicAndAmbience.AmbienceAsset);
				this.currentAmbience = musicAndAmbience.AmbienceAsset;
			}
		}

		private MusicAndAmbience GetMusicAndAmbience(MusicManager.State state)
		{
			switch (state)
			{
			case MusicManager.State.MainMenu:
				return GameHubBehaviour.Hub.AudioSettings.MainMenu;
			case MusicManager.State.CharacterPick:
				return GameHubBehaviour.Hub.AudioSettings.CharacterPick;
			case MusicManager.State.InGame:
				return GameHubBehaviour.Hub.AudioSettings.InGame;
			case MusicManager.State.EndMatch:
				return GameHubBehaviour.Hub.AudioSettings.EndMatch;
			case MusicManager.State.CreateProfile:
				return GameHubBehaviour.Hub.AudioSettings.CreateProfile;
			case MusicManager.State.Welcome:
				return GameHubBehaviour.Hub.AudioSettings.Welcome;
			case MusicManager.State.RankedDrafter:
				return GameHubBehaviour.Hub.AudioSettings.RankedDrafter;
			case MusicManager.State.TournamentDrafter:
				return GameHubBehaviour.Hub.AudioSettings.TournamentDrafter;
			}
			MusicManager.Log.ErrorFormat("GetMusicAndAmbience in invalid state. State={0}", new object[]
			{
				state
			});
			return null;
		}

		private Guid GetMusicId(MusicManager.State state)
		{
			if (state == MusicManager.State.None)
			{
				return Guid.Empty;
			}
			MusicAndAmbience musicAndAmbience = this.GetMusicAndAmbience(state);
			return musicAndAmbience.MusicAsset.Id;
		}

		private Guid GetAmbienceId(MusicManager.State state)
		{
			if (state == MusicManager.State.None)
			{
				return Guid.Empty;
			}
			MusicAndAmbience musicAndAmbience = this.GetMusicAndAmbience(state);
			return musicAndAmbience.AmbienceAsset.Id;
		}

		private void ChangePlayingAudio(Dictionary<Guid, FMODAudioManager.FMODAudio> dictionary, AudioEventAsset currentAudioAsset, AudioEventAsset newAudioAsset)
		{
			if (!FMODAudioManager.StudioSystem.isValid())
			{
				return;
			}
			if (currentAudioAsset != null)
			{
				if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.FMODDebug))
				{
					MusicManager.Log.DebugFormat("stopping {0} with id {1}", new object[]
					{
						currentAudioAsset.name,
						currentAudioAsset.Id
					});
				}
				dictionary[currentAudioAsset.Id].Stop();
			}
			FMODAudioManager.FMODAudio fmodaudio;
			if (dictionary.TryGetValue(newAudioAsset.Id, out fmodaudio))
			{
				if (fmodaudio.IsInvalidated() || fmodaudio.IsStopped())
				{
					if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.FMODDebug))
					{
						MusicManager.Log.DebugFormat("Id {0} name {1} playing with reset timeline", new object[]
						{
							newAudioAsset.Id,
							newAudioAsset.name
						});
					}
					fmodaudio.ResetTimeline();
				}
			}
			else
			{
				if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.FMODDebug))
				{
					MusicManager.Log.DebugFormat("Id {0} name {1} added", new object[]
					{
						newAudioAsset.Id,
						newAudioAsset.name
					});
				}
				dictionary.Add(newAudioAsset.Id, FMODAudioManager.PlayAtVolume(newAudioAsset, base.transform, 1f, true));
			}
		}

		private void OnDestroy()
		{
			this.StopCurrentMusic();
			this.StopCurrentAmbience();
			this._currentState = MusicManager.State.None;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(MusicManager));

		private static readonly byte[] _characterParameter = FmodUtilities.GetBytes("Character");

		private AudioEventAsset currentMusic;

		private AudioEventAsset currentAmbience;

		private MusicManager.State _currentState;

		private static MusicManager _instance;

		private readonly Dictionary<Guid, FMODAudioManager.FMODAudio> _musicDictionary = new Dictionary<Guid, FMODAudioManager.FMODAudio>();

		private readonly Dictionary<Guid, FMODAudioManager.FMODAudio> _ambienceDictionary = new Dictionary<Guid, FMODAudioManager.FMODAudio>();

		public enum State
		{
			None,
			MainMenu,
			CharacterPick,
			InGame,
			MatchFound,
			CharacterTheme,
			EndMatch,
			CreateProfile = 9,
			Welcome,
			RankedDrafter,
			TournamentDrafter
		}
	}
}
