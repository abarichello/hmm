using System;
using HeavyMetalMachines.Combat;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class ScaleGadgetRadiusInOutVFX : BaseVFX
	{
		private void LateUpdate()
		{
			if (this._t >= this._lifeTime && !this._shrinking)
			{
				this._shrinking = true;
				Vector3 initScale = this._initScale;
				this._initScale = this._targetScale;
				this._targetScale = initScale;
				this._t = 0f;
			}
			base.transform.localScale = Vector3.Lerp(this._initScale, this._targetScale, this._t / this._lifeTime);
			this._t += Time.deltaTime;
		}

		protected override void OnActivate()
		{
			PerkDamageIncreasingArea component = this._targetFXInfo.EffectTransform.GetComponent<PerkDamageIncreasingArea>();
			if (component == null)
			{
				Debug.LogError("Effect is dead");
				return;
			}
			this._initScale.Set(this._targetFXInfo.Gadget.GetRange(), this._targetFXInfo.Gadget.GetRange(), this._targetFXInfo.Gadget.GetRange());
			this._shrinking = false;
			this._targetScale.Set(this._targetFXInfo.Gadget.Radius, this._targetFXInfo.Gadget.Radius, this._targetFXInfo.Gadget.Radius);
			this._t = 0f;
			this._lifeTime = this._targetFXInfo.Gadget.LifeTime / 2f;
			base.gameObject.SetActive(true);
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
			base.gameObject.SetActive(false);
		}

		private Vector3 _initScale = Vector3.zero;

		private Vector3 _targetScale;

		private float _lifeTime;

		private float _t;

		private bool _shrinking;
	}
}
