using System;
using System.Linq;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class BotKreatorBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			this._spawnedBots = 0;
			int num = 1;
			for (int i = 0; i < this.BlueBots.Length; i++)
			{
				this.CreateBot(this.BlueBots[i], i, num++, TeamKind.Blue);
			}
			for (int j = 0; j < this.RedBots.Length; j++)
			{
				this.CreateBot(this.RedBots[j], j + 1, num++, TeamKind.Red);
			}
			GameHubBehaviour.Hub.Players.UpdatePlayers();
			GameHubBehaviour.Hub.Events.Bots.ListenToObjectSpawn += this.OnBotSpawned;
			GameHubBehaviour.Hub.Events.Bots.SpawnAllObjects();
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

		private void CreateBot(CharacterInfo info, int slot, int userCount, TeamKind team)
		{
			PlayerData playerData = new PlayerData("-1", slot, team, (byte)(userCount + 10), slot, true, false, new BattlepassProgress());
			playerData.Name = info.LocalizedBotName;
			playerData.Customizations.SelectedSkin = GameHubBehaviour.Hub.SharedConfigs.TutorialConfig.BotSkinGuid;
			playerData.SetCharacter(info.CharacterId);
			GameHubBehaviour.Hub.Players.AddBot(playerData);
			BombMatchBI.CreatePlayer(playerData);
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

		private void ValidateAvailableBotCharacter(ref CharacterInfo[] bots)
		{
			if (bots == null)
			{
				return;
			}
			if (bots.Any((CharacterInfo b) => b.IsAnAvailableBot))
			{
				bots = (from b in bots
				where b.IsAnAvailableBot
				select b).ToArray<CharacterInfo>();
			}
		}

		public CharacterInfo[] BlueBots;

		public CharacterInfo[] RedBots;

		private int _spawnedBots;
	}
}
