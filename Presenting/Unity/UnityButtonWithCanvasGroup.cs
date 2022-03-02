using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnityButtonWithCanvasGroup : IButton, IActivatable, IValueHolder
	{
		public bool HasValue
		{
			get
			{
				return this._button != null && this._canvasGroup != null;
			}
		}

		public void SetActive(bool active)
		{
			this._button.gameObject.SetActive(active);
			this._canvasGroup.gameObject.SetActive(active);
		}

		public IObservable<Unit> OnClick()
		{
			return UnityUIComponentExtensions.OnClickAsObservable(this._button);
		}

		public int GetId()
		{
			return this._button.transform.GetInstanceID();
		}

		public bool IsInteractable
		{
			get
			{
				return this._button.interactable && this._canvasGroup.interactable;
			}
			set
			{
				this._button.interactable = value;
				this._canvasGroup.interactable = value;
				this._canvasGroup.blocksRaycasts = value;
			}
		}

		[SerializeField]
		private Button _button;

		[SerializeField]
		private CanvasGroup _canvasGroup;
	}
}
