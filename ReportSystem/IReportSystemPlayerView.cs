using System;
using HeavyMetalMachines.Presenting;

namespace HeavymetalMachines.ReportSystem
{
	public interface IReportSystemPlayerView
	{
		ILabel PlayerNameLabel { get; }

		ILabel PlayerTagLabel { get; }

		IDynamicImage PlayerAvatarImage { get; }

		IDynamicImage PlayerPortraitImage { get; }

		IActivatable PsnIdIconActivatable { get; }

		ILabel PsnIdLabel { get; }
	}
}
