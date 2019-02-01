using System;
using Assets.Standard_Assets.Scripts.HMM.Customization;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudWinnerObject : GameHubBehaviour
	{
		public void Setup(PlayerData playerData)
		{
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
			this._playerPortraitSprite.SpriteName = HudUtils.GetHudWinnerPlayertName(GameHubBehaviour.Hub, playerData.Customizations.SelectedSkin);
			this._labelsGui.CharacterNameLabel.text = playerData.Character.LocalizedName;
			if (!playerData.IsBot && playerData.IsBotControlled)
			{
				this._labelsGui.PlayerNameLabel.text = playerData.Character.LocalizedBotName;
			}
			else
			{
				this._labelsGui.PlayerNameLabel.text = playerData.Name;
				if (!playerData.IsBot)
				{
					TeamUtils.GetUserTagAsync(GameHubBehaviour.Hub, playerData.UserId, delegate(string teamTag)
					{
						if (!string.IsNullOrEmpty(teamTag))
						{
							this._labelsGui.PlayerNameLabel.text = string.Format("{0} {1}", teamTag, playerData.Name);
						}
					}, delegate(Exception exception)
					{
						HudWinnerObject.Log.WarnFormat("Error on GetUserTagAsync. Exception:{0}", new object[]
						{
							exception
						});
					});
				}
			}
			this._carSprite.SpriteName = HudUtils.GetCarSkinSpriteName(GameHubBehaviour.Hub, playerData.Character, playerData.Customizations.SelectedSkin);
			PortraitDecoratorGui.UpdatePortraitSprite(playerData.Customizations.SelectedPortraitItemTypeId, this._playerPortraitBorder, PortraitDecoratorGui.PortraitSpriteType.LoadingVersusBox);
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
