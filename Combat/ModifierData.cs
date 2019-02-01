using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HeavyMetalMachines.Combat.Gadget;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[Serializable]
	public class ModifierData
	{
		public ModifierData(ModifierData other)
		{
			this.GadgetInfo = other.GadgetInfo;
			this.Info = other.Info;
			this._lifeTime = new Upgradeable(string.Empty, other.LifeTime, null);
			this._tickDelta = new Upgradeable(string.Empty, other.TickDelta, null);
			this._amount = new Upgradeable(string.Empty, other.Amount, null);
			this._status = new Upgradeable(string.Empty, other.Status, null);
			this.BuffCharges = other.BuffCharges;
			this.Direction = other.Direction;
			this.DirectionSet = other.DirectionSet;
			this.Position = other.Position;
			this.PositionSet = other.PositionSet;
		}

		public ModifierData(ModifierInfo info)
		{
			this.GadgetInfo = null;
			this.Info = info;
			this.SetUpgrades(info, null);
		}

		public ModifierData(ModifierInfo info, GadgetInfo gadget)
		{
			this.GadgetInfo = gadget;
			this.Info = info;
			this.SetUpgrades(info, (!(gadget == null)) ? gadget.UpgradesValues : null);
		}

		public bool IsReactive
		{
			get
			{
				return this.Info.IsReactive;
			}
		}

		public int LifeTimeLevel
		{
			get
			{
				return this._lifeTime.Level;
			}
		}

		public float LifeTime
		{
			get
			{
				return this._lifeTime.Get();
			}
		}

		public StatusKind Status
		{
			get
			{
				return this._status.StatusGet();
			}
		}

		public float TickDelta
		{
			get
			{
				return this._tickDelta.Get();
			}
		}

		public int AmountLevel
		{
			get
			{
				return this._amount.Level;
			}
		}

		public float Amount
		{
			get
			{
				return (!this.IsSum) ? this._amount.Get() : this.SumAmount;
			}
		}

		public void SetLevel(string upgradeName, int level)
		{
			this._lifeTime.SetLevel(upgradeName, level);
			this._tickDelta.SetLevel(upgradeName, level);
			this._amount.SetLevel(upgradeName, level);
			this._status.SetLevel(upgradeName, level);
		}

		public void SetInfo(ModifierInfo info, GadgetInfo gadget)
		{
			this.GadgetInfo = gadget;
			this.Info = info;
			this.SetUpgrades(info, (!(gadget == null)) ? gadget.UpgradesValues : null);
		}

		public void SetUpgrades(ModifierInfo info, UpgradeableValue[] upgrades)
		{
			this._lifeTime = new Upgradeable(this.Info.LifeTimeUpgrade, this.Info.LifeTime, upgrades);
			this._tickDelta = new Upgradeable(this.Info.TickDeltaUpgrade, this.Info.TickDelta, upgrades);
			this._amount = new Upgradeable(this.Info.AmountUpgrade, this.Info.Amount, upgrades);
			this._status = new Upgradeable(this.Info.StatusUpgrade, this.Info.Status, upgrades);
		}

		public static ModifierData[] CreateConvoluted(ModifierData[] baseData, float baseAmount)
		{
			ModifierData[] array = ModifierData.CopyData(baseData);
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].Info.NotConvoluted)
				{
					if (array[i].Info.UseAmountCurveMultiplier)
					{
						float num = array[i].Info.AmountCurveMultiplier.Evaluate(baseAmount);
						array[i]._amount.Value *= num;
					}
					else
					{
						array[i]._amount.Value *= baseAmount;
					}
				}
			}
			return array;
		}

		public static IEnumerable<ModifierData> GetFiltered(ModifierData[] baseData, Func<ModifierData, bool> predicate)
		{
			ModifierData[] source = ModifierData.CopyData(baseData);
			return source.Where(predicate);
		}

		public static ModifierData[] CreateConvolutedFiltering(ModifierData[] baseData, float baseAmount, Func<ModifierData, bool> predicate)
		{
			ModifierData[] array = ModifierData.CopyData(baseData);
			IEnumerable<ModifierData> enumerable = array.Where(predicate);
			foreach (ModifierData modifierData in enumerable)
			{
				if (!modifierData.Info.NotConvoluted)
				{
					modifierData._amount.Value *= baseAmount;
				}
			}
			return array;
		}

		public static ModifierData[] SplitAmount(ModifierData[] baseData, float value)
		{
			value = 1f / value;
			ModifierData[] array = ModifierData.CopyData(baseData);
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].Info.NotConvoluted)
				{
					array[i]._amount.Value *= value;
				}
			}
			return array;
		}

		public static ModifierData[] RemoveAmountPercent(ModifierData[] baseData, float value)
		{
			ModifierData[] array = ModifierData.CopyData(baseData);
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].Info.NotConvoluted)
				{
					array[i]._amount.Value = array[i]._amount.Value - array[i]._amount.Value * value;
				}
			}
			return array;
		}

		public static ModifierData[] AddAmountPercent(ModifierData[] baseData, float value)
		{
			ModifierData[] array = ModifierData.CopyData(baseData);
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].Info.NotConvoluted)
				{
					array[i]._amount.Value = array[i]._amount.Value + array[i]._amount.Value * value;
				}
			}
			return array;
		}

		public static ModifierData[] AddAmount(ModifierData[] baseData, float value, Func<ModifierData, bool> filter = null)
		{
			ModifierData[] array = ModifierData.CopyData(baseData);
			for (int i = 0; i < array.Length; i++)
			{
				if (!array[i].Info.NotConvoluted && (filter == null || filter(array[i])))
				{
					array[i]._amount.Value += value;
				}
			}
			return array;
		}

		public static ModifierData[] CopyData(ModifierData[] data)
		{
			ModifierData[] array = new ModifierData[data.Length];
			for (int i = 0; i < data.Length; i++)
			{
				array[i] = new ModifierData(data[i]);
			}
			return array;
		}

		public static ModifierData[] CreateData(ModifierInfo[] infos, GadgetInfo gadget)
		{
			if (infos == null || infos.Length == 0)
			{
				return new ModifierData[0];
			}
			ModifierData[] array = new ModifierData[infos.Length];
			for (int i = 0; i < infos.Length; i++)
			{
				ModifierInfo info = infos[i];
				array[i] = new ModifierData(info, gadget);
			}
			return array;
		}

		public static ModifierData[] CreateData(ModifierInfo[] infos, float amount)
		{
			if (infos == null || infos.Length == 0)
			{
				return new ModifierData[0];
			}
			ModifierData[] array = new ModifierData[infos.Length];
			for (int i = 0; i < infos.Length; i++)
			{
				ModifierInfo info = infos[i];
				array[i] = new ModifierData(info);
				array[i]._amount.Value = amount;
			}
			return array;
		}

		public static ModifierData[] CreateData(ModifierInfo[] infos)
		{
			if (infos == null || infos.Length == 0)
			{
				return new ModifierData[0];
			}
			ModifierData[] array = new ModifierData[infos.Length];
			for (int i = 0; i < infos.Length; i++)
			{
				ModifierInfo info = infos[i];
				array[i] = new ModifierData(info);
			}
			return array;
		}

		public static ModifierData[] StandardDirectionAndPosition(ModifierData[] baseData, Vector3 origin, Vector3 targetPosition)
		{
			Vector3 normalized = (targetPosition - origin).normalized;
			ModifierData[] array = ModifierData.CopyData(baseData);
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetDirection(normalized);
				array[i].SetPosition(origin);
			}
			return array;
		}

		public void ForceLifeTime(string lifeTimeUpgrade, float lifeTime)
		{
			UpgradeableValue[] upgrades = (!this.GadgetInfo) ? null : this.GadgetInfo.UpgradesValues;
			this._lifeTime = new Upgradeable(lifeTimeUpgrade, lifeTime, upgrades);
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Mod={");
			if (this.Info.Effect != EffectKind.None)
			{
				stringBuilder.Append(this.Info.Effect);
			}
			else if (this.Info.Attribute != AttributeBuffKind.None)
			{
				stringBuilder.Append(this.Info.Attribute);
			}
			else if (this.Info.Status != StatusKind.None)
			{
				stringBuilder.Append(this.Info.Status);
			}
			stringBuilder.Append("-");
			stringBuilder.Append(this.Amount);
			stringBuilder.Append("-");
			stringBuilder.Append(this.LifeTime);
			stringBuilder.Append("}");
			return stringBuilder.ToString();
		}

		public static readonly ModifierData[] EmptyArray = new ModifierData[0];

		public ModifierInfo Info;

		public GadgetInfo GadgetInfo;

		private Upgradeable _lifeTime;

		private Upgradeable _tickDelta;

		private Upgradeable _amount;

		private Upgradeable _status;

		public int BuffCharges;

		public bool DirectionSet;

		public Vector3 Direction;

		public bool PositionSet;

		public Vector3 Position;

		public bool IsSum;

		public float SumAmount;
	}
}
