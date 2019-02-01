using System;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Options;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class HudShopButtonController : HudWindow
	{
		private GameGui GameGui
		{
			get
			{
				GameGui result;
				if ((result = this._gameGui) == null)
				{
					result = (this._gameGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<GameGui>());
				}
				return result;
			}
		}

		public void Awake()
		{
			base.SetWindowVisibility(false);
			base.gameObject.SetActive(false);
		}

		public void Start()
		{
		}

		public override void OnDestroy()
		{
		}

		private void EventTriggerOnClick()
		{
		}

		public void Update()
		{
		}

		private void UpdateShopButtonText()
		{
			this.ShopButtonLabel.text = ControlOptions.GetTextlocalized(ControlAction.GUIOpenShop, ControlOptions.ControlActionInputType.Primary);
		}

		private void OnKeyChangedCallback(ControlAction controlAction)
		{
			if (controlAction == ControlAction.GUIOpenShop)
			{
				this.UpdateShopButtonText();
			}
		}

		private void OnKeyDefaultReset()
		{
			this.UpdateShopButtonText();
		}

		private void OnControlModeChangedCallback(CarInput.DrivingStyleKind drivingStyleKind)
		{
			this.UpdateShopButtonText();
		}

		public UILabel BalanceLabel;

		public UIButton ShopButton;

		public UILabel ShopButtonLabel;

		private EventDelegate _shopButtonClickEventDelegate;

		private GameGui _gameGui;

		private PlayerStats _playerStats;

		private int _playerBalanceCache;
	}
}
