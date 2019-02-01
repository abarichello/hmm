using System;
using FMod;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[RequireComponent(typeof(Animator))]
	public class MainMenuMissionsSplash : HudWindow
	{
		protected void Awake()
		{
			this.OverlayEventTrigger.onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnOverlayClick)));
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			this.OverlayEventTrigger.onClick.Clear();
			if (this._iconLoopAudioSrc != null && !this._iconLoopAudioSrc.IsInvalidated())
			{
				this._iconLoopAudioSrc.Stop();
			}
		}

		public void SetupAndShowWindow(MainMenuMissionsSplash.MissionsSplashWindowType type, int missionId, string descriptionText, int rewardValue, bool isRare, MainMenuMissionsSplash.OnExitSplashAnimationDelegate onExitAnimationDelegate)
		{
			this._canCloseWindow = false;
			this._missionType = type;
			this._missionId = missionId;
			this._exitAnimationEvent = onExitAnimationDelegate;
			this.DescriptionLabel.text = descriptionText;
			this.RewardLabel.text = rewardValue.ToString("0");
			this.UpdateTitleLabel(type, isRare);
			this.UpdateSprites(type, isRare);
			base.SetWindowVisibility(true);
		}

		public int GetMissionId()
		{
			return this._missionId;
		}

		public override void ChangeWindowVisibility(bool visible)
		{
			base.ChangeWindowVisibility(visible);
			if (visible)
			{
				FMODAudioManager.PlayOneShotAt(this.SfxUiWindowNewmissionOpen, CarCamera.Singleton.GetComponent<Camera>().transform.position, 0);
				this._iconLoopAudioSrc = FMODAudioManager.PlayAt(this.SfxUiWindowNewmissionIconLoop, base.transform);
			}
			else
			{
				FMODAudioManager.PlayOneShotAt(this.SfxUiWindowNewmissionClose, CarCamera.Singleton.GetComponent<Camera>().transform.position, 0);
				this._iconLoopAudioSrc.Stop();
			}
		}

		protected void AnimationOnWindowReady()
		{
			this._canCloseWindow = true;
		}

		private void OnOverlayClick()
		{
			if (this._canCloseWindow)
			{
				base.SetWindowVisibility(false);
			}
		}

		public override void AnimationOnWindowExit()
		{
			if (this._exitAnimationEvent != null)
			{
				this._exitAnimationEvent(this._missionType, this._missionId);
			}
		}

		private void UpdateSprites(MainMenuMissionsSplash.MissionsSplashWindowType type, bool isRare)
		{
			for (int i = 0; i < this.SpriteInfos.Length; i++)
			{
				MainMenuMissionsSplash.MainMenuMissionsSplashSlotSpriteInfo mainMenuMissionsSplashSlotSpriteInfo = this.SpriteInfos[i];
				if (mainMenuMissionsSplashSlotSpriteInfo.Type == type && mainMenuMissionsSplashSlotSpriteInfo.IsRare == isRare)
				{
					this.InfoBaseGlowSprite.sprite2D = mainMenuMissionsSplashSlotSpriteInfo.InfoBaseGlowSprite;
					this.InfoBottomGlowSprite.sprite2D = mainMenuMissionsSplashSlotSpriteInfo.InfoBottomGlowSprite;
					this.InfoTopGlowSprite.sprite2D = mainMenuMissionsSplashSlotSpriteInfo.InfoTopGlowSprite;
					this.LightSprite.sprite2D = mainMenuMissionsSplashSlotSpriteInfo.LightSprite;
					this.TitleGlowSprite.sprite2D = mainMenuMissionsSplashSlotSpriteInfo.TitleGlowSprite;
					this.TitleLabel.color = mainMenuMissionsSplashSlotSpriteInfo.TitleLabelColor;
					break;
				}
			}
		}

		private void UpdateTitleLabel(MainMenuMissionsSplash.MissionsSplashWindowType type, bool isRare)
		{
			this.TitleLabel.text = Language.Get((type != MainMenuMissionsSplash.MissionsSplashWindowType.New) ? "MISSION_COMPLETE_TITLE" : "NEW_MISSION_TITLE", TranslationSheets.Missions);
			this.TitleRareLabel.gameObject.SetActive(isRare);
		}

		[SerializeField]
		protected UILabel DescriptionLabel;

		[SerializeField]
		protected UILabel RewardLabel;

		[Header("[Audio]")]
		[SerializeField]
		protected FMODAsset SfxUiWindowNewmissionOpen;

		[SerializeField]
		protected FMODAsset SfxUiWindowNewmissionClose;

		[SerializeField]
		protected FMODAsset SfxUiWindowNewmissionIconLoop;

		[Header("[Sprites]")]
		[SerializeField]
		protected UI2DSprite LightSprite;

		[SerializeField]
		protected UI2DSprite TitleGlowSprite;

		[SerializeField]
		protected UILabel TitleLabel;

		[SerializeField]
		protected UILabel TitleRareLabel;

		[SerializeField]
		protected UI2DSprite InfoTopGlowSprite;

		[SerializeField]
		protected UI2DSprite InfoBaseGlowSprite;

		[SerializeField]
		protected UI2DSprite InfoBottomGlowSprite;

		[SerializeField]
		protected UIEventTrigger OverlayEventTrigger;

		[SerializeField]
		protected MainMenuMissionsSplash.MainMenuMissionsSplashSlotSpriteInfo[] SpriteInfos;

		private MainMenuMissionsSplash.OnExitSplashAnimationDelegate _exitAnimationEvent;

		private MainMenuMissionsSplash.MissionsSplashWindowType _missionType;

		private int _missionId;

		private FMODAudioManager.FMODAudio _iconLoopAudioSrc;

		private bool _canCloseWindow;

		public enum MissionsSplashWindowType
		{
			New,
			Completed
		}

		[Serializable]
		protected struct MainMenuMissionsSplashSlotSpriteInfo
		{
			public MainMenuMissionsSplash.MissionsSplashWindowType Type;

			public Sprite LightSprite;

			public Color TitleLabelColor;

			public Sprite TitleGlowSprite;

			public Sprite InfoTopGlowSprite;

			public Sprite InfoBaseGlowSprite;

			public Sprite InfoBottomGlowSprite;

			public bool IsRare;
		}

		public delegate void OnExitSplashAnimationDelegate(MainMenuMissionsSplash.MissionsSplashWindowType type, int missionId);
	}
}
