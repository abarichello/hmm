using System;
using System.Collections.Generic;
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
			this.CheckAnimators();
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
			if (this.deactivationDelay > 0f && this.triggerAnimOnDelay != string.Empty)
			{
				this.AnimTargetGameObject();
			}
		}

		private void AnimTargetGameObject()
		{
			if (this.animators.Count <= 0)
			{
				return;
			}
			foreach (Animator animator in this.animators)
			{
				animator.SetTrigger(this.triggerAnimOnDelay);
			}
		}

		public void CheckAnimators()
		{
			this.animators = new List<Animator>();
			if (this.targetGameObject.GetComponent<Animator>())
			{
				this.animators.Add(this.targetGameObject.GetComponent<Animator>());
			}
			foreach (Animator item in this.targetGameObject.GetComponentsInChildren<Animator>())
			{
				this.animators.Add(item);
			}
		}

		public GameObject targetGameObject;

		private float deactivateTime;

		private DeactivateGameObjectVFX.State currentState;

		public float deactivationDelay;

		public string triggerAnimOnDelay;

		public List<Animator> animators;

		private enum State
		{
			Activated,
			Deactivating,
			Deactivated
		}
	}
}
