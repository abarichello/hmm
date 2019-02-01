using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.GadgetScript;
using Hoplon.GadgetScript;

namespace HeavyMetalMachines.Frontend
{
	public class UICustomGadgetDataInt
	{
		public int LastValue
		{
			get
			{
				return this._lastValue;
			}
		}

		public bool Init(string value, CombatObject obj, Action<int> setValue)
		{
			if (setValue == null)
			{
				return false;
			}
			foreach (KeyValuePair<GadgetSlot, CombatGadget> keyValuePair in obj.CustomGadgets)
			{
				CombatGadget value2 = keyValuePair.Value;
				if (this.Init(value, value2, setValue))
				{
					return true;
				}
			}
			return false;
		}

		public bool Init(string value, CombatGadget gadget, Action<int> setValue)
		{
			if (setValue == null)
			{
				return false;
			}
			IParameter<int> uiparameter = gadget.GetUIParameter<int>(value);
			if (uiparameter == null)
			{
				return false;
			}
			this._parameter = uiparameter;
			this._context = gadget;
			this._setValue = setValue;
			this.SetValue(this._parameter.GetValue(this._context));
			return true;
		}

		private void SetValue(int val)
		{
			this._lastValue = val;
			this._setValue(val);
		}

		public void Update()
		{
			int value = this._parameter.GetValue(this._context);
			if (value != this._lastValue)
			{
				this.SetValue(value);
			}
		}

		private IParameterContext _context;

		private IParameter<int> _parameter;

		private int _lastValue;

		private Action<int> _setValue;
	}
}
