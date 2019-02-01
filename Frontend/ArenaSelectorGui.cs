using System;
using System.Collections;
using System.Collections.Generic;
using FMod;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using Pocketverse;
using SharedUtils.Loading;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ArenaSelectorGui : GameHubBehaviour, IDynamicAssetListener<Texture2D>
	{
		protected void Awake()
		{
			this._backgroundTileGameObject.SetActive(false);
			this.CacheArenaSprites();
			this._state = ArenaSelectorGui.ArenaSelectorState.StartLerp;
			this._lerpNormalized = this._centeredLerp;
			this._lerpIndex = new int[this._arenaCardGuiInfos.Length];
			for (int i = 0; i < this._lerpIndex.Length; i++)
			{
				this._lerpIndex[i] = i;
			}
			GameHubBehaviour.Hub.User.OnMatchDataReceived += this.UserOnMatchDataReceived;
		}

		private IEnumerator TestInitCoroutine()
		{
			yield return new WaitForSeconds(2f);
			if (this._testSingleSelection)
			{
				this.PrepareForOnlyOneArenaUnlocked(this._targetArenaTest);
			}
			this._mainInOutAnimation.Play("ArenaSelectorInAnimation");
			this.SuspensionPointsShow();
			this._backgroundTileGameObject.SetActive(true);
			yield break;
		}

		private int TryGetArenaCardGuiIndex(int arenaIndex)
		{
			for (int i = 0; i < this._arenaCardGuiInfos.Length; i++)
			{
				if (this._arenaCardGuiInfos[i].ArenaIndex == arenaIndex)
				{
					return i;
				}
			}
			return -1;
		}

		private ArenaSelectorGui.ArenaCardInfo GetArenaCardInfo(int arenaIndex)
		{
			for (int i = 0; i < this._arenaCardInfos.Length; i++)
			{
				if (this._arenaCardInfos[i].ArenaIndex == arenaIndex)
				{
					return this._arenaCardInfos[i];
				}
			}
			HeavyMetalMachines.Utils.Debug.Assert(false, string.Format("Arena card info not found fo arena index [{0}]", arenaIndex), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
			return this._arenaCardInfos[0];
		}

		protected void MainAnimationInEnd()
		{
			this._visible = true;
			this.PlayAudio(this._spinAudioAsset);
		}

		protected void MainAnimationOutTriggerLoading()
		{
			this.GoToPickMode();
		}

		protected void MainAnimationOutEnd()
		{
			this._visible = false;
		}

		protected void OnDestroy()
		{
			GameHubBehaviour.Hub.User.OnMatchDataReceived -= this.UserOnMatchDataReceived;
		}

		private void UserOnMatchDataReceived(MatchData.MatchState matchState)
		{
			if (matchState == MatchData.MatchState.CharacterPick)
			{
				if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish) && GameHubBehaviour.Hub.ArenaConfig.Arenas[GameHubBehaviour.Hub.Match.ArenaIndex].IsCustomOnly)
				{
					this.GoToPickMode();
					return;
				}
				if (!this._enableArenaSelector || GameHubBehaviour.Hub.Match.Kind == MatchData.MatchKind.Custom)
				{
					this.GoToPickMode();
					return;
				}
				this.Show();
				GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>().MatchAccept.HideAcceptanceWindow(false);
			}
		}

		protected void LateUpdate()
		{
			if (!this._visible)
			{
				return;
			}
			switch (this._state)
			{
			case ArenaSelectorGui.ArenaSelectorState.StartLerp:
				this.LerpCards(this._lerpNormalized);
				this._lerpNormalized += Time.deltaTime * this._startSpeed;
				if (this._lerpNormalized >= 1f)
				{
					this._lerpNormalized = 0f;
				}
				this._startSpeed += this._startAccelModifier;
				if (this._startSpeed > this._speedModifier)
				{
					this._state = ArenaSelectorGui.ArenaSelectorState.MainLerp;
				}
				break;
			case ArenaSelectorGui.ArenaSelectorState.MainLerp:
				this.LerpCards(this._lerpNormalized);
				this._lerpNormalized += Time.deltaTime * this._speedModifier;
				if (this._lerpNormalized >= 1f)
				{
					this._lerpNormalized = 0f;
					if (this._contentIndex != -1)
					{
						this.ShiftContentIndex();
					}
					this.ShiftLerpIndexes();
					int arenaIndex = this._arenaCardGuiInfos[this._lerpIndex[2]].ArenaIndex;
					bool flag = arenaIndex == GameHubBehaviour.Hub.Match.ArenaIndex;
					if (this._speedModifier <= this._mainLerpLowCapSpeed)
					{
						this._state = ArenaSelectorGui.ArenaSelectorState.EndLerp;
					}
				}
				this._speedModifier -= this._slowdownModifier;
				if (this._speedModifier < this._mainLerpLowCapSpeed)
				{
					this._speedModifier = this._mainLerpLowCapSpeed;
				}
				break;
			case ArenaSelectorGui.ArenaSelectorState.EndLerp:
				this.LerpCards(this._endLerpNormalized);
				this._endLerpNormalized += Time.deltaTime * this._speedModifier;
				if (this._endLerpNormalized >= 1f)
				{
					this._endLerpNormalized = 0f;
					if (this._contentIndex != -1)
					{
						this.ShiftContentIndex();
					}
					int arenaIndex2 = this._arenaCardGuiInfos[this._lerpIndex[0]].ArenaIndex;
					bool flag2 = arenaIndex2 == GameHubBehaviour.Hub.Match.ArenaIndex;
					if (this._speedModifier <= 3f && flag2)
					{
						this._speedModifier *= 2f;
						this._state = ArenaSelectorGui.ArenaSelectorState.EndLerpSmoothStop;
					}
					this.ShiftLerpIndexes();
				}
				this._speedModifier -= this._slowdownModifier;
				this._slowdownModifier = Mathf.Max(0.001f, this._slowdownModifier - 0.001f);
				if (this._speedModifier < 1f)
				{
					this._speedModifier = 1f;
				}
				break;
			case ArenaSelectorGui.ArenaSelectorState.EndLerpSmoothStop:
				this.LerpCards(Mathf.Lerp(0f, this._centeredLerp, this._endLerpNormalized));
				this._endLerpNormalized += Time.deltaTime * this._speedModifier;
				this._speedModifier = Mathf.Max(this._endLerpSmoothStopMinSpeedModifier, this._speedModifier - this._slowdownModifier);
				this._slowdownModifier = Mathf.Max(0.008f, this._slowdownModifier - 0.008f);
				if (this._endLerpNormalized > 1f)
				{
					this._endLerpNormalized = 0f;
					this.SelectArena();
					this._state = ArenaSelectorGui.ArenaSelectorState.ArenaSelection;
				}
				break;
			}
		}

		private void LerpCards(float lerpNormalized)
		{
			for (int i = 0; i < this._arenaCardPositions.Length; i++)
			{
				this._arenaCardGuiInfos[this._lerpIndex[i]].MainGroupTransform.localPosition = Vector3.Lerp(this._arenaCardPositions[i].StartPosition, this._arenaCardPositions[i].EndPosition, lerpNormalized);
			}
		}

		private void ShiftContentIndex()
		{
			ArenaSelectorGui.ArenaCardInfo arenaCardInfo = this._arenaCardInfos[this._contentIndex];
			this._arenaCardGuiInfos[this._lerpIndex[this._lerpIndex.Length - 1]].Setup(arenaCardInfo);
			this._contentIndex--;
			if (this._contentIndex < 0)
			{
				this._contentIndex = this._arenaCardInfos.Length - 1;
			}
		}

		private void ShiftLerpIndexes()
		{
			for (int i = 0; i < this._lerpIndex.Length; i++)
			{
				this._lerpIndex[i]--;
				if (this._lerpIndex[i] < 0)
				{
					this._lerpIndex[i] = this._lerpIndex.Length - 1;
				}
			}
		}

		private void SelectArena()
		{
			this.SuspensionPointsHide();
			ArenaSelectorGui.ArenaCardGuiInfo arenaCardGuiInfo = this._arenaCardGuiInfos[this._lerpIndex[1]];
			UIWidget[] componentsInChildren = arenaCardGuiInfo.MainGroupTransform.GetComponentsInChildren<UIWidget>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].depth += 10;
			}
			arenaCardGuiInfo.SelectedAnimation.Play();
			this._titleSelectionAnimation.Play();
			this.TryToShowLockedAnimation(this._arenaCardGuiInfos[this._lerpIndex[0]]);
			this.TryToShowLockedAnimation(this._arenaCardGuiInfos[this._lerpIndex[2]]);
			this.PlayAudio(this._selectionAudioAsset);
			base.StartCoroutine(this.WaitSelection());
		}

		private void TryToShowLockedAnimation(ArenaSelectorGui.ArenaCardGuiInfo arenaCardGuiInfo)
		{
			if (this.GetArenaCardInfo(arenaCardGuiInfo.ArenaIndex).Locked)
			{
				arenaCardGuiInfo.ShowLockedAnimation();
			}
		}

		private void SuspensionPointsShow()
		{
			this._idleTitleAnimation.GetComponent<UIWidget>().UpdateAnchors();
			this._idleTitleAnimation.wrapMode = WrapMode.Loop;
			GUIUtils.PlayAnimation(this._idleTitleAnimation, false, 1f, string.Empty);
		}

		private void SuspensionPointsHide()
		{
			this._idleTitleAnimation.wrapMode = WrapMode.Once;
			GUIUtils.PlayAnimation(this._idleTitleAnimation, true, 4f, string.Empty);
		}

		private IEnumerator WaitSelection()
		{
			yield return new WaitForSeconds(this._selectionWaitBeforeExitInSec);
			this.Hide();
			yield break;
		}

		private void Show()
		{
			bool flag = this.LoadArenasData();
			if (this._disableSpinForOnlyOneArenaUnlocked && flag)
			{
				this.PrepareForOnlyOneArenaUnlocked(GameHubBehaviour.Hub.Match.ArenaIndex);
			}
			this.SuspensionPointsShow();
			this._mainInOutAnimation.Play("ArenaSelectorInAnimation");
			this.PlayAudio(this._inAudioAsset);
			this._backgroundTileGameObject.SetActive(true);
		}

		private void PrepareForOnlyOneArenaUnlocked(int arenaIndex)
		{
			this.LerpCards(0f);
			this._state = ArenaSelectorGui.ArenaSelectorState.EndLerpSmoothStop;
			this._speedModifier = 4f;
			int arenaIndex2 = this._arenaCardGuiInfos[1].ArenaIndex;
			if (arenaIndex2 != arenaIndex)
			{
				int num = this.TryGetArenaCardGuiIndex(arenaIndex);
				if (num != -1)
				{
					this._arenaCardGuiInfos[num].Setup(this.GetArenaCardInfo(arenaIndex2));
				}
				this._arenaCardGuiInfos[1].Setup(this.GetArenaCardInfo(arenaIndex));
			}
		}

		private void Hide()
		{
			this._mainInOutAnimation.Play("ArenaSelectorOutAnimation");
			this.PlayAudio(this._outAudioAsset);
		}

		private void GoToPickMode()
		{
			GameHubBehaviour.Hub.State.GotoState(GameHubBehaviour.Hub.User.HackPickMode, false);
		}

		private int GetNumValidArenas()
		{
			int num = 0;
			for (int i = 0; i < GameHubBehaviour.Hub.ArenaConfig.Arenas.Length; i++)
			{
				if (this.IsValidArena(GameHubBehaviour.Hub.ArenaConfig.Arenas[i]))
				{
					num++;
				}
			}
			return num;
		}

		private bool IsValidArena(GameArenaInfo arenaInfo)
		{
			return !arenaInfo.IsTutorial && !arenaInfo.IsCustomOnly;
		}

		private bool LoadArenasData()
		{
			int numValidArenas = this.GetNumValidArenas();
			this._arenaCardInfos = new ArenaSelectorGui.ArenaCardInfo[numValidArenas];
			int num = 0;
			int i = 0;
			int num2 = 0;
			while (i < GameHubBehaviour.Hub.ArenaConfig.Arenas.Length)
			{
				GameArenaInfo gameArenaInfo = GameHubBehaviour.Hub.ArenaConfig.Arenas[i];
				if (this.IsValidArena(gameArenaInfo))
				{
					this._arenaCardInfos[num2].NameText = Language.Get(gameArenaInfo.DraftName, TranslationSheets.MainMenuGui);
					this._arenaCardInfos[num2].ImageSprite = this._arenaSprites[num2];
					this._arenaCardInfos[num2].ArenaIndex = i;
					int num3 = 1;
					if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
					{
						num3 = GameHubBehaviour.Hub.User.GetTotalPlayerLevel();
						num3++;
					}
					int unlockLevel = GameHubBehaviour.Hub.SharedConfigs.ArenaConfig.Arenas[i].UnlockLevel;
					this._arenaCardInfos[num2].UnlockLevelText = unlockLevel.ToString("0");
					bool flag = num3 < unlockLevel;
					this._arenaCardInfos[num2].Locked = flag;
					if (flag)
					{
						num++;
					}
					num2++;
				}
				i++;
			}
			if (numValidArenas == 1)
			{
				Array.Resize<ArenaSelectorGui.ArenaCardInfo>(ref this._arenaCardInfos, 3);
				this._arenaCardInfos[1] = (this._arenaCardInfos[2] = this._arenaCardInfos[0]);
			}
			if (numValidArenas == 2)
			{
				Array.Resize<ArenaSelectorGui.ArenaCardInfo>(ref this._arenaCardInfos, 4);
				this._arenaCardInfos[2] = this._arenaCardInfos[0];
				this._arenaCardInfos[3] = this._arenaCardInfos[1];
			}
			this._contentIndex = -1;
			if (this._arenaCardInfos.Length > this._arenaCardGuiInfos.Length)
			{
				this._contentIndex = this._arenaCardGuiInfos.Length;
			}
			for (int j = 0; j < this._arenaCardGuiInfos.Length; j++)
			{
				ArenaSelectorGui.ArenaCardInfo arenaCardInfo = this._arenaCardInfos[j];
				this._arenaCardGuiInfos[j].Setup(arenaCardInfo);
			}
			return num == this._arenaCardInfos.Length - 1;
		}

		private void CacheArenaSprites()
		{
			int numValidArenas = this.GetNumValidArenas();
			this._arenaTextureNameIndexes = new Dictionary<string, int>(numValidArenas);
			this._arenaSprites = new Sprite[numValidArenas];
			int i = 0;
			int num = 0;
			while (i < GameHubBehaviour.Hub.ArenaConfig.Arenas.Length)
			{
				GameArenaInfo gameArenaInfo = GameHubBehaviour.Hub.ArenaConfig.Arenas[i];
				if (this.IsValidArena(gameArenaInfo))
				{
					this._arenaTextureNameIndexes[gameArenaInfo.ArenaSelectorImageName] = num;
					num++;
					if (!SingletonMonoBehaviour<LoadingManager>.Instance.TextureManager.GetAssetAsync(gameArenaInfo.ArenaSelectorImageName, this))
					{
						HeavyMetalMachines.Utils.Debug.Assert(false, string.Format("ArenaSelectorGui.LoadTexture: Image/Bundle not found -> [{0},{1}]", base.name, gameArenaInfo.ArenaSelectorImageName), HeavyMetalMachines.Utils.Debug.TargetTeam.GUI);
					}
				}
				i++;
			}
		}

		public void OnAssetLoaded(string textureName, Texture2D texture)
		{
			this.CreateAndCacheTexture(textureName, texture);
		}

		private void CreateAndCacheTexture(string textureName, Texture2D texture)
		{
			this._arenaSprites[this._arenaTextureNameIndexes[textureName]] = Sprite.Create(texture, new Rect(0f, 0f, (float)texture.width, (float)texture.height), Vector2.zero);
		}

		private void PlayAudio(FMODAsset fmodAsset)
		{
			FMODAudioManager.PlayOneShotAt(fmodAsset, Vector3.zero, 0);
		}

		[SerializeField]
		private bool _enableArenaSelector;

		[Header("[Arena Data]")]
		[SerializeField]
		private ArenaSelectorGui.ArenaCardInfo[] _arenaCardInfos;

		[Header("[Arena GUI Data]")]
		[SerializeField]
		private ArenaSelectorGui.ArenaCardGuiInfo[] _arenaCardGuiInfos;

		[SerializeField]
		private ArenaSelectorGui.ArenaCardPosition[] _arenaCardPositions;

		[Header("[Window GUI Components]")]
		[SerializeField]
		private UILabel _titleLabel;

		[SerializeField]
		private Animation _idleTitleAnimation;

		[SerializeField]
		private Animation _mainInOutAnimation;

		[SerializeField]
		private Animation _titleSelectionAnimation;

		[SerializeField]
		private GameObject _cardsGroupGameObject;

		[SerializeField]
		private GameObject _backgroundTileGameObject;

		private float _lerpNormalized;

		private int _contentIndex;

		private float _startSpeed;

		[Header("[Setup Data]")]
		[SerializeField]
		private float _selectionWaitBeforeExitInSec = 3f;

		[SerializeField]
		private float _centeredLerp = 0.5f;

		[SerializeField]
		private float _startAccelModifier = 1f;

		[SerializeField]
		private float _mainLerpLowCapSpeed = 10f;

		[SerializeField]
		private float _speedModifier = 0.2f;

		[SerializeField]
		private float _slowdownModifier = 0.01f;

		[SerializeField]
		private float _endLerpSmoothStopMinSpeedModifier = 0.5f;

		[SerializeField]
		private bool _disableSpinForOnlyOneArenaUnlocked;

		private int[] _lerpIndex;

		private bool _visible;

		private Sprite[] _arenaSprites;

		private Dictionary<string, int> _arenaTextureNameIndexes;

		[SerializeField]
		private float _endLerpNormalized;

		private ArenaSelectorGui.ArenaSelectorState _state;

		[Header("[Audio]")]
		[SerializeField]
		private FMODAsset _inAudioAsset;

		[SerializeField]
		private FMODAsset _outAudioAsset;

		[SerializeField]
		private FMODAsset _spinAudioAsset;

		[SerializeField]
		private FMODAsset _selectionAudioAsset;

		[Header("[Test Only]")]
		[SerializeField]
		private int _targetArenaTest;

		[SerializeField]
		private bool _testSingleSelection;

		[Serializable]
		private struct ArenaCardInfo
		{
			public string NameText;

			public Sprite ImageSprite;

			public bool Locked;

			public int ArenaIndex;

			public string UnlockLevelText;
		}

		[Serializable]
		private struct ArenaCardGuiInfo
		{
			public void Setup(ArenaSelectorGui.ArenaCardInfo arenaCardInfo)
			{
				this.NameLabel.text = arenaCardInfo.NameText;
				this.ImageSprite.sprite2D = arenaCardInfo.ImageSprite;
				this.LockGroupGameObject.SetActive(arenaCardInfo.Locked);
				this.LockGroupAnimationGameObject.SetActive(false);
				if (arenaCardInfo.Locked)
				{
					this.LockLevelLabel.text = arenaCardInfo.UnlockLevelText;
				}
				this.ImageWidgetAlpha.alpha = ((!arenaCardInfo.Locked) ? 1f : 0.5f);
				this.NameLabelWidgetAlpha.alpha = ((!arenaCardInfo.Locked) ? 1f : 0.7f);
				this.ArenaIndex = arenaCardInfo.ArenaIndex;
			}

			public void ShowLockedAnimation()
			{
				this.LockLevelBig1Label.text = this.LockLevelLabel.text;
				this.LockLevelBig2Label.text = this.LockLevelLabel.text;
				this.LockGroupAnimationGameObject.SetActive(true);
			}

			public int ArenaIndex;

			public Transform MainGroupTransform;

			public UILabel NameLabel;

			public NGUIWidgetAlpha NameLabelWidgetAlpha;

			public UI2DSprite ImageSprite;

			public NGUIWidgetAlpha ImageWidgetAlpha;

			public GameObject LockGroupGameObject;

			public GameObject LockGroupAnimationGameObject;

			public UILabel LockLevelLabel;

			public UILabel LockLevelBig1Label;

			public UILabel LockLevelBig2Label;

			public Animation SelectedAnimation;
		}

		[Serializable]
		private struct ArenaCardPosition
		{
			public Vector3 StartPosition;

			public Vector3 EndPosition;
		}

		private enum ArenaSelectorState
		{
			StartLerp,
			MainLerp,
			EndLerp,
			EndLerpSmoothStop,
			ArenaSelection
		}
	}
}
