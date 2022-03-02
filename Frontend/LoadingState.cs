using System;
using System.Collections;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using FMod;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.DataTransferObjects.Inventory;
using HeavyMetalMachines.Match;
using Hoplon.Serialization;
using Hoplon.Unity.Loading;
using Pocketverse;
using SharedUtils.Loading;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class LoadingState : GameState
	{
		protected override void OnStateEnabled()
		{
			LoadingState.Log.DebugFormat("LoadingState enabled", new object[0]);
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				GameHubBehaviour.Hub.Server.ClientSendPlayerLoadingInfo(0f);
			}
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				GameHubBehaviour.Hub.Server.TryStartPlayersReadyEventTimeout();
			}
			if (this.SnapshotFMODAsset)
			{
				this._snapshotInstance = FMODAudioManager.PlayAt(this.SnapshotFMODAsset, base.transform);
			}
			if (!GameHubBehaviour.Hub.Match.LevelIsTutorial() && GameHubBehaviour.Hub.GuiScripts)
			{
				GameHubBehaviour.Hub.GuiScripts.LoadingVersus.ShowWindow(GameHubBehaviour.Hub.Match.ArenaIndex);
			}
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				GameHubBehaviour.Hub.Swordfish.Log.BILogClient(50, true);
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				ScreenResolutionController.SetInGameQualityLevel();
				CustomizationPreLoader customizationPreLoader = new CustomizationPreLoader();
				customizationPreLoader.GatherCustomizations();
				customizationPreLoader.CommitToResourceLoader();
			}
			this.BasicPreCache();
			GameArenaConfig arenaConfig = GameHubBehaviour.Hub.ArenaConfig;
			int arenaIndex = GameHubBehaviour.Hub.Match.ArenaIndex;
			PreCacheArenaObjects[] array = arenaConfig.SceneObjectsToLoad(arenaIndex);
			for (int i = 0; i < array.Length; i++)
			{
				GameHubBehaviour.Hub.Resources.PreCachePrefab(array[i].Object.name, array[i].CacheCount);
			}
			if (this._loadAssetsAsyncCoroutine != null)
			{
				base.StopCoroutine(this._loadAssetsAsyncCoroutine);
				this._loadAssetsAsyncCoroutine = null;
			}
			this._loadAssetsAsyncCoroutine = base.StartCoroutine(this.LoadAssetsAsync());
		}

		private IEnumerator LoadAssetsAsync()
		{
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				ItemTypeScriptableObject playerCharacterItem = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[GameHubBehaviour.Hub.SharedConfigs.TutorialConfig.PlayerCharacterGuid];
				Future carPreCacheFuture = PlayerCarFactory.CarPreCache(playerCharacterItem.Id, Guid.Empty, GameHubBehaviour.Hub.Net.IsClient());
				while (!carPreCacheFuture.IsDone)
				{
					yield return null;
				}
				ItemTypeScriptableObject botSkinItem = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[GameHubBehaviour.Hub.SharedConfigs.TutorialConfig.BotSkinGuid];
				SkinItemTypeBag botSkinBag = (SkinItemTypeBag)((JsonSerializeable<!0>)botSkinItem.Bag);
				ItemTypeScriptableObject botCharacterItem = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[botSkinBag.CharacterItemTypeId];
				carPreCacheFuture = PlayerCarFactory.CarPreCache(botCharacterItem.Id, botSkinItem.Id, false);
				while (!carPreCacheFuture.IsDone)
				{
					yield return null;
				}
			}
			else
			{
				List<PlayerData> allPlayers = GameHubBehaviour.Hub.Players.PlayersAndBots;
				for (int i = 0; i < allPlayers.Count; i++)
				{
					PlayerData playerData = allPlayers[i];
					Future carPreCacheFuture2 = PlayerCarFactory.CarPreCache(playerData.CharacterItemType.Id, playerData.Customizations.GetGuidBySlot(59), GameHubBehaviour.Hub.Net.IsClient() && playerData.IsCurrentPlayer);
					while (!carPreCacheFuture2.IsDone)
					{
						yield return null;
					}
				}
			}
			LoadingState.Log.Info("Start loading");
			AsyncRequest<LoadingResult> request = Loading.Engine.LoadToken(base.LoadingToken);
			yield return request;
			if (LoadStatusExtensions.IsError(request.Result.Status))
			{
				LoadingState.Log.ErrorFormat("Loading failed with status: {0}", new object[]
				{
					request.Result.Status
				});
				yield return LoadingFailedHandler.HandleFailure(request.Result);
				yield break;
			}
			this._resourceLoaderPrepareCaches = base.StartCoroutine(ResourceLoader.Instance.PrepareCaches());
			yield return this._resourceLoaderPrepareCaches;
			LoadingState.Log.Info("OnLoadingComplete");
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				GameHubBehaviour.Hub.Swordfish.Log.BILogClient(51, true);
			}
			if (GameHubBehaviour.Hub.State.Current.StateKind == GameState.GameStateKind.GameWrapUp)
			{
				yield break;
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				GameHubBehaviour.Hub.Server.ClientSendPlayerLoadingInfo(0.5f);
			}
			this.gameState.LoadingToken.InheritData(base.LoadingToken);
			base.GoToState(this.gameState, false);
			yield break;
		}

		public void BasicPreCache()
		{
			if (GameHubBehaviour.Hub.BombManager.Rules.Weapon)
			{
				GameHubBehaviour.Hub.BombManager.Rules.Weapon.PreCacheAssets();
			}
			if (GameHubBehaviour.Hub.BombManager.Rules.BombInfo.Skin)
			{
				BombSkinItemTypeComponent component = GameHubBehaviour.Hub.BombManager.Rules.BombInfo.Skin.GetComponent<BombSkinItemTypeComponent>();
				GameHubBehaviour.Hub.Resources.PreCachePrefab(component.BombPrefabName, 1);
			}
		}

		protected override void OnStateDisabled()
		{
			base.OnStateDisabled();
			if (this._loadAssetsAsyncCoroutine != null)
			{
				base.StopCoroutine(this._loadAssetsAsyncCoroutine);
				this._loadAssetsAsyncCoroutine = null;
			}
			if (this._resourceLoaderPrepareCaches != null)
			{
				base.StopCoroutine(this._resourceLoaderPrepareCaches);
				this._resourceLoaderPrepareCaches = null;
			}
			if (!(GameHubBehaviour.Hub.State.Current is Game) && GameHubBehaviour.Hub.GuiScripts)
			{
				GameHubBehaviour.Hub.GuiScripts.LoadingVersus.HideWindow();
			}
			if (this._snapshotInstance != null)
			{
				this._snapshotInstance.Stop();
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(LoadingState));

		public GameState gameState;

		public AudioEventAsset SnapshotFMODAsset;

		private FMODAudioManager.FMODAudio _snapshotInstance;

		private Coroutine _loadAssetsAsyncCoroutine;

		private Coroutine _resourceLoaderPrepareCaches;
	}
}
