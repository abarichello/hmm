using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.CompetitiveMode.View.Matchmaking
{
	public class UnityCompetitiveQueueDayPeriodView : MonoBehaviour, ICompetitiveQueueDayPeriodView
	{
		public ILabel OpenTimeLabel
		{
			get
			{
				return this._openTimeLabel;
			}
		}

		public ILabel CloseTimeLabel
		{
			get
			{
				return this._closeTimeLabel;
			}
		}

		[SerializeField]
		private UnityLabel _openTimeLabel;

		[SerializeField]
		private UnityLabel _closeTimeLabel;
	}
}
