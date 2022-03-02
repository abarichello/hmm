using System;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.NGui
{
	[Serializable]
	public class NGuiButton : IButton, IActivatable, IValueHolder
	{
		public UIButton UIButton
		{
			get
			{
				return this._button;
			}
			set
			{
				this._button = value;
			}
		}

		public bool IsInteractable
		{
			get
			{
				return this._button.GetComponent<BoxCollider>().enabled;
			}
			set
			{
				this._button.GetComponent<BoxCollider>().enabled = value;
				UIButton[] components = this._button.GetComponents<UIButton>();
				for (int i = 0; i < components.Length; i++)
				{
					components[i].SetState((!value) ? UIButtonColor.State.Disabled : UIButtonColor.State.Normal, true);
				}
			}
		}

		public IObservable<Unit> OnClick()
		{
			return Observable.Create<Unit>(delegate(IObserver<Unit> observer)
			{
				EventDelegate eventHandler = new EventDelegate(delegate()
				{
					observer.OnNext(Unit.Default);
				});
				this._button.onClick.Add(eventHandler);
				return Disposable.Create(delegate()
				{
					this._button.onClick.Remove(eventHandler);
				});
			});
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
			this._button.pressedSprite2D = sprite;
		}

		public void SetHighlightedSprite(Sprite sprite)
		{
			this._button.hoverSprite2D = sprite;
		}

		[SerializeField]
		private UIButton _button;
	}
}
