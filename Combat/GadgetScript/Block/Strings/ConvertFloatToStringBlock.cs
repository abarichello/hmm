using System;
using Hoplon.Assertions;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block.Strings
{
	public class ConvertFloatToStringBlock : BaseBlock
	{
		private void OnValidate()
		{
			if (this._readFrom == null)
			{
				return;
			}
			if (this._readFrom is TypedParameter<float>)
			{
				return;
			}
			this._readFrom = null;
			Debug.LogError("Read From should be a parameter of float type.");
		}

		public override IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			TypedParameter<float> typedParameter = this._readFrom as TypedParameter<float>;
			Assert.IsNotNull<TypedParameter<float>>(typedParameter, "Read From should be a parameter of float type.");
			float value = typedParameter.GetValue(gadgetContext);
			string value2 = this._format.GetValue(gadgetContext);
			this._writeTo.SetValue(gadgetContext, value.ToString(value2));
			return this._nextBlock;
		}

		[Header("Configuration")]
		[Restrict(true, new Type[]
		{

		})]
		[SerializeField]
		private BaseParameter _readFrom;

		[Restrict(true, new Type[]
		{

		})]
		[SerializeField]
		private StringParameter _writeTo;

		[SerializeField]
		private StringParameter _format;
	}
}
