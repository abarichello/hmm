using System;
using HeavyMetalMachines.Infra.Context;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Relative/Combat Object")]
	public sealed class RelativeCombatObjectParameter : TypedParameter<ICombatObject>
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
				this._relativeParameter = new RelativeParameterTomate<ICombatObject>(this._relativeTo.ParameterTomate, this._defaultValue);
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
			this._relativeParameter = new RelativeParameterTomate<ICombatObject>(this._relativeTo.ParameterTomate, this._defaultValue);
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
			ICombatObject value = base.GetValue(context);
			if (value != null)
			{
				bs.WriteInt(value.Identifiable.ObjId);
			}
			else
			{
				bs.WriteInt(-1);
			}
		}

		protected override void ReadFromBitStream(object context, BitStream bs)
		{
			base.SetValue(context, ((IHMMGadgetContext)context).GetCombatObject(bs.ReadInt()));
		}

		protected override ICombatObject InternalGetValue(object context)
		{
			return ((IParameterTomate<ICombatObject>)this.ParameterTomate).GetValue(context);
		}

		protected override void InternalSetValue(object context, ICombatObject value)
		{
			((IParameterTomate<ICombatObject>)this.ParameterTomate).SetValue(context, value);
		}

		protected override void InternalSetRoute(object context, Func<object, ICombatObject> getter, Action<object, ICombatObject> setter)
		{
			((IParameterTomate<ICombatObject>)this.ParameterTomate).SetRoute(context, getter, setter);
		}

		[SerializeField]
		private BaseParameter _relativeTo;

		[SerializeField]
		private ICombatObject _defaultValue;

		private RelativeParameterTomate<ICombatObject> _relativeParameter;
	}
}
