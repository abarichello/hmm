using System;
using Assets.ClientApiObjects;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.ParentalControl.Restrictions;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Players.Presenting;
using HeavyMetalMachines.Publishing;
using HeavyMetalMachines.Publishing.Presenting;
using HeavyMetalMachines.VFX;
using HeavyMetalMachines.VFX.PlotKids.VoiceChat;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class PlayerSelectionConfig : GameHubBehaviour
	{
		public void SetPlayerName(PlayerData playerData, Color teamColor, IMatchTeams teams)
		{
			string text = NGUIText.EscapeSymbols(playerData.Name);
			string text2 = (!playerData.IsBot) ? this._getDisplayableNickName.GetFormattedNickNameWithPlayerTag(playerData.PlayerId, text, new long?(playerData.PlayerTag)) : text;
			this.PlayerName.Text = this.FormatPlayerName(teamColor, text2);
			if (SpectatorController.IsSpectating)
			{
				this.FounderTooltipTrigger.TooltipText = text2;
			}
		}

		public void RepositionTeamTagPlayerNameAndPsnInfo()
		{
			this.TitleGrid.Reposition();
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
			Debug.Log(string.Format("[PlayerSelectionConfig] reach the SetGridPositionForSpectator({0})", gridIndex));
		}

		public void SetupVoiceChatStatusChangerGUIButton(PlayerData playerData)
		{
			this._voiceChatStatusChangerGuiButton.Setup(playerData.ConvertToPlayer(), playerData.IsBot, playerData.Team != GameHubBehaviour.Hub.Players.CurrentPlayerData.Team);
		}

		public void Update()
		{
			PlayerData anyByAddress = GameHubBehaviour.Hub.Players.GetAnyByAddress((byte)this.PlayerAddress);
			if (this.PlayerDisconnectedGameObject != null && anyByAddress != null)
			{
				this.PlayerDisconnectedGameObject.SetActive(!anyByAddress.Connected && !anyByAddress.IsBot);
			}
		}

		public void UpdatePsnInfo(PlayerData playerData)
		{
			if (playerData.IsBot)
			{
				return;
			}
			Publisher publisherById = Publishers.GetPublisherById(playerData.PublisherId);
			PublisherPresentingData publisherPresentingData = this._getPublisherPresentingData.Get(publisherById);
			if (publisherPresentingData.ShouldShowPublisherUserName)
			{
				this.PsnIdLabel.text = playerData.PublisherUserName;
				this.PsnIdGroupGameObject.SetActive(true);
			}
			else
			{
				this.PsnIdGroupGameObject.SetActive(false);
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PlayerSelectionConfig));

		[NonSerialized]
		public IItemType CharItemType;

		[SerializeField]
		private VoiceChatStatusChangerGUIButton _voiceChatStatusChangerGuiButton;

		[NonSerialized]
		public int PlayerAddress;

		public HMMUI2DDynamicSprite CharacterIcon;

		public HMMUI2DDynamicSprite FounderBorderSprite;

		public HMMTooltipTrigger FounderTooltipTrigger;

		public HMMTooltipTrigger PlayerNameTooltipTrigger;

		public UILabel PlayerName;

		public UIGrid TitleGrid;

		public GameObject PlayerDisconnectedGameObject;

		public GameObject GridPositionGroupGameObject;

		public UILabel GridPositonLabel;

		[SerializeField]
		private GameObject PsnIdGroupGameObject;

		[SerializeField]
		private UILabel PsnIdLabel;

		[Inject]
		private IGetDisplayableNickName _getDisplayableNickName;

		[Inject]
		private ITeamNameRestriction _teamNameRestriction;

		[Inject]
		private IGetPublisherPresentingData _getPublisherPresentingData;

		private bool _labelIsClamped;
	}
}
