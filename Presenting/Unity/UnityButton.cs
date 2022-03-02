using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnityButton : IButton, IActivatable, IValueHolder
	{
		public UnityButton(Button button)
		{
			this._button = button;
			this._spriteState = default(SpriteState);
		}

		public bool IsInteractable
		{
			get
			{
				return this._button.interactable;
			}
			set
			{
				this._button.interactable = value;
			}
		}

		public IObservable<Unit> OnClick()
		{
			return UnityUIComponentExtensions.OnClickAsObservable(this._button);
		}

		public int GetId()
		{
			return this._button.transform.GetInstanceID();
		}

		public bool HasValue
		{
			get
			{
				return this._button != null;
			}
		}

		public void SetActive(bool active)
		{
			this._button.gameObject.SetActive(active);
		}

		public void SetPressedSprite(Sprite sprite)
		{
			this._spriteState.pressedSprite = sprite;
			this._spriteState.disabledSprite = sprite;
			this._button.spriteState = this._spriteState;
		}

		public void SetHighlightedSprite(Sprite sprite)
		{
			this._spriteState.highlightedSprite = sprite;
			this._button.spriteState = this._spriteState;
		}

		[SerializeField]
		private Button _button;

		private SpriteState _spriteState;
	}
}
