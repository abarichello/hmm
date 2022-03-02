using System;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Common;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[Obsolete("Obsolete! Use FilterBlock")]
	[Serializable]
	public class PublicParameterComparison<T> : IParameterComparison where T : IComparable
	{
		public GadgetSlot ParameterGadget
		{
			get
			{
				return this._parameterGadget;
			}
		}

		public bool IsValidParameter
		{
			get
			{
				return this._isValidParameter;
			}
		}

		public void Initialize(IHMMGadgetContext context)
		{
			this._isValidParameter = false;
			this._context = context;
			if (context == null)
			{
				return;
			}
			this._gadgetParameter = this._context.GetUIParameter<T>(this._parameterName);
			this._isValidParameter = (this._gadgetParameter != null);
			this._parameterValue = this.GetCurrentParameterValue(this._context);
			this.DoCompare(this._context, true);
		}

		private T GetCurrentParameterValue(object context)
		{
			return (!this._isValidParameter) ? default(T) : this._gadgetParameter.GetValue(context);
		}

		public bool Compare(object context)
		{
			if (context == null)
			{
				context = this._context;
			}
			return this.DoCompare(context, false);
		}

		private bool DoCompare(object context, bool forceComparison = false)
		{
			T currentParameterValue = this.GetCurrentParameterValue(context);
			if (!forceComparison && this._parameterValue.CompareTo(currentParameterValue) == 0)
			{
				return this._lastComparisonResult;
			}
			this._parameterValue = currentParameterValue;
			switch (this._comparisonType)
			{
			case ComparisonType.Equal:
				this._lastComparisonResult = (this._parameterValue.CompareTo(this._requiredParameterValue) == 0);
				break;
			case ComparisonType.Greater:
				this._lastComparisonResult = (this._parameterValue.CompareTo(this._requiredParameterValue) > 0);
				break;
			case ComparisonType.Lesser:
				this._lastComparisonResult = (this._parameterValue.CompareTo(this._requiredParameterValue) < 0);
				break;
			case ComparisonType.EqualOrGreater:
				this._lastComparisonResult = (this._parameterValue.CompareTo(this._requiredParameterValue) >= 0);
				break;
			case ComparisonType.EqualOrLesser:
				this._lastComparisonResult = (this._parameterValue.CompareTo(this._requiredParameterValue) <= 0);
				break;
			default:
				this._lastComparisonResult = false;
				break;
			}
			return this._lastComparisonResult;
		}

		[SerializeField]
		private string _parameterName;

		[SerializeField]
		private ComparisonType _comparisonType;

		[SerializeField]
		private T _requiredParameterValue;

		[SerializeField]
		private GadgetSlot _parameterGadget;

		private bool _isValidParameter;

		private ComparisonType _lastComparisonType;

		private T _lastRequiredParameterValue;

		private IParameter<T> _gadgetParameter;

		private IHMMGadgetContext _context;

		private T _parameterValue;

		private bool _lastComparisonResult;
	}
}
