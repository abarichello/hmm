using System;
using HeavyMetalMachines.Frontend;
using UnityEngine;
using UnityEngine.Events;

namespace HeavyMetalMachines.UnityUI
{
	public class UnityUiToggleInfo : MonoBehaviour
	{
		public void Setup(bool isOnPreCallback, string titleText, HmmUiText.TextStyles titleTextStyle, UnityAction<bool> onValueChanged)
		{
			this._toggle.onValueChanged.RemoveListener(onValueChanged);
			this._toggle.isOn = isOnPreCallback;
			this._toggle.onValueChanged.AddListener(onValueChanged);
			this._titleText.TextStyle = titleTextStyle;
			this._titleText.text = titleText;
		}

		public void SetToggleValue(bool isOn)
		{
			this._toggle.isOn = isOn;
		}

		[SerializeField]
		private HmmUiToggle _toggle;

		[SerializeField]
		private HmmUiText _titleText;
	}
}
