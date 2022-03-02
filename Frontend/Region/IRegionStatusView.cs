using System;
using HeavyMetalMachines.Frontend.UnityUI;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.Frontend.Region
{
	public interface IRegionStatusView
	{
		IActivatable Group { get; }

		ILabel RegionNameLabel { get; }

		ILabel LatencyLabel { get; }

		ILabel StatusLabel { get; }

		IUiGradient LatencyGradient { get; }

		RegionPingUtils.PingStatus GetPingCategory(int ping);

		string FormatLatencyValue(int ping);

		Color GetLatencyLabelColor(int ping);

		Color GetPingStatusColor(int ping);

		UnityRegionStatusView.GradientColorsData GetGradientColors(int ping);
	}
}
