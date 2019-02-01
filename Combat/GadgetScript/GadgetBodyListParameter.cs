using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/List/GadgetBodyList")]
	public class GadgetBodyListParameter : Parameter<List<GadgetBody>>
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			if (GadgetBodyListParameter._tempParameter == null)
			{
				GadgetBodyListParameter._tempParameter = new GadgetBodyParameter();
			}
		}

		protected override void WriteToBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
			List<GadgetBody> value = base.GetValue(context);
			if (value == null || value.Count == 0)
			{
				bs.WriteCompressedInt(0);
				return;
			}
			bs.WriteCompressedInt(value.Count);
			for (int i = 0; i < value.Count; i++)
			{
				GadgetBody gadgetBody = value[i];
				bool flag = gadgetBody != null;
				bs.WriteBool(flag);
				if (flag)
				{
					GadgetBodyListParameter._tempParameter.SetValue(context, gadgetBody);
					GadgetBodyListParameter._tempParameter.WriteToBitStreamWithContentId(context, bs);
				}
			}
		}

		protected override void ReadFromBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
			int num = bs.ReadCompressedInt();
			List<GadgetBody> list = new List<GadgetBody>(num);
			for (int i = 0; i < num; i++)
			{
				if (bs.ReadBool())
				{
					GadgetBodyListParameter._tempParameter.ReadFromBitStreamWithContentId(context, bs);
					list.Add(GadgetBodyListParameter._tempParameter.GetValue(context));
				}
				else
				{
					list.Add(null);
				}
			}
			base.SetValue(context, list);
		}

		private static GadgetBodyParameter _tempParameter;
	}
}
