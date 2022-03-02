using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.GadgetScript;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[RequireComponent(typeof(CombatData))]
	[RequireComponent(typeof(CombatController))]
	[RequireComponent(typeof(CombatAttributes))]
	[RequireComponent(typeof(CombatFeedback))]
	public class CombatObject : CombatRef, ICombatObject, IObjectSpawnListener, ICachedObject, IGadgetOwner
	{
		public CarInput CarInput
		{
			get
			{
				return this._carInput;
			}
			set
			{
				this._carInput = value;
			}
		}

		public CDummy Dummy
		{
			get
			{
				return this._dummy;
			}
			set
			{
				this._dummy = value;
			}
		}

		public CombatData Data
		{
			get
			{
				CombatData result;
				if ((result = this._data) == null)
				{
					result = (this._data = base.GetComponent<CombatData>());
				}
				return result;
			}
		}

		public IPlayerStats Stats
		{
			get
			{
				PlayerStats result;
				if ((result = this._stats) == null)
				{
					result = (this._stats = base.GetComponent<PlayerStats>());
				}
				return result;
			}
		}

		public CombatAttributes Attributes
		{
			get
			{
				CombatAttributes result;
				if ((result = this._attributes) == null)
				{
					result = (this._attributes = base.GetComponent<CombatAttributes>());
				}
				return result;
			}
		}

		public ICombatMovement CombatMovement
		{
			get
			{
				return this.Movement;
			}
		}

		public ITurretMovement TurretMovement { get; private set; }

		public CombatController Controller
		{
			get
			{
				CombatController result;
				if ((result = this._controller) == null)
				{
					result = (this._controller = base.GetComponent<CombatController>());
				}
				return result;
			}
		}

		public CombatFeedback Feedback
		{
			get
			{
				CombatFeedback result;
				if ((result = this._feedback) == null)
				{
					result = (this._feedback = base.GetComponent<CombatFeedback>());
				}
				return result;
			}
		}

		public CombatLayer Layer
		{
			get
			{
				CombatLayer result;
				if ((result = this._layer) == null)
				{
					result = (this._layer = base.GetComponent<CombatLayer>());
				}
				return result;
			}
		}

		public GadgetData GadgetStates
		{
			get
			{
				GadgetData result;
				if ((result = this._gadgetData) == null)
				{
					result = (this._gadgetData = base.GetComponent<GadgetData>());
				}
				return result;
			}
		}

		public IPhysicalObject PhysicalObject
		{
			get
			{
				return this.Movement;
			}
		}

		public IIdentifiable Identifiable
		{
			get
			{
				return base.Id;
			}
		}

		public ICombatObject GadgetCombatObject
		{
			get
			{
				return this;
			}
		}

		public ICombatController ModifierController
		{
			get
			{
				return this._controller;
			}
		}

		public SpawnController SpawnController
		{
			get
			{
				return this.m_oSpawnController;
			}
		}

		public IPlayerController PlayerController
		{
			get
			{
				IPlayerController result;
				if ((result = this._playerController) == null)
				{
					result = (this._playerController = (base.GetComponent<PlayerController>() ?? base.GetComponent<BotAIController>()));
				}
				return result;
			}
		}

		public IPlayerData PlayerData
		{
			get
			{
				return this.Player;
			}
		}

		public bool IsPlayer { get; set; }

		public bool IsBot { get; set; }

		public bool NoHit { get; private set; }

		public TeamKind Team
		{
			get
			{
				return (!this.IsPlayer) ? this.OwnerTeam : this.Player.Team;
			}
		}

		public bool IsAlive()
		{
			return this.Data.IsAlive();
		}

		public void Kill()
		{
			if (this.Controller)
			{
				this.Controller.ForceDeath();
			}
		}

		public Transform Transform { get; set; }

		public bool BombBlocked
		{
			get
			{
				return this.BombBlocker && this.BombBlocker.activeSelf;
			}
			set
			{
				if (this.BombBlocker)
				{
					this.BombBlocker.SetActive(value);
				}
			}
		}

		public virtual bool IsCarryingBomb
		{
			get
			{
				return GameHubBehaviour.Hub.BombManager.IsCarryingBomb(this);
			}
		}

		public bool IsLocalPlayer
		{
			get
			{
				return this.Player == GameHubBehaviour.Hub.Players.CurrentPlayerData;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			if (this.ListenToEnable != null)
			{
				this.ListenToEnable(this);
			}
		}

		protected void Awake()
		{
			this.Combat = this;
			this.Transform = base.transform;
			this.Body = base.GetComponent<Rigidbody>();
			if (!this.CarInput)
			{
				this.CarInput = base.GetComponent<CarInput>();
			}
			if (!this.Movement)
			{
				this.Movement = base.GetComponent<CarMovement>();
			}
			if (this.TurretMovement == null)
			{
				this.TurretMovement = base.GetComponent<ITurretMovement>();
			}
			if (!this.Gadgets)
			{
				this.Gadgets = new GameObject("Gadgets");
				this.Gadgets.transform.parent = base.transform;
			}
			if (!this._botAiGoalManager)
			{
				this._botAiGoalManager = base.GetComponent<BotAIGoalManager>();
			}
			this.m_oSpawnController = base.GetComponent<SpawnController>();
			CapsuleCollider component = base.GetComponent<CapsuleCollider>();
			if (component)
			{
				this.CapsuleRadius = component.radius;
				this.Radius = Mathf.Max(component.radius, component.height / 2f);
			}
			if (this.BombBlocker)
			{
				this.BombBlocker.SetActive(false);
			}
			HazardArea component2 = base.GetComponent<HazardArea>();
			this.NoHit = (component2 != null);
		}

		public void Clear()
		{
			if (this.IsAlive())
			{
				this.Data.HP = (float)this.Data.HPMax;
			}
			this.Data.EP = 0f;
			this.Data.SetHpTemp(0f);
			this.CustomGadget0.Clear();
			this.CustomGadget1.Clear();
			this.CustomGadget2.Clear();
			this.GenericGadget.Clear();
			this.BoostGadget.Clear();
			this.OutOfCombatGadget.Clear();
			this.PassiveGadget.Clear();
			this.RespawnGadget.Clear();
			this.Controller.Clear();
			this.Feedback.ClearFeedbacks();
			this.Movement.Clear();
			this.TurretMovement.ResetRotation();
		}

		private void Start()
		{
			if (GameHubBehaviour.Hub && GameHubBehaviour.Hub.Net.IsServer() && (this.IsPlayer || this.IsBot))
			{
				TeamBlockController[] array = Object.FindObjectsOfType<TeamBlockController>();
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Add(this);
				}
			}
		}

		public bool IsSameTeamAsCurrentPlayer()
		{
			return this.Team == GameHubBehaviour.Hub.Players.CurrentPlayerTeam;
		}

		public void AddGadget(GadgetSlot slot, IHMMGadgetContext combatGadget)
		{
			this.CustomGadgets.Add(combatGadget);
			this._customGadgets.Add((int)slot, combatGadget);
		}

		public IHMMGadgetContext GetGadgetContext(int id)
		{
			IHMMGadgetContext result;
			this._customGadgets.TryGetValue(id, out result);
			return result;
		}

		public bool HasGadgetContext(int id)
		{
			return this._customGadgets.ContainsKey(id);
		}

		public IGadgetInput GetGadgetInput(GadgetSlot slot)
		{
			IHMMGadgetContext result;
			if (this._customGadgets.TryGetValue((int)slot, out result))
			{
				return result;
			}
			return this.GetGadget(slot);
		}

		public GadgetBehaviour GetGadget(GadgetSlot slot)
		{
			switch (slot)
			{
			case GadgetSlot.CustomGadget0:
				return this.CustomGadget0;
			case GadgetSlot.CustomGadget1:
				return this.CustomGadget1;
			case GadgetSlot.CustomGadget2:
				return this.CustomGadget2;
			case GadgetSlot.BoostGadget:
				return this.BoostGadget;
			case GadgetSlot.PassiveGadget:
				return this.PassiveGadget;
			case GadgetSlot.GenericGadget:
				return this.GenericGadget;
			case GadgetSlot.HPUpgrade:
				return this.HPUpgrade;
			case GadgetSlot.EPUpgrade:
				return this.EPUpgrade;
			case GadgetSlot.RespawnGadget:
				return this.RespawnGadget;
			case GadgetSlot.BombGadget:
				return this.BombGadget;
			case GadgetSlot.DmgUpgrade:
				return this.DmgUpgrade;
			case GadgetSlot.OutOfCombatGadget:
				return this.OutOfCombatGadget;
			case GadgetSlot.TrailGadget:
				return this.TrailGadget;
			case GadgetSlot.TakeoffGadget:
				return this.TakeOffGadget;
			case GadgetSlot.KillGadget:
				return this.KillGadget;
			case GadgetSlot.BombExplosionGadget:
				return this.BombExplosionGadget;
			case GadgetSlot.SprayGadget:
				return this.SprayGadget;
			case GadgetSlot.GridHighlightGadget:
				return this.GridHighlightGadget;
			}
			return null;
		}

		public void SetGadget(GadgetBehaviour beh)
		{
			if (beh == null)
			{
				return;
			}
			switch (beh.Slot)
			{
			case GadgetSlot.CustomGadget0:
				this.CustomGadget0 = beh;
				break;
			case GadgetSlot.CustomGadget1:
				this.CustomGadget1 = beh;
				break;
			case GadgetSlot.CustomGadget2:
				this.CustomGadget2 = beh;
				break;
			case GadgetSlot.BoostGadget:
				this.BoostGadget = beh;
				break;
			case GadgetSlot.PassiveGadget:
				this.PassiveGadget = beh;
				break;
			case GadgetSlot.GenericGadget:
				this.GenericGadget = beh;
				break;
			case GadgetSlot.HPUpgrade:
				this.HPUpgrade = beh;
				break;
			case GadgetSlot.EPUpgrade:
				this.EPUpgrade = beh;
				break;
			case GadgetSlot.BombGadget:
				this.BombGadget = (beh as BombGadget);
				break;
			case GadgetSlot.DmgUpgrade:
				this.DmgUpgrade = beh;
				break;
			case GadgetSlot.OutOfCombatGadget:
				this.OutOfCombatGadget = beh;
				break;
			case GadgetSlot.TrailGadget:
				this.TrailGadget = beh;
				break;
			}
		}

		public void AddGadgetSlotDictionary(int id, GadgetSlot slot)
		{
			if (this.gadgetSlotDictionary.ContainsKey(id))
			{
				CombatObject.Log.WarnFormat("Duplicate gadgetIds={0} on slot={1} and slot={2}", new object[]
				{
					id,
					this.gadgetSlotDictionary[id],
					slot
				});
			}
			this.gadgetSlotDictionary[id] = slot;
		}

		public GadgetSlot GetSlotByGadgetId(int gadgetId)
		{
			if (this.gadgetSlotDictionary.ContainsKey(gadgetId))
			{
				return this.gadgetSlotDictionary[gadgetId];
			}
			return GadgetSlot.None;
		}

		public Transform GetDummy(CDummy.DummyKind dummy, string customDummyName)
		{
			if (!this.Dummy)
			{
				this.Dummy = base.GetComponentInChildren<CDummy>();
				if (!this.Dummy)
				{
					return base.transform;
				}
			}
			return this.Dummy.GetDummy(dummy, customDummyName, this);
		}

		public void OnObjectSpawned(SpawnEvent evt)
		{
			base.enabled = true;
			if (this.ListenToObjectSpawn != null)
			{
				this.ListenToObjectSpawn(this, evt);
			}
		}

		public static CombatObject GetCombatObject(int objectID)
		{
			Identifiable @object = GameHubBehaviour.Hub.ObjectCollection.GetObject(objectID);
			if (!@object)
			{
				CombatObject.Log.WarnFormat("Identifiable not found. ObjectId={0}.", new object[]
				{
					objectID
				});
				return null;
			}
			CombatObject component = @object.GetComponent<CombatObject>();
			if (!component)
			{
				CombatObject.Log.WarnFormat("CombatObject not found. ObjectId={0}.", new object[]
				{
					objectID
				});
				return null;
			}
			return component;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatObject.ObjectUnspawnListener ListenToObjectUnspawn;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatObject.ObjectSpawnListener ListenToObjectSpawn;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatObject.AttributeChangedListener ListenToAttributeChanged;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatObject.OnIndestructibleAlmostDied ListenToIndestructibleAlmostDied;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatObject.BarrierHitListener ListenToBarrierHit;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatObject.PreDamageListener ListenToPreDamageCaused;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatObject.PosDamageListener ListenToPosDamageCaused;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatObject.PreDamageListener ListenToPreDamageTaken;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatObject.PosDamageListener ListenToPosDamageTaken;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatObject.PosDamageListener ListenToPosRepairCaused;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatObject.PosDamageListener ListenToPosRepairTaken;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatObject.PreHealingListener ListenToPreHealingCaused;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatObject.PosHealingListener ListenToPosHealingCaused;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatObject.PreHealingListener ListenToPreHealingTaken;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event CombatObject.PosHealingListener ListenToPosHealingTaken;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<float, int> OnDamageDealt;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<float, int> OnDamageReceived;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<float, Vector3, Vector3> OnDamageReceivedFullData;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<float, int> OnRepairDealt;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<float, int> OnRepairReceived;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<float, int> OnImpulseDealt;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<float, int> OnImpulseReceived;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<float, int> OnCooldownRepairDealt;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<float, int> OnCooldownRepairReceived;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<float, int> OnTempHPReceived;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<ModifierEvent> ListenToModifierReceived;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<Collision> ListenToCollisionStay;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<Collision> ListenToCollisionEnter;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<CombatObject> ListenToEnable;

		private void OnCollisionEnter(Collision col)
		{
			if (this.ListenToCollisionEnter != null)
			{
				this.ListenToCollisionEnter(col);
			}
		}

		private void OnCollisionStay(Collision col)
		{
			if (this.ListenToCollisionStay != null)
			{
				this.ListenToCollisionStay(col);
			}
		}

		public void OnModifierReceived(ModifierEvent e)
		{
			Action<float, int> action = null;
			EffectKind effect = e.Effect;
			switch (effect)
			{
			case EffectKind.CooldownRepair:
				action = this.OnCooldownRepairReceived;
				goto IL_9C;
			case EffectKind.HPLightDamage:
			case EffectKind.HPHeavyDamage:
				break;
			case EffectKind.Impulse:
				action = this.OnImpulseReceived;
				goto IL_9C;
			default:
				switch (effect)
				{
				case EffectKind.HPPureDamage:
				case EffectKind.HPPureDamageNL:
					break;
				case EffectKind.HPRepair:
					action = this.OnRepairReceived;
					goto IL_9C;
				default:
					if (effect != EffectKind.NonLethalDamageIgnoringTempHP)
					{
						goto IL_9C;
					}
					break;
				}
				break;
			}
			action = this.OnDamageReceived;
			if (this.OnDamageReceivedFullData != null)
			{
				this.OnDamageReceivedFullData(e.Amount, Vector3.zero, Vector3.zero);
			}
			IL_9C:
			if (action != null)
			{
				action(e.Amount, e.OtherId);
			}
			if (this.ListenToModifierReceived != null)
			{
				this.ListenToModifierReceived(e);
			}
		}

		public void OnModifierDealt(ModifierEvent e)
		{
			Action<float, int> action = null;
			bool flag = e.ObjId != base.Id.ObjId;
			EffectKind effect = e.Effect;
			switch (effect)
			{
			case EffectKind.CooldownRepair:
				action = this.OnCooldownRepairDealt;
				goto IL_B8;
			case EffectKind.HPLightDamage:
			case EffectKind.HPHeavyDamage:
			case EffectKind.HPGodDamage:
			case EffectKind.NonLethalDamageIgnoringTempHP:
				break;
			case EffectKind.Impulse:
				action = this.OnImpulseDealt;
				goto IL_B8;
			default:
				switch (effect)
				{
				case EffectKind.HPPureDamage:
				case EffectKind.HPPureDamageNL:
					break;
				case EffectKind.HPRepair:
					action = this.OnRepairDealt;
					goto IL_B8;
				default:
					flag = false;
					goto IL_B8;
				}
				break;
			case EffectKind.HPTemp:
				action = this.OnTempHPReceived;
				goto IL_B8;
			}
			action = this.OnDamageDealt;
			IL_B8:
			if (action != null)
			{
				action(e.Amount, e.ObjId);
			}
			if (flag && e.ObjId != base.Id.ObjId)
			{
				switch (e.Slot)
				{
				case GadgetSlot.CustomGadget0:
					this.GadgetStates.G0StateObject.ClientGadgetHit(e.ObjId);
					break;
				case GadgetSlot.CustomGadget1:
					this.GadgetStates.G1StateObject.ClientGadgetHit(e.ObjId);
					break;
				case GadgetSlot.CustomGadget2:
					this.GadgetStates.G2StateObject.ClientGadgetHit(e.ObjId);
					break;
				case GadgetSlot.BoostGadget:
					this.GadgetStates.GBoostStateObject.ClientGadgetHit(e.ObjId);
					break;
				case GadgetSlot.PassiveGadget:
					this.GadgetStates.GPStateObject.ClientGadgetHit(e.ObjId);
					break;
				}
			}
		}

		public void OnObjectUnspawned(UnspawnEvent msg)
		{
			base.enabled = false;
			if (this.ListenToObjectUnspawn != null)
			{
				this.ListenToObjectUnspawn(this, msg);
			}
		}

		protected void OnDestroy()
		{
			foreach (IHMMGadgetContext ihmmgadgetContext in this.CustomGadgets)
			{
				ihmmgadgetContext.CleanUp();
			}
			this.Effects.Clear();
			this.ListenToObjectUnspawn = null;
			this.ListenToObjectSpawn = null;
			this.ListenToAttributeChanged = null;
			this.ListenToBarrierHit = null;
			this.ListenToPreDamageCaused = null;
			this.ListenToPosDamageCaused = null;
			this.ListenToPreDamageTaken = null;
			this.ListenToPosDamageTaken = null;
			this.ListenToPosRepairCaused = null;
			this.ListenToPosRepairTaken = null;
			this.ListenToPreHealingCaused = null;
			this.ListenToPosHealingCaused = null;
			this.ListenToPreHealingTaken = null;
			this.ListenToPosHealingTaken = null;
			this.OnDamageDealt = null;
			this.OnDamageReceived = null;
			this.OnDamageReceivedFullData = null;
			this.OnRepairDealt = null;
			this.OnRepairReceived = null;
			this.OnImpulseDealt = null;
			this.OnImpulseReceived = null;
			this.OnCooldownRepairDealt = null;
			this.OnCooldownRepairReceived = null;
			this.OnTempHPReceived = null;
			this.ListenToCollisionStay = null;
			this.ListenToCollisionEnter = null;
			this.ListenToEnable = null;
			this.ListenToIndestructibleAlmostDied = null;
			this.OwnerTeam = TeamKind.Zero;
			if (GameHubBehaviour.Hub.Net.IsServer() && (this.IsPlayer || this.IsBot))
			{
				TeamBlockController[] array = Object.FindObjectsOfType<TeamBlockController>();
				for (int i = 0; i < array.Length; i++)
				{
					array[i].Remove(this);
				}
			}
		}

		public void AddEffect(BaseFX effect)
		{
			if (!this.Effects.Contains(effect))
			{
				this.Effects.Add(effect);
			}
		}

		public void RemoveEffect(BaseFX effect)
		{
			this.Effects.Remove(effect);
		}

		public void IndestructibleAlmostDied(ModifierData mod, CombatObject causer, int eventId)
		{
			if (this.ListenToIndestructibleAlmostDied == null)
			{
				return;
			}
			this.ListenToIndestructibleAlmostDied(causer, this, mod, eventId);
		}

		public void BarrierHit(CombatObject causer, ModifierInstance mod, int eventId)
		{
			if (this.ListenToBarrierHit == null)
			{
				return;
			}
			this.ListenToBarrierHit(causer, mod, eventId);
		}

		public void PreDamageCaused(ModifierData mod, CombatObject taker, ref float amount, int eventId)
		{
			if (this.ListenToPreDamageCaused == null)
			{
				return;
			}
			this.ListenToPreDamageCaused(this, taker, mod, ref amount, eventId);
		}

		public void PosDamageCaused(ModifierData mod, CombatObject taker, float amount, int eventId)
		{
			if (this.ListenToPosDamageCaused == null)
			{
				return;
			}
			this.ListenToPosDamageCaused(this, taker, mod, amount, eventId);
		}

		public void PreDamageTaken(ModifierData mod, CombatObject causer, ref float amount, int eventId)
		{
			if (this.ListenToPreDamageTaken == null)
			{
				return;
			}
			this.ListenToPreDamageTaken(causer, this, mod, ref amount, eventId);
		}

		public void PosDamageTaken(ModifierData mod, CombatObject causer, float amount, int eventId)
		{
			if (this.ListenToPosDamageTaken == null)
			{
				return;
			}
			this.ListenToPosDamageTaken(causer, this, mod, amount, eventId);
		}

		public void PosRepairCaused(ModifierData mod, CombatObject taker, float amount, int eventId)
		{
			if (this.ListenToPosRepairCaused == null)
			{
				return;
			}
			this.ListenToPosRepairCaused(taker, this, mod, amount, eventId);
		}

		public void PosRepairTaken(ModifierData mod, CombatObject causer, float amount, int eventId)
		{
			if (this.ListenToPosRepairTaken == null)
			{
				return;
			}
			this.ListenToPosRepairTaken(causer, this, mod, amount, eventId);
		}

		public void PreHealingCaused(ModifierData mod, CombatObject taker, ref float amount, int eventId)
		{
			if (this.ListenToPreHealingCaused == null)
			{
				return;
			}
			this.ListenToPreHealingCaused(this, taker, mod, ref amount, eventId);
		}

		public void PosHealingCaused(ModifierData mod, CombatObject taker, float amount, int eventId)
		{
			if (this.ListenToPosHealingCaused == null)
			{
				return;
			}
			this.ListenToPosHealingCaused(this, taker, mod, amount, eventId);
		}

		public void PreHealingTaken(ModifierData mod, CombatObject causer, ref float amount, int eventId)
		{
			if (this.ListenToPreHealingTaken == null)
			{
				return;
			}
			this.ListenToPreHealingTaken(causer, this, mod, ref amount, eventId);
		}

		public void PosHealingTaken(ModifierData mod, CombatObject causer, float amount, int eventId)
		{
			if (this.ListenToPosHealingTaken == null)
			{
				return;
			}
			this.ListenToPosHealingTaken(causer, this, mod, amount, eventId);
		}

		public void AttributeChanged()
		{
			if (this.ListenToAttributeChanged == null)
			{
				return;
			}
			this.ListenToAttributeChanged();
		}

		public void OnSendToCache()
		{
			this.OnDestroy();
		}

		public void OnGetFromCache()
		{
		}

		public bool IsCursorEnabled(GadgetData.GadgetStateObject poState, GadgetBehaviour poGadget)
		{
			bool flag = this.Attributes.CurrentStatus.HasFlag(StatusKind.Jammed);
			bool flag2 = this.Attributes.IsGadgetDisarmed(poGadget.Slot, poGadget.Nature);
			bool flag3 = poState.GadgetState == GadgetState.CoolingAfterOverheat;
			return (poState.GadgetState == GadgetState.Ready || poState.GadgetState == GadgetState.Toggled) && this.Data.EP >= (float)poGadget.Info.ActivationCost && !flag && !flag3 && !flag2;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CombatObject));

		[SerializeField]
		private CarInput _carInput;

		public CombatMovement Movement;

		[SerializeField]
		private CDummy _dummy;

		public CombatData _data;

		public PlayerStats _stats;

		public CombatAttributes _attributes;

		public CombatController _controller;

		public CombatFeedback _feedback;

		public CombatLayer _layer;

		public GadgetData _gadgetData;

		private SpawnController m_oSpawnController;

		private IPlayerController _playerController;

		private BotAIGoalManager _botAiGoalManager;

		[NonSerialized]
		public PlayerData Player;

		public BaseFX WardEffect;

		public bool HideOnUnspawn = true;

		public bool IsCreep;

		public bool IsTurret;

		public bool IsWard;

		public bool IsBuilding;

		public bool IsBoss;

		public bool IsBomb;

		public TeamKind OwnerTeam;

		public int NeutralRewardIndex;

		public GameObject Gadgets;

		public Rigidbody Body;

		public GameObject BombBlocker;

		private Dictionary<int, GadgetSlot> gadgetSlotDictionary = new Dictionary<int, GadgetSlot>();

		public bool IsInBaseServer;

		public float Radius = 3f;

		public float CapsuleRadius = 1.5f;

		[NonSerialized]
		public string InstanceSelected;

		public bool AlyDebug;

		public GadgetBehaviour CustomGadget0;

		public GadgetBehaviour CustomGadget1;

		public GadgetBehaviour CustomGadget2;

		public GadgetBehaviour BoostGadget;

		public GadgetBehaviour PassiveGadget;

		public GadgetBehaviour TrailGadget;

		public GadgetBehaviour OutOfCombatGadget;

		public GadgetBehaviour GenericGadget;

		public GadgetBehaviour DmgUpgrade;

		public GadgetBehaviour HPUpgrade;

		public GadgetBehaviour EPUpgrade;

		public BombGadget BombGadget;

		public GadgetBehaviour RespawnGadget;

		public GadgetBehaviour TakeOffGadget;

		public GadgetBehaviour KillGadget;

		public GadgetBehaviour BombExplosionGadget;

		public GadgetBehaviour SprayGadget;

		public GadgetBehaviour GridHighlightGadget;

		public readonly List<IHMMGadgetContext> CustomGadgets = new List<IHMMGadgetContext>();

		private Dictionary<int, IHMMGadgetContext> _customGadgets = new Dictionary<int, IHMMGadgetContext>();

		public List<BaseFX> Effects = new List<BaseFX>();

		public enum ActionKind
		{
			None,
			OnPreDamageCaused,
			OnPosDamageCaused,
			OnPreDamageTaken,
			OnPosDamageTaken,
			OnPreHealingCaused,
			OnPosHealingCaused,
			OnPreHealingTaken,
			OnPosHealingTaken,
			OnIndestructibleAlmostDied
		}

		public delegate void ObjectUnspawnListener(CombatObject obj, UnspawnEvent msg);

		public delegate void ObjectSpawnListener(CombatObject obj, SpawnEvent msg);

		public delegate void PreDamageListener(CombatObject causer, CombatObject taker, ModifierData mod, ref float amount, int eventId);

		public delegate void PosDamageListener(CombatObject causer, CombatObject taker, ModifierData mod, float amount, int eventId);

		public delegate void PreHealingListener(CombatObject causer, CombatObject taker, ModifierData mod, ref float amount, int eventId);

		public delegate void PosHealingListener(CombatObject causer, CombatObject taker, ModifierData mod, float amount, int eventId);

		public delegate void BarrierHitListener(CombatObject causer, ModifierInstance mod, int eventId);

		public delegate void OnIndestructibleAlmostDied(CombatObject causer, CombatObject taker, ModifierData mod, int eventId);

		public delegate void AttributeChangedListener();
	}
}
