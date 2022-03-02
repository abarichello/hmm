using System;
using System.Collections.Generic;
using System.Linq;
using ClientAPI.Objects;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Presenting;
using UniRx;

namespace HeavyMetalMachines.Storyteller.Presenting
{
	public class UnityGameserverScroller : IEnhancedScrollerDelegate, IStorytellerGameserverConnection, IStorytellerGameserverScroller, IScroller<IStorytellerMatchInfo>
	{
		public UnityGameserverScroller(EnhancedScroller scroller, StorytellerGameserverCellView cellViewPrefab)
		{
			this._scroller = scroller;
			scroller.Delegate = this;
			this._cellViewPrefab = cellViewPrefab;
			this._onConnectToMatch = new Subject<GameServerRunningInfo>();
		}

		~UnityGameserverScroller()
		{
			if (this._onConnectToMatch != null)
			{
				this._onConnectToMatch.OnCompleted();
				this._onConnectToMatch.Dispose();
			}
		}

		public void SetItems(IEnumerable<IStorytellerMatchInfo> matchInfos)
		{
			this._matchInfos = matchInfos.ToList<IStorytellerMatchInfo>();
			this._scroller.ReloadData(0f);
		}

		public void Clear()
		{
			if (this._matchInfos != null)
			{
				this._matchInfos.Clear();
				this._scroller.ReloadData(0f);
			}
		}

		public void JumpToIndex(int index)
		{
			this._scroller.JumpToDataIndex(index, 0f, 0f, true, EnhancedScroller.TweenType.immediate, 0f, null, EnhancedScroller.LoopJumpDirectionEnum.Closest);
		}

		public IObservable<IItemView> OnViewShown { get; private set; }

		public IObservable<IItemView> OnViewHidden { get; private set; }

		public IObservable<Unit> ObserveBottomReached
		{
			get
			{
				return this._scroller.ObserveBottomReached();
			}
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			if (this._matchInfos == null)
			{
				return 0;
			}
			return this._matchInfos.Count;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return this._cellViewPrefab.GetSize();
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			StorytellerGameserverCellView storytellerGameserverCellView = (StorytellerGameserverCellView)scroller.GetCellView(this._cellViewPrefab);
			IStorytellerMatchInfo matchInfo = this._matchInfos[dataIndex];
			storytellerGameserverCellView.Initialize(matchInfo, dataIndex, new Action<int>(this.OnConnectToMatchClick));
			return storytellerGameserverCellView;
		}

		private void OnConnectToMatchClick(int index)
		{
			this._onConnectToMatch.OnNext(this._matchInfos[index].ServerInfo);
		}

		public IObservable<GameServerRunningInfo> OnConnectToMatch()
		{
			return this._onConnectToMatch;
		}

		private void NotifyDatasetChanged()
		{
			this._scroller.ReloadData(this._scroller.NormalizedScrollPosition);
		}

		public void UpdateIsLocalPlayerInQueueState(bool state)
		{
			if (this._matchInfos == null)
			{
				return;
			}
			for (int i = 0; i < this._matchInfos.Count; i++)
			{
				this._matchInfos[i].IsLocalPlayerInQueue = state;
			}
			this.NotifyDatasetChanged();
		}

		public void UpdateIsLocalPlayerInGroupState(bool state)
		{
			if (this._matchInfos == null)
			{
				return;
			}
			for (int i = 0; i < this._matchInfos.Count; i++)
			{
				this._matchInfos[i].IsLocalPlayerInGroup = state;
			}
			this.NotifyDatasetChanged();
		}

		private readonly StorytellerGameserverCellView _cellViewPrefab;

		private readonly EnhancedScroller _scroller;

		private List<IStorytellerMatchInfo> _matchInfos;

		private readonly Subject<GameServerRunningInfo> _onConnectToMatch;
	}
}
