using System;
using HeavyMetalMachines.Combat.GadgetScript.Block;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Gadget/Child Body")]
	public class GadgetChildBodyParameter : Parameter<GadgetChildBody>
	{
		public override int CompareTo(object context, BaseParameter other)
		{
			GadgetChildBodyParameter gadgetChildBodyParameter = (GadgetChildBodyParameter)other;
			if (gadgetChildBodyParameter.GetValue(context) == base.GetValue(context))
			{
				return 0;
			}
			return -1;
		}

		protected override void WriteToBitStream(object context, BitStream bs)
		{
			GadgetChildBody value = base.GetValue(context);
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
				base.SetValue(context, (GadgetChildBody)gadgetContext.Bodies[key]);
			}
		}
	}
}
