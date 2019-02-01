using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Render;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudDamageFeedbackController : GameHubBehaviour, PlayerBuildComplete.IPlayerBuildCompleteListener
	{
		public void Start()
		{
			this._hudDamageFeedback = this.GetHudDamageFeedback();
			this._hudDamageFeedback.enabled = true;
			this._isHudDamageFeedbackActive = false;
		}

		private HudDamageFeedback GetHudDamageFeedback()
		{
			foreach (Camera camera in UnityEngine.Object.FindObjectsOfType<Camera>())
			{
				if (camera.gameObject.layer == 31)
				{
					HudDamageFeedback component = camera.GetComponent<HudDamageFeedback>();
					if (component != null)
					{
						return component;
					}
				}
			}
			return null;
		}

		public void OnDestroy()
		{
			this._isHudDamageFeedbackActive = false;
			this._hudDamageFeedback.SetAlpha(0f);
			this._hudDamageFeedback.enabled = false;
			this._hudDamageFeedback = null;
			if (this._combatObject != null)
			{
				this._combatObject.OnDamageReceived -= this.OnDamageReceived;
			}
			this._combatObject = null;
		}

		public void Update()
		{
			if (!this._isHudDamageFeedbackActive)
			{
				return;
			}
			if (this._fadeIn)
			{
				if (this._countTimeInSec < 0.5f)
				{
					this._countTimeInSec += Time.deltaTime;
					this._countAlphaInSec += Time.deltaTime;
					if (this._countAlphaInSec > 0.5f)
					{
						this._countAlphaInSec = 0.5f;
					}
					this._hudDamageFeedback.SetAlpha(this._countAlphaInSec / 0.5f * this._normalizedDamageModifier);
					return;
				}
				this._countTimeInSec = this._fadeOutTimeInSec;
				this._fadeIn = false;
			}
			if (this._countTimeInSec > 0f)
			{
				this._countTimeInSec -= Time.deltaTime;
				this._hudDamageFeedback.SetAlpha(this._countTimeInSec / this._fadeOutTimeInSec * this._normalizedDamageModifier);
				return;
			}
			this._hudDamageFeedback.SetAlpha(0f);
			this._isHudDamageFeedbackActive = false;
		}

		public void OnPlayerBuildComplete(PlayerBuildComplete evt)
		{
			PlayerData currentPlayerData = GameHubBehaviour.Hub.Players.CurrentPlayerData;
			if (currentPlayerData == null || evt.Id != currentPlayerData.PlayerCarId)
			{
				return;
			}
			this._combatObject = currentPlayerData.CharacterInstance.GetComponent<CombatObject>();
			this._combatObject.OnDamageReceived += this.OnDamageReceived;
		}

		private void OnDamageReceived(float damage, int id)
		{
			if (damage > (float)this.HudDamageFeedbackMaxDamage)
			{
				damage = (float)this.HudDamageFeedbackMaxDamage;
			}
			float num = Mathf.Max(this.MinimumDamageFactor, damage / (float)this.HudDamageFeedbackMaxDamage);
			if (this._isHudDamageFeedbackActive && num < this._normalizedDamageModifier)
			{
				return;
			}
			this._fadeOutTimeInSec = Mathf.Max(0.3f, 0.5f * num);
			this._normalizedDamageModifier = num;
			if (this._isHudDamageFeedbackActive)
			{
				if (!this._fadeIn)
				{
					this._countAlphaInSec = this._countTimeInSec / this._fadeOutTimeInSec * 0.5f;
					this._fadeIn = true;
				}
				else
				{
					this._countAlphaInSec = Mathf.Min(this._countAlphaInSec, 0.5f);
				}
				this._countTimeInSec = 0f;
			}
			else
			{
				this._countAlphaInSec = 0f;
				this._countTimeInSec = 0f;
				this._fadeIn = true;
				this._isHudDamageFeedbackActive = true;
			}
		}

		[Header("[Setup]")]
		public int HudDamageFeedbackMaxDamage = 100;

		public float MinimumDamageFactor = 0.1f;

		private const float HudDamageFeedbackFadeInInSec = 0.5f;

		private const float HudDamageFeedbackMaxTimeInSec = 0.5f;

		private const float MinimumFadeoutTime = 0.3f;

		private HudDamageFeedback _hudDamageFeedback;

		private CombatObject _combatObject;

		private bool _isHudDamageFeedbackActive;

		private float _countTimeInSec;

		private float _countAlphaInSec;

		private float _normalizedDamageModifier;

		private float _fadeOutTimeInSec;

		private bool _fadeIn;
	}
}
