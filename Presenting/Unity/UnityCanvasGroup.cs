using System;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnityCanvasGroup : ICanvasGroup, IActivatable, IValueHolder
	{
		public UnityCanvasGroup(CanvasGroup canvasGroup)
		{
			this._canvasGroup = canvasGroup;
		}

		public float Alpha
		{
			get
			{
				return this._canvasGroup.alpha;
			}
			set
			{
				this._canvasGroup.alpha = value;
			}
		}

		public bool Interactable
		{
			get
			{
				return this._canvasGroup.interactable;
			}
			set
			{
				this._canvasGroup.interactable = value;
				this._canvasGroup.blocksRaycasts = value;
			}
		}

		public bool HasValue { get; private set; }

		public void SetActive(bool active)
		{
			this._canvasGroup.gameObject.SetActive(active);
		}

		[SerializeField]
		private CanvasGroup _canvasGroup;
	}
}
