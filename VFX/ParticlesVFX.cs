using System;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using NewParticleSystem;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class ParticlesVFX : BaseVFX
	{
		protected override void OnActivate()
		{
			this.CanCollectToCache = false;
			float y = base.transform.position.y;
			for (int i = 0; i < this.particleSystems.Length; i++)
			{
				HoplonParticleSystem hoplonParticleSystem = this.particleSystems[i];
				if (hoplonParticleSystem == null)
				{
					HeavyMetalMachines.Utils.Debug.Assert(false, string.Format("Effect:{0} is with null particleSystem on ist's config! Please, fix that now!", base.transform.name), HeavyMetalMachines.Utils.Debug.TargetTeam.TechnicalArtist);
				}
				else
				{
					if (hoplonParticleSystem is HMMTeamParticleSystem)
					{
						VFXTeam team = VFXTeam.Ally;
						if (this._targetFXInfo.Owner != null)
						{
							PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Owner.ObjId);
							if (playerOrBotsByObjectId != null)
							{
								TeamKind team2 = playerOrBotsByObjectId.Team;
								if (team2 != GameHubBehaviour.Hub.Players.CurrentPlayerTeam)
								{
									team = VFXTeam.Enemy;
								}
							}
							else
							{
								ParticlesVFX.Log.WarnFormat("Hub.Players.GetPlayerOrBotsByObjectId did not find owner to check HMMTeamParticleSystem. May be related to QAHMM-22224. Effect{0}", new object[]
								{
									base.transform.name
								});
							}
						}
						else
						{
							ParticlesVFX.Log.WarnFormat("_targetFXInfo.Owner is null to check HMMTeamParticleSystem. May be related to QAHMM-22224. Effect{0}", new object[]
							{
								base.transform.name
							});
						}
						((HMMTeamParticleSystem)hoplonParticleSystem).Play(team);
					}
					else
					{
						hoplonParticleSystem.Play();
					}
					hoplonParticleSystem.SetYPlaneCollision(y);
				}
			}
			if (this.spawnStage == BaseVFX.SpawnState.OnDestroy)
			{
				this.expirationTime = Time.time + 40f;
			}
			this.currentState = ParticlesVFX.State.Activated;
		}

		private void Update()
		{
			ParticlesVFX.State state = this.currentState;
			if (state != ParticlesVFX.State.Activated)
			{
				if (state == ParticlesVFX.State.WaitingForParticlesDeath)
				{
					bool flag = true;
					for (int i = 0; i < this.particleSystems.Length; i++)
					{
						HoplonParticleSystem hoplonParticleSystem = this.particleSystems[i];
						if (hoplonParticleSystem.GetAliveParticlesCount() > 0)
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						this.currentState = ParticlesVFX.State.Dead;
						this.CanCollectToCache = true;
						this.expirationTime = 0f;
					}
				}
			}
			else if (this.spawnStage == BaseVFX.SpawnState.OnDestroy)
			{
				bool flag2 = Time.time > this.expirationTime;
				bool flag3 = true;
				for (int j = 0; j < this.particleSystems.Length; j++)
				{
					HoplonParticleSystem hoplonParticleSystem2 = this.particleSystems[j];
					if (hoplonParticleSystem2.IsPlaying)
					{
						flag3 = false;
						break;
					}
				}
				if (flag3 || flag2)
				{
					this.OnDeactivate();
				}
			}
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
			this.expirationTime = Time.time + 40f;
			if (this.particleSystems != null)
			{
				for (int i = 0; i < this.particleSystems.Length; i++)
				{
					HoplonParticleSystem hoplonParticleSystem = this.particleSystems[i];
					if (hoplonParticleSystem != null)
					{
						hoplonParticleSystem.Stop();
					}
				}
			}
			if (!this.waitForParticlesDeath)
			{
				this.currentState = ParticlesVFX.State.Dead;
				this.CanCollectToCache = true;
				return;
			}
			this.currentState = ParticlesVFX.State.WaitingForParticlesDeath;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ParticlesVFX));

		public HoplonParticleSystem[] particleSystems;

		private ParticlesVFX.State currentState;

		public bool waitForParticlesDeath = true;

		private float expirationTime;

		private const float TotalExpirationTime = 40f;

		private enum State
		{
			Idle,
			Activated,
			WaitingForParticlesDeath,
			Dead
		}
	}
}
