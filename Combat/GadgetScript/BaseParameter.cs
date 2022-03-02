using System;
using System.Collections.Generic;
using System.Diagnostics;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	public abstract class BaseParameter : GameHubScriptableObject, IContent
	{
		public abstract void SetTo(object context, BaseParameter other);

		public abstract int CompareTo(object context, BaseParameter other);

		public abstract void RouteContext(object context, object otherContext);

		protected virtual void Initialize()
		{
			this.ResetParameters();
		}

		protected abstract void Reset();

		public abstract IBaseParameterTomate ParameterTomate { get; }

		public void WriteToBitStreamWithContentId(object context, BitStream bs)
		{
			bs.WriteInt(this.ContentId);
			this.WriteToBitStream(context, bs);
		}

		public void ReadFromBitStreamWithContentId(object context, BitStream bs)
		{
			bs.ReadInt();
			this.ReadFromBitStream(context, bs);
		}

		public static BaseParameter ReadParameterFromBitStreamWithContentId(object context, BitStream bs)
		{
			int id = bs.ReadInt();
			BaseParameter parameter = BaseParameter.GetParameter(id);
			parameter.ReadFromBitStream(context, bs);
			return parameter;
		}

		protected abstract void WriteToBitStream(object context, BitStream bs);

		protected abstract void ReadFromBitStream(object context, BitStream bs);

		protected internal Dictionary<int, object> TouchedContexts
		{
			get
			{
				return this._touchedContexts;
			}
			set
			{
				this._touchedContexts = value;
			}
		}

		protected void TouchContext(object context)
		{
			if (!this._touchedContexts.ContainsKey(context.GetHashCode()))
			{
				this._touchedContexts.Add(context.GetHashCode(), context);
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BaseParameter.OnParameterValueUpdatedListener OnParameterValueUpdated;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnParameterInitialized;

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

		protected void CallOnParameterValueUpdated(object context)
		{
			if (this.OnParameterValueUpdated != null)
			{
				this.OnParameterValueUpdated(context);
			}
		}

		protected void CallOnParameterInitialized()
		{
			if (this.OnParameterInitialized != null)
			{
				this.OnParameterInitialized();
			}
		}

		private void OnEnable()
		{
			this.Initialize();
			if (this.OnParameterInitialized != null && this.ParameterTomate != null)
			{
				this.OnParameterInitialized();
			}
		}

		private void ResetParameters()
		{
			if (BaseParameter._parameters.ContainsKey(this.ContentId))
			{
				if (BaseParameter._parameters[this.ContentId].GetInstanceID() == base.GetInstanceID())
				{
					BaseParameter._parameters[this.ContentId].Reset();
					BaseParameter._parameters[this.ContentId].TouchedContexts.Clear();
				}
			}
			else
			{
				BaseParameter._parameters[this.ContentId] = this;
			}
		}

		public static void ClearRegisteredParameters()
		{
			BaseParameter._parameters.Clear();
		}

		public TValue GetValue<TValue>(object context)
		{
			this.TouchContext(context);
			return ((IParameterTomate<TValue>)this.ParameterTomate).GetValue(context);
		}

		public void SetValue<TValue>(object context, TValue value)
		{
			this.TouchContext(context);
			((IParameterTomate<TValue>)this.ParameterTomate).SetValue(context, value);
			this.CallOnParameterValueUpdated(context);
		}

		private Dictionary<int, object> _touchedContexts = new Dictionary<int, object>(8);

		[ReadOnly]
		[SerializeField]
		private int _parameterId = -1;

		protected static readonly Dictionary<int, BaseParameter> _parameters = new Dictionary<int, BaseParameter>();

		protected static readonly BitLogger Log = new BitLogger(typeof(BaseParameter));

		public delegate void OnParameterValueUpdatedListener(object context);
	}
}
