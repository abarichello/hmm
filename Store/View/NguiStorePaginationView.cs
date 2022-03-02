using System;
using System.Collections.Generic;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Utils;
using UnityEngine;

namespace HeavyMetalMachines.Store.View
{
	public class NguiStorePaginationView : MonoBehaviour, IStorePaginationView
	{
		public IButton PreviousPageButton
		{
			get
			{
				return this._previousPageButton;
			}
		}

		public IButton NextPageButton
		{
			get
			{
				return this._nextPageButton;
			}
		}

		public List<IPageButtonToggle> PageToggles
		{
			get
			{
				return this._pageToggle;
			}
		}

		public void CreatePageToogles(int numberOfPages)
		{
			PageButtonToggle[] array;
			if (this._pageToggle.Count == 0)
			{
				ObjectPoolUtils.CreateObjectPool<PageButtonToggle>(this._togglePageButtonReference, out array, numberOfPages, null);
			}
			else
			{
				ObjectPoolUtils.ExpandObjectPool<PageButtonToggle>(this._togglePageButtonReference, out array, numberOfPages, null);
			}
			foreach (PageButtonToggle item in array)
			{
				this._pageToggle.Add(item);
			}
		}

		public void RefreshPageToggles()
		{
			this._uiGrid.Reposition();
		}

		private readonly List<IPageButtonToggle> _pageToggle = new List<IPageButtonToggle>();

		[SerializeField]
		private NGuiButton _nextPageButton;

		[SerializeField]
		private NGuiButton _previousPageButton;

		[SerializeField]
		private PageButtonToggle _togglePageButtonReference;

		[SerializeField]
		private UIGrid _uiGrid;
	}
}
