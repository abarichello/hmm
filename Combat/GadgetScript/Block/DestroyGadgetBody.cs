using System;
using Hoplon.GadgetScript;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	public static class DestroyGadgetBody
	{
		public static void Destroy(IHMMEventContext hmmEventContext, IHMMGadgetContext hmmGadgetContext, BaseParameter gadgetBodyParameter)
		{
			if (gadgetBodyParameter == null)
			{
				return;
			}
			if (hmmGadgetContext.IsServer)
			{
				hmmEventContext.SaveParameter(gadgetBodyParameter);
			}
			else
			{
				hmmEventContext.LoadParameter(gadgetBodyParameter);
			}
			IGadgetBody gadgetBody = gadgetBodyParameter.ParameterTomate.GetBoxedValue(hmmGadgetContext) as IGadgetBody;
			if (gadgetBody == null || !gadgetBody.IsAlive)
			{
				return;
			}
			if (!hmmGadgetContext.Bodies.ContainsKey(gadgetBody.Id))
			{
				return;
			}
			hmmEventContext.RemoveBody(gadgetBody.Id);
			hmmGadgetContext.Bodies.Remove(gadgetBody.Id);
			if (hmmGadgetContext.IsServer && gadgetBody.WasSentToClient)
			{
				hmmEventContext.SetPreviousEventId(gadgetBody.CreationEventId);
				hmmEventContext.SendToClient();
			}
			else if (hmmGadgetContext.IsClient && hmmEventContext.Id != gadgetBody.CreationEventId)
			{
				hmmGadgetContext.SetBodyDestructionTime(gadgetBody.Id, hmmEventContext.CreationTime);
			}
			gadgetBody.Destroy();
		}
	}
}
