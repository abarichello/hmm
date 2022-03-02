using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Specializations;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Inventory;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.Context;
using Hoplon.Serialization;

namespace HeavyMetalMachines.Battlepass
{
	public class PlayWithCharacterUpdater : IMissionProgressUpdater
	{
		public PlayWithCharacterUpdater(IPlayerStats playerStats, Mission mission, ICollectionScriptableObject inventoryCollection)
		{
			this._playerStats = playerStats;
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
			IItemType itemType = this._inventoryCollection.Get(guid);
			if (itemType is PackageItemTypeScriptableObject)
			{
				PackageItemTypeScriptableObject packageItemTypeScriptableObject = (PackageItemTypeScriptableObject)itemType;
				PackageItemTypeBag packageBag = (PackageItemTypeBag)((JsonSerializeable<!0>)packageItemTypeScriptableObject.Bag);
				num = this.VerifyUsagesInPackageItens(packageBag, num);
			}
			else
			{
				num = this.UpdateItemUsageProgress(guid);
			}
			progressValue.CurrentValue += num;
		}

		private float VerifyUsagesInPackageItens(PackageItemTypeBag packageBag, float progress)
		{
			for (int i = 0; i < packageBag.itens.Length; i++)
			{
				PackageItem packageItem = packageBag.itens[i];
				progress = this.UpdateItemUsageProgress(packageItem.Id);
				if (progress > 0f)
				{
					break;
				}
			}
			return progress;
		}

		private float UpdateItemUsageProgress(Guid itemTypeGuid)
		{
			int num = (!(this._playerStats.CharacterItemTypeGuid == itemTypeGuid)) ? 0 : 1;
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

		private readonly ICollectionScriptableObject _inventoryCollection;
	}
}
