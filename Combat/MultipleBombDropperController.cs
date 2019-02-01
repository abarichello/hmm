using System;
using NewParticleSystem;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class MultipleBombDropperController : GameHubBehaviour
	{
		private void Start()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
		}

		private void OnEnable()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			GameHubBehaviour.Hub.bombDropperAnimatorController.bombDroppers.Add(this);
		}

		private void OnDisable()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			GameHubBehaviour.Hub.bombDropperAnimatorController.bombDroppers.Remove(this);
		}

		private void Update()
		{
			BombDropperController.State state = this.state;
			if (state != BombDropperController.State.Idle)
			{
				if (state != BombDropperController.State.Run)
				{
					if (state == BombDropperController.State.Stop)
					{
						this.Stop();
					}
				}
				else
				{
					this.state = BombDropperController.State.Stop;
				}
				return;
			}
		}

		private void SetAnimatorBool(bool activate)
		{
			if (this.DropperAnimators == null || this.DropperAnimators.Length == 0)
			{
				return;
			}
			for (int i = 0; i < this.DropperAnimators.Length; i++)
			{
				this.DropperAnimators[i].SetBool("drop_activate", activate);
			}
		}

		public void Play()
		{
			this.SetAnimatorBool(true);
			for (int i = 0; i < this.particleSystems.Length; i++)
			{
				HoplonParticleSystem hoplonParticleSystem = this.particleSystems[i];
				hoplonParticleSystem.Play();
			}
			this.state = BombDropperController.State.Run;
		}

		private void Stop()
		{
			this.SetAnimatorBool(false);
			for (int i = 0; i < this.particleSystems.Length; i++)
			{
				HoplonParticleSystem hoplonParticleSystem = this.particleSystems[i];
				hoplonParticleSystem.Stop();
			}
			this.state = BombDropperController.State.Idle;
		}

		private void OnDrawGizmos()
		{
			BombDropperController.State state = this.state;
			if (state != BombDropperController.State.Idle)
			{
				if (state != BombDropperController.State.Run)
				{
					if (state == BombDropperController.State.Stop)
					{
						Gizmos.color = Color.red;
					}
				}
				else
				{
					Gizmos.color = Color.green;
				}
			}
			else
			{
				Gizmos.color = Color.yellow;
			}
			if (this.Dummies.Length == 1)
			{
				Gizmos.DrawWireSphere(this.Dummies[0].transform.position, 3f);
				return;
			}
			for (int i = 1; i < this.Dummies.Length; i++)
			{
				Gizmos.DrawLine(this.Dummies[i - 1].transform.position, this.Dummies[i].transform.position);
			}
		}

		[SerializeField]
		private Animator[] DropperAnimators;

		public Transform[] Dummies;

		public HoplonParticleSystem[] particleSystems;

		private BombDropperController.State state;
	}
}
