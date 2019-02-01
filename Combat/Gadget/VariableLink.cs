using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class VariableLink : BasicLink, LinkCreatedCallback.ILinkCreatedCallbackListener
	{
		private VariableLinkInfo LinkInfo
		{
			get
			{
				return base.Info as VariableLinkInfo;
			}
		}

		protected override void GadgetUpdate()
		{
			base.GadgetUpdate();
			if (!this._linkIsActive)
			{
				return;
			}
			if (this._myLink.IsBroken)
			{
				this._myLink = null;
				this._linkIsActive = false;
				this._currentRange = 0f;
				return;
			}
			float rangeModification = this.LinkInfo.RangeModificationPerSecond * Time.deltaTime;
			this.PressedUpdate(rangeModification);
			this._currentRange = Mathf.Max(this._currentRange, 0f);
			this._currentRange = Mathf.Min(this._currentRange, this.LinkInfo.MaxRangeModification);
			this._myLink.Range = this._initialRange - this._currentRange;
		}

		private void PressedUpdate(float rangeModification)
		{
			if (!this.PressedThisFrame)
			{
				this._buttonHasBeenReleased = true;
			}
			if (this.PressedThisFrame && this._buttonHasBeenReleased && !this._startPressedRangeModification)
			{
				this.StartRangeModification();
			}
			if (!this._startPressedRangeModification)
			{
				if ((long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() > this._timeToStarRangeModification)
				{
					this.StartRangeModification();
				}
				return;
			}
			this.SetLinkValues(rangeModification, this._initialTensionBreakForce + this.LinkInfo.AditionalTensionBreakForce);
			if (this.LinkInfo.DestroyLinkOnMaxRange && this._currentRange >= this.LinkInfo.MaxRangeModification)
			{
				base.DestroyExistingFiredEffects();
			}
		}

		private void StartRangeModification()
		{
			this._startPressedRangeModification = true;
			this.FireRangeModificationSFX();
		}

		private void SetLinkValues(float rangeModification, float tension)
		{
			this._currentRange += rangeModification;
			this._myLink.TensionBreakForce = tension;
		}

		public void OnLinkCreatedCallback(LinkCreatedCallback evt)
		{
			this._myLink = evt.Link;
			this._initialRange = evt.Link.Range;
			this._initialTensionBreakForce = evt.Link.TensionBreakForce;
			this._linkIsActive = true;
			this._timeToStarRangeModification = (long)(this.LinkInfo.LifeTimeMillisToRangeModStart + GameHubBehaviour.Hub.GameTime.GetPlaybackTime());
			this._buttonHasBeenReleased = false;
			this._startPressedRangeModification = false;
		}

		private void FireRangeModificationSFX()
		{
			if (string.IsNullOrEmpty(this.LinkInfo.OnRangeModificationStartEffect.Effect))
			{
				return;
			}
			EffectEvent effectEvent = base.GetEffectEvent(this.LinkInfo.OnRangeModificationStartEffect);
			effectEvent.LifeTime = this.LinkInfo.MaxRangeModification / this.LinkInfo.RangeModificationPerSecond;
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		private CombatLink _myLink;

		private bool _linkIsActive;

		private float _initialRange;

		private float _initialTensionBreakForce;

		private float _currentRange;

		private bool _startPressedRangeModification;

		private long _timeToStarRangeModification = -1L;

		private bool _buttonHasBeenReleased;
	}
}
