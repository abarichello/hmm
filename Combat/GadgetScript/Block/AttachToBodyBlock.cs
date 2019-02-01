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
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._guest == null)
			{
				base.LogSanitycheckError("'Guest' parameter cannot be null.");
				return false;
			}
			if (this._host == null)
			{
				base.LogSanitycheckError("'Host' parameter cannot be null.");
				return false;
			}
			return true;
		}

		protected override IBlock InnerExecute(IGadgetContext gadgetContext, IEventContext eventContext)
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
			GadgetBody value = this._host.GetValue(gadgetContext);
			ICombatObject value2 = this._guest.GetValue(gadgetContext);
			Transform transform = value2.Transform;
			if (ihmmgadgetContext.IsServer)
			{
				base.SetBodiesKinematicState(transform, true);
				base.SetCollidersEnabledState(transform, false);
				value2.PhysicalObject.ResetImpulseAndVelocity();
			}
			Transform component = value.GetComponent<Transform>();
			transform.parent = component;
			transform.gameObject.layer = component.gameObject.layer;
			return this._nextBlock;
		}

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._host, parameterId) || base.CheckIsParameterWithId(this._guest, parameterId);
		}

		[Header("Read")]
		[SerializeField]
		private GadgetBodyParameter _host;

		[SerializeField]
		private CombatObjectParameter _guest;
	}
}
