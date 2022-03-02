using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.MainMenuPresenting.Announcement
{
	public class UnityBattlepassSeasonAnnouncementView : MonoBehaviour
	{
		public IButton OpenWindowButton
		{
			get
			{
				return this._openWindowButton;
			}
		}

		public ILabel StartDayLabel
		{
			get
			{
				return this._startDayLabel;
			}
		}

		public ILabel StartMonthLabel
		{
			get
			{
				return this._startMonthLabel;
			}
		}

		public ILabel EndDayLabel
		{
			get
			{
				return this._endDayLabel;
			}
		}

		public ILabel EndMonthLabel
		{
			get
			{
				return this._endMonthLabel;
			}
		}

		private void Awake()
		{
			this._viewProvider.Bind<UnityBattlepassSeasonAnnouncementView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<UnityBattlepassSeasonAnnouncementView>(null);
		}

		public void Initialize(bool showAnnouncement)
		{
			this._mainGroup.SetActive(showAnnouncement);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private GameObjectActivatable _mainGroup;

		[SerializeField]
		private UnityButton _openWindowButton;

		[SerializeField]
		private UnityLabel _startDayLabel;

		[SerializeField]
		private UnityLabel _startMonthLabel;

		[SerializeField]
		private UnityLabel _endDayLabel;

		[SerializeField]
		private UnityLabel _endMonthLabel;
	}
}
