using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class SyncCaterpillarTrackAnimation : GameHubBehaviour
	{
		private void Start()
		{
			if (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				base.enabled = false;
				return;
			}
			this.carWheelController = base.GetComponent<CarWheelsController>();
		}

		private void LateUpdate()
		{
			this.trackSpeed = Mathf.Abs(this.carWheelController.velocity);
			this.animator.SetFloat("tracks_speed", this.trackSpeed);
		}

		public Animator animator;

		private CarWheelsController carWheelController;

		private float trackSpeed;
	}
}
