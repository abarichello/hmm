using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavyMetalMachines.UnityUI
{
	[RequireComponent(typeof(Selectable))]
	public class UnityUiButtonHoverValue : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
	{
		public void Setup(string valueText)
		{
			this._selectable = base.GetComponent<Selectable>();
			this._valueText.text = valueText;
			this._titleText.gameObject.SetActive(true);
			this._valueText.gameObject.SetActive(true);
			this._titleText.CrossFadeAlpha(1f, 0f, true);
			this._valueText.CrossFadeAlpha(0f, 0f, true);
		}

		protected void OnEnable()
		{
			this._titleText.CrossFadeAlpha(1f, 0f, true);
			this._valueText.CrossFadeAlpha(0f, 0f, true);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (this._selectable.IsInteractable() && this._selectable.image.raycastTarget)
			{
				this._titleText.CrossFadeAlpha(0f, this._crossFadeTimeInSec, true);
				this._valueText.CrossFadeAlpha(1f, this._crossFadeTimeInSec, true);
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (this._selectable.IsInteractable() && this._selectable.image.raycastTarget)
			{
				this._titleText.CrossFadeAlpha(1f, this._crossFadeTimeInSec, true);
				this._valueText.CrossFadeAlpha(0f, this._crossFadeTimeInSec, true);
			}
		}

		[SerializeField]
		private float _crossFadeTimeInSec = 0.2f;

		[SerializeField]
		private Text _titleText;

		[SerializeField]
		private Text _valueText;

		private Selectable _selectable;
	}
}
