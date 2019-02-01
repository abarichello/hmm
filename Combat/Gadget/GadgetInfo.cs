using System;
using System.Collections.Generic;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class GadgetInfo : GameHubScriptableObject, IContent
	{
		public void OnEnable()
		{
			GadgetInfo.Gadgets[this.GadgetId] = this;
			if (this.Effects == null)
			{
				return;
			}
			for (int i = 0; i < this.Effects.Length; i++)
			{
				FXInfo fxinfo = this.Effects[i];
				GadgetInfo.EffectMap[fxinfo.EffectId] = fxinfo;
			}
		}

		public void OnDisable()
		{
			if (Application.isEditor)
			{
				return;
			}
			GadgetInfo.Gadgets.Remove(this.GadgetId);
			if (this.Effects == null)
			{
				return;
			}
			for (int i = 0; i < this.Effects.Length; i++)
			{
				FXInfo fxinfo = this.Effects[i];
				GadgetInfo.EffectMap.Remove(fxinfo.EffectId);
			}
		}

		public int ContentId
		{
			get
			{
				return this.GadgetId;
			}
			set
			{
				this.GadgetId = value;
			}
		}

		public string LocalizedName
		{
			get
			{
				this.CheckTranslationSheet();
				return Language.Get(this.DraftName, this._translationSheet);
			}
		}

		private void CheckTranslationSheet()
		{
			if (this._translationSheet != TranslationSheets.All)
			{
				return;
			}
			this._translationSheet = ((!this.DraftName.StartsWith("SPONSOR")) ? TranslationSheets.CharactersMatchInfo : TranslationSheets.Sponsors);
		}

		public string LocalizedDescription
		{
			get
			{
				this.CheckTranslationSheet();
				return Language.Get(this.DraftDescription, this._translationSheet);
			}
		}

		public string LocalizedCooldownDescription
		{
			get
			{
				this.CheckTranslationSheet();
				return (!string.IsNullOrEmpty(this.DraftCooldownDescription)) ? Language.Get(this.DraftCooldownDescription, this._translationSheet) : string.Empty;
			}
		}

		public void PreCacheBaseFX(FXInfo fxInfo)
		{
			if (string.IsNullOrEmpty(fxInfo.Effect))
			{
				UnityEngine.Debug.LogError(string.Format("GadgetInfo::PreCacheBaseFX - fxInfo.Effect is null or empty - Name: {0}.{1}", this.Name, base.name), this);
				return;
			}
			GameHubScriptableObject.Hub.Resources.PreCachePrefab(fxInfo.Effect, fxInfo.EffectPreCacheCount);
		}

		public void PreCacheNAssets(int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				this.PreCacheAssets();
			}
		}

		public void PreCacheAssets()
		{
			for (int i = 0; i < this.Effects.Length; i++)
			{
				FXInfo fxInfo = this.Effects[i];
				this.PreCacheBaseFX(fxInfo);
			}
			for (int j = 0; j < this.ModifierFeedbacks.Length; j++)
			{
				ModifierFeedbackInfo modifierFeedbackInfo = this.ModifierFeedbacks[j];
				GameHubScriptableObject.Hub.Resources.PreCachePrefab(modifierFeedbackInfo.Name, modifierFeedbackInfo.EffectPreCacheCount);
			}
		}

		public virtual List<float> GetStats(int index)
		{
			return new List<float>();
		}

		public virtual string[] GetStatStrings()
		{
			return new string[0];
		}

		public virtual ModifierInfo GetInfo(int index)
		{
			return null;
		}

		public ModifierInfo GetInfo(ModifierInfo[] modifierInfos, int index)
		{
			return (modifierInfos.Length > index) ? modifierInfos[index] : null;
		}

		protected List<float> GetStatListModifierAmount(ModifierInfo[] modifierInfos, int index)
		{
			if (modifierInfos.Length == 0)
			{
				return new List<float>();
			}
			if (modifierInfos.Length <= index)
			{
				string strMessage = string.Format("Gadget:{0} \nClass: {1} \nMethod: {2} \nIndex: {3} is out of range for {4})", new object[]
				{
					base.name,
					base.GetType(),
					"GetStatListModifierAmount",
					index,
					modifierInfos.GetType()
				});
				HeavyMetalMachines.Utils.Debug.Assert(false, strMessage, HeavyMetalMachines.Utils.Debug.TargetTeam.All);
				return new List<float>();
			}
			ModifierInfo modifierInfo = modifierInfos[index];
			return this.GetStatListModifier(modifierInfo.Amount, modifierInfo.AmountUpgrade);
		}

		protected List<float> GetStatListModifierAmountDif(ModifierInfo[] modifierInfos, int index)
		{
			List<float> statListModifierAmount = this.GetStatListModifierAmount(modifierInfos, index);
			for (int i = 0; i < statListModifierAmount.Count; i++)
			{
				List<float> list;
				int index2;
				(list = statListModifierAmount)[index2 = i] = list[index2] - modifierInfos[index].Amount;
			}
			return statListModifierAmount;
		}

		protected List<float> GetStatListModifierAmountConvoluted(ModifierInfo[] modifierInfos, int index, float multiplier)
		{
			if (modifierInfos.Length == 0)
			{
				return new List<float>();
			}
			if (modifierInfos.Length <= index)
			{
				string strMessage = string.Format("Gadget:{0} \nClass: {1} \nMethod: {2} \nIndex: {3} is out of range for {4})", new object[]
				{
					base.GetType(),
					"GetStatListModifierAmountConvoluted",
					index,
					modifierInfos.GetType()
				});
				HeavyMetalMachines.Utils.Debug.Assert(false, strMessage, HeavyMetalMachines.Utils.Debug.TargetTeam.All);
				return new List<float>();
			}
			ModifierInfo modifierInfo = modifierInfos[index];
			return this.GetStatListModifierConvoluted(modifierInfo.Amount, modifierInfo.AmountUpgrade, multiplier);
		}

		protected List<float> GetStatListModifierAmountConvolutedDif(ModifierInfo[] modifierInfos, int index, float multiplier)
		{
			List<float> statListModifierAmountConvoluted = this.GetStatListModifierAmountConvoluted(modifierInfos, index, multiplier);
			for (int i = 0; i < statListModifierAmountConvoluted.Count; i++)
			{
				List<float> list;
				int index2;
				(list = statListModifierAmountConvoluted)[index2 = i] = list[index2] - modifierInfos[index].Amount * multiplier;
			}
			return statListModifierAmountConvoluted;
		}

		protected List<float> GetStatListModifierAmountPerSecond(ModifierInfo[] modifierInfos, int index)
		{
			if (modifierInfos.Length == 0)
			{
				return new List<float>();
			}
			if (modifierInfos.Length <= index)
			{
				string strMessage = string.Format("Gadget:{0} \nClass: {1} \nMethod: {2} \nIndex: {3} is out of range for {4})", new object[]
				{
					base.GetType(),
					"GetStatListModifierAmountPerSecond",
					index,
					modifierInfos.GetType()
				});
				HeavyMetalMachines.Utils.Debug.Assert(false, strMessage, HeavyMetalMachines.Utils.Debug.TargetTeam.All);
				return new List<float>();
			}
			ModifierInfo modifierInfo = modifierInfos[index];
			return this.GetStatListModifierConvoluted(modifierInfo.Amount, modifierInfo.AmountUpgrade, 1f / modifierInfo.TickDelta);
		}

		protected List<float> GetStatListModifierAmountPerSecondDif(ModifierInfo[] modifierInfos, int index)
		{
			List<float> statListModifierAmountPerSecond = this.GetStatListModifierAmountPerSecond(modifierInfos, index);
			for (int i = 0; i < statListModifierAmountPerSecond.Count; i++)
			{
				List<float> list;
				int index2;
				(list = statListModifierAmountPerSecond)[index2 = i] = list[index2] - modifierInfos[index].Amount;
			}
			return statListModifierAmountPerSecond;
		}

		protected List<float> GetStatListModifierLifeTime(ModifierInfo[] modifierInfos, int index)
		{
			if (modifierInfos.Length == 0)
			{
				return new List<float>();
			}
			if (modifierInfos.Length <= index)
			{
				string strMessage = string.Format("Gadget:{0} \nClass: {1} \nMethod: {2} \nIndex: {3} is out of range for {4})", new object[]
				{
					base.name,
					base.GetType(),
					"GetStatListModifierLifeTime",
					index,
					modifierInfos.GetType()
				});
				HeavyMetalMachines.Utils.Debug.Assert(false, strMessage, HeavyMetalMachines.Utils.Debug.TargetTeam.All);
				return new List<float>();
			}
			ModifierInfo modifierInfo = modifierInfos[index];
			return this.GetStatListModifier(modifierInfo.LifeTime, modifierInfo.LifeTimeUpgrade);
		}

		protected List<float> GetStatListModifierLifeTimeDif(ModifierInfo[] modifierInfos, int index)
		{
			List<float> statListModifierLifeTime = this.GetStatListModifierLifeTime(modifierInfos, index);
			for (int i = 0; i < statListModifierLifeTime.Count; i++)
			{
				List<float> list;
				int index2;
				(list = statListModifierLifeTime)[index2 = i] = list[index2] - modifierInfos[index].LifeTime;
			}
			return statListModifierLifeTime;
		}

		protected List<float> GetStatListModifier(float baseValue, string upgradeName)
		{
			List<float> list = new List<float>();
			list.Add(Mathf.Abs(baseValue));
			if (upgradeName != string.Empty)
			{
				UpgradeableValue upgradeableValue = Array.Find<UpgradeableValue>(this.UpgradesValues, (UpgradeableValue x) => x.Name == upgradeName);
				if (upgradeableValue != null)
				{
					for (int i = 0; i < upgradeableValue.Values.Length; i++)
					{
						float f = upgradeableValue.Values[i];
						list.Add(Mathf.Abs(f));
					}
				}
			}
			return list;
		}

		protected List<float> GetStatListModifierDif(float baseValue, string upgradeName)
		{
			List<float> statListModifier = this.GetStatListModifier(baseValue, upgradeName);
			for (int i = 0; i < statListModifier.Count; i++)
			{
				List<float> list;
				int index;
				(list = statListModifier)[index = i] = list[index] - baseValue;
			}
			return statListModifier;
		}

		protected List<float> GetStatListModifierConvoluted(float baseValue, string upgradeName, float multiplier)
		{
			List<float> list = new List<float>();
			list.Add(Mathf.Abs(baseValue * multiplier));
			if (upgradeName != string.Empty)
			{
				UpgradeableValue upgradeableValue = Array.Find<UpgradeableValue>(this.UpgradesValues, (UpgradeableValue x) => x.Name == upgradeName);
				if (upgradeableValue != null)
				{
					for (int i = 0; i < upgradeableValue.Values.Length; i++)
					{
						float num = upgradeableValue.Values[i];
						list.Add(Mathf.Abs(num * multiplier));
					}
				}
			}
			return list;
		}

		protected List<float> GetStatListSingleValue(float value)
		{
			return new List<float>(1)
			{
				Mathf.Abs(value)
			};
		}

		public virtual Type GadgetType()
		{
			return typeof(GadgetBehaviour);
		}

		public static readonly Dictionary<int, GadgetInfo> Gadgets = new Dictionary<int, GadgetInfo>();

		public static readonly Dictionary<int, FXInfo> EffectMap = new Dictionary<int, FXInfo>();

		[ScriptId]
		public int GadgetId;

		[NonSerialized]
		public GadgetSlot GadgetSlot;

		public string Name;

		public string DraftName;

		private TranslationSheets _translationSheet;

		public string DraftDescription;

		public string DraftCooldownDescription;

		[FlagDrawer(typeof(GadgetNatureKind))]
		public GadgetNatureKind Nature;

		public GadgetKind Kind;

		public bool AlwaysPressed;

		[Tooltip("When not pressing (or disarmed), the Effects are not going to be destroyed (to be used with AlwaysPressed)")]
		public bool DoNotDestroyOnRelease;

		public SpawnController.StateType SpawnStateTypeToRun = SpawnController.StateType.Spawned;

		public JokerBarKind UseJokerBar;

		public string GenericGadgetIconName;

		public int Price;

		[Header("Pressed")]
		public float MinimumUsageTime;

		[Header("Overheat Rate = Pct/Sec")]
		public float OverheatHeatRate;

		public string OverheatHeatRateUpgrade;

		public float ActivationOverheatHeatCost;

		public string ActivationOverheatHeatCostUpgrade;

		public float OverheatDelayBeforeCooling;

		public string OverheatDelayBeforeCoolingUpgrade;

		public float OverheatCoolingRate;

		public string OverheatCoolingRateUpgrade;

		public float OverheatUnblockRate;

		public string OverheatUnblockRateUpgrade;

		public float Cooldown;

		public string CooldownUpgrade;

		[Tooltip("New gadget system - Keeps the gadget disabled while the player has less EP than this. The amount consumed when the gadget is activated is configured in ActivationCost")]
		public int EpRequiredToActivate;

		public int ActivationCost;

		public string ActivationCostUpgrade;

		public int ActivatedCost;

		public string ActivatedCostUpgrade;

		public float LifeTime;

		public string LifeTimeUpgrade;

		public float DrainLife;

		public string DrainLifeUpgrade;

		public float DrainRepair;

		public string DrainRepairUpgrade;

		public float TmpSpecialDamageCharge;

		public string TmpSpecialDamageChargeUpgrade;

		public float TmpSpecialRepairCharge;

		public string TmpSpecialRepairChargeUpgrade;

		[Tooltip("Usualy it's the effect distance to be travelled")]
		public float Range;

		public string RangeUpgrade;

		[Tooltip("Usualy it's the effect radius to apply area damage")]
		public float Radius;

		public string RadiusUpgrade;

		public float WarmupSeconds;

		public FXInfo WarmupEffect;

		public ModifierInfo[] WarmupDamage;

		public FXInfo WarmupEffectExtra;

		public ModifierInfo[] WarmupDamageExtra;

		public FXInfo PredictionEffect;

		public bool TurnOffOutOfCombat = true;

		[Tooltip("Should it disable the trail gadget (passive boost) when used?")]
		public bool TurnOffTrailGadget;

		public bool FireEffectOnGadgetUpgrade;

		public bool FireExtraEffectOnAction;

		public GadgetSlot OnActionOfGadget;

		public CombatObject.ActionKind ActionKind;

		public UpgradeableValue[] UpgradesValues;

		public UpgradeInfo[] Upgrades;

		public string DefaultInstance;

		public InvisibleUpgradeInfo[] InvisibleUpgrades;

		[ReadOnly]
		[Header("##### AutoFilled, don't touch #####")]
		public FXInfo[] Effects;

		[ReadOnly]
		[Header("##### AutoFilled, don't touch #####")]
		public ModifierFeedbackInfo[] ModifierFeedbacks;

		public ModifierInfo[] Damage;

		public FXInfo Effect;

		public FXInfo ExtraEffect;

		public ModifierInfo[] ExtraModifier;

		public int ExtraEffectDelayMillis;

		public float ExtraLifeTime;

		public string ExtraLifeTimeUpgrade;

		public bool FireNormalAndExtraEffectsTogether;

		public string FireNormalAndExtraEffectsTogetherUpgrade;

		public bool ReplaceFireNormalToFireExtra;

		public string ReplaceFireNormalToFireExtraUpgrade;

		public ModifierFeedbackInfo LifeStealFeedback;

		public ModifierFeedbackInfo DrainRepairFeedback;

		public ModifierFeedbackInfo TmpSpecialChargeFeedback;

		[Tooltip("Will destroy effect on second click. Only work with GadgetKind.Instant")]
		public bool DestroyOnSecondClick;

		[Tooltip("Will fire ExtraEffect on second click. Only work with GadgetKind.Instant")]
		public bool FireExtraOnSecondClick;

		public bool FireExtraOnEffectDeathOnlyIfTargetIdIsValid;

		public BaseFX.EDestroyReason FireExtraOnEffectDeathOnlyOnReason = BaseFX.EDestroyReason.None;

		[Tooltip("Will fire extra effect on effect death")]
		public bool UseEffectDeathPosition;

		[Tooltip("Will fire extra effect with targetId from removeData.TargetId")]
		public bool UseTargetIdFromRemoveData = true;

		public bool FireExtraOnEffectDeath;

		public string FireExtraOnEffectDeathUpgrade;

		[Tooltip("Aborts Extra Effect On Effect Death if the gadget owner is dead")]
		public bool AbortFireExtraOnEffectDeathWhenCombatIsDead;

		[Tooltip("Will fire normal effect with specific Target")]
		public bool FireEffectOnSpecificTarget;

		public string FireEffectOnSpecificTargetUpgrade;

		public bool FocusOnBombCarrier;

		public GadgetInfo.SpecificTarget ChosenTarget;

		public string ChosenTargetUpgrade;

		public bool FireExtraOnEnterCallback;

		public string FireExtraOnEnterCallbackUpgrade;

		[Header("[Tooltip]")]
		public bool ShowTooltipCooldownInfo = true;

		public bool UseInputTarget;

		[Header("[Charges]")]
		public int ChargeCount;

		public string ChargeCountUpgrade;

		public float ChargeTime;

		public string ChargeTimeUpgrade;

		[Header("[Charged]")]
		public int ChargedTimeMillis;

		private const string AssertString = "Gadget:{0} \nClass: {1} \nMethod: {2} \nIndex: {3} is out of range for {4})";

		public enum SpecificTarget
		{
			None,
			AllPlayers,
			AllAllies,
			AllEnemies,
			NearestAllied,
			Gedget0Target,
			Gedget1Target,
			Gedget2Target,
			LowestHpAllied,
			LowestHpRelativeAllied
		}
	}
}
