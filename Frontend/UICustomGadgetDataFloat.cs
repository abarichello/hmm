using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.GadgetScript;
using Hoplon.GadgetScript;

namespace HeavyMetalMachines.Frontend
{
	public class UICustomGadgetDataFloat
	{
		public int LastValue
		{
			get
			{
				return (int)this._lastValue;
			}
		}

		public bool Init(string value, CombatObject obj, Action<float> setValue)
		{
			if (setValue == null)
			{
				return false;
			}
			for (int i = 0; i < obj.CustomGadgets.Count; i++)
			{
				IHMMGadgetContext gadget = obj.CustomGadgets[i];
				if (this.Init(value, gadget, setValue))
				{
					return true;
				}
			}
			return false;
		}

		public bool Init(string value, IHMMGadgetContext gadget, Action<float> setValue)
		{
			if (setValue == null)
			{
				return false;
			}
			IParameter<float> uiparameter = gadget.GetUIParameter<float>(value);
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

		private void SetValue(float val)
		{
			this._lastValue = val;
			this._setValue(val);
		}

		public void Update()
		{
			float value = this._parameter.GetValue(this._context);
			if (value != this._lastValue)
			{
				this.SetValue(value);
			}
		}

		private object _context;

		private IParameter<float> _parameter;

		private float _lastValue;

		private Action<float> _setValue;
	}
}
