using System;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Gadget/Body")]
	public class GadgetBodyParameter : Parameter<GadgetBody>
	{
		public override int CompareTo(IParameterContext context, BaseParameter other)
		{
			GadgetBodyParameter gadgetBodyParameter = (GadgetBodyParameter)other;
			if (gadgetBodyParameter.GetValue(context) == base.GetValue(context))
			{
				return 0;
			}
			return -1;
		}

		protected override void WriteToBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
			GadgetBody value = base.GetValue(context);
			bool flag = value != null;
			bs.WriteBool(flag);
			if (flag)
			{
				bs.WriteCompressedInt(value.Context.OwnerId);
				bs.WriteCompressedInt(value.Context.Id);
				bs.WriteCompressedInt(value.Id);
			}
		}

		protected override void ReadFromBitStream(IParameterContext context, Pocketverse.BitStream bs)
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
	}
}
