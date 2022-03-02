using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ClientAPI;
using FMod;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.CharacterSelection.Client;
using HeavyMetalMachines.Frontend.ArenaSelector;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.ParentalControl.Restrictions;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Utils;
using Hoplon.Input.UiNavigation;
using Hoplon.Unity.Loading;
using Pocketverse;
using SharedUtils.Loading;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class ArenaSelectorGui : GameHubBehaviour, IDynamicAssetListener<Texture2D>
	{
		private IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

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

		private ArenaCardInfo GetArenaCardInfo(int arenaIndex)
		{
			for (int i = 0; i < this._arenaCardInfos.Length; i++)
			{
				if (this._arenaCardInfos[i].ArenaIndex == arenaIndex)
				{
					return this._arenaCardInfos[i];
				}
			}
			Debug.Assert(false, string.Format("Arena card info not found fo arena index [{0}]", arenaIndex), Debug.TargetTeam.All);
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
			this._clientBILogger.BILogClientMsg(107, DebugBIMessageFormatter.Get("QAHMM-31739", "UserMatchDataReceived"), true);
			ArenaSelectorGui.Log.InfoFormat("UserOnMatchDataReceived. matchState={0}, kind={1}", new object[]
			{
				matchState,
				GameHubBehaviour.Hub.Match.Kind
			});
			if (matchState != MatchData.MatchState.CharacterPick)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish) && GameHubBehaviour.Hub.ArenaConfig.GetCurrentArena().IsCustomOnly)
			{
				ArenaSelectorGui.Log.Info("skipping Arena Selector because it is custom only");
				this.GoToPickMode();
				return;
			}
			switch (GameHubBehaviour.Hub.Match.Kind)
			{
			case 2:
				ArenaSelectorGui.Log.Info("skipping Arena Selector because it is tutorial");
				this.GoToTutorialLoading();
				return;
			case 3:
			case 4:
			case 5:
			case 6:
				ArenaSelectorGui.Log.Info("skipping Arena Selector because it is kind " + GameHubBehaviour.Hub.Match.Kind.ToString());
				this.GoToPickMode();
				return;
			default:
				ArenaSelectorGui.Log.Info("showing Arena Selector");
				this.Show();
				GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>().MatchAccept.HideAcceptanceWindow(false);
				return;
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
			ArenaCardInfo arenaCardInfo = this._arenaCardInfos[this._contentIndex];
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
			this._idleTitleAnimation.wrapMode = 2;
			GUIUtils.PlayAnimation(this._idleTitleAnimation, false, 1f, string.Empty);
		}

		private void SuspensionPointsHide()
		{
			this._idleTitleAnimation.wrapMode = 1;
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
			this.UiNavigationGroupHolder.AddHighPriorityGroup();
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
			this.UiNavigationGroupHolder.RemoveHighPriorityGroup();
		}

		private void GoToPickMode()
		{
			DisposableExtensions.AddTo<IDisposable>(ObservableExtensions.Subscribe<Unit>(Observable.DoOnCompleted<Unit>(Observable.DoOnCompleted<Unit>(this.PrepareMatch(), delegate()
			{
				this._clientBILogger.BILogClientMsg(107, DebugBIMessageFormatter.Get("QAHMM-31739", "GoToPick"), true);
			}), delegate()
			{
				this._proceedToClientCharacterSelectionState.Proceed();
			})), this);
		}

		private IObservable<Unit> PrepareMatch()
		{
			return Observable.DoOnCompleted<Unit>(this._diContainer.Resolve<IPrepareMatch>().Prepare(), delegate()
			{
				IPlayer player2 = this._diContainer.Resolve<IGetLocalPlayer>().Get();
				IVoiceChatRestriction voiceChatRestriction = this._diContainer.Resolve<IVoiceChatRestriction>();
				PlayerIdentification[] matchPlayerIdentifications = this.GetMatchPlayerIdentifications();
				PlayerIdentification[] array = matchPlayerIdentifications;
				for (int i = 0; i < array.Length; i++)
				{
					PlayerIdentification player = array[i];
					if (player.PlayerId != player2.PlayerId)
					{
						if (voiceChatRestriction.IsEnabledByPlayer(player.PlayerId))
						{
							SwordfishClientApi.Callback callback = delegate(object state)
							{
								Debug.LogFormat("pick blocked voice user {0} - {1}", new object[]
								{
									player.PlayerId,
									player.UniversalId
								});
							};
							SwordfishClientApi.ErrorCallback errorCallback = delegate(object state, Exception exception)
							{
								Debug.LogFormat("pick error blocking voice user {0} - {1} \n {2}", new object[]
								{
									player.PlayerId,
									player.UniversalId,
									exception.Message
								});
							};
							GameHubBehaviour.Hub.ClientApi.voice.BlockUserForMe(null, player.PlayerId, callback, errorCallback);
						}
					}
				}
			});
		}

		private PlayerIdentification[] GetMatchPlayerIdentifications()
		{
			IGetCurrentMatch getCurrentMatch = this._diContainer.Resolve<IGetCurrentMatch>();
			foreach (MatchClient matchClient in GetCurrentMatchExtensions.Get(getCurrentMatch).Clients)
			{
				Debug.LogFormat("TOMATE MatchClient. Name={0} IsBot={1}", new object[]
				{
					matchClient.PlayerName,
					matchClient.IsBot
				});
			}
			return (from client in GetCurrentMatchExtensions.Get(getCurrentMatch).Clients
			where !client.IsBot
			select new PlayerIdentification
			{
				PlayerId = client.PlayerId,
				UniversalId = client.UniversalId
			}).ToArray<PlayerIdentification>();
		}

		private void GoToTutorialLoading()
		{
			GameHubBehaviour.Hub.State.GotoState(GameHubBehaviour.Hub.User.HackLoadingMode, false);
		}

		private bool LoadArenasData()
		{
			this._arenaCardInfos = this._arenaSelectorCardsProvider.Get(this._arenaSprites);
			this._contentIndex = -1;
			if (this._arenaCardInfos.Length > this._arenaCardGuiInfos.Length)
			{
				this._contentIndex = this._arenaCardGuiInfos.Length;
			}
			int num = 0;
			for (int i = 0; i < this._arenaCardGuiInfos.Length; i++)
			{
				ArenaCardInfo arenaCardInfo = this._arenaCardInfos[i];
				this._arenaCardGuiInfos[i].Setup(arenaCardInfo);
				if (arenaCardInfo.Locked)
				{
					num++;
				}
			}
			return num == this._arenaCardInfos.Length - 1;
		}

		private void CacheArenaSprites()
		{
			int numberOfArenas = GameHubBehaviour.Hub.ArenaConfig.GetNumberOfArenas();
			this._arenaTextureNameIndexes = new Dictionary<string, List<int>>(numberOfArenas);
			this._arenaSprites = new Sprite[numberOfArenas];
			int i = 0;
			int num = 0;
			while (i < numberOfArenas)
			{
				IGameArenaInfo arenaByIndex = GameHubBehaviour.Hub.ArenaConfig.GetArenaByIndex(i);
				if (!string.IsNullOrEmpty(arenaByIndex.ArenaSelectorImageName))
				{
					List<int> list;
					if (!this._arenaTextureNameIndexes.TryGetValue(arenaByIndex.ArenaSelectorImageName, out list))
					{
						list = new List<int>();
						this._arenaTextureNameIndexes.Add(arenaByIndex.ArenaSelectorImageName, list);
					}
					list.Add(num);
					if (!Loading.TextureManager.GetAssetAsync(arenaByIndex.ArenaSelectorImageName, this))
					{
						Debug.Assert(false, string.Format("ArenaSelectorGui.LoadTexture: Image/Bundle not found -> [{0},{1}]", base.name, arenaByIndex.ArenaSelectorImageName), Debug.TargetTeam.GUI);
					}
				}
				num++;
				i++;
			}
		}

		public void OnAssetLoaded(string textureName, Texture2D texture)
		{
			this.CreateAndCacheTexture(textureName, texture);
		}

		private void CreateAndCacheTexture(string textureName, Texture2D texture)
		{
			List<int> list = this._arenaTextureNameIndexes[textureName];
			for (int i = 0; i < list.Count; i++)
			{
				this._arenaSprites[list[i]] = Sprite.Create(texture, new Rect(0f, 0f, (float)texture.width, (float)texture.height), Vector2.zero);
			}
		}

		private void PlayAudio(AudioEventAsset fmodAsset)
		{
			FMODAudioManager.PlayOneShotAt(fmodAsset, Vector3.zero, 0);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ArenaSelectorGui));

		[Inject]
		private IProceedToClientCharacterSelectionState _proceedToClientCharacterSelectionState;

		[Inject]
		private IArenaSelectorCardsProvider _arenaSelectorCardsProvider;

		[Inject]
		private DiContainer _diContainer;

		[Inject]
		private IClientBILogger _clientBILogger;

		[Header("[Arena Data]")]
		[SerializeField]
		private ArenaCardInfo[] _arenaCardInfos;

		[Header("[Arena GUI Data]")]
		[SerializeField]
		private ArenaSelectorGui.ArenaCardGuiInfo[] _arenaCardGuiInfos;

		[SerializeField]
		private ArenaSelectorGui.ArenaCardPosition[] _arenaCardPositions;

		[Header("[Window GUI Components]")]
		[SerializeField]
		private Animation _idleTitleAnimation;

		[SerializeField]
		private Animation _mainInOutAnimation;

		[SerializeField]
		private Animation _titleSelectionAnimation;

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

		private Dictionary<string, List<int>> _arenaTextureNameIndexes;

		[SerializeField]
		private float _endLerpNormalized;

		private ArenaSelectorGui.ArenaSelectorState _state;

		[Header("[Audio]")]
		[SerializeField]
		private AudioEventAsset _inAudioAsset;

		[SerializeField]
		private AudioEventAsset _outAudioAsset;

		[SerializeField]
		private AudioEventAsset _spinAudioAsset;

		[SerializeField]
		private AudioEventAsset _selectionAudioAsset;

		[Header("[Ui Navigation]")]
		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[Serializable]
		private struct ArenaCardGuiInfo
		{
			public void Setup(ArenaCardInfo arenaCardInfo)
			{
				this.NameLabel.text = arenaCardInfo.NameText;
				this.ImageSprite.sprite2D = arenaCardInfo.ImageSprite;
				this.LockGroupGameObject.SetActive(arenaCardInfo.Locked);
				this.LockGroupAnimationGameObject.SetActive(false);
				if (arenaCardInfo.Locked)
				{
					this.LockLevelLabel.text = arenaCardInfo.UnlockLevelText;
				}
				this.ImageWidgetAlpha.Alpha = ((!arenaCardInfo.Locked) ? 1f : 0.5f);
				this.NameLabelWidgetAlpha.Alpha = ((!arenaCardInfo.Locked) ? 1f : 0.7f);
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
