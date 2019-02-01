using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using FMod;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;
using SharedUtils.Loading;

namespace HeavyMetalMachines.Frontend
{
	public class LoadingState : GameState
	{
		public override void EnableState()
		{
			base.EnableState();
			if (this.SnapshotFMODAsset)
			{
				this._snapshotInstance = FMODAudioManager.PlayAt(this.SnapshotFMODAsset, base.transform);
			}
			int arenaIndex = GameHubBehaviour.Hub.Match.ArenaIndex;
			GameArenaConfig arenaConfig = GameHubBehaviour.Hub.ArenaConfig;
			CustomizationPreLoader customizationPreLoader = new CustomizationPreLoader();
			customizationPreLoader.GatherCustomizations();
			customizationPreLoader.CommitToResourceLoader();
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				ItemTypeScriptableObject itemTypeScriptableObject = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[GameHubBehaviour.Hub.SharedConfigs.TutorialConfig.PlayerCharacterGuid];
				CharacterItemTypeComponent component = itemTypeScriptableObject.GetComponent<CharacterItemTypeComponent>();
				PlayerCarFactory.CarPreCache(component.MainAttributes, Guid.Empty, GameHubBehaviour.Hub.Net.IsClient());
				ItemTypeScriptableObject itemTypeScriptableObject2 = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[GameHubBehaviour.Hub.SharedConfigs.TutorialConfig.BotSkinGuid];
				SkinItemTypeBag skinItemTypeBag = (SkinItemTypeBag)((JsonSerializeable<T>)itemTypeScriptableObject2.Bag);
				ItemTypeScriptableObject itemTypeScriptableObject3 = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[skinItemTypeBag.CharacterItemTypeId];
				CharacterItemTypeComponent component2 = itemTypeScriptableObject3.GetComponent<CharacterItemTypeComponent>();
				PlayerCarFactory.CarPreCache(component2.MainAttributes, itemTypeScriptableObject2.Id, false);
			}
			else
			{
				if (GameHubBehaviour.Hub.GuiScripts)
				{
					GameHubBehaviour.Hub.GuiScripts.LoadingVersus.ShowWindow(arenaIndex);
				}
				List<PlayerData> playersAndBots = GameHubBehaviour.Hub.Players.PlayersAndBots;
				for (int i = 0; i < playersAndBots.Count; i++)
				{
					PlayerData playerData = playersAndBots[i];
					PlayerCarFactory.CarPreCache(playerData.Character, playerData.Customizations.SelectedSkin, GameHubBehaviour.Hub.Net.IsClient() && playerData.IsCurrentPlayer);
				}
			}
			this.BasicPreCache();
			string sceneName = arenaConfig.GetSceneName(arenaIndex);
			for (int j = 0; j < arenaConfig.SceneObjectsToLoad(arenaIndex).Length; j++)
			{
				GameHubBehaviour.Hub.Resources.PreCachePrefab(arenaConfig.SceneObjectsToLoad(arenaIndex)[j].Object.name, arenaConfig.SceneObjectsToLoad(arenaIndex)[j].CacheCount);
			}
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				base.LoadingToken.AddLoadable(new SceneLoadable(sceneName, false));
				if (GameHubBehaviour.Hub.GuiScripts)
				{
					GameHubBehaviour.Hub.GuiScripts.Loading.TutorialLoadingStarted = true;
				}
				GameHubBehaviour.Hub.Swordfish.Log.BILogClient(ClientBITags.TutorialLoadingStarted, true);
			}
			else
			{
				base.LoadingToken.AddLoadable(new SceneLoadable(sceneName, false));
			}
			base.LoadingToken.StartLoading(new LoadingManager.LoadCompleteCallback(this.OnLoadingComplete));
		}

		public void BasicPreCache()
		{
			if (GameHubBehaviour.Hub.BombManager.Rules.Weapon)
			{
				GameHubBehaviour.Hub.BombManager.Rules.Weapon.PreCacheAssets();
			}
		}

		private void OnLoadingComplete()
		{
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				GameHubBehaviour.Hub.Swordfish.Log.BILogClient(ClientBITags.TutorialLoadingCompleted, true);
			}
			this.gameState.LoadingToken.InheritData(base.LoadingToken);
			base.GoToState(this.gameState, false);
		}

		protected override void OnStateDisabled()
		{
			base.OnStateDisabled();
			if (this._snapshotInstance != null)
			{
				this._snapshotInstance.Stop();
			}
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial() && GameHubBehaviour.Hub.GuiScripts)
			{
				GameHubBehaviour.Hub.GuiScripts.Loading.TutorialLoadingStarted = true;
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(LoadingState));

		public GameState gameState;

		public FMODAsset SnapshotFMODAsset;

		private FMODAudioManager.FMODAudio _snapshotInstance;
	}
}
