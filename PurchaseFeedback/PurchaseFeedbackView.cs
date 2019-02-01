using System;
using System.Collections;
using HeavyMetalMachines.Frontend;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.PurchaseFeedback
{
	public class PurchaseFeedbackView : MonoBehaviour, IPurchaseFeedbackView
	{
		protected void Awake()
		{
			this._purchaseFeedbackComponent = this._purchaseFeedbackComponentAsset;
		}

		protected void Start()
		{
			this._purchaseFeedbackComponent.RegisterView(this);
		}

		public void Show(PurchaseFeedbackView.PurchasedFeedbackViewData viewData)
		{
			this.SetupComponents(viewData);
			this._mainCanvas.enabled = true;
			this._okButton.interactable = false;
			base.StartCoroutine(this.ShowCoroutine());
		}

		private IEnumerator ShowCoroutine()
		{
			yield return this.WaitForAnimation("purchaseFeedbackIn");
			this._mainAnimation.Play("purchaseFeedbackIdle");
			this._okButton.interactable = true;
			yield break;
		}

		public void Hide()
		{
			this._okButton.interactable = true;
			base.StartCoroutine(this.HideCoroutine());
		}

		private IEnumerator HideCoroutine()
		{
			yield return this.WaitForAnimation("purchaseFeedbackOut");
			this._purchaseFeedbackComponent.OnViewClosed();
			yield break;
		}

		private void SetupComponents(PurchaseFeedbackView.PurchasedFeedbackViewData viewData)
		{
			this._descriptionText.text = string.Format(Language.Get("PURCHASED_FEEDBACK_DESCRIPTION", TranslationSheets.Store), viewData.ItemHardCurrencyValue);
			this._cashText.text = viewData.UserCurrentCurrencyValue;
			this._itemImage.TryToLoadAsset(viewData.ItemImageAssetName);
		}

		private WaitForSeconds WaitForAnimation(string clipName)
		{
			this._mainAnimation.Play(clipName);
			return new WaitForSeconds(this._mainAnimation.GetClip(clipName).length + Time.deltaTime);
		}

		[UnityUiComponentCall]
		public void OnButtonOkClick()
		{
			this._purchaseFeedbackComponent.HideView();
		}

		[Header("[Infra]")]
		[SerializeField]
		private PurchaseFeedbackComponent _purchaseFeedbackComponentAsset;

		private IPurchaseFeedbackComponent _purchaseFeedbackComponent;

		[Header("[Infra]")]
		[SerializeField]
		private Canvas _mainCanvas;

		[SerializeField]
		private Animation _mainAnimation;

		[SerializeField]
		private Text _descriptionText;

		[SerializeField]
		private Text _cashText;

		[SerializeField]
		private HmmUiImage _itemImage;

		[SerializeField]
		private Button _okButton;

		public struct PurchasedFeedbackViewData
		{
			public string ItemImageAssetName;

			public string ItemHardCurrencyValue;

			public string UserCurrentCurrencyValue;
		}
	}
}
