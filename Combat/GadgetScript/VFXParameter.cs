using System;
using HeavyMetalMachines.VFX;
using Hoplon.Unity.Loading;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Gadget/VFX")]
	public class VFXParameter : Parameter<MasterVFX>
	{
		public override int CompareTo(object context, BaseParameter other)
		{
			VFXParameter vfxparameter = (VFXParameter)other;
			if (vfxparameter.GetValue(context) == base.GetValue(context))
			{
				return 0;
			}
			return -1;
		}

		protected override void WriteToBitStream(object context, BitStream bs)
		{
			MasterVFX value = base.GetValue(context);
			bool flag = value != null;
			bs.WriteBool(flag);
			if (flag)
			{
				bs.WriteString(value.name);
			}
		}

		protected override void ReadFromBitStream(object context, BitStream bs)
		{
			if (!bs.ReadBool())
			{
				base.SetValue(context, null);
				return;
			}
			string assetName = bs.ReadString();
			Content asset = Loading.Content.GetAsset(assetName);
			Object asset2 = asset.Asset;
			Transform transform = (Transform)asset2;
			MasterVFX component = transform.GetComponent<MasterVFX>();
			base.SetValue(context, component);
		}
	}
}
