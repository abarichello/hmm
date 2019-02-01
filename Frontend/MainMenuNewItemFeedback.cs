using System;
using HeavyMetalMachines.Customization;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenuNewItemFeedback : MonoBehaviour
	{
		private void OnEnable()
		{
			this._inventoryComponent.OnHasNewItemsStateChanged += this.OnHasNewItemsStateChanged;
			this.SetFeedbackState(this._inventoryComponent.HasNewItems);
		}

		private void OnDisable()
		{
			this._inventoryComponent.OnHasNewItemsStateChanged -= this.OnHasNewItemsStateChanged;
		}

		private void OnHasNewItemsStateChanged(bool hasNewItems)
		{
			this.SetFeedbackState(hasNewItems);
		}

		private void SetFeedbackState(bool visible)
		{
			this._newItemsFeedback.SetActive(visible);
		}

		[SerializeField]
		private CustomizationInventoryComponent _inventoryComponent;

		[SerializeField]
		private GameObject _newItemsFeedback;
	}
}
