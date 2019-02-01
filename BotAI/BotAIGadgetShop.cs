using System;
using System.Collections.Generic;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Match;
using Pocketverse;
using Pocketverse.Util;
using UnityEngine;

namespace HeavyMetalMachines.BotAI
{
	public class BotAIGadgetShop : GameHubBehaviour
	{
		private CombatObject _botCombatObj
		{
			get
			{
				return this._carHub.combatObject;
			}
		}

		private HeavyMetalMachines.Character.CharacterInfo _info
		{
			get
			{
				return this._carHub.Player.Character;
			}
		}

		private BotAIGadgetList _gadgetList
		{
			get
			{
				return this._info.GadgetList;
			}
		}

		private void Awake()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				UnityEngine.Object.Destroy(this);
				return;
			}
		}

		private void OnEnable()
		{
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.BombManager_ListenToPhaseChange;
		}

		private void OnDisable()
		{
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.BombManager_ListenToPhaseChange;
		}

		private void BombManager_ListenToPhaseChange(BombScoreBoard.State state)
		{
		}

		public void InitValues(CarComponentHub carHub)
		{
			if (this._initialized)
			{
				return;
			}
			this._initialized = true;
			this._carHub = carHub;
			this._gadgetBehaviours = new Dictionary<GadgetSlot, GadgetBehaviour>();
			this._garageController = ((this._botCombatObj.Team != TeamKind.Red) ? GameHubBehaviour.Hub.BotAIHub.GarageControllerBlu : GameHubBehaviour.Hub.BotAIHub.GarageControllerRed);
		}

		public void BuyGadget()
		{
			for (int i = 0; i < this._gadgetList.GadgetList.Count; i++)
			{
				BotGadgetShopInfo gadgetShopInfo = this._gadgetList.GadgetList[i];
				GadgetBehaviour gadget = this._botCombatObj.GetGadget(gadgetShopInfo.GadgetSlot);
				GadgetBehaviour.UpgradeInstance upgradeInstance = Array.Find<GadgetBehaviour.UpgradeInstance>(gadget.Upgrades, (GadgetBehaviour.UpgradeInstance x) => x.Info.Name == gadgetShopInfo.UpgradeName);
				if (upgradeInstance == null)
				{
					BitLogger log = BotAIGadgetShop.Log;
					string format = "Bot={1} instance not found for upgrade={0} Gadget={2} Upgrades={3}";
					object[] array = new object[4];
					array[0] = gadgetShopInfo.UpgradeName;
					array[1] = base.Id.ObjId;
					array[2] = gadget.Info.Name;
					array[3] = Arrays.ToStringWithComma(Array.ConvertAll<GadgetBehaviour.UpgradeInstance, string>(gadget.Upgrades, (GadgetBehaviour.UpgradeInstance x) => x.Info.Name));
					log.WarnFormat(format, array);
				}
				else if (upgradeInstance.Level < upgradeInstance.MaxLevel)
				{
					if (GameHubBehaviour.Hub.ScrapBank.HaveEnoughScrap(base.Id.ObjId, upgradeInstance.CurrentPrice()))
					{
						bool flag;
						if (string.IsNullOrEmpty(gadgetShopInfo.UpgradeName))
						{
							flag = this._garageController.ServerBotBuyActivate(gadgetShopInfo.GadgetSlot, this._botCombatObj);
						}
						else
						{
							flag = this._garageController.ServerBotBuyUpgrade(gadgetShopInfo.GadgetSlot, gadgetShopInfo.UpgradeName, this._botCombatObj);
						}
						if (flag)
						{
						}
					}
				}
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(BotAIGadgetShop));

		private CarComponentHub _carHub;

		private bool _initialized;

		private GarageController _garageController;

		private Dictionary<GadgetSlot, GadgetBehaviour> _gadgetBehaviours;
	}
}
