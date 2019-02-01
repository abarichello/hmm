using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public abstract class EscMenuScreenGui : GameHubBehaviour
	{
		public virtual void Show()
		{
			this.pivot.SetActive(true);
		}

		public virtual void Hide()
		{
			this.pivot.SetActive(false);
		}

		public abstract void ReloadCurrent();

		public abstract void ResetDefault();

		public GameObject pivot;
	}
}
