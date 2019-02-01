using System;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	[CreateAssetMenu(menuName = "GadgetScript/Block/CombatObject/DettachCombat")]
	public class DettachCombatBlock : BaseAttachBlock
	{
		protected override bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (this._guest == null)
			{
				base.LogSanitycheckError("'Guest' parameter cannot be null.");
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
				ihmmeventContext.SaveParameter(this._guest);
				if (this._sendToClient)
				{
					ihmmeventContext.SendToClient();
				}
			}
			else
			{
				ihmmeventContext.LoadParameter(this._guest);
			}
			ICombatObject value = this._guest.GetValue(gadgetContext);
			if (ihmmgadgetContext.IsServer)
			{
				base.SetBodiesKinematicState(value.Transform, false);
				base.SetCollidersEnabledState(value.Transform, true);
			}
			else
			{
				int objId = value.Identifiable.ObjId;
				CombatObject component = value.Transform.parent.GetComponent<CombatObject>();
				ihmmgadgetContext.SetLifebarVisibility(objId, true);
				if (component != null)
				{
					ihmmgadgetContext.SetAttachedLifebarGroupVisibility(component.Id.ObjId, objId, false);
				}
			}
			value.Transform.parent = ihmmgadgetContext.HierarchyDrawers.Players;
			value.Transform.gameObject.layer = ((value.Team != TeamKind.Blue) ? 10 : 11);
			return this._nextBlock;
		}

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _guest;
	}
}
