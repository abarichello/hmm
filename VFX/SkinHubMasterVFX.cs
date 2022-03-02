using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;
using UnityEngine.Serialization;

namespace HeavyMetalMachines.VFX
{
	public class SkinHubMasterVFX : MasterVFX
	{
		public GameObject GetDefaultSkinVfx
		{
			get
			{
				return this._defaultSkinVfx.gameObject;
			}
		}

		private void Awake()
		{
			if (this._defaultSkinVfx == null)
			{
				SkinHubMasterVFX.Log.FatalFormat("Default Skin VFX is null - {0}", new object[]
				{
					base.gameObject
				});
			}
			GameHubBehaviour.Hub.Resources.PrefabPreCache(this._defaultSkinVfx, 1);
			for (int i = 0; i < this._skinsVfx.Length; i++)
			{
				SkinHubMasterVFX.SkinVFXPair skinVFXPair = this._skinsVfx[i];
				if (skinVFXPair.vfx == null)
				{
					Debug.LogError(string.Format("Preload of vfx for skin: {0} FAILED - Please check if HUB: {1} has correct skin vfx associated", skinVFXPair.skinItem.Name, base.name));
				}
				GameHubBehaviour.Hub.Resources.PrefabPreCache(skinVFXPair.vfx, 1);
			}
		}

		private MasterVFX FindVFX(Guid skinGuid)
		{
			if (this._skinsVfx != null)
			{
				for (int i = 0; i < this._skinsVfx.Length; i++)
				{
					if (this._skinsVfx[i].skinItem.Id == skinGuid)
					{
						return this._skinsVfx[i].vfx;
					}
				}
			}
			return null;
		}

		public override MasterVFX Activate(Identifiable owner, Identifiable target, Transform transform)
		{
			Identifiable identifiable = (this._actualOwner != SkinHubMasterVFX.OwnerTarget.Owner) ? target : owner;
			PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(identifiable.ObjId);
			MasterVFX masterVFX = null;
			if (playerOrBotsByObjectId != null)
			{
				masterVFX = this.FindVFX(playerOrBotsByObjectId.Customizations.GetGuidBySlot(59));
			}
			if (masterVFX == null)
			{
				masterVFX = this._defaultSkinVfx;
			}
			MasterVFX masterVFX2 = (MasterVFX)GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(masterVFX, transform.position, transform.rotation);
			GameHubBehaviour.Hub.Drawer.AddEffect(masterVFX2.transform);
			masterVFX2.baseMasterVFX = masterVFX;
			masterVFX2 = masterVFX2.Activate(this.TargetFX);
			this._targetFXInfo.Owner = owner;
			this._targetFXInfo.Target = target;
			this._targetFXInfo.EffectTransform = transform;
			this.shouldDeactivate = false;
			this.currentState = MasterVFX.State.Activated;
			base.Destroy(BaseFX.EDestroyReason.Default);
			return masterVFX2;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(SkinHubMasterVFX));

		[SerializeField]
		[Tooltip("This should be used to fix cases where 'owner' is not actually the owner of the effect (e.g. ModifierFeedback)")]
		private SkinHubMasterVFX.OwnerTarget _actualOwner;

		[SerializeField]
		[FormerlySerializedAs("defaultSkinVFX")]
		private MasterVFX _defaultSkinVfx;

		[SerializeField]
		[FormerlySerializedAs("skinsVFX")]
		public SkinHubMasterVFX.SkinVFXPair[] _skinsVfx;

		[Serializable]
		public class SkinVFXPair
		{
			public ItemTypeScriptableObject skinItem;

			public MasterVFX vfx;
		}

		[Serializable]
		public enum OwnerTarget
		{
			Owner,
			Target
		}
	}
}
