using System;
using HeavyMetalMachines.Tutorial.InGame;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class KeyPressTimerTutorialBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnClient()
		{
			base.StartBehaviourOnClient();
			Debug.Log("KeyPressTimerTutorial started.", base.gameObject);
			this._adButtonTime = this.VerticalMovement;
			this._driftButtonTime = this.Drift;
		}

		protected override void UpdateOnClient()
		{
			base.UpdateOnClient();
			if ((!this._adButtonTime && !this._driftButtonTime) || base.playerController == null || base.playerController.Inputs.Dir.y == 0f)
			{
				return;
			}
			if (!this._buttonDReady && this._adButtonTime && base.playerController.Inputs.Dir.x == 1f)
			{
				this._buttonDTimer += Time.deltaTime;
				this.UpdateTimerInterface(TutorialButtonKind.D, this._buttonDTimer / this.ButtonDMaxTime);
				if (this._buttonDTimer >= this.ButtonDMaxTime)
				{
					this._buttonDReady = true;
				}
			}
			if (!this._buttonAReady && this._adButtonTime && base.playerController.Inputs.Dir.x == -1f)
			{
				this._buttonATimer += Time.deltaTime;
				this.UpdateTimerInterface(TutorialButtonKind.A, this._buttonATimer / this.ButtonDMaxTime);
				if (this._buttonATimer >= this.ButtonAMaxTime)
				{
					this._buttonAReady = true;
				}
			}
			if (this._buttonAReady && this._buttonDReady)
			{
				this._adButtonTime = false;
				this._buttonAReady = false;
				this._buttonDReady = false;
				this.CompleteBehaviourAndSync();
				return;
			}
		}

		public void UpdateTimerInterface(TutorialButtonKind button, float timer)
		{
			TutorialUIController.Instance.UpdateButton(button, timer);
		}

		private bool _adButtonTime;

		private bool _driftButtonTime;

		public float ButtonAMaxTime = 5f;

		public float ButtonDMaxTime = 5f;

		public float ButtonSpaceMaxTime = 5f;

		private float _buttonATimer;

		private float _buttonDTimer;

		private float _buttonSpaceTimer;

		private bool _buttonAReady;

		private bool _buttonDReady;

		public bool VerticalMovement;

		public bool Drift;
	}
}
