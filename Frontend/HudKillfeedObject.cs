using System;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudKillfeedObject : HudFeedObject<HudKillfeedObject.HudKillfeedGuiData>
	{
		public override HudKillfeedObject.HudKillfeedGuiData Data
		{
			get
			{
				return this._killfeedGuiData;
			}
			protected set
			{
				this._killfeedGuiData = value;
			}
		}

		public void OnDestroy()
		{
			this._killfeedGuiData = null;
		}

		public override void Setup(HudKillfeedObject.HudKillfeedGuiData killfeedGuiData)
		{
			this._killfeedGuiData = killfeedGuiData;
			base.gameObject.SetActive(this._killfeedGuiData != null);
			if (this._killfeedGuiData == null)
			{
				return;
			}
			bool flag = killfeedGuiData.IsSuicide();
			this.KillerGroupGameObject.SetActive(!flag);
			if (!flag)
			{
				this.SetupGuiComponents(true, killfeedGuiData.KillerPlayerData, killfeedGuiData.MaxPlayerNameChar, this.KillerIconSprite, this.KillerNameLabel, this.KillerBorderSprite, this.KillerBgSprite);
			}
			this.CenterIconSprite.sprite2D = killfeedGuiData.CenterSprite;
			this.SetupGuiComponents(false, killfeedGuiData.VictimPlayerData, killfeedGuiData.MaxPlayerNameChar, this.VictimIconSprite, this.VictimNameLabel, this.VictimBorderSprite, this.VictimBgSprite);
			this.BgIconSprite.gameObject.SetActive(!flag);
			this.BgSuicideIconSprite.gameObject.SetActive(flag);
			this.UpdateBgAnchors();
		}

		private void SetupGuiComponents(bool isKiller, HudKillfeedObject.HudKillfeedPlayerData killfeedPlayerData, int maxPlayerNameChar, HMMUI2DDynamicSprite iconSprite, UILabel nameLabel, UI2DSprite borderSprite, UI2DSprite bgSprite)
		{
			PlayerData playerData = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(killfeedPlayerData.Id);
			iconSprite.SpriteName = HudUtils.GetPlayerIconName(GameHubBehaviour.Hub, playerData.Character.CharacterItemTypeGuid, HudUtils.PlayerIconSize.Size128);
			nameLabel.color = Color.white;
			bool flag = !playerData.IsBot && playerData.IsBotControlled;
			if (flag)
			{
				nameLabel.text = string.Format("[{0}]{1}[-]", NGUIText.EncodeColor24(killfeedPlayerData.Color), GUIUtils.GetShortName(playerData.Character.LocalizedBotName, maxPlayerNameChar));
				nameLabel.TryUpdateText();
			}
			else
			{
				nameLabel.text = string.Format("[{0}]{1}[-]", NGUIText.EncodeColor24(killfeedPlayerData.Color), GUIUtils.GetShortName(playerData.Name, maxPlayerNameChar));
				nameLabel.TryUpdateText();
				if (!playerData.IsBot)
				{
					TeamUtils.GetUserTagAsync(GameHubBehaviour.Hub, playerData.UserId, delegate(string teamTag)
					{
						if (string.IsNullOrEmpty(teamTag))
						{
							return;
						}
						HudKillfeedObject.HudKillfeedGuiData data = this.Data;
						if (data == null)
						{
							return;
						}
						if (isKiller && data.KillerPlayerData.Id != killfeedPlayerData.Id)
						{
							return;
						}
						if (!isKiller && data.VictimPlayerData.Id != killfeedPlayerData.Id)
						{
							return;
						}
						string shortName = GUIUtils.GetShortName(playerData.Name, maxPlayerNameChar - NGUIText.StripSymbols(teamTag).Length + 1);
						nameLabel.text = string.Format("{0} [{1}]{2}[-]", teamTag, NGUIText.EncodeColor24(killfeedPlayerData.Color), shortName);
						nameLabel.TryUpdateText();
						this.UpdateBgAnchors();
					}, delegate(Exception exception)
					{
						HudKillfeedObject.Log.Error(string.Format("Error on GetUserTagAsync. Exception:{0}", exception));
					});
				}
			}
			borderSprite.color = killfeedPlayerData.Color;
			bgSprite.sprite2D = killfeedPlayerData.BgSprite;
		}

		private void UpdateBgAnchors()
		{
			if (this.BgIconSprite.gameObject.activeInHierarchy)
			{
				this.BgIconSprite.UpdateAnchors();
			}
			if (this.BgSuicideIconSprite.gameObject.activeInHierarchy)
			{
				this.BgSuicideIconSprite.UpdateAnchors();
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HudKillfeedObject));

		public GameObject KillerGroupGameObject;

		public HMMUI2DDynamicSprite KillerIconSprite;

		public UILabel KillerNameLabel;

		public UI2DSprite KillerBorderSprite;

		public UI2DSprite KillerBgSprite;

		public UI2DSprite CenterIconSprite;

		public HMMUI2DDynamicSprite VictimIconSprite;

		public UILabel VictimNameLabel;

		public UI2DSprite VictimBorderSprite;

		public UI2DSprite VictimBgSprite;

		public UI2DSprite BgIconSprite;

		public UI2DSprite BgSuicideIconSprite;

		private HudKillfeedObject.HudKillfeedGuiData _killfeedGuiData;

		public struct HudKillfeedPlayerData
		{
			public int Id;

			public Color Color;

			public Sprite BgSprite;
		}

		public class HudKillfeedGuiData : HudFeedObject<HudKillfeedObject.HudKillfeedGuiData>.HudFeedData
		{
			public HudKillfeedGuiData(Sprite centerSprite, HudKillfeedObject.HudKillfeedPlayerData victimPlayerData, int maxPlayerNameChar)
			{
				this.CenterSprite = centerSprite;
				this.VictimPlayerData = victimPlayerData;
				this.MaxPlayerNameChar = maxPlayerNameChar;
				this._isSuicide = true;
			}

			public HudKillfeedGuiData(HudKillfeedObject.HudKillfeedPlayerData killerPlayerData, Sprite centerSprite, HudKillfeedObject.HudKillfeedPlayerData victimPlayerData, int maxPlayerNameChar)
			{
				this.KillerPlayerData = killerPlayerData;
				this.CenterSprite = centerSprite;
				this.VictimPlayerData = victimPlayerData;
				this.MaxPlayerNameChar = maxPlayerNameChar;
				this._isSuicide = false;
			}

			public bool IsSuicide()
			{
				return this._isSuicide;
			}

			public HudKillfeedObject.HudKillfeedPlayerData KillerPlayerData;

			public Sprite CenterSprite;

			public HudKillfeedObject.HudKillfeedPlayerData VictimPlayerData;

			public int MaxPlayerNameChar;

			private readonly bool _isSuicide;
		}
	}
}
