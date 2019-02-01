using System;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Operation/Vector3RelativeToCombat")]
	public class Vector3RelativeToCombatParameter : Vector3Parameter
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			this._physicalParameter.OnParameterValueUpdated += this.OnSourceUpdated;
		}

		private void OnSourceUpdated(BaseParameter parameter, IParameterContext context)
		{
			base.SetRoute(context, () => Quaternion.Euler(0f, this._degrees, 0f) * (this._physicalParameter.GetValue(context).Rotation * Vector3.forward), null);
		}

		protected override void WriteToBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
		}

		protected override void ReadFromBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
		}

		[SerializeField]
		private PhysicalObjectParameter _physicalParameter;

		[SerializeField]
		private float _degrees;
	}
}
