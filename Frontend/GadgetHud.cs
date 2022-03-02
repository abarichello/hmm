using System;
using HeavyMetalMachines.Combat.Gadget;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class GadgetHud : MonoBehaviour, IGadgetHud
	{
		public IGadgetHudElement GetElement(GadgetSlot slot)
		{
			switch (slot)
			{
			case GadgetSlot.CustomGadget0:
				return this._customGadget0HudElement;
			case GadgetSlot.CustomGadget1:
				return this._customGadget1HudElement;
			case GadgetSlot.CustomGadget2:
				return this._customGadget2HudElement;
			case GadgetSlot.BoostGadget:
				return this._boostGadgetHudElement;
			case GadgetSlot.PassiveGadget:
				return this._passiveGadgetHudElement;
			default:
				return null;
			}
		}

		[SerializeField]
		private GadgetHudElement _customGadget0HudElement;

		[SerializeField]
		private GadgetHudElement _customGadget1HudElement;

		[SerializeField]
		private GadgetHudElement _customGadget2HudElement;

		[SerializeField]
		private GadgetHudElement _boostGadgetHudElement;

		[SerializeField]
		private GadgetHudElement _passiveGadgetHudElement;
	}
}
