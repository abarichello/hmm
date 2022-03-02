using System;
using System.Collections;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using Assets.Standard_Assets.Scripts.HMM.GameStates.MainMenu.Progression;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.EndMatch.Battlepass
{
	public class EndMatchBattlepassLevelUpWindow : GameHubBehaviour, IAnimationSync
	{
		public void Setup(int currentLevel, bool isPremium, ProgressionLevel[] progress, bool useMockData)
		{
			this._levelUpData = ((!useMockData) ? this.SetupData(progress) : this._LevelUpDataTest);
			this._isPremium = isPremium;
			if (currentLevel == 0 || currentLevel == this._maxLevelUp)
			{
				int num = (currentLevel != 0) ? 1 : -1;
				this._current_level_border.transform.localPosition = new Vector3(this._current_level_border.transform.localPosition.x + this._gridReward.cellWidth * (float)num, this._current_level_border.transform.localPosition.y, this._current_level_border.transform.localPosition.z);
			}
			this.ConfigureSlotLevelType(currentLevel);
			this._premiumLockTicket.sprite2D = ((!this._isPremium) ? this._rewardLocksConfig.PremiumContextLockLocked : this._rewardLocksConfig.PremiumContextLockUnlocked);
			this._premiumMask.SetActive(!this._isPremium);
			this._waitTimeForSyncHeaderAnimation = this._delayAnimLevelUpWindowIn + this._levelUpInAnimation.GetClip("LevelUp_in").length;
		}

		private void ConfigureSlotLevelType(int currentLevel)
		{
			List<Transform> childList = this._gridReward.GetChildList();
			int num = (currentLevel != 0) ? (currentLevel - 1) : 0;
			for (int i = 0; i < childList.Count; i++)
			{
				EndMatchBattlepassRewardSlot component = childList[i].GetComponent<EndMatchBattlepassRewardSlot>();
				EndMatchBattlepassLevelUpWindow.LevelUpData levelUpDataFromLevel = this.GetLevelUpDataFromLevel(num);
				if (levelUpDataFromLevel.Level > currentLevel)
				{
					levelUpDataFromLevel.SlotLevelType = EndMatchBattlepassLevelUpWindow.SlotLevelType.Locked;
				}
				else if (levelUpDataFromLevel.Level == currentLevel)
				{
					levelUpDataFromLevel.SlotLevelType = EndMatchBattlepassLevelUpWindow.SlotLevelType.Current;
				}
				else
				{
					levelUpDataFromLevel.SlotLevelType = EndMatchBattlepassLevelUpWindow.SlotLevelType.Unlocked;
				}
				levelUpDataFromLevel.IsPremium = this._isPremium;
				component.InitialSetup(levelUpDataFromLevel, this._levelInfos, this._border, this._alphaImageConfig, this._rewardLocksConfig);
				num++;
			}
		}

		private void Update()
		{
			if (!this._animateGrid)
			{
				return;
			}
			float num = (Time.time - this._initialAnimationTime) / this._levelUpAnimDuration;
			float pos = Mathf.Lerp(this._initialPostion, this._finalPostion, num);
			EndMatchBattlepassLevelUpWindow.AnimationType currentAnim = this._currentAnim;
			if (currentAnim != EndMatchBattlepassLevelUpWindow.AnimationType.FirstLevel)
			{
				if (currentAnim != EndMatchBattlepassLevelUpWindow.AnimationType.MidLevel)
				{
					if (currentAnim == EndMatchBattlepassLevelUpWindow.AnimationType.LastLevel)
					{
						this.AnimateCurrentRewardBorder(pos, num);
					}
				}
				else
				{
					this.AnimateRewardGrid(pos, num);
				}
			}
			else
			{
				this.AnimateCurrentRewardBorder(pos, num);
			}
		}

		private void AnimateRewardGrid(float pos, float animationProgress)
		{
			this._gridReward.transform.localPosition = new Vector3(pos, this._gridReward.transform.localPosition.y, this._gridReward.transform.localPosition.z);
			if (animationProgress >= 1f)
			{
				this._animateGrid = false;
				this.AnimateUnlockReward();
				this.RepositionRewardGrids();
			}
		}

		private void AnimateCurrentRewardBorder(float pos, float animationProgress)
		{
			this._current_level_border.transform.localPosition = new Vector3(pos, this._current_level_border.transform.localPosition.y, this._current_level_border.transform.localPosition.z);
			this._freeUnlockGlowAnimation.transform.localPosition = new Vector3(pos, this._freeUnlockGlowAnimation.transform.localPosition.y, this._freeUnlockGlowAnimation.transform.localPosition.z);
			this._premiumUnlockGlowAnimation.transform.localPosition = new Vector3(pos, this._premiumUnlockGlowAnimation.transform.localPosition.y, this._premiumUnlockGlowAnimation.transform.localPosition.z);
			if (animationProgress >= 1f)
			{
				this._animateGrid = false;
				this.AnimateUnlockReward();
			}
		}

		private void RepositionRewardGrids()
		{
			List<Transform> childList = this._gridReward.GetChildList();
			Transform transform = childList[childList.Count - 1];
			EndMatchBattlepassLevelUpWindow.LevelUpData levelUpDataFromLevel = this.GetLevelUpDataFromLevel(transform.GetComponent<EndMatchBattlepassRewardSlot>().GetSlotLevel() + 1);
			levelUpDataFromLevel.IsPremium = this._isPremium;
			levelUpDataFromLevel.SlotLevelType = EndMatchBattlepassLevelUpWindow.SlotLevelType.Locked;
			childList[0].localPosition = new Vector3(transform.localPosition.x + this._gridReward.cellWidth, transform.localPosition.y, transform.localPosition.z);
			childList[0].GetComponent<EndMatchBattlepassRewardSlot>().Setup(levelUpDataFromLevel);
		}

		private void AnimateUnlockReward()
		{
			List<Transform> childList = this._gridReward.GetChildList();
			for (int i = 0; i < childList.Count; i++)
			{
				EndMatchBattlepassRewardSlot component = childList[i].GetComponent<EndMatchBattlepassRewardSlot>();
				if (component.GetSlotLevel() == this._currentLevelUp)
				{
					component.PlayUnlockRewardAnimation(this._isPremium);
					component.PlayLevelGlowAnimation();
					component.UpdateSlotInfo(this._isPremium);
					component.UpdateLevelsData(EndMatchBattlepassLevelUpWindow.SlotLevelType.Current, this._levelInfos);
					component.EnableColorFameText();
					if (component.GetFreeSlots().rewardKind != null)
					{
						this._freeUnlockGlowAnimation.Play("glow_level_up_free");
					}
					if (this._isPremium && component.GetPremiumSlots().rewardKind != null)
					{
						this._premiumUnlockGlowAnimation.Play("glow_level_up_premium");
					}
				}
				else if (component.GetSlotLevel() == this._currentLevelUp - 1)
				{
					component.UpdateLevelsData(EndMatchBattlepassLevelUpWindow.SlotLevelType.Unlocked, this._levelInfos);
				}
			}
		}

		private EndMatchBattlepassLevelUpWindow.LevelUpData[] SetupData(ProgressionLevel[] progress)
		{
			List<EndMatchBattlepassLevelUpWindow.LevelUpData> list = new List<EndMatchBattlepassLevelUpWindow.LevelUpData>();
			this._maxLevelUp = progress.Length - 1;
			for (int i = 0; i < progress.Length; i++)
			{
				ProgressionLevel progressionLevel = progress[i];
				this.AddBattlepassRewardItens(progressionLevel, i, list);
			}
			return list.ToArray();
		}

		private void AddBattlepassRewardItens(ProgressionLevel progressionLevel, int level, List<EndMatchBattlepassLevelUpWindow.LevelUpData> listToAdd)
		{
			EndMatchBattlepassLevelUpWindow.LevelUpDataSlot rewardByType = this.GetRewardByType(progressionLevel.FreeRewards.Kind, progressionLevel.FreeRewards);
			EndMatchBattlepassLevelUpWindow.LevelUpDataSlot rewardByType2 = this.GetRewardByType(progressionLevel.PremiumRewards.Kind, progressionLevel.PremiumRewards);
			EndMatchBattlepassLevelUpWindow.LevelUpData item = new EndMatchBattlepassLevelUpWindow.LevelUpData
			{
				FreeSlot = rewardByType,
				Level = level,
				PremiumSlot = rewardByType2
			};
			listToAdd.Add(item);
		}

		private EndMatchBattlepassLevelUpWindow.LevelUpDataSlot GetRewardByType(ProgressionInfo.RewardKind rewardKind, ProgressionReward reward)
		{
			EndMatchBattlepassLevelUpWindow.LevelUpDataSlot result = default(EndMatchBattlepassLevelUpWindow.LevelUpDataSlot);
			switch (rewardKind)
			{
			case 1:
				result = this.GetBattlepassComponentInfo(this._softCoinItemTypeScriptableObject);
				result.FameAmount = int.Parse(reward.Argument);
				break;
			case 2:
			{
				ItemTypeScriptableObject itemTypeReward;
				if (GameHubBehaviour.Hub.InventoryColletion.AllItemTypes.TryGetValue(new Guid(reward.Argument), out itemTypeReward))
				{
					result = this.GetBattlepassComponentInfo(itemTypeReward);
				}
				break;
			}
			case 4:
				result = this.GetBattlepassComponentInfo(this._hardCoinItemTypeScriptableObject);
				result.FameAmount = int.Parse(reward.Argument);
				break;
			}
			result.RewardKind = rewardKind;
			return result;
		}

		private EndMatchBattlepassLevelUpWindow.LevelUpDataSlot GetBattlepassComponentInfo(ItemTypeScriptableObject itemTypeReward)
		{
			EndMatchBattlepassLevelUpWindow.LevelUpDataSlot result = default(EndMatchBattlepassLevelUpWindow.LevelUpDataSlot);
			ItemTypeComponent itemTypeComponent;
			if (!itemTypeReward.GetComponentByEnum(ItemTypeComponent.Type.Battlepass, out itemTypeComponent))
			{
				return result;
			}
			BattlepassItemTypeComponent battlepassItemTypeComponent = (BattlepassItemTypeComponent)itemTypeComponent;
			result.RewardIcon = battlepassItemTypeComponent.IconAssetName;
			return result;
		}

		public void DoLevelUp(int level)
		{
			this._currentLevelUp = level;
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(true);
				base.StartCoroutine(this.DelayForLevelUpWindowInAnimation());
				return;
			}
			this._waitTimeForSyncHeaderAnimation = this._delayAnimationHeader;
			base.StartCoroutine(this.WaitToPlayAnimation());
		}

		private void OpenLevelUpWindow()
		{
			this._levelUpInAnimation.Play("LevelUp_in");
		}

		private IEnumerator DelayForLevelUpWindowInAnimation()
		{
			yield return new WaitForSeconds(this._delayAnimLevelUpWindowIn);
			this.OpenLevelUpWindow();
			yield return new WaitForSeconds(this._levelUpInAnimation.GetClip("LevelUp_in").length);
			base.StartCoroutine(this.WaitToPlayAnimation());
			yield break;
		}

		private IEnumerator WaitToPlayAnimation()
		{
			yield return new WaitForSeconds(this._delayBeforeLevelUpAnim);
			this.AnimateOnLevelUp();
			yield break;
		}

		private void AnimateOnLevelUp()
		{
			if (this._currentLevelUp == 1)
			{
				this._initialPostion = this._current_level_border.transform.localPosition.x;
				this._finalPostion = this._current_level_border.transform.localPosition.x + this._gridReward.cellWidth;
				this._currentAnim = EndMatchBattlepassLevelUpWindow.AnimationType.FirstLevel;
			}
			else if (this._currentLevelUp == this._maxLevelUp)
			{
				this._initialPostion = this._current_level_border.transform.localPosition.x;
				this._finalPostion = this._current_level_border.transform.localPosition.x + this._gridReward.cellWidth;
				this._currentAnim = EndMatchBattlepassLevelUpWindow.AnimationType.LastLevel;
			}
			else
			{
				this._initialPostion = this._gridReward.transform.localPosition.x;
				this._finalPostion = this._gridReward.transform.localPosition.x - this._gridReward.cellWidth;
				this._currentAnim = EndMatchBattlepassLevelUpWindow.AnimationType.MidLevel;
			}
			this._initialAnimationTime = Time.time;
			this._animateGrid = true;
		}

		private EndMatchBattlepassLevelUpWindow.LevelUpData GetLevelUpDataFromLevel(int level)
		{
			for (int i = 0; i < this._levelUpData.Length; i++)
			{
				EndMatchBattlepassLevelUpWindow.LevelUpData result = this._levelUpData[i];
				if (result.Level == level)
				{
					return result;
				}
			}
			return this._levelUpData[0];
		}

		public float GetWaitTimeToPlay()
		{
			return this._waitTimeForSyncHeaderAnimation;
		}

		[SerializeField]
		private UIGrid _gridReward;

		[SerializeField]
		private UITexture _current_level_border;

		[SerializeField]
		private Animation _levelUpInAnimation;

		[SerializeField]
		private Animation _freeUnlockGlowAnimation;

		[SerializeField]
		private Animation _premiumUnlockGlowAnimation;

		[SerializeField]
		private UI2DSprite _premiumLockTicket;

		[SerializeField]
		private GameObject _premiumMask;

		[SerializeField]
		private float _levelUpAnimDuration = 0.5f;

		[SerializeField]
		private float _delayAnimLevelUpWindowIn = 1f;

		[SerializeField]
		private float _delayBeforeLevelUpAnim = 1f;

		[SerializeField]
		private float _delayAnimationHeader;

		[SerializeField]
		private ItemTypeScriptableObject _softCoinItemTypeScriptableObject;

		[SerializeField]
		private ItemTypeScriptableObject _hardCoinItemTypeScriptableObject;

		[Header("[Configs]")]
		[SerializeField]
		private EndMatchBattlepassLevelUpWindow.LevelViewInfo[] _levelInfos;

		[SerializeField]
		private EndMatchBattlepassLevelUpWindow.RewardViewBorderInfo _border;

		[SerializeField]
		private EndMatchBattlepassLevelUpWindow.AlphaImageConfig _alphaImageConfig;

		[SerializeField]
		private EndMatchBattlepassLevelUpWindow.RewardLocksConfig _rewardLocksConfig;

		[SerializeField]
		private string _fameItemImageName;

		[Header("[HACKS]")]
		[SerializeField]
		private bool _isPremium;

		[SerializeField]
		private int _maxLevelUp;

		[SerializeField]
		private EndMatchBattlepassLevelUpWindow.LevelUpData[] _LevelUpDataTest;

		private bool _animateGrid;

		private float _initialPostion;

		private float _finalPostion;

		private float _initialAnimationTime;

		private int _currentLevelUp;

		private float _waitTimeForSyncHeaderAnimation;

		private EndMatchBattlepassLevelUpWindow.LevelUpData[] _levelUpData;

		private EndMatchBattlepassLevelUpWindow.AnimationType _currentAnim;

		[Serializable]
		public struct LevelViewInfo
		{
			public EndMatchBattlepassLevelUpWindow.SlotLevelType SlotLevelType;

			public Color TextColor;

			public int FontSize;

			public UILabel.Effect EffectOutline;

			public Texture BgTexture;
		}

		[Serializable]
		public struct RewardViewBorderInfo
		{
			public Color LockedFreeColor;

			public Color LockedPremiumColor;

			public Texture LockedFreeSprite;

			public Texture LockedPremiumSprite;

			public Color UnlockedFreeColor;

			public Color UnlockedPremiumColor;

			public Texture UnlockedFreeSprite;

			public Texture UnlockedPremiumSprite;

			public Color UnlockedColorWithoutItem;
		}

		[Serializable]
		public struct AlphaImageConfig
		{
			[Range(0f, 1f)]
			public float EnabledIconImageAlpha;

			[Range(0f, 1f)]
			public float DisabledIconImageAlpha;

			public Color FamaColorEnabled;

			public Color FamaColorDisabled;

			public Color FamaOutlineColor;

			public Color HardCointColorEnabled;

			public Color HardCoinColorDisabled;

			public Color HardCoinOutlineColor;
		}

		[Serializable]
		public struct RewardLocksConfig
		{
			public Texture FreeTexture;

			public Texture PremiumLockedTexture;

			public Texture PremiumUnlockableTexture;

			public Sprite PremiumContextLockLocked;

			public Sprite PremiumContextLockUnlocked;
		}

		private enum AnimationType
		{
			FirstLevel,
			MidLevel,
			LastLevel
		}

		public enum SlotLevelType
		{
			Current,
			Locked,
			Unlocked
		}

		[Serializable]
		public struct LevelUpDataSlot
		{
			public string RewardIcon;

			public int FameAmount;

			public ProgressionInfo.RewardKind RewardKind;
		}

		[Serializable]
		public struct LevelUpData
		{
			public int Level;

			public bool IsPremium;

			public EndMatchBattlepassLevelUpWindow.SlotLevelType SlotLevelType;

			public EndMatchBattlepassLevelUpWindow.LevelUpDataSlot FreeSlot;

			public EndMatchBattlepassLevelUpWindow.LevelUpDataSlot PremiumSlot;
		}
	}
}
