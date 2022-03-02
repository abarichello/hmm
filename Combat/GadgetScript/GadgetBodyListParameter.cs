using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/List/GadgetBodyList")]
	public class GadgetBodyListParameter : Parameter<List<GadgetBody>>
	{
		protected override void Initialize()
		{
			base.Initialize();
			if (GadgetBodyListParameter._tempParameter == null)
			{
				GadgetBodyListParameter._tempParameter = ScriptableObject.CreateInstance<GadgetBodyParameter>();
			}
		}

		protected override void WriteToBitStream(object context, BitStream bs)
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
					GadgetBodyListParameter._tempParameter.SetValue<GadgetBody>(context, gadgetBody);
					GadgetBodyListParameter._tempParameter.WriteToBitStreamWithContentId(context, bs);
				}
			}
		}

		protected override void ReadFromBitStream(object context, BitStream bs)
		{
			int num = bs.ReadCompressedInt();
			List<GadgetBody> list = new List<GadgetBody>(num);
			for (int i = 0; i < num; i++)
			{
				if (bs.ReadBool())
				{
					GadgetBodyListParameter._tempParameter.ReadFromBitStreamWithContentId(context, bs);
					list.Add(GadgetBodyListParameter._tempParameter.GetValue<GadgetBody>(context));
				}
				else
				{
					list.Add(null);
				}
			}
			base.SetValue(context, list);
		}

		[Restrict(true, new Type[]
		{
			typeof(GadgetBody)
		})]
		private static BaseParameter _tempParameter;
	}
}
