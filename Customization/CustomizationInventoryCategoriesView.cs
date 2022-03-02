using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Frontend;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Customization
{
	[RequireComponent(typeof(ICustomizationInventoryView))]
	public class CustomizationInventoryCategoriesView : MonoBehaviour, ICustomizationInventoryCategoriesView
	{
		protected void Awake()
		{
			this.InitializeComponents();
		}

		protected void OnEnable()
		{
			this._buttonsDataByInstanceId = new Dictionary<int, CustomizationInventoryCategoriesView.CategoryToggleData>(this._categoriesToggles.Length);
			this._categoriesWithNewItems = new List<Guid>(this._categoriesToggles.Length);
			for (int i = 0; i < this._categoriesToggles.Length; i++)
			{
				this._categoriesToggles[i].CategoryToggleId = this._categoriesToggles[i].CategoryToggle.GetInstanceID();
				this._categoriesToggles[i].NewItemImage.SetActive(false);
				this._buttonsDataByInstanceId[this._categoriesToggles[i].CategoryToggleId] = this._categoriesToggles[i];
			}
			this._inventoryComponent.OnCategoryItemSeenCountChanged += this.UpdateNewItemsMarker;
		}

		private void OnDisable()
		{
			this._inventoryComponent.OnCategoryItemSeenCountChanged -= this.UpdateNewItemsMarker;
		}

		protected void Start()
		{
			this._view.RegisterCategoriesView(this);
			for (int i = 0; i < this._categoriesToggles.Length; i++)
			{
				int newItemsCount = this._view.GetNewItemsCount(this._categoriesToggles[i].CategoryScriptableObject.Id);
				if (newItemsCount > 0)
				{
					this._categoriesToggles[i].NewItemImage.SetActive(true);
					this._categoriesWithNewItems.Add(this._categoriesToggles[i].CategoryScriptableObject.Id);
				}
				else
				{
					this._categoriesToggles[i].NewItemImage.SetActive(false);
				}
			}
			this._currentlySelectedCategoryId = Guid.Empty;
			this._categoriesToggles[0].CategoryToggle.isOn = true;
		}

		private void InitializeComponents()
		{
			this._view = base.GetComponent<CustomizationInventoryView>();
			this._buttonsDataByCategoryId = new Dictionary<Guid, CustomizationInventoryCategoriesView.CategoryToggleData>(this._categoriesToggles.Length);
			for (int i = 0; i < this._categoriesToggles.Length; i++)
			{
				if (this._categoriesToggles[i].CategoryToggle == null)
				{
					CustomizationInventoryCategoriesView.Log.WarnFormat("Null toggle assignment at index {0}", new object[]
					{
						i
					});
				}
				else
				{
					this._categoriesToggles[i].NewItemImage = this.FindChildWithName(this._categoriesToggles[i].CategoryToggle.transform, this._newItemGameObjectName);
				}
				this._buttonsDataByCategoryId[this._categoriesToggles[i].CategoryScriptableObject.Id] = this._categoriesToggles[i];
			}
		}

		public void UpdateNewItemsMarker(Guid categoryId, int newItemsCount)
		{
			CustomizationInventoryCategoriesView.CategoryToggleData categoryToggleData;
			if (this._buttonsDataByCategoryId.TryGetValue(categoryId, out categoryToggleData))
			{
				if (newItemsCount > 0)
				{
					categoryToggleData.NewItemImage.SetActive(true);
				}
				else
				{
					categoryToggleData.NewItemImage.SetActive(false);
					this._categoriesWithNewItems.Remove(categoryToggleData.CategoryScriptableObject.Id);
				}
			}
			this.UpdateMarkAllAsSeenButtonVisibility();
		}

		private void UpdateMarkAllAsSeenButtonVisibility()
		{
			this._markAllAsSeenButton.SetActive(this._categoriesWithNewItems.Count > 0);
		}

		public void OnMarkAllItemsAsSeenClick()
		{
			if (this._categoriesWithNewItems.Count == 0)
			{
				return;
			}
			this._view.MarkAllItemsAsSeen(this._categoriesWithNewItems.ToArray());
		}

		private GameObject FindChildWithName(Transform root, string gameObjectName)
		{
			Transform transform = root.Find(gameObjectName);
			if (transform)
			{
				return transform.gameObject;
			}
			Debug.LogErrorFormat("Required child object with name '{0}' not found under item {1}", new object[]
			{
				gameObjectName,
				root.gameObject.name
			});
			return null;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CustomizationInventoryCategoriesView));

		private ICustomizationInventoryView _view;

		[SerializeField]
		private string _selectedGameObjectName = "ToogleBorder";

		[SerializeField]
		private string _newItemGameObjectName = "NewIcon";

		[SerializeField]
		private CustomizationInventoryCategoriesView.CategoryToggleData[] _categoriesToggles;

		[SerializeField]
		private GameObject _markAllAsSeenButton;

		[Header("Data")]
		[SerializeField]
		private CustomizationInventoryComponent _inventoryComponent;

		private Dictionary<Guid, CustomizationInventoryCategoriesView.CategoryToggleData> _buttonsDataByCategoryId;

		private Dictionary<int, CustomizationInventoryCategoriesView.CategoryToggleData> _buttonsDataByInstanceId;

		private Guid _currentlySelectedCategoryId;

		private List<Guid> _categoriesWithNewItems;

		[Serializable]
		public class CategoryToggleData
		{
			public HmmUiToggle CategoryToggle;

			public ItemCategoryScriptableObject CategoryScriptableObject;

			[HideInInspector]
			public int CategoryToggleId;

			[HideInInspector]
			public GameObject NewItemImage;
		}
	}
}
