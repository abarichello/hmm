using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class BombFlagTrigger : GameHubBehaviour
	{
		private void Awake()
		{
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Net.IsClient())
			{
				UnityEngine.Object.Destroy(this);
				return;
			}
			BombFlagTrigger.lastKindTriggered = BombFlagTrigger.TriggerKind.None;
			BombFlagTrigger.lastTeamTriggered = TeamKind.Zero;
			if (this.kind == BombFlagTrigger.TriggerKind.None)
			{
				BombFlagTrigger.Log.Warn(string.Format("A bombflagtrigger is not setup: {0}. Cleaning!", base.name));
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
		}

		private void OnDestroy()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			CombatObject combat = CombatRef.GetCombat(other);
			if (combat == null || !combat.IsBomb)
			{
				return;
			}
			float num = Vector3.Angle(base.transform.forward, combat.Movement.LastVelocity);
			if (num < 90f && (this.kind != BombFlagTrigger.lastKindTriggered || this.trackTeamKind != BombFlagTrigger.lastTeamTriggered))
			{
				this.BombPhaseChanged();
			}
			BombFlagTrigger.lastKindTriggered = this.kind;
			BombFlagTrigger.lastTeamTriggered = this.trackTeamKind;
		}

		private void BombPhaseChanged()
		{
			switch (this.kind)
			{
			case BombFlagTrigger.TriggerKind.AlmostDelivered:
				GameHubBehaviour.Hub.BombManager.CallListenToBombAlmostDeliveredTriggerEnter(this.trackTeamKind);
				break;
			case BombFlagTrigger.TriggerKind.LastCurve:
				GameHubBehaviour.Hub.BombManager.CallListenToBombLastCurveTriggerEnter(this.trackTeamKind);
				break;
			case BombFlagTrigger.TriggerKind.FirstCurve:
				GameHubBehaviour.Hub.BombManager.CallListenToBombFirstCurveTriggerEnter(this.trackTeamKind);
				break;
			case BombFlagTrigger.TriggerKind.Entrance:
				GameHubBehaviour.Hub.BombManager.CallListenToBombTrackEntryTriggerEnter(this.trackTeamKind);
				break;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BombFlagTrigger));

		[SerializeField]
		protected TeamKind trackTeamKind;

		[SerializeField]
		private BombFlagTrigger.TriggerKind kind;

		private static BombFlagTrigger.TriggerKind lastKindTriggered = BombFlagTrigger.TriggerKind.None;

		private static TeamKind lastTeamTriggered = TeamKind.Zero;

		public enum TriggerKind
		{
			None,
			AlmostDelivered,
			LastCurve,
			FirstCurve,
			Entrance
		}
	}
}
