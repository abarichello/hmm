using System;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Operation/Vector3Operation")]
	public class Vector3OperationParameter : Vector3Parameter
	{
		protected override void OnEnable()
		{
			base.OnEnable();
			if (this._secondParameter is INumericParameter)
			{
				if (this._operation == Vector3OperationParameter.Operation.Addition || this._operation == Vector3OperationParameter.Operation.Subtraction)
				{
					BaseParameter.Log.FatalFormat("Can't add or subtract a Vector with a Scalar: {0}", new object[]
					{
						base.name
					});
				}
			}
			else
			{
				if (!(this._secondParameter is Vector3Parameter))
				{
					BaseParameter.Log.FatalFormat("Second parameter must be a Vector3 or a Numerical parameter {0}", new object[]
					{
						base.name
					});
					return;
				}
				if (this._operation == Vector3OperationParameter.Operation.Multiplication || this._operation == Vector3OperationParameter.Operation.Division)
				{
					BaseParameter.Log.FatalFormat("Can't multiply or divide a Vector with a Vector: {0}", new object[]
					{
						base.name
					});
				}
			}
			this._firstParameter.OnParameterValueUpdated += this.OnSourceUpdated;
			this._secondParameter.OnParameterValueUpdated += this.OnSourceUpdated;
		}

		private Vector3 ResultFunction(IParameterContext context)
		{
			Vector3 vector = Vector3.zero;
			switch (this._operation)
			{
			case Vector3OperationParameter.Operation.Addition:
				vector = this._firstParameter.GetValue(context) + ((Vector3Parameter)this._secondParameter).GetValue(context);
				break;
			case Vector3OperationParameter.Operation.Subtraction:
				vector = this._firstParameter.GetValue(context) - ((Vector3Parameter)this._secondParameter).GetValue(context);
				break;
			case Vector3OperationParameter.Operation.Multiplication:
				vector = this._firstParameter.GetValue(context) * ((INumericParameter)this._secondParameter).GetFloatValue(context);
				break;
			case Vector3OperationParameter.Operation.Division:
				vector = this._firstParameter.GetValue(context) / ((INumericParameter)this._secondParameter).GetFloatValue(context);
				break;
			}
			return (!this._normalize) ? vector : vector.normalized;
		}

		private void OnSourceUpdated(BaseParameter parameter, IParameterContext context)
		{
			base.SetRoute(context, () => this.ResultFunction(context), null);
		}

		protected override void WriteToBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
		}

		protected override void ReadFromBitStream(IParameterContext context, Pocketverse.BitStream bs)
		{
		}

		[SerializeField]
		private Vector3Parameter _firstParameter;

		[SerializeField]
		private Vector3OperationParameter.Operation _operation;

		[SerializeField]
		private BaseParameter _secondParameter;

		[SerializeField]
		private bool _normalize;

		private enum Operation
		{
			Addition,
			Subtraction,
			Multiplication,
			Division
		}
	}
}
