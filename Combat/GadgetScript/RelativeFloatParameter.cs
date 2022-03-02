using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Relative/Float")]
	public sealed class RelativeFloatParameter : TypedParameter<float>
	{
		protected override void Reset()
		{
			this._relativeParameter = null;
		}

		protected override void Initialize()
		{
			base.Initialize();
			if (this._relativeTo.ParameterTomate != null)
			{
				this._relativeParameter = new RelativeParameterTomate<float>(this._relativeTo.ParameterTomate, this._defaultValue);
			}
			else
			{
				this._relativeTo.OnParameterInitialized += this.OnRelativeParameterInitialized;
			}
			this._relativeTo.OnParameterValueUpdated += base.CallOnParameterValueUpdated;
		}

		private void OnRelativeParameterInitialized()
		{
			this._relativeTo.OnParameterInitialized -= this.OnRelativeParameterInitialized;
			this._relativeParameter = new RelativeParameterTomate<float>(this._relativeTo.ParameterTomate, this._defaultValue);
			base.CallOnParameterInitialized();
		}

		public override IBaseParameterTomate ParameterTomate
		{
			get
			{
				return this._relativeParameter;
			}
		}

		protected override void WriteToBitStream(object context, BitStream bs)
		{
			bs.WriteFloat(base.GetValue(context));
		}

		protected override void ReadFromBitStream(object context, BitStream bs)
		{
			base.SetValue(context, bs.ReadFloat());
		}

		protected override float InternalGetValue(object context)
		{
			return ((IParameterTomate<float>)this.ParameterTomate).GetValue(context);
		}

		protected override void InternalSetValue(object context, float value)
		{
			((IParameterTomate<float>)this.ParameterTomate).SetValue(context, value);
		}

		protected override void InternalSetRoute(object context, Func<object, float> getter, Action<object, float> setter)
		{
			((IParameterTomate<float>)this.ParameterTomate).SetRoute(context, getter, setter);
		}

		[SerializeField]
		private BaseParameter _relativeTo;

		[SerializeField]
		private float _defaultValue;

		private RelativeParameterTomate<float> _relativeParameter;
	}
}
