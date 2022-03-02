using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Tournaments.Presenting.Detailing.Rankings
{
	[Obsolete]
	public class TournamentRankingStepSelectorView : MonoBehaviour, ITournamentRankingStepSelectorView
	{
		public IToggle SelectionToggle
		{
			get
			{
				return this._selectionToggleUnity;
			}
		}

		public void SetVisible()
		{
			this._mainGameObject.SetActive(true);
		}

		public void SetNotVisible()
		{
			this._mainGameObject.SetActive(false);
		}

		public void SetOldStep()
		{
			this._baseRawImage.texture = this._oldBaseTexture;
			this._lineImage.sprite = this._doneLineSprite;
			this._lineImage.color = this._lineImage.color.ChangeAlpha(this._oldLineImageAlpha);
			this._toggleBgRawImage.texture = this._oldToggleBgTexture;
			this._highlightGameObject.SetActive(false);
			this.SelectionToggle.IsInteractable = !this.SelectionToggle.IsOn;
		}

		public void SetClosestStep()
		{
			this._baseRawImage.texture = this._closestBaseTexture;
			this._lineImage.sprite = this._futureLineSprite;
			this._lineImage.color = this._lineImage.color.ChangeAlpha(this._futureLineImageAlpha);
			this._toggleBgRawImage.texture = this._closestToggleBgTexture;
			this._highlightGameObject.SetActive(true);
			this.SelectionToggle.IsInteractable = !this.SelectionToggle.IsOn;
		}

		public void SetFutureStep()
		{
			this._baseRawImage.texture = this._futureBaseTexture;
			this._lineImage.sprite = this._futureLineSprite;
			this._lineImage.color = this._lineImage.color.ChangeAlpha(this._futureLineImageAlpha);
			this._toggleBgRawImage.texture = this._futureToggleBgTexture;
			this._highlightGameObject.SetActive(false);
			this.SelectionToggle.IsInteractable = false;
		}

		public void DisableLineConnection()
		{
			this._lineConnectionGameObject.SetActive(false);
		}

		[SerializeField]
		private GameObject _mainGameObject;

		[SerializeField]
		private GameObject _highlightGameObject;

		[SerializeField]
		private UnityToggle _selectionToggleUnity;

		[SerializeField]
		private RawImage _baseRawImage;

		[SerializeField]
		private Image _lineImage;

		[SerializeField]
		private RawImage _toggleBgRawImage;

		[SerializeField]
		private GameObject _lineConnectionGameObject;

		[SerializeField]
		private Texture _oldBaseTexture;

		[SerializeField]
		private Texture _closestBaseTexture;

		[SerializeField]
		private Texture _futureBaseTexture;

		[SerializeField]
		private Sprite _doneLineSprite;

		[SerializeField]
		private Sprite _futureLineSprite;

		[SerializeField]
		private Texture _oldToggleBgTexture;

		[SerializeField]
		private Texture _closestToggleBgTexture;

		[SerializeField]
		private Texture _futureToggleBgTexture;

		[SerializeField]
		[Range(0f, 1f)]
		private float _oldLineImageAlpha = 1f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _futureLineImageAlpha = 0.5f;
	}
}
