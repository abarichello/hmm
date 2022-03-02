using System;
using System.Collections.Generic;
using System.Linq;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Presenting;
using UniRx;

namespace HeavyMetalMachines.Frontend.BigTextScroller
{
	public class BigTextScrollerDelegate : ITextScroller, IEnhancedScrollerDelegate, IScroller<string>
	{
		public BigTextScrollerDelegate(EnhancedScroller scroller, BigTextParagraphView viewPrefab)
		{
			this._scroller = scroller;
			this._scroller.Delegate = this;
			this._viewPrefab = viewPrefab;
			this._viewHidden = new Subject<IItemView>();
			this._viewShown = new Subject<IItemView>();
			EnhancedScroller scroller2 = this._scroller;
			scroller2.cellViewVisibilityChanged = (CellViewVisibilityChangedDelegate)Delegate.Combine(scroller2.cellViewVisibilityChanged, new CellViewVisibilityChangedDelegate(this.CellViewVisibilityChanged));
		}

		public IObservable<Unit> ObserveBottomReached
		{
			get
			{
				return this._scroller.ObserveBottomReached();
			}
		}

		public void SetText(string fullText)
		{
			string[] items = fullText.Split(this._separator, StringSplitOptions.None);
			this.SetItems(items);
		}

		public void SetItems(IEnumerable<string> allItems)
		{
			this.SetItems(allItems.ToArray<string>());
		}

		private void SetItems(string[] allItems)
		{
			this._data = allItems.ToArray<string>();
			this._scroller.ReloadData(0f);
		}

		public void Clear()
		{
			this._data = new string[0];
			this._scroller.ReloadData(0f);
		}

		public void JumpToIndex(int index)
		{
			throw new NotImplementedException();
		}

		public IObservable<IItemView> OnViewShown
		{
			get
			{
				return this._viewShown;
			}
		}

		public IObservable<IItemView> OnViewHidden
		{
			get
			{
				return this._viewHidden;
			}
		}

		private void CellViewVisibilityChanged(EnhancedScrollerCellView cellview)
		{
			if (cellview.active)
			{
				this._viewShown.OnNext(cellview as IItemView);
			}
			else
			{
				this._viewHidden.OnNext(cellview as IItemView);
			}
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return this._data.Length;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			string text = this._data[dataIndex];
			int num = text.Length / 100;
			return this._viewPrefab.Height * (float)(1 + num);
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			BigTextParagraphView bigTextParagraphView = scroller.GetCellView(this._viewPrefab) as BigTextParagraphView;
			string text = this._data[dataIndex];
			bigTextParagraphView.Label.Text = text;
			return bigTextParagraphView;
		}

		private readonly EnhancedScroller _scroller;

		private readonly string[] _separator = new string[]
		{
			Environment.NewLine,
			"\r",
			"\n"
		};

		private string[] _data = new string[0];

		private BigTextParagraphView _viewPrefab;

		private readonly Subject<IItemView> _viewShown;

		private readonly Subject<IItemView> _viewHidden;
	}
}
