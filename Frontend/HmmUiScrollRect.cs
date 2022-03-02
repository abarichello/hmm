using System;
using HeavyMetalMachines.Input.ControllerInput;
using Hoplon.ToggleableFeatures;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	[AddComponentMenu("UI/HMM/HmmUiScrollRect")]
	public class HmmUiScrollRect : ScrollRect
	{
		protected override void Awake()
		{
			base.Awake();
			this._movementTypeBackup = base.movementType;
			this._inputScrollPointerEventData = new PointerEventData(EventSystem.current);
		}

		protected override void Start()
		{
			base.Start();
			if (Application.isPlaying)
			{
				this._ignoreInputScroll = true;
			}
		}

		public override void OnBeginDrag(PointerEventData eventData)
		{
			if (!this._ignoreDrag)
			{
				base.OnBeginDrag(eventData);
			}
		}

		public override void OnDrag(PointerEventData eventData)
		{
			if (!this._ignoreDrag)
			{
				base.OnDrag(eventData);
			}
		}

		public override void OnEndDrag(PointerEventData eventData)
		{
			if (!this._ignoreDrag)
			{
				base.OnEndDrag(eventData);
			}
		}

		public override void OnScroll(PointerEventData eventData)
		{
			if (!this._ignoreScroll)
			{
				base.OnScroll(eventData);
			}
		}

		protected override void LateUpdate()
		{
			if (this._enableFlickeringFix)
			{
				this.FlickeringFixLateUpdate();
			}
			base.LateUpdate();
			if (Application.isPlaying && !this._ignoreScroll && !this._ignoreInputScroll)
			{
				float axis = this._controllerInputActionPoller.GetAxis(38);
				if (axis < -this._inputVerticalAxisTrheshold || axis > this._inputVerticalAxisTrheshold)
				{
					Vector2 zero = Vector2.zero;
					zero.y = axis;
					this._inputScrollPointerEventData.scrollDelta = zero;
					base.OnScroll(this._inputScrollPointerEventData);
				}
			}
		}

		private void FlickeringFixLateUpdate()
		{
			if (this._movementTypeBackup != 2)
			{
				return;
			}
			if (base.horizontal && base.vertical)
			{
				return;
			}
			if (base.movementType == 2)
			{
				this.TryFlickeringFixScrollbar(base.horizontal, base.horizontalScrollbar, base.horizontalScrollbarVisibility);
				this.TryFlickeringFixScrollbar(base.vertical, base.verticalScrollbar, base.verticalScrollbarVisibility);
			}
			else
			{
				this.TryRestoreFlickeringFix(base.horizontal, base.horizontalScrollbar, base.horizontalScrollbarVisibility);
				this.TryRestoreFlickeringFix(base.vertical, base.verticalScrollbar, base.verticalScrollbarVisibility);
			}
		}

		private void TryFlickeringFixScrollbar(bool isScrollbarActive, Scrollbar scrollbar, ScrollRect.ScrollbarVisibility scrollbarVisibility)
		{
			if (!this.CanCheckScrollbar(isScrollbarActive, scrollbar, scrollbarVisibility))
			{
				return;
			}
			if (!scrollbar.isActiveAndEnabled)
			{
				base.movementType = 0;
				this._ignoreScrollBackup = this._ignoreScroll;
				this._ignoreDragBackup = this._ignoreDrag;
				this._ignoreScroll = true;
				this._ignoreDrag = true;
			}
		}

		private void TryRestoreFlickeringFix(bool isScrollbarActive, Scrollbar scrollbar, ScrollRect.ScrollbarVisibility scrollbarVisibility)
		{
			if (!this.CanCheckScrollbar(isScrollbarActive, scrollbar, scrollbarVisibility))
			{
				return;
			}
			if (scrollbar.isActiveAndEnabled)
			{
				base.movementType = this._movementTypeBackup;
				this._ignoreScroll = this._ignoreScrollBackup;
				this._ignoreDrag = this._ignoreDragBackup;
			}
		}

		private bool CanCheckScrollbar(bool isScrollbarActive, Scrollbar scrollbar, ScrollRect.ScrollbarVisibility scrollbarVisibility)
		{
			return isScrollbarActive && scrollbarVisibility != null && null != scrollbar;
		}

		[Inject]
		private IControllerInputActionPoller _controllerInputActionPoller;

		[SerializeField]
		private bool _ignoreDrag = true;

		[SerializeField]
		private bool _ignoreScroll;

		[SerializeField]
		private bool _ignoreInputScroll;

		[SerializeField]
		[Range(0.2f, 0.8f)]
		private float _inputVerticalAxisTrheshold = 0.2f;

		[SerializeField]
		private bool _enableFlickeringFix;

		[Inject]
		private IIsFeatureToggled _isFeatureToggled;

		private ScrollRect.MovementType _movementTypeBackup;

		private bool _ignoreScrollBackup;

		private bool _ignoreDragBackup;

		private PointerEventData _inputScrollPointerEventData;
	}
}
