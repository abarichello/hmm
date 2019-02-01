using System;
using HeavyMetalMachines.Combat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class AttachVFX : BaseVFX
	{
		public override int Priority
		{
			get
			{
				return 2000;
			}
		}

		protected override void OnActivate()
		{
			this._attachTransform = null;
			this._synchRotationTransform = null;
			AttachVFX.AttachType attachType = this.attachType;
			if (attachType != AttachVFX.AttachType.EffectOwner)
			{
				if (attachType != AttachVFX.AttachType.EffectTarget)
				{
					if (attachType == AttachVFX.AttachType.Effect)
					{
						this._attachTransform = this._targetFXInfo.EffectTransform;
						if (this.yOffsetType != AttachVFX.YOffsetType.Disabled)
						{
							Transform transform = null;
							if (this.yOffsetType == AttachVFX.YOffsetType.RespectOwner && this._targetFXInfo.Owner)
							{
								transform = this._targetFXInfo.Owner.transform;
							}
							else if (this.yOffsetType == AttachVFX.YOffsetType.RespectTarget && this._targetFXInfo.Target)
							{
								transform = this._targetFXInfo.Target.transform;
							}
							else
							{
								AttachVFX.Log.Warn("[AttachVFX] Y Offset was enabled but I could find neither an owner nor a target!");
							}
							if (transform)
							{
								Transform dummyTransform = this.GetDummyTransform(transform, this.yOffsetDummyType, this.yOffsetCustomDummyName);
								if (dummyTransform)
								{
									transform = dummyTransform;
								}
								this._yOffset = transform.position.y;
							}
						}
					}
				}
				else if (this._targetFXInfo.Target)
				{
					this._attachTransform = this._targetFXInfo.Target.transform;
				}
			}
			else if (this._targetFXInfo.Owner)
			{
				this._attachTransform = this._targetFXInfo.Owner.transform;
			}
			if (this._attachTransform)
			{
				Transform dummyTransform2 = this.GetDummyTransform(this._attachTransform, this.dummyType, this.customDummyName);
				if (dummyTransform2)
				{
					this._attachTransform = dummyTransform2;
				}
				AttachVFX.SynchRotationType synchRotationType = this.syncRotation;
				if (synchRotationType != AttachVFX.SynchRotationType.FromAttachedToOwner)
				{
					if (synchRotationType != AttachVFX.SynchRotationType.FromAttachedToTarget)
					{
						if (synchRotationType == AttachVFX.SynchRotationType.FromAttachedToEffect)
						{
							this._synchRotationTransform = this._targetFXInfo.EffectTransform;
						}
					}
					else
					{
						this._synchRotationTransform = this._targetFXInfo.Target.transform;
					}
				}
				else
				{
					this._synchRotationTransform = this._targetFXInfo.Owner.transform;
				}
				this.SynchTransform();
			}
		}

		private Transform GetDummyTransform(Transform transform, CDummy.DummyKind dummyKind, string customDummyName = "")
		{
			if (!transform || dummyKind == CDummy.DummyKind.None)
			{
				return null;
			}
			CDummy componentInChildren = transform.GetComponentInChildren<CDummy>();
			if (!componentInChildren)
			{
				return null;
			}
			return componentInChildren.GetDummy(dummyKind, customDummyName);
		}

		protected virtual void OnDisable()
		{
			this._attachTransform = null;
			this._synchRotationTransform = null;
		}

		protected virtual void OnDestroy()
		{
			this._attachTransform = null;
			this._synchRotationTransform = null;
		}

		protected virtual void LateUpdate()
		{
			if (this._attachTransform != null && this._attachTransform.gameObject.activeInHierarchy)
			{
				this.SynchTransform();
			}
		}

		private void SynchTransform()
		{
			if (this.yOffsetType == AttachVFX.YOffsetType.Disabled)
			{
				base.transform.position = this._attachTransform.position;
			}
			else
			{
				Vector3 position = this._attachTransform.position;
				position.y = this._yOffset;
				base.transform.position = position;
			}
			if (this.syncRotation != AttachVFX.SynchRotationType.Disabled)
			{
				base.transform.rotation = ((this.syncRotation != AttachVFX.SynchRotationType.Attached) ? Quaternion.LookRotation(this._synchRotationTransform.position - this._attachTransform.position) : this._attachTransform.rotation);
			}
		}

		protected override void WillDeactivate()
		{
			if (this.detachOnDeactivate || this.attachType == AttachVFX.AttachType.Effect)
			{
				this._attachTransform = null;
				this._synchRotationTransform = null;
			}
		}

		protected override void OnDeactivate()
		{
			if (this.removeEvent != null)
			{
				base.transform.position = this.removeEvent.Origin;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(AttachVFX));

		public AttachVFX.AttachType attachType;

		public CDummy.DummyKind dummyType;

		public string customDummyName = string.Empty;

		public AttachVFX.YOffsetType yOffsetType;

		public CDummy.DummyKind yOffsetDummyType;

		public string yOffsetCustomDummyName = string.Empty;

		private float _yOffset;

		public AttachVFX.SynchRotationType syncRotation;

		private Transform _attachTransform;

		private Transform _synchRotationTransform;

		public bool detachOnDeactivate;

		public enum AttachType
		{
			EffectOwner,
			EffectTarget,
			Effect
		}

		public enum YOffsetType
		{
			Disabled,
			RespectOwner,
			RespectTarget
		}

		public enum SynchRotationType
		{
			Disabled,
			Attached,
			FromAttachedToOwner,
			FromAttachedToTarget,
			FromAttachedToEffect
		}
	}
}
