using System;
using System.Collections.Generic;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Scene;
using HeavyMetalMachines.VFX;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;
using UnityEngine.AI;

namespace HeavyMetalMachines.Character
{
	public class CreepFactory : GameHubBehaviour, ICleanupListener
	{
		private GameGui GameGui
		{
			get
			{
				GameGui result;
				if ((result = this._gameGui) == null)
				{
					result = (this._gameGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<GameGui>());
				}
				return result;
			}
		}

		private void Start()
		{
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.ListenToStateChanged;
		}

		private void OnDestroy()
		{
			this._gameGui = null;
			GameHubBehaviour.Hub.State.ListenToStateChanged -= this.ListenToStateChanged;
		}

		private void ListenToStateChanged(GameState pChangedstate)
		{
			this._gameGui = null;
		}

		private string ChooseAsset(CreepEvent data, CreepInfo info)
		{
			if (info.RandomAssets != null && info.RandomAssets.Length > 0)
			{
				return info.RandomAssets[UnityEngine.Random.Range(0, info.RandomAssets.Length)];
			}
			int round = GameHubBehaviour.Hub.BombManager.Round;
			for (int i = 0; i < info.AssetsByRound.Length; i++)
			{
				CreepInfo.AssetByRound assetByRound = info.AssetsByRound[i];
				int num = Array.IndexOf<int>(assetByRound.Rounds, round);
				if (num >= 0)
				{
					return assetByRound.Asset;
				}
			}
			if (info.BotCharacterInfo != null)
			{
				return info.BotCharacterInfo.Asset;
			}
			TeamKind creepTeam = data.CreepTeam;
			string result;
			if (creepTeam != TeamKind.Red)
			{
				if (creepTeam != TeamKind.Blue)
				{
					result = info.Asset;
				}
				else
				{
					result = ((!string.IsNullOrEmpty(info.AssetBlu)) ? info.AssetBlu : info.Asset);
				}
			}
			else
			{
				result = ((!string.IsNullOrEmpty(info.AssetRed)) ? info.AssetRed : info.Asset);
			}
			return result;
		}

		public Identifiable CreateCreep(CreepEvent data, int creepId)
		{
			CreepInfo creepInfo = CreepInfo.Creeps[data.CreepInfoId];
			bool flag = creepInfo.BotCharacterInfo != null;
			Identifiable template = this.GetTemplate(creepInfo.TemplateKind);
			Identifiable identifiable = (Identifiable)GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(template, data.Location, Quaternion.LookRotation(data.Direction, Vector3.up));
			CarComponentHub carComponentHub = identifiable.gameObject.AddComponent<CarComponentHub>();
			CombatObject bitComponent = identifiable.GetBitComponent<CombatObject>();
			CarMovement bitComponent2 = identifiable.GetBitComponent<CarMovement>();
			CarInput bitComponent3 = identifiable.GetBitComponent<CarInput>();
			SpawnController bitComponent4 = identifiable.GetBitComponent<SpawnController>();
			BotAIGoalManager bitComponent5 = identifiable.GetBitComponent<BotAIGoalManager>();
			BotAIPathFind component = identifiable.GetComponent<BotAIPathFind>();
			BotAIGadgetShop component2 = identifiable.GetComponent<BotAIGadgetShop>();
			CombatFeedback bitComponent6 = identifiable.GetBitComponent<CombatFeedback>();
			CreepController component3 = identifiable.GetComponent<CreepController>();
			carComponentHub.Player = null;
			carComponentHub.combatObject = bitComponent;
			carComponentHub.carInput = bitComponent3;
			carComponentHub.carMovement = bitComponent2;
			carComponentHub.spawnController = bitComponent4;
			carComponentHub.botAIGoalManager = bitComponent5;
			carComponentHub.botAIPathFind = component;
			carComponentHub.botAIGadgetShop = component2;
			carComponentHub.combatFeedback = bitComponent6;
			if (!flag)
			{
				component3.Agent.enabled = true;
			}
			if (component3)
			{
				component3.Creep = creepInfo;
				component3.SpawnEvent = data;
			}
			bitComponent4.RenderObjectAsset = this.ChooseAsset(data, creepInfo);
			string text = bitComponent4.RenderObjectAsset;
			bitComponent4.VisualGadgetsAsset = text;
			if (flag)
			{
				text += "_prefab";
			}
			bitComponent4.Renderer = (Transform)GameHubBehaviour.Hub.Resources.CacheInstantiate(text, typeof(Transform), Vector3.zero, Quaternion.identity);
			if (bitComponent4.RenderObjectAsset.EndsWith("_00") || bitComponent4.RenderObjectAsset.EndsWith("_01") || bitComponent4.RenderObjectAsset.EndsWith("_02") || bitComponent4.RenderObjectAsset.EndsWith("_03") || bitComponent4.RenderObjectAsset.EndsWith("_04") || bitComponent4.RenderObjectAsset.EndsWith("_05") || bitComponent4.RenderObjectAsset.EndsWith("_06"))
			{
				bitComponent4.VisualGadgetsAsset = bitComponent4.RenderObjectAsset.Substring(0, bitComponent4.RenderObjectAsset.Length - 3);
			}
			SpawnController spawnController = bitComponent4;
			spawnController.VisualGadgetsAsset += "_visual_gadgets";
			VisualGadgetGroup visualGadgetGroup = null;
			CreepBotController component4 = identifiable.GetComponent<CreepBotController>();
			if (flag)
			{
				bitComponent4.RendererVisualGadgets = (Transform)GameHubBehaviour.Hub.Resources.CacheInstantiate(bitComponent4.VisualGadgetsAsset, typeof(Transform), Vector3.zero, Quaternion.identity);
				bitComponent4.RendererVisualGadgets.parent = GameHubBehaviour.Hub.Drawer.Gadgets;
				visualGadgetGroup = bitComponent4.RendererVisualGadgets.GetComponent<VisualGadgetGroup>();
				Identifiable @object = GameHubBehaviour.Hub.ObjectCollection.GetObject(data.CauserId);
				if (@object)
				{
					GeneratorCreepSpawn component5 = @object.GetComponent<GeneratorCreepSpawn>();
					if (component5 != null)
					{
						if (GameHubBehaviour.Hub.Net.IsServer())
						{
							component4.PatrolController = (component5.PatrolController ?? @object.GetComponent<BotPatrolController>());
						}
					}
					else
					{
						CreepFactory.Log.ErrorFormat("Could not find GeneratorCreepSpawn in object with id: {0} - Returned Identifiable name: {1}", new object[]
						{
							data.CauserId,
							@object.name
						});
					}
				}
			}
			identifiable.transform.parent = GameHubBehaviour.Hub.Drawer.Creeps;
			bitComponent4.Renderer.transform.parent = identifiable.transform;
			bitComponent4.Renderer.transform.localPosition = Vector3.zero;
			bitComponent4.Renderer.transform.localRotation = Quaternion.identity;
			LayerManager.Layer layer = LayerManager.Layer.Creep;
			if (flag)
			{
				TeamKind creepTeam = data.CreepTeam;
				if (creepTeam != TeamKind.Red)
				{
					if (creepTeam != TeamKind.Blue)
					{
						if (creepTeam == TeamKind.Neutral)
						{
							layer = LayerManager.Layer.PlayerNeutral;
						}
					}
					else
					{
						layer = LayerManager.Layer.PlayerBlu;
					}
				}
				else
				{
					layer = LayerManager.Layer.PlayerRed;
				}
			}
			LayerManager.SetLayerRecursively(identifiable, layer);
			identifiable.Register(creepId);
			identifiable.name = string.Format("[{1}]{0}", creepInfo.Name, creepId);
			bitComponent4.Renderer.name = string.Format("Res[{1}]{0}", bitComponent4.RenderObjectAsset, creepId);
			if (!flag)
			{
				NavMeshAgent component6 = identifiable.GetComponent<NavMeshAgent>();
				component6.radius = creepInfo.CollisionRadius * creepInfo.NavigationRadius;
				component6.avoidancePriority = creepInfo.AvoidancePriority;
				CapsuleCollider component7 = identifiable.GetComponent<CapsuleCollider>();
				component7.radius = creepInfo.CollisionRadius;
			}
			CarGenerator component8 = bitComponent4.Renderer.gameObject.GetComponent<CarGenerator>();
			CDummy cdummy = bitComponent4.Renderer.GetComponent<CDummy>() ?? bitComponent4.Renderer.GetComponentInChildren<CDummy>();
			if (cdummy == null && flag)
			{
				cdummy = component8.cDummy;
			}
			bitComponent.Data.SetInfo(creepInfo.GetCombatInfo());
			bitComponent.Data.UpdateLevel(data.Level);
			bitComponent.IsPlayer = false;
			bitComponent.IsBot = false;
			bitComponent.IsCreep = true;
			bitComponent.Dummy = cdummy;
			bitComponent.CreepTeam = data.CreepTeam;
			if (GameHubBehaviour.Hub.Net.IsClient() || GameHubBehaviour.Hub.Net.isTest)
			{
				CreepAudioController component9 = identifiable.GetComponent<CreepAudioController>();
				if (component9)
				{
					component9.CarMovement = bitComponent2;
				}
			}
			if (!flag)
			{
				if (creepInfo.Attack)
				{
					TeamKind creepTeam2 = data.CreepTeam;
					GadgetInfo gadgetInfo;
					if (creepTeam2 != TeamKind.Red)
					{
						if (creepTeam2 != TeamKind.Blue)
						{
							gadgetInfo = creepInfo.Attack;
						}
						else
						{
							gadgetInfo = (creepInfo.AttackBlu ?? creepInfo.Attack);
						}
					}
					else
					{
						gadgetInfo = (creepInfo.AttackRed ?? creepInfo.Attack);
					}
					if (gadgetInfo.GetType() == typeof(CreepBasicAttackInfo))
					{
						bitComponent.CustomGadget0 = bitComponent.CustomGadget1;
						bitComponent.CustomGadget1.enabled = true;
						bitComponent.CustomGadget2.enabled = false;
					}
					else
					{
						bitComponent.CustomGadget0 = bitComponent.CustomGadget2;
						bitComponent.CustomGadget1.enabled = false;
						bitComponent.CustomGadget2.enabled = true;
					}
					bitComponent.CustomGadget0.Parent = identifiable;
					bitComponent.CustomGadget0.Combat = bitComponent;
					bitComponent.CustomGadget0.Slot = GadgetSlot.CustomGadget0;
					bitComponent.CustomGadget0.SetInfo(gadgetInfo);
				}
			}
			else
			{
				bitComponent2.Char = creepInfo.BotCharacterInfo;
				if (visualGadgetGroup != null)
				{
					visualGadgetGroup.SetSlots(new Transform[]
					{
						cdummy.Turret
					});
				}
				if (creepInfo.BotCharacterInfo.CustomGadget0)
				{
					if (bitComponent.CustomGadget0 == null)
					{
						bitComponent.CustomGadget0 = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(creepInfo.BotCharacterInfo.CustomGadget0.GadgetType());
					}
					bitComponent.CustomGadget0.Parent = identifiable;
					bitComponent.CustomGadget0.Combat = bitComponent;
					bitComponent.CustomGadget0.Slot = GadgetSlot.CustomGadget0;
					bitComponent.CustomGadget0.Activate();
				}
				if (creepInfo.BotCharacterInfo.CustomGadget1)
				{
					if (bitComponent.CustomGadget1 == null)
					{
						bitComponent.CustomGadget1 = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(creepInfo.BotCharacterInfo.CustomGadget1.GadgetType());
					}
					bitComponent.CustomGadget1.Parent = identifiable;
					bitComponent.CustomGadget1.Combat = bitComponent;
					bitComponent.CustomGadget1.Slot = GadgetSlot.CustomGadget1;
					if (creepInfo.BotUseGadgets)
					{
						bitComponent.CustomGadget1.Activate();
					}
				}
				if (creepInfo.BotCharacterInfo.CustomGadget2)
				{
					if (bitComponent.CustomGadget2 == null)
					{
						bitComponent.CustomGadget2 = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(creepInfo.BotCharacterInfo.CustomGadget2.GadgetType());
					}
					bitComponent.CustomGadget2.Parent = identifiable;
					bitComponent.CustomGadget2.Combat = bitComponent;
					bitComponent.CustomGadget2.Slot = GadgetSlot.CustomGadget2;
					if (creepInfo.BotUseGadgets)
					{
						bitComponent.CustomGadget1.Activate();
					}
				}
				if (creepInfo.BotCharacterInfo.PassiveGadget)
				{
					if (bitComponent.PassiveGadget == null)
					{
						bitComponent.PassiveGadget = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(creepInfo.BotCharacterInfo.PassiveGadget.GadgetType());
					}
					bitComponent.PassiveGadget.Parent = identifiable;
					bitComponent.PassiveGadget.Combat = bitComponent;
					bitComponent.PassiveGadget.Slot = GadgetSlot.PassiveGadget;
				}
				if (creepInfo.BotCharacterInfo.TrailGadget)
				{
					if (bitComponent.TrailGadget == null)
					{
						bitComponent.TrailGadget = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(creepInfo.BotCharacterInfo.TrailGadget.GadgetType());
					}
					bitComponent.TrailGadget.Parent = identifiable;
					bitComponent.TrailGadget.Combat = bitComponent;
					bitComponent.TrailGadget.Slot = GadgetSlot.TrailGadget;
				}
				if (creepInfo.BotCharacterInfo.OutOfCombatGadget)
				{
					if (bitComponent.OutOfCombatGadget == null)
					{
						bitComponent.OutOfCombatGadget = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(creepInfo.BotCharacterInfo.OutOfCombatGadget.GadgetType());
					}
					bitComponent.OutOfCombatGadget.Parent = identifiable;
					bitComponent.OutOfCombatGadget.Combat = bitComponent;
					bitComponent.OutOfCombatGadget.Slot = GadgetSlot.OutOfCombatGadget;
				}
				if (creepInfo.BotCharacterInfo.DmgUpgrade)
				{
					if (bitComponent.DmgUpgrade == null)
					{
						bitComponent.DmgUpgrade = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(creepInfo.BotCharacterInfo.DmgUpgrade.GadgetType());
					}
					bitComponent.DmgUpgrade.Parent = identifiable;
					bitComponent.DmgUpgrade.Combat = bitComponent;
					bitComponent.DmgUpgrade.Slot = GadgetSlot.DmgUpgrade;
				}
				if (creepInfo.BotCharacterInfo.HPUpgrade)
				{
					if (bitComponent.HPUpgrade == null)
					{
						bitComponent.HPUpgrade = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(creepInfo.BotCharacterInfo.HPUpgrade.GadgetType());
					}
					bitComponent.HPUpgrade.Parent = identifiable;
					bitComponent.HPUpgrade.Combat = bitComponent;
					bitComponent.HPUpgrade.Slot = GadgetSlot.HPUpgrade;
				}
				if (creepInfo.BotCharacterInfo.EPUpgrade)
				{
					if (bitComponent.EPUpgrade == null)
					{
						bitComponent.EPUpgrade = (GadgetBehaviour)bitComponent.Gadgets.AddComponent(creepInfo.BotCharacterInfo.EPUpgrade.GadgetType());
					}
					bitComponent.EPUpgrade.Parent = identifiable;
					bitComponent.EPUpgrade.Combat = bitComponent;
					bitComponent.EPUpgrade.Slot = GadgetSlot.EPUpgrade;
				}
				if (bitComponent.CustomGadget0)
				{
					bitComponent.CustomGadget0.SetInfo(creepInfo.BotCharacterInfo.CustomGadget0);
				}
				if (bitComponent.CustomGadget1)
				{
					bitComponent.CustomGadget1.SetInfo(creepInfo.BotCharacterInfo.CustomGadget1);
				}
				if (bitComponent.CustomGadget2)
				{
					bitComponent.CustomGadget2.SetInfo(creepInfo.BotCharacterInfo.CustomGadget2);
				}
				if (bitComponent.PassiveGadget)
				{
					bitComponent.PassiveGadget.SetInfo(creepInfo.BotCharacterInfo.PassiveGadget);
				}
				if (bitComponent.TrailGadget)
				{
					bitComponent.TrailGadget.SetInfo(creepInfo.BotCharacterInfo.TrailGadget);
				}
				if (bitComponent.OutOfCombatGadget)
				{
					bitComponent.OutOfCombatGadget.SetInfo(creepInfo.BotCharacterInfo.OutOfCombatGadget);
				}
				if (bitComponent.DmgUpgrade)
				{
					bitComponent.DmgUpgrade.SetInfo(creepInfo.BotCharacterInfo.DmgUpgrade);
				}
				if (bitComponent.HPUpgrade)
				{
					bitComponent.HPUpgrade.SetInfo(creepInfo.BotCharacterInfo.HPUpgrade);
				}
				if (bitComponent.EPUpgrade)
				{
					bitComponent.EPUpgrade.SetInfo(creepInfo.BotCharacterInfo.EPUpgrade);
				}
			}
			if (flag && component4)
			{
				BotAIController botAIController = component4.gameObject.GetComponent<BotAIController>();
				if (botAIController != null)
				{
					UnityEngine.Object.Destroy(botAIController);
				}
				botAIController = component4.gameObject.AddComponent<BotAIController>();
				component4.SetController(botAIController, creepInfo.BotCharacterInfo);
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				component8.carWheelsController.CarMovement = bitComponent2;
			}
			return identifiable;
		}

		private Identifiable GetTemplate(CreepTemplateKind templateKind)
		{
			if (templateKind == CreepTemplateKind.CreepTemplate)
			{
				return this.CreepTemplate;
			}
			if (templateKind == CreepTemplateKind.CreepBotTemplate)
			{
				return this.CreepBotTemplate;
			}
			if (templateKind != CreepTemplateKind.CreepDummyTemplate)
			{
				return null;
			}
			return this.CreepDummyTemplate;
		}

		public void DestroyCreep(SpawnController spawn)
		{
			CreepController component = spawn.GetComponent<CreepController>();
			bool flag = component != null && component.Creep.BotCharacterInfo != null;
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				this.DestroyGui(spawn.Id, flag);
			}
			if (flag)
			{
				this.DestroyBot(spawn);
				return;
			}
			GameHubBehaviour.Hub.Resources.ReturnToCache(spawn.RenderObjectAsset, spawn.Renderer);
			GameHubBehaviour.Hub.Resources.ReturnToPrefabCache(this.CreepTemplate, spawn.Id);
		}

		private void DestroyBot(SpawnController spawn)
		{
			GameHubBehaviour.Hub.Resources.ReturnToCache(spawn.RenderObjectAsset + "_prefab", spawn.Renderer);
			GameHubBehaviour.Hub.Resources.ReturnToCache(spawn.VisualGadgetsAsset, spawn.RendererVisualGadgets);
			GameHubBehaviour.Hub.Resources.ReturnToPrefabCache(this.CreepBotTemplate, spawn.Id);
		}

		private void DestroyGui(Identifiable id, bool isBot)
		{
			if (!isBot)
			{
				return;
			}
			Transform transform;
			if (this._createdLifeBars.TryGetValue(id, out transform))
			{
				this._createdLifeBars.Remove(id);
			}
			if (this._createdEnemyIcons.TryGetValue(id, out transform))
			{
				this._createdEnemyIcons.Remove(id);
			}
		}

		public void OnCleanup(CleanupMessage msg)
		{
			foreach (KeyValuePair<Identifiable, Transform> keyValuePair in this._createdLifeBars)
			{
				UnityEngine.Object.Destroy(keyValuePair.Value);
			}
			foreach (KeyValuePair<Identifiable, Transform> keyValuePair2 in this._createdEnemyIcons)
			{
				UnityEngine.Object.Destroy(keyValuePair2.Value);
			}
			this._createdLifeBars.Clear();
			this._createdEnemyIcons.Clear();
		}

		public void CacheCreeps(CreepInfo info, int amount)
		{
			for (int i = 0; i < info.AssetsByRound.Length; i++)
			{
				string path = info.AssetsByRound[i].Asset + "_prefab";
				GameHubBehaviour.Hub.Resources.PreCache(path, typeof(Transform), amount);
			}
			if (info.RandomAssets != null && info.RandomAssets.Length > 0)
			{
				for (int j = 0; j < info.RandomAssets.Length; j++)
				{
					string text = info.RandomAssets[j] + "_prefab";
					if (!string.IsNullOrEmpty(text))
					{
						GameHubBehaviour.Hub.Resources.PreCache(text, typeof(Transform), amount);
					}
				}
			}
			if (!string.IsNullOrEmpty(info.Asset))
			{
				GameHubBehaviour.Hub.Resources.PreCache(info.Asset, typeof(Transform), amount);
			}
			if (!string.IsNullOrEmpty(info.AssetBlu))
			{
				GameHubBehaviour.Hub.Resources.PreCache(info.AssetBlu, typeof(Transform), amount);
			}
			if (!string.IsNullOrEmpty(info.AssetRed))
			{
				GameHubBehaviour.Hub.Resources.PreCache(info.AssetRed, typeof(Transform), amount);
			}
			if (info.BotCharacterInfo != null)
			{
				this.CachingBots += amount;
				this.CacheBot(info.BotCharacterInfo, amount, this.GetTemplate(info.TemplateKind), delegate
				{
					this.CachingBots--;
				});
				return;
			}
			for (int k = 0; k < amount; k++)
			{
				if (info.Attack != null)
				{
					info.Attack.PreCacheAssets();
				}
				if (info.AttackBlu != null && info.Attack != info.AttackBlu)
				{
					info.AttackBlu.PreCacheAssets();
				}
				if (info.AttackRed != null && info.AttackRed != info.AttackBlu && info.AttackRed != info.Attack)
				{
					info.AttackRed.PreCacheAssets();
				}
			}
			GameHubBehaviour.Hub.Resources.PrefabPreCache(this.CreepTemplate, amount);
			if (info.RewardAmount > 0)
			{
				string rewardPickup = info.RewardPickup;
				int amount2 = info.RewardAmount * amount;
				GameHubBehaviour.Hub.Resources.PreCache(rewardPickup, typeof(Transform), amount2);
			}
		}

		private void CacheBot(CharacterInfo characterInfo, int amount, Identifiable template, Action onCacheEnd)
		{
			GameHubBehaviour.Hub.Resources.PrefabPreCache(template, amount);
			for (int i = 0; i < amount; i++)
			{
				Identifiable obj = (Identifiable)GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(template, Vector3.zero, Quaternion.identity);
				GameHubBehaviour.Hub.Resources.ReturnToPrefabCache(template, obj);
				if (!string.IsNullOrEmpty(characterInfo.Asset))
				{
					string path = characterInfo.Asset + "_prefab";
					Transform obj2 = (Transform)GameHubBehaviour.Hub.Resources.CacheInstantiate(path, typeof(Transform), Vector3.zero, Quaternion.identity);
					GameHubBehaviour.Hub.Resources.ReturnToCache(path, obj2);
				}
				if (onCacheEnd != null)
				{
					onCacheEnd();
				}
			}
			if (characterInfo.CustomGadget0)
			{
				characterInfo.CustomGadget0.PreCacheNAssets(amount);
			}
			if (characterInfo.CustomGadget1)
			{
				characterInfo.CustomGadget1.PreCacheNAssets(amount);
			}
			if (characterInfo.CustomGadget2)
			{
				characterInfo.CustomGadget2.PreCacheNAssets(amount);
			}
			if (characterInfo.PassiveGadget)
			{
				characterInfo.PassiveGadget.PreCacheNAssets(amount);
			}
			if (characterInfo.TrailGadget)
			{
				characterInfo.TrailGadget.PreCacheNAssets(amount);
			}
			if (characterInfo.OutOfCombatGadget)
			{
				characterInfo.OutOfCombatGadget.PreCacheNAssets(amount);
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(CreepFactory));

		public Identifiable CreepTemplate;

		public Identifiable CreepBotTemplate;

		public Identifiable CreepDummyTemplate;

		public Camera UICamera;

		private GameGui _gameGui;

		private Dictionary<Identifiable, Transform> _createdLifeBars = new Dictionary<Identifiable, Transform>();

		private Dictionary<Identifiable, Transform> _createdEnemyIcons = new Dictionary<Identifiable, Transform>();

		public int CachingBots;
	}
}
