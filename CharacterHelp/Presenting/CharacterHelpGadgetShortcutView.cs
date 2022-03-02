using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.CharacterHelp.Presenting
{
	public class CharacterHelpGadgetShortcutView : MonoBehaviour, ICharacterHelpGadgetShortcutView
	{
		public ILabel KeyboardLabel
		{
			get
			{
				return this._keyboardLabel;
			}
		}

		public ISpriteImage GamepadButtonImage
		{
			get
			{
				return this._gamepadButtonImage;
			}
		}

		public void EnableCanvases()
		{
			this._keyboardLabelCanvas.Enable();
			this._keyboardLabelBackgroundCanvas.Enable();
		}

		public void DisableCanvases()
		{
			this._keyboardLabelCanvas.Disable();
			this._keyboardLabelBackgroundCanvas.Disable();
		}

		[SerializeField]
		private UnityLabel _keyboardLabel;

		[SerializeField]
		private UnityCanvas _keyboardLabelCanvas;

		[SerializeField]
		private UnityCanvas _keyboardLabelBackgroundCanvas;

		[SerializeField]
		private UnityImage _gamepadButtonImage;
	}
}
