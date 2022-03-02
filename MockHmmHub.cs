using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using Assets.ClientApiObjects.Components.Testable;
using ClientAPI.Objects;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.ScriptableObjects;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class MockHmmHub : HMMHub
	{
		public override bool IsWaitingInQueue()
		{
			return this.WaitingInQueue;
		}

		protected new void Awake()
		{
			this.SetupMockSwordfish();
			this.SetupMockNetClientTest();
			base.Awake();
		}

		private void Reset()
		{
		}

		public void SetupMockConfig()
		{
			this._config = new ConfigLoader();
			this._config.Initialize();
		}

		public void SetupMockSwordfish()
		{
			this.Swordfish = base.gameObject.AddComponent<SwordfishServices>();
		}

		public void SetupMockNetClientTest()
		{
			this.Net = base.gameObject.AddComponent<NetworkClient>();
			this.Net.isTest = true;
		}

		public void SetupMockNetServerTest()
		{
			this.Net = base.gameObject.AddComponent<NetworkServer>();
			this.Net.isTest = false;
		}

		public void SetupMockServerInfo()
		{
			base.gameObject.AddComponent<Identifiable>();
			this.Server = base.gameObject.AddComponent<ServerInfo>();
		}

		public void SetupMockUserData(bool mockPlayer = false)
		{
			Debug.LogFormat("[MockHmmHub.SetupMockUserData] Mock Player: {0}", new object[]
			{
				mockPlayer
			});
			this.User = base.gameObject.AddComponent<UserInfo>();
			this.User.SetTestBattlepassProgressScriptableObject(new BattlepassProgressScriptableObject());
			BattlepassProgress battlepassProgress = new BattlepassProgress();
			battlepassProgress.CurrentXp = 2000;
			this.User.SetBattlepassProgress(battlepassProgress.ToString());
			PlayerBag playerBag = new PlayerBag();
			playerBag.Level = 5;
			playerBag.FounderPackLevel = 5;
			this.User.UniversalId = "MyMockUserId";
			this.User.Bag = playerBag;
			this.User.Characters = new Character[2];
			Character character = new Character();
			character.Bag = new CharacterBag
			{
				WinsCount = 5
			}.ToString();
			this.User.Characters[0] = character;
			this.User.Characters[1] = character;
			if (!mockPlayer)
			{
				return;
			}
			Player player = new Player();
			player.Name = "Mockonildo da Silva";
			player.Id = 2345678L;
			this.User.PlayerSF = player;
		}

		public void SetupMockInventoryCollection(ItemTypeScriptableObject[] charactersInfo = null)
		{
			this.InventoryColletion = ScriptableObject.CreateInstance<CollectionScriptableObject>();
			charactersInfo = (charactersInfo ?? MockHmmHub.CreateCharacterInfos(30));
			CharacterInfoHandler characterInfoHandler = new CharacterInfoHandler(charactersInfo);
			this.InventoryColletion.SetupCharacterInfoHandler(characterInfoHandler);
		}

		private static ItemTypeScriptableObject[] CreateCharacterInfos(int count)
		{
			List<ItemTypeScriptableObject> list = new List<ItemTypeScriptableObject>();
			for (int i = 0; i < count; i++)
			{
				CharacterItemTypeComponentTestable characterItemTypeComponentTestable = ScriptableObject.CreateInstance<CharacterItemTypeComponentTestable>();
				characterItemTypeComponentTestable.SetCharacterId(i);
				characterItemTypeComponentTestable.SetRole(MockHmmHub.GetDriverRoleKindByCharId(i));
				BotItemTypeComponentTestable botItemTypeComponentTestable = ScriptableObject.CreateInstance<BotItemTypeComponentTestable>();
				botItemTypeComponentTestable.SetIsAnAvailableBot(true);
				ItemTypeScriptableObject itemTypeScriptableObject = ScriptableObject.CreateInstance<ItemTypeScriptableObject>();
				itemTypeScriptableObject.ReplaceItemTypeComponents(new ItemTypeComponent[0]);
				itemTypeScriptableObject.AddItemTypeComponent(characterItemTypeComponentTestable);
				itemTypeScriptableObject.AddItemTypeComponent(botItemTypeComponentTestable);
				list.Add(itemTypeScriptableObject);
			}
			return list.ToArray();
		}

		private static DriverRoleKind GetDriverRoleKindByCharId(int charId)
		{
			switch (charId % 3)
			{
			case 0:
				return 1;
			case 1:
				return 2;
			case 2:
				return 0;
			default:
				return 1;
			}
		}

		private void OnDestroy()
		{
			Debug.LogWarning("DESTROYING MockHmmHub");
		}

		public bool WaitingInQueue;

		public static int CarrierCharId0;

		public static int TacklerCharId0 = 1;

		public static int SupportCharId0 = 2;

		public static int CarrierCharId1 = 3;

		public static int TacklerCharId1 = 4;

		public static int SupportCharId1 = 5;

		public static int CarrierCharId2 = 6;

		public static int TacklerCharId2 = 7;

		public static int SupportCharId2 = 8;
	}
}
