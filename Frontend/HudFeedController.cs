using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudFeedController<T> : GameHubBehaviour where T : HudFeedObject<T>.HudFeedData
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		protected event Action OnFeedAnimationStarted;

		public virtual void Awake()
		{
			HudFeedObject<T> component = this.HudFeedObjectReference.GetComponent<HudFeedObject<T>>();
			ObjectPoolUtils.CreateObjectPool<HudFeedObject<T>>(component, out this._hudFeedObjects, this.FeedMaxSize);
			for (int i = 0; i < this._hudFeedObjects.Length; i++)
			{
				Transform transform = this._hudFeedObjects[i].transform;
				Vector3 localPosition = transform.localPosition;
				localPosition.y = (float)(this.VerticalOffset * i);
				transform.localPosition = localPosition;
			}
			this._feedStack = new Stack<T>(this.FeedMaxSize);
		}

		public void Update()
		{
			if (this._feedStack.Count > 0 && !this._hudFeedObjects[0].InAnimation.isPlaying)
			{
				this.InsertAndAnimateKillfeedObjects(this._feedStack.Pop());
			}
			if (this.HideFeedObject)
			{
				for (int i = 0; i < this._hudFeedObjects.Length; i++)
				{
					if (this._hudFeedObjects[i].gameObject.activeInHierarchy)
					{
						this._hudFeedObjects[i].FeedUpdate(this.HideFeedObjectTimeSec);
					}
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

		public int VerticalOffset = -50;

		public int FeedMaxSize;

		public bool HideFeedObject;

		[Range(1f, 10f)]
		public float HideFeedObjectTimeSec = 5f;

		public GameObject HudFeedObjectReference;

		protected HudFeedObject<T>[] _hudFeedObjects;

		private Stack<T> _feedStack;
	}
}
