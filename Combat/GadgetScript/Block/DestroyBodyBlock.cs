using System;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/Body/DestroyBody")]
	public class DestroyBodyBlock : BaseBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			return true;
		}

		protected override IBlock InnerExecute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			if (this._body == null)
			{
				return this._nextBlock;
			}
			if (ihmmgadgetContext.IsServer)
			{
				ihmmeventContext.SaveParameter(this._body);
			}
			else
			{
				ihmmeventContext.LoadParameter(this._body);
			}
			IGadgetBody value = this._body.GetValue(gadgetContext);
			if (value == null || !value.IsAlive)
			{
				return this._nextBlock;
			}
			ihmmeventContext.RemoveBody(value.Id);
			gadgetContext.Bodies.Remove(value.Id);
			value.Destroy();
			IHMMEventContext ihmmeventContext2 = (IHMMEventContext)value.Event;
			if (ihmmgadgetContext.IsServer && ihmmeventContext2.ShouldBeSent)
			{
				ihmmeventContext.SetPreviousEventId(value.Event.Id);
				ihmmeventContext.SendToClient();
			}
			else if (ihmmgadgetContext.IsClient)
			{
				ihmmeventContext2.SetBodyDestructionEvent(value.Id, ihmmeventContext);
			}
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._body, parameterId);
		}

		[SerializeField]
		private GadgetBodyParameter _body;
	}
}
