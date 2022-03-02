using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block
{
	public abstract class BaseAttachBlock : BaseBlock
	{
		protected void SetBodiesKinematicState(Transform trans, bool isKinematic)
		{
			BaseAttachBlock._resultsBodies.Clear();
			trans.GetComponentsInChildren<Rigidbody>(BaseAttachBlock._resultsBodies);
			for (int i = 0; i < BaseAttachBlock._resultsBodies.Count; i++)
			{
				BaseAttachBlock._resultsBodies[i].isKinematic = isKinematic;
			}
		}

		protected void SetCollidersEnabledState(Transform trans, bool enabled)
		{
			BaseAttachBlock._resultsCols.Clear();
			trans.GetComponentsInChildren<Collider>(BaseAttachBlock._resultsCols);
			for (int i = 0; i < BaseAttachBlock._resultsCols.Count; i++)
			{
				BaseAttachBlock._resultsCols[i].enabled = enabled;
			}
		}

		private static readonly List<Rigidbody> _resultsBodies = new List<Rigidbody>(4);

		private static readonly List<Collider> _resultsCols = new List<Collider>(32);

		[SerializeField]
		protected bool _sendToClient;
	}
}
