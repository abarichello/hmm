using System;
using System.Collections.Generic;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Combat.GadgetScript;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.Playback;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class MatchPlayers : KeyStateParser, IMatchPlayers, IMatchPlayersDispatcher
	{
		public MatchPlayers()
		{
			this.Players = new List<PlayerData>(8);
			this.Bots = new List<PlayerData>(8);
			this.PlayersAndBots = new List<PlayerData>(8);
			this.Narrators = new List<PlayerData>(4);
		}

		public override StateType Type
		{
			get
			{
				return StateType.Players;
			}
		}

		public override void Update(BitStream stream)
		{
			this._serial = stream.ReadCompressedInt();
			int num = 0;
			if (GameHubObject.Hub.Match.LevelIsTutorial())
			{
				num = this.Bots.Count;
			}
			MatchPlayers.Log.DebugFormat("Received updated Players={0}", new object[]
			{
				this._serial
			});
			while (stream.ReadBool())
			{
				byte key = stream.ReadByte();
				PlayerData playerData;
				if (this._allByAddress.TryGetValue(key, out playerData))
				{
					playerData.ReadFromBitStream(stream);
				}
				else
				{
					playerData = new PlayerData(stream);
					if (playerData.IsNarrator)
					{
						this.AddNarrator(playerData);
					}
					else if (playerData.IsBot)
					{
						this.AddBot(playerData);
					}
					else
					{
						this.AddPlayer(playerData);
					}
				}
			}
			if (GameHubObject.Hub.Match.LevelIsTutorial() && num == 0 && this.Bots.Count > 0)
			{
				GameHubObject.Hub.Events.Bots.SpawnAllObjects();
			}
		}

		public void UpdatePlayers()
		{
			this.SendPlayers(1);
		}

		public void UpdatePlayer(int objId)
		{
			PlayerData playerOrBotsByObjectId = this.GetPlayerOrBotsByObjectId(objId);
			if (playerOrBotsByObjectId == null)
			{
				MatchPlayers.Log.FatalFormat("Trying to update unknown player={0}", new object[]
				{
					objId
				});
				return;
			}
			this._serial++;
			MatchPlayers.Log.DebugFormat("Sending single player={1} serial={0}", new object[]
			{
				this._serial,
				playerOrBotsByObjectId.PlayerAddress
			});
			BitStream stream = base.GetStream();
			stream.WriteCompressedInt(this._serial);
			stream.WriteBool(true);
			stream.WriteByte(playerOrBotsByObjectId.PlayerAddress);
			playerOrBotsByObjectId.WriteToBitStream(stream);
			stream.WriteBool(false);
			this._serverDispatcher.SendSnapshot(1, this.Type.Convert(), this._serverDispatcher.GetNextFrameId(), -1, this._gameTime.GetPlaybackTime(), stream.ToArray());
		}

		public void SendPlayers(byte to)
		{
			this._serial++;
			MatchPlayers.Log.DebugFormat("Sending players={0} to={1}", new object[]
			{
				this._serial,
				to
			});
			BitStream stream = base.GetStream();
			stream.WriteCompressedInt(this._serial);
			for (int i = 0; i < this.AllDatas.Count; i++)
			{
				PlayerData playerData = this.AllDatas[i];
				stream.WriteBool(true);
				stream.WriteByte(playerData.PlayerAddress);
				playerData.WriteToBitStream(stream);
			}
			stream.WriteBool(false);
			this._serverDispatcher.SendSnapshot(1, this.Type.Convert(), this._serverDispatcher.GetNextFrameId(), -1, this._gameTime.GetPlaybackTime(), stream.ToArray());
		}

		public PlayerData CurrentPlayerData
		{
			get
			{
				if (this._currentPlayerData != null)
				{
					return this._currentPlayerData;
				}
				HMMHub hub = GameHubBehaviour.Hub;
				if (hub.Net.IsTest())
				{
					this._currentPlayerData = this.AllDatas.Find((PlayerData p) => p.Name == "PlayerBot");
					return this._currentPlayerData;
				}
				if (hub.Net.IsServer())
				{
					MatchPlayers.Log.FatalFormatStackTrace("Trying to access currentPlayerData on server side!", new object[0]);
					this._currentPlayerData = new PlayerData("-1", -1, TeamKind.Zero, byte.MaxValue, -1, true, 0, false, new BattlepassProgress());
					return this._currentPlayerData;
				}
				byte myAddress = hub.Net.GetMyAddress();
				this._allByAddress.TryGetValue(myAddress, out this._currentPlayerData);
				return this._currentPlayerData;
			}
		}

		public TeamKind CurrentPlayerTeam
		{
			get
			{
				if (SpectatorController.IsSpectating)
				{
					return SingletonMonoBehaviour<SpectatorController>.Instance.CurrentNarratorTeamKindPointOfView;
				}
				if (this.CurrentPlayerData == null)
				{
					return TeamKind.Zero;
				}
				return this.CurrentPlayerData.Team;
			}
		}

		public TeamKind GetTeamKindById(int playerId)
		{
			PlayerData playerData = null;
			this._playersAndBotsId.TryGetValue(playerId, out playerData);
			return playerData.Team;
		}

		public int RedMMR
		{
			get
			{
				this.CalculateAverageMMR();
				return this._redMMR;
			}
		}

		public int BlueMMR
		{
			get
			{
				this.CalculateAverageMMR();
				return this._blueMMR;
			}
		}

		private void CalculateAverageMMR()
		{
			if (this._mmrSet)
			{
				return;
			}
			this._mmrSet = true;
			if (GameHubObject.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				this._redMMR = GameHubObject.Hub.Config.GetIntValue(ConfigAccess.RedMMR);
				this._blueMMR = GameHubObject.Hub.Config.GetIntValue(ConfigAccess.BlueMMR);
				MatchPlayers.Log.DebugFormat("SkipSwordfish Average MMR Blue={0} Red={1}", new object[]
				{
					this._blueMMR,
					this._redMMR
				});
				return;
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < this.Players.Count; i++)
			{
				PlayerData playerData = this.Players[i];
				TeamKind team = playerData.Team;
				if (team != TeamKind.Red)
				{
					if (team == TeamKind.Blue)
					{
						num2++;
						num4 += playerData.MMR;
					}
				}
				else
				{
					num++;
					num3 += playerData.MMR;
				}
			}
			if (num > 0)
			{
				this._redMMR = num3 / num;
			}
			if (num2 > 0)
			{
				this._blueMMR = num4 / num2;
			}
			if (num == 0)
			{
				this._redMMR = int.MaxValue;
			}
			if (num2 == 0)
			{
				this._blueMMR = int.MaxValue;
			}
			MatchPlayers.Log.DebugFormat("Average MMR Blue={0} Red={1}", new object[]
			{
				this._blueMMR,
				this._redMMR
			});
		}

		public List<PlayerData> Players { get; private set; }

		public List<PlayerData> Bots { get; private set; }

		public List<PlayerData> PlayersAndBots { get; private set; }

		public List<PlayerData> Narrators { get; private set; }

		public void AddPlayer(PlayerData data)
		{
			MatchPlayers.Log.DebugFormat("Adding player={0}", new object[]
			{
				data.PlayerAddress
			});
			this.Players.Add(data);
			this.Clients.Add(data);
			this.AddToStructures(data);
		}

		public void AddBot(PlayerData data)
		{
			MatchPlayers.Log.DebugFormat("Adding bot={0}", new object[]
			{
				data.PlayerAddress
			});
			this.Bots.Add(data);
			this.AddToStructures(data);
		}

		private void AddToStructures(PlayerData data)
		{
			this.AllDatas.Add(data);
			this.PlayersAndBots.Add(data);
			this._allByAddress[data.PlayerAddress] = data;
			this._playersAndBotsId[data.PlayerCarId] = data;
			TeamKind team = data.Team;
			if (team != TeamKind.Red)
			{
				if (team == TeamKind.Blue)
				{
					this.BlueTeamPlayersAndBots.Add(data);
				}
			}
			else
			{
				this.RedTeamPlayersAndBots.Add(data);
			}
		}

		public void AddNarrator(PlayerData data)
		{
			MatchPlayers.Log.DebugFormat("Adding narrator={0}", new object[]
			{
				data.PlayerAddress
			});
			this.Narrators.Add(data);
			this.AllDatas.Add(data);
			this.Clients.Add(data);
			this._allByAddress[data.PlayerAddress] = data;
		}

		public void RemoveNarrator(byte address)
		{
			PlayerData playerData;
			if (!this._allByAddress.TryGetValue(address, out playerData) || !playerData.IsNarrator)
			{
				return;
			}
			this.Narrators.Remove(playerData);
			this.AllDatas.Remove(playerData);
			this.Clients.Remove(playerData);
			this._allByAddress.Remove(address);
		}

		public PlayerData GetPlayer(TeamKind team, int slot)
		{
			for (int i = 0; i < this.Players.Count; i++)
			{
				PlayerData playerData = this.Players[i];
				if (!(playerData == null))
				{
					if (playerData.Team == team && playerData.TeamSlot == slot)
					{
						return playerData;
					}
				}
			}
			return null;
		}

		public PlayerData GetPlayerOrBot(MatchClient matchClient)
		{
			for (int i = 0; i < this.Players.Count; i++)
			{
				PlayerData playerData = this.Players[i];
				if (!(playerData == null))
				{
					if (playerData.IsBot && playerData.BotId == matchClient.BotId)
					{
						return playerData;
					}
					if (playerData.PlayerId == matchClient.PlayerId)
					{
						return playerData;
					}
				}
			}
			return null;
		}

		public PlayerData GetPlayerOrBot(TeamKind team, int slot)
		{
			for (int i = 0; i < this.PlayersAndBots.Count; i++)
			{
				PlayerData playerData = this.PlayersAndBots[i];
				if (!(playerData == null))
				{
					if (playerData.Team == team && playerData.TeamSlot == slot)
					{
						return playerData;
					}
				}
			}
			return null;
		}

		public PlayerData GetAnyByPlayerId(long id)
		{
			for (int i = 0; i < this.AllDatas.Count; i++)
			{
				PlayerData playerData = this.AllDatas[i];
				if (playerData.PlayerId == id)
				{
					return playerData;
				}
			}
			return null;
		}

		public PlayerData GetPlayerOrBotsByObjectId(int id)
		{
			PlayerData result = null;
			this._playersAndBotsId.TryGetValue(id, out result);
			return result;
		}

		public PlayerData GetPlayerByObjectId(int id)
		{
			PlayerData playerOrBotsByObjectId = this.GetPlayerOrBotsByObjectId(id);
			return (!(playerOrBotsByObjectId == null)) ? ((!playerOrBotsByObjectId.IsBot) ? playerOrBotsByObjectId : null) : null;
		}

		public PlayerData GetAnyByAddress(byte address)
		{
			PlayerData result = null;
			this._allByAddress.TryGetValue(address, out result);
			return result;
		}

		public PlayerData GetPlayerByAddress(byte address)
		{
			PlayerData anyByAddress = this.GetAnyByAddress(address);
			return (!(anyByAddress == null)) ? ((!anyByAddress.IsBot) ? anyByAddress : null) : null;
		}

		public PlayerData GetBotByAddress(byte address)
		{
			PlayerData anyByAddress = this.GetAnyByAddress(address);
			return (!(anyByAddress == null)) ? ((!anyByAddress.IsBot) ? null : anyByAddress) : null;
		}

		public PlayerData GetAnyRandomlyByTeam(TeamKind team)
		{
			List<PlayerData> list = (team != TeamKind.Blue) ? this.RedTeamPlayersAndBots : this.BlueTeamPlayersAndBots;
			int objId = list[Random.Range(0, list.Count)].CharacterInstance.ObjId;
			return this.GetPlayerOrBotsByObjectId(objId);
		}

		public bool IsTeamBotOnly(List<PlayerData> playerDatas)
		{
			for (int i = 0; i < playerDatas.Count; i++)
			{
				if (!playerDatas[i].IsBot)
				{
					return false;
				}
			}
			return true;
		}

		public bool IsSomeoneDisconnected(TeamKind teamKind = TeamKind.Zero)
		{
			bool flag = false;
			for (int i = 0; i < this.Players.Count; i++)
			{
				PlayerData playerData = this.Players[i];
				flag |= (!playerData.Connected && (playerData.Team == teamKind || teamKind == TeamKind.Zero));
			}
			return flag;
		}

		public void ReloadPlayerCustomizations(Action whenDone)
		{
			Future future = new Future();
			future.WhenDone(delegate(IFuture x)
			{
				if (whenDone != null)
				{
					whenDone();
				}
			});
			for (int i = 0; i < this.Players.Count; i++)
			{
				PlayerData playerData = this.Players[i];
				playerData.ReloadCustomizations(future);
			}
			future.Result = null;
		}

		public List<PlayerData> GetPlayersAndBotsByTeam(TeamKind team)
		{
			switch (team)
			{
			case TeamKind.Zero:
				return this.PlayersAndBots;
			case TeamKind.Red:
				return this.RedTeamPlayersAndBots;
			case TeamKind.Blue:
				return this.BlueTeamPlayersAndBots;
			default:
				return null;
			}
		}

		public void OnCleanup()
		{
			this._currentPlayerData = null;
			this.Players.Clear();
			this.Bots.Clear();
			this.PlayersAndBots.Clear();
			this.RedTeamPlayersAndBots.Clear();
			this.BlueTeamPlayersAndBots.Clear();
			this._allByAddress.Clear();
			this._playersAndBotsId.Clear();
			this.Narrators.Clear();
			this.AllDatas.Clear();
			BaseParameter.ClearRegisteredParameters();
			PlayerCarFactory.ClearCharacterInfoDictionary();
		}

		public static BitLogger Log = new BitLogger(typeof(MatchPlayers));

		private int _serial;

		private PlayerData _currentPlayerData;

		private bool _mmrSet;

		private int _redMMR;

		private int _blueMMR;

		public readonly List<PlayerData> Clients = new List<PlayerData>(8);

		public readonly List<PlayerData> AllDatas = new List<PlayerData>(4);

		public readonly List<PlayerData> BlueTeamPlayersAndBots = new List<PlayerData>(4);

		public readonly List<PlayerData> RedTeamPlayersAndBots = new List<PlayerData>(4);

		private readonly Dictionary<byte, PlayerData> _allByAddress = new Dictionary<byte, PlayerData>(8);

		private readonly Dictionary<int, PlayerData> _playersAndBotsId = new Dictionary<int, PlayerData>(8);
	}
}
