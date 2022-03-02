using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class ThreadmillCollision : GameHubBehaviour, IHazard
	{
		public TeamKind Team { get; private set; }

		public bool ShouldHitOnlyBombCarrier { get; private set; }

		private void Awake()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			if (base.Id)
			{
				this._hasIdentifiable = true;
				this._combatObject = base.Id.GetBitComponent<CombatObject>();
			}
			this._hitChecker = new HazardHit(this);
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChanged;
		}

		private void OnEnable()
		{
			if (this.Modifiers == null)
			{
				ThreadmillCollision.Log.ErrorFormat("GD: ThreadmillCollision with null Modifier on {0}", new object[]
				{
					base.gameObject.name
				});
				base.gameObject.SetActive(false);
			}
		}

		protected virtual void OnDestroy()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChanged;
			GameHubBehaviour.Hub.BotAIHub.RepairPoints.Remove(base.transform);
		}

		protected virtual void OnTriggerStay(Collider col)
		{
			this.InternalTrigger(col, this.Modifiers);
		}

		private void InternalTrigger(Collider col, HazardModifiers modifiers)
		{
			if (!base.enabled || modifiers == null)
			{
				return;
			}
			this._currentTarget = CombatRef.GetCombat(col);
			if (this._currentTarget == null)
			{
				return;
			}
			this._currentModifiers = modifiers;
			Vector3 vector = base.transform.position - this._currentTarget.Transform.position;
			if (vector.magnitude < this.EffectiveDistance)
			{
				return;
			}
			Vector3 vector2 = Vector3.Normalize(vector);
			Vector3 direction = Vector3.Normalize(Vector3.Cross(Vector3.up, vector2));
			modifiers.Data.SetDirection(direction);
			this._hitChecker.TryHit(this._currentTarget);
			this._currentModifiers = null;
			this._currentTarget = null;
		}

		public void HitTarget()
		{
			this._currentTarget.Controller.AddModifiers(this._currentModifiers.Data, (!this._hasIdentifiable) ? null : this._combatObject, -1, false);
			this._currentModifiers = null;
			this._currentTarget = null;
		}

		protected void OnPhaseChanged(BombScoreboardState state)
		{
			if (state != BombScoreboardState.PreReplay && state != BombScoreboardState.BombDelivery)
			{
				base.enabled = false;
			}
			else
			{
				base.enabled = true;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HazardArea));

		[Tooltip("Modifiers to apply on TriggerStay or on CollisionEnter")]
		public HazardModifiers Modifiers;

		private HazardHit _hitChecker;

		private CombatObject _currentTarget;

		private HazardModifiers _currentModifiers;

		private bool _hasIdentifiable;

		private ICombatObject _combatObject;

		public float EffectiveDistance;
	}
}
