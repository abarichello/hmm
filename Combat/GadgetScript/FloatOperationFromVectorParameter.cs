using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "Parameter/Operation/FloatOperationFromVector")]
	public class FloatOperationFromVectorParameter : TypedParameter<float>
	{
		protected override void Initialize()
		{
			base.Initialize();
			this._firstParameter.OnParameterInitialized += this.ParameterInitialized;
			if (FloatOperationFromVectorParameter.Operations.ContainsKey(this._operation))
			{
				this._secondParameter.OnParameterInitialized += this.ParameterInitialized;
			}
			this.ParameterInitialized();
		}

		private void ParameterInitialized()
		{
			if (this._firstParameter.ParameterTomate == null || (FloatOperationFromVectorParameter.Operations.ContainsKey(this._operation) && this._secondParameter.ParameterTomate == null))
			{
				return;
			}
			if (FloatOperationFromVectorParameter.Operations.ContainsKey(this._operation))
			{
				this._parameter = new OperationParameterTomate<float, Vector3, Vector3>((IParameterTomate<Vector3>)this._firstParameter.ParameterTomate, (IParameterTomate<Vector3>)this._secondParameter.ParameterTomate, FloatOperationFromVectorParameter.Operations[this._operation]);
				base.CallOnParameterInitialized();
				return;
			}
			this._parameter = new OperationParameterTomate<float, Vector3, Vector3>((IParameterTomate<Vector3>)this._firstParameter.ParameterTomate, FloatOperationFromVectorParameter.UnaryOperation[this._operation]);
			base.CallOnParameterInitialized();
		}

		private static float XComponent(Vector3 a)
		{
			return a.x;
		}

		private static float YComponent(Vector3 a)
		{
			return a.y;
		}

		private static float ZComponent(Vector3 a)
		{
			return a.z;
		}

		private static float Dot(Vector3 a, Vector3 b)
		{
			return Vector3.Dot(a, b);
		}

		private static float Angle(Vector3 a, Vector3 b)
		{
			return Vector3.SignedAngle(a, b, Vector3.up);
		}

		private static float Magnitude(Vector3 a)
		{
			return a.magnitude;
		}

		private static float MagnitudeSquared(Vector3 a)
		{
			return a.sqrMagnitude;
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
		static FloatOperationFromVectorParameter()
		{
			Dictionary<FloatOperationFromVectorParameter.Operation, Func<Vector3, Vector3, float>> dictionary = new Dictionary<FloatOperationFromVectorParameter.Operation, Func<Vector3, Vector3, float>>();
			Dictionary<FloatOperationFromVectorParameter.Operation, Func<Vector3, Vector3, float>> dictionary2 = dictionary;
			FloatOperationFromVectorParameter.Operation key = FloatOperationFromVectorParameter.Operation.Dot;
			if (FloatOperationFromVectorParameter.<>f__mg$cache0 == null)
			{
				FloatOperationFromVectorParameter.<>f__mg$cache0 = new Func<Vector3, Vector3, float>(FloatOperationFromVectorParameter.Dot);
			}
			dictionary2.Add(key, FloatOperationFromVectorParameter.<>f__mg$cache0);
			Dictionary<FloatOperationFromVectorParameter.Operation, Func<Vector3, Vector3, float>> dictionary3 = dictionary;
			FloatOperationFromVectorParameter.Operation key2 = FloatOperationFromVectorParameter.Operation.Angle;
			if (FloatOperationFromVectorParameter.<>f__mg$cache1 == null)
			{
				FloatOperationFromVectorParameter.<>f__mg$cache1 = new Func<Vector3, Vector3, float>(FloatOperationFromVectorParameter.Angle);
			}
			dictionary3.Add(key2, FloatOperationFromVectorParameter.<>f__mg$cache1);
			FloatOperationFromVectorParameter.Operations = dictionary;
			Dictionary<FloatOperationFromVectorParameter.Operation, Func<Vector3, float>> dictionary4 = new Dictionary<FloatOperationFromVectorParameter.Operation, Func<Vector3, float>>();
			Dictionary<FloatOperationFromVectorParameter.Operation, Func<Vector3, float>> dictionary5 = dictionary4;
			FloatOperationFromVectorParameter.Operation key3 = FloatOperationFromVectorParameter.Operation.XComponent;
			if (FloatOperationFromVectorParameter.<>f__mg$cache2 == null)
			{
				FloatOperationFromVectorParameter.<>f__mg$cache2 = new Func<Vector3, float>(FloatOperationFromVectorParameter.XComponent);
			}
			dictionary5.Add(key3, FloatOperationFromVectorParameter.<>f__mg$cache2);
			Dictionary<FloatOperationFromVectorParameter.Operation, Func<Vector3, float>> dictionary6 = dictionary4;
			FloatOperationFromVectorParameter.Operation key4 = FloatOperationFromVectorParameter.Operation.YComponent;
			if (FloatOperationFromVectorParameter.<>f__mg$cache3 == null)
			{
				FloatOperationFromVectorParameter.<>f__mg$cache3 = new Func<Vector3, float>(FloatOperationFromVectorParameter.YComponent);
			}
			dictionary6.Add(key4, FloatOperationFromVectorParameter.<>f__mg$cache3);
			Dictionary<FloatOperationFromVectorParameter.Operation, Func<Vector3, float>> dictionary7 = dictionary4;
			FloatOperationFromVectorParameter.Operation key5 = FloatOperationFromVectorParameter.Operation.ZComponent;
			if (FloatOperationFromVectorParameter.<>f__mg$cache4 == null)
			{
				FloatOperationFromVectorParameter.<>f__mg$cache4 = new Func<Vector3, float>(FloatOperationFromVectorParameter.ZComponent);
			}
			dictionary7.Add(key5, FloatOperationFromVectorParameter.<>f__mg$cache4);
			Dictionary<FloatOperationFromVectorParameter.Operation, Func<Vector3, float>> dictionary8 = dictionary4;
			FloatOperationFromVectorParameter.Operation key6 = FloatOperationFromVectorParameter.Operation.Magnitude;
			if (FloatOperationFromVectorParameter.<>f__mg$cache5 == null)
			{
				FloatOperationFromVectorParameter.<>f__mg$cache5 = new Func<Vector3, float>(FloatOperationFromVectorParameter.Magnitude);
			}
			dictionary8.Add(key6, FloatOperationFromVectorParameter.<>f__mg$cache5);
			Dictionary<FloatOperationFromVectorParameter.Operation, Func<Vector3, float>> dictionary9 = dictionary4;
			FloatOperationFromVectorParameter.Operation key7 = FloatOperationFromVectorParameter.Operation.MagnitudeSquared;
			if (FloatOperationFromVectorParameter.<>f__mg$cache6 == null)
			{
				FloatOperationFromVectorParameter.<>f__mg$cache6 = new Func<Vector3, float>(FloatOperationFromVectorParameter.MagnitudeSquared);
			}
			dictionary9.Add(key7, FloatOperationFromVectorParameter.<>f__mg$cache6);
			FloatOperationFromVectorParameter.UnaryOperation = dictionary4;
		}

		[SerializeField]
		private BaseParameter _firstParameter;

		[SerializeField]
		private FloatOperationFromVectorParameter.Operation _operation;

		[Tooltip("Needed for Dot and Angle operations")]
		[SerializeField]
		private BaseParameter _secondParameter;

		private IParameterTomate<float> _parameter;

		private static readonly Dictionary<FloatOperationFromVectorParameter.Operation, Func<Vector3, Vector3, float>> Operations;

		private static readonly Dictionary<FloatOperationFromVectorParameter.Operation, Func<Vector3, float>> UnaryOperation;

		[CompilerGenerated]
		private static Func<Vector3, Vector3, float> <>f__mg$cache0;

		[CompilerGenerated]
		private static Func<Vector3, Vector3, float> <>f__mg$cache1;

		[CompilerGenerated]
		private static Func<Vector3, float> <>f__mg$cache2;

		[CompilerGenerated]
		private static Func<Vector3, float> <>f__mg$cache3;

		[CompilerGenerated]
		private static Func<Vector3, float> <>f__mg$cache4;

		[CompilerGenerated]
		private static Func<Vector3, float> <>f__mg$cache5;

		[CompilerGenerated]
		private static Func<Vector3, float> <>f__mg$cache6;

		private enum Operation
		{
			XComponent,
			YComponent,
			ZComponent,
			Dot,
			Angle,
			Magnitude,
			MagnitudeSquared
		}
	}
}
