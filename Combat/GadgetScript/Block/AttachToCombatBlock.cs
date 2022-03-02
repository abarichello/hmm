using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/CombatObject/AttachToCombat")]
	public class AttachToCombatBlock : BaseAttachBlock
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
					ihmmeventContext.SendToClient();
				}
			}
			else
			{
				ihmmeventContext.LoadParameter(this._host);
				ihmmeventContext.LoadParameter(this._guest);
			}
			ICombatObject value = this._guest.GetValue(gadgetContext);
			Transform transform = value.Transform;
			ICombatObject value2 = this._host.GetValue(gadgetContext);
			if (ihmmgadgetContext.IsServer)
			{
				base.SetBodiesKinematicState(transform, true);
				base.SetCollidersEnabledState(transform, false);
				value.PhysicalObject.PauseSimulation();
				value.PhysicalObject.ResetImpulseAndVelocity();
				transform.parent = value2.Transform;
			}
			else
			{
				int objId = value.Identifiable.ObjId;
				ihmmgadgetContext.SetLifebarVisibility(objId, false);
				ihmmgadgetContext.SetAttachedLifebarGroupVisibility(value2.Identifiable.ObjId, objId, true);
			}
			transform.gameObject.layer = value2.Transform.gameObject.layer;
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _host;

		[SerializeField]
		private CombatObjectParameter _guest;
	}
}
