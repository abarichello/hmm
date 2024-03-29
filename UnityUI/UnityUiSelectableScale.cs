﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HeavyMetalMachines.UnityUI
{
	[RequireComponent(typeof(Selectable))]
	public class UnityUiSelectableScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler, IEventSystemHandler
	{
		protected virtual void Awake()
		{
			this._selectable = base.GetComponent<Selectable>();
		}

		public virtual void OnPointerEnter(PointerEventData eventData)
		{
			if (!this._selectable.IsInteractable())
			{
				return;
			}
			this._isEnter = true;
			this._scaleUpCoroutine = base.StartCoroutine(this.ScaleUp());
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			this._isEnter = false;
			this._scaleDownCoroutine = base.StartCoroutine(this.ScaleDown());
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (eventData.button != null || !this._selectable.IsInteractable())
			{
				return;
			}
			if (this._isEnter)
			{
				this._scaleDownCoroutine = base.StartCoroutine(this.ScaleDown());
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (this._ignorePointerUp || eventData.button != null || !this._selectable.IsInteractable())
			{
				return;
			}
			if (this._isEnter)
			{
				this._scaleUpCoroutine = base.StartCoroutine(this.ScaleUp());
			}
		}

		private IEnumerator ScaleUp()
		{
			if (this._scaleDownCoroutine != null)
			{
				base.StopCoroutine(this._scaleDownCoroutine);
				this._scaleDownCoroutine = null;
			}
			while (this._timerCountInSec < this._crossFadeTimeInSec)
			{
				this._selectable.transform.localScale = Vector3.Lerp(this._fromScale, this._toScale, this._timerCountInSec / this._crossFadeTimeInSec);
				yield return null;
				this._timerCountInSec += Time.deltaTime;
			}
			this._timerCountInSec = this._crossFadeTimeInSec;
			this._selectable.transform.localScale = Vector3.Lerp(this._fromScale, this._toScale, 1f);
			this._scaleUpCoroutine = null;
			yield break;
		}

		private IEnumerator ScaleDown()
		{
			if (this._scaleUpCoroutine != null)
			{
				base.StopCoroutine(this._scaleUpCoroutine);
				this._scaleUpCoroutine = null;
			}
			while (this._timerCountInSec > 0f)
			{
				this._selectable.transform.localScale = Vector3.Lerp(this._fromScale, this._toScale, this._timerCountInSec / this._crossFadeTimeInSec);
				yield return null;
				this._timerCountInSec -= Time.deltaTime;
			}
			this._timerCountInSec = 0f;
			this._selectable.transform.localScale = Vector3.Lerp(this._fromScale, this._toScale, 0f);
			this._scaleDownCoroutine = null;
			yield break;
		}

		public void OnSelect(BaseEventData eventData)
		{
			if (this._isEnter)
			{
				return;
			}
			if (this._scaleUpCoroutine == null && this._selectable.IsInteractable())
			{
				this._scaleUpCoroutine = base.StartCoroutine(this.ScaleUp());
			}
		}

		public void OnDeselect(BaseEventData eventData)
		{
			if (this._scaleDownCoroutine == null)
			{
				this._scaleDownCoroutine = base.StartCoroutine(this.ScaleDown());
			}
		}

		[SerializeField]
		private float _crossFadeTimeInSec = 0.1f;

		[SerializeField]
		private Vector3 _fromScale = Vector3.one;

		[SerializeField]
		private Vector3 _toScale = new Vector3(1.1f, 1.1f, 1.1f);

		[SerializeField]
		private bool _ignorePointerUp;

		private Selectable _selectable;

		private float _timerCountInSec;

		private Coroutine _scaleUpCoroutine;

		private Coroutine _scaleDownCoroutine;

		private bool _isEnter;
	}
}
