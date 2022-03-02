using System;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class LoadingVersusKeyBindingsView : MonoBehaviour
	{
		public void UpdateBindings()
		{
			foreach (LoadingVersusKeyBindingsView.LoadingVersusBinding binding in this._bindings)
			{
				this.UpdateBinding(binding);
			}
		}

		private void UpdateBinding(LoadingVersusKeyBindingsView.LoadingVersusBinding binding)
		{
			this.UpdateKeyboardMouseBinding(binding);
			if (!this.TryUpdateJoystickMovementForward(binding))
			{
				this.UpdateJoystickBinding(binding);
			}
		}

		private void UpdateKeyboardMouseBinding(LoadingVersusKeyBindingsView.LoadingVersusBinding binding)
		{
			ISprite sprite;
			string text;
			if (this._inputTranslation.TryToGetInputActionKeyboardMouseAssetOrFallbackToTranslation(binding.InputAction, ref sprite, ref text))
			{
				binding.KeyIconSprite.gameObject.SetActive(true);
				binding.KeyNameLabel.gameObject.SetActive(false);
				binding.KeyIconSprite.sprite2D = (sprite as UnitySprite).GetSprite();
			}
			else
			{
				binding.KeyIconSprite.gameObject.SetActive(false);
				binding.KeyNameLabel.gameObject.SetActive(true);
				binding.KeyNameLabel.text = text;
			}
		}

		private bool TryUpdateJoystickMovementForward(LoadingVersusKeyBindingsView.LoadingVersusBinding binding)
		{
			if (binding.InputAction == 4)
			{
				ISprite sprite;
				string text;
				this._inputTranslation.TryToGetInputJoystickAssetOrFallbackToTranslation(22, ref sprite, ref text);
				binding.JoystickIconSprite.sprite2D = (sprite as UnitySprite).GetSprite();
				return true;
			}
			return false;
		}

		private void UpdateJoystickBinding(LoadingVersusKeyBindingsView.LoadingVersusBinding binding)
		{
			ISprite sprite;
			string text;
			this._inputTranslation.TryToGetInputActionJoystickAssetOrFallbackToTranslation(binding.InputAction, ref sprite, ref text);
			binding.JoystickIconSprite.sprite2D = (sprite as UnitySprite).GetSprite();
		}

		[SerializeField]
		private LoadingVersusKeyBindingsView.LoadingVersusBinding[] _bindings;

		[InjectOnClient]
		private IInputTranslation _inputTranslation;

		[Serializable]
		private struct LoadingVersusBinding
		{
			public ControllerInputActions InputAction;

			public UI2DSprite JoystickIconSprite;

			public UI2DSprite KeyIconSprite;

			public UILabel KeyNameLabel;
		}
	}
}
