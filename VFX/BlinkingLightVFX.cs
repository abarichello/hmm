using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[RequireComponent(typeof(Light))]
	public class BlinkingLightVFX : BaseVFX
	{
		private void Awake()
		{
			this._light = base.GetComponent<Light>();
			this._initialIntensity = this._light.intensity;
			this._light.enabled = false;
		}

		protected override void OnActivate()
		{
			MatchPlayers players = GameHubBehaviour.Hub.Players;
			this._light.color = ((players.CurrentPlayerTeam != players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Owner.ObjId).Team) ? this.EnemyColor : this.TeamColor);
			this._light.enabled = true;
			this._activationTime = Time.time;
			this.CanCollectToCache = false;
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
			this._light.enabled = false;
			this.CanCollectToCache = true;
		}

		private void LateUpdate()
		{
			float num = this.Frequency;
			float num2 = Time.time - this._activationTime;
			BlinkingLightVFX.BlinkMode mode = this.Mode;
			if (mode != BlinkingLightVFX.BlinkMode.LinearIncrease)
			{
				if (mode == BlinkingLightVFX.BlinkMode.QuadraticIncrease)
				{
					num += num2 * num2;
				}
			}
			else
			{
				num += num2;
			}
			this._light.intensity = 0.5f * this._initialIntensity * (1f - Mathf.Cos(6.2831855f * num * num2));
		}

		[Tooltip("The light color of your own team (blue team).")]
		public Color TeamColor = Color.blue;

		[Tooltip("The light color of the enemy team (red team).")]
		public Color EnemyColor = Color.red;

		[Tooltip("The blinking frequency (Hz).")]
		public float Frequency;

		public BlinkingLightVFX.BlinkMode Mode;

		private Light _light;

		private float _activationTime;

		private float _initialIntensity;

		public enum BlinkMode
		{
			Constant,
			LinearIncrease,
			QuadraticIncrease
		}
	}
}
