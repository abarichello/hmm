using System;
using HeavyMetalMachines.Localization;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HMMTooltipTrigger : GameHubBehaviour
	{
		private void OnTooltip(bool isHovering)
		{
			if (!base.enabled)
			{
				return;
			}
			if (!isHovering)
			{
				this.HideTooltip();
				return;
			}
			if (this._mustShowTooltip)
			{
				return;
			}
			this._nextDisplayTime = Time.unscaledTime + this.DelayInSec;
			this._mustShowTooltip = true;
		}

		public static bool IsDisplayingTooltip
		{
			get
			{
				return HMMTooltipTrigger._isDisplayingTooltip;
			}
		}

		private void ShowTooltip()
		{
			if (HMMTooltipTrigger._isDisplayingTooltip)
			{
				return;
			}
			HMMTooltipTrigger._isDisplayingTooltip = true;
			Vector3 targetPosition = (!(this.TargetPosition != null)) ? Vector3.zero : this.TargetPosition.position;
			UITooltip.Show((!this.TranslateText || !GameHubBehaviour.Hub || !GameHubBehaviour.Hub.Net) ? this.TooltipText : Language.Get(this.TooltipText, this.Sheet), this.FollowMouse, this.ScaleTextInOverflow, targetPosition);
		}

		private void HideTooltip()
		{
			if (!HMMTooltipTrigger._isDisplayingTooltip)
			{
				return;
			}
			HMMTooltipTrigger._isDisplayingTooltip = false;
			this._mustShowTooltip = false;
			UITooltip.Hide();
		}

		private void Update()
		{
			if (HMMTooltipTrigger._isDisplayingTooltip)
			{
				return;
			}
			if (!this._mustShowTooltip)
			{
				return;
			}
			if (this._nextDisplayTime < Time.unscaledTime)
			{
				return;
			}
			this.ShowTooltip();
		}

		private void OnPress()
		{
			this._mustShowTooltip = false;
		}

		private void OnDisable()
		{
			if (this._mustShowTooltip)
			{
				this.HideTooltip();
			}
		}

		public string TooltipText;

		public TranslationSheets Sheet;

		public bool TranslateText;

		public float DelayInSec;

		public bool ScaleTextInOverflow;

		public bool FollowMouse;

		public Transform TargetPosition;

		private float _nextDisplayTime;

		private bool _mustShowTooltip;

		private static bool _isDisplayingTooltip;
	}
}
