using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using Assets.Customization;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.Match;
using Hoplon.Unity.Loading;
using Pocketverse;
using SharedUtils.Loading;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Renderer))]
	public class CustomizationTextureVFX : BaseVFX, IDynamicAssetListener<Texture2D>
	{
		public override int Priority
		{
			get
			{
				return 3000;
			}
		}

		protected void Awake()
		{
			this._mainTexId = Shader.PropertyToID("_MainTex");
			this._propertyBlock = new MaterialPropertyBlock();
		}

		protected void OnDestroy()
		{
			this._propertyBlock = null;
		}

		protected override void OnActivate()
		{
			PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Owner.ObjId);
			ItemTypeScriptableObject itemTypeScriptableObjectBySlot = GameHubBehaviour.Hub.CustomizationAssets.GetItemTypeScriptableObjectBySlot(this._itemSlot, playerOrBotsByObjectId.Customizations);
			SprayItemTypeComponent component = itemTypeScriptableObjectBySlot.GetComponent<SprayItemTypeComponent>();
			if (component == null)
			{
				CustomizationTextureVFX.Log.Error(string.Format("ItemType does not have a SprayComponent: {0}", itemTypeScriptableObjectBySlot.name));
				return;
			}
			string texture = component.Texture;
			if (!Loading.TextureManager.GetAssetAsync(texture, this))
			{
				CustomizationTextureVFX.Log.ErrorFormat("Texture not found: {0}", new object[]
				{
					texture
				});
			}
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
		}

		public void OnAssetLoaded(string textureName, Texture2D texture)
		{
			this._propertyBlock.SetTexture(this._mainTexId, texture);
			this._renderer.SetPropertyBlock(this._propertyBlock);
		}

		protected void OnValidate()
		{
			this._renderer = base.GetComponent<Renderer>();
			if (!CustomizationAssetsScriptableObject.SlotIsTexture(this._itemSlot))
			{
				this._itemSlot = 1;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(CustomizationTextureVFX));

		[SerializeField]
		private PlayerCustomizationSlot _itemSlot = 1;

		[SerializeField]
		[HideInInspector]
		private Renderer _renderer;

		[NonSerialized]
		private int _mainTexId;

		[NonSerialized]
		private MaterialPropertyBlock _propertyBlock;
	}
}
