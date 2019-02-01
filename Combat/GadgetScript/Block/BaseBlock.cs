using System;
using System.Collections.Generic;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	public abstract class BaseBlock : ScriptableObject, IBlock, IContent
	{
		public IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext)
		{
			if (!this.CheckSanity(gadgetContext, eventContext))
			{
				return null;
			}
			return this.InnerExecute(gadgetContext, eventContext);
		}

		protected abstract bool CheckSanity(IGadgetContext gadgetContext, IEventContext eventContext);

		protected abstract IBlock InnerExecute(IGadgetContext gadgetContext, IEventContext eventContext);

		public abstract bool UsesParameterWithId(int parameterId);

		public int Id
		{
			get
			{
				return this._blockId;
			}
		}

		public static IBlock GetBlock(int id)
		{
			IBlock result;
			BaseBlock._blocks.TryGetValue(id, out result);
			return result;
		}

		public int ContentId
		{
			get
			{
				return this._blockId;
			}
			set
			{
				this._blockId = value;
			}
		}

		public bool LogThisBlock
		{
			get
			{
				return !this._dontLogThisBlock;
			}
		}

		protected virtual void OnEnable()
		{
			BaseBlock._blocks[this._blockId] = this;
		}

		protected void LogSanitycheckError(string message)
		{
			BaseBlock.Log.ErrorFormat("Sanity check error detected on block: {0} - {1}", new object[]
			{
				base.name,
				message
			});
		}

		protected bool CheckIsParameterWithId(BaseParameter parameter, int id)
		{
			return parameter != null && parameter.ContentId == id;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BaseBlock));

		private static readonly Dictionary<int, IBlock> _blocks = new Dictionary<int, IBlock>();

		[ReadOnly]
		[SerializeField]
		private int _blockId;

		[SerializeField]
		private bool _dontLogThisBlock;

		[Header("Blocks")]
		[SerializeField]
		protected BaseBlock _nextBlock;
	}
}
