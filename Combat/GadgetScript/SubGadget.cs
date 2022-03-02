using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.GadgetScript.Block;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript
{
	[CreateAssetMenu(menuName = "GadgetScript/SubGadget")]
	public class SubGadget : ScriptableObject, IGadgetEventsListener, ISerializationCallbackReceiver
	{
		public List<KeyValuePair<IBlock, IBlock>> Events { get; private set; }

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			this.Events = new List<KeyValuePair<IBlock, IBlock>>(this._eventList.Count);
			for (int i = 0; i < this._eventList.Count; i++)
			{
				SubGadget.BlockPair blockPair = this._eventList[i];
				if (!(null == blockPair.TriggerBlock) && !(null == blockPair.ResultBlock))
				{
					KeyValuePair<IBlock, IBlock> item = new KeyValuePair<IBlock, IBlock>(blockPair.TriggerBlock, blockPair.ResultBlock);
					this.Events.Add(item);
				}
			}
		}

		[SerializeField]
		private List<SubGadget.BlockPair> _eventList;

		[Serializable]
		public struct BlockPair
		{
			public BaseBlock TriggerBlock;

			public BaseBlock ResultBlock;
		}
	}
}
