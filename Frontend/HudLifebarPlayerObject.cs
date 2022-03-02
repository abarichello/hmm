using System;
using System.Text;
using Assets.Customization;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.GadgetScript;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.ParentalControl.Restrictions;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Players.Presenting;
using HeavyMetalMachines.Utils;
using Hoplon.UserInterface;
using Pocketverse;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class HudLifebarPlayerObject : HudLifebarObject
	{
		public HudIconBar IconBar
		{
			get
			{
				return this._iconBar;
			}
		}

		public CombatObject CombatObject
		{
			get
			{
				return this._combatObject;
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this._combatObject)
			{
				this._combatObject.Player.ListenToBotControlChanged -= this.ListenToBotControlChanged;
			}
			if (this.hudEmotePresenter != null)
			{
				this.hudEmotePresenter.Dispose();
				this.hudEmotePresenter = null;
			}
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.BombManagerOnListenToPhaseChange;
			GameHubBehaviour.Hub.Options.Game.ListenToShowGadgetsLifebarChanged -= this.OptionsOnListenToShowGadgetsLifebarChanged;
			GameHubBehaviour.Hub.Options.Game.ShowLifebarTextChanged -= this.OptionsOnShowLifebarTextChanged;
		}

		public override void Setup(CombatObject combatObject)
		{
			base.Setup(combatObject);
			this.CooldownIndicatorData.MainGroup.SetActive(false);
			if (combatObject.IsLocalPlayer && !SpectatorController.IsSpectating)
			{
				this._lifebarPlayerType = HudLifebarPlayerObject.HudLifebarPlayerType.LocalPlayer;
				this._colorHpEncoded = NGUIText.EncodeColor24(this.HudLifebarSettings.PlayerHpTextColor);
				this._colorHpMaxEncoded = NGUIText.EncodeColor24(this.HudLifebarSettings.PlayerHpMaxTextColor);
				this._nameText.gameObject.SetActive(false);
				this._hpText.gameObject.SetActive(true);
				GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.BombManagerOnListenToPhaseChange;
				this.uiLifeBar.kind = UILifeBar.Kind.Self;
				this.SetupCurrentPlayerGadgets();
				GameHubBehaviour.Hub.Options.Game.ListenToShowGadgetsLifebarChanged += this.OptionsOnListenToShowGadgetsLifebarChanged;
			}
			else
			{
				bool flag = combatObject.IsSameTeamAsCurrentPlayer();
				this._lifebarPlayerType = ((!flag) ? HudLifebarPlayerObject.HudLifebarPlayerType.Enemy : HudLifebarPlayerObject.HudLifebarPlayerType.Ally);
				this.UpdateNameText(!combatObject.Player.IsBot && combatObject.Player.IsBotControlled);
				combatObject.Player.ListenToBotControlChanged += this.ListenToBotControlChanged;
				this._nameText.gameObject.SetActive(true);
				this._hpText.gameObject.SetActive(false);
				this.uiLifeBar.kind = ((!flag) ? UILifeBar.Kind.Enemy : UILifeBar.Kind.Ally);
				Object.Destroy(this.CounselorParentGameObject);
			}
			this.hudEmotePresenter = new HudEmotePresenter(this.hudEmoteView, GameHubBehaviour.Hub.Players, this._customizationAssets, this._isPlayerRestrictedByTextChat);
			this.hudEmotePresenter.Initialize(combatObject.Player.PlayerCarId);
			this.SetLifebarBackground(this.uiLifeBar.kind);
			this.OptionsOnShowLifebarTextChanged();
			GameHubBehaviour.Hub.Options.Game.ShowLifebarTextChanged += this.OptionsOnShowLifebarTextChanged;
			this.InvulnerableData.Object.SetActive(false);
			this._showIndestructibleFeedback = GameHubBehaviour.Hub.ArenaConfig.GetCurrentArena().LifebarShowIndestructibleFeedback;
		}

		private void SetLifebarBackground(UILifeBar.Kind kind)
		{
			if (kind != UILifeBar.Kind.Enemy)
			{
				if (kind != UILifeBar.Kind.Ally)
				{
					if (kind == UILifeBar.Kind.Self)
					{
						this._backgroundLifebarImage.Sprite = this._lifebarBackgroundImages.PlayerBackGroundSprite;
					}
				}
				else
				{
					this._backgroundLifebarImage.Sprite = this._lifebarBackgroundImages.AllyBackGroundSprite;
				}
			}
			else
			{
				this._backgroundLifebarImage.Sprite = this._lifebarBackgroundImages.EnemyBackGroundSprite;
			}
		}

		private void ListenToBotControlChanged(PlayerData obj)
		{
			if (HudLifebarPlayerObject.IsMatchOver())
			{
				return;
			}
			if (!SpectatorController.IsSpectating && this.CombatObject.Id.ObjId == GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.ObjId)
			{
				return;
			}
			this.UpdateNameText(!obj.IsBot && obj.IsBotControlled);
		}

		private static bool IsMatchOver()
		{
			return GameHubBehaviour.Hub.Match.State == MatchData.MatchState.MatchOverTie || GameHubBehaviour.Hub.Match.State == MatchData.MatchState.MatchOverBluWins || GameHubBehaviour.Hub.Match.State == MatchData.MatchState.MatchOverRedWins;
		}

		private void UpdateNameText(bool useLocalizedBotName)
		{
			string text;
			if (useLocalizedBotName)
			{
				text = this.CombatObject.Player.GetCharacterBotLocalizedName();
			}
			else
			{
				text = GUIUtils.GetShortName(this.CombatObject.Player.Name, this.HudLifebarSettings.PlayerNameMaxChars);
				if (!this.CombatObject.Player.IsBot)
				{
					text = this._getDisplayableNickName.GetFormattedNickNameWithPlayerTag(this.CombatObject.Player.PlayerId, text, new long?(this.CombatObject.Player.PlayerTag));
				}
			}
			this._nameText.Text = text;
			Color color = (!this.CombatObject.IsSameTeamAsCurrentPlayer()) ? this.HudLifebarSettings.EnemyNameColor : this.HudLifebarSettings.AllyNameColor;
			this._nameText.Color = color;
			this._nameText.gameObject.SetActive(true);
			if (!useLocalizedBotName && !this.CombatObject.Player.IsBot)
			{
				this._nameText.SupportRichText = true;
				this._nameText.Color = Color.white;
				this._nameText.Text = string.Format("<color=#{0}>{1}</color>", HudUtils.RGBToHex(color), text);
			}
		}

		private void BombManagerOnListenToPhaseChange(BombScoreboardState state)
		{
			if (state == BombScoreboardState.Shop)
			{
				this.RenderStateOutOfCombatUpdate();
			}
		}

		protected override void RenderUpdate()
		{
			base.RenderUpdate();
			if (this._lifebarPlayerType == HudLifebarPlayerObject.HudLifebarPlayerType.LocalPlayer && GameHubBehaviour.Hub.Options.Game.ShowLifebarText)
			{
				this._hpTextBuilder.Length = 0;
				this._hpTextBuilder.AppendFormat("<color=#{0}>", this._colorHpEncoded);
				this._hpTextBuilder.Append(Mathf.RoundToInt(Mathf.Ceil(this._lastFullHp)));
				this._hpTextBuilder.AppendFormat("</color><color=#{0}>/", this._colorHpMaxEncoded);
				this._hpTextBuilder.Append(Mathf.RoundToInt(Mathf.Ceil(this._maxFullHp)));
				this._hpTextBuilder.Append("</color>");
				this._hpText.Text = this._hpTextBuilder.ToString();
			}
			StatusKind currentStatus = this._combatObject.Attributes.CurrentStatus;
			this.RenderStateIntangibleUpdate(currentStatus);
			this.RenderStateInvulnerableUpdate(currentStatus);
			this.RenderStateOutOfCombatUpdate();
			if (this.uiLifeBar.kind == UILifeBar.Kind.Self)
			{
				this.RenderGadgetUpdate();
			}
			this.RenderAttachedGroupUpdate();
		}

		private void RenderAttachedGroupUpdate()
		{
			if (this._attachedCombatId == 0)
			{
				return;
			}
			this._attachedEpProgressImage.FillAmount = this._attachedCombat.Data.EP / (float)this._attachedCombat.Data.EPMax;
			this._attachedHpProgressImage.FillAmount = this._attachedCombat.Data.CurrentHPPercent;
		}

		private void RenderStateOutOfCombatUpdate()
		{
			if (!this._combatObject || !this._combatObject.IsAlive())
			{
				return;
			}
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				this.OutOfCombatImage.FillAmount = 0f;
				return;
			}
			CombatData data = this._combatObject.Data;
			if (data.HP >= (float)data.HPMax)
			{
				this.OutOfCombatImage.FillAmount = 0f;
				return;
			}
			GadgetData.GadgetStateObject gadgetState = this._combatObject.Combat.GadgetStates.GetGadgetState(GadgetSlot.OutOfCombatGadget);
			if (gadgetState == null)
			{
				return;
			}
			int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			long cooldown = gadgetState.Cooldown;
			if ((long)playbackTime >= cooldown)
			{
				this.OutOfCombatImage.FillAmount = 1f;
				return;
			}
			float num = (this._combatObject.OutOfCombatGadget.Cooldown - 0.5f) * 1000f;
			float num2 = (float)(cooldown - (long)playbackTime);
			this.OutOfCombatImage.FillAmount = 1f - Mathf.Clamp01(num2 / num);
		}

		private void RenderStateIntangibleUpdate(StatusKind statusKind)
		{
			bool flag = statusKind.HasFlag(StatusKind.Banished);
			base.SetMaxAlpha((!flag) ? 1f : 0.5f);
		}

		private void RenderStateInvulnerableUpdate(StatusKind statusKind)
		{
			if (this._showIndestructibleFeedback && statusKind.HasFlag(StatusKind.Invulnerable))
			{
				this.InvulnerableData.Object.SetActive(true);
			}
			else
			{
				this.InvulnerableData.Object.SetActive(false);
			}
		}

		protected override void CombatObjectOnObjectSpawn(CombatObject combatObject, SpawnEvent msg)
		{
			base.CombatObjectOnObjectSpawn(combatObject, msg);
			this.OutOfCombatImage.FillAmount = 1f;
		}

		private void RenderGadgetUpdate()
		{
			if (!GameHubBehaviour.Hub.Options.Game.ShowGadgetsLifebar)
			{
				return;
			}
			this.RenderGadgetUpdate(this.CooldownIndicatorData.Gadget0FillImage, GadgetSlot.CustomGadget0, this._combatObject.CustomGadget0);
			this.RenderGadgetUpdate(this.CooldownIndicatorData.Gadget1FillImage, GadgetSlot.CustomGadget1, this._combatObject.CustomGadget1);
			this.RenderGadgetUpdate(this.CooldownIndicatorData.GadgetBoostFillImage, GadgetSlot.BoostGadget, this._combatObject.BoostGadget);
		}

		private void RenderGadgetUpdate(HoplonImage fillImage, GadgetSlot gadgetSlot, GadgetBehaviour gadgetBehaviour)
		{
			if (this.TryRenderCustomGadgetUpdate(fillImage, gadgetSlot))
			{
				return;
			}
			GadgetData.GadgetStateObject gadgetState = this._combatObject.GadgetStates.GetGadgetState(gadgetSlot);
			if (gadgetState.GadgetState == GadgetState.Cooldown)
			{
				long num = gadgetState.Cooldown - (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
				float num2 = (float)num * 0.001f / gadgetBehaviour.Cooldown;
				fillImage.FillAmount = 1f - num2;
			}
			else if (fillImage.FillAmount > 0.001f)
			{
				fillImage.FillAmount = 1f;
			}
		}

		private bool TryRenderCustomGadgetUpdate(HoplonImage fillImage, GadgetSlot gadgetSlot)
		{
			CombatGadget combatGadget = (CombatGadget)this._combatObject.GetGadgetContext((int)gadgetSlot);
			if (null == combatGadget)
			{
				return false;
			}
			float fillAmount = 1f;
			if (combatGadget.HasCooldownParameters())
			{
				int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
				int cooldownEndTime = combatGadget.GetCooldownEndTime();
				float cooldownTotalTime = combatGadget.GetCooldownTotalTime();
				if (cooldownEndTime > playbackTime)
				{
					float num = (float)(cooldownEndTime - playbackTime) * 0.001f;
					if (cooldownTotalTime > 0f)
					{
						fillAmount = 1f - num / cooldownTotalTime;
					}
				}
			}
			fillImage.FillAmount = fillAmount;
			return true;
		}

		private void SetupCurrentPlayerGadgets()
		{
			this.CooldownIndicatorData.SetVisibility(GameHubBehaviour.Hub.Options.Game.ShowGadgetsLifebar);
		}

		private void OptionsOnListenToShowGadgetsLifebarChanged(bool isEnabled)
		{
			this.SetupCurrentPlayerGadgets();
		}

		private void OptionsOnShowLifebarTextChanged()
		{
			this._textControlObject.SetActive(GameHubBehaviour.Hub.Options.Game.ShowLifebarText);
		}

		public void SetAttachedGroupVisibility(int attachedObjectId, bool visible)
		{
			this._attachedGroup.SetActive(visible);
			if (visible)
			{
				this._attachedCombat = CombatRef.GetCombat(attachedObjectId);
				this._attachedCombatId = attachedObjectId;
				if (this._attachedCombat.IsLocalPlayer)
				{
					this._attachedHpProgressImage.Color = this._attachedHpColorSelf;
				}
				else if (this._attachedCombat.IsSameTeamAsCurrentPlayer())
				{
					this._attachedHpProgressImage.Color = this._attachedHpColorAlly;
				}
				else
				{
					this._attachedHpProgressImage.Color = this._attachedHpColorEnemy;
				}
				this.RenderAttachedGroupUpdate();
			}
			else
			{
				this._attachedCombat = null;
				this._attachedCombatId = 0;
			}
		}

		private HudLifebarPlayerObject.HudLifebarPlayerType _lifebarPlayerType;

		[Inject]
		private IMatchTeams _teams;

		[Inject]
		private ICustomizationAssets _customizationAssets;

		[Inject]
		private IIsPlayerRestrictedByTextChat _isPlayerRestrictedByTextChat;

		[Inject]
		private IGetDisplayableNickName _getDisplayableNickName;

		[Inject]
		private ITeamNameRestriction _teamNameRestriction;

		[Header("Icons")]
		[SerializeField]
		private HudIconBar _iconBar;

		[Header("[Text info]")]
		[SerializeField]
		private GameObject _textControlObject;

		[SerializeField]
		private HoplonText _nameText;

		[SerializeField]
		private HoplonText _hpText;

		[SerializeField]
		[Header("Emote")]
		private HudEmoteView hudEmoteView;

		public IHudEmotePresenter hudEmotePresenter;

		[Header("[State data]")]
		[SerializeField]
		public HudLifebarPlayerObject.HudLifebarInvulnerableData InvulnerableData;

		[Header("[Out of combat bar]")]
		public HoplonImage OutOfCombatImage;

		[Header("[Counselor]")]
		public GameObject CounselorParentGameObject;

		[Header("[Attached]")]
		[SerializeField]
		private GameObject _attachedGroup;

		[SerializeField]
		private HoplonImage _attachedEpProgressImage;

		[SerializeField]
		private HoplonImage _attachedHpProgressImage;

		[SerializeField]
		private Color _attachedHpColorAlly;

		[SerializeField]
		private Color _attachedHpColorSelf;

		[SerializeField]
		private Color _attachedHpColorEnemy;

		[Header("[Cooldown Indicator]")]
		[SerializeField]
		private HudLifebarPlayerObject.HudLifebarCooldownIndicatorData CooldownIndicatorData;

		[SerializeField]
		private HudLifebarPlayerObject.LifebarBackgroundImages _lifebarBackgroundImages;

		[SerializeField]
		private HoplonImage _backgroundLifebarImage;

		private string _colorHpEncoded;

		private string _colorHpMaxEncoded;

		private readonly StringBuilder _hpTextBuilder = new StringBuilder();

		private bool _showIndestructibleFeedback;

		private CombatObject _attachedCombat;

		private int _attachedCombatId;

		private enum HudLifebarPlayerType : byte
		{
			LocalPlayer,
			Ally,
			Enemy
		}

		[Serializable]
		public class CustomValuesConfig
		{
			public Text TargetText;

			public string Value;
		}

		[Serializable]
		public class HudLifebarInvulnerableData
		{
			public GameObject Object;
		}

		[Serializable]
		private struct HudLifebarCooldownIndicatorData
		{
			public void SetVisibility(bool isVisible)
			{
				this.MainGroup.SetActive(isVisible);
			}

			public GameObject MainGroup;

			public HoplonImage Gadget0FillImage;

			public HoplonImage Gadget1FillImage;

			public HoplonImage GadgetBoostFillImage;
		}

		[Serializable]
		private struct LifebarBackgroundImages
		{
			public Sprite AllyBackGroundSprite;

			public Sprite EnemyBackGroundSprite;

			public Sprite PlayerBackGroundSprite;
		}
	}
}
