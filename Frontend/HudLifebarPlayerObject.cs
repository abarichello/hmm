using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.GadgetScript;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	public class HudLifebarPlayerObject : HudLifebarObject
	{
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
				this._combatObject.GadgetStates.GetGadgetState(GadgetSlot.CustomGadget0).ListenToGadgetReady -= this.OnListenToGadget0Ready;
				this._combatObject.GadgetStates.GetGadgetState(GadgetSlot.CustomGadget1).ListenToGadgetReady -= this.OnListenToGadget1Ready;
				this._combatObject.GadgetStates.GetGadgetState(GadgetSlot.BoostGadget).ListenToGadgetReady -= this.OnListenToGadgetBoostReady;
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
				this.NameText.gameObject.SetActive(false);
				this.HpText.gameObject.SetActive(true);
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
				this.HpText.gameObject.SetActive(false);
				this.uiLifeBar.kind = ((!flag) ? UILifeBar.Kind.Enemy : UILifeBar.Kind.Ally);
				UnityEngine.Object.Destroy(this.CounselorParentGameObject);
			}
			this.SetLifebarBackground(this.uiLifeBar.kind);
			this.SetupTextInfoVisibility();
			GameHubBehaviour.Hub.Options.Game.ShowLifebarTextChanged += this.OptionsOnShowLifebarTextChanged;
			this.RepairData.Animation.playAutomatically = false;
			this.ArmourData.ArmourAnimation.playAutomatically = false;
			this.ArmourData.VulnerableAnimation.playAutomatically = false;
			this.InvulnerableData.MainCanvasGroup.alpha = 0f;
			this._showIndestructibleFeedback = GameHubBehaviour.Hub.ArenaConfig.Arenas[GameHubBehaviour.Hub.Match.ArenaIndex].LifebarShowIndestructibleFeedback;
		}

		private void SetLifebarBackground(UILifeBar.Kind kind)
		{
			if (kind != UILifeBar.Kind.Enemy)
			{
				if (kind != UILifeBar.Kind.Ally)
				{
					if (kind == UILifeBar.Kind.Self)
					{
						this._backgroundLifebarImage.sprite = this._lifebarBackgroundImages.PlayerBackGroundSprite;
					}
				}
				else
				{
					this._backgroundLifebarImage.sprite = this._lifebarBackgroundImages.AllyBackGroundSprite;
				}
			}
			else
			{
				this._backgroundLifebarImage.sprite = this._lifebarBackgroundImages.EnemyBackGroundSprite;
			}
		}

		private void ListenToBotControlChanged(PlayerData obj)
		{
			if (!SpectatorController.IsSpectating && this.CombatObject.Id.ObjId == GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.ObjId)
			{
				return;
			}
			this.UpdateNameText(!obj.IsBot && obj.IsBotControlled);
		}

		private void UpdateNameText(bool useLocalizedBotName)
		{
			string nameText = (!useLocalizedBotName) ? GUIUtils.GetShortName(this.CombatObject.Player.Name, this.HudLifebarSettings.PlayerNameMaxChars) : this.CombatObject.Player.Character.LocalizedBotName;
			this.NameText.text = nameText;
			this.NameText.supportRichText = false;
			Color playerColor = (!this.CombatObject.IsSameTeamAsCurrentPlayer()) ? this.HudLifebarSettings.EnemyNameColor : this.HudLifebarSettings.AllyNameColor;
			this.NameText.color = playerColor;
			this.NameText.gameObject.SetActive(true);
			if (!useLocalizedBotName && !this.CombatObject.Player.IsBot)
			{
				TeamUtils.GetUserTagAsync(GameHubBehaviour.Hub, this.CombatObject.Player.UserId, delegate(string teamTag)
				{
					if (!string.IsNullOrEmpty(teamTag))
					{
						this.NameText.supportRichText = true;
						this.NameText.color = Color.white;
						this.NameText.text = string.Format("<color=#{0}>{1}</color> <color=#{2}>{3}</color>", new object[]
						{
							HudUtils.RGBToHex(GameHubBehaviour.Hub.GuiScripts.GUIColors.TeamTagColor),
							NGUIText.StripSymbols(teamTag),
							HudUtils.RGBToHex(playerColor),
							nameText
						});
					}
				}, delegate(Exception exception)
				{
					HudLifebarObject.Log.Warn(string.Format("Error on GetUserTagAsync. Exception:{0}", exception));
				});
			}
		}

		private void BombManagerOnListenToPhaseChange(BombScoreBoard.State state)
		{
			if (state == BombScoreBoard.State.Shop)
			{
				this.RenderStateOutOfCombatUpdate();
			}
		}

		protected override void RenderUpdate()
		{
			base.RenderUpdate();
			if (this._lifebarPlayerType == HudLifebarPlayerObject.HudLifebarPlayerType.LocalPlayer && GameHubBehaviour.Hub.Options.Game.ShowLifebarText)
			{
				this.HpText.text = string.Format("<color=#{0}>{1}</color><color=#{2}>/{3}</color>", new object[]
				{
					this._colorHpEncoded,
					Mathf.Ceil(this._lastFullHp).ToString("0"),
					this._colorHpMaxEncoded,
					Mathf.Ceil(this._maxFullHp).ToString("0")
				});
			}
			StatusKind currentStatus = this._combatObject.Attributes.CurrentStatus;
			this.RenderStateIntangibleUpdate(currentStatus);
			this.RenderStateInvulnerableUpdate(currentStatus);
			this.RenderStateArmourUpdate(currentStatus);
			this.RenderStateRepairUpdate();
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
			this._attachedEpProgressImage.fillAmount = this._attachedCombat.Data.EP / (float)this._attachedCombat.Data.EPMax;
			this._attachedHpProgressImage.fillAmount = this._attachedCombat.Data.CurrentHPPercent;
		}

		private void RenderStateOutOfCombatUpdate()
		{
			if (!this._combatObject || !this._combatObject.IsAlive())
			{
				return;
			}
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				this.OutOfCombatImage.fillAmount = 0f;
				return;
			}
			CombatData data = this._combatObject.Data;
			if (data.HP >= (float)data.HPMax)
			{
				this.OutOfCombatImage.fillAmount = 0f;
				return;
			}
			GadgetData.GadgetStateObject gadgetState = this._combatObject.Combat.GadgetStates.GetGadgetState(GadgetSlot.OutOfCombatGadget);
			if (gadgetState == null)
			{
				return;
			}
			int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			long coolDown = gadgetState.CoolDown;
			if ((long)playbackTime >= coolDown)
			{
				this.OutOfCombatImage.fillAmount = 1f;
				return;
			}
			float num = (this._combatObject.OutOfCombatGadget.Cooldown - 0.5f) * 1000f;
			float num2 = (float)(coolDown - (long)playbackTime);
			this.OutOfCombatImage.fillAmount = 1f - Mathf.Clamp01(num2 / num);
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
				if (this.InvulnerableData.MainCanvasGroup.alpha < 0.001f)
				{
					this.InvulnerableData.ActiveAnimation.Play();
					this.InvulnerableData.MainCanvasGroup.alpha = 1f;
				}
			}
			else
			{
				this.InvulnerableData.MainCanvasGroup.alpha = 0f;
			}
		}

		private void RenderStateArmourUpdate(StatusKind statusKind)
		{
			int hplightArmor = this._combatObject.Data.HPLightArmor;
			bool isArmoured = hplightArmor > 0;
			bool isVulnerable = hplightArmor < 0;
			bool isInvulnerable = statusKind.HasFlag(StatusKind.Invulnerable);
			this.ArmourData.Update(isArmoured, isVulnerable, isInvulnerable);
		}

		private void RenderStateRepairUpdate()
		{
			float hpregenPct = this._combatObject.Attributes.HPRegenPct;
			this.RepairData.Update(hpregenPct);
		}

		public void SetVisible(bool isVisible)
		{
			this.LifebarCanvas.alpha = ((!isVisible) ? 0f : 1f);
		}

		protected override void CombatObjectOnObjectSpawn(CombatObject combatObject, SpawnEvent msg)
		{
			base.CombatObjectOnObjectSpawn(combatObject, msg);
			this.OutOfCombatImage.fillAmount = 1f;
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

		private void RenderGadgetUpdate(Image fillImage, GadgetSlot gadgetSlot, GadgetBehaviour gadgetBehaviour)
		{
			if (this.TryRenderCustomGadgetUpdate(fillImage, gadgetSlot))
			{
				return;
			}
			GadgetData.GadgetStateObject gadgetState = this._combatObject.GadgetStates.GetGadgetState(gadgetSlot);
			if (gadgetState.GadgetState == GadgetState.Cooldown)
			{
				long num = gadgetState.CoolDown - (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
				float num2 = (float)num * 0.001f / gadgetBehaviour.Cooldown;
				fillImage.fillAmount = 1f - num2;
			}
			else if (fillImage.fillAmount > 0.001f)
			{
				fillImage.fillAmount = 1f;
			}
		}

		private bool TryRenderCustomGadgetUpdate(Image fillImage, GadgetSlot gadgetSlot)
		{
			CombatGadget combatGadget;
			if (!this._combatObject.CustomGadgets.TryGetValue(gadgetSlot, out combatGadget))
			{
				return false;
			}
			float num = 1f;
			if (combatGadget.HasCooldownParameters())
			{
				int playbackTime = GameHubBehaviour.Hub.Clock.GetPlaybackTime();
				int cooldownEndTime = combatGadget.GetCooldownEndTime();
				float cooldownTotalTime = combatGadget.GetCooldownTotalTime();
				if (cooldownEndTime > playbackTime)
				{
					float num2 = (float)(cooldownEndTime - playbackTime) * 0.001f;
					if (cooldownTotalTime > 0f)
					{
						num = 1f - num2 / cooldownTotalTime;
					}
				}
				if (num >= 1f && fillImage.fillAmount < num)
				{
					this.PlayCooldownIndicatorGadgetReadyAnimation(gadgetSlot);
				}
			}
			fillImage.fillAmount = num;
			return true;
		}

		private void SetupCurrentPlayerGadgets()
		{
			if (GameHubBehaviour.Hub.Options.Game.ShowGadgetsLifebar)
			{
				this.CooldownIndicatorData.SetVisibility(true);
				this._combatObject.GadgetStates.GetGadgetState(GadgetSlot.CustomGadget0).ListenToGadgetReady += this.OnListenToGadget0Ready;
				this._combatObject.GadgetStates.GetGadgetState(GadgetSlot.CustomGadget1).ListenToGadgetReady += this.OnListenToGadget1Ready;
				this._combatObject.GadgetStates.GetGadgetState(GadgetSlot.BoostGadget).ListenToGadgetReady += this.OnListenToGadgetBoostReady;
			}
			else
			{
				this.CooldownIndicatorData.SetVisibility(false);
				this._combatObject.GadgetStates.GetGadgetState(GadgetSlot.CustomGadget0).ListenToGadgetReady -= this.OnListenToGadget0Ready;
				this._combatObject.GadgetStates.GetGadgetState(GadgetSlot.CustomGadget1).ListenToGadgetReady -= this.OnListenToGadget1Ready;
				this._combatObject.GadgetStates.GetGadgetState(GadgetSlot.BoostGadget).ListenToGadgetReady -= this.OnListenToGadgetBoostReady;
			}
		}

		private void SetupTextInfoVisibility()
		{
			this.TextCanvasGroup.alpha = ((!GameHubBehaviour.Hub.Options.Game.ShowLifebarText) ? 0f : 1f);
		}

		private void OptionsOnListenToShowGadgetsLifebarChanged(bool isEnabled)
		{
			this.SetupCurrentPlayerGadgets();
		}

		private void OptionsOnShowLifebarTextChanged()
		{
			this.SetupTextInfoVisibility();
		}

		private void OnListenToGadget0Ready()
		{
			this.PlayCooldownIndicatorGadgetReadyAnimation(GadgetSlot.CustomGadget0);
		}

		private void OnListenToGadget1Ready()
		{
			this.PlayCooldownIndicatorGadgetReadyAnimation(GadgetSlot.CustomGadget1);
		}

		private void OnListenToGadgetBoostReady()
		{
			this.PlayCooldownIndicatorGadgetReadyAnimation(GadgetSlot.BoostGadget);
		}

		private void PlayCooldownIndicatorGadgetReadyAnimation(GadgetSlot slot)
		{
			if (slot != GadgetSlot.CustomGadget0)
			{
				if (slot != GadgetSlot.CustomGadget1)
				{
					if (slot == GadgetSlot.BoostGadget)
					{
						this.CooldownIndicatorData.GadgetBoostGlowAnimation.Play();
					}
				}
				else
				{
					this.CooldownIndicatorData.Gadget1GlowAnimation.Play();
				}
			}
			else
			{
				this.CooldownIndicatorData.Gadget0GlowAnimation.Play();
			}
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
					this._attachedHpProgressImage.color = this._attachedHpColorSelf;
				}
				else if (this._attachedCombat.IsSameTeamAsCurrentPlayer())
				{
					this._attachedHpProgressImage.color = this._attachedHpColorAlly;
				}
				else
				{
					this._attachedHpProgressImage.color = this._attachedHpColorEnemy;
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

		[Header("[Text info]")]
		public CanvasGroup TextCanvasGroup;

		public Text NameText;

		public Text HpText;

		public CanvasRenderer HpTextCanvasRenderer;

		[Header("[Debug data]")]
		public List<StatusKind> TestStatus;

		public HudLifebarPlayerObject.DebugCombatData TestCombatData;

		[Header("[State datas]")]
		[SerializeField]
		public HudLifebarPlayerObject.HudLifebarInvulnerableData InvulnerableData;

		[SerializeField]
		public HudLifebarPlayerObject.HudLifebarArmourData ArmourData;

		[SerializeField]
		public HudLifebarPlayerObject.HudLifebarRepairData RepairData;

		[Header("[Out of combat bar]")]
		public Image OutOfCombatImage;

		[Header("[Counselor]")]
		public GameObject CounselorParentGameObject;

		[Header("[Attached]")]
		[SerializeField]
		private GameObject _attachedGroup;

		[SerializeField]
		private Image _attachedEpProgressImage;

		[SerializeField]
		private Image _attachedHpProgressImage;

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
		private Image _backgroundLifebarImage;

		private string _colorHpEncoded;

		private string _colorHpMaxEncoded;

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
		public struct DebugCombatData
		{
			[Range(-1f, 1f)]
			public float Repair;

			public int Armor;
		}

		[Serializable]
		public class HudLifebarInvulnerableData
		{
			public Animation ActiveAnimation;

			public CanvasGroup MainCanvasGroup;
		}

		[Serializable]
		public class HudLifebarArmourData
		{
			public void Update(bool isArmoured, bool isVulnerable, bool isInvulnerable)
			{
				bool flag = (isArmoured || isVulnerable) && !isInvulnerable;
				this._nextAnimationIsBuff = isArmoured;
				if (this._currentAnimation != null && this._currentAnimation.isPlaying)
				{
					flag = (flag && this._currentAnimationIsBuff == this._nextAnimationIsBuff);
					if (flag)
					{
						IEnumerator enumerator = this._currentAnimation.GetEnumerator();
						try
						{
							while (enumerator.MoveNext())
							{
								object obj = enumerator.Current;
								AnimationState animationState = (AnimationState)obj;
								if (animationState.normalizedTime > 0.5f)
								{
									animationState.normalizedTime = 0.5f;
									animationState.speed = 0f;
								}
							}
						}
						finally
						{
							IDisposable disposable;
							if ((disposable = (enumerator as IDisposable)) != null)
							{
								disposable.Dispose();
							}
						}
					}
					else
					{
						IEnumerator enumerator2 = this._currentAnimation.GetEnumerator();
						try
						{
							while (enumerator2.MoveNext())
							{
								object obj2 = enumerator2.Current;
								AnimationState animationState2 = (AnimationState)obj2;
								animationState2.speed = 1f;
							}
						}
						finally
						{
							IDisposable disposable2;
							if ((disposable2 = (enumerator2 as IDisposable)) != null)
							{
								disposable2.Dispose();
							}
						}
					}
					return;
				}
				if (flag)
				{
					this._currentAnimationIsBuff = isArmoured;
				}
				this.MainGroup.SetActive(flag);
				if (flag)
				{
					this.ArmourImage.gameObject.SetActive(isArmoured && !isVulnerable);
					this.VulnerableImage.gameObject.SetActive(isVulnerable);
					this._currentAnimation = ((!isVulnerable) ? this.ArmourAnimation : this.VulnerableAnimation);
					IEnumerator enumerator3 = this._currentAnimation.GetEnumerator();
					try
					{
						while (enumerator3.MoveNext())
						{
							object obj3 = enumerator3.Current;
							AnimationState animationState3 = (AnimationState)obj3;
							animationState3.speed = 1f;
						}
					}
					finally
					{
						IDisposable disposable3;
						if ((disposable3 = (enumerator3 as IDisposable)) != null)
						{
							disposable3.Dispose();
						}
					}
					this._currentAnimation.Play();
				}
			}

			public GameObject MainGroup;

			public Image ArmourImage;

			public Image VulnerableImage;

			public Animation ArmourAnimation;

			public Animation VulnerableAnimation;

			private Animation _currentAnimation;

			private bool _currentAnimationIsBuff;

			private bool _nextAnimationIsBuff;
		}

		[Serializable]
		public class HudLifebarRepairData
		{
			private void SetAnimationSpeed(Animation animation, float speed)
			{
				IEnumerator enumerator = animation.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						AnimationState animationState = (AnimationState)obj;
						animationState.speed = speed;
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
			}

			public void Update(float repairValue)
			{
				bool flag = repairValue != 0f;
				this._nextAnimationIsBuff = (repairValue > 0f);
				if (this.Animation.isPlaying)
				{
					bool flag2 = this._nextAnimationIsBuff == this._currentAnimationIsBuff;
					if (flag2)
					{
						float speed = -Mathf.Clamp(this.MaxAnimationSpeed * repairValue, -this.MaxAnimationSpeed, this.MaxAnimationSpeed);
						this.SetAnimationSpeed(this.RepairAnimation, speed);
					}
					flag = (flag && flag2);
					if (flag)
					{
						IEnumerator enumerator = this.Animation.GetEnumerator();
						try
						{
							while (enumerator.MoveNext())
							{
								object obj = enumerator.Current;
								AnimationState animationState = (AnimationState)obj;
								if (animationState.normalizedTime > 0.5f)
								{
									animationState.normalizedTime = 0.5f;
									animationState.speed = 0f;
								}
							}
						}
						finally
						{
							IDisposable disposable;
							if ((disposable = (enumerator as IDisposable)) != null)
							{
								disposable.Dispose();
							}
						}
					}
					else
					{
						this.SetAnimationSpeed(this.Animation, 1f);
					}
					return;
				}
				this.MainGroup.SetActive(flag);
				if (flag)
				{
					float speed2 = -Mathf.Clamp(this.MaxAnimationSpeed * repairValue, -this.MaxAnimationSpeed, this.MaxAnimationSpeed);
					this.SetAnimationSpeed(this.RepairAnimation, speed2);
					this._currentAnimationIsBuff = this._nextAnimationIsBuff;
					this.RepairImage.sprite = ((!this._currentAnimationIsBuff) ? this.DebuffSprite : this.BuffSprite);
					this.SetAnimationSpeed(this.Animation, 1f);
					this.Animation.Play();
					this.RepairAnimation.Play();
				}
			}

			public GameObject MainGroup;

			public Image RepairImage;

			public Animation Animation;

			public Animation RepairAnimation;

			public float MaxAnimationSpeed = 3f;

			[Header("[Assets]")]
			public Sprite BuffSprite;

			public Sprite DebuffSprite;

			private bool _currentAnimationIsBuff;

			private bool _nextAnimationIsBuff;
		}

		[Serializable]
		private struct HudLifebarCooldownIndicatorData
		{
			public void SetVisibility(bool isVisible)
			{
				this.MainGroup.SetActive(isVisible);
			}

			public GameObject MainGroup;

			public Image Gadget0FillImage;

			public Image Gadget1FillImage;

			public Image GadgetBoostFillImage;

			public Animation Gadget0GlowAnimation;

			public Animation Gadget1GlowAnimation;

			public Animation GadgetBoostGlowAnimation;
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
