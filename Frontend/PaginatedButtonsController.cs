using System;
using System.Diagnostics;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class PaginatedButtonsController : BaseMonoBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private event PaginatedButtonObject.OnButtonClickDelegate _onAnyButtonClickEvent;

		public void Configure(int tabsCount, PaginatedButtonObject.OnButtonClickDelegate onAnyButtonCleckEvent)
		{
			this._onAnyButtonClickEvent = onAnyButtonCleckEvent;
			for (int i = 0; i < this.Buttons.Length; i++)
			{
				PaginatedButtonObject paginatedButtonObject = this.Buttons[i];
				paginatedButtonObject.gameObject.SetActive(false);
			}
			for (int j = 0; j < tabsCount; j++)
			{
				PaginatedButtonObject paginatedButtonObject2 = this.Buttons[j];
				paginatedButtonObject2.gameObject.SetActive(true);
				paginatedButtonObject2.Configure(new PaginatedButtonObject.OnButtonClickDelegate(this.OnButtonClick));
			}
			this.ButtonsGrid.repositionNow = true;
			this.ButtonsGrid.Reposition();
			this.DisableAllButtons();
		}

		protected void Ondestroy()
		{
			this._onAnyButtonClickEvent = null;
		}

		private void OnButtonClick()
		{
			this._onAnyButtonClickEvent();
		}

		public void SetProgressBarAnimation(int buttonIndex, float from, float to, float time)
		{
			if (buttonIndex < 0 || buttonIndex >= this.Buttons.Length)
			{
				PaginatedButtonsController.Log.ErrorFormat("Index Out of range. Index={0}", new object[]
				{
					buttonIndex
				});
				return;
			}
			this.Buttons[buttonIndex].SetProgressBarAnimation(from, to, time);
		}

		public void EnableAllButtons()
		{
			for (int i = 0; i < this.Buttons.Length; i++)
			{
				this.EnableButton(i);
			}
		}

		private void EnableButton(int index)
		{
			PaginatedButtonObject paginatedButtonObject = this.Buttons[index];
			paginatedButtonObject.SetButtonState(true, this.EnabledColor);
		}

		public void SetEnableColor(int index)
		{
			PaginatedButtonObject paginatedButtonObject = this.Buttons[index];
			paginatedButtonObject.SetColor(this.EnabledColor);
		}

		public void DisableAllButtons()
		{
			for (int i = 0; i < this.Buttons.Length; i++)
			{
				this.DisableButton(i);
			}
		}

		public void DisableButton(int index)
		{
			PaginatedButtonObject paginatedButtonObject = this.Buttons[index];
			paginatedButtonObject.SetButtonState(false, this.DisabledColor);
		}

		public void SetDisabledColor(int index)
		{
			PaginatedButtonObject paginatedButtonObject = this.Buttons[index];
			paginatedButtonObject.SetColor(this.DisabledColor);
		}

		public void ToggleButton(bool enable, int index)
		{
			PaginatedButtonObject paginatedButtonObject = this.Buttons[index];
			paginatedButtonObject.ToggleButton(enable);
		}

		public void ResetAllProgressBar()
		{
			for (int i = 0; i < this.Buttons.Length; i++)
			{
				this.ResetProgressBar(i);
			}
		}

		public void ResetProgressBar(int index)
		{
			PaginatedButtonObject paginatedButtonObject = this.Buttons[index];
			paginatedButtonObject.ResetProgressBar();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PaginatedButtonsController));

		public PaginatedButtonObject[] Buttons;

		public Color EnabledColor;

		public Color DisabledColor;

		public UIGrid ButtonsGrid;
	}
}
