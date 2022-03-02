using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class HudFeedController<T> : GameHubBehaviour where T : HudFeedObject<T>.HudFeedData
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected event Action OnFeedAnimationStarted;

		public virtual void Awake()
		{
			HudFeedObject<T> component = this.HudFeedObjectReference.GetComponent<HudFeedObject<T>>();
			ObjectPoolUtils.CreateInjectedObjectPool<HudFeedObject<T>>(component, out this._hudFeedObjects, this.FeedMaxSize, this._container, 1, null);
			for (int i = 0; i < this._hudFeedObjects.Length; i++)
			{
				this._hudFeedObjects[i].gameObject.SetActive(true);
				RectTransform rectTransform = this._hudFeedObjects[i].transform as RectTransform;
				Vector2 anchoredPosition = rectTransform.anchoredPosition;
				anchoredPosition.x = this.HudFeedObjectReferenceOffset.x;
				anchoredPosition.y = this.HudFeedObjectReferenceOffset.y + (float)(this.VerticalOffset * i);
				rectTransform.anchoredPosition = anchoredPosition;
			}
			this._feedStack = new Stack<T>(this.FeedMaxSize);
		}

		public virtual void Update()
		{
			if (Time.frameCount % 2 == 0)
			{
				return;
			}
			if (this._feedStack.Count > 0 && !this._hudFeedObjects[0].InAnimation.isPlaying)
			{
				this.InsertAndAnimateKillfeedObjects(this._feedStack.Pop());
			}
			if (this.HideFeedObject)
			{
				for (int i = 0; i < this._hudFeedObjects.Length; i++)
				{
					this._hudFeedObjects[i].FeedUpdate(this.HideFeedObjectTimeSec);
				}
			}
		}

		public void PushToFeed(T data)
		{
			this._feedStack.Push(data);
		}

		private void InsertAndAnimateKillfeedObjects(T killfeedGuiData)
		{
			for (int i = this._hudFeedObjects.Length - 1; i > 0; i--)
			{
				this._hudFeedObjects[i].Setup(this._hudFeedObjects[i - 1].Data);
				this._hudFeedObjects[i].AnimateDown();
			}
			this._hudFeedObjects[0].Setup(killfeedGuiData);
			this._hudFeedObjects[0].AnimateIn();
			if (this.OnFeedAnimationStarted != null)
			{
				this.OnFeedAnimationStarted();
			}
		}

		private void OnDestroy()
		{
			this._hudFeedObjects = null;
			this._feedStack.Clear();
			this._feedStack = null;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HudFeedController<T>));

		[Inject]
		private DiContainer _container;

		public int VerticalOffset = -50;

		public int FeedMaxSize;

		public bool HideFeedObject;

		[Range(1f, 10f)]
		public float HideFeedObjectTimeSec = 5f;

		public GameObject HudFeedObjectReference;

		public Vector2 HudFeedObjectReferenceOffset;

		protected HudFeedObject<T>[] _hudFeedObjects;

		private Stack<T> _feedStack;
	}
}
