using System;
using HeavyMetalMachines.Frontend;
using UnityEngine;

namespace HeavyMetalMachines.UnityUI
{
	public class UnityUiTitleInfo : MonoBehaviour
	{
		public void Setup(string titleText, HmmUiText.TextStyles titleTextStyle, string subtitleText, HmmUiText.TextStyles subtitleTextStyle)
		{
			this._titleText.TextStyle = titleTextStyle;
			this._titleText.text = titleText;
			this._titleShadowText.TextStyle = titleTextStyle;
			this._titleShadowText.text = titleText;
			this.SetSubtitle(subtitleText, subtitleTextStyle);
		}

		public void SetSubtitle(string subtitleText, HmmUiText.TextStyles subtitleTextStyle)
		{
			this._subtitleText.TextStyle = subtitleTextStyle;
			this._subtitleText.text = subtitleText;
		}

		[SerializeField]
		private HmmUiText _titleText;

		[SerializeField]
		private HmmUiText _titleShadowText;

		[SerializeField]
		private HmmUiText _subtitleText;
	}
}
