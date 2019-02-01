using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class VisualGadgetGroup : GameHubBehaviour
	{
		private void Start()
		{
			if (this.gadgetDictionary == null && this.Slots != null)
			{
				this.SetSlots(this.Slots);
			}
		}

		public void SetSlots(Transform[] slots)
		{
			this.Slots = slots;
			if (this.gadgetDictionary == null)
			{
				this.gadgetDictionary = new Dictionary<GadgetSlot, VisualGadget>(this.Gadgets.Length);
			}
			else
			{
				this.gadgetDictionary.Clear();
			}
			for (int i = 0; i < this.Gadgets.Length; i++)
			{
				VisualGadget visualGadget = this.Gadgets[i];
				visualGadget.Slots = this.Slots;
				this.gadgetDictionary[visualGadget.targetSlot] = visualGadget;
			}
		}

		public void Fire(GadgetSlot slot, int targetId, float delay, Vector3 relativePosition)
		{
			Identifiable identifiable = (targetId != -1) ? GameHubBehaviour.Hub.ObjectCollection.GetObject(targetId) : null;
			VisualGadget visualGadget;
			if (this.gadgetDictionary.TryGetValue(slot, out visualGadget))
			{
				if (identifiable)
				{
					visualGadget.Target = identifiable.transform;
					visualGadget.TargetIdentifiable = identifiable;
				}
				else
				{
					visualGadget.Target = null;
					visualGadget.TargetIdentifiable = null;
				}
				visualGadget.RelativePosition = relativePosition;
				visualGadget.Fire(delay);
			}
		}

		public Transform[] Slots;

		public VisualGadget[] Gadgets;

		private Dictionary<GadgetSlot, VisualGadget> gadgetDictionary;
	}
}
