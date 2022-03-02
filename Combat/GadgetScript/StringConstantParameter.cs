using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public class StringConstantParameter : StringParameter
	{
		public string ConstantValue
		{
			get
			{
				return this._constantValue;
			}
			set
			{
				this._constantValue = value;
			}
		}

		protected override string InternalGetValue(object context)
		{
			return this._constantValue;
		}

		[SerializeField]
		private string _constantValue;
	}
}
