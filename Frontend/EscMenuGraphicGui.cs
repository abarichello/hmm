using System;
using System.Collections.Generic;
using HeavyMetalMachines.Localization;
using Pocketverse;
using UnityEngine;
using UnityEngine.Serialization;

namespace HeavyMetalMachines.Frontend
{
	public class EscMenuGraphicGui : EscMenuScreenGui
	{
		public override void Show()
		{
			base.Show();
			GameHubBehaviour.Hub.GuiScripts.ScreenResolution.ListenToResolutionChange += this.ListenToResolutionChange;
		}

		public override void Hide()
		{
			base.Hide();
			GameHubBehaviour.Hub.GuiScripts.ScreenResolution.ListenToResolutionChange -= this.ListenToResolutionChange;
		}

		private void ListenToResolutionChange()
		{
			this.ReloadCurrent();
		}

		public override void ReloadCurrent()
		{
			this._reloading = true;
			this._visualResolutionPopup.value = null;
			this._visualFullScreenPopup.value = null;
			this._visualMonitorPopup.value = null;
			this._visualVsyncPopup.value = null;
			this._visualFpsPopup.value = null;
			List<Resolution> availableResolutions = GameHubBehaviour.Hub.GuiScripts.ScreenResolution.GetAvailableResolutions();
			this._resolutions.Clear();
			this._visualResolutionPopup.items.Clear();
			for (int i = availableResolutions.Count - 1; i >= 0; i--)
			{
				Resolution item = availableResolutions[i];
				this._resolutions.Add(item);
				this._visualResolutionPopup.items.Add(item.ToString());
			}
			this._visualResolutionPopup.value = GameHubBehaviour.Hub.GuiScripts.ScreenResolution.GetCurrentResolution().ToString();
			this._visualFullScreenPopup.items.Clear();
			this._visualFullScreenPopup.items.Add(Language.Get("VISUAL_FULLSCREEN_WINDOWED", TranslationContext.Options));
			this._visualFullScreenPopup.items.Add(Language.Get("VISUAL_FULLSCREEN_FULLSCREEN", TranslationContext.Options));
			int index = (!GameHubBehaviour.Hub.GuiScripts.ScreenResolution.IsFullscreen()) ? 0 : 1;
			this._visualFullScreenPopup.value = this._visualFullScreenPopup.items[index];
			this._displays = GameHubBehaviour.Hub.GuiScripts.ScreenResolution.GetAvailableDisplays();
			this._visualMonitorPopup.Clear();
			for (int j = 0; j < this._displays.Count; j++)
			{
				this._visualMonitorPopup.items.Add((this._displays[j].Index + 1).ToString());
			}
			this._visualMonitorPopup.value = (GameHubBehaviour.Hub.GuiScripts.ScreenResolution.GetCurrentDisplay() + 1).ToString();
			this._visualVsyncPopup.items.Clear();
			this._visualVsyncPopup.items.Add(Language.Get("Desligado", TranslationContext.Options));
			this._visualVsyncPopup.items.Add(Language.Get("Ligado", TranslationContext.Options));
			this._visualVsyncPopup.itemData.Clear();
			this._visualVsyncPopup.itemData.Add(0);
			this._visualVsyncPopup.itemData.Add(1);
			this._visualVsyncPopup.value = ((GameHubBehaviour.Hub.GuiScripts.ScreenResolution.GetCurrentVsyncCount() <= 0) ? this._visualVsyncPopup.items[0] : this._visualVsyncPopup.items[1]);
			this._visualFpsPopup.items.Clear();
			this._visualFpsPopup.items.Add(Language.Get("Desligado", TranslationContext.Options));
			this._visualFpsPopup.items.Add("30");
			this._visualFpsPopup.items.Add("60");
			this._visualFpsPopup.items.Add("120");
			this._visualFpsPopup.itemData.Add(-1);
			this._visualFpsPopup.itemData.Add(30);
			this._visualFpsPopup.itemData.Add(60);
			this._visualFpsPopup.itemData.Add(120);
			int currentTargetFps = GameHubBehaviour.Hub.GuiScripts.ScreenResolution.GetCurrentTargetFps();
			this._visualFpsPopup.value = ((currentTargetFps >= 0) ? currentTargetFps.ToString() : this._visualFpsPopup.items[0]);
			this.ConfigureFpsPopup(GameHubBehaviour.Hub.GuiScripts.ScreenResolution.GetCurrentVsyncCount() == 0);
			this._reloading = false;
		}

		public override void ResetDefault()
		{
		}

		public void OnVisualResolutionPopupChanged()
		{
			if (this._reloading)
			{
				return;
			}
			int index = this._visualResolutionPopup.items.IndexOf(this._visualResolutionPopup.value);
			if (this._resolutions[index].CompareTo(GameHubBehaviour.Hub.GuiScripts.ScreenResolution.GetCurrentResolution()) == 0)
			{
				return;
			}
			ScreenResolutionController.SetWindowResolution(this._resolutions[index]);
		}

		public void OnVisualFullScreenPopupChanged()
		{
			if (this._reloading)
			{
				return;
			}
			int num = this._visualFullScreenPopup.items.IndexOf(this._visualFullScreenPopup.value);
			ScreenResolutionController.SetFullscreen(num != 0);
		}

		public void OnVisualMonitorPopupChanged()
		{
			if (this._reloading)
			{
				return;
			}
			int index = this._visualMonitorPopup.items.IndexOf(this._visualMonitorPopup.value);
			GameHubBehaviour.Hub.GuiScripts.ScreenResolution.SetDisplay(this._displays[index].Index);
		}

		public void OnVisualVsyncPopupChanged()
		{
			if (this._reloading)
			{
				return;
			}
			this.ConfigureFpsPopup((int)this._visualVsyncPopup.data == 0);
			ScreenResolutionController.SetVsync((int)this._visualVsyncPopup.data);
		}

		public void OnVisualFpsPopupChanged()
		{
			if (this._reloading)
			{
				return;
			}
			ScreenResolutionController.SetTargetFps((int)this._visualFpsPopup.data);
		}

		private void ConfigureFpsPopup(bool enable)
		{
			if (enable)
			{
				this._visualFpsPopup.gameObject.SetActive(true);
				this._visualFpsPopupDisabled.gameObject.SetActive(false);
			}
			else
			{
				this._visualFpsPopup.gameObject.SetActive(false);
				this._visualFpsPopupDisabled.gameObject.SetActive(true);
			}
		}

		[Header("GRAPHICS")]
		[SerializeField]
		[FormerlySerializedAs("VisualResolutionPopup")]
		private UIPopupList _visualResolutionPopup;

		[SerializeField]
		[FormerlySerializedAs("VisualFullScreenPopup")]
		private UIPopupList _visualFullScreenPopup;

		[SerializeField]
		[FormerlySerializedAs("VisualMonitorPopup")]
		private UIPopupList _visualMonitorPopup;

		[SerializeField]
		private UIPopupList _visualVsyncPopup;

		[SerializeField]
		private UIPopupList _visualFpsPopup;

		[SerializeField]
		private GameObject _visualFpsPopupDisabled;

		private bool _reloading = true;

		private List<Resolution> _resolutions = new List<Resolution>();

		private List<Display> _displays;
	}
}
