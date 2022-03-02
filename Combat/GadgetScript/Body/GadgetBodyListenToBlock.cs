using System;
using HeavyMetalMachines.Combat.GadgetScript.Block;
using Hoplon.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	[RequireComponent(typeof(GadgetBody))]
	public class GadgetBodyListenToBlock : MonoBehaviour
	{
		private void Awake()
		{
			this._body = base.GetComponent<GadgetBody>();
			if (null != this._listenTo && null != this._onBlockExecuted)
			{
				this._body.OnBodyInitialized += this.Initialize;
				this._body.OnBodyDestroyed += this.Destroy;
			}
		}

		private void Initialize(IGadgetBody body)
		{
			IHMMGadgetContext ihmmgadgetContext = (IHMMGadgetContext)this._body.Context;
			if (ihmmgadgetContext.IsServer || ihmmgadgetContext.IsTest)
			{
				this._body.Context.OnBlockExecutionExit += this.BlockExecuted;
			}
		}

		private void Destroy(IGadgetBody body)
		{
			this._body.Context.OnBlockExecutionExit -= this.BlockExecuted;
		}

		private void BlockExecuted(IBlock block)
		{
			if (block.Id == this._listenTo.Id)
			{
				this._body.GetEventParameters();
				GadgetEvent instance = GadgetEvent.GetInstance(this._onBlockExecuted.Id, (IHMMGadgetContext)this._body.Context);
				this._body.Context.TriggerEvent(instance);
			}
		}

		[SerializeField]
		private BaseBlock _listenTo;

		[SerializeField]
		private BaseBlock _onBlockExecuted;

		private GadgetBody _body;
	}
}
