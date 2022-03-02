using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/List/FindBodyByTargetInListBlock")]
	public class FindBodyByTargetInListBlock : BaseBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (ihmmgadgetContext.IsClient)
			{
				ihmmeventContext.LoadParameter(this._body);
				return this._nextBlock;
			}
			List<GadgetBody> value = this._list.GetValue(gadgetContext);
			for (int i = 0; i < value.Count; i++)
			{
				GadgetBody gadgetBody = value[i];
				if (!(gadgetBody == null))
				{
					AttachToDummyBodyMovement component = gadgetBody.GetComponent<AttachToDummyBodyMovement>();
					if (!(component == null))
					{
						ICombatObject target = component.GetTarget();
						if (target.Identifiable.ObjId == this._combat.GetValue(gadgetContext).Identifiable.ObjId)
						{
							this._body.SetValue<GadgetBody>(gadgetContext, gadgetBody);
							ihmmeventContext.SaveParameter(this._body);
							return this._nextBlock;
						}
					}
				}
			}
			this._body.SetValue<GadgetBody>(gadgetContext, null);
			ihmmeventContext.SaveParameter(this._body);
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		private GadgetBodyListParameter _list;

		[SerializeField]
		private CombatObjectParameter _combat;

		[Header("Write")]
		[Restrict(true, new Type[]
		{
			typeof(GadgetBody)
		})]
		[SerializeField]
		private BaseParameter _body;
	}
}
