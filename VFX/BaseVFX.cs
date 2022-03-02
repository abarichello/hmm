using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Infra;
using Pocketverse;

namespace HeavyMetalMachines.VFX
{
	public abstract class BaseVFX : ICachedAsset, IComparable<BaseVFX>
	{
		public virtual bool CanCollectToCache
		{
			get
			{
				return this._canCollectToCache;
			}
			protected set
			{
				this._canCollectToCache = value;
			}
		}

		public virtual int Priority
		{
			get
			{
				return 0;
			}
		}

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

		public void SetTargetFXInfo(MasterVFX.TargetFXInfo targetFXInfo)
		{
			this._targetFXInfo = targetFXInfo;
		}

		public void Activate(MasterVFX.TargetFXInfo targetInfo)
		{
			this._targetFXInfo = targetInfo;
			bool flag = true;
			if (this.CheckCondition != BaseVFX.CheckFunc.None)
			{
				if (this._targetFXInfo.Gadget != null)
				{
					BaseVFX.CheckFunc checkCondition = this.CheckCondition;
					if (checkCondition != BaseVFX.CheckFunc.Equals)
					{
						if (checkCondition != BaseVFX.CheckFunc.GreaterThan)
						{
							if (checkCondition == BaseVFX.CheckFunc.LessThan)
							{
								flag = (this._targetFXInfo.Gadget.GetLevel(this.UpgradeName) < this.Level);
							}
						}
						else
						{
							flag = (this._targetFXInfo.Gadget.GetLevel(this.UpgradeName) > this.Level);
						}
					}
					else
					{
						flag = (this._targetFXInfo.Gadget.GetLevel(this.UpgradeName) == this.Level);
					}
				}
				else
				{
					BaseVFX.Log.ErrorFormat("VFX tried to validate conditions but there was no Gadget information!\n{0}", new object[]
					{
						this._targetFXInfo
					});
					flag = false;
				}
			}
			if (flag)
			{
				this.OnActivate();
				this.m_boIsActive = true;
			}
		}

		public void SignalDeactivation()
		{
			this.WillDeactivate();
		}

		public void Destroy(EffectRemoveEvent removeEvent)
		{
			if (this.m_boIsActive)
			{
				this.removeEvent = removeEvent;
				this.OnDeactivate();
				this.m_boIsActive = false;
				this._targetFXInfo.Clear();
			}
		}

		protected abstract void OnActivate();

		protected abstract void WillDeactivate();

		protected abstract void OnDeactivate();

		public int CompareTo(BaseVFX other)
		{
			return (this.Priority != other.Priority) ? ((this.Priority <= other.Priority) ? 1 : -1) : 0;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BaseVFX));

		protected bool _canCollectToCache = true;

		public BaseVFX.CheckFunc CheckCondition;

		public string UpgradeName;

		public int Level;

		public string PerkVFXCondition;

		[NonSerialized]
		public bool m_boIsActive;

		public bool onlyForSelf;

		public float delay;

		public BaseVFX.SpawnState spawnStage;

		public BaseVFX.CheckReasonMode checkReasonMode;

		public BaseFX.EDestroyReason destroyReason;

		private bool previzMode;

		private VFXTeam currentTeam;

		protected MasterVFX.TargetFXInfo _targetFXInfo;

		protected EffectRemoveEvent removeEvent;

		public enum SpawnState
		{
			OnActivate,
			OnDestroy
		}

		public enum CheckFunc
		{
			None,
			Equals,
			GreaterThan,
			LessThan
		}

		public enum CheckReasonMode
		{
			DefaultOrReason,
			ReasonOnly
		}
	}
}
