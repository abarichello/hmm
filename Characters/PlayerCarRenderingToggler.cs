using System;
using System.Runtime.CompilerServices;
using HeavyMetalMachines.Infra.GameObjects;
using NewParticleSystem;
using UnityEngine;

namespace HeavyMetalMachines.Characters
{
	public class PlayerCarRenderingToggler
	{
		public PlayerCarRenderingToggler(GameObject playerCar)
		{
			this._playerCar = playerCar;
		}

		public bool ForceDestroyParticles { get; set; }

		public void DisableRendering(PlayerCarRenderingToggler.Mode mode)
		{
			if (mode == PlayerCarRenderingToggler.Mode.All)
			{
				this.SetMeshRenderersEnabledState(false);
				this.SetSkinnedMeshRenderersEnabledState(false);
				if (this.ForceDestroyParticles)
				{
					this.DestroyParticleSystems();
				}
				else
				{
					this.SetParticleSystemsEnabledState(false);
				}
			}
		}

		public void EnableRendering(PlayerCarRenderingToggler.Mode mode)
		{
			if (mode == PlayerCarRenderingToggler.Mode.All)
			{
				this.SetMeshRenderersEnabledState(true);
				this.SetSkinnedMeshRenderersEnabledState(true);
				if (!this.ForceDestroyParticles)
				{
					this.SetParticleSystemsEnabledState(true);
				}
			}
		}

		private void SetMeshRenderersEnabledState(bool enabledState)
		{
			this._playerCar.ForeachComponentInChildren(delegate(MeshRenderer renderer)
			{
				renderer.enabled = enabledState;
			});
		}

		private void SetSkinnedMeshRenderersEnabledState(bool enabledState)
		{
			this._playerCar.ForeachComponentInChildren(delegate(SkinnedMeshRenderer renderer)
			{
				renderer.enabled = enabledState;
			});
		}

		private void SetParticleSystemsEnabledState(bool enabledState)
		{
			this._playerCar.ForeachComponentInChildren(delegate(HoplonParticleSystem particleSystem)
			{
				particleSystem.enabled = enabledState;
				if (enabledState)
				{
					particleSystem.Play();
				}
			});
		}

		private void DestroyParticleSystems()
		{
			GameObject playerCar = this._playerCar;
			if (PlayerCarRenderingToggler.<>f__mg$cache0 == null)
			{
				PlayerCarRenderingToggler.<>f__mg$cache0 = new Action<HoplonParticleSystem>(Object.Destroy);
			}
			playerCar.ForeachComponentInChildren(PlayerCarRenderingToggler.<>f__mg$cache0);
		}

		private readonly GameObject _playerCar;

		[CompilerGenerated]
		private static Action<HoplonParticleSystem> <>f__mg$cache0;

		public enum Mode
		{
			None,
			All
		}
	}
}
