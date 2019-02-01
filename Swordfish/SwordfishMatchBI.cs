using System;
using System.Globalization;
using System.Text;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Swordfish
{
	public class SwordfishMatchBI : GameHubObject
	{
		public SwordfishMatchBI()
		{
			this._disabled = GameHubObject.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false);
		}

		private string ServerHeartbeatLog
		{
			get
			{
				this._biStringBuilder.Length = 0;
				this._biStringBuilder.AppendFormat("ActivityId={0} NumberOfConnectedPlayers={1} ", GameHubObject.Hub.Swordfish.Connection.ServerMatchId, this._currentConnectedPlayers);
				this._biStringBuilder.AppendFormat("FpsAvg={0} FpsMin={1}", this._avgFpsCount, this._minFpsCount);
				return this._biStringBuilder.ToString();
			}
		}

		private void ServerUpdateMatchData(MatchController.GameOverMessage msg)
		{
			this._gameTimeMinutes = (int)((float)GameHubObject.Hub.GameTime.GetPlaybackTime() * 0.001f / 60f);
			this._mapId = ((!(GameHubObject.Hub.Server.Level != null)) ? -1 : GameHubObject.Hub.Server.Level.Id);
			this._matchmakingType = ((GameHubObject.Hub.Players.Bots.Count <= 0) ? 1 : 0);
			bool flag = this._gameTimeMinutes <= 15;
			MatchData.MatchState state = msg.State;
			if (state != MatchData.MatchState.MatchOverRedWins)
			{
				if (state != MatchData.MatchState.MatchOverBluWins)
				{
					if (state == MatchData.MatchState.MatchOverTie)
					{
						this._gameResult = ((!flag) ? -1 : -2);
					}
				}
				else
				{
					this._gameResult = ((!flag) ? 2 : 3);
				}
			}
			else
			{
				this._gameResult = ((!flag) ? 0 : 1);
			}
		}

		private void ServerUpdateHeartbeatData()
		{
			this._currentConnectedPlayers = 0;
			for (int i = 0; i < GameHubObject.Hub.Players.Players.Count; i++)
			{
				PlayerData playerData = GameHubObject.Hub.Players.Players[i];
				if (playerData.CharacterInstance)
				{
					if (!GameHubObject.Hub.ScrapBank.PlayerAccounts[playerData.CharacterInstance.ObjId].Disconnected)
					{
						this._currentConnectedPlayers++;
					}
				}
			}
		}

		public void ServerOnMatchStarted()
		{
			if (this._disabled)
			{
				return;
			}
			if (GameHubObject.Hub.Net.IsClient())
			{
				return;
			}
			bool flag = true;
			int num = 0;
			while (flag && num < GameHubObject.Hub.Players.Players.Count)
			{
				PlayerData playerData = GameHubObject.Hub.Players.Players[num++];
				if (playerData.CharacterInstance)
				{
					PlayerStats playerStats = GameHubObject.Hub.ScrapBank.PlayerAccounts[playerData.CharacterInstance.ObjId];
					flag = !playerStats.Disconnected;
				}
			}
			this._allPlayersIn = flag;
		}

		public void GameOver(MatchController.GameOverMessage msg)
		{
			if (this._disabled)
			{
				return;
			}
			if (GameHubObject.Hub.Net.IsClient())
			{
				return;
			}
			if (GameHubObject.Hub.Match.LevelIsTutorial())
			{
				return;
			}
			if (msg.State == MatchData.MatchState.Nothing)
			{
				return;
			}
			this.ServerUpdateMatchData(msg);
			SwordfishMatchBI.Log.InfoFormat("GameOver MatchTime={0}", new object[]
			{
				(float)GameHubObject.Hub.GameTime.GetPlaybackTime() / 1000f
			});
		}

		public void ClientOnMatchLoaded()
		{
			if (!GameHubObject.Hub.Net.IsClient())
			{
				return;
			}
			this._clientPerfStatsIndex = 0;
			this.ResetFrameTimeCount();
			this.ResetPingCount();
			this._clientPerfStatsAccum = 0f;
			this._clientPingUpdater.Reset();
			this._clientPingUpdater.ShouldHalt();
			if (!this._bombDeliveryListenerReady)
			{
				GameHubObject.Hub.BombManager.ListenToBombDelivery += this.ListenToBombDelivery;
				this._bombDeliveryListenerReady = true;
			}
			this._clientStatsReady = true;
			this._clientLogRunning = true;
		}

		private void ListenToBombDelivery(int causerid, TeamKind scoredteam, Vector3 deliveryPosition)
		{
			this._bombDeliveryTeamKind = scoredteam;
		}

		public void ClientOnMatchEnded()
		{
			GameHubObject.Hub.BombManager.ListenToBombDelivery -= this.ListenToBombDelivery;
			this._bombDeliveryListenerReady = false;
			this._clientStatsReady = false;
		}

		private void SendPerformanceLogs()
		{
			this._biStringBuilder.Length = 0;
			this._biStringBuilder.AppendFormat("PlayerId={0} LogIndex={1} BombDeliveryTeam={2} ", GameHubObject.Hub.User.UniversalId, this._clientPerfStatsIndex, this._bombDeliveryTeamKind);
			this._biStringBuilder.AppendFormat("CurrentGameState={0} CurrentRound={1} IsTutorial={2} ", GameHubObject.Hub.BombManager.CurrentBombGameState, GameHubObject.Hub.BombManager.Round, GameHubObject.Hub.Match.LevelIsTutorial());
			this._biStringBuilder.AppendFormat("IsSpectator={0} ", SpectatorController.IsSpectating);
			int length = this._biStringBuilder.Length;
			this._biStringBuilder.Append("Fps=[");
			for (int i = 0; i < this._clientFrameTimeCount.Length; i++)
			{
				this._biStringBuilder.Append(this._clientFrameTimeCount[i].ToString(CultureInfo.InvariantCulture)).Append(',');
			}
			if (this._biStringBuilder[this._biStringBuilder.Length - 1] == ',')
			{
				this._biStringBuilder[this._biStringBuilder.Length - 1] = ']';
			}
			GameHubObject.Hub.Swordfish.Log.BILogClientMatchMsg(ClientBITags.ClientFrameTimeV3, this._biStringBuilder.ToString(), false);
			this._biStringBuilder.Length = length;
			this._biStringBuilder.AppendFormat("PingMax={0} Ping=[", this._clientMaxPing);
			for (int j = 0; j < this._clientPingCount.Length; j++)
			{
				this._biStringBuilder.Append(this._clientPingCount[j]).Append(',');
			}
			if (this._biStringBuilder[this._biStringBuilder.Length - 1] == ',')
			{
				this._biStringBuilder[this._biStringBuilder.Length - 1] = ']';
			}
			GameHubObject.Hub.Swordfish.Log.BILogClientMatchMsg(ClientBITags.ClientPingV2, this._biStringBuilder.ToString(), false);
		}

		public void Update()
		{
			if (this._disabled)
			{
				return;
			}
			if (GameHubObject.Hub.Net.IsClient())
			{
				this.ClientUpdate();
				return;
			}
			if (GameHubObject.Hub.Net.IsServer())
			{
				this.ServerUpdate();
				return;
			}
		}

		private void ClientUpdate()
		{
			if (!this._clientStatsReady)
			{
				return;
			}
			if (SwordfishMatchBIStates.ShouldRecordPerformanceStats())
			{
				this.ClientFrameTimeUpdate();
				this.ClientPingUpdate();
			}
			if (!this._clientLogRunning)
			{
				return;
			}
			if (this._clientPerfStatsAccum < 60f)
			{
				return;
			}
			this.SendPerformanceLogs();
			if (!GameHubObject.Hub.Match.State.IsGame())
			{
				this._clientLogRunning = false;
			}
			this.ResetFrameTimeCount();
			this.ResetPingCount();
			this._bombDeliveryTeamKind = TeamKind.Zero;
			this._clientPerfStatsIndex++;
		}

		private void ServerUpdate()
		{
			if (GameHubObject.Hub.Match.State != MatchData.MatchState.MatchStarted)
			{
				return;
			}
			this.FpsUpdate();
			if (this._serverHeartbeatTimedUpdater.ShouldHalt())
			{
				return;
			}
			this._avgFpsCount = (float)this._currentFpsCount / this._fpsCountTime;
			this.ServerUpdateHeartbeatData();
			GameHubObject.Hub.Swordfish.Log.BILogServerMsg(ServerBITags.ServerHeartbeat, this.ServerHeartbeatLog, false);
			this.ResetFpsCount();
		}

		public void ClientAnnouncerConfigured(int isChange)
		{
			string msg = string.Format("UserID={0} AnnouncerIndex={1} isChange={2}", GameHubObject.Hub.User.UniversalId, GameHubObject.Hub.Options.Audio.AnnouncerIndex, isChange);
			GameHubObject.Hub.Swordfish.Log.BILogClientMsg(ClientBITags.AnnouncerConfigured, msg, false);
		}

		private void ClientFrameTimeUpdate()
		{
			float unscaledDeltaTime = Time.unscaledDeltaTime;
			int num = Mathf.FloorToInt(1f / unscaledDeltaTime);
			if (num > 59)
			{
				num = 59;
			}
			else if (num < 0)
			{
				num = 0;
			}
			this._clientFrameTimeCount[num] += unscaledDeltaTime;
			this._clientPerfStatsAccum += Time.unscaledDeltaTime;
		}

		private void ResetFrameTimeCount()
		{
			Array.Clear(this._clientFrameTimeCount, 0, this._clientFrameTimeCount.Length);
			this._clientPerfStatsAccum = 0f;
		}

		private void ClientPingUpdate()
		{
			if (this._clientPingUpdater.ShouldHalt())
			{
				return;
			}
			float num = GameHubObject.Hub.Net.GetPing();
			if (num > (float)this._clientMaxPing)
			{
				this._clientMaxPing = (int)num;
			}
			num = Mathf.Clamp(num, 0f, 500f);
			int num2 = (int)(num / 50f);
			if (num2 >= this._clientPingCount.Length)
			{
				num2 = this._clientPingCount.Length - 1;
			}
			this._clientPingCount[num2] += 1u;
		}

		private void FpsUpdate()
		{
			this._lastFpsCount++;
			this._currentFpsCount++;
			this._lastFpsCountTime += Time.unscaledDeltaTime;
			this._fpsCountTime += Time.unscaledDeltaTime;
			if (this._lastFpsCountTime > 1f)
			{
				float num = (float)this._lastFpsCount / this._lastFpsCountTime;
				if (num < this._minFpsCount)
				{
					this._minFpsCount = num;
				}
				this._lastFpsCount = 0;
				this._lastFpsCountTime = 0f;
			}
		}

		private void ResetPingCount()
		{
			Array.Clear(this._clientPingCount, 0, this._clientPingCount.Length);
			this._clientMaxPing = 0;
		}

		private void ResetFpsCount()
		{
			this._lastFpsCount = 0;
			this._currentFpsCount = 0;
			this._minFpsCount = 2.14748365E+09f;
			this._avgFpsCount = 0f;
			this._lastFpsCountTime = 0f;
			this._fpsCountTime = 0f;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(SwordfishMatchBI));

		private bool _disabled;

		private Guid _activityId;

		private int _matchmakingType;

		private bool _allPlayersIn;

		private int _gameTimeMinutes;

		private int _gameResult;

		private int _mapId;

		private int _team1Tower1DownSeconds;

		private int _team1Tower2DownSeconds;

		private int _team1Tower3DownSeconds;

		private int _team1Tower4DownSeconds;

		private int _team1Tower5DownSeconds;

		private int _team1Tower6DownSeconds;

		private int _team1Tower7DownSeconds;

		private int _team1Tower8DownSeconds;

		private int _team1Tower9DownSeconds;

		private int _team2Tower1DownSeconds;

		private int _team2Tower2DownSeconds;

		private int _team2Tower3DownSeconds;

		private int _team2Tower4DownSeconds;

		private int _team2Tower5DownSeconds;

		private int _team2Tower6DownSeconds;

		private int _team2Tower7DownSeconds;

		private int _team2Tower8DownSeconds;

		private int _team2Tower9DownSeconds;

		private int _currentConnectedPlayers;

		private TimedUpdater _serverHeartbeatTimedUpdater = new TimedUpdater(15000, true, true);

		private TimedUpdater _clientPingUpdater = new TimedUpdater(500, true, true);

		private TeamKind _bombDeliveryTeamKind;

		private readonly StringBuilder _biStringBuilder = new StringBuilder();

		private int _lastFpsCount;

		private int _currentFpsCount;

		private float _lastFpsCountTime;

		private float _fpsCountTime;

		private float _minFpsCount;

		private float _avgFpsCount;

		private bool _clientStatsReady;

		private bool _bombDeliveryListenerReady;

		private readonly float[] _clientFrameTimeCount = new float[60];

		private float _clientPerfStatsAccum;

		private readonly uint[] _clientPingCount = new uint[10];

		private int _clientMaxPing;

		private int _clientPerfStatsIndex;

		private const int PING_INTERVAL = 50;

		private const int MAX_PING = 500;

		private const int PING_BIN_COUNT = 10;

		private const int FPS_BIN_COUNT = 60;

		private const float FPS_SAMPLE_SECONDS = 60f;

		private bool _clientLogRunning;
	}
}
