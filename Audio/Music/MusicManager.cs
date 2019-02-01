using System;
using System.Collections.Generic;
using FMod;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Frontend;
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
			GameHubBehaviour.Hub.BombManager.ListenToOvertimeStarted += this.ListenToOvertimeStarted;
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.StateOnListenToStateChanged;
		}

		private void OnDisable()
		{
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.ListenToPhaseChange;
			GameHubBehaviour.Hub.BombManager.ListenToOvertimeStarted -= this.ListenToOvertimeStarted;
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

		private void ListenToOvertimeStarted()
		{
			MusicManager.PlayMusic(MusicManager.State.Overtime);
		}

		private void ListenToPhaseChange(BombScoreBoard.State state)
		{
			if (state != BombScoreBoard.State.BombDelivery && state != BombScoreBoard.State.Replay)
			{
				return;
			}
			MusicManager.PlayMusic(MusicManager.State.InGame);
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
				}
				return;
			}
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.FMODDebug))
			{
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
				}
				if (fmodaudio != null)
				{
					fmodaudio.Stop();
				}
				this.currentAmbience = null;
			}
			else if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.FMODDebug))
			{
			}
		}

		public static void PlayCharacterMusic(CharacterInfo character)
		{
			if (MusicManager._instance == null)
			{
				MusicManager.Log.Error("No music manager found!");
			}
			MusicManager._instance.Play(MusicManager.State.CharacterPick, true, true);
			int num = (!(character == null)) ? character.characterMusicID : 0;
			FMODAudioManager.FMODAudio fmodaudio;
			if (!MusicManager._instance._musicDictionary.TryGetValue(MusicManager._instance.GetMusicId(MusicManager.State.CharacterPick), out fmodaudio))
			{
				return;
			}
			fmodaudio.ChangeParameter(MusicManager._instance.CHARACTERPARAMETER, (float)num);
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
				this.ChangePlayingAudio(this._musicDictionary, this.currentMusic, musicAndAmbience.MusicAsset);
				this.currentMusic = musicAndAmbience.MusicAsset;
			}
			if (playAmbience && this.currentAmbience != musicAndAmbience.AmbienceAsset)
			{
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
			case MusicManager.State.PreOvertime:
				return GameHubBehaviour.Hub.AudioSettings.PreOvertime;
			case MusicManager.State.Overtime:
				return GameHubBehaviour.Hub.AudioSettings.Overtime;
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
			return musicAndAmbience.MusicAsset.idGUID;
		}

		private Guid GetAmbienceId(MusicManager.State state)
		{
			if (state == MusicManager.State.None)
			{
				return Guid.Empty;
			}
			MusicAndAmbience musicAndAmbience = this.GetMusicAndAmbience(state);
			return musicAndAmbience.AmbienceAsset.idGUID;
		}

		private void ChangePlayingAudio(Dictionary<Guid, FMODAudioManager.FMODAudio> dictionary, FMODAsset currentAudioAsset, FMODAsset newAudioAsset)
		{
			if (currentAudioAsset != null)
			{
				if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.FMODDebug))
				{
				}
				dictionary[currentAudioAsset.idGUID].Stop();
			}
			FMODAudioManager.FMODAudio fmodaudio;
			if (dictionary.TryGetValue(newAudioAsset.idGUID, out fmodaudio))
			{
				if (fmodaudio.IsInvalidated() || fmodaudio.IsStopped())
				{
					if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.FMODDebug))
					{
					}
					fmodaudio.ResetTimeline();
				}
			}
			else
			{
				if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.FMODDebug))
				{
				}
				dictionary.Add(newAudioAsset.idGUID, FMODAudioManager.PlayAtVolume(newAudioAsset, base.transform, 1f, true));
			}
		}

		private void OnDestroy()
		{
			this.StopCurrentMusic();
			this.StopCurrentAmbience();
			this._currentState = MusicManager.State.None;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(MusicManager));

		private readonly byte[] CHARACTERPARAMETER = FMODAudioManager.GetBytes("Character");

		private FMODAsset currentMusic;

		private FMODAsset currentAmbience;

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
			PreOvertime,
			Overtime
		}
	}
}
