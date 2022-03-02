using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Tournaments.Presenting.Detailing.Rankings
{
	public class UnityTournamentRankingTopCellView : UnityTournamentRankingCellView, ITournamentRankingTopCellView, ITournamentRankingCellView
	{
		public ILabel Ranking2Text
		{
			get
			{
				return this._ranking2Label;
			}
		}

		public void SetFirstPlace()
		{
			this.ShowMedalAndRankingText();
			this._medalImage.sprite = this._firstPlaceMedalSprite;
		}

		public void SetSecondPlace()
		{
			this.ShowMedalAndRankingText();
			this._medalImage.sprite = this._secondPlaceMedalSprite;
		}

		public void SetThirdPlace()
		{
			this.ShowMedalAndRankingText();
			this._medalImage.sprite = this._thirdPlaceMedalSprite;
		}

		public void ShowRanking2Text()
		{
			this._rankingTextGameObject.SetActive(false);
			this._ranking2TextGameObject.SetActive(true);
			this._medalImage.gameObject.SetActive(false);
		}

		private void ShowMedalAndRankingText()
		{
			this._rankingTextGameObject.SetActive(true);
			this._ranking2TextGameObject.SetActive(false);
			this._medalImage.gameObject.SetActive(true);
		}

		[SerializeField]
		private UnityLabel _ranking2Label;

		[SerializeField]
		private Image _medalImage;

		[SerializeField]
		private Sprite _firstPlaceMedalSprite;

		[SerializeField]
		private Sprite _secondPlaceMedalSprite;

		[SerializeField]
		private Sprite _thirdPlaceMedalSprite;

		[SerializeField]
		private GameObject _rankingTextGameObject;

		[SerializeField]
		private GameObject _ranking2TextGameObject;
	}
}
