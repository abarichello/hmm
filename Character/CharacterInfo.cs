using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Audio;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Combat.GadgetScript;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Character
{
	public class CharacterInfo : GameHubScriptableObject, IContent, ISerializationCallbackReceiver
	{
		public int ContentId
		{
			get
			{
				return this.CharacterId;
			}
			set
			{
				this.CharacterId = value;
			}
		}

		public override string ToString()
		{
			return string.Format("CharacterId={0}", this.CharacterId);
		}

		public string LocalizedBotName
		{
			get
			{
				return Language.Get(this.BotName, TranslationSheets.CharactersBaseInfo);
			}
		}

		public string LocalizedName
		{
			get
			{
				return Language.Get(this.DraftName, TranslationSheets.CharactersBaseInfo);
			}
		}

		public string LocalizedPhrase
		{
			get
			{
				return Language.Get(this.DraftPhrase, TranslationSheets.CharactersBaseInfo);
			}
		}

		public string LocalizedPickPhrase
		{
			get
			{
				return Language.Get(this.DraftPickPhrase, TranslationSheets.PickMode);
			}
		}

		public Guid CharacterItemTypeGuid
		{
			get
			{
				return this._characterItemTypeGuid;
			}
		}

		public string GetRoleTranslation()
		{
			switch (this.Role)
			{
			case CharacterInfo.DriverRoleKind.Support:
				return Language.Get("CLASS_SUPPORT", TranslationSheets.CharactersBaseInfo);
			case CharacterInfo.DriverRoleKind.Carrier:
				return Language.Get("CLASS_TRANSPORTER", TranslationSheets.CharactersBaseInfo);
			case CharacterInfo.DriverRoleKind.Tackler:
				return Language.Get("CLASS_INTERCEPTOR", TranslationSheets.CharactersBaseInfo);
			default:
				return "Invalid Class";
			}
		}

		public CharacterInfo.Difficulty GetDifficultyKind()
		{
			return (CharacterInfo.Difficulty)this.Dificult;
		}

		public string GetDifficultTranslatedText()
		{
			switch (this.GetDifficultyKind())
			{
			case CharacterInfo.Difficulty.DifficultyLevel1:
				return Language.Get("DIFFICULTY_LEVEL_1", TranslationSheets.CharactersBaseInfo);
			case CharacterInfo.Difficulty.DifficultyLevel2:
				return Language.Get("DIFFICULTY_LEVEL_2", TranslationSheets.CharactersBaseInfo);
			case CharacterInfo.Difficulty.DifficultyLevel3:
				return Language.Get("DIFFICULTY_LEVEL_3", TranslationSheets.CharactersBaseInfo);
			case CharacterInfo.Difficulty.DifficultyLevel4:
				return Language.Get("DIFFICULTY_LEVEL_4", TranslationSheets.CharactersBaseInfo);
			case CharacterInfo.Difficulty.DifficultyLevel5:
				return Language.Get("DIFFICULTY_LEVEL_5", TranslationSheets.CharactersBaseInfo);
			default:
				return Language.Get("DIFFICULTY_LEVEL_1", TranslationSheets.CharactersBaseInfo);
			}
		}

		public void PrecacheGadgets(int arenaIndex)
		{
			for (int i = 0; i < this.CustomGadgets.Length; i++)
			{
				this.CustomGadgets[i].PrecacheAssets();
			}
			if (this.CustomGadget0)
			{
				this.CustomGadget0.PreCacheAssets();
			}
			if (this.CustomGadget1)
			{
				this.CustomGadget1.PreCacheAssets();
			}
			if (this.CustomGadget2)
			{
				this.CustomGadget2.PreCacheAssets();
			}
			if (this.BoostGadget)
			{
				this.BoostGadget.PreCacheAssets();
			}
			if (this.GenericGadget)
			{
				this.GenericGadget.PreCacheAssets();
			}
			if (this.PassiveGadget)
			{
				this.PassiveGadget.PreCacheAssets();
			}
			if (this.TrailGadget)
			{
				this.TrailGadget.PreCacheAssets();
			}
			if (this.OutOfCombatGadget)
			{
				this.OutOfCombatGadget.PreCacheAssets();
			}
			if (this.DmgUpgrade)
			{
				this.DmgUpgrade.PreCacheAssets();
			}
			if (this.HPUpgrade)
			{
				this.HPUpgrade.PreCacheAssets();
			}
			if (this.EPUpgrade)
			{
				this.EPUpgrade.PreCacheAssets();
			}
			if (this.BombGadget)
			{
				this.BombGadget.PreCacheAssets();
			}
			if (this.RespawnGadget)
			{
				this.RespawnGadget.PreCacheAssets();
			}
			GadgetInfo currentTakeoffGadget = this.GetCurrentTakeoffGadget(arenaIndex);
			if (currentTakeoffGadget)
			{
				currentTakeoffGadget.PreCacheAssets();
			}
			if (this.KillGadget)
			{
				this.KillGadget.PreCacheAssets();
			}
			if (this.BombExplosionGadget)
			{
				this.BombExplosionGadget.PreCacheAssets();
			}
			if (this.SprayGadget)
			{
				this.SprayGadget.PreCacheAssets();
			}
			if (this.GridHighlightGadget)
			{
				this.GridHighlightGadget.PreCacheAssets();
			}
		}

		public GadgetInfo GetCurrentTakeoffGadget(int arenaIndex)
		{
			if (this.TakeoffGadgets == null || arenaIndex < 0 || arenaIndex >= this.TakeoffGadgets.Length)
			{
				return null;
			}
			UnityEngine.Debug.Log("Takeoff=" + arenaIndex);
			return this.TakeoffGadgets[arenaIndex];
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			if (!string.IsNullOrEmpty(this._characterItemType))
			{
				this._characterItemTypeGuid = new Guid(this._characterItemType);
			}
		}

		[ScriptId]
		public int CharacterId;

		public bool IsAnAvailableBot;

		public bool CanBePicked = true;

		public CharacterTarget Character;

		public string ReleaseDate;

		public string BotName;

		public string BIName;

		public string DraftName;

		public string URLName;

		public string DraftPhrase;

		public string DraftPickPhrase;

		public string Asset;

		public int characterMusicID;

		public BotAIGoal GoalList;

		public BotAIGoal GoalEasy;

		public BotAIGoal GoalMedium;

		public BotAIGoal GoalHard;

		public BotAIGadgetList GadgetList;

		public CarInfo Car;

		public CarCollider Collider;

		public CarCollider[] ExtraColliders;

		public CombatInfo Combat;

		public CarIndicator.CustomConfig IndicatorConfig;

		[ItemType(typeof(ItemTypeScriptableObject))]
		[SerializeField]
		private string _characterItemType;

		private Guid _characterItemTypeGuid;

		public CharacterInfo.DriverRoleKind Role;

		public int PreferedGridPosition;

		public int Dificult;

		public bool HasPassive = true;

		public GadgetInfo CustomGadget0;

		public GadgetInfo CustomGadget1;

		public GadgetInfo CustomGadget2;

		public GadgetInfo GenericGadget;

		public GadgetInfo BoostGadget;

		public GadgetInfo PassiveGadget;

		public GadgetInfo TrailGadget;

		public GadgetInfo OutOfCombatGadget;

		public GadgetInfo DmgUpgrade;

		public GadgetInfo HPUpgrade;

		public GadgetInfo EPUpgrade;

		public GadgetInfo BombGadget;

		public GadgetInfo RespawnGadget;

		public GadgetInfo[] TakeoffGadgets;

		public GadgetInfo KillGadget;

		public GadgetInfo BombExplosionGadget;

		public GadgetInfo SprayGadget;

		public GadgetInfo GridHighlightGadget;

		public CombatGadget[] CustomGadgets;

		public bool ApplyOnGadget0UsedModifiers = true;

		public bool ApplyOnGadget1UsedModifiers;

		public bool ApplyOnGadget2UsedModifiers;

		public ModifierInfo[] OnGadgetUsedModifiers = new ModifierInfo[]
		{
			new ModifierInfo
			{
				Amount = -0.34f,
				Attribute = AttributeBuffKind.ObsoleteSpeedMax,
				FriendlyFire = true,
				IsPercent = true,
				LifeTime = 1.5f
			}
		};

		[Header("[Audio]")]
		public CarAudioData carAudio;

		public VoiceOver voiceOver;

		[Header("[Pick Mode Stats]")]
		public CharacterInfo.PickModeStats PickModeStatsInfo;

		public enum DriverRoleKind
		{
			Support,
			Carrier,
			Tackler,
			CarrierSupport,
			TacklerSupport,
			CarrierTackler,
			SupportCarrierTackler
		}

		public enum Difficulty
		{
			DifficultyLevel1 = 1,
			DifficultyLevel2,
			DifficultyLevel3,
			DifficultyLevel4,
			DifficultyLevel5
		}

		public enum StatCounterKind
		{
			None,
			Number,
			Float,
			Speed,
			Time
		}

		[Serializable]
		public struct PickModeStats
		{
			[Range(0f, 1f)]
			public float Durability;

			[Range(0f, 1f)]
			public float Repair;

			[Range(0f, 1f)]
			public float Control;

			[Range(0f, 1f)]
			public float Damage;

			[Range(0f, 1f)]
			public float Mobility;
		}
	}
}
