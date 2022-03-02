using System;
using System.Collections.Generic;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.CompetitiveMode.View.Matchmaking
{
	public class UnityCompetitiveQueueDayView : MonoBehaviour, ICompetitiveQueueDayView
	{
		public ILabel DateLabel
		{
			get
			{
				return this._dateLabel;
			}
		}

		public ILabel DayOfWeekLabel
		{
			get
			{
				return this._dayOfWeekLabel;
			}
		}

		public IImage BackgroundImage
		{
			get
			{
				return this._backgroundImage;
			}
		}

		public IActivatable EmptyPeriodsIndicator
		{
			get
			{
				return this._emptyPeriodsGameObject;
			}
		}

		public Color ActiveBackgroundColor
		{
			get
			{
				return this._activeBackgroundColor.ToHmmColor();
			}
		}

		public Color ActiveDateColor
		{
			get
			{
				return this._activeDateColor.ToHmmColor();
			}
		}

		public Color ActiveDayOfWeekColor
		{
			get
			{
				return this._activeDayOfWeekColor.ToHmmColor();
			}
		}

		public Color EmptyBackgroundColor
		{
			get
			{
				return this._emptyBackgroundColor.ToHmmColor();
			}
		}

		public Color EmptyDateColor
		{
			get
			{
				return this._emptyDateColor.ToHmmColor();
			}
		}

		public Color EmptyDayOfWeekColor
		{
			get
			{
				return this._EmptyDayOfWeekColor.ToHmmColor();
			}
		}

		public ICompetitiveQueueDayPeriodView CreateAndAddPeriod()
		{
			UnityCompetitiveQueueDayPeriodView unityCompetitiveQueueDayPeriodView = Object.Instantiate<UnityCompetitiveQueueDayPeriodView>(this._queuePeriodsViewPrefab, this._periodsParent, false);
			this._periodViews.Add(unityCompetitiveQueueDayPeriodView);
			return unityCompetitiveQueueDayPeriodView;
		}

		public void ClearPeriods()
		{
			foreach (UnityCompetitiveQueueDayPeriodView unityCompetitiveQueueDayPeriodView in this._periodViews)
			{
				Object.Destroy(unityCompetitiveQueueDayPeriodView.gameObject);
			}
			this._periodViews.Clear();
		}

		[SerializeField]
		private UnityLabel _dateLabel;

		[SerializeField]
		private UnityLabel _dayOfWeekLabel;

		[SerializeField]
		private UnityImage _backgroundImage;

		[SerializeField]
		private UnityCompetitiveQueueDayPeriodView _queuePeriodsViewPrefab;

		[SerializeField]
		private GameObjectActivatable _emptyPeriodsGameObject;

		[SerializeField]
		private Transform _periodsParent;

		[SerializeField]
		private Color _activeBackgroundColor;

		[SerializeField]
		private Color _activeDateColor;

		[SerializeField]
		private Color _activeDayOfWeekColor;

		[SerializeField]
		private Color _emptyBackgroundColor;

		[SerializeField]
		private Color _emptyDateColor;

		[SerializeField]
		private Color _EmptyDayOfWeekColor;

		private readonly List<UnityCompetitiveQueueDayPeriodView> _periodViews = new List<UnityCompetitiveQueueDayPeriodView>();
	}
}
