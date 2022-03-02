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
		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
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
			if (value == null)
			{
				return this._nextBlock;
			}
			if (ihmmgadgetContext.IsServer)
			{
				base.SetBodiesKinematicState(value.Transform, false);
				base.SetCollidersEnabledState(value.Transform, true);
				value.PhysicalObject.UnpauseSimulation();
			}
			else
			{
				int objId = value.Identifiable.ObjId;
				CombatObject combatObject = (!(value.Transform.parent != null)) ? null : value.Transform.parent.GetComponent<CombatObject>();
				ihmmgadgetContext.SetLifebarVisibility(objId, true);
				if (combatObject != null)
				{
					ihmmgadgetContext.SetAttachedLifebarGroupVisibility(combatObject.Id.ObjId, objId, false);
				}
				else
				{
					Debug.LogWarning("Null host combat");
				}
			}
			value.Transform.parent = ihmmgadgetContext.HierarchyDrawers.Players;
			value.Transform.gameObject.layer = ((value.Team != TeamKind.Blue) ? 10 : 11);
			DettachCombatBlock.ResetCombatObjectVerticalPosition(value);
			return this._nextBlock;
		}

		private static void ResetCombatObjectVerticalPosition(ICombatObject combat)
		{
			Vector3 position = combat.Transform.position;
			position.y = 0f;
			combat.Transform.position = position;
		}

		[Header("Read")]
		[SerializeField]
		private CombatObjectParameter _guest;
	}
}
