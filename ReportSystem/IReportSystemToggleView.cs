using System;
using HeavyMetalMachines.Presenting;

namespace HeavymetalMachines.ReportSystem
{
	public interface IReportSystemToggleView
	{
		IToggle ReportToggle { get; }

		ILabel ReportToggleLabel { get; }
	}
}
