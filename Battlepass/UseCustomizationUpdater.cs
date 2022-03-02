using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Specializations;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Inventory;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.Context;
using Hoplon.Serialization;
using Pocketverse;

namespace HeavyMetalMachines.Battlepass
{
	public class UseCustomizationUpdater : IMissionProgressUpdater
	{
		public UseCustomizationUpdater(IPlayerStats playerStats, Mission mission, ICollectionScriptableObject inventoryCollection)
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
			if (!this._inventoryCollection.Exists(itemTypeGuid))
			{
				UseCustomizationUpdater.Log.ErrorStackTrace(string.Format("Invalid item={0} trying to update item usage progress.", itemTypeGuid));
				return 0f;
			}
			IItemType itemType = this._inventoryCollection.Get(itemTypeGuid);
			float num = (float)((!checker.HasUsesCustomizationInMatch(itemType, this._playerStats)) ? 0 : 1);
			if (this._playerStats.MatchWon)
			{
				num *= this._mission.TargetVictoryModifier;
			}
			return num;
		}

		private readonly IPlayerStats _playerStats;

		private readonly Mission _mission;

		private readonly ICollectionScriptableObject _inventoryCollection;

		private static readonly BitLogger Log = new BitLogger(typeof(UseCustomizationUpdater));
	}
}
