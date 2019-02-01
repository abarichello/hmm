using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class BombDropper : GameHubBehaviour
	{
		public TeamKind TeamOwner
		{
			get
			{
				return this._teamOwner;
			}
		}

		private void Start()
		{
			this._isClient = GameHubBehaviour.Hub.Net.IsClient();
			if (this._isClient)
			{
				base.enabled = false;
			}
		}

		private void OnTriggerStay(Collider otherCollider)
		{
			if (this._isClient)
			{
				return;
			}
			CombatObject combat = CombatRef.GetCombat(otherCollider);
			if (!combat)
			{
				return;
			}
			if (combat.IsBomb)
			{
				if (this._dropWhenBombTouch && GameHubBehaviour.Hub.BombManager.IsSomeoneCarryingBomb())
				{
					List<int> bombCarriersIds = GameHubBehaviour.Hub.BombManager.ActiveBomb.BombCarriersIds;
					for (int i = 0; i < bombCarriersIds.Count; i++)
					{
						CombatObject combat2 = CombatRef.GetCombat(bombCarriersIds[i]);
						if (this._teamOwner != combat2.Team)
						{
							combat2.BombGadget.Disable(BombGadget.DisableReason.Dropper);
						}
					}
				}
				GameHubBehaviour.Hub.BombManager.CancelDispute();
			}
			else if (combat.IsPlayer && combat.Team != this._teamOwner)
			{
				combat.BombGadget.Disable(BombGadget.DisableReason.Dropper);
				GameHubBehaviour.Hub.BombManager.DisableBombGrabber(combat);
			}
		}

		[SerializeField]
		private TeamKind _teamOwner;

		[SerializeField]
		private bool _dropWhenBombTouch;

		private bool _isClient;
	}
}
