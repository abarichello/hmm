using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using ClientAPI.Objects;
using HeavyMetalMachines.Character;
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

		private void Reset()
		{
			this.SetupMockSwordfish();
			this.SetupMockNetClientTest();
			base.Awake();
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

		public void SetupMockArenaConfig(int arenasCount = 1, int currArenaIndex = 0)
		{
			this.ArenaConfig = ScriptableObject.CreateInstance<GameArenaConfig>();
			this.ArenaConfig.Arenas = new GameArenaInfo[arenasCount];
			this.Match.ArenaIndex = currArenaIndex;
			for (int i = 0; i < this.ArenaConfig.Arenas.Length; i++)
			{
				this.ArenaConfig.Arenas[i] = new GameArenaInfo();
			}
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

		public void SetupMockInventoryCollection(HeavyMetalMachines.Character.CharacterInfo[] charactersInfo = null)
		{
			this.InventoryColletion = ScriptableObject.CreateInstance<CollectionScriptableObject>();
			charactersInfo = (charactersInfo ?? MockHmmHub.CreateCharacterInfos(30));
			CharacterInfoHandler characterInfoHandler = new CharacterInfoHandler(charactersInfo);
			this.InventoryColletion.SetupCharacterInfoHandler(characterInfoHandler);
		}

		private static HeavyMetalMachines.Character.CharacterInfo[] CreateCharacterInfos(int count)
		{
			List<HeavyMetalMachines.Character.CharacterInfo> list = new List<HeavyMetalMachines.Character.CharacterInfo>();
			for (int i = 0; i < count; i++)
			{
				HeavyMetalMachines.Character.CharacterInfo characterInfo = ScriptableObject.CreateInstance<HeavyMetalMachines.Character.CharacterInfo>();
				characterInfo.CharacterId = i;
				characterInfo.Role = MockHmmHub.GetDriverRoleKindByCharId(i);
				characterInfo.IsAnAvailableBot = true;
				list.Add(characterInfo);
			}
			return list.ToArray();
		}

		private static HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind GetDriverRoleKindByCharId(int charId)
		{
			switch (charId % 3)
			{
			case 0:
				return HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Carrier;
			case 1:
				return HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Tackler;
			case 2:
				return HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Support;
			default:
				return HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Carrier;
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
