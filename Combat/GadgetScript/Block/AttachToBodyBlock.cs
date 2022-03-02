using System;
using HeavyMetalMachines.Combat.GadgetScript.Body;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/CombatObject/AttachToBody")]
	public class AttachToBodyBlock : BaseAttachBlock
	{
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)gadgetContext;
			IHMMEventContext ihmmeventContext = (IHMMEventContext)eventContext;
			if (ihmmgadgetContext.IsServer)
			{
				ihmmeventContext.SaveParameter(this._host);
				ihmmeventContext.SaveParameter(this._guest);
				if (this._sendToClient)
				{
					((IHMMEventContext)eventContext).SendToClient();
				}
			}
			else
			{
				ihmmeventContext.LoadParameter(this._host);
				ihmmeventContext.LoadParameter(this._guest);
			}
			if (ihmmgadgetContext.IsServer)
			{
				GadgetBody value = this._host.GetValue<GadgetBody>(gadgetContext);
				ICombatObject value2 = this._guest.GetValue(gadgetContext);
				Transform transform = value2.Transform;
				base.SetBodiesKinematicState(transform, true);
				base.SetCollidersEnabledState(transform, false);
				value2.PhysicalObject.PauseSimulation();
				value2.PhysicalObject.ResetImpulseAndVelocity();
				Transform component = value.GetComponent<Transform>();
				transform.parent = component;
				transform.gameObject.layer = component.gameObject.layer;
			}
			return this._nextBlock;
		}

		[Header("Read")]
		[Restrict(true, new Type[]
		{
			typeof(GadgetBody)
		})]
		[SerializeField]
		private BaseParameter _host;

		[SerializeField]
		private CombatObjectParameter _guest;
	}
}
