using System;
using Pocketverse;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	public class HudLifebarStartupMinigame : GameHubBehaviour
	{
		public void Setup(int objectId, bool isCurrentPlayer, Transform lifebarParentTransform, float gridYellowZone, float gridGreenZone)
		{
			float num = (!isCurrentPlayer) ? this.ScaleForOthers : this.ScaleForCurrentPlayer;
			this.CanvasGroup.transform.localScale = new Vector3(num, num, num);
			this._lifebarParentTransform = lifebarParentTransform;
			this._objectId = objectId;
			this._gridYellowZone = gridYellowZone;
			this._gridGreenZone = gridGreenZone;
			this.OrangeBgImage.fillAmount = this._gridYellowZone;
			this.GreenBgImage.fillAmount = this._gridGreenZone;
			this.RedBgImage.fillAmount = 1f - this._gridYellowZone - this._gridGreenZone;
			Vector3 localPosition = this.RedRect.localPosition;
			localPosition.x = Mathf.Ceil(this.RedRect.sizeDelta.x * (this._gridYellowZone + this._gridGreenZone)) + this.BarsSpacing;
			this.RedRect.localPosition = localPosition;
			Vector3 localPosition2 = this.GreenRect.localPosition;
			localPosition2.x = Mathf.Ceil(this.GreenRect.sizeDelta.x * this._gridYellowZone) + this.BarsSpacing;
			this.GreenRect.localPosition = localPosition2;
			this.BorderLeftImage.fillAmount = gridYellowZone;
			this.BorderRightImage.fillAmount = 1f - (gridYellowZone + gridGreenZone);
			Vector2 sizeDelta = this.BorderGreenImage.rectTransform.sizeDelta;
			float num2 = this.GreenBgImage.rectTransform.sizeDelta.x * gridGreenZone;
			sizeDelta.x = num2 + this.BorderGreenImage.sprite.border.x + this.BorderGreenImage.sprite.border.z;
			this.BorderGreenImage.rectTransform.sizeDelta = sizeDelta;
			Vector3 localPosition3 = this.BorderGreenImage.rectTransform.localPosition;
			localPosition3.x = this.GreenBgImage.rectTransform.sizeDelta.x * gridYellowZone - this.BorderGreenImage.sprite.border.x;
			this.BorderGreenImage.rectTransform.localPosition = localPosition3;
		}

		public int GetObjectId()
		{
			return this._objectId;
		}

		public void Update()
		{
			if (this.CanvasGroup.alpha < 0.001f)
			{
				return;
			}
			this.RenderUpdate();
		}

		public void SetRaceStartupProgress(float value)
		{
			this._minigameStartupProgress = value / 100f;
		}

		public void SetSliderValue(float value, float normGreenStart, float normGreenEnd)
		{
			this.OrangeSlider.value = Mathf.Min(value, normGreenStart);
			this.GreenSlider.value = Mathf.Max(0f, Mathf.Min(value, normGreenEnd) - normGreenStart);
			this.RedSlider.value = Mathf.Max(0f, value - normGreenEnd);
		}

		private void RenderUpdate()
		{
			float gridYellowZone = this._gridYellowZone;
			float normGreenEnd = gridYellowZone + this._gridGreenZone;
			this.SetSliderValue(this._minigameStartupProgress, gridYellowZone, normGreenEnd);
		}

		public void SetVisible(bool isVisible)
		{
			this.CanvasGroup.alpha = ((!isVisible) ? 0f : 1f);
			base.transform.SetParent((!isVisible) ? this.MainParentTransform : this._lifebarParentTransform);
			base.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HudLifebarStartupMinigame));

		public float ScaleForCurrentPlayer = 1f;

		public float ScaleForOthers = 0.75f;

		public float BarsSpacing = 2f;

		public Transform MainParentTransform;

		private Transform _lifebarParentTransform;

		public CanvasGroup CanvasGroup;

		public Slider OrangeSlider;

		public Image OrangeBgImage;

		public RectTransform GreenRect;

		public Slider GreenSlider;

		public Image GreenBgImage;

		public RectTransform RedRect;

		public Slider RedSlider;

		public Image RedBgImage;

		public Image BorderLeftImage;

		public Image BorderRightImage;

		public Image BorderGreenImage;

		private int _objectId;

		private float _gridYellowZone;

		private float _gridGreenZone;

		private float _minigameStartupProgress;
	}
}
