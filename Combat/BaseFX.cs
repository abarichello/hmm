using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class BaseFX : AbstractFX, ICachedObject, PerkAttachToObject.IEffectAttachListener
	{
		public Vector3 LastPosition { get; private set; }

		public Vector3 PreviousPosition
		{
			get
			{
				return this._beforeLastPosition;
			}
		}

		public override int EventId { get; set; }

		public override byte CustomVar
		{
			get
			{
				return (this.Data != null) ? this.Data.CustomVar : 0;
			}
		}

		public bool CheckBombBlocking
		{
			get
			{
				return this._checkBombBlocker && this.BombBlocker.activeSelf;
			}
		}

		public TeamKind Team
		{
			get
			{
				return (this.Data == null || !this.Data.SourceCombat) ? TeamKind.Zero : this.Data.SourceCombat.Team;
			}
		}

		public override Identifiable Target
		{
			get
			{
				if (this._target)
				{
					return this._target;
				}
				if (this.Data == null)
				{
					return null;
				}
				if (this.Data.TargetId == -1)
				{
					return null;
				}
				this._target = GameHubBehaviour.Hub.ObjectCollection.GetObject(this.Data.TargetId);
				return this._target;
			}
		}

		public override Identifiable Owner
		{
			get
			{
				if (this._owner)
				{
					return this._owner;
				}
				if (this.Data == null)
				{
					return null;
				}
				if (this.Data.SourceId == -1)
				{
					return null;
				}
				this._owner = GameHubBehaviour.Hub.ObjectCollection.GetObject(this.Data.SourceId);
				return this._owner;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BaseFX.DestroyEffectListener OnDestroyEvent;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnPreEffectDestroyed;

		public override Vector3 TargetPosition
		{
			get
			{
				return (!(this._targetPosition == Vector3.zero) || this.Data == null) ? this._targetPosition : (this._targetPosition = this.Data.Target);
			}
		}

		public override CDummy.DummyKind GetDummyKind()
		{
			return this.Data.EffectInfo.ShotPosAndDir.Dummy;
		}

		public override Transform GetDummy(CDummy.DummyKind kind)
		{
			return this.Gadget.Combat.Dummy.GetDummy(kind, null);
		}

		public override GadgetBehaviour GetGadget()
		{
			return this.Gadget;
		}

		public override bool WasCreatedInFog()
		{
			return this.Data.FirstPackageSent;
		}

		public float GetRadius()
		{
			return this._radius;
		}

		public void SetRadius(float radius)
		{
			this._radius = radius;
			if (base.GetComponent<Collider>() is SphereCollider)
			{
				((SphereCollider)base.GetComponent<Collider>()).radius = radius;
			}
		}

		public bool TriggerDefaultDestroy(int targetId)
		{
			return this.TriggerDestroy(targetId, this.AttachedTransform.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Default, false);
		}

		public bool TriggerDestroy(int targetId, Vector3 position, bool wasScenery, Collider other, Vector3 normal, BaseFX.EDestroyReason reason, bool wasBarrier)
		{
			if (this._destroyTriggered)
			{
				return false;
			}
			if (this.Data == null)
			{
				BaseFX.Log.ErrorFormat("Data==null obj={0}", new object[]
				{
					base.name ?? "name is null"
				});
				return false;
			}
			EffectRemoveEvent effectRemoveEvent = new EffectRemoveEvent();
			effectRemoveEvent.TargetEventId = this.EventId;
			EffectRemoveEvent effectRemoveEvent2 = effectRemoveEvent;
			this.AttachedTransform.position = position;
			base.transform.position = position;
			effectRemoveEvent2.Origin = position;
			effectRemoveEvent.SrvEffect = this;
			effectRemoveEvent.SrvEffectCollider = base.GetComponent<Collider>();
			effectRemoveEvent.TargetId = -1;
			effectRemoveEvent.SrvWasBarrier = wasBarrier;
			effectRemoveEvent.SrvWasScenery = wasScenery;
			effectRemoveEvent.SrvOtherCollider = other;
			effectRemoveEvent.SrvNormal = normal;
			effectRemoveEvent.SrvPlayerID = this.Data.SourceId;
			effectRemoveEvent.DestroyReason = reason;
			EffectRemoveEvent effectRemoveEvent3 = effectRemoveEvent;
			if (!this.Gadget.CanDestroyEffect(ref effectRemoveEvent3))
			{
				return false;
			}
			if (this.Data.EffectInfo.Instantaneous)
			{
				this.DestroyEffect(effectRemoveEvent3);
				return true;
			}
			this._destroyTriggered = true;
			if (this.OnPreEffectDestroyed != null)
			{
				this.OnPreEffectDestroyed();
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return true;
			}
			effectRemoveEvent3.TargetId = targetId;
			GameHubBehaviour.Hub.Events.TriggerEvent(effectRemoveEvent3);
			return true;
		}

		public bool IsDead
		{
			get
			{
				return this._destroyTriggered;
			}
		}

		public void Init()
		{
			if (!this.m_boAwoken)
			{
				this.Awake();
			}
			if (this.UseGadgetRadius && base.GetComponent<Collider>() is SphereCollider)
			{
				float radius = this.Gadget.Radius;
				this.SetRadius(radius);
			}
			this.isSleeping = false;
			this._destroyTriggered = false;
			this._positionReset = true;
		}

		public DestroyEffect DestroyEffect(EffectRemoveEvent removeData)
		{
			if (this.isSleeping)
			{
				return default(DestroyEffect);
			}
			if (this.vfxInstance)
			{
				this.vfxInstance.Destroy(removeData);
			}
			if (this.sfxInstance)
			{
				this.sfxInstance.Destroy(removeData);
			}
			DestroyEffect destroyEffect = new DestroyEffect(this.Data, removeData);
			if (GameHubBehaviour.Hub.Net.IsClient() && Vector3.zero != removeData.Origin)
			{
				base.transform.position = removeData.Origin;
			}
			if (this.OnDestroyEvent != null)
			{
				this.OnDestroyEvent();
				this.OnDestroyEvent = null;
			}
			for (int i = 0; i < base.DestroyEffectListenerScripts.Length; i++)
			{
				if (base.DestroyEffectListenerScripts[i] is DestroyEffect.IDestroyEffectListener)
				{
					((DestroyEffect.IDestroyEffectListener)base.DestroyEffectListenerScripts[i]).OnDestroyEffect(destroyEffect);
				}
			}
			if (this.Gadget)
			{
				((DestroyEffect.IDestroyEffectListener)this.Gadget).OnDestroyEffect(destroyEffect);
			}
			this.BluCreated = (this.RedCreated = false);
			this._transform = base.transform;
			this.isSleeping = true;
			this.Data.Release();
			this.Data = null;
			this.Gadget = null;
			this._target = null;
			this._owner = null;
			this._targetPosition = Vector3.zero;
			this.Visible = true;
			this._combatHits.Clear();
			this._combatCollider.Clear();
			this._combatHitsList.Clear();
			this._triggeringColliders.Clear();
			this._triggerHits.Clear();
			this._nullCombatTriggeringColliders.Clear();
			if (this.DetachedVisualFXCount == 0)
			{
				this.Cleanup();
				GameHubBehaviour.Hub.Resources.ReturnToPrefabCache(this.prefabRef, this);
			}
			else
			{
				base.gameObject.SetActive(false);
			}
			return destroyEffect;
		}

		public void ActivateEffect(string strCondition)
		{
			this.vfxInstance.Activate(strCondition);
		}

		public void DeactivateEffect(string strCondition)
		{
			this.vfxInstance.Deactivate(strCondition);
		}

		public void OnVisualFXDeattach()
		{
			if (this.onCache)
			{
				BaseFX.Log.ErrorFormat("BaseFX::OnVisualFXDeattach - Something is trying to dettach from an cached object!!!", new object[]
				{
					base.gameObject
				});
				throw new Exception("BaseFX::OnVisualFXDeattach - Something is trying to dettach from an cached object!!!");
			}
			this.DetachedVisualFXCount++;
		}

		public void OnVisualFXAttach()
		{
			if (this.onCache)
			{
				BaseFX.Log.ErrorFormat("BaseFX::OnVisualFXAttach - Something is trying to dettach from an cached object!!!", new object[]
				{
					base.gameObject
				});
				throw new Exception("BaseFX::OnVisualFXAttach - Something is trying to dettach from an cached object!!!");
			}
			this.DetachedVisualFXCount--;
			if (this.DetachedVisualFXCount <= 0 && this.isSleeping)
			{
				this.DetachedVisualFXCount = 0;
				this.Cleanup();
				GameHubBehaviour.Hub.Resources.ReturnToPrefabCache(this.prefabRef, this);
			}
		}

		public bool CheckHit(CombatObject other)
		{
			return BaseFX.CheckHit(this.Gadget.Combat, other, this.Data);
		}

		public static bool CheckHit(CombatObject owner, CombatObject other, EffectEvent data)
		{
			return BaseFX.CheckHit(owner, other, data.EffectInfo);
		}

		public static bool CheckHit(CombatObject owner, CombatObject other, IHitMask info)
		{
			if (other == null)
			{
				return false;
			}
			if (other.IsWard && (other.WardEffect == null || (owner == other.WardEffect.Attached && !BaseFX.CheckHit(owner, owner, info))))
			{
				return false;
			}
			bool flag = other == owner && info.Self;
			bool flag2 = other.Team == owner.Team;
			bool flag3 = (flag2 && info.Friends && other != owner) || (!flag2 && info.Enemies);
			bool flag4 = (info.Bomb && other.IsBomb) || (info.Creeps && other.IsCreep) || (info.Turrets && other.IsTurret) || (info.Wards && other.IsWard) || (info.Buildings && other.IsBuilding) || (info.Players && other.IsPlayer) || (info.Boss && other.IsBoss);
			bool flag5 = !other.Attributes.CurrentStatus.HasFlag(StatusKind.Banished) || info.Banished;
			return flag4 && ((!other.IsPlayer && !other.IsWard) || flag || flag3) && flag5;
		}

		public CombatObject GetTargetCombat(BasePerk.PerkTarget target)
		{
			CombatObject result = null;
			if (target != BasePerk.PerkTarget.Owner)
			{
				if (target != BasePerk.PerkTarget.Target)
				{
					if (target == BasePerk.PerkTarget.Effect)
					{
						result = CombatRef.GetCombat(GameHubBehaviour.Hub.ObjectCollection.GetObjectByKind(ContentKind.Wards.Byte(), this.EventId));
					}
				}
				else
				{
					result = CombatRef.GetCombat(this.Target);
				}
			}
			else
			{
				result = this.Data.SourceCombat;
			}
			return result;
		}

		public Transform GetTargetTransform(BasePerk.PerkTarget target)
		{
			CombatObject targetCombat = this.GetTargetCombat(target);
			return (!(targetCombat == null)) ? targetCombat.Transform : base.transform;
		}

		public T GetComponentInTarget<T>(BasePerk.PerkTarget target, bool logInfoIfNotFound) where T : Component
		{
			CombatObject targetCombat = this.GetTargetCombat(target);
			return targetCombat.GetComponent<T>();
		}

		public static BaseFX GetFX(Component comp)
		{
			if (!comp)
			{
				return null;
			}
			BaseFX component = comp.GetComponent<BaseFX>();
			if (!component)
			{
				return null;
			}
			return component;
		}

		public static BaseFX GetFX(Collider col)
		{
			return BaseFX._collidersToFX[col];
		}

		public void Cleanup()
		{
			this._target = (this._owner = null);
			this._targetPosition = Vector3.zero;
		}

		public void OnSendToCache()
		{
			this.onCache = true;
		}

		public void OnGetFromCache()
		{
			this.onCache = false;
		}

		public Transform AttachedTransform
		{
			get
			{
				return this._transform;
			}
		}

		private void Awake()
		{
			if (this.m_boAwoken)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				if (this.VFX)
				{
					GameHubBehaviour.Hub.Resources.PrefabPreCache(this.VFX, 1);
				}
				if (this.SFX)
				{
					GameHubBehaviour.Hub.Resources.PrefabPreCache(this.SFX, 1);
				}
			}
			this.m_boAwoken = true;
			this._transform = base.transform;
			this._body = base.GetComponent<Rigidbody>();
			if (BaseFX._collidersToFX == null)
			{
				BaseFX._collidersToFX = new Dictionary<Collider, BaseFX>();
			}
			Collider[] componentsInChildren = base.gameObject.GetComponentsInChildren<Collider>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				BaseFX._collidersToFX.Add(componentsInChildren[i], this);
			}
			BasePerk[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<BasePerk>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				if (componentsInChildren2[j] is IPerkWithCollision)
				{
					this._myPerksWithCollision.Add((IPerkWithCollision)componentsInChildren2[j]);
				}
				if (componentsInChildren2[j] is IPerkWithDestruction)
				{
					this._destructionPerks.Add((IPerkWithDestruction)componentsInChildren2[j]);
				}
				if (componentsInChildren2[j] is IPerkMovement)
				{
					HeavyMetalMachines.Utils.Debug.Assert(this._movementPerk == null, string.Format("Effect={0} with more movement perks, not capable yet.", base.name), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
					this._movementPerk = (IPerkMovement)componentsInChildren2[j];
				}
			}
			this._skipChecks = (this._myPerksWithCollision.Count == 0 && this._movementPerk == null && this._destructionPerks.Count == 0);
			this._myPerksWithCollision.Sort((IPerkWithCollision a, IPerkWithCollision b) => a.Priority().CompareTo(b.Priority()));
			float internalRadius = PhysicsUtils.GetInternalRadius(base.GetComponent<Collider>());
			this.SetRadius((internalRadius < 0f) ? 1f : internalRadius);
			if (this.BombBlocker)
			{
				this._checkBombBlocker = true;
				this.BombBlocker.SetActive(false);
			}
		}

		private void Update()
		{
			if (this._skipChecks || this.IsDead)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				return;
			}
			if (this.Attached)
			{
				return;
			}
			this.InnerUpdate();
		}

		private void FixedUpdate()
		{
			if (this._skipChecks || this.IsDead || GameHubBehaviour.Hub.Global.LockAllPlayers)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (this._checkBombBlocker)
			{
				this.BombBlocker.SetActive((this.Attached && this.Attached.BombBlocked) || this.Gadget.IsBombBlocked(this));
			}
			this.TriggerCheck();
			this.InnerUpdate();
			this.CheckRemoveCollision();
		}

		private void InnerUpdate()
		{
			this._beforeLastPosition = this.LastPosition;
			if (this._positionReset)
			{
				this._beforeLastPosition = this._transform.position;
				this._positionReset = false;
			}
			Vector3 vector = this.CheckPosition();
			this.LastPosition = vector;
			Vector3 vector2 = vector;
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				this._transform.position = vector2;
			}
			else if (Time.deltaTime > 0f && this._canSetVelocity && this._body != null)
			{
				this._body.velocity = (vector2 - this._body.position) / Time.deltaTime;
			}
			for (int i = 0; i < this._destructionPerks.Count; i++)
			{
				this._destructionPerks[i].PerkUpdate();
			}
		}

		private Vector3 CheckPosition()
		{
			if (this._movementPerk == null)
			{
				return this._transform.position;
			}
			Vector3 vector = this._movementPerk.UpdatePosition();
			if (float.IsNaN(vector.x) || float.IsNaN(vector.y) || float.IsNaN(vector.z))
			{
				BaseFX.Log.ErrorFormat("NaN on newPosition: {0}, oldPosition: {1} movementType: {2} Attached: {3}", new object[]
				{
					vector,
					this._transform.position,
					this._movementPerk.GetType(),
					this.Attached
				});
				return this._transform.position;
			}
			if (this.Attached != null)
			{
				this.Attached.transform.position = vector;
			}
			return vector;
		}

		private void HitTriggers(List<BarrierUtils.CombatHit> hits)
		{
			for (int i = 0; i < hits.Count; i++)
			{
				Collider col = hits[i].Col;
				bool barrier = hits[i].Barrier;
				for (int j = 0; j < this._myPerksWithCollision.Count; j++)
				{
					IPerkWithCollision perkWithCollision = this._myPerksWithCollision[j];
					perkWithCollision.OnHit(col, Vector3.zero, Vector3.zero, true, barrier);
					perkWithCollision.OnStay(col, Vector3.zero, Vector3.zero, true, barrier);
				}
			}
		}

		private void TriggerCheck()
		{
			if (!base.enabled)
			{
				this._triggeringColliders.Clear();
				return;
			}
			this._nullCombatTriggeringColliders.Clear();
			for (int i = 0; i < this._triggeringColliders.Count; i++)
			{
				Collider collider = this._triggeringColliders[i];
				CombatObject combat = CombatRef.GetCombat(collider);
				if (combat == null)
				{
					this._nullCombatTriggeringColliders.Add(new BarrierUtils.CombatHit
					{
						Barrier = false,
						Col = collider,
						Combat = null
					});
				}
				else
				{
					this._triggerHits.Add(new BarrierUtils.CombatHit
					{
						Barrier = BarrierUtils.IsBarrier(collider),
						Col = collider,
						Combat = combat
					});
				}
			}
			if (this.Data.EffectInfo.PrioritizeBarrier)
			{
				BarrierUtils.FilterByBarrierPriority(this._triggerHits);
			}
			else
			{
				BarrierUtils.FilterByRaycastFromPoint(this._beforeLastPosition, this._triggerHits);
			}
			this.HitTriggers(this._nullCombatTriggeringColliders);
			this.HitTriggers(this._triggerHits);
			this._triggerHits.Clear();
			this._nullCombatTriggeringColliders.Clear();
			this._triggeringColliders.Clear();
		}

		protected virtual void OnTriggerStay(Collider other)
		{
			if (!base.enabled || this.IsDead)
			{
				return;
			}
			if (this._triggeringColliders.Contains(other))
			{
				return;
			}
			this._triggeringColliders.Add(other);
		}

		protected virtual void OnCollisionStay(Collision collisionInfo)
		{
			if (!base.enabled || this.IsDead)
			{
				return;
			}
			bool isBarrier = BarrierUtils.IsBarrier(collisionInfo.collider);
			for (int i = 0; i < this._myPerksWithCollision.Count; i++)
			{
				IPerkWithCollision perkWithCollision = this._myPerksWithCollision[i];
				perkWithCollision.OnHit(collisionInfo.collider, collisionInfo.contacts[0].point, collisionInfo.contacts[0].normal, true, isBarrier);
				perkWithCollision.OnStay(collisionInfo.collider, collisionInfo.contacts[0].point, collisionInfo.contacts[0].normal, true, isBarrier);
			}
		}

		protected virtual void OnTriggerEnter(Collider other)
		{
			if (!base.enabled || this.IsDead)
			{
				return;
			}
			this.EnterCollision(other);
			this.OnTriggerStay(other);
		}

		protected virtual void OnCollisionEnter(Collision collisionInfo)
		{
			if (!base.enabled || this.IsDead)
			{
				return;
			}
			this.EnterCollision(collisionInfo.collider);
			this.OnCollisionStay(collisionInfo);
		}

		protected virtual void OnTriggerExit(Collider other)
		{
			if (!base.enabled || this.IsDead)
			{
				return;
			}
			this.ExitCollision(other);
		}

		private void EnterCollision(Collider col)
		{
			CombatObject combat = CombatRef.GetCombat(col);
			bool isBarrier = BarrierUtils.IsBarrier(col);
			if (combat == null)
			{
				for (int i = 0; i < this._myPerksWithCollision.Count; i++)
				{
					IPerkWithCollision perkWithCollision = this._myPerksWithCollision[i];
					perkWithCollision.OnEnter(col, Vector3.zero, Vector3.zero, true, isBarrier);
				}
				return;
			}
			if (this._combatHits.ContainsKey(combat))
			{
				Dictionary<CombatObject, int> combatHits;
				CombatObject key;
				(combatHits = this._combatHits)[key = combat] = combatHits[key] + 1;
			}
			else
			{
				this._combatHits.Add(combat, 1);
				this._combatCollider.Add(combat, col);
				this._combatHitsList.Add(combat);
				for (int j = 0; j < this._myPerksWithCollision.Count; j++)
				{
					IPerkWithCollision perkWithCollision2 = this._myPerksWithCollision[j];
					perkWithCollision2.OnEnter(col, Vector3.zero, Vector3.zero, true, isBarrier);
				}
			}
		}

		private void ExitCollision(Collider col)
		{
			CombatObject combat = CombatRef.GetCombat(col);
			bool isBarrier = BarrierUtils.IsBarrier(col);
			if (combat == null)
			{
				for (int i = 0; i < this._myPerksWithCollision.Count; i++)
				{
					IPerkWithCollision perkWithCollision = this._myPerksWithCollision[i];
					perkWithCollision.OnExit(col, Vector3.zero, Vector3.zero, true, isBarrier);
				}
				return;
			}
			if (this._combatHits.ContainsKey(combat))
			{
				Dictionary<CombatObject, int> combatHits;
				CombatObject key;
				(combatHits = this._combatHits)[key = combat] = combatHits[key] - 1;
			}
			else
			{
				BaseFX.Log.Error(string.Format("Try remove collision with combat but combat: {0} are not found in list WHY?", combat));
			}
		}

		private void CheckRemoveCollision()
		{
			for (int i = 0; i < this._combatHitsList.Count; i++)
			{
				CombatObject combatObject = this._combatHitsList[i];
				if (this._combatHits[combatObject] < 1)
				{
					Collider collider = this._combatCollider[combatObject];
					bool isBarrier = BarrierUtils.IsBarrier(collider);
					for (int j = 0; j < this._myPerksWithCollision.Count; j++)
					{
						IPerkWithCollision perkWithCollision = this._myPerksWithCollision[j];
						perkWithCollision.OnExit(collider, Vector3.zero, Vector3.zero, true, isBarrier);
					}
					this._combatHits.Remove(combatObject);
					this._combatHitsList.Remove(combatObject);
					this._combatCollider.Remove(combatObject);
					i--;
				}
			}
		}

		public void OnAttachEffect(PerkAttachToObject.EffectAttachToTarget msg)
		{
			this._transform = ((!msg.Target) ? base.transform : msg.Target);
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(base.transform.position, this.GetRadius());
		}

		public void TriggerVFX(EffectEvent content)
		{
			if (GameHubBehaviour.Hub.Net.IsClient() || GameHubBehaviour.Hub.Net.IsTest())
			{
				if (this.VFX)
				{
					this.vfxInstance = (MasterVFX)GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(this.VFX, base.transform.position, base.transform.rotation);
					this.vfxInstance.transform.parent = GameHubBehaviour.Hub.Drawer.Effects;
					this.vfxInstance.Origin = content.Origin;
					this.vfxInstance.baseMasterVFX = this.VFX;
					this.vfxInstance = this.vfxInstance.Activate(this);
				}
				if (this.SFX)
				{
					this.sfxInstance = (MasterVFX)GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(this.SFX, base.transform.position, base.transform.rotation);
					this.sfxInstance.transform.parent = GameHubBehaviour.Hub.Drawer.Effects;
					this.sfxInstance.Origin = content.Origin;
					this.sfxInstance.baseMasterVFX = this.SFX;
					this.sfxInstance = this.sfxInstance.Activate(this);
				}
			}
		}

		public bool UseGadgetRadius;

		public static readonly BitLogger Log = new BitLogger(typeof(BaseFX));

		private bool onCache;

		public int Source;

		public GadgetBehaviour Gadget;

		private static Dictionary<Collider, BaseFX> _collidersToFX;

		public MasterVFX VFX;

		protected MasterVFX vfxInstance;

		public MasterVFX SFX;

		protected MasterVFX sfxInstance;

		public int DetachedVisualFXCount;

		private bool isSleeping;

		private Vector3 _beforeLastPosition;

		private bool _positionReset;

		public CombatObject Attached;

		private bool m_boAwoken;

		public EffectEvent Data;

		public EventData Event;

		public bool RedCreated;

		public bool BluCreated;

		[SerializeField]
		private bool _canSetVelocity = true;

		public GameObject BombBlocker;

		private bool _checkBombBlocker;

		private Identifiable _target;

		private Identifiable _owner;

		private readonly Dictionary<CombatObject, int> _combatHits = new Dictionary<CombatObject, int>();

		private readonly Dictionary<CombatObject, Collider> _combatCollider = new Dictionary<CombatObject, Collider>();

		private readonly List<CombatObject> _combatHitsList = new List<CombatObject>();

		private Vector3 _targetPosition = Vector3.zero;

		public BaseFX prefabRef;

		private bool _destroyTriggered;

		private List<IPerkWithCollision> _myPerksWithCollision = new List<IPerkWithCollision>(5);

		private List<IPerkWithDestruction> _destructionPerks = new List<IPerkWithDestruction>(3);

		private IPerkMovement _movementPerk;

		protected Transform _transform;

		protected Rigidbody _body;

		private float _radius;

		private bool _skipChecks;

		private List<BarrierUtils.CombatHit> _nullCombatTriggeringColliders = new List<BarrierUtils.CombatHit>(10);

		private List<Collider> _triggeringColliders = new List<Collider>(10);

		private List<BarrierUtils.CombatHit> _triggerHits = new List<BarrierUtils.CombatHit>(10);

		public enum EDestroyReason
		{
			None = -1,
			Default,
			HitScenery,
			HitIdentifiable,
			Lifetime,
			Gadget,
			LostTarget,
			Cleanup
		}

		public delegate void DestroyEffectListener();
	}
}
