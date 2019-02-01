using System;
using HeavyMetalMachines.Combat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	internal class UIRespawnController : GameHubBehaviour, PlayerBuildComplete.IPlayerBuildCompleteListener
	{
		private void Awake()
		{
			this.CoolDownGameObject.SetActive(false);
			this._activated = false;
		}

		public void StartCountingDown()
		{
			this._activated = true;
			this.CoolDownGameObject.SetActive(true);
		}

		private void Update()
		{
			if (!this._configured)
			{
				return;
			}
			if (this._playerCombat != null && !this._playerCombat.IsAlive())
			{
				if (!this._activated)
				{
					this._activated = true;
					this.CoolDownGameObject.SetActive(true);
					string name = this.CoolDownGlowAnimation.clip.name;
					this.CoolDownGlowAnimation.Rewind();
					this.CoolDownGlowAnimation.Sample();
					this.CoolDownGlowAnimation[name].speed = 1f;
					this.CoolDownGlowAnimation.Play();
					string name2 = this.CoolDownSkullsAnimation.clip.name;
					this.CoolDownSkullsAnimation.Rewind();
					this.CoolDownSkullsAnimation.Sample();
					this.CoolDownSkullsAnimation[name2].speed = 1f;
					this.CoolDownSkullsAnimation.Play();
				}
				this._timeToRespawn = (float)this._playerSpawnController.GetDeathTimeRemainingMillis();
				this._maxTimeDead = (float)this._playerSpawnController.GetPlayerMaxTimeDeadMillis();
				this._time = new TimeSpan(0, 0, Math.Abs((int)(this._timeToRespawn * 0.001f)));
				int minutes = this._time.Minutes;
				int seconds = this._time.Seconds;
				this.NumberCountdown.text = (minutes * 100 + seconds + 1).ToString("00:00");
				this.Countdown.fillAmount = this._timeToRespawn / this._maxTimeDead;
			}
			else if (this._activated)
			{
				this._activated = false;
				string name3 = this.CoolDownSkullsAnimation.clip.name;
				this.CoolDownSkullsAnimation.Stop(name3);
				this.CoolDownSkullsAnimation[name3].speed = -1f;
				this.CoolDownSkullsAnimation[name3].time = this.CoolDownSkullsAnimation[name3].length;
				this.CoolDownSkullsAnimation.Sample();
				this.CoolDownSkullsAnimation.Play(name3);
				string name4 = this.CoolDownGlowAnimation.clip.name;
				this.CoolDownGlowAnimation.Stop(name4);
				this.CoolDownGlowAnimation[name4].speed = -1f;
				this.CoolDownGlowAnimation[name4].time = this.CoolDownGlowAnimation[name4].length;
				this.CoolDownGlowAnimation.Sample();
				this.CoolDownGlowAnimation.Play(name4);
			}
			else if (!this.CoolDownSkullsAnimation.isPlaying)
			{
				this.CoolDownGameObject.SetActive(false);
			}
		}

		public void OnPlayerBuildComplete(PlayerBuildComplete evt)
		{
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData == null || GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance == null || this._playerCombat != null || this._playerSpawnController != null)
			{
				return;
			}
			this._playerCombat = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetBitComponent<CombatObject>();
			this._playerSpawnController = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetBitComponent<SpawnController>();
			this._configured = true;
		}

		private void OnDestroy()
		{
			this._playerCombat = null;
			this._playerSpawnController = null;
			this._configured = false;
		}

		public UISprite Countdown;

		public UILabel NumberCountdown;

		public GameObject CoolDownGameObject;

		public Animation CoolDownGlowAnimation;

		public Animation CoolDownSkullsAnimation;

		private CombatObject _playerCombat;

		private SpawnController _playerSpawnController;

		private bool _configured;

		private bool _activated;

		private float _maxTimeDead;

		private float _timeToRespawn;

		private TimeSpan _time;
	}
}
