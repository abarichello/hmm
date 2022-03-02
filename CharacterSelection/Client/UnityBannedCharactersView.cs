using System;
using System.Collections.Generic;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.Infra;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class UnityBannedCharactersView : MonoBehaviour, IBannedCharactersView
	{
		public IActivatable AlliedNoBanActivatable
		{
			get
			{
				return this._alliedNoBanActivatable;
			}
		}

		public IActivatable OpponentNoBanActivatable
		{
			get
			{
				return this._opponentNoBanActivatable;
			}
		}

		public IAnimation ShowStepResultAnimation
		{
			get
			{
				return this._showStepResultAnimation;
			}
		}

		public IEnumerable<IBanCandidateView> AlliedBanCandidates
		{
			get
			{
				return this._alliedBanCandidates;
			}
		}

		public IEnumerable<IBanCandidateView> OpponentBanCandidates
		{
			get
			{
				return this._opponentBanCandidates;
			}
		}

		public IEnumerable<IBannedCharacterSlotView> AlliedBannedCharacterSlots
		{
			get
			{
				return this._alliedBannedCharacterSlots;
			}
		}

		public IEnumerable<IBannedCharacterSlotView> OpponentBannedCharacterSlots
		{
			get
			{
				return this._opponentBannedCharacterSlots;
			}
		}

		public IAnimationCurve BanMoveToSlotAnimationCurve
		{
			get
			{
				return this._banMoveToSlotAnimationCurve;
			}
		}

		public float BanMoveToSlotAnimationDurationSeconds
		{
			get
			{
				return this._banMoveToSlotAnimationDurationSeconds;
			}
		}

		public IObservable<Unit> OnShouldShowAlliedBan
		{
			get
			{
				return this._onShouldShowFirstTeamSubject;
			}
		}

		public IObservable<Unit> OnShouldShowOpponentBan
		{
			get
			{
				return this._onShouldShowSecondTeamSubject;
			}
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<IBannedCharactersView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<IBannedCharactersView>(null);
		}

		public void ShowFirstTeamBan()
		{
			this._onShouldShowFirstTeamSubject.OnNext(Unit.Default);
		}

		public void ShowSecondTeamBan()
		{
			this._onShouldShowSecondTeamSubject.OnNext(Unit.Default);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private GameObjectActivatable _alliedNoBanActivatable;

		[SerializeField]
		private GameObjectActivatable _opponentNoBanActivatable;

		[SerializeField]
		private UnityAnimation _showStepResultAnimation;

		[SerializeField]
		private UnityBanCandidateView[] _alliedBanCandidates;

		[SerializeField]
		private UnityBanCandidateView[] _opponentBanCandidates;

		[SerializeField]
		private UnityBannedCharacterSlotView[] _alliedBannedCharacterSlots;

		[SerializeField]
		private UnityBannedCharacterSlotView[] _opponentBannedCharacterSlots;

		[SerializeField]
		private UnityAnimationCurve _banMoveToSlotAnimationCurve;

		[SerializeField]
		private float _banMoveToSlotAnimationDurationSeconds;

		private readonly Subject<Unit> _onShouldShowFirstTeamSubject = new Subject<Unit>();

		private readonly Subject<Unit> _onShouldShowSecondTeamSubject = new Subject<Unit>();
	}
}
