using System;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.Tournaments.Presenting.Detailing.Rankings
{
	public class UnityTournamentRankingCellView : EnhancedScrollerCellView, ITournamentRankingCellView
	{
		public ILabel RankingText
		{
			get
			{
				return this._rankingText;
			}
		}

		public ILabel TeamTagText
		{
			get
			{
				return this._teamTagText;
			}
		}

		public ILabel TeamNameText
		{
			get
			{
				return this._teamNameText;
			}
		}

		public ILabel TeamUserGeneratedContentCurrentOwnerPublisherUserNameLabel
		{
			get
			{
				return this._teamUserGeneratedContentCurrentOwnerPublisherUserNameLabel;
			}
		}

		public ILabel ScoreText
		{
			get
			{
				return this._scoreText;
			}
		}

		public ILabel ClassificatoryText
		{
			get
			{
				return this._classificatoryText;
			}
		}

		public void SetTeamMembersTooltip(string tooltipText)
		{
			this._teamMembersTooltipTrigger.TooltipText = tooltipText;
		}

		public void DisableTeamMembersTooltip()
		{
			this._teamMembersTooltipTrigger.IteractableStateToShow = HMMUnityUiTooltipTrigger.ShowOnInteractableState.Disabled;
		}

		public float GetSize()
		{
			return this._rectTransform.sizeDelta.y;
		}

		public void SetClassificatoryVisible()
		{
			this._classificatoryGameObject.SetActive(true);
		}

		public void SetClassificatoryNotVisible()
		{
			this._classificatoryGameObject.SetActive(false);
		}

		public void LoadTeamIcon(string assetName)
		{
			if (string.IsNullOrEmpty(assetName))
			{
				this._teamIconRawImage.ClearAsset();
			}
			else
			{
				this._teamIconRawImage.TryToLoadAsset(assetName);
			}
		}

		public void SetAsMyTeam()
		{
			this._myTeamBgGameObject.SetActive(true);
			this._myTeamBgClassificatoryGameObject.SetActive(true);
		}

		public void SetAsNotMyTeam()
		{
			this._myTeamBgGameObject.SetActive(false);
			this._myTeamBgClassificatoryGameObject.SetActive(false);
		}

		public void SetTieBreakerTooltip(string tooltipText)
		{
			this._tieBreakerTooltipTrigger.TooltipText = tooltipText;
		}

		public void SetAsDeleted()
		{
			this._teamGameObject.SetActive(false);
			this._teamDeletedGameObject.SetActive(true);
		}

		public void SetAsNotDeleted()
		{
			this._teamGameObject.SetActive(true);
			this._teamDeletedGameObject.SetActive(false);
		}

		private void OnValidate()
		{
			this._rectTransform = base.GetComponent<RectTransform>();
		}

		[SerializeField]
		private UnityLabel _rankingText;

		[SerializeField]
		private UnityLabel _teamTagText;

		[SerializeField]
		private UnityLabel _teamNameText;

		[SerializeField]
		private UnityLabel _teamUserGeneratedContentCurrentOwnerPublisherUserNameLabel;

		[SerializeField]
		private UnityLabel _scoreText;

		[SerializeField]
		private UnityLabel _classificatoryText;

		[SerializeField]
		private GameObject _classificatoryGameObject;

		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private HMMUnityUiTooltipTrigger _teamMembersTooltipTrigger;

		[SerializeField]
		private HmmUiRawImage _teamIconRawImage;

		[SerializeField]
		private GameObject _myTeamBgGameObject;

		[SerializeField]
		private GameObject _myTeamBgClassificatoryGameObject;

		[SerializeField]
		private HMMUnityUiTooltipTrigger _tieBreakerTooltipTrigger;

		[SerializeField]
		private GameObject _teamGameObject;

		[SerializeField]
		private GameObject _teamDeletedGameObject;
	}
}
