using System;
using System.Collections.Generic;
using System.Reflection;
using HeavyMetalMachines.Infra.Context;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	public abstract class BaseBlock : ScriptableObject, IBlock, IContent
	{
		public int Id
		{
			get
			{
				return this._blockId;
			}
		}

		public abstract IBlock Execute(IGadgetContext gadgetContext, IEventContext eventContext);

		public void Initialize(ref IList<BaseBlock> referencedBlocks, IHMMContext context)
		{
			FieldInfo[] fields = base.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				BaseBlock baseBlock = fields[i].GetValue(this) as BaseBlock;
				if (null != baseBlock)
				{
					referencedBlocks.Add(baseBlock);
				}
			}
			this.InternalInitialize(ref referencedBlocks, context);
		}

		protected virtual void InternalInitialize(ref IList<BaseBlock> referencedBlocks, IHMMContext context)
		{
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

		public string Name
		{
			get
			{
				return (!string.IsNullOrEmpty(this._name)) ? this._name : base.name;
			}
			set
			{
				this._name = value;
			}
		}

		protected virtual void OnEnable()
		{
			BaseBlock._blocks[this._blockId] = this;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BaseBlock));

		private static readonly Dictionary<int, IBlock> _blocks = new Dictionary<int, IBlock>();

		[ReadOnly]
		[SerializeField]
		private int _blockId;

		[SerializeField]
		private string _name;

		[Header("Blocks")]
		[SerializeField]
		protected BaseBlock _nextBlock;
	}
}
