using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Operation/Vector3Operation")]
	public class Vector3OperationParameter : TypedParameter<Vector3>
	{
		protected override void Initialize()
		{
			base.Initialize();
			this._operationMethod = Vector3OperationParameter.Operations[this._operation];
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
			if (this._secondParameter.ParameterTomate is IParameterTomate<float>)
			{
				this._parameter = new OperationParameterTomate<Vector3, Vector3, float>((IParameterTomate<Vector3>)this._firstParameter.ParameterTomate, (IParameterTomate<float>)this._secondParameter.ParameterTomate, new Func<Vector3, float, Vector3>(this.ScalarOperationFunction));
			}
			else
			{
				this._parameter = new OperationParameterTomate<Vector3, Vector3, Vector3>((IParameterTomate<Vector3>)this._firstParameter.ParameterTomate, (IParameterTomate<Vector3>)this._secondParameter.ParameterTomate, new Func<Vector3, Vector3, Vector3>(this.OperationFunction));
			}
			base.CallOnParameterInitialized();
		}

		private Vector3 OperationFunction(Vector3 a, Vector3 b)
		{
			if (this._normalize)
			{
				return this._operationMethod(a, b).normalized;
			}
			return this._operationMethod(a, b);
		}

		private Vector3 ScalarOperationFunction(Vector3 a, float b)
		{
			return this.OperationFunction(a, Vector3.one * b);
		}

		private static Vector3 Addition(Vector3 a, Vector3 b)
		{
			return a + b;
		}

		private static Vector3 Subtraction(Vector3 a, Vector3 b)
		{
			return a - b;
		}

		private static Vector3 Multiplication(Vector3 a, Vector3 b)
		{
			a.Scale(b);
			return a;
		}

		private static Vector3 Division(Vector3 a, Vector3 b)
		{
			Vector3 result = default(Vector3);
			result.x = a.x / b.x;
			result.y = a.y / b.y;
			result.z = a.z / b.z;
			return result;
		}

		private static Vector3 Ricochet(Vector3 a, Vector3 b)
		{
			Vector3 normalized = b.normalized;
			Vector3 vector = Mathf.Abs(Vector3.Dot(a, normalized)) * 2f * normalized;
			return a + vector;
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

		protected override Vector3 InternalGetValue(object context)
		{
			return this._parameter.GetValue(context);
		}

		protected override void InternalSetValue(object context, Vector3 value)
		{
			this._parameter.SetValue(context, value);
		}

		protected override void InternalSetRoute(object context, Func<object, Vector3> getter, Action<object, Vector3> setter)
		{
			this._parameter.SetRoute(context, getter, setter);
		}

		// Note: this type is marked as 'beforefieldinit'.
		static Vector3OperationParameter()
		{
			Dictionary<Vector3OperationParameter.Operation, Vector3OperationParameter.OperationMethod> dictionary = new Dictionary<Vector3OperationParameter.Operation, Vector3OperationParameter.OperationMethod>();
			Dictionary<Vector3OperationParameter.Operation, Vector3OperationParameter.OperationMethod> dictionary2 = dictionary;
			Vector3OperationParameter.Operation key = Vector3OperationParameter.Operation.Addition;
			if (Vector3OperationParameter.<>f__mg$cache0 == null)
			{
				Vector3OperationParameter.<>f__mg$cache0 = new Vector3OperationParameter.OperationMethod(Vector3OperationParameter.Addition);
			}
			dictionary2.Add(key, Vector3OperationParameter.<>f__mg$cache0);
			Dictionary<Vector3OperationParameter.Operation, Vector3OperationParameter.OperationMethod> dictionary3 = dictionary;
			Vector3OperationParameter.Operation key2 = Vector3OperationParameter.Operation.Subtraction;
			if (Vector3OperationParameter.<>f__mg$cache1 == null)
			{
				Vector3OperationParameter.<>f__mg$cache1 = new Vector3OperationParameter.OperationMethod(Vector3OperationParameter.Subtraction);
			}
			dictionary3.Add(key2, Vector3OperationParameter.<>f__mg$cache1);
			Dictionary<Vector3OperationParameter.Operation, Vector3OperationParameter.OperationMethod> dictionary4 = dictionary;
			Vector3OperationParameter.Operation key3 = Vector3OperationParameter.Operation.Multiplication;
			if (Vector3OperationParameter.<>f__mg$cache2 == null)
			{
				Vector3OperationParameter.<>f__mg$cache2 = new Vector3OperationParameter.OperationMethod(Vector3OperationParameter.Multiplication);
			}
			dictionary4.Add(key3, Vector3OperationParameter.<>f__mg$cache2);
			Dictionary<Vector3OperationParameter.Operation, Vector3OperationParameter.OperationMethod> dictionary5 = dictionary;
			Vector3OperationParameter.Operation key4 = Vector3OperationParameter.Operation.Division;
			if (Vector3OperationParameter.<>f__mg$cache3 == null)
			{
				Vector3OperationParameter.<>f__mg$cache3 = new Vector3OperationParameter.OperationMethod(Vector3OperationParameter.Division);
			}
			dictionary5.Add(key4, Vector3OperationParameter.<>f__mg$cache3);
			Dictionary<Vector3OperationParameter.Operation, Vector3OperationParameter.OperationMethod> dictionary6 = dictionary;
			Vector3OperationParameter.Operation key5 = Vector3OperationParameter.Operation.Ricochet;
			if (Vector3OperationParameter.<>f__mg$cache4 == null)
			{
				Vector3OperationParameter.<>f__mg$cache4 = new Vector3OperationParameter.OperationMethod(Vector3OperationParameter.Ricochet);
			}
			dictionary6.Add(key5, Vector3OperationParameter.<>f__mg$cache4);
			Vector3OperationParameter.Operations = dictionary;
		}

		[SerializeField]
		private BaseParameter _firstParameter;

		[SerializeField]
		private Vector3OperationParameter.Operation _operation;

		[SerializeField]
		private BaseParameter _secondParameter;

		[SerializeField]
		private bool _normalize;

		private IParameterTomate<Vector3> _parameter;

		private Vector3OperationParameter.OperationMethod _operationMethod;

		private static readonly Dictionary<Vector3OperationParameter.Operation, Vector3OperationParameter.OperationMethod> Operations;

		[CompilerGenerated]
		private static Vector3OperationParameter.OperationMethod <>f__mg$cache0;

		[CompilerGenerated]
		private static Vector3OperationParameter.OperationMethod <>f__mg$cache1;

		[CompilerGenerated]
		private static Vector3OperationParameter.OperationMethod <>f__mg$cache2;

		[CompilerGenerated]
		private static Vector3OperationParameter.OperationMethod <>f__mg$cache3;

		[CompilerGenerated]
		private static Vector3OperationParameter.OperationMethod <>f__mg$cache4;

		private delegate Vector3 OperationMethod(Vector3 a, Vector3 b);

		private enum Operation
		{
			Addition,
			Subtraction,
			Multiplication,
			Division,
			Ricochet
		}
	}
}
