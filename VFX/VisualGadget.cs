using System;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public abstract class VisualGadget : GameHubBehaviour
	{
		public abstract void Fire(float homingTime);

		public Transform[] Slots;

		public Transform Target;

		public Identifiable TargetIdentifiable;

		public GadgetSlot targetSlot;

		public Vector3 RelativePosition;
	}
}
