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
		public override int CompareTo(object context, BaseParameter other)
		{
			GadgetBodyParameter gadgetBodyParameter = (GadgetBodyParameter)other;
			if (gadgetBodyParameter.GetValue(context) == base.GetValue(context))
			{
				return 0;
			}
			return -1;
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
			int num = -1;
			int num2 = -1;
			try
			{
				if (!bs.ReadBool())
				{
					base.SetValue(context, null);
				}
				else
				{
					num = bs.ReadCompressedInt();
					num2 = bs.ReadCompressedInt();
					int key = bs.ReadCompressedInt();
					IGadgetOwner component = GameHubScriptableObject.Hub.ObjectCollection.GetObject(num).GetComponent<IGadgetOwner>();
					IGadgetContext gadgetContext = component.GetGadgetContext(num2);
					if (gadgetContext.Bodies.ContainsKey(key))
					{
						base.SetValue(context, (GadgetBody)gadgetContext.Bodies[key]);
					}
				}
			}
			catch (Exception)
			{
				Debug.LogError("GadgetBodyParameter::ReadFromBitStream");
				Debug.LogErrorFormat("Context Is Null: {0}", new object[]
				{
					context == null
				});
				Debug.LogErrorFormat("bs Is Null: {0}", new object[]
				{
					bs == null
				});
				HMMHub hub = GameHubScriptableObject.Hub;
				Debug.LogErrorFormat("hub Is Null: {0}", new object[]
				{
					hub == null
				});
				RPCObjectCollection objectCollection = hub.ObjectCollection;
				Debug.LogErrorFormat("objectCollection Is Null: {0}", new object[]
				{
					objectCollection == null
				});
				Debug.LogErrorFormat("ownerId: {0}", new object[]
				{
					num
				});
				Identifiable @object = objectCollection.GetObject(num);
				Debug.LogErrorFormat("identifiable Is Null: {0}", new object[]
				{
					@object == null
				});
				IGadgetOwner component2 = @object.GetComponent<IGadgetOwner>();
				Debug.LogErrorFormat("owner Is Null: {0}", new object[]
				{
					component2 == null
				});
				Debug.LogErrorFormat("contextId: {0}", new object[]
				{
					num2
				});
				IHMMGadgetContext gadgetContext2 = component2.GetGadgetContext(num2);
				Debug.LogErrorFormat("gadget Is Null: {0}", new object[]
				{
					gadgetContext2 == null
				});
				throw;
			}
		}
	}
}
