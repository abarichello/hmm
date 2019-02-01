using System;
using System.Diagnostics;
using UnityEngine;

namespace HeavyMetalMachines.Event
{
	public class WeaponAnimationEndEvent : MonoBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event WeaponAnimationEndEvent.OnExitDelegate OnWeapon1ActivatedEnd;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event WeaponAnimationEndEvent.OnExitDelegate OnWeapon2ActivatedEnd;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event WeaponAnimationEndEvent.OnExitDelegate OnWeaponUltimateActivatedEnd;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event WeaponAnimationEndEvent.OnExitDelegate OnNitroActivatedEnd;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event WeaponAnimationEndEvent.OnExitDelegate OnWeapon1ReadyEnd;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event WeaponAnimationEndEvent.OnExitDelegate OnWeapon2ReadyEnd;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event WeaponAnimationEndEvent.OnExitDelegate OnWeaponUltimateReadyEnd;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event WeaponAnimationEndEvent.OnExitDelegate OnNitroReadyEnd;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event WeaponAnimationEndEvent.OnExitDelegate OnWeapon1IdleEnd;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event WeaponAnimationEndEvent.OnExitDelegate OnWeapon2IdleEnd;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event WeaponAnimationEndEvent.OnExitDelegate OnWeaponUltimateIdleEnd;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event WeaponAnimationEndEvent.OnExitDelegate OnNitroIdleEnd;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event WeaponAnimationEndEvent.OnExitDelegate OnIdleEnd;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event WeaponAnimationEndEvent.OnExitDelegate OnDeathEnd;

		public virtual void Weapon1ActivatedEnd()
		{
			if (this.OnWeapon1ActivatedEnd != null)
			{
				this.OnWeapon1ActivatedEnd();
			}
		}

		public virtual void Weapon2ActivatedEnd()
		{
			if (this.OnWeapon2ActivatedEnd != null)
			{
				this.OnWeapon2ActivatedEnd();
			}
		}

		public virtual void WeaponUltimateActivatedEnd()
		{
			if (this.OnWeaponUltimateActivatedEnd != null)
			{
				this.OnWeaponUltimateActivatedEnd();
			}
		}

		public virtual void WeaponNitroActivatedEnd()
		{
			if (this.OnNitroActivatedEnd != null)
			{
				this.OnNitroActivatedEnd();
			}
		}

		public virtual void Weapon1ReadyEnd()
		{
			if (this.OnWeapon1ReadyEnd != null)
			{
				this.OnWeapon1ReadyEnd();
			}
		}

		public virtual void Weapon2ReadyEnd()
		{
			if (this.OnWeapon2ReadyEnd != null)
			{
				this.OnWeapon2ReadyEnd();
			}
		}

		public virtual void WeaponUltimateReadyEnd()
		{
			if (this.OnWeaponUltimateReadyEnd != null)
			{
				this.OnWeaponUltimateReadyEnd();
			}
		}

		public virtual void WeaponNitroReadyEnd()
		{
			if (this.OnNitroReadyEnd != null)
			{
				this.OnNitroReadyEnd();
			}
		}

		public virtual void Weapon1IdleEnd()
		{
			if (this.OnWeapon1IdleEnd != null)
			{
				this.OnWeapon1IdleEnd();
			}
		}

		public virtual void Weapon2IdleEnd()
		{
			if (this.OnWeapon2IdleEnd != null)
			{
				this.OnWeapon2IdleEnd();
			}
		}

		public virtual void WeaponUltimateIdleEnd()
		{
			if (this.OnWeaponUltimateIdleEnd != null)
			{
				this.OnWeaponUltimateIdleEnd();
			}
		}

		public virtual void WeaponNitroIdleEnd()
		{
			if (this.OnNitroIdleEnd != null)
			{
				this.OnNitroIdleEnd();
			}
		}

		public virtual void WeaponIdleEnd()
		{
			if (this.OnIdleEnd != null)
			{
				this.OnIdleEnd();
			}
		}

		public virtual void WeaponDeathEnd()
		{
			if (this.OnDeathEnd != null)
			{
				this.OnDeathEnd();
			}
		}

		public delegate void OnExitDelegate();
	}
}
