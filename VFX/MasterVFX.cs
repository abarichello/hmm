using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[HelpURL("https://confluence.hoplon.com/display/HMM/Master+VFX")]
	public class MasterVFX : GameHubBehaviour
	{
		public virtual bool PrevizMode
		{
			get
			{
				return this.previzMode;
			}
			set
			{
				this.previzMode = value;
			}
		}

		public virtual VFXTeam CurrentTeam
		{
			get
			{
				return this.currentTeam;
			}
			set
			{
				this.currentTeam = value;
			}
		}

		public MasterVFX.TargetFXInfo GetTargetInfoForStateRecording
		{
			get
			{
				return this._targetFXInfo;
			}
		}

		public float StartTime
		{
			get
			{
				return this.startTime;
			}
		}

		private void Awake()
		{
			this.expireTime = ((this.expireTime <= 0f) ? float.MaxValue : this.expireTime);
			for (int i = 0; i < this.associatedVFX.Length; i++)
			{
				BaseVFX baseVFX = this.associatedVFX[i];
				if (baseVFX == null)
				{
					MasterVFX.Log.ErrorFormat("Incoherent associated VFX list on {0}", new object[]
					{
						base.gameObject.name
					});
				}
				else if (!string.IsNullOrEmpty(baseVFX.PerkVFXCondition))
				{
					List<BaseVFX> list;
					if (this.m_cConditionVFX.ContainsKey(baseVFX.PerkVFXCondition))
					{
						list = this.m_cConditionVFX[baseVFX.PerkVFXCondition];
					}
					else
					{
						list = new List<BaseVFX>();
						this.m_cConditionVFX.Add(baseVFX.PerkVFXCondition, list);
					}
					list.Add(baseVFX);
				}
			}
		}

		private void CollectVFX()
		{
			if (GameHubBehaviour.Hub)
			{
				GameHubBehaviour.Hub.Resources.ReturnToPrefabCache(this.baseMasterVFX, this);
			}
			this.TargetFX = null;
			this._targetFXInfo.Clear();
		}

		public void Activate(string strCondition)
		{
			List<BaseVFX> list = null;
			bool isSelf = this.IsSelf();
			if (this.m_cConditionVFX.TryGetValue(strCondition, out list))
			{
				int i = 0;
				int count = list.Count;
				while (i < count)
				{
					BaseVFX baseVFX = list[i];
					if (!baseVFX.m_boIsActive)
					{
						this.ActivateEffectInstance(baseVFX, isSelf, false);
					}
					i++;
				}
			}
		}

		public void Deactivate(string strCondition)
		{
			List<BaseVFX> list = null;
			if (this.m_cConditionVFX.TryGetValue(strCondition, out list))
			{
				int i = 0;
				int count = list.Count;
				while (i < count)
				{
					BaseVFX baseVFX = list[i];
					if (baseVFX.m_boIsActive)
					{
						this.DeactivateEffectInstance(list[i]);
					}
					i++;
				}
			}
		}

		public virtual MasterVFX Activate(Identifiable owner, Identifiable target, Transform transform)
		{
			this.wasDestroyedStatusForStateRecording = false;
			this._targetFXInfo.Owner = owner;
			this._targetFXInfo.Target = target;
			this._targetFXInfo.Origin = this.Origin;
			this._targetFXInfo.EffectTransform = transform;
			for (int i = 0; i < this.associatedVFX.Length; i++)
			{
				BaseVFX baseVFX = this.associatedVFX[i];
				baseVFX.SetTargetFXInfo(this._targetFXInfo);
			}
			if (this.TeamRequirement == MasterVFX.TargetTeam.LocalPlayerOnly && !this._targetFXInfo.Gadget.Combat.IsLocalPlayer)
			{
				this.CollectVFX();
				return this;
			}
			if (this.TeamRequirement != MasterVFX.TargetTeam.Any && owner != null && target != null)
			{
				CombatObject bitComponent = owner.GetBitComponent<CombatObject>();
				CombatObject bitComponent2 = target.GetBitComponent<CombatObject>();
				if (bitComponent == null || bitComponent2 == null)
				{
					this.CollectVFX();
					return this;
				}
				MasterVFX.TargetTeam teamRequirement = this.TeamRequirement;
				if (teamRequirement != MasterVFX.TargetTeam.OwnerTeam)
				{
					if (teamRequirement == MasterVFX.TargetTeam.NotOwnerTeam)
					{
						if (bitComponent.Team == bitComponent2.Team)
						{
							this.CollectVFX();
							return this;
						}
					}
				}
				else if (bitComponent.Team != bitComponent2.Team)
				{
					this.CollectVFX();
					return this;
				}
			}
			this.shouldDeactivate = false;
			this.currentState = MasterVFX.State.Activating;
			bool isSelf = this.IsSelf();
			this.startTime = Time.time;
			if (this.delayedFX == null)
			{
				this.delayedFX = new List<BaseVFX>();
			}
			for (int j = 0; j < this.associatedVFX.Length; j++)
			{
				BaseVFX baseVFX2 = this.associatedVFX[j];
				if (string.IsNullOrEmpty(baseVFX2.PerkVFXCondition))
				{
					this.ActivateEffectInstance(baseVFX2, isSelf, true);
				}
			}
			this.delayedFX.Sort(new Comparison<BaseVFX>(this.SortDelayedList));
			return this;
		}

		public virtual MasterVFX Activate(AbstractFX target)
		{
			this.removeEvent = null;
			this.TargetFX = target;
			this._targetFXInfo.Gadget = target.GetGadget();
			return this.Activate(target.Owner, target.Target, target.transform);
		}

		public virtual MasterVFX Activate(AbstractFX target, Transform fakeTransform)
		{
			this.removeEvent = null;
			this.TargetFX = target;
			this._targetFXInfo.Gadget = target.GetGadget();
			return this.Activate(target.Owner, target.Target, fakeTransform);
		}

		private int SortDelayedList(BaseVFX x, BaseVFX y)
		{
			return x.delay.CompareTo(y.delay);
		}

		private bool IsSelf()
		{
			bool result = true;
			if (GameHubBehaviour.Hub == null)
			{
				return true;
			}
			if (GameHubBehaviour.Hub.Players == null)
			{
				return true;
			}
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData == null)
			{
				return true;
			}
			if (this._targetFXInfo.Target != null)
			{
				result = (this._targetFXInfo.Target.ObjId == GameHubBehaviour.Hub.Players.CurrentPlayerData.GetPlayerCarObjectId());
			}
			return result;
		}

		private void ActivateEffectInstance(BaseVFX vfx, bool isSelf, bool boAllowDelay)
		{
			if (vfx.spawnStage != BaseVFX.SpawnState.OnActivate)
			{
				return;
			}
			if (vfx.onlyForSelf && !isSelf)
			{
				return;
			}
			if (vfx.delay <= 0f || !boAllowDelay)
			{
				vfx.PrevizMode = this.previzMode;
				vfx.CurrentTeam = this.currentTeam;
				vfx.Activate(this._targetFXInfo);
			}
			else
			{
				this.delayedFX.Add(vfx);
			}
		}

		private void DeactivateEffectInstance(BaseVFX vfx)
		{
			if (vfx.spawnStage != BaseVFX.SpawnState.OnDestroy)
			{
				vfx.Destroy(this.removeEvent);
			}
			else if (vfx.checkReasonMode == BaseVFX.CheckReasonMode.ReasonOnly)
			{
				if (vfx.destroyReason == this.destroyReason)
				{
					vfx.Activate(this._targetFXInfo);
				}
			}
			else if (vfx.destroyReason == BaseFX.EDestroyReason.Default || vfx.destroyReason == this.destroyReason)
			{
				vfx.Activate(this._targetFXInfo);
			}
		}

		private void Update()
		{
			switch (this.currentState)
			{
			case MasterVFX.State.Activating:
				this.currentState = MasterVFX.State.Activated;
				break;
			case MasterVFX.State.Activated:
				break;
			case MasterVFX.State.Deactivating:
				goto IL_64;
			case MasterVFX.State.Hidden:
				return;
			case MasterVFX.State.WaitingForCollect:
				goto IL_C6;
			default:
				return;
			}
			if (!this.shouldDeactivate || Time.time - this.startTime < this.minDuration)
			{
				this.ActivateDelayedVFX();
				return;
			}
			IL_64:
			if (this.delayedFX != null)
			{
				this.delayedFX.Clear();
			}
			for (int i = 0; i < this.associatedVFX.Length; i++)
			{
				BaseVFX vfx = this.associatedVFX[i];
				this.DeactivateEffectInstance(vfx);
			}
			if (this.DontUseCache)
			{
				this.currentState = MasterVFX.State.Deactivated;
				return;
			}
			this.currentState = MasterVFX.State.WaitingForCollect;
			IL_C6:
			bool flag = true;
			for (int j = 0; j < this.associatedVFX.Length; j++)
			{
				BaseVFX baseVFX = this.associatedVFX[j];
				if (!baseVFX.CanCollectToCache)
				{
					flag = false;
					break;
				}
			}
			if (flag || Time.time - this.startTime > this.expireTime)
			{
				this.currentState = MasterVFX.State.Deactivated;
				this.CollectVFX();
			}
		}

		private void ActivateDelayedVFX()
		{
			if (this.delayedFX == null || this.delayedFX.Count == 0)
			{
				return;
			}
			while (this.delayedFX[0].delay < Time.time - this.startTime)
			{
				this.ActivateEffectInstance(this.delayedFX[0], this.IsSelf(), false);
				this.delayedFX.RemoveAt(0);
				if (this.delayedFX.Count == 0)
				{
					break;
				}
			}
		}

		public void Destroy(BaseFX.EDestroyReason reason)
		{
			this.wasDestroyedStatusForStateRecording = true;
			this.destroyReason = reason;
			this.shouldDeactivate = true;
			for (int i = 0; i < this.associatedVFX.Length; i++)
			{
				this.associatedVFX[i].SignalDeactivation();
			}
		}

		public void Destroy(EffectRemoveEvent data)
		{
			this.removeEvent = data;
			this.Destroy(this.removeEvent.DestroyReason);
		}

		public override string ToString()
		{
			return string.Format("{0}, BaseMasterVfx: {1}, VfxTarget: {2}, AssociatedVfx: {3}", new object[]
			{
				base.ToString(),
				this.baseMasterVFX,
				this.TargetFX,
				this.associatedVFX
			});
		}

		private void OnValidate()
		{
			BaseVFX[] componentsInChildren = base.gameObject.GetComponentsInChildren<BaseVFX>(true);
			Array.Sort<BaseVFX>(componentsInChildren);
			this.associatedVFX = componentsInChildren;
		}

		public Identifiable GetOwner()
		{
			return this._targetFXInfo.Owner;
		}

		public void PrestartForpreviz()
		{
			for (int i = 0; i < this.associatedVFX.Length; i++)
			{
				BaseVFX baseVFX = this.associatedVFX[i];
				if (baseVFX.GetType() == typeof(BillboardVFX))
				{
					BaseVFX baseVFX2 = baseVFX;
					baseVFX2.Activate(this._targetFXInfo);
				}
			}
		}

		public static void EditorAssertMinDurationOrWaitForParticlesDeath(MasterVFX vfx)
		{
		}

		private static readonly BitLogger Log = new BitLogger(typeof(MasterVFX));

		[HideInInspector]
		public MasterVFX baseMasterVFX;

		public BaseVFX[] associatedVFX;

		[HideInInspector]
		public List<BaseVFX> delayedFX;

		[HideInInspector]
		private float startTime;

		public float minDuration;

		public float expireTime;

		public bool DontUseCache;

		private bool previzMode;

		private VFXTeam currentTeam;

		public MasterVFX.TargetTeam TeamRequirement;

		[HideInInspector]
		[NonSerialized]
		public MasterVFX.State currentState = MasterVFX.State.Deactivated;

		[HideInInspector]
		public BaseFX.EDestroyReason destroyReason;

		protected EffectRemoveEvent removeEvent;

		protected bool shouldDeactivate;

		public AbstractFX TargetFX;

		protected MasterVFX.TargetFXInfo _targetFXInfo;

		private Dictionary<string, List<BaseVFX>> m_cConditionVFX = new Dictionary<string, List<BaseVFX>>();

		public Vector3 Origin;

		public bool wasDestroyedStatusForStateRecording;

		public enum State
		{
			Activating,
			Activated,
			Deactivating,
			Hidden,
			WaitingForCollect,
			Deactivated
		}

		public enum TargetTeam
		{
			Any,
			OwnerTeam,
			NotOwnerTeam,
			LocalPlayerOnly
		}

		public struct TargetFXInfo
		{
			public void Clear()
			{
				this.Owner = null;
				this.Target = null;
				this.EffectTransform = null;
				this.Gadget = null;
			}

			public override string ToString()
			{
				return string.Format("TargetFXInfo:[Owner:{0},Target:{1},EffectTransform:{2},Gadget:{3}]", new object[]
				{
					this.Owner,
					this.Target,
					this.EffectTransform,
					this.Gadget
				});
			}

			public Identifiable Owner;

			public Identifiable Target;

			public Transform EffectTransform;

			public GadgetBehaviour Gadget;

			public Vector3 Origin;
		}
	}
}
