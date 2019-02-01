using System;
using System.Diagnostics;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[Serializable]
	public class UICustomGadgetDataComponent
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private event Action _update;

		public int Data
		{
			get
			{
				return (this._data != null) ? this._data.LastValue : this._currentValue;
			}
		}

		public int Max
		{
			get
			{
				return (this._max != null) ? this._max.LastValue : this._maxValue;
			}
		}

		public bool IsChargesParameter
		{
			get
			{
				return this._isChargesParameter;
			}
		}

		public bool Init(CombatGadget gadget)
		{
			bool flag = false;
			UICustomGadgetDataComponent.Kind kind = this._kind;
			if (kind != UICustomGadgetDataComponent.Kind.Label)
			{
				if (kind == UICustomGadgetDataComponent.Kind.ProgressBar)
				{
					if (this._progressBar == null)
					{
						return false;
					}
					this._data = new UICustomGadgetDataInt();
					this._max = new UICustomGadgetDataInt();
					flag = (this._data.Init(this._paramName, gadget, new Action<int>(this.SetProgress)) && this._max.Init(this._paramMax, gadget, new Action<int>(this.SetMax)));
					if (flag)
					{
						this._componentGroup.SetActive(true);
						this._update += this._max.Update;
						this._update += this._data.Update;
					}
				}
			}
			else
			{
				if (this._label == null)
				{
					return false;
				}
				this._data = new UICustomGadgetDataInt();
				flag = this._data.Init(this._paramName, gadget, new Action<int>(this.SetLabel));
				if (flag)
				{
					this._componentGroup.SetActive(true);
					this._update += this._data.Update;
				}
			}
			return flag;
		}

		private void SetLabel(int value)
		{
			this._label.SetTextIntFormat(this._labelFormat, value);
		}

		private void SetMax(int value)
		{
			this._maxValue = value;
			this.SetProgress(this._currentValue);
		}

		private void SetProgress(int value)
		{
			this._currentValue = value;
			this._progressBar.fillAmount = (float)this._currentValue / (float)this._maxValue;
		}

		public void Update()
		{
			this._update();
		}

		private void LegacyUpdate()
		{
			if (this._currentValue != this._legacyState.Value)
			{
				this._currentValue = this._legacyState.Value;
				UICustomGadgetDataComponent.Kind kind = this._kind;
				if (kind != UICustomGadgetDataComponent.Kind.Label)
				{
					if (kind == UICustomGadgetDataComponent.Kind.ProgressBar)
					{
						this.SetProgress(this._currentValue);
					}
				}
				else
				{
					this.SetLabel(this._currentValue);
				}
			}
			if (this._maxValue != this._legacyGadget.MaxChargeCount)
			{
				this._maxValue = this._legacyGadget.MaxChargeCount;
				if (this._kind == UICustomGadgetDataComponent.Kind.ProgressBar)
				{
					this.SetMax(this._maxValue);
				}
			}
		}

		public bool InitLegacy(GadgetBehaviour gadget)
		{
			if (gadget.Kind != GadgetKind.InstantWithCharges)
			{
				return false;
			}
			this._legacyGadget = gadget;
			this._legacyState = gadget.Combat._gadgetData.GetGadgetState(gadget.Slot);
			this._currentValue = this._legacyState.Value;
			this._maxValue = gadget.MaxChargeCount;
			this._update += this.LegacyUpdate;
			this._componentGroup.SetActive(true);
			return true;
		}

		[SerializeField]
		private UICustomGadgetDataComponent.Kind _kind;

		[SerializeField]
		private UILabel _label;

		[SerializeField]
		private UI2DSprite _progressBar;

		[SerializeField]
		private GameObject _componentGroup;

		[SerializeField]
		private string _paramName;

		[SerializeField]
		private string _paramMax;

		[SerializeField]
		private string _labelFormat = "{0:00}";

		[SerializeField]
		private bool _isChargesParameter;

		private UICustomGadgetDataInt _data;

		private UICustomGadgetDataInt _max;

		private int _maxValue;

		private int _currentValue;

		private GadgetBehaviour _legacyGadget;

		private GadgetData.GadgetStateObject _legacyState;

		public enum Kind
		{
			Label,
			ProgressBar
		}
	}
}
