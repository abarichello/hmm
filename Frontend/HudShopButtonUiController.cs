using System;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	public class HudShopButtonUiController : GameHubBehaviour
	{
		protected void Awake()
		{
			if (!this._windowAnimation.isPlaying)
			{
				this._mainCanvas.enabled = false;
				this._isVisible = false;
			}
		}

		protected void Start()
		{
			this._isInitialized = false;
			this._playerMetalCache = 0;
			this._metalText.text = "0";
			this._playerStats = null;
		}

		protected void OnDestroy()
		{
			this._playerStats = null;
		}

		public void OnEnable()
		{
			GameHubBehaviour.Hub.Events.Players.CurrentPlayerCreatedCallback += this.CurrentPlayerCreated;
		}

		public void OnDisable()
		{
			GameHubBehaviour.Hub.Events.Players.CurrentPlayerCreatedCallback -= this.CurrentPlayerCreated;
		}

		private void CurrentPlayerCreated(PlayerEvent playerEvent)
		{
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(playerEvent.PlayerAddress);
			this._playerStats = playerByAddress.CharacterInstance.GetBitComponent<PlayerStats>();
			this._isInitialized = true;
		}

		protected void Update()
		{
			if (!this._isInitialized)
			{
				return;
			}
			if (this._playerMetalCache == this._playerStats.Scrap)
			{
				return;
			}
			this._playerMetalCache = this._playerStats.Scrap;
			this._metalText.text = this._playerMetalCache.ToString("0");
		}

		public void OpenShopButtonClick()
		{
		}

		public void TryTochangeVisibility(bool isVisible)
		{
			if (this._isVisible == isVisible)
			{
				return;
			}
			this._isVisible = isVisible;
			this._mainCanvas.enabled = !isVisible;
			GUIUtils.PlayAnimation(this._windowAnimation, !isVisible, 1f, string.Empty);
		}

		[SerializeField]
		private Canvas _mainCanvas;

		[SerializeField]
		private Text _metalText;

		[SerializeField]
		private Animation _windowAnimation;

		private PlayerStats _playerStats;

		private int _playerMetalCache;

		private bool _isInitialized;

		private bool _isVisible;
	}
}
