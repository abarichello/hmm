using System;
using FMod;
using UnityEngine;

namespace HeavyMetalMachines.Audio
{
	internal class UIAudio : MonoBehaviour
	{
		private bool canPlay
		{
			get
			{
				return base.enabled && Application.isFocused;
			}
		}

		private void OnEnable()
		{
			this.activationTime = Time.time;
		}

		private void OnHover(bool isOver)
		{
			if (this.mIsOver == isOver)
			{
				return;
			}
			this.mIsOver = isOver;
			if (Time.time - this.activationTime < 0.1f)
			{
				return;
			}
			if (this.canPlay)
			{
				if (isOver && this.OnMouseOverAudio)
				{
					FMODAudioManager.PlayOneShotAt(this.OnMouseOverAudio, Vector3.zero, 0);
				}
				if (!isOver && this.OnMouseOverOutAudio)
				{
					FMODAudioManager.PlayOneShotAt(this.OnMouseOverOutAudio, Vector3.zero, 0);
				}
			}
		}

		private void OnPress(bool isPressed)
		{
			if (this.mIsPressed == isPressed)
			{
				return;
			}
			this.mIsPressed = isPressed;
			if (this.canPlay && isPressed && this.OnPressAudio)
			{
				FMODAudioManager.PlayOneShotAt(this.OnPressAudio, Vector3.zero, 0);
			}
		}

		private void OnClick()
		{
			if (this.canPlay && this.OnClickAudio)
			{
				if (UIButtonColor.GetUiMouseButton() != this.MouseButton)
				{
					return;
				}
				FMODAudioManager.PlayOneShotAt(this.OnClickAudio, Vector3.zero, 0);
			}
		}

		private void OnSelect(bool isSelected)
		{
			if (this.canPlay && (!isSelected || UICamera.currentScheme == UICamera.ControlScheme.Controller))
			{
				this.OnHover(isSelected);
			}
		}

		public AudioEventAsset OnClickAudio;

		public UIButtonColor.UIMouseButton MouseButton = UIButtonColor.UIMouseButton.Left;

		public AudioEventAsset OnMouseOverAudio;

		public AudioEventAsset OnMouseOverOutAudio;

		public AudioEventAsset OnPressAudio;

		private bool mIsOver;

		private bool mIsPressed;

		private float activationTime;
	}
}
