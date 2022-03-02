using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Operation/FloatOperation")]
	public class FloatOperationParameter : TypedParameter<float>
	{
		protected override void Initialize()
		{
			base.Initialize();
			this._firstParameter.OnParameterInitialized += this.ParameterInitialized;
			this._secondParameter.OnParameterInitialized += this.ParameterInitialized;
			this.ParameterInitialized();
		}

		private void ParameterInitialized()
		{
			if (this._firstParameter.ParameterTomate == null || this._secondParameter.ParameterTomate == null)
			{
				return;
			}
			this._parameter = new OperationParameterTomate<float, float, float>((IParameterTomate<float>)this._firstParameter.ParameterTomate, (IParameterTomate<float>)this._secondParameter.ParameterTomate, FloatOperationParameter.Operations[this._operation]);
			base.CallOnParameterInitialized();
		}

		private static float Addition(float a, float b)
		{
			return a + b;
		}

		private static float Subtraction(float a, float b)
		{
			return a - b;
		}

		private static float Multiplication(float a, float b)
		{
			return a * b;
		}

		private static float Division(float a, float b)
		{
			return a / b;
		}

		protected override void Reset()
		{
			this._parameter = null;
		}

		public override IBaseParameterTomate ParameterTomate
		{
			get
			{
				return this._parameter;
			}
		}

		protected override void WriteToBitStream(object context, BitStream bs)
		{
		}

		protected override void ReadFromBitStream(object context, BitStream bs)
		{
		}

		protected override float InternalGetValue(object context)
		{
			return this._parameter.GetValue(context);
		}

		protected override void InternalSetValue(object context, float value)
		{
			this._parameter.SetValue(context, value);
		}

		protected override void InternalSetRoute(object context, Func<object, float> getter, Action<object, float> setter)
		{
			this._parameter.SetRoute(context, getter, setter);
		}

		// Note: this type is marked as 'beforefieldinit'.
		static FloatOperationParameter()
		{
			Dictionary<FloatOperationParameter.Operation, Func<float, float, float>> dictionary = new Dictionary<FloatOperationParameter.Operation, Func<float, float, float>>();
			Dictionary<FloatOperationParameter.Operation, Func<float, float, float>> dictionary2 = dictionary;
			FloatOperationParameter.Operation key = FloatOperationParameter.Operation.Addition;
			if (FloatOperationParameter.<>f__mg$cache0 == null)
			{
				FloatOperationParameter.<>f__mg$cache0 = new Func<float, float, float>(FloatOperationParameter.Addition);
			}
			dictionary2.Add(key, FloatOperationParameter.<>f__mg$cache0);
			Dictionary<FloatOperationParameter.Operation, Func<float, float, float>> dictionary3 = dictionary;
			FloatOperationParameter.Operation key2 = FloatOperationParameter.Operation.Subtraction;
			if (FloatOperationParameter.<>f__mg$cache1 == null)
			{
				FloatOperationParameter.<>f__mg$cache1 = new Func<float, float, float>(FloatOperationParameter.Subtraction);
			}
			dictionary3.Add(key2, FloatOperationParameter.<>f__mg$cache1);
			Dictionary<FloatOperationParameter.Operation, Func<float, float, float>> dictionary4 = dictionary;
			FloatOperationParameter.Operation key3 = FloatOperationParameter.Operation.Multiplication;
			if (FloatOperationParameter.<>f__mg$cache2 == null)
			{
				FloatOperationParameter.<>f__mg$cache2 = new Func<float, float, float>(FloatOperationParameter.Multiplication);
			}
			dictionary4.Add(key3, FloatOperationParameter.<>f__mg$cache2);
			Dictionary<FloatOperationParameter.Operation, Func<float, float, float>> dictionary5 = dictionary;
			FloatOperationParameter.Operation key4 = FloatOperationParameter.Operation.Division;
			if (FloatOperationParameter.<>f__mg$cache3 == null)
			{
				FloatOperationParameter.<>f__mg$cache3 = new Func<float, float, float>(FloatOperationParameter.Division);
			}
			dictionary5.Add(key4, FloatOperationParameter.<>f__mg$cache3);
			FloatOperationParameter.Operations = dictionary;
		}

		[SerializeField]
		private BaseParameter _firstParameter;

		[SerializeField]
		private FloatOperationParameter.Operation _operation;

		[SerializeField]
		private BaseParameter _secondParameter;

		private IParameterTomate<float> _parameter;

		private static readonly Dictionary<FloatOperationParameter.Operation, Func<float, float, float>> Operations;

		[CompilerGenerated]
		private static Func<float, float, float> <>f__mg$cache0;

		[CompilerGenerated]
		private static Func<float, float, float> <>f__mg$cache1;

		[CompilerGenerated]
		private static Func<float, float, float> <>f__mg$cache2;

		[CompilerGenerated]
		private static Func<float, float, float> <>f__mg$cache3;

		private enum Operation
		{
			Addition,
			Subtraction,
			Multiplication,
			Division
		}
	}
}
