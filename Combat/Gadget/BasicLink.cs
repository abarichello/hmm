using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BasicLink : BasicCannon
	{
		public BasicLinkInfo MyInfo
		{
			get
			{
				return base.Info as BasicLinkInfo;
			}
		}

		public float OwnerMass
		{
			get
			{
				return this._ownerMass.Get();
			}
		}

		public float TargetMass
		{
			get
			{
				return this._targetMass.Get();
			}
		}

		public float OwnerStrugglingMass
		{
			get
			{
				return this._ownerStrugglingMass.Get();
			}
		}

		public float TargetStrugglingMass
		{
			get
			{
				return this._targetStrugglingMass.Get();
			}
		}

		public bool StealLink
		{
			get
			{
				return this._stealLink.BoolGet();
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._ownerMass = new Upgradeable(this.MyInfo.OwnerMassUpgrade, this.MyInfo.OwnerMass, this.MyInfo.UpgradesValues);
			this._targetMass = new Upgradeable(this.MyInfo.TargetMassUpgrade, this.MyInfo.TargetMass, this.MyInfo.UpgradesValues);
			this._stealLink = new Upgradeable(this.MyInfo.StealLinkUpgrade, this.MyInfo.StealLink, this.MyInfo.UpgradesValues);
			this._ownerStrugglingMass = new Upgradeable(this.MyInfo.OwnerMassWhenStrugglingUpgrade, this.MyInfo.OwnerMassWhenStruggling, this.MyInfo.UpgradesValues);
			this._targetStrugglingMass = new Upgradeable(this.MyInfo.TargetMassWhenStrugglingUpgrade, this.MyInfo.TargetMassWhenStruggling, this.MyInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._ownerMass.SetLevel(upgradeName, level);
			this._targetMass.SetLevel(upgradeName, level);
			this._stealLink.SetLevel(upgradeName, level);
			this._ownerStrugglingMass.SetLevel(upgradeName, level);
			this._targetStrugglingMass.SetLevel(upgradeName, level);
		}

		protected Upgradeable _ownerMass;

		protected Upgradeable _targetMass;

		protected Upgradeable _ownerStrugglingMass;

		protected Upgradeable _targetStrugglingMass;

		protected Upgradeable _stealLink;
	}
}
