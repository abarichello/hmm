using System;
using FMod;
using HeavyMetalMachines.Combat;
using NewParticleSystem;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class CarHitFeedback : GameHubBehaviour
	{
		private void Start()
		{
			CombatFeedback component = base.GetComponent<CombatFeedback>();
			if (component)
			{
				component.OnCollisionEvent += this.OnCarCollision;
				this.isOwner = component.Id.IsOwner;
			}
		}

		public void OnCarCollision(Vector3 worldSpacePosition, Vector3 worldSpaceDirection, float intensity, byte otherLayer)
		{
			this.lastHit = worldSpacePosition;
			switch (otherLayer)
			{
			case 19:
			case 22:
			case 24:
				if (this.bombBlockerCollision)
				{
					this.bombBlockerCollision.transform.position = worldSpacePosition;
					this.bombBlockerCollision.Play();
				}
				if (this.isOwner && this.hitBombBlockerAudio)
				{
					FMODAudioManager.PlayOneShotAt(this.hitBombBlockerAudio, worldSpacePosition, 0);
				}
				else if (!this.isOwner && this.hitBombBlockerOtherAudio)
				{
					FMODAudioManager.PlayOneShotAt(this.hitBombBlockerOtherAudio, worldSpacePosition, 0);
				}
				return;
			}
			if (this.heavyCollision)
			{
				this.heavyCollision.transform.position = worldSpacePosition;
				this.heavyCollision.Play();
			}
			if (this.isOwner && this.hitAudio)
			{
				FMODAudioManager.PlayOneShotAt(this.hitAudio, worldSpacePosition, 0);
			}
			else if (!this.isOwner && this.hitOtherAudio)
			{
				FMODAudioManager.PlayOneShotAt(this.hitOtherAudio, worldSpacePosition, 0);
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawSphere(this.lastHit, 2f);
		}

		public HoplonParticleSystem heavyCollision;

		public HoplonParticleSystem bombBlockerCollision;

		public FMODAsset hitAudio;

		public FMODAsset hitOtherAudio;

		public FMODAsset hitBombBlockerAudio;

		public FMODAsset hitBombBlockerOtherAudio;

		public bool isOwner;

		private Vector3 lastHit;
	}
}
