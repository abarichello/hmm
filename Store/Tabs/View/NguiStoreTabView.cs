using System;
using System.Collections.Generic;
using HeavyMetalMachines.Store.View;
using HeavyMetalMachines.Utils;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Store.Tabs.View
{
	public class NguiStoreTabView : MonoBehaviour, IStoreTabView
	{
		public IUiNavigationSubGroupHolder UiNavigationSubGroupHolder
		{
			get
			{
				return this._uiNavigationSubGroupHolder;
			}
		}

		public IUiNavigationAxisSelector UiNavigationAxisSelector
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

		public List<IStoreItemView> StoreItems
		{
			get
			{
				return this._storeItems;
			}
		}

		private void Awake()
		{
			this._storeItems.Clear();
			this.CreateItemsPool();
		}

		public void Show()
		{
			base.gameObject.SetActive(true);
			this._animator.SetBool(NguiStoreTabView.ShowAnimationId, true);
		}

		public void Hide()
		{
			base.gameObject.SetActive(false);
			this._animator.SetBool(NguiStoreTabView.ShowAnimationId, false);
		}

		public void Reposition()
		{
			this._uiGrid.Reposition();
		}

		public IObservable<Unit> AnimateShow()
		{
			throw new NotImplementedException();
		}

		public IObservable<Unit> AnimateHide()
		{
			throw new NotImplementedException();
		}

		private void CreateItemsPool()
		{
			NguiStoreItem[] array;
			ObjectPoolUtils.CreateObjectPool<NguiStoreItem>(this._nguiStoreItemReference, out array, this._numberOfItensPerPage, null);
			for (int i = 0; i < array.Length; i++)
			{
				this._storeItems.Add(array[i]);
			}
		}

		private static readonly int ShowAnimationId = Animator.StringToHash("show");

		[SerializeField]
		private NguiStoreItem _nguiStoreItemReference;

		[SerializeField]
		private Animator _animator;

		[SerializeField]
		private UIGrid _uiGrid;

		[SerializeField]
		private int _numberOfItensPerPage = 8;

		[Header("[Ui Navigation]")]
		[SerializeField]
		private UiNavigationSubGroupHolder _uiNavigationSubGroupHolder;

		[SerializeField]
		private UiNavigationAxisSelector _uiNavigationAxisSelector;

		private readonly List<IStoreItemView> _storeItems = new List<IStoreItemView>();
	}
}
