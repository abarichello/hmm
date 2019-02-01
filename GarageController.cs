using System;
using System.Collections.Generic;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	[RemoteClass]
	public class GarageController : GameHubBehaviour, IBitComponent
	{
		private void ListenToStateChanged(GameState pChangedstate)
		{
			if (pChangedstate is Game)
			{
				return;
			}
			GarageController._gameGui = null;
		}

		private void Awake()
		{
			this._gadgetShopVisibilityStates = new Dictionary<byte, bool>(10);
			this._buybackUpgradeInfos = new Dictionary<byte, List<GarageController.BuybackUpgradeInfo>>();
			this._gadgetCountInfos = new Dictionary<byte, GarageController.GadgetCountInfoGroup>();
			if (GameHubBehaviour.Hub.Net.IsClient() && this.Team == GameHubBehaviour.Hub.Players.CurrentPlayerData.Team)
			{
				GameHubBehaviour.Hub.State.ListenToStateChanged += this.ListenToStateChanged;
				GarageController._gameGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<GameGui>();
				if (GarageController._gameGui == null)
				{
					GameGui.ListenToGameGuiCreation += this.OnGameGuiCreated;
				}
				else
				{
					this.OnGameGuiCreated();
				}
			}
			else if (GameHubBehaviour.Hub.Net.IsServer())
			{
				TeamKind team = this.Team;
				if (team != TeamKind.Red)
				{
					if (team == TeamKind.Blue)
					{
						GameHubBehaviour.Hub.BotAIHub.GarageControllerBlu = this;
					}
				}
				else
				{
					GameHubBehaviour.Hub.BotAIHub.GarageControllerRed = this;
				}
				GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.BombManagerOnListenToPhaseChange;
			}
			else
			{
				base.enabled = false;
			}
		}

		private void OnGameGuiCreated()
		{
			GameGui.ListenToGameGuiCreation -= this.OnGameGuiCreated;
			if (!GarageController._gameGui)
			{
				GarageController._gameGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<GameGui>();
			}
		}

		public void ClientPlayerOpenGadgetShop()
		{
			if (GameHubBehaviour.Hub.Net.isTest)
			{
				return;
			}
			this.DispatchReliable(new byte[0]).ServerPlayerOpenGadgetShop();
		}

		public void ClientPlayerCloseGadgetShop()
		{
			if (GameHubBehaviour.Hub.Net.isTest)
			{
				return;
			}
			this.DispatchReliable(new byte[0]).ServerPlayerCloseGadgetShop();
		}

		public void BuyActivateBySlot(int gadgetslot)
		{
			this.DispatchReliable(new byte[0]).ServerBuyActivate(gadgetslot);
		}

		public void BuyUpgradeBySlot(int gadgetslot, string upgradeName)
		{
			this.DispatchReliable(new byte[0]).ServerBuyUpgrade(gadgetslot, upgradeName);
		}

		public void SellUpgradeBySlot(int gadgetslot, string upgradeName)
		{
			this.DispatchReliable(new byte[0]).ServerSellUpgrade(gadgetslot, upgradeName);
		}

		[RemoteMethod]
		private void ServerPlayerOpenGadgetShop()
		{
			if (this.IsPlayerGarageOpen(this.Sender))
			{
				GarageController.Log.WarnFormat("[ClientShop] Open gadget shop - Shop already open for player={0}, how!?", new object[]
				{
					this.Sender
				});
				return;
			}
			this._gadgetShopVisibilityStates[this.Sender] = true;
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(this.Sender);
			if (playerByAddress == null)
			{
				GarageController.Log.ErrorFormat("[ClientShop] Open gadget shop - Failed to find player={0}, how!?", new object[]
				{
					this.Sender
				});
				return;
			}
			PlayerController bitComponent = playerByAddress.CharacterInstance.GetBitComponent<PlayerController>();
			if (bitComponent == null)
			{
				GarageController.Log.ErrorFormat("[ClientShop] Open gadget shop - Failed to find player controller={0}, how!?", new object[]
				{
					this.Sender
				});
				return;
			}
			bitComponent.ShopInterfaceOpen = true;
		}

		[RemoteMethod]
		private void ServerPlayerCloseGadgetShop()
		{
			if (!this.IsPlayerGarageOpen(this.Sender))
			{
				GarageController.Log.WarnFormat("[ClientShop] Close gadget shop - Shop already closed for player={0}, how!?", new object[]
				{
					this.Sender
				});
				return;
			}
			this._gadgetShopVisibilityStates[this.Sender] = false;
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(this.Sender);
			if (playerByAddress == null)
			{
				GarageController.Log.ErrorFormat("[ClientShop] Close gadget shop - Failed to find player={0}, how!?", new object[]
				{
					this.Sender
				});
				return;
			}
			PlayerController bitComponent = playerByAddress.CharacterInstance.GetBitComponent<PlayerController>();
			if (bitComponent == null)
			{
				GarageController.Log.ErrorFormat("[ClientShop] Close gadget shop - Failed to find player controller={0}, how!?", new object[]
				{
					this.Sender
				});
				return;
			}
			List<GarageController.BuybackUpgradeInfo> playerBuybackList = this.GetPlayerBuybackList(this.Sender);
			playerBuybackList.Clear();
			bitComponent.ShopInterfaceOpen = false;
		}

		[RemoteMethod]
		private void ServerBuyActivate(int gadgetKind)
		{
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(this.Sender);
			if (playerByAddress == null)
			{
				GarageController.Log.ErrorFormat("[Activate] Failed to find player={0}, how!?", new object[]
				{
					this.Sender
				});
				return;
			}
			this.InnerServerBuyActivate(this.Sender, (GadgetSlot)gadgetKind, playerByAddress.CharacterInstance.GetComponent<CombatObject>());
		}

		public bool ServerBotBuyActivate(GadgetSlot gadgetSlot, CombatObject bot)
		{
			if (bot == null)
			{
				GarageController.Log.ErrorFormat("[Activate] Failed to find GadgetSlot={0}, how!?", new object[]
				{
					gadgetSlot
				});
				return false;
			}
			return this.InnerServerBuyActivate(0, gadgetSlot, bot);
		}

		private bool InnerServerBuyActivate(byte sender, GadgetSlot slot, CombatObject combat)
		{
			GadgetBehaviour gadget = combat.GetGadget(slot);
			int price = gadget.Info.Price;
			if (!GameHubBehaviour.Hub.ScrapBank.SpendScrap(combat.Id.ObjId, price))
			{
				return false;
			}
			gadget.Activate();
			return true;
		}

		[RemoteMethod]
		private void ServerBuyUpgrade(int gadgetKind, string upgradeName)
		{
			if (!this.IsPlayerGarageOpen(this.Sender))
			{
				GarageController.Log.WarnFormat("[Upgrade] Buy - Shop is not open for player={0}, how!?", new object[]
				{
					this.Sender
				});
				return;
			}
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(this.Sender);
			if (playerByAddress == null)
			{
				GarageController.Log.ErrorFormat("[Upgrade] Buy - Failed to find player={0}, how!?", new object[]
				{
					this.Sender
				});
				return;
			}
			this.InnerServerBuyUpgrade(this.Sender, (GadgetSlot)gadgetKind, upgradeName, playerByAddress.CharacterInstance.GetBitComponent<CombatObject>());
		}

		public bool ServerBotBuyUpgrade(GadgetSlot gadgetSlot, string upgradeName, CombatObject combat)
		{
			if (combat == null)
			{
				GarageController.Log.ErrorFormat("[Upgrade] Failed to find GadgetSlot={0}, how!?", new object[]
				{
					gadgetSlot
				});
				return false;
			}
			return this.InnerServerBuyUpgrade(0, gadgetSlot, upgradeName, combat);
		}

		private bool InnerServerBuyUpgrade(byte sender, GadgetSlot slot, string upgradeName, CombatObject combat)
		{
			GadgetBehaviour gadget = combat.GetGadget(slot);
			if (!this.HasFunds(combat.Id.ObjId, upgradeName, gadget))
			{
				return false;
			}
			GadgetBehaviour.UpgradeInstance upgradeInstance = gadget.GetUpgradeInstance(upgradeName);
			if (upgradeInstance != null && upgradeInstance.Level >= upgradeInstance.MaxLevel)
			{
				return false;
			}
			GarageController.GadgetCountInfo playerGadgetCountInfo = this.GetPlayerGadgetCountInfo(combat.Player.PlayerAddress, slot);
			if (playerGadgetCountInfo.IsMaxed())
			{
				return false;
			}
			if (!combat.IsBot && !combat.Player.IsBotControlled)
			{
				BombMatchBI.PlayerUpgraded(combat.Id.ObjId, upgradeName, GarageController.UpgradeOperationKind.Upgrade);
				MatchLogWriter.Upgraded(combat.Id.ObjId, gadget.Info.DraftName, upgradeName, GarageController.UpgradeOperationKind.Upgrade);
			}
			gadget.Upgrade(upgradeName);
			playerGadgetCountInfo.Inc();
			List<GarageController.BuybackUpgradeInfo> playerBuybackList = this.GetPlayerBuybackList(sender);
			if (!this.IsSlotInPlayerBuybackList(playerBuybackList, slot, upgradeName))
			{
				playerBuybackList.Add(new GarageController.BuybackUpgradeInfo(slot, upgradeName));
			}
			return true;
		}

		private bool HasFunds(int objId, string upgradeName, GadgetBehaviour gadget)
		{
			int upgradeCost = this.GetUpgradeCost(gadget, upgradeName);
			return GameHubBehaviour.Hub.ScrapBank.SpendScrap(objId, upgradeCost);
		}

		[RemoteMethod]
		private void ServerSellUpgrade(int gadgetKind, string upgradeName)
		{
			if (!this.IsPlayerGarageOpen(this.Sender))
			{
				GarageController.Log.WarnFormat("[Upgrade] Buy - Shop is not open for player={0}, how!?", new object[]
				{
					this.Sender
				});
				return;
			}
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(this.Sender);
			if (playerByAddress == null)
			{
				GarageController.Log.ErrorFormat("[Upgrade] Sell. Failed to find player={0}, how!?", new object[]
				{
					this.Sender
				});
				return;
			}
			this.InnerServerSellUpgrade(this.Sender, (GadgetSlot)gadgetKind, upgradeName, playerByAddress.CharacterInstance.GetBitComponent<CombatObject>());
		}

		private bool InnerServerSellUpgrade(byte sender, GadgetSlot slot, string upgradeName, CombatObject combat)
		{
			GadgetBehaviour gadget = combat.GetGadget(slot);
			int num = this.GetUpgradeCost(gadget, upgradeName);
			if (!gadget.Downgrade(upgradeName))
			{
				return false;
			}
			List<GarageController.BuybackUpgradeInfo> playerBuybackList = this.GetPlayerBuybackList(sender);
			ScrapBank.ScrapReason reason;
			if (this.IsSlotInPlayerBuybackList(playerBuybackList, slot, upgradeName))
			{
				this.RemovePlayerBuybackInfo(playerBuybackList, slot, upgradeName);
				reason = ScrapBank.ScrapReason.sellUpgradeBuyback;
				BombMatchBI.PlayerUpgraded(combat.Id.ObjId, upgradeName, GarageController.UpgradeOperationKind.Return);
				MatchLogWriter.Upgraded(combat.Id.ObjId, gadget.Info.DraftName, upgradeName, GarageController.UpgradeOperationKind.Return);
			}
			else
			{
				num = Mathf.RoundToInt((float)num * this.HudGarageShopSettings.NonBuybackRefundModifier);
				reason = ScrapBank.ScrapReason.sellUpgrade;
				BombMatchBI.PlayerUpgraded(combat.Id.ObjId, upgradeName, GarageController.UpgradeOperationKind.Sell);
				MatchLogWriter.Upgraded(combat.Id.ObjId, gadget.Info.DraftName, upgradeName, GarageController.UpgradeOperationKind.Sell);
			}
			GameHubBehaviour.Hub.ScrapBank.AddScrap(combat.Id.ObjId, num, reason);
			GarageController.GadgetCountInfo playerGadgetCountInfo = this.GetPlayerGadgetCountInfo(sender, slot);
			playerGadgetCountInfo.Dec();
			return true;
		}

		private bool IsPlayerGarageOpen(byte sender)
		{
			bool flag;
			return this._gadgetShopVisibilityStates.TryGetValue(sender, out flag) && flag;
		}

		private int GetUpgradeCost(GadgetBehaviour gadget, string upgrade)
		{
			for (int i = 0; i < gadget.Upgrades.Length; i++)
			{
				GadgetBehaviour.UpgradeInstance upgradeInstance = gadget.Upgrades[i];
				if (string.Compare(upgrade, upgradeInstance.Info.Name) == 0)
				{
					return upgradeInstance.CurrentPrice();
				}
			}
			return 0;
		}

		private void OnDestroy()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				GarageController._gameGui = null;
				GameHubBehaviour.Hub.State.ListenToStateChanged -= this.ListenToStateChanged;
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.BombManagerOnListenToPhaseChange;
			}
			GameHubBehaviour.Hub.BotAIHub.GarageControllerRed = null;
			GameHubBehaviour.Hub.BotAIHub.GarageControllerBlu = null;
		}

		private List<GarageController.BuybackUpgradeInfo> GetPlayerBuybackList(byte sender)
		{
			List<GarageController.BuybackUpgradeInfo> list;
			if (!this._buybackUpgradeInfos.TryGetValue(sender, out list))
			{
				this._buybackUpgradeInfos[sender] = new List<GarageController.BuybackUpgradeInfo>();
			}
			return this._buybackUpgradeInfos[sender];
		}

		private bool IsSlotInPlayerBuybackList(List<GarageController.BuybackUpgradeInfo> buybackUpgradeInfos, GadgetSlot slot, string upgradeName)
		{
			for (int i = 0; i < buybackUpgradeInfos.Count; i++)
			{
				GarageController.BuybackUpgradeInfo buybackUpgradeInfo = buybackUpgradeInfos[i];
				if (buybackUpgradeInfo.IsSlot(slot, upgradeName))
				{
					return true;
				}
			}
			return false;
		}

		private bool RemovePlayerBuybackInfo(List<GarageController.BuybackUpgradeInfo> buybackUpgradeInfos, GadgetSlot slot, string upgradeName)
		{
			GarageController.BuybackUpgradeInfo buybackUpgradeInfo = null;
			for (int i = 0; i < buybackUpgradeInfos.Count; i++)
			{
				GarageController.BuybackUpgradeInfo buybackUpgradeInfo2 = buybackUpgradeInfos[i];
				if (buybackUpgradeInfo2.IsSlot(slot, upgradeName))
				{
					buybackUpgradeInfo = buybackUpgradeInfo2;
					break;
				}
			}
			if (buybackUpgradeInfo == null)
			{
				return false;
			}
			buybackUpgradeInfos.Remove(buybackUpgradeInfo);
			return true;
		}

		private GarageController.GadgetCountInfo GetPlayerGadgetCountInfo(byte sender, GadgetSlot slot)
		{
			GarageController.GadgetCountInfoGroup gadgetCountInfoGroup;
			if (!this._gadgetCountInfos.TryGetValue(sender, out gadgetCountInfoGroup))
			{
				this._gadgetCountInfos[sender] = new GarageController.GadgetCountInfoGroup(this.HudGarageShopSettings.Gadget0MaxQuantity, this.HudGarageShopSettings.Gadget1MaxQuantity, this.HudGarageShopSettings.UltimateMaxQuantity, this.HudGarageShopSettings.GenericMaxQuantity);
			}
			return this._gadgetCountInfos[sender].GetGadgetCountInfo(slot);
		}

		public void SelectInstance(string instanceName)
		{
			if (!GameHubBehaviour.Hub.BombManager.Rules.InstancesEnabled)
			{
				return;
			}
			this.DispatchReliable(new byte[0]).ServerSelectInstance(instanceName);
		}

		[RemoteMethod]
		public void ServerSelectInstance(string instanceName)
		{
			if (!GameHubBehaviour.Hub.BombManager.Rules.InstancesEnabled)
			{
				return;
			}
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(this.Sender);
			if (playerByAddress == null)
			{
				GarageController.Log.ErrorFormat("Failed to select instance. PlayerData not found for sender={0}", new object[]
				{
					this.Sender
				});
				return;
			}
			PlayerController bitComponent = playerByAddress.CharacterInstance.GetBitComponent<PlayerController>();
			if (bitComponent == null)
			{
				GarageController.Log.ErrorFormat("Failed to select instance. PlayerController not found for sender={0}", new object[]
				{
					this.Sender
				});
				return;
			}
			bitComponent.SelectedInstance = instanceName;
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				this.ServerBuyInstance();
			}
		}

		private void BombManagerOnListenToPhaseChange(BombScoreBoard.State state)
		{
			if (!GameHubBehaviour.Hub.BombManager.Rules.InstancesEnabled)
			{
				return;
			}
			if (state == BombScoreBoard.State.Shop)
			{
				this._isInShop = true;
				this.ServerSellInstance();
			}
			else if (this._isInShop)
			{
				this._isInShop = false;
				this.ServerBuyInstance();
			}
		}

		private void ServerBuyInstance()
		{
			if (!this.MyTeamCanChangeInstances() || !GameHubBehaviour.Hub.BombManager.Rules.InstancesEnabled)
			{
				return;
			}
			List<PlayerData> playersFromMyTeam = this.GetPlayersFromMyTeam();
			for (int i = 0; i < playersFromMyTeam.Count; i++)
			{
				PlayerData playerData = playersFromMyTeam[i];
				string text = string.Empty;
				bool flag = GameHubBehaviour.Hub.BombManager.ScoreBoard.Round > 0;
				bool flag2 = GameHubBehaviour.Hub.Match.LevelIsTutorial();
				if (!playerData.IsBot || (!flag2 && !flag))
				{
					PlayerController playerController = null;
					if (!playerData.IsBot)
					{
						playerController = playerData.CharacterInstance.GetBitComponent<PlayerController>();
						if (playerController == null)
						{
							GarageController.Log.ErrorFormat("Can't select instance. Failed to get PlayerController for PlayerData={0}", new object[]
							{
								playerData
							});
							goto IL_199;
						}
						text = playerController.SelectedInstance;
					}
					if (string.IsNullOrEmpty(text))
					{
						text = playerData.Character.CustomGadget0.DefaultInstance;
						if (string.IsNullOrEmpty(text))
						{
							GarageController.Log.WarnFormat("Default instance not configured for PlayerData={0}", new object[]
							{
								playerData
							});
							if (playerData.Character.CustomGadget0.Upgrades.Length <= 0)
							{
								GarageController.Log.ErrorFormat("Can't select instance. No upgrades configured for PlayerData={0}", new object[]
								{
									playerData
								});
								goto IL_199;
							}
							text = playerData.Character.CustomGadget0.Upgrades[0].Name;
						}
						if (playerController != null)
						{
							playerController.SelectedInstance = text;
						}
					}
					this.InnerServerBuyUpgrade(playerData.PlayerAddress, GadgetSlot.CustomGadget0, text, playerData.CharacterInstance.GetBitComponent<CombatObject>());
				}
				IL_199:;
			}
		}

		private void ServerSellInstance()
		{
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.Round == 0 || !this.MyTeamCanChangeInstances() || !GameHubBehaviour.Hub.BombManager.Rules.InstancesEnabled)
			{
				return;
			}
			List<PlayerData> playersFromMyTeam = this.GetPlayersFromMyTeam();
			for (int i = 0; i < playersFromMyTeam.Count; i++)
			{
				PlayerData playerData = playersFromMyTeam[i];
				if (!playerData.IsBot)
				{
					PlayerController bitComponent = playerData.CharacterInstance.GetBitComponent<PlayerController>();
					if (bitComponent == null)
					{
						GarageController.Log.ErrorFormat("Can't clear selected instance. Failed to get PlayerController for PlayerData={0}", new object[]
						{
							playerData
						});
					}
					else if (string.IsNullOrEmpty(bitComponent.SelectedInstance))
					{
						CombatObject bitComponent2 = playerData.CharacterInstance.GetBitComponent<CombatObject>();
						GadgetBehaviour gadget = bitComponent2.GetGadget(GadgetSlot.CustomGadget0);
						UpgradeInfo[] upgrades = playerData.Character.CustomGadget0.Upgrades;
						for (int j = 0; j < upgrades.Length; j++)
						{
							GadgetBehaviour.UpgradeInstance upgradeInstance = gadget.GetUpgradeInstance(upgrades[j].Name);
							if (upgradeInstance.Level > 0)
							{
								this.InnerServerSellUpgrade(playerData.PlayerAddress, GadgetSlot.CustomGadget0, upgrades[j].Name, bitComponent2);
							}
						}
					}
					else
					{
						this.InnerServerSellUpgrade(playerData.PlayerAddress, GadgetSlot.CustomGadget0, bitComponent.SelectedInstance, playerData.CharacterInstance.GetBitComponent<CombatObject>());
					}
				}
			}
		}

		private List<PlayerData> GetPlayersFromMyTeam()
		{
			return (this.Team != TeamKind.Blue) ? GameHubBehaviour.Hub.Players.RedTeamPlayersAndBots : GameHubBehaviour.Hub.Players.BlueTeamPlayersAndBots;
		}

		private bool MyTeamCanChangeInstances()
		{
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				return true;
			}
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.Round == 0)
			{
				return true;
			}
			int num = GameHubBehaviour.Hub.BombManager.ScoreBoard.Rounds.Count - 1;
			return num >= 0 && GameHubBehaviour.Hub.BombManager.ScoreBoard.Rounds[num].DeliverTeam != this.Team;
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

		public IGarageControllerAsync Async()
		{
			return this.Async(0);
		}

		public IGarageControllerAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new GarageControllerAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IGarageControllerDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new GarageControllerDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IGarageControllerDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new GarageControllerDispatch(this.OID);
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

		public object Invoke(int classId, short methodId, object[] args)
		{
			if (classId != 1017)
			{
				throw new Exception("Hierarchy in RemoteClass is not allowed!!! " + classId);
			}
			this._delayed = null;
			switch (methodId)
			{
			case 9:
				this.ServerPlayerOpenGadgetShop();
				return null;
			case 10:
				this.ServerPlayerCloseGadgetShop();
				return null;
			case 11:
				this.ServerBuyActivate((int)args[0]);
				return null;
			default:
				if (methodId != 28)
				{
					throw new ScriptMethodNotFoundException(classId, (int)methodId);
				}
				this.ServerSelectInstance((string)args[0]);
				return null;
			case 14:
				this.ServerBuyUpgrade((int)args[0], (string)args[1]);
				return null;
			case 18:
				this.ServerSellUpgrade((int)args[0], (string)args[1]);
				return null;
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(GarageController));

		public HudGarageShopSettings HudGarageShopSettings;

		public TeamKind Team;

		private CombatData _currentPlayerCombatData;

		private static GameGui _gameGui;

		private const int GadgetShopStatesCapacity = 10;

		private Dictionary<byte, bool> _gadgetShopVisibilityStates;

		private Dictionary<byte, List<GarageController.BuybackUpgradeInfo>> _buybackUpgradeInfos;

		private Dictionary<byte, GarageController.GadgetCountInfoGroup> _gadgetCountInfos;

		private bool _isInShop;

		public const int StaticClassId = 1017;

		private Identifiable _identifiable;

		[ThreadStatic]
		private GarageControllerAsync _async;

		[ThreadStatic]
		private GarageControllerDispatch _dispatch;

		private IFuture _delayed;

		public enum UpgradeOperationKind
		{
			Upgrade,
			Return,
			Sell
		}

		private class BuybackUpgradeInfo
		{
			public BuybackUpgradeInfo(GadgetSlot slot, string upgradeName)
			{
				this._slot = slot;
				this._upgradeName = upgradeName;
			}

			public bool IsSlot(GadgetSlot slot, string upgradeName)
			{
				return slot == this._slot && upgradeName == this._upgradeName;
			}

			private GadgetSlot _slot;

			private string _upgradeName;
		}

		private class GadgetCountInfoGroup
		{
			public GadgetCountInfoGroup(int gadget0CountInfoMax, int gadget1CountInfoMax, int ultimateCountInfoMax, int genericCountInfoMax)
			{
				this._gadget0CountInfo = new GarageController.GadgetCountInfo(gadget0CountInfoMax);
				this._gadget1CountInfo = new GarageController.GadgetCountInfo(gadget1CountInfoMax);
				this._ultimateCountInfo = new GarageController.GadgetCountInfo(ultimateCountInfoMax);
				this._genericCountInfo = new GarageController.GadgetCountInfo(genericCountInfoMax);
			}

			public GarageController.GadgetCountInfo GetGadgetCountInfo(GadgetSlot slot)
			{
				switch (slot)
				{
				case GadgetSlot.CustomGadget0:
					return this._gadget0CountInfo;
				case GadgetSlot.CustomGadget1:
					return this._gadget1CountInfo;
				case GadgetSlot.CustomGadget2:
					return this._ultimateCountInfo;
				case GadgetSlot.GenericGadget:
					return this._genericCountInfo;
				}
				return null;
			}

			private readonly GarageController.GadgetCountInfo _gadget0CountInfo;

			private readonly GarageController.GadgetCountInfo _gadget1CountInfo;

			private readonly GarageController.GadgetCountInfo _ultimateCountInfo;

			private readonly GarageController.GadgetCountInfo _genericCountInfo;
		}

		private class GadgetCountInfo
		{
			public GadgetCountInfo(int max)
			{
				this._count = 0;
				this._max = max;
			}

			public void Inc()
			{
				this._count++;
				if (this._count > this._max)
				{
					this._count = this._max;
				}
			}

			public void Dec()
			{
				this._count--;
				if (this._count < 0)
				{
					this._count = 0;
				}
			}

			public bool IsMaxed()
			{
				return this._count == this._max;
			}

			private readonly int _max;

			private int _count;
		}
	}
}
