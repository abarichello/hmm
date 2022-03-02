using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.NGui
{
	public class NGuiDropdown<T> : IDropdown<T>
	{
		protected NGuiDropdown(UIPopupList popupList)
		{
			this._popupList = popupList;
		}

		private Dictionary<T, string> Labels
		{
			get
			{
				Dictionary<T, string> result;
				if ((result = this._labels) == null)
				{
					result = (this._labels = new Dictionary<T, string>());
				}
				return result;
			}
		}

		private Dictionary<string, T> Options
		{
			get
			{
				Dictionary<string, T> result;
				if ((result = this._options) == null)
				{
					result = (this._options = new Dictionary<string, T>());
				}
				return result;
			}
		}

		public void Hide()
		{
			this._popupList.CloseSelf();
		}

		public string SelectedLabel
		{
			get
			{
				return this._popupList.value;
			}
			set
			{
				this._popupList.value = value;
				this._currentIndex = this._popupList.items.FindIndex((string item) => item == value);
			}
		}

		public T SelectedOption
		{
			get
			{
				T result;
				this.Options.TryGetValue(this.SelectedLabel, out result);
				return result;
			}
			set
			{
				string selectedLabel;
				this.Labels.TryGetValue(value, out selectedLabel);
				this.SelectedLabel = selectedLabel;
			}
		}

		public bool Interactable
		{
			get
			{
				Collider component = this._popupList.GetComponent<Collider>();
				return component && component.enabled;
			}
			set
			{
				Collider component = this._popupList.GetComponent<Collider>();
				if (component)
				{
					component.enabled = value;
				}
				UIButton[] components = this._popupList.GetComponents<UIButton>();
				UIButtonColor.State state = (!value) ? UIButtonColor.State.Disabled : UIButtonColor.State.Normal;
				foreach (UIButton uibutton in components)
				{
					uibutton.SetState(state, true);
				}
			}
		}

		public IObservable<T> OnSelectionChanged()
		{
			return Observable.Create<T>(delegate(IObserver<T> observer)
			{
				EventDelegate eventHandler = new EventDelegate(delegate()
				{
					string stringValue = this._popupList.value;
					int num = this._popupList.items.FindIndex((string item) => item == stringValue);
					if (num != this._currentIndex)
					{
						observer.OnNext(this.Options[stringValue]);
						this._currentIndex = num;
					}
				});
				this._popupList.onChange.Add(eventHandler);
				return Disposable.Create(delegate()
				{
					this._popupList.onChange.Remove(eventHandler);
				});
			});
		}

		public void AddOptions(List<T> options, List<string> labels)
		{
			this._popupList.items = labels;
			for (int i = 0; i < labels.Count; i++)
			{
				this.Options[labels[i]] = options[i];
				this.Labels[options[i]] = labels[i];
			}
		}

		public void ClearOptions()
		{
			this.Options.Clear();
			this.Labels.Clear();
			this._popupList.items.Clear();
			this._currentIndex = -1;
		}

		[SerializeField]
		private UIPopupList _popupList;

		private Dictionary<string, T> _options = new Dictionary<string, T>();

		private Dictionary<T, string> _labels = new Dictionary<T, string>();

		private int _currentIndex = -1;
	}
}
