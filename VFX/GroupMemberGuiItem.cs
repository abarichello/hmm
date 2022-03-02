using System;
using Assets.Standard_Assets.Scripts.HMM.Customization;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI;
using ClientAPI.Objects;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.PlayerSummary.Presenting.Group;
using HeavyMetalMachines.Presenting.Tooltip;
using HeavyMetalMachines.Social.Avatar.Business;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.VFX.PlotKids;
using HeavyMetalMachines.VoiceChat.Business;
using Hoplon.Serialization;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.VFX
{
	public class GroupMemberGuiItem : BaseGUIItem<GroupMemberGuiItem, GroupMember>
	{
		public bool IsCurrentUser
		{
			get
			{
				return base.ReferenceObject != null && string.Equals(base.ReferenceObject.UniversalID, GameHubBehaviour.Hub.User.UniversalId, StringComparison.InvariantCultureIgnoreCase);
			}
		}

		protected override void SetPropertiesTasks(GroupMember groupMember)
		{
			if (groupMember == null)
			{
				this.UpdateCurrentSlotGroup(0);
				this._groupMember = new Player();
				return;
			}
			this._groupMember = groupMember.ConvertToPlayer();
			if (groupMember.GroupId.Equals(Guid.Empty))
			{
				this.UpdateCurrentSlotGroup(1);
				return;
			}
			this.UpdateCurrentSlotGroup(2);
		}

		private void UpdateCurrentSlotGroup(GroupSlotState targetGroupSlotState)
		{
			if (!string.Equals(this._currentUniversalID, (base.ReferenceObject != null) ? base.ReferenceObject.UniversalID : null))
			{
				this.SetLoadingIconActive(false);
			}
			this._currentUniversalID = ((base.ReferenceObject != null) ? base.ReferenceObject.UniversalID : null);
			this._currentState = targetGroupSlotState;
			bool flag = ManagerController.Get<GroupManager>().GetSelfGroupStatus() == GroupStatus.Owner;
			this._timerGameObject.SetActive(this._currentState == 1);
			this.SetLoadingIconActive(this._currentState == 1);
			this._addPlayerGameObject.SetActive(this._currentState == null || this._currentState == -1);
			this._playerAvatarPlayerGameObject.SetActive(this._currentState == 1 || this._currentState == 2);
			this._founderSprite.gameObject.SetActive(false);
			this._removeFromPartyButtonGameObject.SetActive(!this.IsCurrentUser && flag && targetGroupSlotState != 0);
			if (targetGroupSlotState != null)
			{
				if (targetGroupSlotState != 1)
				{
					if (this._hmmTooltipTrigger != null)
					{
						this._hmmTooltipTrigger.TooltipText = base.ReferenceObject.PlayerName;
					}
				}
				else
				{
					this.TryUpdatePendingInviteTime();
					if (this._hmmTooltipTrigger != null)
					{
						this._hmmTooltipTrigger.TooltipText = base.ReferenceObject.PlayerName;
					}
				}
			}
			else
			{
				this.SetLoadingIconActive(false);
			}
			this._groupLeader2DSprite.enabled = (base.ReferenceObject != null && base.ReferenceObject.IsOwner);
			this.UpdateSocialComponents();
			this.TryUpdatePortraitInfo();
		}

		private void UpdateSocialComponents()
		{
			if (base.ReferenceObject == null)
			{
				return;
			}
			if (this._disposables != null)
			{
				this._disposables.Dispose();
			}
			this._disposables = new CompositeDisposable();
			IDisposable disposable = ObservableExtensions.Subscribe<string>(Observable.Do<string>(this._getPlayerAvatarIconName.GetSmallIcon(base.ReferenceObject.PlayerId), delegate(string spriteName)
			{
				this._avatarIcon.TextureName = spriteName;
			}));
			this._disposables.Add(disposable);
		}

		public void EnableRemoveFromGroupButton()
		{
			if (this._currentState == 2 || this._currentState == 1)
			{
				this._removeFromPartyButtonGameObject.SetActive(true);
			}
		}

		public void DisableRemoveFromGroupButton()
		{
			if (this._currentState == 2 || this._currentState == 1)
			{
				this._removeFromPartyButtonGameObject.SetActive(false);
			}
		}

		private void SetLoadingIconActive(bool active)
		{
			this._loadinGameObject.SetActive(active);
		}

		private void TryUpdatePortraitInfo()
		{
			if (base.ReferenceObject == null)
			{
				this._founderSprite.gameObject.SetActive(false);
				return;
			}
			PlayerCustomWS.GetPlayerPortraitCustomizationByUniversalId(base.ReferenceObject.UniversalID, new SwordfishClientApi.ParameterizedCallback<string>(this.OnGetPlayerPortraitSuccess), new SwordfishClientApi.ErrorCallback(this.OnGetPlayerPortraitError));
		}

		private void OnGetPlayerPortraitSuccess(object state, string obj)
		{
			NetResult netResult = (NetResult)((JsonSerializeable<!0>)obj);
			if (!netResult.Success)
			{
				string text = (base.ReferenceObject != null) ? base.ReferenceObject.PlayerName : "null";
				string text2 = (base.ReferenceObject != null) ? base.ReferenceObject.PlayerId.ToString() : "null";
				GroupMemberGuiItem.Log.ErrorFormat("OnGetPlayerPortraitSuccess - Unknown error when trying to get player portrait. Player: {0} ({1}); ErrorId: {2} ErrorMsg: {3}", new object[]
				{
					text,
					text2,
					netResult.Error,
					netResult.Msg
				});
				this._founderSprite.gameObject.SetActive(false);
				this.TryUpdatePortraitInfo();
				return;
			}
			PortraitDecoratorGui.UpdatePortraitSprite(new Guid(netResult.Msg), this._founderSprite, PortraitDecoratorGui.PortraitSpriteType.LoadingVersusIcon);
		}

		private void OnGetPlayerPortraitError(object state, Exception exception)
		{
			GroupMemberGuiItem.Log.ErrorFormat("OnGetPlayerPortraitError - state:[{0}], exception:[{1}]", new object[]
			{
				state,
				exception
			});
		}

		private void TryUpdatePendingInviteTime()
		{
			if (this._currentState != 1)
			{
				return;
			}
			float pendingInviteRemainingTime = ManagerController.Get<GroupManager>().GetPendingInviteRemainingTime(base.ReferenceObject.UniversalID);
			this._timerLabel.text = this.FloatToTime(pendingInviteRemainingTime);
		}

		private void Update()
		{
			this.TryUpdatePendingInviteTime();
			this.TryAlertMemberSpeaking();
		}

		public string FloatToTime(float toConvert)
		{
			return string.Format("{0:#00}:{1:00}", Mathf.Floor(toConvert / 60f), Mathf.Floor(toConvert) % 60f);
		}

		public void onButtonClick_GroupMemberClick()
		{
			if (UICamera.currentTouchID != -2)
			{
				return;
			}
			GroupMemberContextMenuModalGUI groupMemberContextMenuModalGUI;
			SingletonMonoBehaviour<PanelController>.Instance.ShowModalWindow<GroupMemberContextMenuModalGUI>(out groupMemberContextMenuModalGUI);
			groupMemberContextMenuModalGUI.GroupMember = base.ReferenceObject;
			groupMemberContextMenuModalGUI.ParentGUI = this._parentUI;
		}

		public void OnButtonClick_GroupMemberRemove()
		{
			SingletonMonoBehaviour<ManagerController>.Instance.GetManager<GroupManager>().TryKickMemberOrCancelInvite(base.ReferenceObject);
			GameHubBehaviour.Hub.Swordfish.Log.BILogClient(76, true);
		}

		private void TryAlertMemberSpeaking()
		{
			if (this._currentState != 2)
			{
				if (this._voiceAlertObject.activeSelf)
				{
					this._voiceAlertObject.SetActive(false);
				}
				return;
			}
			bool flag = this._isPlayerSpeakingOnVoice.IsSpeaking(this._groupMember);
			if (flag == this._voiceAlertObject.activeSelf)
			{
				return;
			}
			this._voiceAlertObject.SetActive(flag);
		}

		protected static readonly BitLogger Log = new BitLogger(typeof(GroupMemberGuiItem));

		[Inject]
		private IGetPlayerAvatarIconName _getPlayerAvatarIconName;

		[SerializeField]
		private NGuiNewTooltipTrigger _tooltipTrigger;

		private GroupSlotState _currentState = -1;

		[Header("Group Member GUI Item Properties")]
		[SerializeField]
		private GameObject _addPlayerGameObject;

		[SerializeField]
		private GameObject _playerAvatarPlayerGameObject;

		[SerializeField]
		private GameObject _loadinGameObject;

		[SerializeField]
		private UI2DSprite _groupLeader2DSprite;

		[SerializeField]
		private HMMUI2DDynamicTexture _avatarIcon;

		[SerializeField]
		private GameObject _timerGameObject;

		[SerializeField]
		private UILabel _timerLabel;

		[SerializeField]
		private HMMUI2DDynamicSprite _founderSprite;

		[SerializeField]
		private HMMTooltipTrigger _hmmTooltipTrigger;

		private string _currentUniversalID;

		[SerializeField]
		private GameObject _voiceAlertObject;

		[SerializeField]
		private GameObject _removeFromPartyButtonGameObject;

		private CompositeDisposable _disposables;

		private IPlayer _groupMember;

		[Inject]
		private DiContainer _diContainer;

		[Inject]
		private IIsPlayerSpeakingOnVoiceChat _isPlayerSpeakingOnVoice;

		[SerializeField]
		private SocialModalGUI _parentUI;
	}
}
