using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.UpdateStream;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class BombGridController : GameHubObject
	{
		public BombGridController(BombManager bombManager)
		{
			this._bombManager = bombManager;
			if (GameHubObject.Hub.Net.IsServer())
			{
				this._bombManager.ListenToPhaseChange += this.OnPhaseChanged;
			}
			this._gridGameRunning = false;
		}

		private BombRulesInfo Rules
		{
			get
			{
				return this._bombManager.Rules;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnGridGamePlayersCreated;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<byte, float> ListenToGridGameFinished;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action ListenToGridGameStarted;

		~BombGridController()
		{
			if (this._bombManager != null)
			{
				this._bombManager.ListenToPhaseChange -= this.OnPhaseChanged;
			}
			this.ListenToGridGameStarted = null;
			this.OnGridGamePlayersCreated = null;
			this.ListenToGridGameFinished = null;
		}

		public void OnPlayersAndBotsSpawnFinish()
		{
			this._gridGameRunning = false;
			this.ClearAll();
			List<PlayerData> list;
			if (GameHubObject.Hub.Match.LevelIsTutorial())
			{
				list = GameHubObject.Hub.Players.Players;
			}
			else
			{
				list = GameHubObject.Hub.Players.PlayersAndBots;
			}
			for (int i = 0; i < list.Count; i++)
			{
				BombGridController.GridGamePlayer gridGamePlayer = new BombGridController.GridGamePlayer();
				gridGamePlayer.Data = list[i];
				gridGamePlayer.Value = 0f;
				this._players.Add(gridGamePlayer);
				GameHubObject.Hub.Stream.StateStream.AddObject(gridGamePlayer);
				if (GameHubObject.Hub.Net.IsClient() && list[i] == GameHubObject.Hub.Players.CurrentPlayerData)
				{
					this.CurrentPlayer = gridGamePlayer;
				}
			}
			if (this.OnGridGamePlayersCreated != null)
			{
				this.OnGridGamePlayersCreated();
			}
		}

		private void ClearAll()
		{
			this.CurrentPlayer = null;
			for (int i = 0; i < this._players.Count; i++)
			{
				BombGridController.GridGamePlayer gridGamePlayer = this._players[i];
				GameHubObject.Hub.Stream.StateStream.Remove(gridGamePlayer);
				gridGamePlayer.Data = null;
			}
			this._players.Clear();
		}

		public void RunUpdate(int matchTime)
		{
			if (!this._gridGameRunning)
			{
				return;
			}
			for (int i = 0; i < this._players.Count; i++)
			{
				this._players[i].UpdatePlayer(matchTime, this.Rules);
			}
		}

		public void OnPlayerUpdatedGridProgress(byte playerAddress, int clientProgress)
		{
			if (!this._gridGameRunning)
			{
				return;
			}
			for (int i = 0; i < this._players.Count; i++)
			{
				if (this._players[i].Data.PlayerAddress == playerAddress)
				{
					this._players[i].CalculateDiffProgress(clientProgress);
					break;
				}
			}
		}

		private void OnPhaseChanged(BombScoreBoard.State phase)
		{
			if (phase != BombScoreBoard.State.PreBomb)
			{
				if (phase == BombScoreBoard.State.BombDelivery)
				{
					this.StopGridGame();
				}
			}
			else
			{
				this.StartGridGame();
			}
		}

		private void StartGridGame()
		{
			this._gridGameRunning = true;
			for (int i = 0; i < this._players.Count; i++)
			{
				this._players[i].Reset();
			}
			this.OnGridGameStarted();
			this._bombManager.DispatchReliable(GameHubObject.Hub.SendAll).OnGridGameStarted();
		}

		public void OnGridGameStarted()
		{
			if (this.ListenToGridGameStarted != null)
			{
				this.ListenToGridGameStarted();
			}
		}

		private void StopGridGame()
		{
			this._gridGameRunning = false;
			for (int i = 0; i < this._players.Count; i++)
			{
				BombGridController.GridGamePlayer gridGamePlayer = this._players[i];
				float num = gridGamePlayer.ValueFloat;
				float gridYellowZone = gridGamePlayer.Data.Character.Car.GridYellowZone;
				float gridGreenZone = gridGamePlayer.Data.Character.Car.GridGreenZone;
				int averageDiff = gridGamePlayer.AverageDiff;
				if (averageDiff < this.Rules.GridMaxDiffClientServer)
				{
					num = gridGamePlayer.LastClientProgress * 0.01f;
					gridGamePlayer.Value = gridGamePlayer.LastClientProgress;
				}
				CombatObject combat = gridGamePlayer.Combat;
				if (!combat.IsBot)
				{
					num = Mathf.Clamp01(num);
					byte playerAddress = gridGamePlayer.Data.PlayerAddress;
					this.OnGridGameFinished(playerAddress, num);
					this._bombManager.DispatchReliable(new byte[]
					{
						playerAddress
					}).OnGridGameFinished(playerAddress, num);
				}
			}
		}

		public void OnGridGameFinished(byte playerAdress, float finalValue)
		{
			if (this.ListenToGridGameFinished != null)
			{
				this.ListenToGridGameFinished(playerAdress, finalValue);
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(BombGridController));

		private readonly BombManager _bombManager;

		private bool _gridGameRunning;

		private readonly List<BombGridController.GridGamePlayer> _players = new List<BombGridController.GridGamePlayer>();

		public BombGridController.GridGamePlayer CurrentPlayer;

		public class GridGamePlayer : IStateContent
		{
			~GridGamePlayer()
			{
				this.OnValueChanged = null;
			}

			public float LastClientProgress
			{
				get
				{
					return this._lastClientProgressReceived;
				}
			}

			public int AverageDiff
			{
				get
				{
					if (this._progressReceivedCount <= 0 || this._totalDiff <= 0f)
					{
						return 0;
					}
					return Mathf.CeilToInt(this._totalDiff / (float)this._progressReceivedCount);
				}
			}

			public float ValueFloat
			{
				get
				{
					return this._value / 100f;
				}
			}

			public float Value
			{
				get
				{
					return this._value;
				}
				set
				{
					this._value = value;
					if (GameHubObject.Hub.Net.IsClient() && this.OnValueChanged != null)
					{
						this.OnValueChanged(this._value);
					}
				}
			}

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public event Action<float> OnValueChanged;

			public CombatObject Combat
			{
				get
				{
					if (this._combat == null)
					{
						this._combat = this.Data.CharacterInstance.GetBitComponent<CombatObject>();
					}
					return this._combat;
				}
			}

			public void Reset()
			{
				this.Value = 0f;
				this._lastTime = GameHubObject.Hub.GameTime.GetPlaybackTime();
				this._minDiffProgressReceivedFromClient = 0f;
				this._minDiffProgressReceivedFromClient = 0f;
				this._totalDiff = 0f;
				this._progressReceivedCount = 0;
			}

			public void UpdatePlayer(int matchTime, BombRulesInfo rules)
			{
				IPlayerController playerController = this.Combat.PlayerController;
				AnimationCurve animationCurve = (!playerController.AcceleratingForward) ? rules.GridCursorCurveDown : rules.GridCursorCurveUp;
				float num = (!playerController.AcceleratingForward) ? (-rules.GridCursorSpeedDown) : rules.GridCursorSpeedUp;
				float num2 = (float)(matchTime - this._lastTime) / 1000f;
				this._lastTime = matchTime;
				float num3 = this.Value + num2 * animationCurve.Evaluate(this._value / 100f) * num;
				if (num3 < 0f)
				{
					this.Value = 0f;
				}
				else if (num3 > 100f)
				{
					this.Value = 100f;
				}
				else
				{
					this.Value = num3;
				}
				GameHubObject.Hub.Stream.StateStream.Changed(this);
			}

			public void CalculateDiffProgress(int clientProgress)
			{
				int num = (int)Mathf.Abs((float)clientProgress - this.Value);
				this._lastClientProgressReceived = (float)clientProgress;
				this._totalDiff += (float)num;
				this._progressReceivedCount++;
				if ((float)num < this._minDiffProgressReceivedFromClient || this._minDiffProgressReceivedFromClient == 0f)
				{
					this._minDiffProgressReceivedFromClient = (float)num;
				}
				else if ((float)num > this._maxDiffProgressReceivedFromClient || this._maxDiffProgressReceivedFromClient == 0f)
				{
					this._maxDiffProgressReceivedFromClient = (float)num;
				}
			}

			public int ObjId
			{
				get
				{
					return this.Data.PlayerCarId;
				}
			}

			public byte ClassId
			{
				get
				{
					return 2;
				}
			}

			public short Version
			{
				get
				{
					return this._version;
				}
				set
				{
					this._version = value;
				}
			}

			public bool IsCached()
			{
				return this.Data.CharacterInstance == null || this.Data.CharacterInstance.IsCached;
			}

			public byte[] GetStreamData()
			{
				return new byte[]
				{
					(byte)this.Value
				};
			}

			public void ApplyStreamData(byte[] data)
			{
				this.Value = (float)data[0];
			}

			private int _lastTime;

			private float _minDiffProgressReceivedFromClient;

			private float _maxDiffProgressReceivedFromClient;

			private float _lastClientProgressReceived;

			private float _totalDiff;

			private int _progressReceivedCount;

			private float _value;

			public PlayerData Data;

			private CombatObject _combat;

			private short _version = short.MinValue;
		}
	}
}
