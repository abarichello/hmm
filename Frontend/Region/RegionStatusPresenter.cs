using System;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Localization.TranslationTable;
using UniRx;

namespace HeavyMetalMachines.Frontend.Region
{
	public class RegionStatusPresenter : IRegionStatusPresenter, IDisposable
	{
		public RegionStatusPresenter(IViewProvider viewProvider, IGetThenObserveRegionPing getThenObserveRegionPing, ILocalizeKey translation)
		{
			this._viewProvider = viewProvider;
			this._getThenObserveRegionPing = getThenObserveRegionPing;
			this._translation = translation;
		}

		public void Initialize()
		{
			this._view = this._viewProvider.Provide<IRegionStatusView>(null);
			this._regionPingObservation = ObservableExtensions.Subscribe<RegionServerPing>(this._getThenObserveRegionPing.GetThenObserve(), new Action<RegionServerPing>(this.UpdateView));
		}

		private void UpdateView(RegionServerPing ping)
		{
			if (!this.ShouldDisplayPingInfo(ping))
			{
				ActivatableExtensions.Deactivate(this._view.Group);
				return;
			}
			ActivatableExtensions.Activate(this._view.Group);
			this._view.LatencyLabel.Text = string.Format("{0} - ", this._view.FormatLatencyValue(ping.Ping));
			this._view.LatencyLabel.Color = this._view.GetLatencyLabelColor(ping.Ping);
			UnityRegionStatusView.GradientColorsData gradientColors = this._view.GetGradientColors(ping.Ping);
			this._view.LatencyGradient.SetColors(gradientColors.TopColor.ToUnityColor(), gradientColors.BottonColor.ToUnityColor());
			this._view.RegionNameLabel.Text = this._translation.Get(ping.Region.RegionNameI18N, TranslationContext.Region);
			this._view.StatusLabel.Text = this.GetPingStatusText(ping);
			this._view.StatusLabel.Color = this._view.GetPingStatusColor(ping.Ping);
		}

		private bool ShouldDisplayPingInfo(RegionServerPing ping)
		{
			bool flag = ping.Ping != 999999;
			bool flag2 = this._view.GetPingCategory(ping.Ping) != RegionPingUtils.PingStatus.None;
			return flag && flag2;
		}

		private string GetPingStatusText(RegionServerPing ping)
		{
			switch (this._view.GetPingCategory(ping.Ping))
			{
			case RegionPingUtils.PingStatus.None:
				return this._translation.Get("INFO_PING_DESCRIPTION_NOPING", TranslationContext.Region);
			case RegionPingUtils.PingStatus.High:
				return this._translation.Get("INFO_PING_DESCRIPTION_HIGH", TranslationContext.Region);
			case RegionPingUtils.PingStatus.Average:
				return this._translation.Get("INFO_PING_DESCRIPTION_AVERAGE", TranslationContext.Region);
			case RegionPingUtils.PingStatus.Low:
				return this._translation.Get("INFO_PING_DESCRIPTION_LOW", TranslationContext.Region);
			default:
				throw new Exception("Cannot get ping status of unknown ping category.");
			}
		}

		void IDisposable.Dispose()
		{
			this._regionPingObservation.Dispose();
		}

		private readonly IViewProvider _viewProvider;

		private readonly IGetThenObserveRegionPing _getThenObserveRegionPing;

		private readonly ILocalizeKey _translation;

		private IRegionStatusView _view;

		private IDisposable _regionPingObservation;
	}
}
