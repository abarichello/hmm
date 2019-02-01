using System;
using System.Collections;
using ClientAPI.Objects;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;
using Swordfish.Common.exceptions;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class ProgressionController : GameHubObject
	{
		public IEnumerator GetAndApplyBoosters(PlayerData player, RewardsBag rewards)
		{
			Inventory[] holder = new Inventory[1];
			yield return GameHubObject.Hub.StartCoroutine(this.GetBoostersInventory(player.PlayerId, holder));
			Inventory boosterInv = holder[0];
			if (boosterInv == null)
			{
				ProgressionController.Log.FatalFormat("Player={0} has no booster inventory.", new object[]
				{
					player.PlayerId
				});
				yield break;
			}
			InventoryBag bag = (InventoryBag)((JsonSerializeable<T>)boosterInv.Bag);
			BoostersContent content = (BoostersContent)((JsonSerializeable<T>)bag.Content);
			if (content == null)
			{
				yield break;
			}
			BoosterConfig boosterConfigs = GameHubObject.Hub.SharedConfigs.BoosterConfigs;
			bool usedDaily = false;
			if (content.FreeDailyBoosterUses > 0)
			{
				rewards.XpByBoostDaily = Mathf.RoundToInt((float)(rewards.GetBaseXp() * boosterConfigs.FreeBoosterXpPercentage) / 100f);
				usedDaily = true;
			}
			long now = DateTime.UtcNow.Ticks;
			long boosterCutTime = now - (long)(GameHubObject.Hub.GameTime.GetSynchTime() + boosterConfigs.ExpirationGracePeriodMinutes * 60000) * 10000L;
			long finalXpTime = content.GetFinalXpTime();
			if (boosterCutTime < finalXpTime)
			{
				rewards.XpByBoost = Mathf.RoundToInt((float)(rewards.GetBaseXp() * boosterConfigs.XpBounsPercentage) / 100f);
			}
			if (usedDaily)
			{
				for (;;)
				{
					Future<long> res = new Future<long>();
					Future<Exception> error = new Future<Exception>();
					bag = (InventoryBag)((JsonSerializeable<T>)boosterInv.Bag);
					content = (BoostersContent)((JsonSerializeable<T>)bag.Content);
					content.FreeDailyBoosterUses--;
					bag.Content = content.ToString();
					BagWrapper wrapp = new BagWrapper
					{
						bagVersion = boosterInv.BagVersion,
						Owner = boosterInv.Id,
						Text = bag.ToString()
					};
					GameHubObject.Hub.ClientApi.inventory.UpdateInventoryBag(null, wrapp, delegate(object x, long y)
					{
						res.Result = y;
					}, delegate(object x, Exception e)
					{
						error.Result = e;
					});
					while (!res.IsDone && !error.IsDone)
					{
						yield return null;
					}
					if (error.IsDone)
					{
						if (!(error.Result is BagVersionException))
						{
							break;
						}
						holder[0] = null;
						yield return GameHubObject.Hub.StartCoroutine(this.GetBoostersInventory(player.PlayerId, holder));
						boosterInv = holder[0];
						if (boosterInv == null)
						{
							goto Block_8;
						}
					}
					if (res.IsDone)
					{
						goto Block_9;
					}
				}
				this.LogFail(<GetAndApplyBoosters>c__AnonStorey.error, string.Format("Failed to update bag for inv={0} player={1}", boosterInv.Id, player.PlayerId));
				yield break;
				Block_8:
				ProgressionController.Log.FatalFormat("Player={0} has no booster inventory.", new object[]
				{
					player.PlayerId
				});
				yield break;
				Block_9:
				yield break;
			}
			yield break;
		}

		private IEnumerator GetBoostersInventory(long playerId, Inventory[] holder)
		{
			Future<Exception> error = new Future<Exception>();
			Future<Inventory[]> getInvs = new Future<Inventory[]>();
			GameHubObject.Hub.ClientApi.inventory.GetPlayerInventories(null, playerId, delegate(object x, Inventory[] invs)
			{
				getInvs.Result = invs;
			}, delegate(object x, Exception fail)
			{
				error.Result = fail;
			});
			while (!error.IsDone && !getInvs.IsDone)
			{
				yield return null;
			}
			if (error.IsDone)
			{
				this.LogFail(error, string.Format("Failed to get inventories for player={0}", playerId));
				yield break;
			}
			Inventory[] inventories = getInvs.Result;
			Inventory boosterInv = null;
			foreach (Inventory inventory in inventories)
			{
				InventoryBag inventoryBag = (InventoryBag)((JsonSerializeable<T>)inventory.Bag);
				if (inventoryBag.Kind == InventoryBag.InventoryKind.Boosters)
				{
					boosterInv = inventory;
					break;
				}
			}
			holder[0] = boosterInv;
			yield break;
		}

		public IEnumerator GetEventBoosters(MarketingEventBag evtBag)
		{
			DateTime utcNow = DateTime.UtcNow;
			Future<MarketingEvent[]> futEvts = new Future<MarketingEvent[]>();
			Future<Exception> futError = new Future<Exception>();
			GameHubObject.Hub.ClientApi.marketing.GetEvents(null, utcNow.AddMilliseconds((double)(-(double)GameHubObject.Hub.GameTime.GetSynchTime())), utcNow, delegate(object x, MarketingEvent[] e)
			{
				futEvts.Result = e;
			}, delegate(object x, Exception e)
			{
				futError.Result = e;
			});
			while (!futError.IsDone && !futEvts.IsDone)
			{
				yield return null;
			}
			if (futError.IsDone)
			{
				this.LogFail(futError, "Failed to get marketing events");
				yield break;
			}
			foreach (MarketingEvent marketingEvent in futEvts.Result)
			{
				MarketingEventBag marketingEventBag = (MarketingEventBag)((JsonSerializeable<T>)marketingEvent.Bag);
				evtBag.XpBoostPercentage += marketingEventBag.XpBoostPercentage;
				evtBag.ScBoostPercentage += marketingEventBag.ScBoostPercentage;
			}
			MarketingEvent[] events;
			if (events.Length > 0)
			{
			}
			yield break;
		}

		private void LogFail(Future<Exception> error, string msg)
		{
			ProgressionController.Log.Fatal(msg, error.Result);
		}

		private void OnItemDeleted(object state)
		{
		}

		private void OnItemDeletedError(object state, Exception exception)
		{
			ProgressionController.Log.Fatal("Failed to delete booster=" + state, exception);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ProgressionController));

		private struct BoosterItemData
		{
			public Item Item;

			public BoostItem ItemType;
		}
	}
}
