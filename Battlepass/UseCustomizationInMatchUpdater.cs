using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Specializations;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Inventory;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using Hoplon.Serialization;

namespace HeavyMetalMachines.Battlepass
{
	public class UseCustomizationInMatchUpdater : IMissionProgressUpdater
	{
		public UseCustomizationInMatchUpdater(IPlayerStats playerStats, IMatchPlayers matchPlayers, Mission mission, ICollectionScriptableObject inventoryCollection)
		{
			this._playerStats = playerStats;
			this._matchPlayers = matchPlayers;
			this._mission = mission;
			this._inventoryCollection = inventoryCollection;
		}

		public void Update(MissionProgressValue progressValue, Objectives missionObjectives)
		{
			float num = 0f;
			Guid guid = new Guid(missionObjectives.ItemId);
			if (!this._inventoryCollection.Exists(guid))
			{
				return;
			}
			CustomizationUsesInMatchChecker checker = new CustomizationUsesInMatchChecker();
			IItemType itemType = this._inventoryCollection.Get(guid);
			if (itemType is PackageItemTypeScriptableObject)
			{
				PackageItemTypeScriptableObject packageItemTypeScriptableObject = (PackageItemTypeScriptableObject)itemType;
				PackageItemTypeBag packageBag = (PackageItemTypeBag)((JsonSerializeable<!0>)packageItemTypeScriptableObject.Bag);
				num = this.VerifyUsagesInPackageItens(packageBag, num, checker);
			}
			else
			{
				num = this.UpdateItemUsageProgress(guid, checker);
			}
			progressValue.CurrentValue += num;
		}

		private float VerifyUsagesInPackageItens(PackageItemTypeBag packageBag, float progress, CustomizationUsesInMatchChecker checker)
		{
			for (int i = 0; i < packageBag.itens.Length; i++)
			{
				PackageItem packageItem = packageBag.itens[i];
				progress = this.UpdateItemUsageProgress(packageItem.Id, checker);
				if (progress > 0f)
				{
					break;
				}
			}
			return progress;
		}

		private float UpdateItemUsageProgress(Guid itemTypeGuid, CustomizationUsesInMatchChecker checker)
		{
			List<PlayerData> playersAndBotsByTeam = this._matchPlayers.GetPlayersAndBotsByTeam(TeamKind.Zero);
			int num = 0;
			IItemType itemType = this._inventoryCollection.Get(itemTypeGuid);
			for (int i = 0; i < playersAndBotsByTeam.Count; i++)
			{
				IPlayerStats playerStats = playersAndBotsByTeam[i].PlayerStats;
				if (checker.HasUsesCustomizationInMatch(itemType, playerStats))
				{
					num = 1;
					break;
				}
			}
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

		private readonly IMatchPlayers _matchPlayers;

		private readonly Mission _mission;

		private readonly ICollectionScriptableObject _inventoryCollection;
	}
}
