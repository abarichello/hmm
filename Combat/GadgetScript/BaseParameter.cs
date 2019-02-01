using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public abstract class BaseParameter : GameHubScriptableObject, IContent
	{
		public abstract void SetTo(IParameterContext context, BaseParameter other);

		public abstract int CompareTo(IParameterContext context, BaseParameter other);

		public abstract void RouteContext(IParameterContext context, IParameterContext otherContext);

		public abstract void CreateRouteToObject(IParameterContext context, MethodInfo getter, MethodInfo setter, object boundToObject);

		protected abstract void Reset();

		public abstract bool IsRoutedToObject { get; }

		public void WriteToBitStreamWithContentId(IParameterContext context, Pocketverse.BitStream bs)
		{
			bs.WriteInt(this.ContentId);
			if (!this.IsRoutedToObject)
			{
				this.WriteToBitStream(context, bs);
			}
		}

		public void ReadFromBitStreamWithContentId(IParameterContext context, Pocketverse.BitStream bs)
		{
			bs.ReadInt();
			if (!this.IsRoutedToObject)
			{
				this.ReadFromBitStream(context, bs);
			}
		}

		public static BaseParameter ReadParameterFromBitStreamWithContentId(IParameterContext context, Pocketverse.BitStream bs)
		{
			int id = bs.ReadInt();
			BaseParameter parameter = BaseParameter.GetParameter(id);
			if (!parameter.IsRoutedToObject)
			{
				parameter.ReadFromBitStream(context, bs);
			}
			return parameter;
		}

		protected abstract void WriteToBitStream(IParameterContext context, Pocketverse.BitStream bs);

		protected abstract void ReadFromBitStream(IParameterContext context, Pocketverse.BitStream bs);

		public abstract IParameterContext[] GetAllSetContexts();

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BaseParameter.OnParameterValueUpdatedListener OnParameterValueUpdated;

		public static BaseParameter GetParameter(int id)
		{
			return BaseParameter._parameters[id];
		}

		public int ContentId
		{
			get
			{
				return this._parameterId;
			}
			set
			{
				this._parameterId = value;
			}
		}

		public static List<BaseParameter> GetAllRegisteredParameters()
		{
			return new List<BaseParameter>(BaseParameter._parameters.Values);
		}

		protected void CallOnParameterValueUpdated(IParameterContext context)
		{
			if (this.OnParameterValueUpdated != null)
			{
				this.OnParameterValueUpdated(this, context);
			}
		}

		protected virtual void OnEnable()
		{
			this.ResetParameters();
		}

		private void ResetParameters()
		{
			if (BaseParameter._parameters.ContainsKey(this.ContentId))
			{
				BaseParameter._parameters[this.ContentId].Reset();
			}
			else
			{
				BaseParameter._parameters[this.ContentId] = this;
			}
		}

		[ReadOnly]
		[SerializeField]
		private int _parameterId = -1;

		protected static readonly Dictionary<int, BaseParameter> _parameters = new Dictionary<int, BaseParameter>();

		protected static readonly BitLogger Log = new BitLogger(typeof(BaseParameter));

		public delegate void OnParameterValueUpdatedListener(BaseParameter parameter, IParameterContext context);
	}
}
