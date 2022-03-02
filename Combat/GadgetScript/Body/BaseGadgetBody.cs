using System;
using System.Diagnostics;
using Boo.Lang;
using HeavyMetalMachines.VFX;
using Hoplon.GadgetScript;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body
{
	public abstract class BaseGadgetBody : MonoBehaviour, IGadgetBody
	{
		public int Id { get; protected set; }

		public string Name
		{
			get
			{
				return base.name;
			}
		}

		public int CreationEventId { get; protected set; }

		public bool WasSentToClient { get; set; }

		public abstract IGadgetContext Context { get; }

		public bool IsAlive { get; protected set; }

		public Vector3 Position
		{
			get
			{
				return base.transform.position;
			}
		}

		public Quaternion Rotation
		{
			get
			{
				return base.transform.rotation;
			}
		}

		public abstract Identifiable Identifiable { get; }

		public abstract Transform Transform { get; }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BodyEvent OnBodyInitialized;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event BodyEvent OnBodyDestroyed;

		protected void Initialize()
		{
			this._attachedVfxs.Clear();
		}

		public virtual void Destroy()
		{
			for (int i = 0; i < this._attachedVfxs.Count; i++)
			{
				this._attachedVfxs[i].Destroy(this._destroyReason);
			}
		}

		public void AttachVfx(MasterVFX vfx)
		{
			this._attachedVfxs.Add(vfx);
		}

		protected void RaiseBodyInitialized()
		{
			if (this.OnBodyInitialized != null)
			{
				this.OnBodyInitialized(this);
			}
		}

		protected void RaiseBodyDestroyed()
		{
			if (this.OnBodyDestroyed != null)
			{
				this.OnBodyDestroyed(this);
			}
		}

		private readonly List<MasterVFX> _attachedVfxs = new List<MasterVFX>();

		[Obsolete("Needed while VFX is not rewritten to not need the Old Gadget and Effect data.")]
		protected BaseFX.EDestroyReason _destroyReason;
	}
}
