using System;
using HeavyMetalMachines.Combat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Utils.Test
{
	internal class TestPhysicsTriggerTrigger : BasePerk, IPerkWithCollision
	{
		private void Update()
		{
			base.transform.position += base.transform.forward * this.SpeedPerSecond * Time.deltaTime;
			UnityUtils.SnapToGroundPlane(base.transform, this.Offset);
			this._alreadyHittedThisFrame = false;
		}

		public int Priority()
		{
			return 0;
		}

		public void OnHit(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool barrier)
		{
			if (other.name == "Terrain_blue_test2")
			{
				return;
			}
			if (this._alreadyHittedThisFrame)
			{
				return;
			}
			this._alreadyHittedThisFrame = true;
		}

		public void OnStay(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool barrier)
		{
		}

		public void OnEnter(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool barrier)
		{
		}

		public void OnExit(Collider other, Vector3 hitPoint, Vector3 hitNormal, bool isOverlapping, bool barrier)
		{
		}

		public float Offset = 3f;

		public float SpeedPerSecond = 30f;

		private bool _alreadyHittedThisFrame;
	}
}
