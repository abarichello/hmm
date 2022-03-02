using System;
using System.Collections.Generic;
using Hoplon.Input.UiNavigation;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class SharedPreGameGui : MonoBehaviour
	{
		private IUiNavigationGroupHolder WaitWindowUiNavigationGroupHolder
		{
			get
			{
				return this._waitWindowUiNavigationGroupHolder;
			}
		}

		public void ShowWaitingWindow(Type owner)
		{
			this.ShowWaitingWindow(true, owner);
		}

		public void ShowWaitingWindow(bool showLabel, Type owner)
		{
			this._waitingWindowOwners.Add(owner);
			this.waitWindow.SetActive(true);
			this.WaitWindowLabelGameObject.SetActive(showLabel);
			SharedPreGameGui.Log.DebugFormat("Show Wainting Windown. Owner: {0}, Owners Count: {1}", new object[]
			{
				owner,
				this._waitingWindowOwners.Count
			});
			this.WaitWindowUiNavigationGroupHolder.AddHighPriorityGroup();
		}

		public void HideWaitingWindow(Type owner)
		{
			if (this._waitingWindowOwners.Contains(owner))
			{
				this._waitingWindowOwners.Remove(owner);
				if (this._waitingWindowOwners.Count == 0)
				{
					this.waitWindow.SetActive(false);
					SharedPreGameGui.Log.DebugFormat("Hide Wainting Windown. Last Owner: {0}", new object[]
					{
						owner
					});
					this.WaitWindowUiNavigationGroupHolder.RemoveHighPriorityGroup();
				}
				else
				{
					SharedPreGameGui.Log.DebugFormat("Remove Wainting Windown Owner . Owners remainds: {0}", new object[]
					{
						this._waitingWindowOwners.Count
					});
				}
			}
			else
			{
				SharedPreGameGui.Log.DebugFormat("Owner not found. Wainting Windown Owners Count: {0}, How try hide: {1}", new object[]
				{
					this._waitingWindowOwners.Count,
					owner
				});
			}
		}

		public void ForceCloseWaitingWindow()
		{
			this.waitWindow.SetActive(false);
			this._waitingWindowOwners.Clear();
			SharedPreGameGui.Log.Debug("Force Close Wainting Windown");
			this.WaitWindowUiNavigationGroupHolder.RemoveHighPriorityGroup();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(SharedPreGameGui));

		public ItemBuyWindow ItemBuyWindow;

		[SerializeField]
		private GameObject waitWindow;

		[SerializeField]
		private GameObject WaitWindowLabelGameObject;

		[SerializeField]
		private UiNavigationGroupHolder _waitWindowUiNavigationGroupHolder;

		private readonly HashSet<Type> _waitingWindowOwners = new HashSet<Type>();
	}
}
