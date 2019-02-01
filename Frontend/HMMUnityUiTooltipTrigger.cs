using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	[RequireComponent(typeof(Selectable))]
	public class HMMUnityUiTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
	{
		public static bool IsDisplayingTooltip
		{
			get
			{
				return HMMUnityUiTooltipTrigger._isDisplayingTooltip;
			}
		}

		protected void OnValidate()
		{
			this._selectable = base.GetComponent<Selectable>();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (!base.enabled || !this._selectable.IsInteractable())
			{
				return;
			}
			if (this._mustShowTooltip)
			{
				return;
			}
			this._nextDisplayTime = Time.unscaledTime + this.DelayInSec;
			this._mustShowTooltip = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (!base.enabled)
			{
				return;
			}
			this.HideTooltip();
		}

		private void ShowTooltip()
		{
			if (HMMUnityUiTooltipTrigger._isDisplayingTooltip)
			{
				return;
			}
			HMMUnityUiTooltipTrigger._isDisplayingTooltip = true;
			Vector3 targetPosition = (!(this.TargetPosition != null)) ? Vector3.zero : this.TargetPosition.position;
			UITooltip.Show((!this.TranslateText) ? this.TooltipText : Language.Get(this.TooltipText, this.Sheet.ToString()), this.FollowMouse, this.ScaleTextInOverflow, targetPosition);
		}

		private void HideTooltip()
		{
			if (!HMMUnityUiTooltipTrigger._isDisplayingTooltip)
			{
				return;
			}
			HMMUnityUiTooltipTrigger._isDisplayingTooltip = false;
			this._mustShowTooltip = false;
			UITooltip.Hide();
		}

		protected void Update()
		{
			if (HMMUnityUiTooltipTrigger._isDisplayingTooltip)
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

		protected void OnDisable()
		{
			this.HideTooltip();
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

		[SerializeField]
		private Selectable _selectable;
	}
}
