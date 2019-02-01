using System;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkCreateLink : BasePerk
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			BasicLink basicLink = this.Effect.Gadget as BasicLink;
			if (basicLink == null)
			{
				PerkCreateLink.Log.ErrorFormat("Gadget is null or not a BasicLink! Owner: {0}", new object[]
				{
					this.Effect.Data.SourceCombat
				});
				return;
			}
			CombatObject targetCombat = base.GetTargetCombat(this.Effect, this.target);
			if (targetCombat == null)
			{
				PerkCreateLink.Log.ErrorFormat("CombatObject is null! Owner: {0}, Effect: {1}", new object[]
				{
					this.Effect.Data.SourceCombat,
					this.Effect
				});
				return;
			}
			if (!this.Effect.CheckHit(targetCombat))
			{
				return;
			}
			this._target = targetCombat.Movement;
			this._owner = this.Effect.Data.SourceCombat.Movement;
			if (this._target == null || this._owner == null)
			{
				PerkCreateLink.Log.ErrorFormat("One of the hooks of the link is null! Owner: {0}. Target: {1}. Gadget: {2}", new object[]
				{
					this._target,
					this._owner,
					basicLink
				});
				return;
			}
			if (this._target == this._owner)
			{
				PerkCreateLink.Log.ErrorFormat("Can't create a link of a Combat with itself! Owner: {0}. Gadget: {1}. Effect: {2}.", new object[]
				{
					this._owner,
					basicLink,
					this.Effect
				});
				return;
			}
			this.ConfigureOptions(basicLink);
			this.ConfigureMass(basicLink);
			this.StealLink |= (basicLink != null && basicLink.StealLink);
			if (this.TargetMass <= 0f || this.OwnerMass <= 0f || ((this.TargetStrugglingMass <= 0f || this.OwnerStrugglingMass <= 0f) && string.IsNullOrEmpty(this.Tag)))
			{
				PerkCreateLink.Log.ErrorFormat("Cannot create a Link with one of the points with mass (or struggling mass if struggle is possible) 0. Owner: {0}. Target: {1}. OwnerMass: {2}. TargetMass: {3}. OwnerStrugglingMass: {4}. TargetStrugglingMass: {5}.", new object[]
				{
					this._owner,
					this._target,
					this.OwnerMass,
					this.TargetMass,
					this.OwnerStrugglingMass,
					this.TargetStrugglingMass
				});
				return;
			}
			if (this.OwnerDummy == CDummy.DummyKind.None)
			{
				this.OwnerDummy = basicLink.MyInfo.OwnerDummy;
				this.OwnerCustomDummyName = basicLink.MyInfo.OwnerCustomDummyName;
			}
			if (this.TargetDummy == CDummy.DummyKind.None)
			{
				this.TargetDummy = basicLink.MyInfo.TargetDummy;
				this.TargetCustomDummyName = basicLink.MyInfo.TargetCustomDummyName;
			}
			CombatLink.LinkHook point = new CombatLink.LinkHook(this._owner.Combat.GetDummy(this.OwnerDummy, this.OwnerCustomDummyName), this._owner, this.OwnerMass, this.OwnerStrugglingMass);
			CombatLink.LinkHook point2 = new CombatLink.LinkHook(this._target.Combat.GetDummy(this.TargetDummy, this.TargetCustomDummyName), this._target, this.TargetMass, this.TargetStrugglingMass);
			string tag = this.Tag;
			if (string.IsNullOrEmpty(this.Tag))
			{
				tag = base.name;
			}
			this._link = new CombatLink(point, point2, this.Range, this.Compression, this.Tension, this.TensionBreakForce, this.ClampIn, this.ClampOut, this.ClampOnCorners, tag);
			this._owner.AddLink(this._link, this.StealLink);
			this._target.AddLink(this._link, this.StealLink);
			LinkCreatedCallback.ILinkCreatedCallbackListener linkCreatedCallbackListener = this.GetTargetGadget() as LinkCreatedCallback.ILinkCreatedCallbackListener;
			if (linkCreatedCallbackListener != null)
			{
				linkCreatedCallbackListener.OnLinkCreatedCallback(new LinkCreatedCallback(this._owner.Combat, this._target.Combat, this._link));
			}
			this._fixedTimeStart = Time.fixedTime;
			this._changeIndex = 0;
		}

		private GadgetBehaviour GetTargetGadget()
		{
			switch (this.TargetGadgetCallback)
			{
			case TargetGadget.Gadget0:
				return this._owner.Combat.CustomGadget0;
			case TargetGadget.Gadget1:
				return this._owner.Combat.CustomGadget1;
			case TargetGadget.Gadget2:
				return this._owner.Combat.CustomGadget2;
			case TargetGadget.GadgetBoost:
				return this._owner.Combat.BoostGadget;
			}
			return this.Effect.Gadget;
		}

		public void FixedUpdate()
		{
			if (this._link == null)
			{
				return;
			}
			if (this._link.IsBroken)
			{
				this.Effect.TriggerDefaultDestroy(-1);
				return;
			}
			if (this._changeIndex >= this.TensionChanges.Length)
			{
				return;
			}
			float num = Time.fixedTime - this._fixedTimeStart;
			PerkCreateLink.TensionBreakForceChange tensionBreakForceChange = this.TensionChanges[this._changeIndex];
			if (tensionBreakForceChange.TimeSeconds > num)
			{
				return;
			}
			this._changeIndex++;
			this._link.TensionBreakForce = tensionBreakForceChange.TensionBreakForce;
		}

		public override void PerkDestroyed(DestroyEffect destroyEffect)
		{
			if (this._link != null)
			{
				this._owner.RemoveLink(this._link);
				this._target.RemoveLink(this._link);
				this._link.Break();
			}
		}

		private void ConfigureMass(BasicLink gadget)
		{
			if (this.OwnerMass <= 0f && gadget != null)
			{
				this.OwnerMass = gadget.OwnerMass;
			}
			if (this.TargetMass <= 0f && gadget != null)
			{
				this.TargetMass = gadget.TargetMass;
			}
			if (this.OwnerStrugglingMass <= 0f && gadget != null)
			{
				this.OwnerStrugglingMass = gadget.OwnerStrugglingMass;
			}
			if (this.TargetStrugglingMass <= 0f && gadget != null)
			{
				this.TargetStrugglingMass = gadget.TargetStrugglingMass;
			}
		}

		private void ConfigureOptions(BasicLink gadget)
		{
			if (this.Range <= 0f)
			{
				this.Range = ((!this.UseGadgetRange) ? this.Effect.Data.Range : this.Effect.Gadget.GetRange());
			}
			if (this.Compression <= 0f && gadget != null)
			{
				this.Compression = gadget.MyInfo.Compression;
			}
			if (string.IsNullOrEmpty(this.Tag) && gadget != null)
			{
				this.Tag = gadget.MyInfo.TagLink;
			}
			if (this.Tension <= 0f && gadget != null)
			{
				this.Tension = gadget.MyInfo.Tension;
			}
			if (gadget != null)
			{
				this.ClampIn |= gadget.MyInfo.ClampIn;
			}
			if (gadget != null)
			{
				this.ClampOut |= gadget.MyInfo.ClampOut;
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkCreateLink));

		public CDummy.DummyKind OwnerDummy;

		public string OwnerCustomDummyName;

		public CDummy.DummyKind TargetDummy;

		public string TargetCustomDummyName;

		public BasePerk.PerkTarget target = BasePerk.PerkTarget.Target;

		public TargetGadget TargetGadgetCallback;

		public string Tag;

		public bool StealLink;

		public float Range;

		public bool UseGadgetRange;

		public float Compression;

		public float Tension;

		public float TensionBreakForce;

		public bool ClampIn;

		public bool ClampOut;

		public bool ClampOnCorners = true;

		public float OwnerMass;

		public float TargetMass;

		public float OwnerStrugglingMass;

		public float TargetStrugglingMass;

		public PerkCreateLink.TensionBreakForceChange[] TensionChanges;

		private CombatMovement _target;

		private CombatMovement _owner;

		private CombatLink _link;

		private float _fixedTimeStart;

		private int _changeIndex;

		[Serializable]
		public class TensionBreakForceChange
		{
			public float TimeSeconds;

			public float TensionBreakForce;
		}
	}
}
