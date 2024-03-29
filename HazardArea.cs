﻿using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class HazardArea : GameHubBehaviour, IHazard
	{
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
			this._dirCalculator = new HazardDirectionProvider(this.Direction, base.transform, this.DirectionTransform);
			this._hitChecker = new HazardHit(this);
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChanged;
			if (this.AiRepairPoint && !GameHubBehaviour.Hub.BotAIHub.RepairPoints.Contains(base.transform))
			{
				GameHubBehaviour.Hub.BotAIHub.RepairPoints.Add(base.transform);
			}
		}

		private void OnEnable()
		{
			if (this.Modifiers == null && this.OnEnterModifiers == null && this.OnExitModifiers == null)
			{
				HazardArea.Log.ErrorFormat("GD: HazardArea with null Modifier on {0}", new object[]
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

		protected virtual void OnCollisionEnter(Collision col)
		{
			if (!base.enabled)
			{
				return;
			}
			this._currentTarget = CombatRef.GetCombat(col.collider);
			if (this._currentTarget == null || this.Modifiers == null)
			{
				return;
			}
			this._currentModifiers = this.Modifiers;
			this.Modifiers.Data.SetDirection(this._dirCalculator.GetUpdatedDirection(this.InvertDirectionIfOpposite, this._currentTarget.Transform.position, this._currentTarget.Movement.LastVelocity, col.contacts[0].normal));
			this._hitChecker.TryHit(this._currentTarget);
			this._currentModifiers = null;
			this._currentTarget = null;
		}

		protected virtual void OnTriggerEnter(Collider col)
		{
			this.InternalTrigger(col, this.OnEnterModifiers);
		}

		protected virtual void OnTriggerStay(Collider col)
		{
			this.InternalTrigger(col, this.Modifiers);
		}

		protected virtual void OnTriggerExit(Collider col)
		{
			this.InternalTrigger(col, this.OnExitModifiers);
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
			modifiers.Data.SetDirection(this._dirCalculator.GetUpdatedDirection(this.InvertDirectionIfOpposite, this._currentTarget.Transform.position, this._currentTarget.Movement.LastVelocity, default(Vector3)));
			this._hitChecker.TryHit(this._currentTarget);
			this._currentModifiers = null;
			this._currentTarget = null;
		}

		public TeamKind Team
		{
			get
			{
				return this._team;
			}
		}

		public bool ShouldHitOnlyBombCarrier
		{
			get
			{
				return this.OnlyWhenCarryingBomb;
			}
		}

		public Vector3 Position
		{
			get
			{
				return base.transform.position;
			}
		}

		public Vector3 Forward
		{
			get
			{
				return base.transform.forward;
			}
		}

		public Vector3 Right
		{
			get
			{
				return base.transform.right;
			}
		}

		public void HitTarget()
		{
			if (this._hasIdentifiable)
			{
				this._currentTarget.Controller.AddModifiers(this._currentModifiers.Data, this._combatObject, -1, false);
			}
			else
			{
				this._currentTarget.Controller.AddModifiers(this._currentModifiers.Data, null, -1, false);
			}
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

		[Tooltip("Modifiers to apply on TriggerEnter")]
		public HazardModifiers OnEnterModifiers;

		[Tooltip("Modifiers to apply on TriggerExit")]
		public HazardModifiers OnExitModifiers;

		[SerializeField]
		public TeamKind _team;

		public HazardDirectionProvider.EDirection Direction;

		[Tooltip("Invert HazardDirection if Target velocity is opposite to the Hazard Direction")]
		public bool InvertDirectionIfOpposite;

		public Transform DirectionTransform;

		public bool OnlyWhenCarryingBomb;

		private HazardDirectionProvider _dirCalculator;

		private HazardHit _hitChecker;

		private CombatObject _currentTarget;

		private HazardModifiers _currentModifiers;

		private bool _hasIdentifiable;

		private ICombatObject _combatObject;

		[Tooltip("Set this to add this hazard's transform to AI targets when searching for repair")]
		public bool AiRepairPoint;
	}
}
