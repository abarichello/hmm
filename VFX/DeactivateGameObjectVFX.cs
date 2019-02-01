using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class DeactivateGameObjectVFX : BaseVFX
	{
		public override int Priority
		{
			get
			{
				return 1000;
			}
		}

		protected override void OnActivate()
		{
			this.currentState = DeactivateGameObjectVFX.State.Activated;
			if (this.targetGameObject)
			{
				this.targetGameObject.SetActive(true);
			}
			this.CanCollectToCache = false;
		}

		private void Update()
		{
			if (this.currentState == DeactivateGameObjectVFX.State.Deactivating)
			{
				if (Time.time - this.deactivateTime < this.deactivationDelay)
				{
					return;
				}
				this.currentState = DeactivateGameObjectVFX.State.Deactivated;
				this.CanCollectToCache = true;
				this.targetGameObject.SetActive(false);
			}
		}

		protected override void WillDeactivate()
		{
			this.deactivateTime = Time.time;
			this.currentState = DeactivateGameObjectVFX.State.Deactivating;
		}

		protected override void OnDeactivate()
		{
			this.deactivateTime = Time.time;
			this.currentState = DeactivateGameObjectVFX.State.Deactivating;
		}

		public GameObject targetGameObject;

		private float deactivateTime;

		private DeactivateGameObjectVFX.State currentState;

		public float deactivationDelay;

		private enum State
		{
			Activated,
			Deactivating,
			Deactivated
		}
	}
}
