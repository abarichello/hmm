using System;
using System.Runtime.CompilerServices;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.Items.DataTransferObjects;
using Hoplon.Unity.Loading;
using Pocketverse;

namespace HeavyMetalMachines.VFX.HACKS.Business
{
	public class SetLocalPlayerCustomization : GameHubObject, ISetLocalPlayerCustomization
	{
		public SetLocalPlayerCustomization(IMatchPlayers matchPlayers)
		{
			this._matchPlayers = matchPlayers;
		}

		public void Set(PlayerCustomizationSlot slot, Guid itemTypeId)
		{
			CustomizationContent customizations = this._matchPlayers.CurrentPlayerData.Customizations;
			customizations.SetGuidAndSlot(slot, itemTypeId);
			switch (slot)
			{
			case 1:
				break;
			case 2:
			case 3:
			case 4:
			case 5:
				this.LoadVfxItemTypeComponent(customizations, slot);
				break;
			default:
				Platform.Current.ErrorMessageBox(string.Format("Unsupported Slot {0}. Please ask your friendly SD to improve this tool", slot), "Unsupported hack");
				break;
			}
		}

		private void LoadVfxItemTypeComponent(CustomizationContent customizations, PlayerCustomizationSlot slot)
		{
			ItemTypeScriptableObject itemTypeScriptableObjectBySlot = GameHubObject.Hub.CustomizationAssets.GetItemTypeScriptableObjectBySlot(slot, customizations);
			VfxItemTypeComponent component = itemTypeScriptableObjectBySlot.GetComponent<VfxItemTypeComponent>();
			if (component == null)
			{
				return;
			}
			for (int i = 0; i < component.ArraySize; i++)
			{
				string prefabNameAt = component.GetPrefabNameAt(i);
				ResourceLoader.Instance.PreCachePrefab(prefabNameAt, 1);
			}
			ILoadingEngine engine = Loading.Engine;
			LoadingToken loadingToken = GameHubObject.Hub.State.Current.LoadingToken;
			if (SetLocalPlayerCustomization.<>f__mg$cache0 == null)
			{
				SetLocalPlayerCustomization.<>f__mg$cache0 = new ILoadingEngine.LoadCompleteCallback(SetLocalPlayerCustomization.LoadFinished);
			}
			engine.LoadToken(loadingToken, SetLocalPlayerCustomization.<>f__mg$cache0);
		}

		private static void LoadFinished(LoadingResult result)
		{
			if (!LoadStatusExtensions.IsError(result.Status))
			{
				ResourceLoader.Instance.StartCoroutine(ResourceLoader.Instance.PrepareCaches());
			}
		}

		private readonly IMatchPlayers _matchPlayers;

		[CompilerGenerated]
		private static ILoadingEngine.LoadCompleteCallback <>f__mg$cache0;
	}
}
