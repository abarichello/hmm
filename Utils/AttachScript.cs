using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	public class AttachScript : GameHubBehaviour
	{
		public Transform Target
		{
			get
			{
				return this._target;
			}
			set
			{
				this._target = value;
				this._attachableMath.Setup(base.transform, this.Target, this.Type);
			}
		}

		protected void LateUpdate()
		{
			this._attachableMath.UpdateTransform();
		}

		private readonly AttachableMath _attachableMath = new AttachableMath();

		private Transform _target;

		public AttachableMath.ConstraintType Type;
	}
}
