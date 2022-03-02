using System;
using System.Linq;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class BotKreatorBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			this._spawnedBots = 0;
			this._userCount = 1;
			int num = 0;
			this.CreateBots(this.BlueBots, TeamKind.Blue, ref num);
			this.CreateBots(this.RedBots, TeamKind.Red, ref num);
			this._playersDispatcher.UpdatePlayers();
			GameHubBehaviour.Hub.Events.Bots.ListenToObjectSpawn += this.OnBotSpawned;
			GameHubBehaviour.Hub.Events.Bots.SpawnAllObjects();
		}

		private void CreateBots(ItemTypeScriptableObject[] botList, TeamKind team, ref int botId)
		{
			for (int i = 0; i < botList.Length; i++)
			{
				CharacterItemTypeComponent component = botList[i].GetComponent<CharacterItemTypeComponent>();
				this.CreateBot(component, i, this._userCount++, team, botId);
				botId++;
			}
		}

		private void OnBotSpawned(PlayerEvent data)
		{
			this._spawnedBots++;
			if (this._spawnedBots < GameHubBehaviour.Hub.Players.Bots.Count)
			{
				return;
			}
			this.CompleteBehaviourAndSync();
			GameHubBehaviour.Hub.Events.Bots.ListenToObjectSpawn -= this.OnBotSpawned;
		}

		private void CreateBot(CharacterItemTypeComponent charComponent, int slot, int userCount, TeamKind team, int botId)
		{
			PlayerData playerData = new PlayerData("-1", slot, team, (byte)(userCount + 10), slot, true, botId, false, new BattlepassProgress());
			playerData.Name = charComponent.GetBotLocalizedName();
			playerData.Customizations.SetGuidAndSlot(59, GameHubBehaviour.Hub.SharedConfigs.TutorialConfig.BotSkinGuid);
			playerData.SetCharacter(charComponent.CharacterId, GameHubBehaviour.Hub.InventoryColletion);
			GameHubBehaviour.Hub.Players.AddBot(playerData);
		}

		protected override void Destroy()
		{
			base.Destroy();
			this._spawnedBots = 0;
		}

		private void OnValidate()
		{
			this.ValidateAvailableBotCharacter(ref this.BlueBots);
			this.ValidateAvailableBotCharacter(ref this.RedBots);
		}

		private void ValidateAvailableBotCharacter(ref ItemTypeScriptableObject[] bots)
		{
			if (bots == null)
			{
				return;
			}
			if (bots.Any((ItemTypeScriptableObject b) => b.ContainsComponent<BotItemTypeComponent>()))
			{
				bots = (from b in bots
				where b.ContainsComponent<CharacterItemTypeComponent>() && b.ContainsComponent<BotItemTypeComponent>() && b.GetComponent<BotItemTypeComponent>().IsAnAvailableBot
				select b).ToArray<ItemTypeScriptableObject>();
			}
		}

		[Inject]
		private IMatchPlayersDispatcher _playersDispatcher;

		public ItemTypeScriptableObject[] BlueBots;

		public ItemTypeScriptableObject[] RedBots;

		private int _spawnedBots;

		private int _userCount;
	}
}
