using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Customization;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Inventory.Tab.View
{
	public class LegacyInventoryTabView : MonoBehaviour, IPortraitInventoryTabView, IScoreInventoryTabView, IAvatarsInventoryTabView, IKillsInventoryTabView, ILoreInventoryTabView, IRespawnsInventoryTabView, ISkinsInventoryTabView, ISpraysInventoryTabView, ITakeoffInventoryTabView, IEmotesInventoryTabView, IInventoryTabView
	{
		public string NavigationName { get; private set; }

		public IToggle ToggleButton
		{
			get
			{
				return this._toggleButton;
			}
		}

		private void Awake()
		{
			string text = this._category.Id.ToString("D");
			switch (text)
			{
			case "eea8bb33-91f6-4773-b1fb-39a387dfa4d3":
				this._viewProvider.Bind<IPortraitInventoryTabView>(this, null);
				this.NavigationName = "Portraits";
				break;
			case "878f8a54-e9ae-40e8-a571-8c1366302d24":
				this._viewProvider.Bind<IScoreInventoryTabView>(this, null);
				this.NavigationName = "Scores";
				break;
			case "4e31d864-f5eb-8274-da18-601b2ba558c4":
				this._viewProvider.Bind<IAvatarsInventoryTabView>(this, null);
				this.NavigationName = "Avatars";
				break;
			case "af328b20-3796-495e-8208-e07890a3a909":
				this._viewProvider.Bind<IKillsInventoryTabView>(this, null);
				this.NavigationName = "Kills";
				break;
			case "5646bb1d-4ece-4b0b-b44a-ed5cc22250ae":
				this._viewProvider.Bind<ILoreInventoryTabView>(this, null);
				this.NavigationName = "Lores";
				break;
			case "60dd34a5-1581-4596-8519-1f10ddd04ecb":
				this._viewProvider.Bind<IRespawnsInventoryTabView>(this, null);
				this.NavigationName = "Respawns";
				break;
			case "c9fc7b17-6ef1-44a1-b27e-39c1fdc83b68":
				this._viewProvider.Bind<ISkinsInventoryTabView>(this, null);
				this.NavigationName = "Skins";
				break;
			case "0f3b8ebe-73e6-4ea6-90f0-3dd0da96771d":
				this._viewProvider.Bind<ISpraysInventoryTabView>(this, null);
				this.NavigationName = "Sprays";
				break;
			case "a91edbdf-9fc6-4282-a185-826893724742":
				this._viewProvider.Bind<ITakeoffInventoryTabView>(this, null);
				this.NavigationName = "Takeoffs";
				break;
			case "100e5ce6-f2d2-1894-18c2-37c9b507a1a6":
				this._viewProvider.Bind<IEmotesInventoryTabView>(this, null);
				this.NavigationName = "Emotes";
				break;
			}
		}

		private void OnDestroy()
		{
			string text = this._category.Id.ToString("D");
			switch (text)
			{
			case "eea8bb33-91f6-4773-b1fb-39a387dfa4d3":
				this._viewProvider.Unbind<IPortraitInventoryTabView>(null);
				break;
			case "878f8a54-e9ae-40e8-a571-8c1366302d24":
				this._viewProvider.Unbind<IScoreInventoryTabView>(null);
				break;
			case "4e31d864-f5eb-8274-da18-601b2ba558c4":
				this._viewProvider.Unbind<IAvatarsInventoryTabView>(null);
				break;
			case "af328b20-3796-495e-8208-e07890a3a909":
				this._viewProvider.Unbind<IKillsInventoryTabView>(null);
				break;
			case "5646bb1d-4ece-4b0b-b44a-ed5cc22250ae":
				this._viewProvider.Unbind<ILoreInventoryTabView>(null);
				break;
			case "60dd34a5-1581-4596-8519-1f10ddd04ecb":
				this._viewProvider.Unbind<IRespawnsInventoryTabView>(null);
				break;
			case "c9fc7b17-6ef1-44a1-b27e-39c1fdc83b68":
				this._viewProvider.Unbind<ISkinsInventoryTabView>(null);
				break;
			case "0f3b8ebe-73e6-4ea6-90f0-3dd0da96771d":
				this._viewProvider.Unbind<ISpraysInventoryTabView>(null);
				break;
			case "a91edbdf-9fc6-4282-a185-826893724742":
				this._viewProvider.Unbind<ITakeoffInventoryTabView>(null);
				break;
			case "100e5ce6-f2d2-1894-18c2-37c9b507a1a6":
				this._viewProvider.Unbind<IEmotesInventoryTabView>(null);
				break;
			}
		}

		public IObservable<Unit> Show()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._toggleButton.IsOn = true;
				this._legacyView.SetCategory(this._category.Id);
				return this._legacyView.AnimateCategoryShow();
			});
		}

		public IObservable<Unit> Hide()
		{
			return this._legacyView.AnimateCategoryHide();
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private ItemCategoryScriptableObject _category;

		[SerializeField]
		private CustomizationInventoryView _legacyView;

		[SerializeField]
		private UnityToggle _toggleButton;
	}
}
