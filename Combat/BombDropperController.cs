using System;
using NewParticleSystem;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	internal class BombDropperController : GameHubBehaviour
	{
		private void OnEnable()
		{
			GameHubBehaviour.Hub.BombManager.ClientListenToBombDrop += this.OnBombDrop;
		}

		private void OnDisable()
		{
			GameHubBehaviour.Hub.BombManager.ClientListenToBombDrop -= this.OnBombDrop;
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
						this.DropperAnimator.SetBool("drop_activate", false);
						for (int i = 0; i < this.particleSystems.Length; i++)
						{
							HoplonParticleSystem hoplonParticleSystem = this.particleSystems[i];
							hoplonParticleSystem.Stop();
						}
						this.state = BombDropperController.State.Idle;
					}
				}
				else
				{
					this.timeToRun -= Time.deltaTime;
					if (this.timeToRun <= 0f)
					{
						this.state = BombDropperController.State.Stop;
					}
				}
				return;
			}
		}

		private void OnBombDrop(BombInstance bombinstance, SpawnReason reason, int causer)
		{
			if (reason == SpawnReason.TriggerDrop)
			{
				Transform transform = BombVisualController.GetInstance(false).transform;
				if (transform == null)
				{
					return;
				}
				if (Vector3.Distance(base.transform.position, transform.position) < this.Range)
				{
					this.timeToRun = 0.5f;
					this.DropperAnimator.SetBool("drop_activate", true);
					for (int i = 0; i < this.particleSystems.Length; i++)
					{
						HoplonParticleSystem hoplonParticleSystem = this.particleSystems[i];
						hoplonParticleSystem.Play();
					}
					this.state = BombDropperController.State.Run;
				}
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawWireSphere(base.transform.position, this.Range);
		}

		public Animator DropperAnimator;

		public HoplonParticleSystem[] particleSystems;

		public float Range;

		private BombDropperController.State state;

		private float timeToRun;

		public enum State
		{
			Idle,
			Run,
			Stop
		}
	}
}
