using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkInstantConfig : BasePerk
	{
		protected override void Awake()
		{
			base.Awake();
			if (!this.Effect.Data.EffectInfo.Instantaneous)
			{
				this.Effect.Data.EffectInfo.Instantaneous = true;
			}
		}

		public override void PerkInitialized()
		{
			if (!this.Effect.Data.EffectInfo.Instantaneous)
			{
				this.Effect.Data.EffectInfo.Instantaneous = true;
			}
			if (this.WaitForParticle)
			{
				return;
			}
			this.m_fEffectElapsedTime = 0f;
			this.m_boEffectStarted = true;
		}

		private void FixedUpdate()
		{
			if (!this.WaitForParticle)
			{
				if (!this.m_boEffectStarted)
				{
					return;
				}
				this.m_fEffectElapsedTime += Time.fixedDeltaTime;
				if (this.m_fEffectElapsedTime >= this.EffectDuration)
				{
					this.EffectOver();
					this.m_boEffectStarted = false;
				}
				return;
			}
			else
			{
				if (this.GFX && this.GFX.IsAlive())
				{
					return;
				}
				this.EffectOver();
				this.m_boEffectStarted = false;
				return;
			}
		}

		private void EffectOver()
		{
			if (this.Effect == null)
			{
				Debug.LogError("effect is null");
			}
			if (this.Effect.Data == null)
			{
				Debug.LogError("data is null");
			}
			if (this.Effect.Data.EffectInfo == null)
			{
				Debug.LogError("EffectInfo is null");
			}
			if (!this.Effect.Data.EffectInfo.Instantaneous)
			{
				this.Effect.Data.EffectInfo.Instantaneous = true;
			}
			this.Effect.TriggerDestroy(-1, base.transform.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Default, false);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkInstantConfig));

		public bool WaitForParticle;

		public ParticleSystem GFX;

		public float EffectDuration;

		private bool m_boEffectStarted;

		private float m_fEffectElapsedTime;
	}
}
