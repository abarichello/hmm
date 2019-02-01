using System;
using HeavyMetalMachines.Combat;
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
			this._dirCalculator = new HazardDirectionProvider(this.Direction, base.transform, this.DirectionTransform);
			this._hitChecker = new HazardHit(this);
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChanged;
		}

		private void OnEnable()
		{
			if (this.Modifiers == null)
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
		}

		protected virtual void OnCollisionEnter(Collision col)
		{
			if (!base.enabled)
			{
				return;
			}
			this._currentTarget = CombatRef.GetCombat(col.collider);
			if (this._currentTarget == null)
			{
				return;
			}
			this.Modifiers.Data.SetDirection(this._dirCalculator.GetUpdatedDirection(this.InvertDirectionIfOpposite, this._currentTarget.Transform.position, this._currentTarget.Movement.LastVelocity, col.contacts[0].normal));
			this._hitChecker.TryHit(this._currentTarget);
		}

		protected virtual void OnTriggerStay(Collider col)
		{
			if (!base.enabled)
			{
				return;
			}
			this._currentTarget = CombatRef.GetCombat(col);
			if (this._currentTarget == null)
			{
				return;
			}
			this.Modifiers.Data.SetDirection(this._dirCalculator.GetUpdatedDirection(this.InvertDirectionIfOpposite, this._currentTarget.Transform.position, this._currentTarget.Movement.LastVelocity, default(Vector3)));
			this._hitChecker.TryHit(this._currentTarget);
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
			this._currentTarget.Controller.AddModifiers(this.Modifiers.Data, null, -1, false);
			this._currentTarget = null;
		}

		protected void OnPhaseChanged(BombScoreBoard.State state)
		{
			if (state != BombScoreBoard.State.PreReplay && state != BombScoreBoard.State.BombDelivery)
			{
				base.enabled = false;
			}
			else
			{
				base.enabled = true;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HazardArea));

		public HazardModifiers Modifiers;

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
	}
}
