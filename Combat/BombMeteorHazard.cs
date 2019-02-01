using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class BombMeteorHazard : GameHubBehaviour
	{
		private void Awake()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this._directionCalculator = new HazardDirectionProvider(this.Direction, base.transform, this.DirectionTransform);
			if (this.Team == TeamKind.Neutral || this.Team == TeamKind.Zero)
			{
				BombMeteorHazard.Log.FatalFormat("Bomb Hazard without team is not yet implemented! {0}", new object[]
				{
					base.gameObject
				});
			}
		}

		private void OnCollisionEnter(Collision col)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			CombatObject combat = CombatRef.GetCombat(col.collider);
			if (combat != null && combat.IsBomb && !GameHubBehaviour.Hub.BombManager.IsSomeoneCarryingBomb())
			{
				int num = GameHubBehaviour.Hub.BombManager.ActiveBomb.LastCarriersByTeam[this.Team];
				if (num == -1)
				{
					num = GameHubBehaviour.Hub.Players.GetAnyRandomlyByTeam(this.Team).CharacterInstance.ObjId;
				}
				CombatObject combat2 = CombatRef.GetCombat(num);
				combat2.BombGadget.FireMeteor(this._directionCalculator.GetUpdatedDirection(false, combat.Transform.position, combat.Movement.LastVelocity, col.contacts[0].normal), 1f, false);
			}
		}

		private void OnTriggerEnter(Collider col)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			CombatObject combat = CombatRef.GetCombat(col);
			if (combat != null && combat.IsBomb && !GameHubBehaviour.Hub.BombManager.IsSomeoneCarryingBomb())
			{
				int num = GameHubBehaviour.Hub.BombManager.ActiveBomb.LastCarriersByTeam[this.Team];
				if (num == -1)
				{
					num = GameHubBehaviour.Hub.Players.GetAnyRandomlyByTeam(this.Team).CharacterInstance.ObjId;
				}
				CombatObject combat2 = CombatRef.GetCombat(num);
				combat2.BombGadget.FireMeteor(this._directionCalculator.GetUpdatedDirection(false, combat.Transform.position, combat.Movement.LastVelocity, default(Vector3)), 1f, false);
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(BombMeteorHazard));

		public TeamKind Team;

		public HazardDirectionProvider.EDirection Direction;

		public Transform DirectionTransform;

		private HazardDirectionProvider _directionCalculator;
	}
}
