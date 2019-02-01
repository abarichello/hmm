using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Audio;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.GadgetScript;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Pipeline;
using HeavyMetalMachines.Render;
using HeavyMetalMachines.VFX;
using Pocketverse;
using SharedUtils.Loading;
using UnityEngine;

namespace HeavyMetalMachines.Character
{
	public class PlayerCarFactory : GameHubBehaviour
	{
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
			if (playerOrBotsByObjectId == null || !playerOrBotsByObjectId.Character)
			{
				PlayerCarFactory.Log.ErrorFormat("Player={0} not found or with null character! {1} is null", new object[]
				{
					id.GetInstanceId(),
					(!(playerOrBotsByObjectId == null)) ? "character" : "Player"
				});
				result.Cancel();
				return;
			}
			string carAssetName = PlayerCarFactory.GetCarAssetName(playerOrBotsByObjectId.Character, playerOrBotsByObjectId.Customizations.SelectedSkin);
			Identifiable identifiable = UnityEngine.Object.Instantiate<Identifiable>(this.CarTemplate, GameHubBehaviour.Hub.Drawer.Players);
			identifiable.IsOwner = (!GameHubBehaviour.Hub.Net.IsServer() && id == GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId);
			Transform transform = identifiable.transform;
			CarComponentHub component = identifiable.gameObject.GetComponent<CarComponentHub>();
			CombatObject bitComponent = identifiable.GetBitComponent<CombatObject>();
			CarMovement bitComponent2 = identifiable.GetBitComponent<CarMovement>();
			CarInput bitComponent3 = identifiable.GetBitComponent<CarInput>();
			SpawnController bitComponent4 = identifiable.GetBitComponent<SpawnController>();
			BotAIGoalManager bitComponent5 = identifiable.GetBitComponent<BotAIGoalManager>();
			bitComponent5.Goals = PlayerCarFactory.GetAIGoals(playerOrBotsByObjectId);
			BotAIPathFind component2 = identifiable.GetComponent<BotAIPathFind>();
			BotAIGadgetShop component3 = identifiable.GetComponent<BotAIGadgetShop>();
			CombatFeedback bitComponent6 = identifiable.GetBitComponent<CombatFeedback>();
			PlayerController component4 = identifiable.GetComponent<PlayerController>();
			component.Player = playerOrBotsByObjectId;
			component.combatObject = bitComponent;
			component.carInput = bitComponent3;
			component.carMovement = bitComponent2;
			component.spawnController = bitComponent4;
			component.botAIGoalManager = bitComponent5;
			component.botAIPathFind = component2;
			component.botAIGadgetShop = component3;
			component.combatFeedback = bitComponent6;
			component.playerController = component4;
			BoxCollider component5 = identifiable.GetComponent<BoxCollider>();
			component5.size = playerOrBotsByObjectId.Character.Collider.Size;
			component5.center = playerOrBotsByObjectId.Character.Collider.Center;
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
			component4.CarHub = component;
			Transform original = (Transform)LoadingManager.ResourceContent.GetAsset(carAssetName).Asset;
			Transform transform3 = UnityEngine.Object.Instantiate<Transform>(original, transform);
			CarSkin component6 = transform3.GetComponent<CarSkin>();
			if (component6)
			{
				bool isAlly = GameHubBehaviour.Hub.Net.IsTest() || (GameHubBehaviour.Hub.Net.IsClient() && GameHubBehaviour.Hub.Players.CurrentPlayerTeam == playerOrBotsByObjectId.Team);
				component6.SetSkin(carAssetName, isAlly);
			}
			transform3.transform.localPosition = Vector3.zero;
			transform3.transform.localRotation = Quaternion.identity;
			LayerManager.SetLayerRecursively(identifiable, (playerOrBotsByObjectId.Team != TeamKind.Red) ? LayerManager.Layer.PlayerBlu : LayerManager.Layer.PlayerRed);
			identifiable.Register(id);
			identifiable.name = string.Format("[{1}]{0}", playerOrBotsByObjectId.Character.Character, playerOrBotsByObjectId.PlayerAddress);
			transform3.name = string.Format("Res[{1}]{0}", playerOrBotsByObjectId.Character.Asset, playerOrBotsByObjectId.PlayerAddress);
			bitComponent4.Renderer = transform3.transform;
			component.combatObject.Id = identifiable;
			CarGenerator component7 = transform3.gameObject.GetComponent<CarGenerator>();
			CDummy componentInChildren = transform3.GetComponentInChildren<CDummy>();
			component.carGenerator = component7;
			component.dummy = componentInChildren;
			if (component7 != null)
			{
				component7.carComponentHub = component;
				component7.suspensionGroup.CarMovement = bitComponent2;
				component7.transform.localPosition = Vector3.zero;
				bitComponent6.OnCollisionEvent += component7.OnCarCollision;
				component7.SetParentCarTemplate(component.transform);
				if (GameHubBehaviour.Hub.Net.IsClient() || GameHubBehaviour.Hub.Net.IsTest())
				{
					CarAudioController bitComponent7 = identifiable.GetBitComponent<CarAudioController>();
					CarMovementFeedback component8 = component7.GetComponent<CarMovementFeedback>();
					component.carMovementFeedback = component8;
					if (component8)
					{
						component8._carMovement = bitComponent2;
						component8._carAudioController = bitComponent7;
					}
					if (identifiable.IsOwner)
					{
						component7.ShakeCameraOnCollision = true;
					}
					component7.carWheelsController.CarMovement = bitComponent2;
					CombatData bitComponent8 = identifiable.GetBitComponent<CombatData>();
					component.combatData = bitComponent8;
					if (GameHubBehaviour.Hub.Net.IsClient() || GameHubBehaviour.Hub.Net.isTest)
					{
						SurfaceEffect component9 = identifiable.GetComponent<SurfaceEffect>();
						component.surfaceEffect = component9;
						if (identifiable.IsOwner)
						{
							CarIndicator carIndicator = identifiable.gameObject.AddComponent<CarIndicator>();
							component.carIndicator = carIndicator;
							carIndicator.AddCarIndicator(transform, playerOrBotsByObjectId, GUIColorsInfo.GetColorByPlayerCarId(playerOrBotsByObjectId.PlayerCarId.GetInstanceId(), GameHubBehaviour.Hub.Options.Game.UseTeamColor));
							carIndicator.SetPlayerIndicationBorderAnimationConfig(this._playerIndicatorBorderAnimationConfig);
							CarCamera singleton = CarCamera.Singleton;
							singleton.MyPlayerTransform = transform;
							singleton.SetTarget("PlayerSnap", () => true, singleton.MyPlayerTransform, true, true, true);
							singleton.MyPlayerCombatData = bitComponent8;
						}
					}
				}
				bitComponent2.Char = playerOrBotsByObjectId.Character;
				bitComponent.Data.SetInfo(playerOrBotsByObjectId.Character.Combat);
				bitComponent.Player = playerOrBotsByObjectId;
				bitComponent.IsPlayer = true;
				bitComponent.IsBot = this.IsBot;
				bitComponent.Dummy = componentInChildren;
				if (playerOrBotsByObjectId.Character.CustomGadget0)
				{
					bitComponent.CustomGadget0 = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(playerOrBotsByObjectId.Character.CustomGadget0.GadgetType());
					bitComponent.CustomGadget0.Parent = identifiable;
					bitComponent.CustomGadget0.Combat = bitComponent;
					bitComponent.CustomGadget0.Slot = GadgetSlot.CustomGadget0;
				}
				if (playerOrBotsByObjectId.Character.CustomGadget1)
				{
					bitComponent.CustomGadget1 = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(playerOrBotsByObjectId.Character.CustomGadget1.GadgetType());
					bitComponent.CustomGadget1.Parent = identifiable;
					bitComponent.CustomGadget1.Combat = bitComponent;
					bitComponent.CustomGadget1.Slot = GadgetSlot.CustomGadget1;
				}
				if (playerOrBotsByObjectId.Character.CustomGadget2)
				{
					bitComponent.CustomGadget2 = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(playerOrBotsByObjectId.Character.CustomGadget2.GadgetType());
					bitComponent.CustomGadget2.Parent = identifiable;
					bitComponent.CustomGadget2.Combat = bitComponent;
					bitComponent.CustomGadget2.Slot = GadgetSlot.CustomGadget2;
				}
				if (playerOrBotsByObjectId.Character.GenericGadget)
				{
					bitComponent.GenericGadget = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(playerOrBotsByObjectId.Character.GenericGadget.GadgetType());
					bitComponent.GenericGadget.Parent = identifiable;
					bitComponent.GenericGadget.Combat = bitComponent;
					bitComponent.GenericGadget.Slot = GadgetSlot.GenericGadget;
				}
				if (playerOrBotsByObjectId.Character.BoostGadget)
				{
					bitComponent.BoostGadget = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(playerOrBotsByObjectId.Character.BoostGadget.GadgetType());
					bitComponent.BoostGadget.Parent = identifiable;
					bitComponent.BoostGadget.Combat = bitComponent;
					bitComponent.BoostGadget.Slot = GadgetSlot.BoostGadget;
				}
				if (playerOrBotsByObjectId.Character.PassiveGadget)
				{
					bitComponent.PassiveGadget = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(playerOrBotsByObjectId.Character.PassiveGadget.GadgetType());
					bitComponent.PassiveGadget.Parent = identifiable;
					bitComponent.PassiveGadget.Combat = bitComponent;
					bitComponent.PassiveGadget.Slot = GadgetSlot.PassiveGadget;
				}
				if (playerOrBotsByObjectId.Character.TrailGadget)
				{
					bitComponent.TrailGadget = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(playerOrBotsByObjectId.Character.TrailGadget.GadgetType());
					bitComponent.TrailGadget.Parent = identifiable;
					bitComponent.TrailGadget.Combat = bitComponent;
					bitComponent.TrailGadget.Slot = GadgetSlot.TrailGadget;
				}
				if (playerOrBotsByObjectId.Character.OutOfCombatGadget)
				{
					bitComponent.OutOfCombatGadget = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(playerOrBotsByObjectId.Character.OutOfCombatGadget.GadgetType());
					bitComponent.OutOfCombatGadget.Parent = identifiable;
					bitComponent.OutOfCombatGadget.Combat = bitComponent;
					bitComponent.OutOfCombatGadget.Slot = GadgetSlot.OutOfCombatGadget;
				}
				if (playerOrBotsByObjectId.Character.DmgUpgrade)
				{
					bitComponent.DmgUpgrade = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(playerOrBotsByObjectId.Character.DmgUpgrade.GadgetType());
					bitComponent.DmgUpgrade.Parent = identifiable;
					bitComponent.DmgUpgrade.Combat = bitComponent;
					bitComponent.DmgUpgrade.Slot = GadgetSlot.DmgUpgrade;
				}
				if (playerOrBotsByObjectId.Character.HPUpgrade)
				{
					bitComponent.HPUpgrade = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(playerOrBotsByObjectId.Character.HPUpgrade.GadgetType());
					bitComponent.HPUpgrade.Parent = identifiable;
					bitComponent.HPUpgrade.Combat = bitComponent;
					bitComponent.HPUpgrade.Slot = GadgetSlot.HPUpgrade;
				}
				if (playerOrBotsByObjectId.Character.EPUpgrade)
				{
					bitComponent.EPUpgrade = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(playerOrBotsByObjectId.Character.EPUpgrade.GadgetType());
					bitComponent.EPUpgrade.Parent = identifiable;
					bitComponent.EPUpgrade.Combat = bitComponent;
					bitComponent.EPUpgrade.Slot = GadgetSlot.EPUpgrade;
				}
				if (playerOrBotsByObjectId.Character.BombGadget)
				{
					bitComponent.BombGadget = (BombGadget)bitComponent.Gadgets.AddComponent(playerOrBotsByObjectId.Character.BombGadget.GadgetType());
					bitComponent.BombGadget.Parent = identifiable;
					bitComponent.BombGadget.Combat = bitComponent;
					bitComponent.BombGadget.Slot = GadgetSlot.BombGadget;
				}
				if (playerOrBotsByObjectId.Character.RespawnGadget)
				{
					bitComponent.RespawnGadget = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(playerOrBotsByObjectId.Character.RespawnGadget.GadgetType());
					bitComponent.RespawnGadget.Parent = identifiable;
					bitComponent.RespawnGadget.Combat = bitComponent;
					bitComponent.RespawnGadget.Slot = GadgetSlot.RespawnGadget;
				}
				GadgetInfo currentTakeoffGadget = playerOrBotsByObjectId.Character.GetCurrentTakeoffGadget(GameHubBehaviour.Hub.Match.ArenaIndex);
				if (currentTakeoffGadget)
				{
					bitComponent.TakeOffGadget = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(currentTakeoffGadget.GadgetType());
					bitComponent.TakeOffGadget.Parent = identifiable;
					bitComponent.TakeOffGadget.Combat = bitComponent;
					bitComponent.TakeOffGadget.Slot = GadgetSlot.TakeoffGadget;
				}
				if (playerOrBotsByObjectId.Character.KillGadget)
				{
					bitComponent.KillGadget = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(playerOrBotsByObjectId.Character.KillGadget.GadgetType());
					bitComponent.KillGadget.Parent = identifiable;
					bitComponent.KillGadget.Combat = bitComponent;
					bitComponent.KillGadget.Slot = GadgetSlot.KillGadget;
				}
				if (playerOrBotsByObjectId.Character.BombExplosionGadget)
				{
					bitComponent.BombExplosionGadget = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(playerOrBotsByObjectId.Character.BombExplosionGadget.GadgetType());
					bitComponent.BombExplosionGadget.Parent = identifiable;
					bitComponent.BombExplosionGadget.Combat = bitComponent;
					bitComponent.BombExplosionGadget.Slot = GadgetSlot.BombExplosionGadget;
				}
				if (playerOrBotsByObjectId.Character.SprayGadget)
				{
					bitComponent.SprayGadget = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(playerOrBotsByObjectId.Character.SprayGadget.GadgetType());
					bitComponent.SprayGadget.Parent = identifiable;
					bitComponent.SprayGadget.Combat = bitComponent;
					bitComponent.SprayGadget.Slot = GadgetSlot.SprayGadget;
				}
				if (playerOrBotsByObjectId.Character.GridHighlightGadget)
				{
					bitComponent.GridHighlightGadget = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(playerOrBotsByObjectId.Character.GridHighlightGadget.GadgetType());
					bitComponent.GridHighlightGadget.Parent = identifiable;
					bitComponent.GridHighlightGadget.Combat = bitComponent;
					bitComponent.GridHighlightGadget.Slot = GadgetSlot.GridHighlightGadget;
				}
				bitComponent.CustomGadgets.Clear();
				if (playerOrBotsByObjectId.Character.CustomGadgets != null)
				{
					for (int j = 0; j < playerOrBotsByObjectId.Character.CustomGadgets.Length; j++)
					{
						CombatGadget combatGadget = playerOrBotsByObjectId.Character.CustomGadgets[j];
						if (bitComponent.CustomGadgets.ContainsKey(combatGadget.Slot))
						{
							PlayerCarFactory.Log.ErrorFormat("{0} Slot is already used by another gadget in {1}", new object[]
							{
								combatGadget.Slot,
								bitComponent
							});
						}
						else
						{
							CombatGadget value = (CombatGadget)combatGadget.CreateGadgetContext((int)combatGadget.Slot, bitComponent.Id, (GadgetEventParser)PlaybackManager.GetFrameParser(KeyFrameType.GadgetEvent), GameHubBehaviour.Hub.GetContext());
							bitComponent.CustomGadgets.Add(combatGadget.Slot, value);
						}
					}
					for (int k = 0; k < playerOrBotsByObjectId.Character.CustomGadgets.Length; k++)
					{
						CombatGadget combatGadget2 = playerOrBotsByObjectId.Character.CustomGadgets[k];
						bitComponent.CustomGadgets[combatGadget2.Slot].RouteParametersGadgets();
					}
				}
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
					bitComponent.TakeOffGadget.SetInfo(currentTakeoffGadget);
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
					bitComponent5.BotAIController = bitComponent5.gameObject.AddComponent<BotAIController>();
					bitComponent5.BotAIController.goalManager = bitComponent5;
					bitComponent5.BotAIController.Directives = bitComponent5;
					if (playerOrBotsByObjectId.IsBot || GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.AutoTest))
					{
						component4.EnableBotControllerComponents(true);
						component.botAIGadgetShop.InitValues(component);
					}
					else
					{
						component4.EnableBotControllerComponents(false);
						component4.ServerPlayerCarFactoryInit();
					}
					bitComponent5.enabled = true;
				}
				if (GameHubBehaviour.Hub.Net.IsClient())
				{
					component.RenderTransform = transform3.transform;
					component.ArtReference = transform3.GetComponent<ArtReference>();
					component.carAudioController = identifiable.GetBitComponent<CarAudioController>();
					component.carAudioController.Initialize(component);
					component.VoiceOverController = identifiable.GetBitComponent<VoiceOverController>();
					component.VoiceOverController.Initialize(component);
					if (identifiable.IsOwner)
					{
						GameHubBehaviour.Hub.Input.CurrentController = component4;
						CarCamera singleton2 = CarCamera.Singleton;
						float angle = -90f - singleton2.CameraInversionAngleY;
						bitComponent3.SetAngle(angle);
						component4.JoyRange = Mathf.Max(component4.JoyRange, bitComponent.CustomGadget0.GetRange());
						component4.JoyRange = Mathf.Max(component4.JoyRange, bitComponent.CustomGadget1.GetRange());
						component4.JoyRange = Mathf.Max(component4.JoyRange, bitComponent.CustomGadget2.GetRange());
						if (component4.JoyRange > component4.JoyMaxRange)
						{
							component4.JoyRange = component4.JoyMaxRange;
						}
					}
					component7.CreateCarAnimation(bitComponent);
				}
				else if (GameHubBehaviour.Hub.Net.IsTest())
				{
					component.RenderTransform = transform3.transform;
					component.carAudioController = identifiable.GetBitComponent<CarAudioController>();
					component.carAudioController.Initialize(component);
					component.VoiceOverController = identifiable.GetBitComponent<VoiceOverController>();
					component.VoiceOverController.Initialize(component);
				}
			}
			result.Result = identifiable;
		}

		public static string GetCarAssetName(CharacterInfo character, Guid customizationSkin)
		{
			ItemTypeScriptableObject skinItemTypeScriptableObjectByGuid = GameHubBehaviour.Hub.InventoryColletion.GetSkinItemTypeScriptableObjectByGuid(character, customizationSkin);
			SkinPrefabItemTypeComponent component = skinItemTypeScriptableObjectByGuid.GetComponent<SkinPrefabItemTypeComponent>();
			return component.SkinPrefabName;
		}

		public static void CarPreCache(CharacterInfo characterInfo, Guid customizationSkin, bool isCurrentPlayer)
		{
			string carAssetName = PlayerCarFactory.GetCarAssetName(characterInfo, customizationSkin);
			GameHubBehaviour.Hub.Resources.PreCachePrefab(carAssetName, 1);
			characterInfo.PrecacheGadgets(GameHubBehaviour.Hub.Match.ArenaIndex);
			if (isCurrentPlayer)
			{
				CarIndicator.PrecacheCarIndicator();
			}
		}

		private static BotAIGoal GetAIGoals(PlayerData player)
		{
			if (player.Character.GoalEasy == null || player.Character.GoalHard == null || player.Character.GoalMedium == null)
			{
				PlayerCarFactory.Log.ErrorFormat("Character={0} still not configured for new AI goals", new object[]
				{
					player.Character.BIName
				});
				return UnityEngine.Object.Instantiate<BotAIGoal>(player.Character.GoalList);
			}
			switch (GameHubBehaviour.Hub.Players.GetBotDifficulty(player.Team))
			{
			case BotAIGoal.BotDifficulty.Easy:
				return UnityEngine.Object.Instantiate<BotAIGoal>(player.Character.GoalEasy);
			case BotAIGoal.BotDifficulty.Medium:
				return UnityEngine.Object.Instantiate<BotAIGoal>(player.Character.GoalMedium);
			case BotAIGoal.BotDifficulty.Hard:
				return UnityEngine.Object.Instantiate<BotAIGoal>(player.Character.GoalHard);
			default:
				PlayerCarFactory.Log.ErrorFormat("Team={0} Character={1} with invalid difficulty", new object[]
				{
					player.Team,
					player.Character.BIName
				});
				return UnityEngine.Object.Instantiate<BotAIGoal>(player.Character.GoalList);
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PlayerCarFactory));

		public Identifiable CarTemplate;

		[SerializeField]
		private PlayerIndicatorBorderAnimationConfig _playerIndicatorBorderAnimationConfig;

		protected bool IsBot;
	}
}
