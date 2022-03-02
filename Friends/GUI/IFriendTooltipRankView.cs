using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.Friends.GUI
{
	public interface IFriendTooltipRankView
	{
		IActivatable LoadingActivatable { get; }

		IActivatable MainGroup { get; }

		IDynamicImage RankDynamicImage { get; }

		IActivatable RankImageActivatable { get; }

		ILabel RankLabel { get; }
	}
}
