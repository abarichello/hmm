using System;
using System.Collections.Generic;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Tournaments.Presenting.Detailing.Teams
{
	public class TournamentTeamView : MonoBehaviour, ITournamentTeamView
	{
		public ICanvasGroup MainCanvasGroup
		{
			get
			{
				return this._mainCanvasGroupUnity;
			}
		}

		public IButton CreateButton
		{
			get
			{
				return this._createButtonUnity;
			}
		}

		public IButton FindButton
		{
			get
			{
				return this._findButtonUnity;
			}
		}

		public ILabel TagLabel
		{
			get
			{
				return this._tagLabelUnity;
			}
		}

		public ILabel NameLabel
		{
			get
			{
				return this._nameLabelUnity;
			}
		}

		public ILabel TeamCurrentUgcOwnerOnlineIdLabel
		{
			get
			{
				return this._teamCurrentUgcOwnerOnlineIdLabel;
			}
		}

		public ILabel PositionLabel
		{
			get
			{
				return this._positionLabelUnity;
			}
		}

		public ILabel ScoreLabel
		{
			get
			{
				return this._scoreLabelUnity;
			}
		}

		public IDynamicImage TeamIconImage
		{
			get
			{
				return this._teamIconRawImage;
			}
		}

		public IButton MyTeamButton
		{
			get
			{
				return this._myTeamButtonUnity;
			}
		}

		public ICanvasGroup HasTeamGroup
		{
			get
			{
				return this._hasTeamGameObject;
			}
		}

		public ICanvasGroup HasNoTeamGroup
		{
			get
			{
				return this._noTeamGameObject;
			}
		}

		public ICanvasGroup NotAbleToParticipateBackgroundGroup
		{
			get
			{
				return this._notAbleToParticipateBackgroundGroup;
			}
		}

		public ICanvasGroup AbleToParticipateBackgroundGroup
		{
			get
			{
				return this._ableToParticipateBackgroundGroup;
			}
		}

		public ICanvasGroup ParticipatingBackgroundGroup
		{
			get
			{
				return this._participatingBackgroundGroup;
			}
		}

		public IEnumerable<ITournamentTeamMemberView> MembersViews
		{
			get
			{
				return this._teamMembers;
			}
		}

		public IAnimation JoinedTeamAnimation
		{
			get
			{
				return this._joinedTeamAnimation;
			}
		}

		public IAnimation LeftTeamAnimation
		{
			get
			{
				return this._leftTeamAnimation;
			}
		}

		public IAnimation NotAbleToParticipateGroupHideAnimation
		{
			get
			{
				return this._notAbleToParticipateGroupHideAnimation;
			}
		}

		public IAnimation NotAbleToParticipateGroupShowAnimation
		{
			get
			{
				return this._notAbleToParticipateGroupShowAnimation;
			}
		}

		public IAnimation AbleToParticipateGroupHideAnimation
		{
			get
			{
				return this._ableToParticipateGroupHideAnimation;
			}
		}

		public IAnimation AbleToParticipateGroupShowAnimation
		{
			get
			{
				return this._ableToParticipateGroupShowAnimation;
			}
		}

		public IAnimation ParticipatingGroupHideAnimation
		{
			get
			{
				return this._participatingGroupHideAnimation;
			}
		}

		public IAnimation ParticipatingGroupShowAnimation
		{
			get
			{
				return this._participatingGroupShowAnimation;
			}
		}

		public ILabel NotAbleToParticipateReasonLabel
		{
			get
			{
				return this._notAbleToParticipateReasonLabel;
			}
		}

		public IAnimation ParticipatingInfoShowAnimation
		{
			get
			{
				return this._participatingInfoShowAnimation;
			}
		}

		public IAnimation ParticipatingInfoHideAnimation
		{
			get
			{
				return this._participatingInfoHideAnimation;
			}
		}

		public ICanvasGroup TeamTournamentPlacementGroup
		{
			get
			{
				return this._teamTournamentPlacementGroup;
			}
		}

		public ICanvasGroup LeaveTournamentButtonGroup
		{
			get
			{
				return this._leaveTournamentButtonGroup;
			}
		}

		public IActivatable TeamIsSubscribedToAnotherTournamentHint
		{
			get
			{
				return this._teamIsSubscribedToAnotherTournamentHint;
			}
		}

		public ILabel ParticipatingInformationLabel
		{
			get
			{
				return this._participatingInformationLabel;
			}
		}

		public IButton UnsubscribeFromTournamentButton
		{
			get
			{
				return this._unsubscribeFromTournamentButton;
			}
		}

		public ILabel CriteriaTitleLabel
		{
			get
			{
				return this._criteriaTitleLabel;
			}
		}

		private void Start()
		{
			this._viewProvider.Bind<ITournamentTeamView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<ITournamentTeamView>(null);
		}

		[SerializeField]
		private UnityCanvasGroup _mainCanvasGroupUnity;

		[SerializeField]
		private UnityCanvasGroup _hasTeamGameObject;

		[SerializeField]
		private UnityCanvasGroup _noTeamGameObject;

		[SerializeField]
		private UnityButton _createButtonUnity;

		[SerializeField]
		private UnityButton _findButtonUnity;

		[SerializeField]
		private UnityLabel _tagLabelUnity;

		[SerializeField]
		private UnityLabel _nameLabelUnity;

		[SerializeField]
		private UnityLabel _teamCurrentUgcOwnerOnlineIdLabel;

		[SerializeField]
		private UnityLabel _positionLabelUnity;

		[SerializeField]
		private UnityLabel _scoreLabelUnity;

		[SerializeField]
		private UnityButton _myTeamButtonUnity;

		[SerializeField]
		private UnityCanvasGroup _notAbleToParticipateBackgroundGroup;

		[SerializeField]
		private UnityCanvasGroup _ableToParticipateBackgroundGroup;

		[SerializeField]
		private UnityCanvasGroup _participatingBackgroundGroup;

		[SerializeField]
		private UnityDynamicRawImage _teamIconRawImage;

		[SerializeField]
		private TournamentTeamMemberView[] _teamMembers;

		[SerializeField]
		private UnityAnimation _joinedTeamAnimation;

		[SerializeField]
		private UnityAnimation _leftTeamAnimation;

		[SerializeField]
		private UnityAnimation _notAbleToParticipateGroupHideAnimation;

		[SerializeField]
		private UnityAnimation _notAbleToParticipateGroupShowAnimation;

		[SerializeField]
		private UnityAnimation _ableToParticipateGroupHideAnimation;

		[SerializeField]
		private UnityAnimation _ableToParticipateGroupShowAnimation;

		[SerializeField]
		private UnityAnimation _participatingGroupHideAnimation;

		[SerializeField]
		private UnityAnimation _participatingGroupShowAnimation;

		[SerializeField]
		private UnityLabel _notAbleToParticipateReasonLabel;

		[SerializeField]
		private UnityAnimation _participatingInfoShowAnimation;

		[SerializeField]
		private UnityAnimation _participatingInfoHideAnimation;

		[SerializeField]
		private UnityCanvasGroup _teamTournamentPlacementGroup;

		[SerializeField]
		private UnityCanvasGroup _leaveTournamentButtonGroup;

		[SerializeField]
		private GameObjectActivatable _teamIsSubscribedToAnotherTournamentHint;

		[SerializeField]
		private UnityLabel _participatingInformationLabel;

		[SerializeField]
		private UnityButton _unsubscribeFromTournamentButton;

		[SerializeField]
		private UnityLabel _criteriaTitleLabel;

		[Inject]
		private IViewProvider _viewProvider;
	}
}
