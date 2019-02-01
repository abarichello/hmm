using System;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(Rigidbody))]
	public class JoystickSchemeButtonActivator : GameHubBehaviour
	{
		public UIWidget TargetButtonWidget
		{
			get
			{
				if (this._targetButtonWidget != null)
				{
					return this._targetButtonWidget;
				}
				this._targetButtonWidget = this.targetButton.GetComponent<UIWidget>();
				if (this._targetButtonWidget == null)
				{
					this._targetButtonWidget = this.targetButton.GetComponentInChildren<UIWidget>();
				}
				if (this._targetButtonWidget == null)
				{
					this._targetButtonWidget = base.GetComponent<UIWidget>();
				}
				return this._targetButtonWidget;
			}
		}

		public bool Activated
		{
			get
			{
				return this._activated;
			}
			set
			{
				this._activated = value;
			}
		}

		private void OnEnable()
		{
			this._targetButtonWidget = this.TargetButtonWidget;
			this.Register();
			GUIUtils.AnimateTweenAlpha(this._tweenalpha, this._activated, this.InverseAlphaAnimation);
		}

		public void Register()
		{
			GameHubBehaviour.Hub.GuiScripts.JoystickScheme.PreRegister(this);
		}

		public void Unregister()
		{
			GameHubBehaviour.Hub.GuiScripts.JoystickScheme.PreUnRegister(this);
		}

		private void OnDisable()
		{
			this.Unregister();
		}

		public void Activate(bool action)
		{
			this._activated = action;
			GUIUtils.AnimateTweenAlpha(this._tweenalpha, this._activated, this.InverseAlphaAnimation);
		}

		public GameObject targetButton;

		private UIWidget _targetButtonWidget;

		private bool _activated;

		public TweenAlpha _tweenalpha;

		public bool InverseAlphaAnimation;

		public JoystickSchemeController.JoystickButtons JoystickButtonKind;
	}
}
