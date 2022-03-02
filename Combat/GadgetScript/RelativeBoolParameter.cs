using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Relative/Bool")]
	public sealed class RelativeBoolParameter : TypedParameter<bool>
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
				this._relativeParameter = new RelativeParameterTomate<bool>(this._relativeTo.ParameterTomate, this._defaultValue);
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
			this._relativeParameter = new RelativeParameterTomate<bool>(this._relativeTo.ParameterTomate, this._defaultValue);
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
			bs.WriteBool(base.GetValue(context));
		}

		protected override void ReadFromBitStream(object context, BitStream bs)
		{
			base.SetValue(context, bs.ReadBool());
		}

		protected override bool InternalGetValue(object context)
		{
			return ((IParameterTomate<bool>)this.ParameterTomate).GetValue(context);
		}

		protected override void InternalSetValue(object context, bool value)
		{
			((IParameterTomate<bool>)this.ParameterTomate).SetValue(context, value);
		}

		protected override void InternalSetRoute(object context, Func<object, bool> getter, Action<object, bool> setter)
		{
			((IParameterTomate<bool>)this.ParameterTomate).SetRoute(context, getter, setter);
		}

		[SerializeField]
		private BaseParameter _relativeTo;

		[SerializeField]
		private bool _defaultValue;

		private RelativeParameterTomate<bool> _relativeParameter;
	}
}
