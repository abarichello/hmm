using System;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Relative/GadgetBody")]
	public sealed class RelativeGadgetBodyParameter : TypedParameter<GadgetBody>
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
				this._relativeParameter = new RelativeParameterTomate<GadgetBody>(this._relativeTo.ParameterTomate, this._defaultValue);
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
			this._relativeParameter = new RelativeParameterTomate<GadgetBody>(this._relativeTo.ParameterTomate, this._defaultValue);
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
			GadgetBody value = base.GetValue(context);
			bool flag = value != null;
			bs.WriteBool(flag);
			if (flag)
			{
				IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)value.Context;
				bs.WriteCompressedInt(ihmmgadgetContext.Owner.Identifiable.ObjId);
				bs.WriteCompressedInt(ihmmgadgetContext.Id);
				bs.WriteCompressedInt(value.Id);
			}
		}

		protected override void ReadFromBitStream(object context, BitStream bs)
		{
			if (!bs.ReadBool())
			{
				base.SetValue(context, null);
				return;
			}
			int id = bs.ReadCompressedInt();
			int id2 = bs.ReadCompressedInt();
			int key = bs.ReadCompressedInt();
			IGadgetOwner component = GameHubScriptableObject.Hub.ObjectCollection.GetObject(id).GetComponent<IGadgetOwner>();
			IGadgetContext gadgetContext = component.GetGadgetContext(id2);
			if (gadgetContext.Bodies.ContainsKey(key))
			{
				base.SetValue(context, (GadgetBody)gadgetContext.Bodies[key]);
			}
		}

		protected override GadgetBody InternalGetValue(object context)
		{
			return ((IParameterTomate<GadgetBody>)this.ParameterTomate).GetValue(context);
		}

		protected override void InternalSetValue(object context, GadgetBody value)
		{
			((IParameterTomate<GadgetBody>)this.ParameterTomate).SetValue(context, value);
		}

		protected override void InternalSetRoute(object context, Func<object, GadgetBody> getter, Action<object, GadgetBody> setter)
		{
			((IParameterTomate<GadgetBody>)this.ParameterTomate).SetRoute(context, getter, setter);
		}

		[SerializeField]
		private BaseParameter _relativeTo;

		[SerializeField]
		private GadgetBody _defaultValue;

		private RelativeParameterTomate<GadgetBody> _relativeParameter;
	}
}
