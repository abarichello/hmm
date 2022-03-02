using System;
using HeavyMetalMachines.Frontend.UnityUI;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend.Region
{
	public class UnityRegionStatusView : MonoBehaviour, IRegionStatusView
	{
		public IActivatable Group
		{
			get
			{
				return this._group;
			}
		}

		public ILabel RegionNameLabel
		{
			get
			{
				return this._regionNameLabel;
			}
		}

		public ILabel LatencyLabel
		{
			get
			{
				return this._latencyLabel;
			}
		}

		public ILabel StatusLabel
		{
			get
			{
				return this._statusLabel;
			}
		}

		public IUiGradient LatencyGradient
		{
			get
			{
				return this._latencyGradient;
			}
		}

		public RegionPingUtils.PingStatus GetPingCategory(int ping)
		{
			return RegionPingUtils.GetPingStatus(ping, this._guiValuesInfo);
		}

		public string FormatLatencyValue(int ping)
		{
			return RegionPingUtils.FormatPingValue(ping, this._guiValuesInfo);
		}

		public Color GetLatencyLabelColor(int ping)
		{
			RegionPingUtils.PingStatus pingCategory = this.GetPingCategory(ping);
			if (pingCategory == RegionPingUtils.PingStatus.High)
			{
				return GUIColorsInfo.Instance.PingHighGradientBottonColor.ToHmmColor();
			}
			if (pingCategory == RegionPingUtils.PingStatus.Average)
			{
				return GUIColorsInfo.Instance.PingMediumGradientBottonColor.ToHmmColor();
			}
			if (pingCategory != RegionPingUtils.PingStatus.Low)
			{
				return GUIColorsInfo.Instance.NoPingColor.ToHmmColor();
			}
			return GUIColorsInfo.Instance.PingLowGradientBottonColor.ToHmmColor();
		}

		public UnityRegionStatusView.GradientColorsData GetGradientColors(int ping)
		{
			UnityRegionStatusView.GradientColorsData result;
			switch (this.GetPingCategory(ping))
			{
			case RegionPingUtils.PingStatus.High:
				result.TopColor = GUIColorsInfo.Instance.PingHighGradientTopColor.ToHmmColor();
				result.BottonColor = GUIColorsInfo.Instance.PingHighGradientBottonColor.ToHmmColor();
				break;
			case RegionPingUtils.PingStatus.Average:
				result.TopColor = GUIColorsInfo.Instance.PingMediumGradientTopColor.ToHmmColor();
				result.BottonColor = GUIColorsInfo.Instance.PingMediumGradientBottonColor.ToHmmColor();
				break;
			case RegionPingUtils.PingStatus.Low:
				result.TopColor = GUIColorsInfo.Instance.PingLowGradientTopColor.ToHmmColor();
				result.BottonColor = GUIColorsInfo.Instance.PingLowGradientBottonColor.ToHmmColor();
				break;
			default:
				result.TopColor = GUIColorsInfo.Instance.NoPingColor.ToHmmColor();
				result.BottonColor = GUIColorsInfo.Instance.NoPingColor.ToHmmColor();
				break;
			}
			return result;
		}

		public Color GetPingStatusColor(int ping)
		{
			return RegionPingUtils.GetPingColor(ping, this._guiValuesInfo, false).ToHmmColor();
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<IRegionStatusView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<IRegionStatusView>(null);
		}

		[SerializeField]
		private GameObjectActivatable _group;

		[SerializeField]
		private UnityLabel _regionNameLabel;

		[SerializeField]
		private UnityLabel _latencyLabel;

		[SerializeField]
		private UnityLabel _statusLabel;

		[SerializeField]
		private HmmUiGradient _latencyGradient;

		[Inject]
		private IViewProvider _viewProvider;

		[Inject]
		private GUIValuesInfo _guiValuesInfo;

		public struct GradientColorsData
		{
			public Color TopColor;

			public Color BottonColor;
		}
	}
}
