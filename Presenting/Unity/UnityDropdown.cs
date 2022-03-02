using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class UnityDropdown<T> : IDropdown<T>
	{
		public UnityDropdown(Dropdown dropdown)
		{
			this._dropdown = dropdown;
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

		public void ClearOptions()
		{
			this.Options.Clear();
			this.Labels.Clear();
			this._dropdown.ClearOptions();
		}

		public void Hide()
		{
			this._dropdown.Hide();
		}

		public void AddOptions(List<T> options, List<string> labels)
		{
			this._dropdown.AddOptions(labels);
			for (int i = 0; i < labels.Count; i++)
			{
				this.Options[labels[i]] = options[i];
				this.Labels[options[i]] = labels[i];
			}
		}

		public bool Interactable
		{
			get
			{
				return this._dropdown.interactable;
			}
			set
			{
				this._dropdown.interactable = value;
			}
		}

		public string SelectedLabel
		{
			get
			{
				return this._dropdown.options[this._dropdown.value].text;
			}
			set
			{
				this._dropdown.value = this._dropdown.options.FindIndex((Dropdown.OptionData item) => value == item.text);
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

		public IObservable<T> OnSelectionChanged()
		{
			return Observable.Select<int, T>(Observable.Where<int>(Observable.Skip<int>(UnityUIComponentExtensions.OnValueChangedAsObservable(this._dropdown), 1), (int _) => this.Options.ContainsKey(this.SelectedLabel)), (int index) => this.Options[this.SelectedLabel]);
		}

		[SerializeField]
		private Dropdown _dropdown;

		private Dictionary<string, T> _options = new Dictionary<string, T>();

		private Dictionary<T, string> _labels = new Dictionary<T, string>();
	}
}
