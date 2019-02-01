using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using HeavyMetalMachines.VFX.PlotKids.VoiceChat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class PlayerSelectionConfig : GameHubBehaviour
	{
		public void SetPlayerName(PlayerData playerData, Color teamColor)
		{
			string text = NGUIText.EscapeSymbols(playerData.Name);
			this._labelIsClamped = GUIUtils.ClampLabel(this.PlayerName, this.FormatPlayerName(teamColor, text));
			if (this._labelIsClamped)
			{
				this.PlayerNameTooltipTrigger.TooltipText = text;
				this.PlayerNameTooltipTrigger.enabled = true;
			}
			this.TitleGrid.Reposition();
			this.TeamTagLabel.gameObject.SetActive(false);
			TeamUtils.GetUserTagAsync(GameHubBehaviour.Hub, playerData.UserId, delegate(string teamTag)
			{
				if (!string.IsNullOrEmpty(teamTag))
				{
					this.TeamTagLabel.gameObject.SetActive(true);
					this.TeamTagLabel.text = teamTag;
					this.TitleGrid.Reposition();
				}
			}, delegate(Exception exception)
			{
				PlayerSelectionConfig.Log.Warn(string.Format("Error on GetUserTagAsync. Exception:{0}", exception));
			});
			if (SpectatorController.IsSpectating)
			{
				this.FounderTooltipTrigger.TooltipText = text;
			}
		}

		private string FormatPlayerName(Color teamColor, string playerName)
		{
			return string.Format("[{0}]{1}[-]", HudUtils.RGBToHex(teamColor), playerName);
		}

		public void SetGridPositionForSpectator(int gridIndex)
		{
			gridIndex++;
			this.GridPositionGroupGameObject.SetActive(true);
			this.GridPositonLabel.text = gridIndex.ToString();
			UnityEngine.Debug.Log(string.Format("[PlayerSelectionConfig] reach the SetGridPositionForSpectator({0})", gridIndex));
		}

		public void SetupVoiceChatStatusChangerGUIButton(PlayerData playerData)
		{
			this._voiceChatStatusChangerGuiButton.Setup(playerData.UserId, playerData.IsBot, playerData.Team != GameHubBehaviour.Hub.Players.CurrentPlayerData.Team);
		}

		public void Update()
		{
			PlayerData anyByAddress = GameHubBehaviour.Hub.Players.GetAnyByAddress((byte)this.PlayerAddress);
			if (this.PlayerDisconnectedGameObject != null && anyByAddress != null)
			{
				this.PlayerDisconnectedGameObject.SetActive(!anyByAddress.Connected && !anyByAddress.IsBot);
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PlayerSelectionConfig));

		[NonSerialized]
		public HeavyMetalMachines.Character.CharacterInfo CharInfo;

		[SerializeField]
		private VoiceChatStatusChangerGUIButton _voiceChatStatusChangerGuiButton;

		[NonSerialized]
		public int PlayerAddress;

		public HMMUI2DDynamicSprite CharacterIcon;

		public HMMUI2DDynamicSprite FounderBorderSprite;

		public HMMTooltipTrigger FounderTooltipTrigger;

		public HMMTooltipTrigger PlayerNameTooltipTrigger;

		public UILabel PlayerName;

		public UILabel TeamTagLabel;

		public UIGrid TitleGrid;

		public GameObject PlayerDisconnectedGameObject;

		public GameObject GridPositionGroupGameObject;

		public UILabel GridPositonLabel;

		private bool _labelIsClamped;
	}
}
