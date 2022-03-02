using System;
using HeavyMetalMachines.Presenting.Unity;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudKillfeedObjectUnityUI : HudFeedObject<HudKillfeedObjectUnityUI.HudKillfeedGuiData>
	{
		public override HudKillfeedObjectUnityUI.HudKillfeedGuiData Data
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

		public override void FeedUpdate(float timeoutDeltaInSec)
		{
			base.FeedUpdate(timeoutDeltaInSec);
			if (this.CanvasGroup == null || this.CanvasGroup.gameObject == null)
			{
				return;
			}
			if (this.CanvasGroup.alpha == 0f && this.CanvasGroup.gameObject.activeSelf && this.Data == null)
			{
				this.CanvasGroup.gameObject.SetActive(false);
			}
		}

		public override void Setup(HudKillfeedObjectUnityUI.HudKillfeedGuiData killfeedGuiData)
		{
			this._killfeedGuiData = killfeedGuiData;
			int num = (this._killfeedGuiData == null) ? 0 : 1;
			this.CanvasGroup.alpha = (float)num;
			this.CanvasGroup.gameObject.SetActive(true);
			if (this._killfeedGuiData == null)
			{
				return;
			}
			bool flag = killfeedGuiData.IsSuicide();
			this.KillerCanvasGroup.alpha = (float)(flag ? 0 : 1);
			if (!flag)
			{
				this.SetupGuiComponents(killfeedGuiData.KillerPlayerData, this.KillerIconSprite, this.KillerNameLabel, this.KillerBorderSprite, this.KillerBgSprite);
			}
			this.CenterIconSprite.Sprite = killfeedGuiData.UnityCenterSprite;
			this.SetupGuiComponents(killfeedGuiData.VictimPlayerData, this.VictimIconSprite, this.VictimNameLabel, this.VictimBorderSprite, this.VictimBgSprite);
			this.BgIconSprite.IsActive = !flag;
			this.BgSuicideIconSprite.IsActive = flag;
		}

		private void SetupGuiComponents(HudKillfeedObjectUnityUI.HudKillfeedPlayerData killfeedPlayerData, UnityDynamicImage iconSprite, UnityLabel nameLabel, UnityImage borderSprite, UnityImage bgSprite)
		{
			iconSprite.SetSprite(killfeedPlayerData.CharacterSprite);
			nameLabel.Text = killfeedPlayerData.Name;
			nameLabel.Color = killfeedPlayerData.Color.ToHmmColor();
			borderSprite.Color = killfeedPlayerData.Color.ToHmmColor();
			bgSprite.Sprite = killfeedPlayerData.UnityBgSprite;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HudKillfeedObjectUnityUI));

		public UnityDynamicImage KillerIconSprite;

		public UnityLabel KillerNameLabel;

		public UnityImage KillerBorderSprite;

		public UnityImage KillerBgSprite;

		public CanvasGroup KillerCanvasGroup;

		public UnityImage CenterIconSprite;

		public UnityDynamicImage VictimIconSprite;

		public UnityLabel VictimNameLabel;

		public UnityImage VictimBorderSprite;

		public UnityImage VictimBgSprite;

		public UnityImage BgIconSprite;

		public UnityImage BgSuicideIconSprite;

		private HudKillfeedObjectUnityUI.HudKillfeedGuiData _killfeedGuiData;

		public struct HudKillfeedPlayerData
		{
			public UnitySprite UnityBgSprite
			{
				get
				{
					if (this._unityBgSprite == null)
					{
						this._unityBgSprite = new UnitySprite(this.BgSprite);
					}
					return this._unityBgSprite;
				}
			}

			public int Id;

			public Color Color;

			public Sprite BgSprite;

			public Sprite CharacterSprite;

			public string Name;

			private UnitySprite _unityBgSprite;
		}

		public class HudKillfeedGuiData : HudFeedObject<HudKillfeedObjectUnityUI.HudKillfeedGuiData>.HudFeedData
		{
			public HudKillfeedGuiData(Sprite centerSprite, HudKillfeedObjectUnityUI.HudKillfeedPlayerData victimPlayerData, int maxPlayerNameChar)
			{
				this.CenterSprite = centerSprite;
				this.VictimPlayerData = victimPlayerData;
				this.MaxPlayerNameChar = maxPlayerNameChar;
				this._isSuicide = true;
			}

			public HudKillfeedGuiData(HudKillfeedObjectUnityUI.HudKillfeedPlayerData killerPlayerData, Sprite centerSprite, HudKillfeedObjectUnityUI.HudKillfeedPlayerData victimPlayerData, int maxPlayerNameChar)
			{
				this.KillerPlayerData = killerPlayerData;
				this.CenterSprite = centerSprite;
				this.VictimPlayerData = victimPlayerData;
				this.MaxPlayerNameChar = maxPlayerNameChar;
				this._isSuicide = false;
			}

			public UnitySprite UnityCenterSprite
			{
				get
				{
					if (this._unityCenterSprite == null)
					{
						this._unityCenterSprite = new UnitySprite(this.CenterSprite);
					}
					return this._unityCenterSprite;
				}
			}

			public bool IsSuicide()
			{
				return this._isSuicide;
			}

			private UnitySprite _unityCenterSprite;

			public HudKillfeedObjectUnityUI.HudKillfeedPlayerData KillerPlayerData;

			public Sprite CenterSprite;

			public HudKillfeedObjectUnityUI.HudKillfeedPlayerData VictimPlayerData;

			public int MaxPlayerNameChar;

			private readonly bool _isSuicide;
		}
	}
}
