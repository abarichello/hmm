using System;
using HeavyMetalMachines.Frontend;
using UnityEngine;

namespace HeavyMetalMachines.UnityUI
{
	public class UnityUiGenericButton : MonoBehaviour
	{
		public void DisableAutomaticTranslation()
		{
			this._buttonText.UseTranslation = false;
		}

		public void SetText(string text)
		{
			this._buttonText.text = text;
		}

		[SerializeField]
		private HmmUiText _buttonText;
	}
}
