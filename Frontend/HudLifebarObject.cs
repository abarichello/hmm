using System;
using HeavyMetalMachines.Combat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudLifebarObject : GameHubBehaviour
	{
		protected void Awake()
		{
			this._maxAlpha = 1f;
			if (GameHubBehaviour.Hub == null)
			{
				this.Setup(null);
				this._isAlive = true;
			}
		}

		public void CreatePool()
		{
			this.uiLifeBar.ShieldColor = this.HudLifebarSettings.ShieldColor;
			this.uiLifeBar.BleedColor = this.HudLifebarSettings.BleedColor;
		}

		public virtual void Setup(CombatObject combatObject)
		{
			this._combatObject = combatObject;
			this._healthLifebarInterpolator = new HudLifebarObject.LifebarInterpolator(this.HudLifebarSettings.HpTimeInSec);
			this._isAlive = false;
			if (this._combatObject != null)
			{
				this.HudLifebarFollowTarget.SetTarget(this._combatObject, this._combatObject.transform);
				this._combatObject.ListenToObjectSpawn += this.CombatObjectOnObjectSpawn;
				this._combatObject.ListenToObjectUnspawn += this.CombatObjectOnObjectUnspawn;
				this._isAlive = this._combatObject.IsAlive();
				this.HudLifebarFollowTarget.IsAlive = this._isAlive;
			}
			this._lifebarWidth = this.HudLifebarSettings.LifebarWidth;
			this._lifebarHeight = this.HudLifebarSettings.LifebarHeight;
			Vector2 sizeDelta = this._lifebarRectTransform.sizeDelta;
			sizeDelta.x = this._lifebarWidth;
			sizeDelta.y = this._lifebarHeight;
			this._lifebarRectTransform.sizeDelta = sizeDelta;
			this.FeedLifebarData();
			this._maxFullHp = this.GetMaxFullHp();
			this._lastFullHp = this._lifebarDataWrapper.Hp + this._lifebarDataWrapper.HpTemp;
			this.RenderBarHp(0f, 0f, 0f);
		}

		protected virtual void CombatObjectOnObjectSpawn(CombatObject combatObject, SpawnEvent msg)
		{
			this._isAlive = true;
			this.HudLifebarFollowTarget.IsAlive = true;
		}

		protected virtual void CombatObjectOnObjectUnspawn(CombatObject combatObject, UnspawnEvent msg)
		{
			this._isAlive = false;
			this.HudLifebarFollowTarget.IsAlive = false;
		}

		public virtual void SetVisibility(bool visible)
		{
			this._lifebarObject.SetActive(visible);
		}

		public void SetCanBeVisible(bool can)
		{
			this._canBeVisible = can;
		}

		private float GetMaxFullHp()
		{
			return Mathf.Max(this._lifebarDataWrapper.HpMax, this._lifebarDataWrapper.Hp + this._lifebarDataWrapper.HpTemp);
		}

		private void RenderBarHp(float healthHp, float shieldHp, float bleedHp)
		{
			bool flag = Math.Abs(this.uiLifeBar.HPAmount - healthHp) > 0.001f || Math.Abs(this.uiLifeBar.BleedHp - bleedHp) > 0.001f || Math.Abs(this.uiLifeBar.tempHPAmount - shieldHp) > 0.001f || Math.Abs(this.uiLifeBar.maxHp - this._maxFullHp) > 0.001f;
			this.uiLifeBar.HPAmount = healthHp;
			this.uiLifeBar.maxHp = this._maxFullHp;
			this.uiLifeBar.tempHPAmount = shieldHp;
			this.uiLifeBar.BleedHp = bleedHp;
			if (flag)
			{
				this.uiLifeBar.SetGeometryDirty();
			}
		}

		private void FeedLifebarData()
		{
			if (this._combatObject == null)
			{
				return;
			}
			CombatData data = this._combatObject.Data;
			this._lifebarDataWrapper.Hp = data.HP;
			this._lifebarDataWrapper.HpMax = (float)data.HPMax;
			this._lifebarDataWrapper.HpTemp = data.HPTemp;
		}

		protected void Update()
		{
			if (this.HudLifebarFollowTarget.IsOutScreen || !this._isAlive || !this._canBeVisible)
			{
				this.SetVisibility(false);
				this._bleedHp = 0f;
				this._lastFullHp = 0f;
				return;
			}
			if (!this._lifebarObject.activeSelf && this._canBeVisible)
			{
				this.SetVisibility(true);
			}
			this.FeedLifebarData();
			this.RenderUpdate();
		}

		protected virtual void RenderUpdate()
		{
			if (this.HackedToEP)
			{
				this._maxFullHp = (float)this._combatObject.Data.EPMax;
				this.RenderBarHp(this._combatObject.Data.EP, 0f, 0f);
				return;
			}
			float maxFullHp = this.GetMaxFullHp();
			if (Math.Abs(maxFullHp - this._maxFullHp) > 0.01f)
			{
				this._maxFullHp = maxFullHp;
			}
			float num = this._lifebarDataWrapper.Hp + this._lifebarDataWrapper.HpTemp;
			bool flag = this._lastFullHp > num;
			bool flag2 = this._lifebarDataWrapper.HpTemp > 0.01f;
			float healthHp = this._healthLifebarInterpolator.Update(this._lifebarDataWrapper.Hp, (Time.timeScale <= 0f) ? Time.unscaledDeltaTime : Time.deltaTime);
			if (flag && this._lifebarDataWrapper.HpMax >= maxFullHp && (!flag2 || this.HudLifebarSettings.BleedOnShieldDamage))
			{
				bool flag3 = false;
				if (this.HudLifebarSettings.BleedIgnoreDotDamage)
				{
					float num2 = this._lastFullHp - num;
					flag3 = (num2 < this.HudLifebarSettings.BleedMaxDotDamagePct * maxFullHp);
				}
				if (this._bleedHp <= 0f)
				{
					this._bleedHp = Mathf.Min(this._lastFullHp, maxFullHp);
					this._bleedTime = this.HudLifebarSettings.BleedDelayTimeInSec;
					this._bleedReductionModifier = this.HudLifebarSettings.BleedHpReductionOverSec;
				}
				else if (!flag3)
				{
					if (this.HudLifebarSettings.BleedResetDelayOnDamage)
					{
						this._bleedTime = this.HudLifebarSettings.BleedDelayTimeInSec;
					}
					if (this.HudLifebarSettings.BleedSlowOnDamage)
					{
						this._bleedSlow = true;
						if (this.HudLifebarSettings.BleedSlowDecayResetOnDamage)
						{
							this._bleedReductionModifier = this.HudLifebarSettings.BleedSlowHpReductionOverSec;
						}
					}
				}
			}
			if (this._bleedHp > num)
			{
				if (this._bleedTime > 0f)
				{
					this._bleedTime -= Time.deltaTime;
				}
				else
				{
					this._bleedHp -= Time.deltaTime * this._bleedReductionModifier;
					if (this._bleedSlow)
					{
						this._bleedReductionModifier += Time.deltaTime * this.HudLifebarSettings.BleedSlowHpIncreaseOverSec;
						this._bleedReductionModifier = Mathf.Min(this._bleedReductionModifier, this.HudLifebarSettings.BleedHpReductionOverSec);
					}
				}
				if (this._bleedHp < 0f)
				{
					this._bleedHp = 0f;
					this._bleedSlow = false;
				}
			}
			else
			{
				this._bleedHp = 0f;
				this._bleedSlow = false;
			}
			this.RenderBarHp(healthHp, this._lifebarDataWrapper.HpTemp, this._bleedHp);
			this._lastFullHp = num;
		}

		protected void SetMaxAlpha(float alpha)
		{
			this._maxAlpha = alpha;
		}

		protected virtual void OnValidate()
		{
			this._lifebarObject = ((!this._lifebarRectTransform) ? null : this._lifebarRectTransform.gameObject);
		}

		protected virtual void OnDestroy()
		{
			this.Dispose();
		}

		private void Dispose()
		{
			this.HudLifebarFollowTarget.Dispose();
			if (this._combatObject != null)
			{
				this._combatObject.ListenToObjectSpawn -= this.CombatObjectOnObjectSpawn;
				this._combatObject.ListenToObjectUnspawn -= this.CombatObjectOnObjectUnspawn;
				this._isAlive = false;
				this._combatObject = null;
			}
			this._bleedHp = 0f;
			this._lastFullHp = 0f;
		}

		public void HackToEP(bool hack)
		{
			if (this.HackedToEP == hack)
			{
				return;
			}
			this.HackedToEP = hack;
			base.GetComponent<HudLifebarFollowTarget>().OffsetFromCenterY = ((!hack) ? this.HudLifebarSettings.DefaultCharacterOffset.y : 50f);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HudLifebarObject));

		public HudLifebarSettings HudLifebarSettings;

		public HudLifebarFollowTarget HudLifebarFollowTarget;

		public UILifeBar uiLifeBar;

		private bool _canBeVisible = true;

		[SerializeField]
		private RectTransform _lifebarRectTransform;

		[SerializeField]
		[HideInInspector]
		private GameObject _lifebarObject;

		private float _lifebarWidth;

		private float _lifebarHeight;

		internal float _maxFullHp;

		internal float _lastFullHp;

		private float _bleedTime;

		private float _bleedHp;

		private float _bleedReductionModifier;

		private bool _bleedSlow;

		private HudLifebarObject.LifebarInterpolator _healthLifebarInterpolator;

		internal CombatObject _combatObject;

		[SerializeField]
		internal HudLifebarObject.LifebarDataWrapper _lifebarDataWrapper;

		private bool _isAlive;

		[SerializeField]
		private float _maxAlpha;

		public bool HackedToEP;

		[Serializable]
		internal struct LifebarDataWrapper
		{
			public float Hp;

			public float HpTemp;

			public float HpMax;

			public bool StreamObjectVisible;
		}

		private class LifebarInterpolator
		{
			public LifebarInterpolator(float durationInSec)
			{
				this._durationInSec = durationInSec;
			}

			public float Update(float value, float deltaTime)
			{
				if (Math.Abs(value - this._endValue) > 0.01f)
				{
					this._startValue = this._currentValue;
					this._endValue = value;
					this._timeInSec = 0f;
				}
				if (this._timeInSec > this._durationInSec)
				{
					return this._endValue;
				}
				this._timeInSec += deltaTime;
				this._timeInSec = Mathf.Clamp(this._timeInSec, 0f, this._durationInSec);
				this._currentValue = Mathf.Lerp(this._startValue, this._endValue, this._timeInSec / this._durationInSec);
				return this._currentValue;
			}

			private readonly float _durationInSec;

			private float _startValue;

			private float _endValue;

			private float _currentValue;

			private float _timeInSec;
		}
	}
}
