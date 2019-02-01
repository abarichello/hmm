using System;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	internal class GadgetFeedbackTrackMouse : MonoBehaviour
	{
		private void Start()
		{
			this.carInput = base.GetComponentInParent<CarInput>();
			this.identity = this.target.localRotation;
			this.parent = this.target.parent;
			if (this.slot != GadgetSlot.None)
			{
				CombatObject componentInParent = base.GetComponentInParent<CombatObject>();
				if (componentInParent == null)
				{
					return;
				}
				this.gadgetState = componentInParent.Combat.GadgetStates.GetGadgetState(this.slot);
			}
		}

		private void LateUpdate()
		{
			if (!this.carInput)
			{
				return;
			}
			bool flag;
			if (this.effectStateBased)
			{
				flag = (this.enableTracking && (this.gadgetState == null || this.gadgetState.EffectState == EffectState.Idle));
			}
			else
			{
				flag = (this.enableTracking && (this.gadgetState == null || this.gadgetState.GadgetState == GadgetState.Ready));
			}
			if (this.delayCounter < this.delay)
			{
				this.delayCounter += Time.deltaTime;
				if (this.delayCounter > this.delay)
				{
					this.currentState = flag;
					this.rotation = this.target.rotation;
				}
			}
			else if (this.currentState != flag)
			{
				this.delayCounter = 0f;
				this.currentState = flag;
			}
			if (this.currentState)
			{
				this.rotation = Quaternion.RotateTowards(this.rotation, Quaternion.Euler(new Vector3(0f, this.carInput.TurretAngle, 0f)), 360f * Time.deltaTime);
			}
			else if (!this.lockRotationOnNotReady)
			{
				this.rotation = Quaternion.RotateTowards(this.rotation, (!(this.parent == null)) ? (this.parent.rotation * this.identity) : this.identity, 360f * Time.deltaTime);
			}
			this.target.rotation = this.rotation;
		}

		public GadgetSlot slot;

		public Transform target;

		public bool enableTracking;

		public bool lockRotationOnNotReady;

		public Quaternion identity;

		public float delay;

		public bool effectStateBased;

		private GadgetData.GadgetStateObject gadgetState;

		private CarInput carInput;

		private float delayCounter;

		private bool currentState;

		private Transform parent;

		private Quaternion rotation;
	}
}
