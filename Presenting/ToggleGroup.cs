using System;
using System.Collections.Generic;
using UniRx;

namespace HeavyMetalMachines.Presenting
{
	public class ToggleGroup<T>
	{
		public T SelectedItem
		{
			get
			{
				return this._selectedItem;
			}
		}

		public void AddItem(T item, IToggle toggle)
		{
			ToggleGroupItem toggleGroupItem = new ToggleGroupItem();
			toggleGroupItem.toggle = toggle;
			toggleGroupItem.disposable = ObservableExtensions.Subscribe<Unit>(toggle.OnToggleOn(), delegate(Unit _)
			{
				this._selectedItem = item;
				this._selectedItemChanged.OnNext(this._selectedItem);
			});
			this._itemToggles.Add(item, toggleGroupItem);
		}

		public void SelectItem(T item)
		{
			if (this._itemToggles.Count > 0)
			{
				ToggleGroupItem toggleGroupItem = this._itemToggles[item];
				toggleGroupItem.toggle.IsOn = true;
			}
		}

		public void ClearItems()
		{
			foreach (KeyValuePair<T, ToggleGroupItem> keyValuePair in this._itemToggles)
			{
				keyValuePair.Value.disposable.Dispose();
			}
			this._itemToggles.Clear();
		}

		public IObservable<T> OnSelectedItemChanged()
		{
			return this._selectedItemChanged;
		}

		private T _selectedItem;

		private readonly Dictionary<T, ToggleGroupItem> _itemToggles = new Dictionary<T, ToggleGroupItem>();

		private readonly Subject<T> _selectedItemChanged = new Subject<T>();
	}
}
