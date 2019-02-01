using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class TutorialSrapConfig : GameHubBehaviour
	{
		public void OnCollisionEnter(Collision collision)
		{
			this.BoxCollider.isTrigger = true;
			this.Rigidbody.useGravity = false;
			this.Rigidbody.constraints |= RigidbodyConstraints.FreezePositionY;
		}

		public Rigidbody Rigidbody;

		public BoxCollider BoxCollider;
	}
}
