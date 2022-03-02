using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Specializations;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Inventory;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using Hoplon.Serialization;

namespace HeavyMetalMachines.Battlepass
{
	public class PlayWithCharacterInMatchUpdater : IMissionProgressUpdater
	{
		public PlayWithCharacterInMatchUpdater(IPlayerStats playerStats, IMatchPlayers matchPlayers, Mission mission, ICollectionScriptableObject inventoryCollection)
		{
			this._playerStats = playerStats;
			this._matchPlayers = matchPlayers;
			this._mission = mission;
			this._inventoryCollection = inventoryCollection;
		}

		public void Update(MissionProgressValue progressValue, Objectives missionObjectives)
		{
			float num = 0f;
			List<PlayerData> playersAndBotsByTeam = this._matchPlayers.GetPlayersAndBotsByTeam(TeamKind.Zero);
			Guid guid = new Guid(missionObjectives.ItemId);
			if (!this._inventoryCollection.Exists(guid))
			{
				return;
			}
			IItemType itemType = this._inventoryCollection.Get(guid);
			PackageItemTypeScriptableObject packageItemTypeScriptableObject = itemType as PackageItemTypeScriptableObject;
			if (packageItemTypeScriptableObject != null)
			{
				PackageItemTypeBag packageBag = (PackageItemTypeBag)((JsonSerializeable<!0>)packageItemTypeScriptableObject.Bag);
				num = this.VerifyUsagesInPackageItens(packageBag, num, playersAndBotsByTeam);
			}
			else
			{
				num = this.UpdateItemUsageProgress(guid, playersAndBotsByTeam);
			}
			progressValue.CurrentValue += num;
		}

		private float VerifyUsagesInPackageItens(PackageItemTypeBag packageBag, float progress, List<PlayerData> allPlayers)
		{
			for (int i = 0; i < packageBag.itens.Length; i++)
			{
				PackageItem packageItem = packageBag.itens[i];
				progress = this.UpdateItemUsageProgress(packageItem.Id, allPlayers);
				if (progress > 0f)
				{
					break;
				}
			}
			return progress;
		}

		private int CharacterHasInThematch(Guid itemTypeGuid, List<PlayerData> allPlayers)
		{
			for (int i = 0; i < allPlayers.Count; i++)
			{
				CharacterInfo character = allPlayers[i].Character;
				if (itemTypeGuid == character.CharacterItemTypeGuid)
				{
					return 1;
				}
			}
			return 0;
		}

		private float UpdateItemUsageProgress(Guid itemTypeGuid, List<PlayerData> allPlayers)
		{
			int num = this.CharacterHasInThematch(itemTypeGuid, allPlayers);
			float result;
			if (this._playerStats.MatchWon)
			{
				result = (float)num * this._mission.TargetVictoryModifier;
			}
			else
			{
				result = (float)num;
			}
			return result;
		}

		private readonly IPlayerStats _playerStats;

		private readonly Mission _mission;

		private readonly IMatchPlayers _matchPlayers;

		private readonly ICollectionScriptableObject _inventoryCollection;
	}
}
