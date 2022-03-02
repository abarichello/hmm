using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.Tournaments.Presenting.Detailing.Teams
{
	public class TournamentTeamMemberView : MonoBehaviour, ITournamentTeamMemberView
	{
		public ILabel NameLabel
		{
			get
			{
				return this._nameLabel;
			}
		}

		public IDynamicImage AvatarIconImage
		{
			get
			{
				return this._avatarIconImage;
			}
		}

		public IImage BorderImage
		{
			get
			{
				return this._borderImage;
			}
		}

		public IDynamicImage PortraitImage
		{
			get
			{
				return this._portraitImage;
			}
		}

		public IActivatable IsLeaderIndicator
		{
			get
			{
				return this._isLeaderIndicator;
			}
		}

		public IActivatable ExistingMemberGroup
		{
			get
			{
				return this._existingMemberGroup;
			}
		}

		public IActivatable EmptyMemberGroup
		{
			get
			{
				return this._emptyMemberGroup;
			}
		}

		public IAnimation CriteriaUnmetToCriteriaMetAnimation
		{
			get
			{
				return this._criteriaUnmetToCriteriaMetAnimation;
			}
		}

		public IAnimation CriteriaMetToCriteriaUnmetAnimation
		{
			get
			{
				return this._criteriaMetToCriteriaUnmetAnimation;
			}
		}

		public IAnimation CriteriaMetToParticipatingAnimation
		{
			get
			{
				return this._criteriaMetToParticipatingAnimation;
			}
		}

		public IAnimation CriteriaUnmetToParticipatingAnimation
		{
			get
			{
				return this._criteriaUnmetToParticipatingAnimation;
			}
		}

		public IAnimation ParticipatingToCriteriaUnmetAnimation
		{
			get
			{
				return this._participatingToCriteriaUnmetAnimation;
			}
		}

		public IAnimation ParticipatingToCriteriaMetAnimation
		{
			get
			{
				return this._participatingToCriteriaMetAnimation;
			}
		}

		public float EmptySlotAlpha
		{
			get
			{
				return this._emptySlotAlpha;
			}
		}

		public float NormalSlotAlpha
		{
			get
			{
				return this._normalSlotAlpha;
			}
		}

		public ILabel VictoriesCriteriaLabel
		{
			get
			{
				return this._victoriesCriteriaLabel;
			}
		}

		public ILabel CompetitiveCriteriaLabel
		{
			get
			{
				return this._competitiveCriteriaLabel;
			}
		}

		public Color CriteriaMetColor
		{
			get
			{
				return this._criteriaMetColor;
			}
		}

		public Color CriteriaUnmetColor
		{
			get
			{
				return this._criteriaUnmetColor;
			}
		}

		public IDynamicImage CompetitiveCriteriaImage
		{
			get
			{
				return this._competitiveCriteriaImage;
			}
		}

		public IAnimation JoinedGroupAnimation
		{
			get
			{
				return this._joinedGroupAnimation;
			}
		}

		public IAnimation LeftGroupAnimation
		{
			get
			{
				return this._leftGroupAnimation;
			}
		}

		public ICanvasGroup GroupJoinCanvasGroup
		{
			get
			{
				return this._groupJoinCanvasGroup;
			}
		}

		public ICanvasGroup CriteriaMetCanvasGroup
		{
			get
			{
				return this._criteriaMetCanvasGroup;
			}
		}

		public ICanvasGroup CriteriaUnmetCanvasGroup
		{
			get
			{
				return this._criteriaUnmetCanvasGroup;
			}
		}

		public IActivatable PsnIdGroupActivatable
		{
			get
			{
				return this._psnIdGroupActivatable;
			}
		}

		public IActivatable PlayerBannedGroupActivatable
		{
			get
			{
				return this._playerBannedGroupActivatable;
			}
		}

		public ILabel PsnIdLabel
		{
			get
			{
				return this._psnIdLabel;
			}
		}

		[SerializeField]
		private GameObjectActivatable _isLeaderIndicator;

		[SerializeField]
		private UnityDynamicRawImage _avatarIconImage;

		[SerializeField]
		private UnityImage _borderImage;

		[SerializeField]
		private UnityDynamicImage _portraitImage;

		[SerializeField]
		[Range(0f, 1f)]
		private float _normalSlotAlpha = 1f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _emptySlotAlpha = 0.5f;

		[SerializeField]
		private UnityLabel _nameLabel;

		[SerializeField]
		private GameObjectActivatable _existingMemberGroup;

		[SerializeField]
		private GameObjectActivatable _emptyMemberGroup;

		[SerializeField]
		private UnityAnimation _criteriaUnmetToCriteriaMetAnimation;

		[SerializeField]
		private UnityAnimation _criteriaMetToCriteriaUnmetAnimation;

		[SerializeField]
		private UnityAnimation _criteriaMetToParticipatingAnimation;

		[SerializeField]
		private UnityAnimation _criteriaUnmetToParticipatingAnimation;

		[SerializeField]
		private UnityAnimation _participatingToCriteriaUnmetAnimation;

		[SerializeField]
		private UnityAnimation _participatingToCriteriaMetAnimation;

		[SerializeField]
		private UnityLabel _victoriesCriteriaLabel;

		[SerializeField]
		private UnityLabel _competitiveCriteriaLabel;

		[SerializeField]
		private UnityDynamicImage _competitiveCriteriaImage;

		[SerializeField]
		private UnityAnimation _joinedGroupAnimation;

		[SerializeField]
		private UnityAnimation _leftGroupAnimation;

		[SerializeField]
		private UnityCanvasGroup _groupJoinCanvasGroup;

		[SerializeField]
		private UnityCanvasGroup _criteriaMetCanvasGroup;

		[SerializeField]
		private UnityCanvasGroup _criteriaUnmetCanvasGroup;

		[SerializeField]
		private Color _criteriaUnmetColor;

		[SerializeField]
		private Color _criteriaMetColor;

		[SerializeField]
		private GameObjectActivatable _psnIdGroupActivatable;

		[SerializeField]
		private GameObjectActivatable _playerBannedGroupActivatable;

		[SerializeField]
		private UnityLabel _psnIdLabel;
	}
}
