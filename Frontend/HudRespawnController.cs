using System;
using FMod;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudRespawnController : GameHubBehaviour, PlayerBuildComplete.IPlayerBuildCompleteListener
	{
		private void Awake()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				Object.Destroy(this);
				return;
			}
			this.AnimationQueue = new AnimationQueue();
			this.ShouldSetRespawningTime = (this.RespawningNumberAnimation != null);
			for (int i = 0; i < this.Animations.Length; i++)
			{
				HudRespawnController.AnimationEvent animationEvent = this.Animations[i];
				switch (animationEvent.EvenKind)
				{
				case PlayerEvent.Kind.Unspawn:
					this.UnspawnAnimationIndex = i;
					break;
				case PlayerEvent.Kind.Respawn:
					this.SpawnAnimationIndex = i;
					break;
				case PlayerEvent.Kind.PreRespawn:
					this.PreSpawnAnimationIndex = i;
					break;
				case PlayerEvent.Kind.Respawning:
					this.RespawningAnimationIndex = i;
					break;
				}
			}
		}

		private void OnDestroy()
		{
			this.Combat = null;
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery -= this.OnBombDelivery;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChanged;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn -= this.OnPlayerUnspawned;
			GameHubBehaviour.Hub.Events.Players.ListenToPreObjectSpawn -= this.OnPlayerPreSpawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectRespawning -= this.OnPlayerRespawning;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn -= this.OnPlayerSpawn;
			GameHubBehaviour.Hub.Events.Bots.ListenToObjectUnspawn -= this.OnPlayerUnspawned;
			GameHubBehaviour.Hub.Events.Bots.ListenToPreObjectSpawn -= this.OnPlayerPreSpawn;
			GameHubBehaviour.Hub.Events.Bots.ListenToObjectRespawning -= this.OnPlayerRespawning;
			GameHubBehaviour.Hub.Events.Bots.ListenToObjectSpawn -= this.OnPlayerSpawn;
			this.StopPreRespawnCountdownAudio();
		}

		protected virtual void Update()
		{
			if (Time.frameCount % 2 == 0)
			{
				return;
			}
			if (!this.Configured || this.BombAlreadyDelivered || this.GameOver)
			{
				return;
			}
			if (this.Combat.IsAlive())
			{
				return;
			}
			if (this.AnimationQueue != null)
			{
				this.AnimationQueue.Update();
			}
			this.UpdateTimers();
			this.UpdateAudio();
			int num = Mathf.Clamp(this._remainingDeadTimeSeconds, 0, 59);
			this.DeathTimerNumberCountdown.text = HudRespawnController.CachedCountDown[num];
			if (this.ShouldSetRespawningTime && this.RespawningTime >= 0f && this.DeathTimeMillis <= 0f)
			{
				this._previousRespawningTimeSeconds = this._remainingRespawningTimeSeconds;
				this._remainingRespawningTimeSeconds = (int)(this.RespawningTime * HudUtils.MillisToSeconds);
				num = Mathf.Clamp(this._remainingRespawningTimeSeconds, 0, 59);
				this.RespawningNumberCountdown.text = HudRespawnController.CachedCountDown[num];
				if (this.RespawningTime > 0f && (int)this.RespawningTime - 200 != this.LastRespawningTime)
				{
					this.RespawningNumberAnimation.Play(this.RespawningNumberAnimationClipName);
				}
				if (this._remainingRespawningTimeSeconds != this._previousRespawningTimeSeconds)
				{
					FMODAudioManager.PlayOneShotAt(this._respawnCounterAsset, Vector3.zero, 0);
				}
			}
		}

		private void UpdateAudio()
		{
			if (this._remainingDeadTimeSeconds != this._previousDeadTimeSeconds)
			{
				if (this.ShouldPlayRespawnCountdownAudio && this._remainingDeadTimeSeconds == this._preRespawnCountdownAudioTime)
				{
					this._preRespawnCountdownAudioToken = FMODAudioManager.PlayAt(this._preRespawnCountdownAsset, base.transform);
					FMODAudioManager.PlayOneShotAt(this._respawnCounterAsset, base.transform.position, 0);
					this.ShouldPlayRespawnCountdownAudio = false;
				}
				else if (this._remainingDeadTimeSeconds < this._preRespawnCountdownAudioTime)
				{
					FMODAudioManager.PlayOneShotAt(this._respawnCounterAsset, base.transform.position, 0);
				}
			}
		}

		public void Configure(CombatObject combatObject)
		{
			this.Combat = combatObject;
			if (this.Combat == null)
			{
				return;
			}
			this.Configured = true;
			this.GameState = (GameHubBehaviour.Hub.State.Current as Game);
			if (this.GameState != null)
			{
				this.GameState.OnGameOver += this.OnGameOver;
			}
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery += this.OnBombDelivery;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChanged;
			if (this.Combat.IsBot)
			{
				GameHubBehaviour.Hub.Events.Bots.ListenToObjectUnspawn += this.OnPlayerUnspawned;
				GameHubBehaviour.Hub.Events.Bots.ListenToPreObjectSpawn += this.OnPlayerPreSpawn;
				GameHubBehaviour.Hub.Events.Bots.ListenToObjectRespawning += this.OnPlayerRespawning;
				GameHubBehaviour.Hub.Events.Bots.ListenToObjectSpawn += this.OnPlayerSpawn;
			}
			else
			{
				GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn += this.OnPlayerUnspawned;
				GameHubBehaviour.Hub.Events.Players.ListenToPreObjectSpawn += this.OnPlayerPreSpawn;
				GameHubBehaviour.Hub.Events.Players.ListenToObjectRespawning += this.OnPlayerRespawning;
				GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn += this.OnPlayerSpawn;
			}
			this.BombAlreadyDelivered = (GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreboardState.BombDelivery);
		}

		private void OnGameOver(MatchData.MatchState matchwinner)
		{
			if (this.GameState != null)
			{
				this.GameState.OnGameOver -= this.OnGameOver;
			}
			this.Configured = false;
			this.GameOver = true;
		}

		public void OnPlayerBuildComplete(PlayerBuildComplete evt)
		{
			if (!this.AutoConfigure || this.Combat != null || (this.OnlyOwner && !evt.Object.IsOwner))
			{
				return;
			}
			this.Configure(evt.Object.GetBitComponent<CombatObject>());
		}

		public void SetVisibility(bool isVisible)
		{
			if (isVisible)
			{
				this.NguiWidgetAlpha.Alpha = 1f;
			}
			else
			{
				this.NguiWidgetAlpha.Alpha = 0f;
			}
		}

		private void UpdateTimers()
		{
			this.DeathTimeMillis = (float)this.Combat.SpawnController.GetDeathTimeRemainingMillis();
			this._previousDeadTimeSeconds = this._remainingDeadTimeSeconds;
			this._remainingDeadTimeSeconds = (int)((this.DeathTimeMillis + 1000f) * HudUtils.MillisToSeconds);
			if (this.ShouldSetRespawningTime)
			{
				this.LastRespawningTime = (int)this.RespawningTime;
				this.RespawningTime = (float)this.Combat.SpawnController.GetRespawningRemainingMillis();
			}
		}

		protected virtual void OnPlayerUnspawned(PlayerEvent data)
		{
			if (!this.Configured || this.BombAlreadyDelivered || data.TargetId != this.Combat.Player.PlayerCarId)
			{
				return;
			}
			this.DeathTimeMillis = (float)this.Combat.SpawnController.GetDeathTimeRemainingMillis();
			this.ShouldPlayRespawnCountdownAudio = true;
			this.PlayInAnimationsQueued(this.UnspawnAnimationIndex, null, 1f);
		}

		protected virtual void OnPlayerPreSpawn(PlayerEvent data)
		{
			if (!this.Configured || this.BombAlreadyDelivered || data.TargetId != this.Combat.Player.PlayerCarId)
			{
				return;
			}
			this.PlayOutAnimationsQueued(this.UnspawnAnimationIndex, null, 1f);
			this.PlayInAnimationsQueued(this.PreSpawnAnimationIndex, null, 1f);
			this.StopPreRespawnCountdownAudio();
		}

		private void StopPreRespawnCountdownAudio()
		{
			if (this._preRespawnCountdownAudioToken != null && !this._preRespawnCountdownAudioToken.IsInvalidated())
			{
				this._preRespawnCountdownAudioToken.Stop();
				this._preRespawnCountdownAudioToken = null;
			}
		}

		protected virtual void OnPlayerRespawning(PlayerEvent data)
		{
			if (!this.Configured || this.BombAlreadyDelivered || data.TargetId != this.Combat.Player.PlayerCarId)
			{
				return;
			}
			this.PlayOutAnimationsQueued(this.PreSpawnAnimationIndex, null, 1f);
			this.PlayInAnimationsQueued(this.RespawningAnimationIndex, null, 1f);
		}

		protected virtual void OnPlayerSpawn(PlayerEvent data)
		{
			if (!this.Configured || this.BombAlreadyDelivered || data.TargetId != this.Combat.Player.PlayerCarId || data.EventKind == PlayerEvent.Kind.Create)
			{
				return;
			}
			this.PlayOutAnimationsQueued(this.RespawningAnimationIndex, null, 1f);
			this.PlayInAnimationsQueued(this.SpawnAnimationIndex, null, 1f);
			this.RespawningTime = -1f;
		}

		protected void PlayInAnimationsQueued(int indexKind, Action action = null, float speed = 1f)
		{
			this.AnimationQueue.StoploopinAnimation();
			if (indexKind == -1)
			{
				return;
			}
			this.PlayQueuedAnimations(this.Animations[indexKind].AnimationsClipNameIn, this.Animations[indexKind].Animation, action, 1f);
		}

		protected void PlayOutAnimationsQueued(int indexKind, Action action = null, float speed = 1f)
		{
			this.AnimationQueue.StoploopinAnimation();
			if (indexKind == -1)
			{
				return;
			}
			this.PlayQueuedAnimations(this.Animations[indexKind].AnimationsClipNameOut, this.Animations[indexKind].Animation, action, speed);
		}

		protected bool PlayQueuedAnimations(string[] animationsClipName, Animation anim, Action action, float speed = 1f)
		{
			if (animationsClipName.Length <= 0)
			{
				return false;
			}
			for (int i = 0; i < animationsClipName.Length; i++)
			{
				this.AnimationQueue.Queue(anim, animationsClipName[i], action, speed);
			}
			return true;
		}

		protected virtual void OnBombDelivery(int causerid, TeamKind scoredteam, Vector3 deliveryPosition)
		{
			this.RespawningTime = -1f;
			this.BombAlreadyDelivered = true;
			this.AnimationQueue.Clear();
			if (this.ResetHudStatusAnimation != null)
			{
				this.ResetHudStatusAnimation.Play();
			}
			this.StopPreRespawnCountdownAudio();
		}

		private void OnPhaseChanged(BombScoreboardState state)
		{
			if (state == BombScoreboardState.BombDelivery)
			{
				this.BombAlreadyDelivered = false;
			}
		}

		public bool IsStackableWithType(Type type)
		{
			return false;
		}

		private static readonly string[] CachedCountDown = new string[]
		{
			"0",
			"1",
			"2",
			"3",
			"4",
			"5",
			"6",
			"7",
			"8",
			"9",
			"10",
			"11",
			"12",
			"13",
			"14",
			"15",
			"16",
			"17",
			"18",
			"19",
			"20",
			"21",
			"22",
			"23",
			"24",
			"25",
			"26",
			"27",
			"28",
			"29",
			"30",
			"31",
			"32",
			"33",
			"34",
			"35",
			"36",
			"37",
			"38",
			"39",
			"40",
			"41",
			"42",
			"43",
			"44",
			"45",
			"46",
			"47",
			"48",
			"49",
			"50",
			"51",
			"52",
			"53",
			"54",
			"55",
			"56",
			"57",
			"58",
			"59"
		};

		public bool AutoConfigure;

		public bool OnlyOwner;

		public NGUIWidgetAlpha NguiWidgetAlpha;

		public UILabel DeathTimerNumberCountdown;

		public UILabel RespawningNumberCountdown;

		public Animation RespawningNumberAnimation;

		public string RespawningNumberAnimationClipName;

		[Header("Audio")]
		[SerializeField]
		private AudioEventAsset _preRespawnCountdownAsset;

		[SerializeField]
		private AudioEventAsset _respawnCounterAsset;

		[SerializeField]
		private int _preRespawnCountdownAudioTime = 3;

		private FMODAudioManager.FMODAudio _preRespawnCountdownAudioToken;

		protected bool ShouldPlayRespawnCountdownAudio;

		private int _previousDeadTimeSeconds = -1;

		private int _remainingDeadTimeSeconds = -1;

		private int _previousRespawningTimeSeconds = -1;

		private int _remainingRespawningTimeSeconds = -1;

		public HudRespawnController.AnimationEvent[] Animations;

		public Animation ResetHudStatusAnimation;

		protected AnimationQueue AnimationQueue;

		protected int UnspawnAnimationIndex = -1;

		protected int PreSpawnAnimationIndex = -1;

		protected int RespawningAnimationIndex = -1;

		protected int SpawnAnimationIndex = -1;

		protected float DeathTimeMillis;

		protected bool ShouldSetRespawningTime;

		protected float RespawningTime = -1f;

		protected int LastRespawningTime;

		protected bool Configured;

		protected CombatObject Combat;

		protected bool BombAlreadyDelivered;

		protected Game GameState;

		protected bool GameOver;

		[Serializable]
		public struct AnimationEvent
		{
			public PlayerEvent.Kind EvenKind;

			public Animation Animation;

			public string[] AnimationsClipNameIn;

			public string[] AnimationsClipNameOut;
		}
	}
}
