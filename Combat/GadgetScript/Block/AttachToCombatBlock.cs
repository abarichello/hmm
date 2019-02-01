using System;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/CombatObject/AttachToCombat")]
	public class AttachToCombatBlock : BaseAttachBlock
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

		public override bool UsesParameterWithId(int parameterId)
		{
			return base.CheckIsParameterWithId(this._host, parameterId) || base.CheckIsParameterWithId(this._guest, parameterId);
		}

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _host;

		[SerializeField]
		private CombatObjectParameter _guest;
	}
}
