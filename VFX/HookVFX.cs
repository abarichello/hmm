using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Render;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class HookVFX : BaseVFX
	{
		private void Awake()
		{
			this._target = null;
			this._chain = null;
			this._hookOrigPos = this.HookObject.localPosition;
			this._hookOrigRot = this.HookObject.localRotation;
			if (this.WarningPrefab)
			{
				GameHubBehaviour.Hub.Resources.PrefabPreCache(this.WarningPrefab, 1);
			}
		}

		private void LateUpdate()
		{
			if (this._chain == null || this._target == null)
			{
				return;
			}
			switch (this.Stage)
			{
			case HookVFX.HookStage.Go:
			case HookVFX.HookStage.Stay:
				if (this.WarningPrefab && this._warningPrefabInst == null && this._lifeTime - this._time < this.WarningTime)
				{
					this._warningPrefabInst = (MasterVFX)GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(this.WarningPrefab, base.transform.position, base.transform.rotation);
					this._warningPrefabInst.transform.parent = GameHubBehaviour.Hub.Drawer.Effects;
					this._warningPrefabInst.baseMasterVFX = this.WarningPrefab;
					this._warningPrefabInst.Activate(this._targetFXInfo.Owner, this._targetFXInfo.Owner, this._targetFXInfo.EffectTransform);
				}
				this._time += Time.deltaTime;
				break;
			}
			base.transform.position = this._target.position;
			base.transform.rotation = this._target.rotation;
		}

		protected override void OnActivate()
		{
			this._chain = this._targetFXInfo.Owner.GetComponentInChildren<ChainController>();
			if (this._chain == null)
			{
				return;
			}
			switch (this.Stage)
			{
			case HookVFX.HookStage.Go:
			case HookVFX.HookStage.Fail:
				this._target = this._targetFXInfo.EffectTransform;
				break;
			case HookVFX.HookStage.Stay:
			case HookVFX.HookStage.Back:
			{
				DirtDevilStayingHookInfo dirtDevilStayingHookInfo = this._targetFXInfo.Gadget.Info as DirtDevilStayingHookInfo;
				PlayerData playerData = null;
				if (this._targetFXInfo.Target && this._targetFXInfo.Owner != this._targetFXInfo.Target)
				{
					playerData = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Target.ObjId);
					if (playerData != null)
					{
						this._target = this._targetFXInfo.Target.transform;
						this._lifeTime = dirtDevilStayingHookInfo.HookStayLifeTime;
						Bounds bounds = playerData.CharacterInstance.GetComponent<Collider>().bounds;
						float d = Mathf.Max(bounds.size.x, bounds.size.z) * 0.5f;
						Vector3 position = this._targetFXInfo.Owner.transform.position;
						Vector3 vector = this._target.position;
						Vector3 vector2 = vector - position;
						vector2.y = 0f;
						vector2.Normalize();
						vector -= vector2 * d;
						vector.y = bounds.center.y;
						this.HookObject.localPosition = this._target.InverseTransformPoint(vector);
						this.HookObject.localRotation = Quaternion.LookRotation(this._target.InverseTransformDirection(vector2));
					}
				}
				if (playerData == null)
				{
					this._lifeTime = dirtDevilStayingHookInfo.HookStayWallLifeTime;
					this._target = this._targetFXInfo.EffectTransform;
				}
				this._hasUpgrade = (this._targetFXInfo.Gadget.GetLevel(this.ChainUpgradeName) > 0);
				break;
			}
			}
			if (this.Stage == HookVFX.HookStage.Stay && this._targetFXInfo.Owner.IsOwner && UIGadgetConstructor.TryToGetUiGadgetConstructor(out this._uiGadgetConstructor))
			{
				this._uiGadgetConstructor.OnVfxActivated(this._targetFXInfo.Gadget.Slot, this._lifeTime);
			}
			this._chain.Attach(this.ChainDummy, this.Stage, this._hasUpgrade);
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
			if (this._chain)
			{
				this._chain.Dettach(this.Stage, this.TEMP_ResetChainOnDestroy, this._hasUpgrade);
			}
			if (this._warningPrefabInst)
			{
				this._warningPrefabInst.Destroy(BaseFX.EDestroyReason.Default);
			}
			this._target = null;
			this._warningPrefabInst = null;
			this._time = 0f;
			this.HookObject.localPosition = this._hookOrigPos;
			this.HookObject.localRotation = this._hookOrigRot;
			if (this._uiGadgetConstructor != null)
			{
				this._uiGadgetConstructor.OnVfxDeactivated(this._targetFXInfo.Gadget.Slot);
			}
			this._uiGadgetConstructor = null;
		}

		public HookVFX.HookStage Stage;

		public Transform HookObject;

		public Transform ChainDummy;

		public MasterVFX WarningPrefab;

		public float WarningTime = 2f;

		public string ChainUpgradeName;

		public bool TEMP_ResetChainOnDestroy;

		private Transform _target;

		private ChainController _chain;

		private MasterVFX _warningPrefabInst;

		private float _lifeTime;

		private float _time;

		private bool _hasUpgrade;

		private UIGadgetConstructor _uiGadgetConstructor;

		[NonSerialized]
		private Vector3 _hookOrigPos;

		[NonSerialized]
		private Quaternion _hookOrigRot;

		public enum HookStage
		{
			Go,
			Stay,
			Back,
			Fail
		}
	}
}
