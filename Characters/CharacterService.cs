﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Characters.PickServiceBehavior.Apis;
using HeavyMetalMachines.CharacterSelection.Rotation;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.ScriptableObjects;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matchmaking.Configuration;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Player;
using Hoplon.Serialization;
using Pocketverse;
using Pocketverse.Util;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Characters
{
	[RemoteClass]
	[Serializable]
	public class CharacterService : GameHubBehaviour, ISerializationCallbackReceiver, IBitComponent
	{
		public QueueConfiguration CompetitiveQueueConfiguration { get; set; }

		private void Awake()
		{
			this.SetInventoryCollection(GameHubBehaviour.Hub.InventoryColletion);
		}

		public void SetInventoryCollection(CollectionScriptableObject collectionScriptableObject)
		{
			this._inventoryCollection = collectionScriptableObject;
		}

		public void SetPickServiceBehavior(IPickServiceBehavior behavior)
		{
			this._currBehavior = behavior;
		}

		private IEnumerator Start()
		{
			this._characterRotation = new CharacterRotationHandler(GameHubBehaviour.Hub.Config, GameHubBehaviour.Hub.SharedConfigs, GameHubBehaviour.Hub.Swordfish.Connection, this._battlepassProgressScriptableObject, this._rotationWeekStorage, this._getPlayerRotation);
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				yield break;
			}
			yield return this._characterRotation.Initialize();
			yield break;
		}

		private int[] GetAvailableCharactersFor(PlayerData player, PlayerBag bag)
		{
			if (player.IsBot)
			{
				return this.GetAvailableCharactersForBots();
			}
			IEnumerable<int> enumerable = this.GetAllCharacterIds();
			if (!this._characterRotation.IsRotationDisabled)
			{
				IEnumerable<int> availableCharactersIdsFor = this._characterRotation.GetAvailableCharactersIdsFor(player, bag);
				enumerable = enumerable.Intersect(availableCharactersIdsFor);
			}
			if (GameHubBehaviour.Hub.Match.Kind == 3)
			{
				IEnumerable<int> competitiveModeBannedCharactersIds = this.GetCompetitiveModeBannedCharactersIds();
				enumerable = enumerable.Except(competitiveModeBannedCharactersIds);
			}
			return enumerable.ToArray<int>();
		}

		private IEnumerable<int> GetAllCharacterIds()
		{
			List<IItemType> allCharacters = this._inventoryCollection.GetAllCharacters();
			return allCharacters.Select(delegate(IItemType character)
			{
				CharacterItemTypeComponent component = character.GetComponent<CharacterItemTypeComponent>();
				return component.CharacterId;
			});
		}

		private IEnumerable<int> GetCompetitiveModeBannedCharactersIds()
		{
			return from character in this.CompetitiveQueueConfiguration.LockedCharacters
			select character.Id;
		}

		private int[] GetAvailableCharactersForBots()
		{
			List<IItemType> allCharacters = this._inventoryCollection.GetAllCharacters();
			List<int> list = new List<int>();
			for (int i = 0; i < allCharacters.Count; i++)
			{
				CharacterItemTypeComponent component = allCharacters[i].GetComponent<CharacterItemTypeComponent>();
				BotItemTypeComponent component2 = allCharacters[i].GetComponent<BotItemTypeComponent>();
				if (component2 != null && component2.IsAnAvailableBot)
				{
					list.Add(component.CharacterId);
				}
			}
			return list.ToArray();
		}

		public bool IsCharacterUnderRotationForPlayer(int charId, PlayerBag bag)
		{
			return this._characterRotation.IsCharacterUnderRotationForPlayer(charId, bag);
		}

		public bool AllConfirmed
		{
			get
			{
				return this._allConfirmed;
			}
		}

		public void InitPickMode()
		{
			CharacterService.Log.DebugFormat("InitPickMode called. Player count={0} Bots count={1} Narrator count={2} Pick time={3} Customization time={4}", new object[]
			{
				GameHubBehaviour.Hub.Players.Players.Count,
				GameHubBehaviour.Hub.Players.Bots.Count,
				GameHubBehaviour.Hub.Players.Narrators.Count,
				this.PickTime,
				this.CustomizationTime
			});
			this._allConfirmed = (GameHubBehaviour.Hub.Players.Players.Count <= 0);
		}

		[RemoteMethod]
		public bool SelectCharacter(int characterId)
		{
			return this._currBehavior.SelectCharacter(this.Sender, characterId);
		}

		public void SelectRandomCharacterByPriority(PlayerData player, bool priorityAvailableBots)
		{
			CharacterService.<SelectRandomCharacterByPriority>c__AnonStorey1 <SelectRandomCharacterByPriority>c__AnonStorey = new CharacterService.<SelectRandomCharacterByPriority>c__AnonStorey1();
			<SelectRandomCharacterByPriority>c__AnonStorey.player = player;
			<SelectRandomCharacterByPriority>c__AnonStorey.$this = this;
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				CharacterService.Log.ErrorFormat("SelectRandomCharacter called by client. How?!?", new object[0]);
				return;
			}
			int[] availableCharactersFor = this.GetAvailableCharactersFor(<SelectRandomCharacterByPriority>c__AnonStorey.player, <SelectRandomCharacterByPriority>c__AnonStorey.player.Bag);
			CharacterService.Log.InfoFormat("Random select for player={0} Characters={1} AllCharacters={2}", new object[]
			{
				<SelectRandomCharacterByPriority>c__AnonStorey.player.PlayerAddress,
				Arrays.ToStringWithComma(availableCharactersFor),
				Arrays.ToStringWithComma(this._inventoryCollection.AllCharactersByCharacterId.Keys.ToArray<int>())
			});
			IItemType[] array = Array.ConvertAll<int, IItemType>(availableCharactersFor, (int charId) => <SelectRandomCharacterByPriority>c__AnonStorey.$this._inventoryCollection.AllCharactersByCharacterId[charId]);
			TeamKind team = <SelectRandomCharacterByPriority>c__AnonStorey.player.Team;
			<SelectRandomCharacterByPriority>c__AnonStorey.priorityRoles = this.GetCurrentPriorityRoles(team);
			int i;
			for (i = 0; i < <SelectRandomCharacterByPriority>c__AnonStorey.priorityRoles.Count; i++)
			{
				IItemType[] array2 = Array.FindAll<IItemType>(array, delegate(IItemType charItemType)
				{
					CharacterItemTypeComponent component = charItemType.GetComponent<CharacterItemTypeComponent>();
					return component.Role == <SelectRandomCharacterByPriority>c__AnonStorey.priorityRoles[i];
				});
				if (priorityAvailableBots)
				{
					array2 = Array.FindAll<IItemType>(array2, delegate(IItemType charItemType)
					{
						BotItemTypeComponent component = charItemType.GetComponent<BotItemTypeComponent>();
						return component != null && component.IsAnAvailableBot;
					});
				}
				array2 = Array.FindAll<IItemType>(array2, delegate(IItemType charItemType)
				{
					CharacterItemTypeComponent component = charItemType.GetComponent<CharacterItemTypeComponent>();
					return <SelectRandomCharacterByPriority>c__AnonStorey.$this.AvailableToPickCharFilter(component.CharacterId, <SelectRandomCharacterByPriority>c__AnonStorey.player);
				});
				if (array2.Length > 0)
				{
					CharacterService.Log.DebugFormat("SelectRandomCharacter CharId={0} for Player={1}", new object[]
					{
						<SelectRandomCharacterByPriority>c__AnonStorey.player.SelectedChar,
						<SelectRandomCharacterByPriority>c__AnonStorey.player.PlayerAddress
					});
					IItemType itemType = array2[SysRandom.Int(0, array2.Length)];
					<SelectRandomCharacterByPriority>c__AnonStorey.player.SelectedChar = itemType.GetComponent<CharacterItemTypeComponent>().CharacterId;
					this.DispatchConfirmSelection(<SelectRandomCharacterByPriority>c__AnonStorey.player);
					return;
				}
			}
			if (<SelectRandomCharacterByPriority>c__AnonStorey.player.SelectedChar < 0)
			{
				CharacterService.Log.WarnFormat("Final failsafe to SelectRandomCharacterByPriority for player={0} team={1} playername={2}", new object[]
				{
					<SelectRandomCharacterByPriority>c__AnonStorey.player.PlayerAddress,
					<SelectRandomCharacterByPriority>c__AnonStorey.player.Team,
					<SelectRandomCharacterByPriority>c__AnonStorey.player.Name
				});
				this.SelectAnyAvailableRandom(<SelectRandomCharacterByPriority>c__AnonStorey.player, availableCharactersFor);
			}
		}

		private void SelectAnyAvailableRandom(PlayerData player, int[] charactersIds)
		{
			if (charactersIds == null)
			{
				charactersIds = this.GetAvailableCharactersFor(player, player.Bag);
			}
			int[] array = Array.FindAll<int>(charactersIds, (int charId) => this.AvailableToPickCharFilter(charId, player));
			if (array.Length <= 0)
			{
				CharacterService.Log.ErrorFormat("No possible character to randomly select for player={0} team={1}", new object[]
				{
					player.PlayerAddress,
					player.Team
				});
			}
			player.SelectedChar = array[SysRandom.Int(0, array.Length)];
			CharacterService.Log.InfoFormat("SelectAnyAvailableRandom Charid={0} for Player={1}", new object[]
			{
				player.SelectedChar,
				player.PlayerAddress
			});
			this.DispatchConfirmSelection(player);
		}

		private bool AvailableToPickCharFilter(int characterId, PlayerData player)
		{
			return !this.CharAlreadyPicked(player, player.Team, characterId) && this.IsCharacterAvailable(characterId);
		}

		private bool IsCharacterAvailable(int characterId)
		{
			IItemType itemType;
			if (!this._inventoryCollection.AllCharactersByCharacterId.TryGetValue(characterId, out itemType))
			{
				return false;
			}
			CharacterItemTypeComponent component = itemType.GetComponent<CharacterItemTypeComponent>();
			return component != null && component.CanBePicked;
		}

		public void DispatchConfirmSelection(PlayerData playerSelection)
		{
			this.DispatchReliable(GameHubBehaviour.Hub.AddressGroups.GetGroup((int)playerSelection.Team)).ConfirmSelection(playerSelection.PlayerAddress, playerSelection.SelectedChar);
			for (int i = 0; i < GameHubBehaviour.Hub.Players.Narrators.Count; i++)
			{
				PlayerData playerData = GameHubBehaviour.Hub.Players.Narrators[i];
				if (playerData.Team != playerSelection.Team)
				{
					this.DispatchReliable(new byte[]
					{
						playerData.PlayerAddress
					}).ConfirmSelection(playerSelection.PlayerAddress, playerSelection.SelectedChar);
				}
			}
		}

		public bool CharAlreadyPicked(PlayerData player, TeamKind team, int characterId)
		{
			List<PlayerData> playersAndBotsByTeam = GameHubBehaviour.Hub.Players.GetPlayersAndBotsByTeam(team);
			if (playersAndBotsByTeam == null)
			{
				CharacterService.Log.ErrorFormat("Player={0} asking picked with invalid team={1}", new object[]
				{
					player.PlayerAddress,
					team
				});
				return true;
			}
			for (int i = 0; i < playersAndBotsByTeam.Count; i++)
			{
				PlayerData playerData = playersAndBotsByTeam[i];
				if (!(playerData == null) && !(playerData == player))
				{
					if (playerData.CharacterId == characterId)
					{
						return true;
					}
				}
			}
			return false;
		}

		[RemoteMethod]
		public void ConfirmSelection(byte playerAddress, int characterId)
		{
			PickModeSetup pickModeSetup = GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.Pick) as PickModeSetup;
			pickModeSetup.OnConfirmSelectionCallback(new ConfirmSelectionCallback(playerAddress, characterId));
		}

		[RemoteMethod]
		public int PickCharacter()
		{
			return (int)this._currBehavior.PickCharacter(this.Sender);
		}

		public int PickCharacter(PlayerData player, bool isBot)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return 2;
			}
			if (player == null)
			{
				return 3;
			}
			if (player.CharacterId != -1 || player.CharSkinGridConfirmed)
			{
				CharacterService.Log.WarnFormat("PickPhaseOver player={0} characterId={1}", new object[]
				{
					player.PlayerAddress,
					player.CharacterId
				});
				return 7;
			}
			IItemType itemType = null;
			if (player.SelectedChar != -1)
			{
				this._inventoryCollection.AllCharactersByCharacterId.TryGetValue(player.SelectedChar, out itemType);
			}
			PickResult pickResult = PickResult.Ok;
			if (itemType == null)
			{
				CharacterService.Log.WarnFormat("CharacterNotFound player={0} characterId={1}", new object[]
				{
					player.PlayerAddress,
					player.SelectedChar
				});
				player.SelectedChar = -1;
				this.SelectRandomCharacterByPriority(player, true);
				pickResult = PickResult.CharacterNotFound;
			}
			if (player.SelectedChar < 0 || this.CharAlreadyPicked(player, player.Team, player.SelectedChar))
			{
				CharacterService.Log.WarnFormat("CharAlreadyPicked player={0} characterId={1}", new object[]
				{
					player.PlayerAddress,
					player.SelectedChar
				});
				player.SelectedChar = -1;
				this.SelectRandomCharacterByPriority(player, true);
				if (pickResult == PickResult.Ok)
				{
					pickResult = PickResult.CharacterAlreadyPicked;
				}
			}
			if (!isBot && !this.HasCharacter(player, player.SelectedChar))
			{
				CharacterService.Log.WarnFormat("CharacterNotOwned player={0} characterId={1}", new object[]
				{
					player.PlayerAddress,
					player.SelectedChar
				});
				player.SelectedChar = -1;
				this.SelectAnyAvailableRandom(player, null);
				if (pickResult == PickResult.Ok)
				{
					pickResult = PickResult.CharacterNotOwned;
				}
			}
			if (player.SelectedChar < 0)
			{
				CharacterService.Log.ErrorFormat("LastCharacterFallback. player={0} characterId ={1}", new object[]
				{
					player.PlayerAddress,
					player.SelectedChar
				});
				List<IItemType> allCharacters = this._inventoryCollection.GetAllCharacters();
				List<int> list = new List<int>();
				for (int i = 0; i < allCharacters.Count; i++)
				{
					CharacterItemTypeComponent component = allCharacters[i].GetComponent<CharacterItemTypeComponent>();
					if (allCharacters[i].IsActive)
					{
						list.Add(component.CharacterId);
					}
				}
				player.SelectedChar = list[SysRandom.Int(0, list.Count)];
				pickResult = PickResult.LastCharacterFallback;
			}
			CharacterService.Log.InfoFormat("PickCharacter. Player {0} SelectedChar={1} pickResult={2}", new object[]
			{
				player.PlayerAddress,
				player.SelectedChar,
				pickResult
			});
			if (pickResult != PickResult.Ok)
			{
				this.DispatchConfirmSelection(player);
				CharacterService.Log.WarnFormat("DispatchConfirmSelection on a fallback. player={0} characterId ={1} pickResult={2}", new object[]
				{
					player.PlayerAddress,
					player.SelectedChar,
					pickResult
				});
			}
			if (pickResult == PickResult.Ok || this.PickTime <= 0f)
			{
				player.SetCharacter(player.SelectedChar, GameHubBehaviour.Hub.InventoryColletion);
				this.DispatchConfirmPick(player.CharacterId, player);
			}
			this.CheckAllConfirmed();
			return (int)pickResult;
		}

		private void DispatchConfirmPick(int characterId, PlayerData player)
		{
			CharacterService.Log.DebugFormat("DispatchConfirmPick. player={0} characterId={1}", new object[]
			{
				player.PlayerAddress,
				characterId
			});
			this.DispatchReliable(GameHubBehaviour.Hub.AddressGroups.GetGroup((int)player.Team)).ConfirmPick(player.PlayerAddress, (int)player.Team, characterId, player.Customizations.GetGuidBySlot(59));
			for (int i = 0; i < GameHubBehaviour.Hub.Players.Narrators.Count; i++)
			{
				PlayerData playerData = GameHubBehaviour.Hub.Players.Narrators[i];
				if (playerData.Team != player.Team)
				{
					this.DispatchReliable(new byte[]
					{
						playerData.PlayerAddress
					}).ConfirmPick(player.PlayerAddress, (int)player.Team, characterId, player.Customizations.GetGuidBySlot(59));
				}
			}
		}

		public bool HasCharacter(PlayerData player, int characterId)
		{
			int num = Array.IndexOf<int>(this.GetAvailableCharactersFor(player, player.Bag), characterId);
			return num >= 0;
		}

		[RemoteMethod]
		public void ConfirmPick(byte playerAddress, int teamID, int characterId, Guid lastSkin)
		{
			PickModeSetup pickModeSetup = GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.Pick) as PickModeSetup;
			pickModeSetup.OnConfirmPickCallback(new ConfirmPickCallback(playerAddress, teamID, characterId, lastSkin));
		}

		[RemoteMethod]
		public void ConfirmSkin(string characterGuid, string skinGuid)
		{
			this._currBehavior.ConfirmSkin(this.Sender, characterGuid, skinGuid);
		}

		public void ServerConfirmSkin(PlayerData playerData, Guid characterGuid, Guid skinGuid)
		{
			CharacterBag message = new CharacterBag();
			message.PlayerId = playerData.PlayerId;
			message.CharacterId = characterGuid;
			if (skinGuid == Guid.Empty)
			{
				CharacterService.Log.InfoFormatStackTrace("Empty guid on ServerConfirmSkin. Going to default skin. Player={0} Character={1} ", new object[]
				{
					playerData.PlayerAddress,
					characterGuid
				});
				ItemTypeScriptableObject defaultSkin = this._inventoryCollection.GetDefaultSkin(characterGuid);
				message.Skin = defaultSkin.Id;
			}
			else
			{
				message.Skin = skinGuid;
			}
			CharacterService.Log.InfoFormat("ConfirmSkin Player={0} Skin={1}", new object[]
			{
				playerData.PlayerAddress,
				message.Skin
			});
			bool boolValue = GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false);
			if (boolValue)
			{
				NetResult netResult = new NetResult
				{
					Success = true
				};
				this.OnServerEquipCharacterResponse(playerData, message.Skin, netResult.ToString());
				return;
			}
			CharacterCustomWS.ServerEquipCharacter(message, delegate(object s, string o)
			{
				this.OnServerEquipCharacterResponse(playerData, message.Skin, o);
			}, delegate(object s, Exception e)
			{
				ItemTypeScriptableObject defaultSkin2 = this._inventoryCollection.GetDefaultSkin(characterGuid);
				this.OnServerEquipCharacterResponse(playerData, defaultSkin2.Id, string.Empty);
			});
		}

		private void OnServerEquipCharacterResponse(PlayerData playerData, Guid skinGuid, string result)
		{
			bool flag = false;
			NetResult netResult = new NetResult();
			if (!string.IsNullOrEmpty(result))
			{
				netResult = (NetResult)((JsonSerializeable<!0>)result);
				flag = netResult.Success;
			}
			if (flag)
			{
				playerData.Customizations.SetGuidAndSlot(59, skinGuid);
			}
			else
			{
				ItemTypeScriptableObject defaultSkin = this._inventoryCollection.GetDefaultSkin(playerData.CharacterItemType.Id);
				playerData.Customizations.SetGuidAndSlot(59, defaultSkin.Id);
			}
			CharacterService.Log.InfoFormat("DispatchConfirmSkinCallback Player={0} Skin={1} success={2}, message= {3}", new object[]
			{
				playerData.PlayerAddress,
				playerData.Customizations.GetGuidBySlot(59),
				flag,
				netResult.Msg
			});
			playerData.UpdateCharacters();
			this.DispatchReliable(GameHubBehaviour.Hub.AddressGroups.GetGroup(0)).ConfirmSkinCallback(playerData.PlayerAddress, (int)playerData.Team, flag, playerData.Customizations.GetGuidBySlot(59).ToString());
			this.DispatchConfirmSkinCallback(playerData.PlayerAddress, playerData.Team, flag, playerData.Customizations.GetGuidBySlot(59));
		}

		public void DispatchConfirmSkinCallback(byte playerAddress, TeamKind team, bool isSuccess, Guid selectedSkin)
		{
			this.DispatchReliable(GameHubBehaviour.Hub.AddressGroups.GetGroup(0)).ConfirmSkinCallback(playerAddress, (int)team, isSuccess, selectedSkin.ToString());
		}

		[RemoteMethod]
		public void ConfirmSkinCallback(byte playerAddress, int teamID, bool success, string skinGuid)
		{
			CharacterService.Log.DebugFormat("Confirm skin={0} for player={1} success={2}", new object[]
			{
				skinGuid,
				playerAddress,
				success
			});
			PickModeSetup pickModeSetup = GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.Pick) as PickModeSetup;
			pickModeSetup.OnConfirmSkinCallback(new ConfirmSkinCallback(playerAddress, teamID, success, skinGuid));
		}

		[RemoteMethod]
		public float GetPickTime()
		{
			return this.PickTime;
		}

		public void ClientAskPickModeState()
		{
			this.DispatchReliable(new byte[0]).ServerSendPickModeStateToPlayer();
		}

		[RemoteMethod]
		public void ServerSendPickModeStateToPlayer()
		{
			for (int i = 0; i < GameHubBehaviour.Hub.Players.PlayersAndBots.Count; i++)
			{
				this.SendPlayerEntryStateToPlayer(GameHubBehaviour.Hub.Players.PlayersAndBots[i], this.Sender);
			}
		}

		private void SendPlayerEntryStateToPlayer(PlayerData entry, byte playerToSend)
		{
			if (entry == null)
			{
				return;
			}
			byte playerAddress = entry.PlayerAddress;
			if (entry.SelectedChar < 0)
			{
				return;
			}
			this.DispatchReliable(new byte[]
			{
				playerToSend
			}).ConfirmSelection(playerAddress, entry.SelectedChar);
			if (!entry.CharConfirmed)
			{
				return;
			}
			int team = (int)entry.Team;
			Guid guidBySlot = entry.Customizations.GetGuidBySlot(59);
			this.DispatchReliable(new byte[]
			{
				playerToSend
			}).ConfirmPick(playerAddress, team, entry.CharacterId, guidBySlot);
			if (guidBySlot != Guid.Empty)
			{
				this.DispatchReliable(new byte[]
				{
					playerToSend
				}).ConfirmSkinCallback(playerAddress, team, true, guidBySlot.ToString());
			}
			if (entry.SelectedGridIndex < 0)
			{
				return;
			}
			this.DispatchConfirmGridSelection(playerAddress, entry.SelectedGridIndex, new byte[]
			{
				playerToSend
			});
			if (!entry.CharSkinGridConfirmed)
			{
				return;
			}
			this.DispatchConfirmGridPick(playerAddress, entry.GridIndex, new byte[]
			{
				playerToSend
			});
		}

		public void DispatchConfirmGridSelection(byte playerAddress, int gridIndex, params byte[] destnAddresses)
		{
			this.DispatchReliable(destnAddresses).ConfirmGridSelection(playerAddress, gridIndex);
		}

		public void DispatchConfirmGridPick(byte playerAddress, int gridIndex, params byte[] destnAddresses)
		{
			this.DispatchReliable(destnAddresses).ConfirmGridPick(playerAddress, gridIndex);
		}

		public void ServerSendCustomizationTime()
		{
			this.DispatchReliable(GameHubBehaviour.Hub.AddressGroups.GetGroup(0)).PickTimeOutClient(this.CustomizationTime);
		}

		private List<DriverRoleKind> GetCurrentPriorityRoles(TeamKind team)
		{
			List<DriverRoleKind> list = new List<DriverRoleKind>(GameHubBehaviour.Hub.BotAIMatchRules.BotPickConfig.roles);
			for (int i = 0; i < GameHubBehaviour.Hub.Players.PlayersAndBots.Count; i++)
			{
				PlayerData playerData = GameHubBehaviour.Hub.Players.PlayersAndBots[i];
				if (playerData.Team == team && playerData.CharacterItemType != null)
				{
					list.Remove(playerData.GetCharacterRole());
				}
			}
			return list;
		}

		[RemoteMethod]
		public void PickTimeOutClient(float customizationTime)
		{
			PickModeSetup pickModeSetup = GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.Pick) as PickModeSetup;
			pickModeSetup.OnPickTimeOutCallback(new PickTimeOutCallback(customizationTime));
		}

		public void CheckAllConfirmed()
		{
			for (int i = 0; i < GameHubBehaviour.Hub.Players.Players.Count; i++)
			{
				PlayerData playerData = GameHubBehaviour.Hub.Players.Players[i];
				if (!playerData.CharConfirmed)
				{
					CharacterService.Log.DebugFormat("CheckAllConfirmed: Player {0} didn't confirm yet.", new object[]
					{
						playerData.Name
					});
					this._allConfirmed = false;
					return;
				}
			}
			CharacterService.Log.Debug("CheckAllConfirmed: All confirmed.");
			this._allConfirmed = true;
		}

		[RemoteMethod]
		public void SelectGrid(int gridIndex)
		{
			this._currBehavior.SelectGrid(this.Sender, gridIndex);
		}

		[RemoteMethod]
		public void ConfirmGridSelection(byte playerAddress, int gridIndex)
		{
			PickModeSetup pickModeSetup = GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.Pick) as PickModeSetup;
			pickModeSetup.OnConfirmGridSelectionCallback(new ConfirmGridSelectionCallback(playerAddress, gridIndex));
		}

		[RemoteMethod]
		public int PickGrid()
		{
			return (int)this._currBehavior.PickGrid(this.Sender);
		}

		[RemoteMethod]
		public void ConfirmGridPick(byte playerAddress, int gridIndex)
		{
			PickModeSetup pickModeSetup = GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.Pick) as PickModeSetup;
			pickModeSetup.OnConfirmGridPickCallback(new ConfirmGridPickCallback(playerAddress, gridIndex));
		}

		[RemoteMethod]
		public void ClientSendCounselorActivation(bool counselorActivation)
		{
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(this.Sender);
			playerByAddress.HasCounselor = counselorActivation;
			CharacterService.Log.DebugFormat("ClientSendCounselorActivation. PlayerAddress: {0} counselorActivation {1}", new object[]
			{
				this.Sender,
				counselorActivation
			});
		}

		private int OID
		{
			get
			{
				if (!this._identifiable)
				{
					this._identifiable = base.GetComponent<Identifiable>();
				}
				return this._identifiable.ObjId;
			}
		}

		public byte Sender { get; set; }

		public ICharacterServiceAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterServiceAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterServiceAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterServiceDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterServiceDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterServiceDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterServiceDispatch(this.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		protected IFuture Delayed
		{
			get
			{
				return this._delayed;
			}
		}

		protected void Delay(IFuture future)
		{
			this._delayed = future;
		}

		public object Invoke(int classId, short methodId, object[] args, BitStream bitstream = null)
		{
			this._delayed = null;
			switch (methodId)
			{
			case 37:
				this.PickTimeOutClient((float)args[0]);
				return null;
			default:
				switch (methodId)
				{
				case 18:
					this.ConfirmSelection((byte)args[0], (int)args[1]);
					return null;
				case 19:
				{
					object result = this.PickCharacter();
					if (this._delayed != null)
					{
						return this._delayed;
					}
					return result;
				}
				default:
					switch (methodId)
					{
					case 28:
						this.ConfirmSkinCallback((byte)args[0], (int)args[1], (bool)args[2], (string)args[3]);
						return null;
					case 29:
					{
						object result2 = this.GetPickTime();
						if (this._delayed != null)
						{
							return this._delayed;
						}
						return result2;
					}
					default:
					{
						if (methodId != 11)
						{
							throw new ScriptMethodNotFoundException(classId, (int)methodId);
						}
						object result3 = this.SelectCharacter((int)args[0]);
						if (this._delayed != null)
						{
							return this._delayed;
						}
						return result3;
					}
					case 31:
						this.ServerSendPickModeStateToPlayer();
						return null;
					}
					break;
				case 23:
					this.ConfirmPick((byte)args[0], (int)args[1], (int)args[2], (Guid)args[3]);
					return null;
				case 24:
					this.ConfirmSkin((string)args[0], (string)args[1]);
					return null;
				}
				break;
			case 39:
				this.SelectGrid((int)args[0]);
				return null;
			case 40:
				this.ConfirmGridSelection((byte)args[0], (int)args[1]);
				return null;
			case 41:
			{
				object result4 = this.PickGrid();
				if (this._delayed != null)
				{
					return this._delayed;
				}
				return result4;
			}
			case 42:
				this.ConfirmGridPick((byte)args[0], (int)args[1]);
				return null;
			case 43:
				this.ClientSendCounselorActivation((bool)args[0]);
				return null;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(CharacterService));

		public float PickTime;

		public float CustomizationTime;

		private CharacterRotationHandler _characterRotation;

		private CollectionScriptableObject _inventoryCollection;

		private IPickServiceBehavior _currBehavior;

		[Inject]
		private IRotationWeekStorage _rotationWeekStorage;

		[Inject]
		private IGetPlayerRotation _getPlayerRotation;

		[SerializeField]
		private BattlepassProgressScriptableObject _battlepassProgressScriptableObject;

		private bool _allConfirmed;

		public int MaxGridSlots = 4;

		public const int StaticClassId = 1076;

		private Identifiable _identifiable;

		[ThreadStatic]
		private CharacterServiceAsync _async;

		[ThreadStatic]
		private CharacterServiceDispatch _dispatch;

		private IFuture _delayed;
	}
}
