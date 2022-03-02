﻿using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.CompetitiveMode.View.Matchmaking
{
	public interface ICompetitiveQueueDayPeriodView
	{
		ILabel OpenTimeLabel { get; }

		ILabel CloseTimeLabel { get; }
	}
}
