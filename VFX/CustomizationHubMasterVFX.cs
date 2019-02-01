using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using Assets.Customization;
using Commons.Swordfish.Battlepass;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using Pocketverse;
using SharedUtils.Loading;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[DisallowMultipleComponent]
	public class CustomizationHubMasterVFX : MasterVFX
	{
		public override MasterVFX Activate(Identifiable owner, Identifiable target, Transform effect)
		{
			Identifiable identifiable = (this._actualOwner != CustomizationHubMasterVFX.OwnerTarget.Owner) ? target : owner;
			PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(identifiable.ObjId);
			ItemTypeScriptableObject itemTypeScriptableObjectBySlot = GameHubBehaviour.Hub.CustomizationAssets.GetItemTypeScriptableObjectBySlot(this._itemSlot, playerOrBotsByObjectId.Customizations);
			VfxItemTypeComponent component = itemTypeScriptableObjectBySlot.GetComponent<VfxItemTypeComponent>();
			if (component == null)
			{
				CustomizationHubMasterVFX.Log.Error(string.Format("ItemType does not have a VfxComponent: {0}", itemTypeScriptableObjectBySlot.name));
				return this;
			}
			string prefabNameAt = component.GetPrefabNameAt(this._subSlot);
			ResourcesContent.Content asset = LoadingManager.ResourceContent.GetAsset(prefabNameAt);
			Component component2 = asset.Asset as Component;
			Component component3 = GameHubBehaviour.Hub.Resources.PrefabCacheInstantiate(component2, effect.position, effect.rotation);
			MasterVFX masterVFX = component3.GetComponent<MasterVFX>();
			masterVFX.transform.parent = GameHubBehaviour.Hub.Drawer.Effects;
			masterVFX.baseMasterVFX = component2.GetComponent<MasterVFX>();
			masterVFX = masterVFX.Activate(this.TargetFX);
			this._targetFXInfo.Owner = owner;
			this._targetFXInfo.Target = target;
			this._targetFXInfo.EffectTransform = base.transform;
			this.shouldDeactivate = false;
			this.currentState = MasterVFX.State.Activated;
			base.Destroy(BaseFX.EDestroyReason.Default);
			return masterVFX;
		}

		public void OnValidate()
		{
			if (!CustomizationAssetsScriptableObject.SlotIsVFX(this._itemSlot))
			{
				this._itemSlot = PlayerCustomizationSlot.TakeOffVFX;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(CustomizationHubMasterVFX));

		[SerializeField]
		private CustomizationHubMasterVFX.OwnerTarget _actualOwner;

		[SerializeField]
		private PlayerCustomizationSlot _itemSlot = PlayerCustomizationSlot.TakeOffVFX;

		[SerializeField]
		private int _subSlot;

		public enum OwnerTarget
		{
			Owner,
			Target
		}
	}
}
