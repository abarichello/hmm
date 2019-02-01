using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class SharedPreGameGui : MonoBehaviour
	{
		public void ShowWaitingWindow(Type owner)
		{
			this.ShowWaitingWindow(true, owner);
		}

		public void ShowWaitingWindow(bool showLabel, Type owner)
		{
			this._waitingWindownOwners.Add(owner);
			this.waitWindow.SetActive(true);
			this.WaitWindowLabelGameObject.SetActive(showLabel);
		}

		public void HideWaitinWindow(Type owner)
		{
			if (this._waitingWindownOwners.Contains(owner))
			{
				this._waitingWindownOwners.Remove(owner);
				if (this._waitingWindownOwners.Count == 0)
				{
					this.waitWindow.SetActive(false);
				}
			}
		}

		public void ForceCloseWaitingWindown()
		{
			this.waitWindow.SetActive(false);
			this._waitingWindownOwners.Clear();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(SharedPreGameGui));

		public ItemBuyWindow ItemBuyWindow;

		public GameObject waitWindow;

		public GameObject WaitWindowLabelGameObject;

		private HashSet<Type> _waitingWindownOwners = new HashSet<Type>();
	}
}
