using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavyMetalMachines.UnityUI
{
	[RequireComponent(typeof(Selectable))]
	public class UnityUiHoverVisibility : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
	{
		protected void Awake()
		{
			this._selectable = base.GetComponent<Selectable>();
			this._targetGraphics = this._targetGameObject.GetComponentsInChildren<Graphic>();
			for (int i = 0; i < this._targetGraphics.Length; i++)
			{
				this._targetGraphics[i].CrossFadeAlpha(0f, 0f, true);
			}
		}

		protected void OnEnable()
		{
			this._targetCanvasGroup.alpha = 0f;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (this._selectable.IsInteractable())
			{
				this._targetCanvasGroup.alpha = 1f;
				for (int i = 0; i < this._targetGraphics.Length; i++)
				{
					this._targetGraphics[i].CrossFadeAlpha(1f, this._crossFadeTimeInSec, true);
				}
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (this._selectable.IsInteractable())
			{
				this._targetCanvasGroup.alpha = 1f;
				for (int i = 0; i < this._targetGraphics.Length; i++)
				{
					this._targetGraphics[i].CrossFadeAlpha(0f, this._crossFadeTimeInSec, true);
				}
			}
		}

		[SerializeField]
		private GameObject _targetGameObject;

		[SerializeField]
		private CanvasGroup _targetCanvasGroup;

		[SerializeField]
		private float _crossFadeTimeInSec = 0.3f;

		private Graphic[] _targetGraphics;

		private Selectable _selectable;
	}
}
