using System;
using HeavyMetalMachines.Combat;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class LineVFX : BaseVFX
	{
		protected virtual void Awake()
		{
			this._line = base.gameObject.GetComponent<LineRenderer>();
			if (this._line == null)
			{
				this._line = base.gameObject.AddComponent<LineRenderer>();
			}
			this._materialInstance = UnityEngine.Object.Instantiate<Material>(this.material);
			this._line.material = this._materialInstance;
			this._line.SetVertexCount(2);
			this._line.useWorldSpace = true;
			this.materialYScale = this._line.material.mainTextureScale.y;
		}

		protected override void OnActivate()
		{
			this.CanCollectToCache = false;
			this._active = true;
			this._transform = this._targetFXInfo.EffectTransform;
			if (this._targetFXInfo.Owner)
			{
				this._owner = this.GetDummy(this._targetFXInfo.Owner.transform, this.dummyType, this.customDummyName);
			}
			if (this.kind == LineVFX.ELineKind.OwnerToSelf)
			{
				this._target = base.transform;
			}
			else if (this._targetFXInfo.Target)
			{
				this._target = this._targetFXInfo.Target.transform;
			}
			this._target = this.GetDummy(this._target, this.targetDummyType, this.targetCustomDummyName);
			this.SetLineMaterial();
			this._line.SetPosition(0, Vector3.zero);
			this._line.SetPosition(1, Vector3.zero);
			this._line.SetWidth(this.lineWidth, this.lineWidth);
			this.currentMaterialTime = (this.currentMaterialYOffset = 0f);
			this._line.enabled = true;
			this.UpdateSourceAndTarget();
		}

		private Transform GetDummy(Transform root, CDummy.DummyKind dummyKind, string customDummyName)
		{
			if (!root)
			{
				return null;
			}
			if (dummyKind == CDummy.DummyKind.None)
			{
				return root;
			}
			CDummy componentInChildren = root.GetComponentInChildren<CDummy>();
			if (!componentInChildren)
			{
				return root;
			}
			Transform dummy = componentInChildren.GetDummy(dummyKind, customDummyName);
			return (!dummy) ? root : dummy;
		}

		protected virtual void SetLineMaterial()
		{
			this._line.material = this._materialInstance;
		}

		protected override void WillDeactivate()
		{
			this.OnDeactivate();
		}

		protected override void OnDeactivate()
		{
			this.CanCollectToCache = true;
			this._active = false;
			this._line.enabled = false;
			this._transform = null;
			this._owner = null;
			this._target = null;
			this.source = Vector3.zero;
			this.target = Vector3.zero;
		}

		protected virtual void LateUpdate()
		{
			if (!this._active)
			{
				return;
			}
			this.UpdateSourceAndTarget();
			this.currentMaterialTime += Time.deltaTime * this.animationSpeed;
			if (this.currentMaterialTime > 1f)
			{
				this.currentMaterialYOffset += this.materialYScale;
				this.currentMaterialTime -= 1f;
				this.currentMaterialYOffset = ((this.currentMaterialYOffset <= 1f) ? this.currentMaterialYOffset : 0f);
			}
			float magnitude = (this.target - this.source).magnitude;
			if (this._line)
			{
				this._line.material.SetTextureScale("_MainTex", new Vector2(magnitude / this.tilingSize, this.materialYScale));
				this._line.material.SetTextureOffset("_MainTex", new Vector2(-magnitude / this.tilingSize, this.currentMaterialYOffset));
				this._line.SetPosition(0, this.source);
				this._line.SetPosition(1, this.target);
			}
		}

		protected virtual void UpdateSourceAndTarget()
		{
			switch (this.kind)
			{
			case LineVFX.ELineKind.OwnerToTarget:
				if (this._target)
				{
					this.target = this._target.position;
				}
				if (this._owner)
				{
					this.source = this._owner.position;
				}
				break;
			case LineVFX.ELineKind.OwnerToPosition:
				if (this._owner)
				{
					this.source = this._owner.position;
				}
				if (this._transform)
				{
					this.target = this._transform.position;
				}
				break;
			case LineVFX.ELineKind.SelfToTarget:
				if (!this._target)
				{
					return;
				}
				this.source = base.transform.position;
				this.target = this._target.position;
				break;
			case LineVFX.ELineKind.OwnerToSelf:
				if (!this._owner)
				{
					return;
				}
				this.source = this._owner.position;
				this.target = this._target.position;
				break;
			}
		}

		public Material material;

		public float lineWidth = 1f;

		public LineVFX.ELineKind kind;

		public CDummy.DummyKind dummyType;

		public string customDummyName = string.Empty;

		public CDummy.DummyKind targetDummyType;

		public string targetCustomDummyName = string.Empty;

		public float tilingSize = 10f;

		public float animationSpeed = 10f;

		protected Transform _target;

		protected Transform _owner;

		protected Transform _transform;

		protected LineRenderer _line;

		protected Vector3 source;

		protected Vector3 target;

		protected float materialYScale;

		protected float currentMaterialYOffset;

		protected float currentMaterialTime;

		protected bool _active;

		protected Material _materialInstance;

		public enum ELineKind
		{
			OwnerToTarget,
			OwnerToPosition,
			SelfToTarget,
			OwnerToSelf
		}
	}
}
