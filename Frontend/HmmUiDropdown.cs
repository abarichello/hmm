using System;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	[AddComponentMenu("UI/HMM/HmmUiDropdown")]
	public class HmmUiDropdown : Dropdown
	{
		protected override GameObject CreateDropdownList(GameObject template)
		{
			GameObject gameObject = base.CreateDropdownList(template);
			if (this._enableUiNavigation)
			{
				this.AddUiNavigationGroupHolderAndSetHighPriority(gameObject);
			}
			return gameObject;
		}

		private void AddUiNavigationGroupHolderAndSetHighPriority(GameObject dropdownList)
		{
			UiNavigationGroupHolder uiNavigationGroupHolder = this._diContainer.InstantiateComponent<UiNavigationGroupHolder>(dropdownList);
			this._diContainer.InstantiateComponent<UiNavigationAxisSelector>(dropdownList);
			uiNavigationGroupHolder.AddHighPriorityGroup();
			this._inputCancelDownDisposable = ObservableExtensions.Subscribe<Unit>(uiNavigationGroupHolder.ObserveInputCancelDown(), delegate(Unit _)
			{
				base.Hide();
			});
			this._focusChangeDisposable = ObservableExtensions.Subscribe<bool>(uiNavigationGroupHolder.ObserveFocusChange(), delegate(bool hasFocus)
			{
				if (!hasFocus)
				{
					base.Hide();
				}
			});
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this._inputCancelDownDisposable != null)
			{
				this._inputCancelDownDisposable.Dispose();
				this._inputCancelDownDisposable = null;
			}
			if (this._focusChangeDisposable != null)
			{
				this._focusChangeDisposable.Dispose();
				this._focusChangeDisposable = null;
			}
		}

		protected override void DestroyDropdownList(GameObject dropdownList)
		{
			base.DestroyDropdownList(dropdownList);
			base.Hide();
		}

		[SerializeField]
		private bool _enableUiNavigation;

		[Inject]
		private DiContainer _diContainer;

		private IDisposable _inputCancelDownDisposable;

		private IDisposable _focusChangeDisposable;
	}
}
