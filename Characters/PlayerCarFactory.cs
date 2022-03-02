using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.AI;
using HeavyMetalMachines.Audio;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.GadgetScript;
using HeavyMetalMachines.Combat.Modifier;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Pipeline;
using HeavyMetalMachines.Playback;
using HeavyMetalMachines.Render;
using HeavyMetalMachines.VFX;
using Hoplon.DependencyInjection;
using Hoplon.Unity.Loading;
using Pocketverse;
using SharedUtils.Loading;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Characters
{
	public class PlayerCarFactory : GameHubBehaviour, IDynamicAssetListener<Object>
	{
		private void Awake()
		{
			PlayerCarFactory._instance = this;
			if (PlayerCarFactory._characterInfos == null)
			{
				PlayerCarFactory._characterInfos = new Dictionary<int, CharacterInfo>(8);
			}
		}

		public bool OrderObject(int id, IFuture<Identifiable> result)
		{
			this.CreateCar(id, result);
			return true;
		}

		private void CreateCar(int id, IFuture<Identifiable> result)
		{
			if (!GameHubBehaviour.Hub.Characters)
			{
				result.Cancel();
				return;
			}
			PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(id);
			if (playerOrBotsByObjectId == null)
			{
				PlayerCarFactory.Log.ErrorFormat("PlayerData not found for player={0}!", new object[]
				{
					id.GetInstanceId()
				});
				result.Cancel();
				return;
			}
			playerOrBotsByObjectId.Character = PlayerCarFactory._characterInfos[playerOrBotsByObjectId.CharacterId];
			if (!playerOrBotsByObjectId.Character)
			{
				PlayerCarFactory.Log.ErrorFormat("Player={0} with null character!", new object[]
				{
					id.GetInstanceId()
				});
				result.Cancel();
				return;
			}
			Guid guidBySlot = playerOrBotsByObjectId.Customizations.GetGuidBySlot(59);
			string carAssetName = PlayerCarFactory.GetCarAssetName(playerOrBotsByObjectId.CharacterItemType.Id, guidBySlot);
			PlayerCarFactory.Log.DebugFormat("Car Creation start - PlayerData: {0} Skin={1}", new object[]
			{
				playerOrBotsByObjectId,
				carAssetName
			});
			Identifiable component = this._container.InstantiatePrefab(this.CarTemplate, GameHubBehaviour.Hub.Drawer.Players).GetComponent<Identifiable>();
			component.IsOwner = (!GameHubBehaviour.Hub.Net.IsServer() && id == GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId);
			Transform transform = component.transform;
			PlayerCarFactory.Log.DebugFormat("Customization={0} GUID={1}", new object[]
			{
				carAssetName,
				guidBySlot.ToString()
			});
			CarComponentHub component2 = component.gameObject.GetComponent<CarComponentHub>();
			CombatObject bitComponent = component.GetBitComponent<CombatObject>();
			CarMovement bitComponent2 = component.GetBitComponent<CarMovement>();
			TurretMovement bitComponent3 = component.GetBitComponent<TurretMovement>();
			CarInput bitComponent4 = component.GetBitComponent<CarInput>();
			SpawnController bitComponent5 = component.GetBitComponent<SpawnController>();
			CombatFeedback bitComponent6 = component.GetBitComponent<CombatFeedback>();
			PlayerController component3 = component.GetComponent<PlayerController>();
			component2.Player = playerOrBotsByObjectId;
			component2.combatObject = bitComponent;
			component2.carInput = bitComponent4;
			component2.carMovement = bitComponent2;
			component2.spawnController = bitComponent5;
			component2.combatFeedback = bitComponent6;
			component2.playerController = component3;
			BoxCollider component4 = component.GetComponent<BoxCollider>();
			component4.size = playerOrBotsByObjectId.Character.Collider.Size;
			component4.center = playerOrBotsByObjectId.Character.Collider.Center;
			if (playerOrBotsByObjectId.Character.ExtraColliders != null)
			{
				for (int i = 0; i < playerOrBotsByObjectId.Character.ExtraColliders.Length; i++)
				{
					CarCollider carCollider = playerOrBotsByObjectId.Character.ExtraColliders[i];
					GameObject gameObject = new GameObject(carCollider.Name);
					Transform transform2 = gameObject.transform;
					transform2.parent = transform;
					transform2.localPosition = Vector3.zero;
					transform2.rotation = transform.rotation;
					BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
					boxCollider.center = carCollider.Center;
					boxCollider.size = carCollider.Size;
				}
			}
			component3.CarHub = component2;
			Transform prefab = (Transform)Loading.Content.GetAsset(carAssetName).Asset;
			Transform transform3 = this._container.InstantiatePrefab(prefab, transform).transform;
			CarSkin component5 = transform3.GetComponent<CarSkin>();
			if (component5)
			{
				bool isAlly = GameHubBehaviour.Hub.Net.IsTest() || (GameHubBehaviour.Hub.Net.IsClient() && GameHubBehaviour.Hub.Players.CurrentPlayerTeam == playerOrBotsByObjectId.Team);
				component5.SetSkin(carAssetName, isAlly);
			}
			transform3.transform.localPosition = Vector3.zero;
			transform3.transform.localRotation = Quaternion.identity;
			LayerManager.SetLayerRecursively(component, (playerOrBotsByObjectId.Team != TeamKind.Red) ? LayerManager.Layer.PlayerBlu : LayerManager.Layer.PlayerRed);
			component.Register(id);
			component.name = string.Format("[{1}]{0}", playerOrBotsByObjectId.GetCharacter(), playerOrBotsByObjectId.PlayerAddress);
			transform3.name = string.Format("Res[{1}]{0}", playerOrBotsByObjectId.GetCharacterAssetPrefix(), playerOrBotsByObjectId.PlayerAddress);
			bitComponent5.Renderer = transform3.transform;
			component2.combatObject.Id = component;
			CarGenerator component6 = transform3.gameObject.GetComponent<CarGenerator>();
			CDummy componentInChildren = transform3.GetComponentInChildren<CDummy>();
			component2.carGenerator = component6;
			component2.dummy = componentInChildren;
			if (component6 != null)
			{
				component6.carComponentHub = component2;
				component6.suspensionGroup.CarMovement = bitComponent2;
				component6.transform.localPosition = Vector3.zero;
				component6.SetParentCarTemplate(component2.transform);
				if (GameHubBehaviour.Hub.Net.IsClient() || GameHubBehaviour.Hub.Net.IsTest())
				{
					CarAudioController bitComponent7 = component.GetBitComponent<CarAudioController>();
					CarMovementFeedback component7 = component6.GetComponent<CarMovementFeedback>();
					component2.carMovementFeedback = component7;
					if (component7)
					{
						component7._carMovement = bitComponent2;
						component7._carAudioController = bitComponent7;
					}
					if (component.IsOwner)
					{
						bitComponent6.OnCollisionEvent += component6.OnCarCollision;
					}
					component6.carWheelsController.CarMovement = bitComponent2;
					CombatData bitComponent8 = component.GetBitComponent<CombatData>();
					component2.combatData = bitComponent8;
					if (GameHubBehaviour.Hub.Net.IsClient() || GameHubBehaviour.Hub.Net.isTest)
					{
						SurfaceEffect component8 = component.GetComponent<SurfaceEffect>();
						component2.surfaceEffect = component8;
						if (component.IsOwner)
						{
							CarIndicator carIndicator = this._container.InstantiateComponent<CarIndicator>(component.gameObject);
							component2.carIndicator = carIndicator;
							carIndicator.AddCarIndicator(transform, playerOrBotsByObjectId, GUIColorsInfo.GetColorByPlayerCarId(playerOrBotsByObjectId.PlayerCarId.GetInstanceId(), GameHubBehaviour.Hub.Options.Game.UseTeamColor));
							carIndicator.SetPlayerIndicationBorderAnimationConfig(this._playerIndicatorBorderAnimationConfig);
							this._playerCamera.SetupCurrentPlayer(transform, bitComponent8);
						}
					}
				}
				bitComponent2.Char = playerOrBotsByObjectId.Character;
				bitComponent3.TurretConfiguration = playerOrBotsByObjectId.Character.TurretMovementConfiguration;
				bitComponent.Data.SetInfo(playerOrBotsByObjectId.Character.Combat);
				bitComponent.Player = playerOrBotsByObjectId;
				bitComponent.IsPlayer = true;
				bitComponent.IsBot = this.IsBot;
				bitComponent.Dummy = componentInChildren;
				if (playerOrBotsByObjectId.Character.CustomGadget0)
				{
					bitComponent.CustomGadget0 = (GadgetBehaviour)this._container.InstantiateComponent(playerOrBotsByObjectId.Character.CustomGadget0.GadgetType(), bitComponent.Gadgets);
					bitComponent.CustomGadget0.Parent = component;
					bitComponent.CustomGadget0.Combat = bitComponent;
					bitComponent.CustomGadget0.Slot = GadgetSlot.CustomGadget0;
				}
				if (playerOrBotsByObjectId.Character.CustomGadget1)
				{
					bitComponent.CustomGadget1 = (GadgetBehaviour)this._container.InstantiateComponent(playerOrBotsByObjectId.Character.CustomGadget1.GadgetType(), bitComponent.Gadgets);
					bitComponent.CustomGadget1.Parent = component;
					bitComponent.CustomGadget1.Combat = bitComponent;
					bitComponent.CustomGadget1.Slot = GadgetSlot.CustomGadget1;
				}
				if (playerOrBotsByObjectId.Character.CustomGadget2)
				{
					bitComponent.CustomGadget2 = (GadgetBehaviour)this._container.InstantiateComponent(playerOrBotsByObjectId.Character.CustomGadget2.GadgetType(), bitComponent.Gadgets);
					bitComponent.CustomGadget2.Parent = component;
					bitComponent.CustomGadget2.Combat = bitComponent;
					bitComponent.CustomGadget2.Slot = GadgetSlot.CustomGadget2;
				}
				if (playerOrBotsByObjectId.Character.GenericGadget)
				{
					bitComponent.GenericGadget = (GadgetBehaviour)this._container.InstantiateComponent(playerOrBotsByObjectId.Character.GenericGadget.GadgetType(), bitComponent.Gadgets);
					bitComponent.GenericGadget.Parent = component;
					bitComponent.GenericGadget.Combat = bitComponent;
					bitComponent.GenericGadget.Slot = GadgetSlot.GenericGadget;
				}
				if (playerOrBotsByObjectId.Character.BoostGadget)
				{
					bitComponent.BoostGadget = (GadgetBehaviour)this._container.InstantiateComponent(playerOrBotsByObjectId.Character.BoostGadget.GadgetType(), bitComponent.Gadgets);
					bitComponent.BoostGadget.Parent = component;
					bitComponent.BoostGadget.Combat = bitComponent;
					bitComponent.BoostGadget.Slot = GadgetSlot.BoostGadget;
				}
				if (playerOrBotsByObjectId.Character.PassiveGadget)
				{
					bitComponent.PassiveGadget = (GadgetBehaviour)this._container.InstantiateComponent(playerOrBotsByObjectId.Character.PassiveGadget.GadgetType(), bitComponent.Gadgets);
					bitComponent.PassiveGadget.Parent = component;
					bitComponent.PassiveGadget.Combat = bitComponent;
					bitComponent.PassiveGadget.Slot = GadgetSlot.PassiveGadget;
				}
				if (playerOrBotsByObjectId.Character.TrailGadget)
				{
					bitComponent.TrailGadget = (GadgetBehaviour)this._container.InstantiateComponent(playerOrBotsByObjectId.Character.TrailGadget.GadgetType(), bitComponent.Gadgets);
					bitComponent.TrailGadget.Parent = component;
					bitComponent.TrailGadget.Combat = bitComponent;
					bitComponent.TrailGadget.Slot = GadgetSlot.TrailGadget;
				}
				if (playerOrBotsByObjectId.Character.OutOfCombatGadget)
				{
					bitComponent.OutOfCombatGadget = (GadgetBehaviour)this._container.InstantiateComponent(playerOrBotsByObjectId.Character.OutOfCombatGadget.GadgetType(), bitComponent.Gadgets);
					bitComponent.OutOfCombatGadget.Parent = component;
					bitComponent.OutOfCombatGadget.Combat = bitComponent;
					bitComponent.OutOfCombatGadget.Slot = GadgetSlot.OutOfCombatGadget;
				}
				if (playerOrBotsByObjectId.Character.DmgUpgrade)
				{
					bitComponent.DmgUpgrade = (GadgetBehaviour)this._container.InstantiateComponent(playerOrBotsByObjectId.Character.DmgUpgrade.GadgetType(), bitComponent.Gadgets);
					bitComponent.DmgUpgrade.Parent = component;
					bitComponent.DmgUpgrade.Combat = bitComponent;
					bitComponent.DmgUpgrade.Slot = GadgetSlot.DmgUpgrade;
				}
				if (playerOrBotsByObjectId.Character.HPUpgrade)
				{
					bitComponent.HPUpgrade = (GadgetBehaviour)this._container.InstantiateComponent(playerOrBotsByObjectId.Character.HPUpgrade.GadgetType(), bitComponent.Gadgets);
					bitComponent.HPUpgrade.Parent = component;
					bitComponent.HPUpgrade.Combat = bitComponent;
					bitComponent.HPUpgrade.Slot = GadgetSlot.HPUpgrade;
				}
				if (playerOrBotsByObjectId.Character.EPUpgrade)
				{
					bitComponent.EPUpgrade = (GadgetBehaviour)this._container.InstantiateComponent(playerOrBotsByObjectId.Character.EPUpgrade.GadgetType(), bitComponent.Gadgets);
					bitComponent.EPUpgrade.Parent = component;
					bitComponent.EPUpgrade.Combat = bitComponent;
					bitComponent.EPUpgrade.Slot = GadgetSlot.EPUpgrade;
				}
				if (playerOrBotsByObjectId.Character.BombGadget)
				{
					bitComponent.BombGadget = (BombGadget)this._container.InstantiateComponent(playerOrBotsByObjectId.Character.BombGadget.GadgetType(), bitComponent.Gadgets);
					bitComponent.BombGadget.Parent = component;
					bitComponent.BombGadget.Combat = bitComponent;
					bitComponent.BombGadget.Slot = GadgetSlot.BombGadget;
				}
				if (playerOrBotsByObjectId.Character.RespawnGadget)
				{
					bitComponent.RespawnGadget = (GadgetBehaviour)this._container.InstantiateComponent(playerOrBotsByObjectId.Character.RespawnGadget.GadgetType(), bitComponent.Gadgets);
					bitComponent.RespawnGadget.Parent = component;
					bitComponent.RespawnGadget.Combat = bitComponent;
					bitComponent.RespawnGadget.Slot = GadgetSlot.RespawnGadget;
				}
				GadgetInfo takeoffGadgetForCurrentArena = this.GetTakeoffGadgetForCurrentArena(playerOrBotsByObjectId.Character);
				if (takeoffGadgetForCurrentArena)
				{
					bitComponent.TakeOffGadget = (GadgetBehaviour)this._container.InstantiateComponent(takeoffGadgetForCurrentArena.GadgetType(), bitComponent.Gadgets);
					bitComponent.TakeOffGadget.Parent = component;
					bitComponent.TakeOffGadget.Combat = bitComponent;
					bitComponent.TakeOffGadget.Slot = GadgetSlot.TakeoffGadget;
				}
				if (playerOrBotsByObjectId.Character.KillGadget)
				{
					bitComponent.KillGadget = (GadgetBehaviour)this._container.InstantiateComponent(playerOrBotsByObjectId.Character.KillGadget.GadgetType(), bitComponent.Gadgets);
					bitComponent.KillGadget.Parent = component;
					bitComponent.KillGadget.Combat = bitComponent;
					bitComponent.KillGadget.Slot = GadgetSlot.KillGadget;
				}
				if (playerOrBotsByObjectId.Character.BombExplosionGadget)
				{
					bitComponent.BombExplosionGadget = (GadgetBehaviour)this._container.InstantiateComponent(playerOrBotsByObjectId.Character.BombExplosionGadget.GadgetType(), bitComponent.Gadgets);
					bitComponent.BombExplosionGadget.Parent = component;
					bitComponent.BombExplosionGadget.Combat = bitComponent;
					bitComponent.BombExplosionGadget.Slot = GadgetSlot.BombExplosionGadget;
				}
				if (playerOrBotsByObjectId.Character.SprayGadget)
				{
					bitComponent.SprayGadget = (GadgetBehaviour)this._container.InstantiateComponent(playerOrBotsByObjectId.Character.SprayGadget.GadgetType(), bitComponent.Gadgets);
					bitComponent.SprayGadget.Parent = component;
					bitComponent.SprayGadget.Combat = bitComponent;
					bitComponent.SprayGadget.Slot = GadgetSlot.SprayGadget;
				}
				if (playerOrBotsByObjectId.Character.GridHighlightGadget)
				{
					bitComponent.GridHighlightGadget = (GadgetBehaviour)this._container.InstantiateComponent(playerOrBotsByObjectId.Character.GridHighlightGadget.GadgetType(), bitComponent.Gadgets);
					bitComponent.GridHighlightGadget.Parent = component;
					bitComponent.GridHighlightGadget.Combat = bitComponent;
					bitComponent.GridHighlightGadget.Slot = GadgetSlot.GridHighlightGadget;
				}
				bitComponent.CustomGadgets.Clear();
				if (playerOrBotsByObjectId.Character.CustomGadgets != null)
				{
					for (int j = 0; j < playerOrBotsByObjectId.Character.CustomGadgets.Length; j++)
					{
						CombatGadget combatGadget = playerOrBotsByObjectId.Character.CustomGadgets[j];
						if (bitComponent.HasGadgetContext((int)combatGadget.Slot))
						{
							PlayerCarFactory.Log.ErrorFormat("{0} Slot is already used by another gadget in {1}", new object[]
							{
								combatGadget.Slot,
								bitComponent
							});
						}
						else
						{
							CombatGadget combatGadget2 = (CombatGadget)combatGadget.CreateGadgetContext((int)combatGadget.Slot, bitComponent, this._eventDispatcher, GameHubBehaviour.Hub.GetContext(), this._server, this._injectionResolver);
							bitComponent.AddGadget(combatGadget.Slot, combatGadget2);
						}
					}
					for (int k = 0; k < playerOrBotsByObjectId.Character.CustomGadgets.Length; k++)
					{
						CombatGadget combatGadget3 = playerOrBotsByObjectId.Character.CustomGadgets[k];
						((CombatGadget)bitComponent.GetGadgetContext((int)combatGadget3.Slot)).RouteParametersGadgets();
					}
				}
				IHMMGadgetContext combatGadget4 = ScriptableObject.CreateInstance<EffectBehaviour>().CreateGadgetContext(25, bitComponent, this._eventDispatcher, GameHubBehaviour.Hub.GetContext(), this._server, this._injectionResolver);
				bitComponent.AddGadget(GadgetSlot.EffectBehaviourGadget, combatGadget4);
				if (bitComponent.CustomGadget0)
				{
					bitComponent.CustomGadget0.SetInfo(playerOrBotsByObjectId.Character.CustomGadget0);
				}
				if (bitComponent.CustomGadget1)
				{
					bitComponent.CustomGadget1.SetInfo(playerOrBotsByObjectId.Character.CustomGadget1);
				}
				if (bitComponent.CustomGadget2)
				{
					bitComponent.CustomGadget2.SetInfo(playerOrBotsByObjectId.Character.CustomGadget2);
				}
				if (bitComponent.GenericGadget)
				{
					bitComponent.GenericGadget.SetInfo(playerOrBotsByObjectId.Character.GenericGadget);
				}
				if (bitComponent.BoostGadget)
				{
					bitComponent.BoostGadget.SetInfo(playerOrBotsByObjectId.Character.BoostGadget);
				}
				if (bitComponent.PassiveGadget)
				{
					bitComponent.PassiveGadget.SetInfo(playerOrBotsByObjectId.Character.PassiveGadget);
				}
				if (bitComponent.TrailGadget)
				{
					bitComponent.TrailGadget.SetInfo(playerOrBotsByObjectId.Character.TrailGadget);
				}
				if (bitComponent.OutOfCombatGadget)
				{
					bitComponent.OutOfCombatGadget.SetInfo(playerOrBotsByObjectId.Character.OutOfCombatGadget);
				}
				if (bitComponent.DmgUpgrade)
				{
					bitComponent.DmgUpgrade.SetInfo(playerOrBotsByObjectId.Character.DmgUpgrade);
				}
				if (bitComponent.HPUpgrade)
				{
					bitComponent.HPUpgrade.SetInfo(playerOrBotsByObjectId.Character.HPUpgrade);
				}
				if (bitComponent.EPUpgrade)
				{
					bitComponent.EPUpgrade.SetInfo(playerOrBotsByObjectId.Character.EPUpgrade);
				}
				if (bitComponent.BombGadget)
				{
					bitComponent.BombGadget.SetInfo(playerOrBotsByObjectId.Character.BombGadget);
				}
				if (bitComponent.RespawnGadget)
				{
					bitComponent.RespawnGadget.SetInfo(playerOrBotsByObjectId.Character.RespawnGadget);
				}
				if (bitComponent.TakeOffGadget)
				{
					bitComponent.TakeOffGadget.SetInfo(takeoffGadgetForCurrentArena);
				}
				if (bitComponent.KillGadget)
				{
					bitComponent.KillGadget.SetInfo(playerOrBotsByObjectId.Character.KillGadget);
				}
				if (bitComponent.BombExplosionGadget)
				{
					bitComponent.BombExplosionGadget.SetInfo(playerOrBotsByObjectId.Character.BombExplosionGadget);
				}
				if (bitComponent.SprayGadget)
				{
					bitComponent.SprayGadget.SetInfo(playerOrBotsByObjectId.Character.SprayGadget);
				}
				if (bitComponent.GridHighlightGadget)
				{
					bitComponent.GridHighlightGadget.SetInfo(playerOrBotsByObjectId.Character.GridHighlightGadget);
				}
				if (GameHubBehaviour.Hub.Net.IsServer())
				{
					component2.AIAgent = this._agentFactory.CreateAIAgent(component.gameObject);
					if (playerOrBotsByObjectId.IsBot || GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.AutoTest))
					{
						component3.EnableBotControllerComponents(true);
					}
					else
					{
						component3.EnableBotControllerComponents(false);
						component3.ServerPlayerCarFactoryInit();
					}
					component2.AIAgent.GoalManager.enabled = true;
				}
				if (GameHubBehaviour.Hub.Net.IsClient())
				{
					component2.RenderTransform = transform3.transform;
					component2.ArtReference = transform3.GetComponent<ArtReference>();
					component2.carAudioController = component.GetBitComponent<CarAudioController>();
					component2.carAudioController.Initialize(component2);
					component2.VoiceOverController = component.GetBitComponent<VoiceOverController>();
					component2.VoiceOverController.Initialize(component2);
					if (component.IsOwner)
					{
						GameHubBehaviour.Hub.Input.CurrentController = component3;
						bitComponent4.SetAngle(this._gameCameraInversion.ScreenSpaceAngle);
					}
					component6.CreateCarAnimation(bitComponent);
				}
				else if (GameHubBehaviour.Hub.Net.IsTest())
				{
					component2.RenderTransform = transform3.transform;
					component2.carAudioController = component.GetBitComponent<CarAudioController>();
					component2.carAudioController.Initialize(component2);
					component2.VoiceOverController = component.GetBitComponent<VoiceOverController>();
					component2.VoiceOverController.Initialize(component2);
					component6.CreateCarAnimation(bitComponent);
				}
			}
			result.Result = component;
			PlayerCarFactory.DisablePlayerCarRenderingIfNeeded(transform3.gameObject);
		}

		private static void DisablePlayerCarRenderingIfNeeded(GameObject playerCar)
		{
			int intValue = GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.DisableCarRendering);
			if (intValue == 0 || GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			new PlayerCarRenderingToggler(playerCar)
			{
				ForceDestroyParticles = true
			}.DisableRendering(PlayerCarRenderingToggler.Mode.All);
		}

		private static string GetCarAssetName(Guid charItemTypeId, Guid customizationSkin)
		{
			ItemTypeScriptableObject skinItemTypeScriptableObjectByGuid = GameHubBehaviour.Hub.InventoryColletion.GetSkinItemTypeScriptableObjectByGuid(charItemTypeId, customizationSkin);
			SkinPrefabItemTypeComponent component = skinItemTypeScriptableObjectByGuid.GetComponent<SkinPrefabItemTypeComponent>();
			return component.SkinPrefabName;
		}

		public static Future CarPreCache(Guid charItemTypeId, Guid customizationSkin, bool isCurrentPlayer)
		{
			string carAssetName = PlayerCarFactory.GetCarAssetName(charItemTypeId, customizationSkin);
			GameHubBehaviour.Hub.Resources.PreCachePrefab(carAssetName, 1);
			PlayerCarFactory._instance._characterInfoFuture = new Future();
			string characterInfoName = PlayerCarFactory.GetCharacterInfoName(charItemTypeId);
			Loading.GenericAssetManager.GetAssetAsync(characterInfoName, PlayerCarFactory._instance);
			if (isCurrentPlayer)
			{
				CarIndicator.PrecacheCarIndicator();
			}
			return PlayerCarFactory._instance._characterInfoFuture;
		}

		private static string GetCharacterInfoName(Guid charItemTypeId)
		{
			ItemTypeScriptableObject itemTypeScriptableObject = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[charItemTypeId];
			CharacterItemTypeComponent component = itemTypeScriptableObject.GetComponent<CharacterItemTypeComponent>();
			return string.Format("{0}_MainAttributes", component.AssetPrefix);
		}

		public void OnAssetLoaded(string name, Object asset)
		{
			this._characterInfoFuture.Result = name;
			CharacterInfo characterInfo = (CharacterInfo)asset;
			this.PreCacheCharacterInfoGadgets(characterInfo);
			if (!PlayerCarFactory._characterInfos.ContainsKey(characterInfo.CharacterId))
			{
				PlayerCarFactory._characterInfos[characterInfo.CharacterId] = characterInfo;
			}
		}

		private GadgetInfo GetTakeoffGadgetForCurrentArena(CharacterInfo characterInfo)
		{
			int arenaIndex = GameHubBehaviour.Hub.Match.ArenaIndex;
			if (arenaIndex < 0 || characterInfo.TakeoffGadgets == null || arenaIndex >= characterInfo.TakeoffGadgets.Length)
			{
				return null;
			}
			return characterInfo.TakeoffGadgets[arenaIndex];
		}

		private void PreCacheCharacterInfoGadgets(CharacterInfo characterInfo)
		{
			this.PreCacheGadget(characterInfo.CustomGadget0);
			this.PreCacheGadget(characterInfo.CustomGadget1);
			this.PreCacheGadget(characterInfo.CustomGadget2);
			this.PreCacheGadget(characterInfo.BoostGadget);
			this.PreCacheGadget(characterInfo.GenericGadget);
			this.PreCacheGadget(characterInfo.PassiveGadget);
			this.PreCacheGadget(characterInfo.TrailGadget);
			this.PreCacheGadget(characterInfo.OutOfCombatGadget);
			this.PreCacheGadget(characterInfo.DmgUpgrade);
			this.PreCacheGadget(characterInfo.HPUpgrade);
			this.PreCacheGadget(characterInfo.EPUpgrade);
			this.PreCacheGadget(characterInfo.BombGadget);
			this.PreCacheGadget(characterInfo.RespawnGadget);
			this.PreCacheGadget(characterInfo.KillGadget);
			this.PreCacheGadget(characterInfo.BombExplosionGadget);
			this.PreCacheGadget(characterInfo.SprayGadget);
			this.PreCacheGadget(characterInfo.GridHighlightGadget);
			this.PreCacheGadget(this.GetTakeoffGadgetForCurrentArena(characterInfo));
			for (int i = 0; i < characterInfo.CustomGadgets.Length; i++)
			{
				this.PreCacheGadget(characterInfo.CustomGadgets[i]);
			}
		}

		private void PreCacheGadget(GadgetInfo gadgetInfo)
		{
			if (gadgetInfo)
			{
				gadgetInfo.PreCacheAssets();
			}
		}

		private void PreCacheGadget(CombatGadget combatGadget)
		{
			if (combatGadget)
			{
				combatGadget.PrecacheAssets(GameHubBehaviour.Hub.GetContext());
			}
		}

		public static void ClearCharacterInfoDictionary()
		{
			PlayerCarFactory._characterInfos.Clear();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PlayerCarFactory));

		public Identifiable CarTemplate;

		[Inject]
		private DiContainer _container;

		[Inject]
		private IInjectionResolver _injectionResolver;

		[Inject]
		private IGadgetEventDispatcher _eventDispatcher;

		[Inject]
		private IServerPlaybackDispatcher _server;

		[InjectOnServer]
		private IAIAgentFactory _agentFactory;

		[InjectOnClient]
		private IGameCamera _gameCamera;

		[InjectOnClient]
		private IGameCameraInversion _gameCameraInversion;

		[InjectOnClient]
		private IGameCameraPlayerSetup _playerCamera;

		[SerializeField]
		private PlayerIndicatorBorderAnimationConfig _playerIndicatorBorderAnimationConfig;

		protected bool IsBot;

		private static PlayerCarFactory _instance;

		private static Dictionary<int, CharacterInfo> _characterInfos;

		private Future _characterInfoFuture;
	}
}
