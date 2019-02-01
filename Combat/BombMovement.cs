using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[RequireComponent(typeof(Rigidbody))]
	public class BombMovement : CombatMovement
	{
		public override MovementInfo Info
		{
			get
			{
				return this._info.Movement;
			}
		}

		protected override void Awake()
		{
			GameHubBehaviour.Hub.BombManager.BombMovement = this;
			base.Awake();
		}

		protected override void Start()
		{
			base.Start();
			if (GameHubBehaviour.Hub.MatchMan.ArenaColliders != null && GameHubBehaviour.Hub.MatchMan.ArenaColliders.Length > 0)
			{
				this._shouldValidatePosition = true;
				this._updater = new TimedUpdater(1000, true, false);
			}
		}

		protected override void MovementFixedUpdate()
		{
			if (GameHubBehaviour.Hub.BombManager.ActiveBomb.IsSpawned && !this._updater.ShouldHalt())
			{
				Vector3 position = this._trans.position;
				if (MatchController.IsValidPoint(position, this.Info.DepthOfMeshValidators))
				{
					this._lastValidPosition = position;
				}
				else
				{
					this._trans.position = GameHubBehaviour.Hub.MatchMan.GetClosestValidPoint(position, this._lastValidPosition, this.Info.DepthOfMeshValidators, this.Info.ValidatorTeleportOffset);
				}
			}
		}

		public override void OnObjectSpawned(SpawnEvent msg)
		{
			this._info = GameHubBehaviour.Hub.BombManager.ActiveBomb.GetBombInfo();
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			base.OnObjectSpawned(msg);
		}

		public void PauseMovement(bool pause)
		{
			if (pause)
			{
				base.LockMovement();
				return;
			}
			base.UnlockMovement();
		}

		public override void AddLink(CombatLink newLink, bool force)
		{
			base.AddLink(newLink, force);
			if (force && !string.IsNullOrEmpty(newLink.Tag))
			{
				for (int i = 0; i < this._links.Count; i++)
				{
					if (this._links[i].Tag != newLink.Tag)
					{
						this._links[i].Break();
					}
				}
			}
		}

		public float GetSpeedAngleToX()
		{
			return base.LastVelocity.Rotation();
		}

		private const int VALIDATION_TIME = 1000;

		private BombInfo _info;

		private TimedUpdater _updater;

		private bool _shouldValidatePosition;

		private Vector3 _lastValidPosition;
	}
}
