using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	internal class HudGadgetController : GameHubBehaviour
	{
		private void Start()
		{
			this._isCountingDown = false;
			this._countdown = 5f;
			this._countdownTimer = 0f;
			this.Countdown.value = 0f;
			if (this.isPassive)
			{
				this.Button.isEnabled = false;
			}
		}

		private void Update()
		{
			if (!this.isPassive && this._countdownTimer > 0f)
			{
				this._countdownTimer -= Time.deltaTime;
				if (this._countdownTimer < 0f)
				{
					this._isCountingDown = false;
					this.Countdown.value = 0f;
					this.Button.isEnabled = true;
					this.Animation.Emit(1);
				}
				else
				{
					this.Countdown.value = this._countdownTimer / this._countdown;
				}
			}
		}

		private void GadgetActivated()
		{
			this._countdownTimer = this._countdown;
			this.Button.isEnabled = false;
			this.Animation.Emit(1);
		}

		public UISlider Countdown;

		public UIButton Button;

		public ParticleSystem Animation;

		public bool isPassive;

		public float _countdown;

		public float _countdownTimer;

		public bool _isCountingDown;
	}
}
