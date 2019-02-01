using System;
using Assets.Standard_Assets.Scripts.HMM.Customization;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI;
using ClientAPI.Objects;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.VFX.PlotKids;
using HeavyMetalMachines.VFX.PlotKids.VoiceChat;
using Pocketverse;
using UnityEngine;

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
				this.UpdateCurrentSlotGroup(GroupMemberGuiItem.GroupSlotState.Available);
				return;
			}
			if (groupMember.GroupId.Equals(Guid.Empty))
			{
				this.UpdateCurrentSlotGroup(GroupMemberGuiItem.GroupSlotState.Pending);
				return;
			}
			this.UpdateCurrentSlotGroup(GroupMemberGuiItem.GroupSlotState.Filled);
		}

		private void UpdateCurrentSlotGroup(GroupMemberGuiItem.GroupSlotState targetGroupSlotState)
		{
			if (!string.Equals(this._currentUniversalID, (base.ReferenceObject != null) ? base.ReferenceObject.UniversalID : null))
			{
				this._steamIconLoader.ResetPlayerIcon();
			}
			this._currentUniversalID = ((base.ReferenceObject != null) ? base.ReferenceObject.UniversalID : null);
			this._currentState = targetGroupSlotState;
			bool flag = ManagerController.Get<GroupManager>().GetSelfGroupStatus() == GroupStatus.Owner;
			this._timerGameObject.SetActive(this._currentState == GroupMemberGuiItem.GroupSlotState.Pending);
			this._addPlayerGameObject.SetActive(this._currentState == GroupMemberGuiItem.GroupSlotState.Available || this._currentState == GroupMemberGuiItem.GroupSlotState.Null);
			this._playerAvatarPlayerGameObject.SetActive(this._currentState == GroupMemberGuiItem.GroupSlotState.Pending || this._currentState == GroupMemberGuiItem.GroupSlotState.Filled);
			this._founderSprite.gameObject.SetActive(false);
			this._removeFromPartyButtonGameObject.SetActive(!this.IsCurrentUser && flag && targetGroupSlotState != GroupMemberGuiItem.GroupSlotState.Available);
			if (targetGroupSlotState != GroupMemberGuiItem.GroupSlotState.Available)
			{
				if (targetGroupSlotState != GroupMemberGuiItem.GroupSlotState.Pending)
				{
					this._steamIconLoader.UpdatePlayerIcon(base.ReferenceObject.UniversalID);
					if (this._hmmTooltipTrigger != null)
					{
						this._hmmTooltipTrigger.TooltipText = base.ReferenceObject.PlayerName;
					}
				}
				else
				{
					this.TryUpdatePendingInviteTime();
					this._steamIconLoader.UpdatePlayerIcon(base.ReferenceObject.UniversalID);
					if (this._hmmTooltipTrigger != null)
					{
						this._hmmTooltipTrigger.TooltipText = base.ReferenceObject.PlayerName;
					}
				}
			}
			else
			{
				this._steamIconLoader.ResetPlayerIcon();
			}
			this._groupLeader2DSprite.enabled = (base.ReferenceObject != null && base.ReferenceObject.IsOwner);
			this.TryUpdatePortraitInfo();
		}

		public void EnableRemoveFromGroupButton()
		{
			if (this._currentState == GroupMemberGuiItem.GroupSlotState.Filled || this._currentState == GroupMemberGuiItem.GroupSlotState.Pending)
			{
				this._removeFromPartyButtonGameObject.SetActive(true);
			}
		}

		public void DisableRemoveFromGroupButton()
		{
			if (this._currentState == GroupMemberGuiItem.GroupSlotState.Filled || this._currentState == GroupMemberGuiItem.GroupSlotState.Pending)
			{
				this._removeFromPartyButtonGameObject.SetActive(false);
			}
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
			NetResult netResult = (NetResult)((JsonSerializeable<T>)obj);
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
			if (this._currentState != GroupMemberGuiItem.GroupSlotState.Pending)
			{
				return;
			}
			if (!this._steamIconLoader.loading_ico.activeSelf)
			{
				this._steamIconLoader.loading_ico.SetActive(true);
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
		}

		private void TryAlertMemberSpeaking()
		{
			if (this._currentState != GroupMemberGuiItem.GroupSlotState.Filled)
			{
				if (this._voiceAlertObject.activeSelf)
				{
					this._voiceAlertObject.SetActive(false);
				}
				return;
			}
			bool flag = SingletonMonoBehaviour<VoiceChatController>.Instance.IsUserSpeaking(base.ReferenceObject.UniversalID);
			if (flag == this._voiceAlertObject.activeSelf)
			{
				return;
			}
			this._voiceAlertObject.SetActive(flag);
		}

		protected static readonly BitLogger Log = new BitLogger(typeof(GroupMemberGuiItem));

		private GroupMemberGuiItem.GroupSlotState _currentState = GroupMemberGuiItem.GroupSlotState.Null;

		[Header("Group Member GUI Item Properties")]
		[SerializeField]
		private GameObject _addPlayerGameObject;

		[SerializeField]
		private GameObject _playerAvatarPlayerGameObject;

		[SerializeField]
		private UI2DSprite _groupLeader2DSprite;

		[SerializeField]
		private SteamIconLoader _steamIconLoader;

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

		[SerializeField]
		private SocialModalGUI _parentUI;

		public enum GroupSlotState
		{
			Null = -1,
			Available,
			Pending,
			Filled
		}
	}
}
