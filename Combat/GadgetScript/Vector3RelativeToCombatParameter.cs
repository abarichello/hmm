using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Operation/Vector3RelativeToCombat")]
	public class Vector3RelativeToCombatParameter : Vector3Parameter
	{
		protected override void Initialize()
		{
			base.Initialize();
			if (this._physicalParameter != null)
			{
				this._physicalParameter.OnParameterValueUpdated += this.OnSourceUpdated;
			}
		}

		private void OnSourceUpdated(object context)
		{
			if (this._routedContexts.Contains(context))
			{
				return;
			}
			this._routedContexts.Add(context);
			if (this._degreesParameter == null)
			{
				base.SetRoute(context, (object c) => Quaternion.Euler(0f, this._degrees, 0f) * (this._physicalParameter.GetValue(c).Rotation * Vector3.forward), new Action<object, Vector3>(base.NullSet));
			}
			else
			{
				IParameterTomate<float> degreesParameter = this._degreesParameter.ParameterTomate as IParameterTomate<float>;
				base.SetRoute(context, (object c) => Quaternion.Euler(0f, degreesParameter.GetValue(c), 0f) * (this._physicalParameter.GetValue(c).Rotation * Vector3.forward), new Action<object, Vector3>(base.NullSet));
			}
		}

		protected override void WriteToBitStream(object context, BitStream bs)
		{
		}

		protected override void ReadFromBitStream(object context, BitStream bs)
		{
		}

		[SerializeField]
		private PhysicalObjectParameter _physicalParameter;

		[SerializeField]
		private float _degrees;

		[Tooltip("Optional: Use it if you want to change how many degrees in runtime")]
		[SerializeField]
		private BaseParameter _degreesParameter;

		private HashSet<object> _routedContexts = new HashSet<object>();
	}
}
