using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class Upgradeable
	{
		public Upgradeable(string name, bool baseValue, UpgradeableValue[] upgrades)
		{
			this.Setup(name, (float)Convert.ToInt32(baseValue), upgrades);
		}

		public Upgradeable(string name, StatusKind targetStausKind, UpgradeableValue[] upgrades)
		{
			this.Setup(name, (float)targetStausKind, upgrades);
			if (this.Upgrades != null && this.Upgrades.Values != null)
			{
				this.Value = 0f;
				for (int i = 0; i < this.Upgrades.Values.Length; i++)
				{
					this.Upgrades.Values[i] = (float)targetStausKind;
				}
			}
		}

		public Upgradeable(string name, float baseValue, UpgradeableValue[] upgrades)
		{
			this.Setup(name, baseValue, upgrades);
		}

		public Upgradeable SetLevel(string upgradeName, int level)
		{
			if (this.Upgrades != null && !string.IsNullOrEmpty(this.Upgrades.Name) && this.Upgrades.Name.StartsWith(upgradeName))
			{
				this.Level = level;
			}
			return this;
		}

		public static implicit operator float(Upgradeable up)
		{
			return (up != null) ? up.Get() : 0f;
		}

		public float Get()
		{
			if (this.Level == 0 || this.Upgrades == null || this.Upgrades.Values.Length < this.Level)
			{
				return this.Value;
			}
			return this.Upgrades.Values[this.Level - 1];
		}

		public int IntGet()
		{
			return (int)this.Get();
		}

		public bool BoolGet()
		{
			return Convert.ToBoolean(this.Get());
		}

		public StatusKind StatusGet()
		{
			return (StatusKind)this.Get();
		}

		private void Setup(string name, float baseValue, UpgradeableValue[] upgrades)
		{
			this.Value = baseValue;
			this.Upgrades = ((upgrades != null) ? Array.Find<UpgradeableValue>(upgrades, (UpgradeableValue x) => string.Compare(x.Name, name, StringComparison.Ordinal) == 0) : null);
			this.Level = 0;
		}

		public UpgradeableValue Upgrades;

		public float Value;

		public int Level;
	}
}
