using System;
using Assets.Standard_Assets.Scripts.HMM.Customization;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.ParentalControl.Restrictions;
using HeavyMetalMachines.Players.Presenting;
using HeavyMetalMachines.Publishing;
using HeavyMetalMachines.Publishing.Presenting;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class HudWinnerObject : GameHubBehaviour
	{
		public void Setup(PlayerData playerData, IMatchTeams teams, IGetDisplayableNickName getDisplayableNickName, ITeamNameRestriction teamNameRestriction)
		{
			this._labelsGui.PsnLabel.gameObject.SetActive(false);
			bool flag = playerData.Team == GameHubBehaviour.Hub.Players.CurrentPlayerTeam;
			bool isCurrentPlayer = playerData.IsCurrentPlayer;
			this._borderSpriteGui.BorderLeftSprite.sprite2D = ((!flag) ? this._borderSpriteGui.BorderLeftEnemySprite : this._borderSpriteGui.BorderLeftAllySprite);
			this._borderSpriteGui.BorderRightSprite.sprite2D = ((!flag) ? this._borderSpriteGui.BorderRightEnemySprite : this._borderSpriteGui.BorderRightAllySprite);
			if (isCurrentPlayer)
			{
				this._borderSpriteGui.BaseSprite.sprite2D = this._borderSpriteGui.BaseCurrentPlayerSprite;
			}
			else
			{
				this._borderSpriteGui.BaseSprite.sprite2D = ((!flag) ? this._borderSpriteGui.BaseEnemySprite : this._borderSpriteGui.BaseAllySprite);
			}
			this._labelsGui.PlayerNameLabel.color = ((!flag) ? this._labelsGui.PlayerNameEnemyLabelColor : this._labelsGui.PlayerNameAllyLabelColor);
			Guid guidBySlot = playerData.Customizations.GetGuidBySlot(59);
			this._playerPortraitSprite.SpriteName = HudUtils.GetHudWinnerPlayertName(GameHubBehaviour.Hub, guidBySlot);
			this._labelsGui.CharacterNameLabel.text = playerData.GetCharacterLocalizedName();
			if (!playerData.IsBot && playerData.IsBotControlled)
			{
				this._labelsGui.PlayerNameLabel.text = playerData.GetCharacterBotLocalizedName();
			}
			else
			{
				string text = (!playerData.IsBot) ? getDisplayableNickName.GetFormattedNickName(playerData.PlayerId, playerData.Name) : playerData.Name;
				this._labelsGui.PlayerNameLabel.text = text;
				if (!playerData.IsBot)
				{
					this.SetupPsnInfo(playerData);
				}
			}
			this._carSprite.SpriteName = HudUtils.GetCarSkinSpriteName(GameHubBehaviour.Hub.InventoryColletion, playerData.CharacterItemType.Id, guidBySlot);
			Guid guidBySlot2 = playerData.Customizations.GetGuidBySlot(60);
			PortraitDecoratorGui.UpdatePortraitSprite(guidBySlot2, this._playerPortraitBorder, PortraitDecoratorGui.PortraitSpriteType.LoadingVersusBox);
		}

		private void SetupPsnInfo(PlayerData playerData)
		{
			Publisher publisherById = Publishers.GetPublisherById(playerData.PublisherId);
			PublisherPresentingData publisherPresentingData = this._getPublisherPresentingData.Get(publisherById);
			if (publisherPresentingData.ShouldShowPublisherUserName)
			{
				this._labelsGui.PsnLabel.gameObject.SetActive(true);
				this._labelsGui.PsnLabel.text = playerData.PublisherUserName;
			}
			else
			{
				this._labelsGui.PsnLabel.gameObject.SetActive(false);
			}
		}

		public void PlayAnimationOpen()
		{
			this._animationGui.CardAnimation.Play(this._animationGui.AnimationOpenName);
		}

		public void PlayAnimationClose()
		{
			this._animationGui.CardAnimation.Play(this._animationGui.AnimationCloseName);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HudWinnerObject));

		[SerializeField]
		private HMMUI2DDynamicSprite _playerPortraitSprite;

		[SerializeField]
		private HMMUI2DDynamicSprite _carSprite;

		[SerializeField]
		private HMMUI2DDynamicSprite _playerPortraitBorder;

		[Inject]
		private IGetPublisherPresentingData _getPublisherPresentingData;

		[SerializeField]
		private HudWinnerObject.GuiLabels _labelsGui;

		[SerializeField]
		private HudWinnerObject.GuiAnimation _animationGui;

		[SerializeField]
		private HudWinnerObject.GuiCardBorderSprite _borderSpriteGui;

		[Serializable]
		private struct GuiLabels
		{
			public Color PlayerNameAllyLabelColor;

			public Color PlayerNameEnemyLabelColor;

			public UILabel CharacterNameLabel;

			public UILabel PlayerNameLabel;

			public UILabel PlayerTagLabel;

			public UILabel PsnLabel;
		}

		[Serializable]
		private struct GuiAnimation
		{
			public string AnimationOpenName;

			public string AnimationCloseName;

			public Animation CardAnimation;
		}

		[Serializable]
		private struct GuiCardBorderSprite
		{
			public Sprite BorderLeftAllySprite;

			public Sprite BorderLeftEnemySprite;

			public Sprite BorderRightAllySprite;

			public Sprite BorderRightEnemySprite;

			public Sprite BaseCurrentPlayerSprite;

			public Sprite BaseAllySprite;

			public Sprite BaseEnemySprite;

			public UI2DSprite BorderLeftSprite;

			public UI2DSprite BorderRightSprite;

			public UI2DSprite BaseSprite;
		}
	}
}
